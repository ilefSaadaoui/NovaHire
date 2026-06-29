-- ============================================================
--  SCRIPT DE NETTOYAGE DES OFFRES DUPLIQUÉES - NovaHire
--  Base de données : PostgreSQL
--  Table cible    : "JobOffers"
--
--  LOGIQUE :
--    Pour chaque groupe de doublons (même Title + Location +
--    Department + CompanyId), on conserve UNIQUEMENT l'offre
--    ayant le plus de candidatures (ApplicationsCount).
--    En cas d'égalité, on garde la plus ancienne (CreatedAt).
--    Les doublons sont marqués comme "Archived" (Status = 4),
--    jamais supprimés physiquement (soft-delete conforme au backend).
--
--  SÉCURITÉ :
--    - Tout est enveloppé dans une transaction avec ROLLBACK possible.
--    - Une étape de PRÉVISUALISATION est fournie (sans modification).
--    - Le DELETE physique n'est JAMAIS exécuté.
-- ============================================================

-- ─────────────────────────────────────────────────────────────
--  ÉTAPE 0 : Vérifier les valeurs d'enum Status
--  Draft=0 | Published=1 | Closed=2 | Paused=3 | Archived=4
-- ─────────────────────────────────────────────────────────────

-- ─────────────────────────────────────────────────────────────
--  ÉTAPE 1 : PRÉVISUALISATION (lecture seule, aucune modification)
--  Exécuter ce bloc d'abord pour voir les doublons détectés.
-- ─────────────────────────────────────────────────────────────

SELECT
    jo."Title"                                          AS "Titre",
    jo."Location"                                       AS "Lieu",
    jo."Department"                                     AS "Département",
    COUNT(*)                                            AS "Nombre de doublons",
    MIN(jo."CreatedAt")                                 AS "Plus ancienne création",
    MAX(jo."CreatedAt")                                 AS "Plus récente création",
    SUM(
        (SELECT COUNT(*) FROM "JobApplications" ja WHERE ja."JobOfferId" = jo."Id")
    )                                                   AS "Total candidatures cumulées"
FROM "JobOffers" jo
WHERE jo."Status" != 4  -- Exclure ceux déjà archivés
GROUP BY
    jo."Title",
    LOWER(TRIM(COALESCE(jo."Location", ''))),
    LOWER(TRIM(COALESCE(jo."Department", ''))),
    jo."CompanyId"
HAVING COUNT(*) > 1
ORDER BY "Nombre de doublons" DESC, "Titre";


-- ─────────────────────────────────────────────────────────────
--  ÉTAPE 2 : IDENTIFIER les IDs à archiver (doublons à supprimer)
--  Conserve l'offre avec le plus de candidatures (ou la plus ancienne).
-- ─────────────────────────────────────────────────────────────

WITH ranked_offers AS (
    SELECT
        jo."Id",
        jo."Title",
        jo."Location",
        jo."Department",
        jo."CompanyId",
        jo."CreatedAt",
        jo."Status",
        (
            SELECT COUNT(*)
            FROM "JobApplications" ja
            WHERE ja."JobOfferId" = jo."Id"
        )                               AS app_count,
        ROW_NUMBER() OVER (
            PARTITION BY
                jo."CompanyId",
                jo."Title",
                LOWER(TRIM(COALESCE(jo."Location", ''))),
                LOWER(TRIM(COALESCE(jo."Department", '')))
            ORDER BY
                (SELECT COUNT(*) FROM "JobApplications" ja WHERE ja."JobOfferId" = jo."Id") DESC,
                jo."CreatedAt" ASC   -- en cas d'égalité, garder la plus ancienne
        )                               AS rn
    FROM "JobOffers" jo
    WHERE jo."Status" != 4             -- ignorer les déjà archivés
)
SELECT
    "Id"            AS "ID à archiver",
    "Title"         AS "Titre",
    "Location"      AS "Lieu",
    "Department"    AS "Département",
    app_count       AS "Candidatures",
    "CreatedAt"     AS "Créé le",
    rn              AS "Rang (1 = à conserver)"
FROM ranked_offers
WHERE rn > 1    -- tout sauf le meilleur = doublons à archiver
ORDER BY "Title", "CreatedAt";


-- ─────────────────────────────────────────────────────────────
--  ÉTAPE 3 : ARCHIVAGE DES DOUBLONS (modification réelle)
--  Décommenter et exécuter APRÈS avoir vérifié l'étape 2.
-- ─────────────────────────────────────────────────────────────

BEGIN;

WITH ranked_offers AS (
    SELECT
        jo."Id",
        ROW_NUMBER() OVER (
            PARTITION BY
                jo."CompanyId",
                jo."Title",
                LOWER(TRIM(COALESCE(jo."Location", ''))),
                LOWER(TRIM(COALESCE(jo."Department", '')))
            ORDER BY
                (SELECT COUNT(*) FROM "JobApplications" ja WHERE ja."JobOfferId" = jo."Id") DESC,
                jo."CreatedAt" ASC
        ) AS rn
    FROM "JobOffers" jo
    WHERE jo."Status" != 4
),
to_archive AS (
    SELECT "Id" FROM ranked_offers WHERE rn > 1
)
UPDATE "JobOffers"
SET
    "Status"    = 4,                    -- Archived
    "UpdatedAt" = NOW()
WHERE "Id" IN (SELECT "Id" FROM to_archive);

-- Vérifier le nombre de lignes modifiées avant de valider :
-- GET DIAGNOSTICS row_count = ROW_COUNT;

-- ─────────────────────────────────────────────────────────────
-- Si le résultat vous convient  → COMMIT;
-- Si quelque chose semble faux  → ROLLBACK;
-- ─────────────────────────────────────────────────────────────

-- COMMIT;   -- ← Décommenter pour valider
ROLLBACK;    -- ← Décommenter pour annuler (par défaut : sécurité)


-- ─────────────────────────────────────────────────────────────
--  ÉTAPE 4 : VÉRIFICATION POST-NETTOYAGE
--  Exécuter après le COMMIT pour confirmer qu'il n'y a plus de doublons.
-- ─────────────────────────────────────────────────────────────

SELECT
    jo."Title",
    jo."Location",
    jo."Department",
    COUNT(*) AS "Nombre actifs restants"
FROM "JobOffers" jo
WHERE jo."Status" != 4
GROUP BY jo."Title", jo."Location", jo."Department", jo."CompanyId"
HAVING COUNT(*) > 1;

-- Si ce SELECT retourne 0 lignes → plus aucun doublon actif. ✅


-- ─────────────────────────────────────────────────────────────
--  BONUS : Compter le total avant / après
-- ─────────────────────────────────────────────────────────────

SELECT
    COUNT(*) FILTER (WHERE "Status" != 4) AS "Offres actives/draft/publiées",
    COUNT(*) FILTER (WHERE "Status"  = 4) AS "Offres archivées (doublons inclus)",
    COUNT(*)                               AS "Total"
FROM "JobOffers";

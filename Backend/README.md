# 🚀 NovaHire - Plateforme SaaS de Recrutement Intelligente

NovaHire est une plateforme complète (Backend .NET 10 & Frontend Vue.js) de recrutement intégrant des fonctionnalités d'Intelligence Artificielle pour l'analyse des CV et le matching des candidats.

---

## 📋 Prérequis Système

Avant de commencer, assurez-vous d'avoir installé :
- **.NET 10 SDK** (Pour l'API Backend)
- **Node.js (v18+) & npm** (Pour le Frontend Vue.js)
- **PostgreSQL** (Base de données principale)
- **Python 3.10+** (Requis uniquement si vous lancez le module IA d'extraction en local)

---

## ⚙️ Configuration & Lancement Rapide (Quick Start)

### 1. Configuration de la Base de Données
Le backend utilise Entity Framework Core avec PostgreSQL.
Ouvrez le fichier `src/API/appsettings.json` et configurez votre chaîne de connexion :
```json
"ConnectionStrings": {
  "PostgreSQL": "Host=localhost;Database=NovaHireDB;Username=postgres;Password=VotreMotDePasse"
}
```

### 2. Démarrer le Backend (.NET API)
Ouvrez un terminal dans le dossier `Backend/src/API` et exécutez :
```bash
# Restaurer les dépendances et appliquer les migrations à la base de données
dotnet restore
dotnet ef database update --project ../Infrastructure --startup-project .

# Lancer l'API
dotnet run
```
L'API sera accessible sur `http://localhost:5000` (ou `https://localhost:5001`). Vous pouvez consulter la documentation Swagger via votre navigateur.

### 3. Démarrer le Frontend (Vue.js)
Ouvrez un autre terminal dans le dossier `frontend` et exécutez :
```bash
# Installer les dépendances Node
npm install

# Lancer le serveur de développement Vite
npm run dev
```
Le portail sera accessible à l'adresse indiquée par Vite (généralement `http://localhost:5173`).

---

## 👑 Administration & Données de Test

La plateforme est livrée avec des données par défaut pour faciliter vos premiers tests.

**Compte SuperAdmin par défaut :**
- **Email :** `admin@neoledge.com`
- **Mot de passe :** `Admin@123`

### Accès au Dashboard d'Administration de la Base de Données
Un outil d'administration interne est directement embarqué dans le backend pour gérer directement les données brutes (Utilisateurs, Entreprises, Offres).
1. Assurez-vous que l'API est lancée (`dotnet run`).
2. Accédez à `http://localhost:5000/admin` depuis votre navigateur.
3. Connectez-vous avec les identifiants SuperAdmin ci-dessus.

---

## 🎨 Personnalisation (Multi-Tenant)

NovaHire est conçu pour héberger plusieurs entreprises (Multi-Tenant).
- Chaque `Company` (Entreprise) est complètement isolée au niveau de la base de données.
- Les couleurs (`PrimaryColor`, `SecondaryColor`) et le logo sont configurables par entreprise.

---

## 🆘 Dépannage (Troubleshooting)

- **Erreur "JWT SecretKey must be at least 32 characters" :** Vérifiez que vous avez défini une clé `SecretKey` d'au moins 32 caractères dans `appsettings.json` ou dans vos variables d'environnement.
- **Port 5000 déjà utilisé :** L'API essaiera automatiquement de basculer sur le port 5001. Sinon, vous pouvez tuer le processus occupant le port via votre gestionnaire de tâches.
- **Fuite de données Candidat :** Les candidats sont strictement isolés par `CompanyId` via des Global Query Filters dans EF Core.

🎉 **Félicitations, vous êtes prêt à lancer NovaHire !**

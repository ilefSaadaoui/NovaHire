
# Recruitment SaaS — Plateforme de gestion d'offres et de candidatures

Une plateforme SaaS professionnelle pour la gestion des offres d'emploi, la collecte et l'analyse des candidatures, enrichie par des modules d'intelligence artificielle pour l'extraction de CV, la génération de résumés d'expérience et le scoring des candidats.

## Objectif
Fournir aux entreprises une solution clé en main pour publier et diffuser des offres, personnaliser l'expérience candidat, automatiser l'analyse des CV et prioriser les candidatures grâce à des modèles IA/NLP.

## Cas d'usage
- Portails carrières multi-sociétés
- Création et publication d'offres d'emploi et de stage
- Formulaires de candidature personnalisables (champs, pièces jointes)
- Analyse automatique des CV (OCR, extraction de champs, résumé d'expérience)
- Notation et présélection intelligente des candidats
- Export et génération de rapports (Excel, Word, PDF)

## Fonctionnalités clés
- Multi-tenant (gestion multi-sociétés)
- Espace recruteur complet : création, personnalisation, publication d'offres
- URL publiques de diffusion des offres
- Pipeline d'analyse des candidatures (ingestion CV, OCR, extraction, LLM)
- Scoring automatique des candidats selon critères métiers
- Rapports exportables et historisation des décisions

## Structure du dépôt
Le dépôt contient le frontend (Vue 3) ainsi qu'un dossier `backend/` avec la solution back-end (.NET 10).

- Frontend (Vue 3, Vite) — racine du projet
	- Points d'entrée : `src/App.vue`, `src/main.js`
	- Routes : `src/router/index.js`
	- Vues : `src/views/` (Login, Register, CompanyPayment, etc.)
- Backend (`backend/`) — solution .NET 10
	- Solution : `backend/RecruitmentSaasBackend.slnx`
	- API : `backend/src/API` (contrôleurs, `Program.cs`, `appsettings.json`)
	- Modules DDD : `Application`, `Domain`, `Infrastructure`
	- Documentation et guides : `backend/docs/`

Consultez les guides dans `backend/docs/` pour les détails d'API, d'architecture et d'administration.

## Technologies

- Frontend : Vue.js 3, JavaScript, Vite, HTML, CSS
- Backend : .NET 10, C#, ASP.NET Core
- Bases de données : SQL Server / PostgreSQL (configurable)
- IA/NLP : Python (optionnel), OCR, LLM/NLP pour extraction et résumé
- Outils : GitHub, VS Code, Visual Studio, Azure Boards

## Prérequis (développement)
- Node.js + npm (ou yarn)
- .NET 10 SDK
- (Optionnel) Python 3.x si vous exécutez localement des composants IA

## Lancer le projet en local

Frontend

```bash
cd frontend-v2/Plateforme_recrutement
npm install
npm run dev
```

Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/API
```

Configuration
- Mettez à jour les chaînes de connexion et variables d'environnement dans `backend/src/API/appsettings.Development.json` ou via les variables d'environnement.

## Déploiement
Déployez le backend sur une plateforme .NET (Azure App Service, Containers) et le frontend sur un hébergeur statique/CDN (Azure Static Web Apps, Netlify, Vercel). Assurez-vous de provisionner la base de données et les secrets (clé LLM, OCR, etc.) dans votre environnement sécurisé.

## Documentation
- `backend/docs/API_GUIDE.md` — référence API
- `backend/docs/ARCHITECTURE.md` — architecture technique
- `backend/QUICKSTART.md` — guide démarrage

## Contribution & support
Ouvrez une issue pour signaler un bug ou proposer une fonctionnalité. Pour contributions, proposez une branche par fonctionnalité et une PR avec une description claire.

## Contact
Ajoutez ici les contacts projet ou l'email de support.



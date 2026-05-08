# NovaHire - Plateforme de Recrutement Intelligente (SaaS)

NovaHire est une solution SaaS innovante conçue pour moderniser et automatiser le processus de recrutement grâce à l'Intelligence Artificielle. Ce projet a été développé dans le cadre d'un **Projet de Fin d'Études (PFE)** avec une architecture robuste et un design premium.

---

## 💡 Le Concept NovaHire

La problématique actuelle des recruteurs est le volume massif de candidatures et le temps passé à trier des profils parfois non qualifiés. **NovaHire** répond à ce défi en agissant comme un copilote intelligent :

1. **Analyse Instantanée** : L'IA lit et comprend les CV mieux qu'un simple scanner de mots-clés.
2. **Matching Sémantique** : Score de compatibilité entre le candidat et l'offre basé sur les compétences, l'expérience et la formation.
3. **Génération de Contenu** : Aide à la rédaction d'offres et à la préparation des entretiens.

---

## ✨ Fonctionnalités Implémentées

### 🤖 Intelligence Artificielle (Moteur Python)
- **Analyse Automatisée de CV** : Extraction des données, calcul du score de matching (0-100%).
- **Diagnostic AFOM** : Identification automatique des points forts et points de vigilance.
- **Générateur d'Offres** : Création assistée de descriptions de postes optimisées.
- **Guide d'Entretien Stratégique** : Questions personnalisées suggérées pour chaque candidat.

### 🏢 Gestion Multi-Tenancy (.NET Clean Architecture)
- **SuperAdmin** : Cockpit exécutif pour la gestion globale de la plateforme.
- **Admin Entreprise** : Gestion de l'équipe de recrutement et du branding.
- **Recruteur** : Opérationnel pur (gestion des offres, analyse des talents, suivi pipeline).

### 📊 Reporting & Analytics
- **Dashboard Recruteur** : Visualisation de la pipeline, statistiques de conversion et qualité des talents.
- **Exports Premium "Celestial"** : Génération de rapports PDF et Excel haute précision pour les réunions de décision.

### 🧩 Évaluation Technique
- **Quiz IA** : Envoi de tests techniques automatiques avec scoring intégré au profil candidat.

---

## 🛠 Stack Technique

- **Backend** : .NET 10 (C#), Entity Framework Core, PostgreSQL.
- **Frontend** : Vue.js 3, Pinia (State Management), Vite, CSS Moderne (Design Système "Celestial").
- **Moteur IA** : Python (FastAPI), Spacy, Sentence-Transformers, Groq LLM / OpenAI.
- **Reporting** : QuestPDF (PDF), ClosedXML (Excel).

---

## 🚀 Guide d'Installation (Local)

Pour installer et faire fonctionner NovaHire sur votre machine, suivez ces étapes rapides :

### 1. Prérequis
- **SDK .NET 10**
- **Node.js** (v18+)
- **Python** (3.10+)
- **PostgreSQL** (v15+)
- **Tesseract OCR** (à installer dans `C:\Program Files\Tesseract-OCR`)

### 2. Configuration & Lancement

#### Backend (.NET)
```powershell
cd Backend/src/API
dotnet restore
dotnet run --launch-profile http
```
*L'API sera sur `http://localhost:5000`*

#### Service AI (Python)
```powershell
cd Backend/src/AI
python -m venv venv
.\venv\Scripts\activate
pip install -r requirements.txt
python main.py
```
*Le service AI sera sur `http://localhost:8000`*

#### Frontend (Vue.js)
```powershell
cd frontend
npm install
npm run dev
```
*Le site sera sur `http://localhost:3010`*

---

## 🔑 Identifiants de Test (Admin)
- **Email** : `admin@neoledge.com`
- **Mot de passe** : `Admin@123`

---

*Développé avec passion pour transformer le futur du recrutement.*

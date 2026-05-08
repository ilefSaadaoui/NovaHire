# Guide d'installation NovaHire (FullStack)

Ce guide contient toutes les étapes pour faire fonctionner le projet NovaHire sur une nouvelle machine.

## 1. Prérequis (Installation Logicielle)

Avant de commencer, assurez-vous d'avoir installé :
- **Node.js** (v18 ou plus) : [Télécharger](https://nodejs.org/)
- **SDK .NET 10** : [Télécharger](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Python** (3.10 ou plus) : [Télécharger](https://www.python.org/)
- **PostgreSQL** (v15 ou plus) : [Télécharger](https://www.postgresql.org/)
- **Tesseract OCR** (Pour le service AI) : [Télécharger](https://github.com/UB-Mannheim/tesseract/wiki)
  - **IMPORTANT** : Installez-le dans le chemin par défaut : `C:\Program Files\Tesseract-OCR`

---

## 2. Configuration de la Base de Données

Le projet est configuré pour utiliser PostgreSQL sur le port **5433**.

1. Ouvrez **pgAdmin** ou utilisez la ligne de commande.
2. Créez une base de données nommée `novahiredb2`.
3. Le mot de passe par défaut configuré est `root`. Si vous utilisez un autre mot de passe, modifiez-le dans `Backend/src/API/appsettings.json`.
4. **Initialisation des tables** :
   - Ouvrez un terminal dans `Backend/src/API`.
   - Exécutez la commande suivante pour créer les tables via les migrations :
     ```powershell
     dotnet ef database update --project "../Infrastructure/Infrastructure.csproj" --startup-project "API.csproj"
     ```
   - *Alternative* : Vous pouvez aussi importer le fichier `Backend/src/API/novahire_schema.sql` directement dans votre base `novahiredb2`.

---

## 3. Lancer le Backend (.NET API)

1. Ouvrez un terminal dans le dossier `Backend/src/API`.
2. Restaurez les paquets et lancez l'application :
```powershell
dotnet restore
dotnet run --launch-profile http
```
- L'API sera accessible sur : `http://localhost:5000`
- Documentation Swagger : `http://localhost:5000/swagger`

---

## 4. Lancer le Service AI (Python)

Ce service gère l'analyse intelligente des CV et la génération de quiz.

1. Ouvrez un terminal dans `Backend/src/AI`.
2. Créez un environnement virtuel (recommandé) :
```powershell
python -m venv venv
.\venv\Scripts\activate
```
3. Installez les dépendances :
```powershell
pip install -r requirements.txt
```
4. Téléchargez les modèles linguistiques nécessaires :
```powershell
python -m spacy download fr_core_news_lg
python -m spacy download en_core_web_sm
```
5. **Configuration API** : Créez un fichier `.env` dans le dossier `Backend/src/AI` :
```text
GROQ_API_KEY=votre_cle_groq_ici
# OU
OPENAI_API_KEY=votre_cle_openai_ici
```
6. Lancez le service :
```powershell
python main.py
```
- Le service AI sera sur : `http://localhost:8000`

---

## 5. Lancer le Frontend (Vue.js)

1. Ouvrez un terminal dans le dossier `frontend`.
2. Installez les modules :
```powershell
npm install
```
3. Lancez le serveur de développement :
```powershell
npm run dev
```
- Le site sera accessible sur : `http://localhost:3010`

---

## 6. Informations de Connexion (Test)

Utilisez ces identifiants pour tester l'application :
- **Email** : `admin@neoledge.com`
- **Mot de passe** : `Admin@123`
- **Rôle** : SuperAdmin

---

## Résumé des accès :
- **Application Web** : `http://localhost:3010`
- **API Backend** : `http://localhost:5000`
- **Documentation API** : `http://localhost:5000/swagger`
- **Service AI** : `http://localhost:8000`
- **Base de données** : Port `5433` (PostgreSQL)

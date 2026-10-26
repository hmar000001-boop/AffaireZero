# Affaire Zéro — Godot 4

Premier prototype jouable d'enquête 3D pour Android. Le projet est conçu pour être compilé par GitHub Actions sans licence moteur payante.

## Contenu

- Exploration 3D low-poly générée à l'exécution (aucun asset externe requis).
- Contrôles tactiles : glisser sur le côté gauche pour avancer/se déplacer, glisser à droite pour regarder ; bouton Inspecter pour examiner un indice proche. Sur ordinateur : WASD/flèches et bouton Inspecter.
- Huit indices, trois suspects, un puzzle de trappe chronologique, conclusion fondée sur les preuves et trois niveaux d'indices.
- Interface anglais/arabe avec choix mémorisé, progression locale hors ligne.
- Workflow `.github/workflows/android-apk.yml` qui produit un APK de test signé debug comme artefact GitHub Actions.

## Obtenir l'APK depuis un téléphone

1. Crée un dépôt GitHub vide nommé `Affaire-Zero`.
2. Télécharge cette archive ZIP sur ton téléphone et extrais-la avec le gestionnaire de fichiers **ou** téléverse le ZIP dans ton dépôt puis utilise le navigateur GitHub pour extraire les fichiers dans le dépôt. GitHub n'exécute pas un workflow qui reste à l'intérieur d'un ZIP : `project.godot` et `.github/workflows/android-apk.yml` doivent être à la racine du dépôt.
3. Vérifie que le dépôt contient `project.godot`, `scenes/`, `scripts/` et `.github/workflows/android-apk.yml`.
4. Ouvre l'onglet Actions et lance **Affaire Zero - Godot Android APK** avec **Run workflow**. Le workflow se déclenche aussi lors d'un push de fichiers du projet sur `main`.
5. À la fin, ouvre le run réussi, section Artifacts, et télécharge `AffaireZero-Android-APK`. Décompresse l'artefact si GitHub le fournit en ZIP ; installe `AffaireZero.apk` sur Android. Autorise temporairement l'installation depuis cette source si Android le demande.

## Notes techniques

- Moteur : Godot 4.4.1, rendu `gl_compatibility` pour compatibilité mobile.
- Le workflow utilise `barichello/godot-ci:4.4.1`, qui fournit Godot, les templates d'export et les outils Android.
- APK debug pour test local uniquement ; pas destiné à Google Play.
- Pas d'achats intégrés dans cette première tranche.
- Les modèles 3D sont des formes simples créées par code : le jeu est un prototype fonctionnel de mécanique, pas encore un rendu photoréaliste.
- La scène et le script doivent encore être testés sur appareil réel ; un build vert ne remplace pas le test tactile et la validation de gameplay.

## Dépannage

Si l'action échoue, ouvre le run dans GitHub Actions et copie les 30 à 50 dernières lignes de l'étape rouge. Ne partage jamais de mots de passe ou de clés privées.

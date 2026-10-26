# Affaire Zéro — paquet unique

Ce paquet contient le projet Unity complet et les workflows CI GitHub Actions :
- `.github/workflows/request-unity-license.yml`
- `.github/workflows/android-apk.yml`
- `.github/workflows/affaire-zero-bootstrap.yml`

## Important concernant le workflow bootstrap
Le workflow `affaire-zero-bootstrap.yml` est prévu pour le scénario mobile où l'archive `Affaire_Zero_Unity_Project.zip` est téléversée à la racine d'un dépôt GitHub. Il est inclus ici avec les autres fichiers de CI afin que tout soit livré dans une seule archive. Pour que GitHub puisse exécuter un workflow, son fichier YAML doit être présent dans le dépôt sous `.github/workflows/` après l'ajout des fichiers au dépôt ; GitHub n'exécute pas directement les workflows encore enfermés dans un ZIP.

Le projet n'a pas été compilé avec Unity dans cet environnement. L'APK doit être généré par GitHub Actions après configuration de la licence Unity et vérifié avant toute publication.

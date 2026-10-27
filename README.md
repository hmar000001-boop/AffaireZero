# Affaire Zéro — Godot 4 / Android

A 3D investigation vertical slice built with Godot 4 and GDScript. This repository includes the game source and its GitHub Actions workflow.

## Build an Android test APK from a phone

1. Extract this ZIP on GitHub so that `project.godot` and `.github/` are at the repository root (not inside another nested folder).
2. In the GitHub repository, open **Actions** and enable workflows if GitHub asks.
3. Select **Affaire Zero - Godot Android APK** → **Run workflow** → **Run workflow**.
4. Open the completed run and download `AffaireZero-Android-APK` to install the debug APK. Download `AffaireZero-Build-Logs` if the build fails.

The workflow uses Godot 4.4.1 via `barichello/godot-ci:4.4.1`, validates/imports the project, runs a short headless smoke test, exports a debug-signed APK, and uploads logs even when a step fails.

## Notes

- This is a development prototype / vertical slice, not yet a commercial release candidate.
- The generated APK is debug-signed for local testing, not Play Store publication.
- Do not commit production signing keys or passwords.
- The first CI run may fail if the Godot Android export templates or Android SDK components are missing in the container. Review the uploaded logs; do not assume an APK exists unless the APK artifact is uploaded.

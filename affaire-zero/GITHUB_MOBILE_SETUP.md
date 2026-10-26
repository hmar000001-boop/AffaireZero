# GitHub Android-only build setup

This is the simplest path if you only have an Android phone. You do not need a local Unity installation.

## Files to upload

1. Create an empty GitHub repository.
2. Upload `Affaire_Zero_Unity_Project.zip` to the repository root. Do not rename it.
3. Use **Add file → Create new file** and name the file `.github/workflows/affaire-zero-bootstrap.yml`.
4. Copy the content from `docs/Affaire_Zero_GitHub_Bootstrap.yml` (or the separate `Affaire_Zero_GitHub_Bootstrap.yml` file included with the delivery) into the new workflow file and commit it to `main`.

GitHub must have this bootstrap workflow as a regular repository file; it cannot discover a workflow inside a ZIP that has not been extracted.

## Request a Unity activation file

1. Open the repository's **Actions** tab.
2. Select **Affaire Zero · GitHub mobile bootstrap**, tap **Run workflow**, select `request-license`, and run it.
3. Open that run, download the `Unity-activation-file` artifact and use the current GameCI/Unity instructions to convert the `.alf` activation request into a `.ulf` license file.
4. Open repository **Settings → Secrets and variables → Actions**, create `UNITY_LICENSE`, and paste the full contents of the `.ulf` file. If your licensing setup specifically requires them, also add `UNITY_EMAIL` and `UNITY_PASSWORD`. Never place credentials in source files.

## Build and download APK

1. In **Actions → Affaire Zero · GitHub mobile bootstrap → Run workflow**, select `build-apk`.
2. If the build fails, open the run and inspect its logs. Download `BuildReport.txt` if the custom build script reached the report-writing stage; for import/licensing failures, the Unity Editor log is the main diagnostic.
3. When successful, download artifact `AffaireZero-Android-*`, extract the APK from it on your phone, and install it. Android may ask you to allow installs from your browser or file manager.

The build is unsigned/debug-installable. Google Play production signing and store setup are not included. The current episode is free; purchases for future paid episodes must be integrated when those products exist.

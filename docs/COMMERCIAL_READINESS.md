# Commercial-readiness review — Affaire Zéro

## What this iteration improves

- More atmospheric, readable 3D blockout: contrasting warm/cool lights, sky/fog, room props, desk contents, shelves, pipes and evidence color language.
- A title screen, continue/restart actions, pause menu, portrait-independent landscape layout and bilingual controls.
- A more explicit investigation loop: collect evidence, consult case notes, compare the timeline, review suspects, use graded hints, open the hatch and make a deduction.
- Local persistence for evidence, hatch state, hint use, solved state and language.
- Debug APK build and upload workflow for phone-only GitHub Actions usage.
- Custom SVG/PNG project/app icon and a subtle procedural ambience loop.

## What still prevents a true commercial release

This iteration is not represented as production-certified. It has not been compiled or played on a physical Android device in this authoring environment. Its 3D objects are blockout geometry, not final art. No bespoke character models, animation, voice acting, bespoke music/foley, accessibility audit, device matrix, privacy review, store package, purchase integration or production release signing have been included. The current ambience is procedural placeholder sound, not a final audio mix. The build workflow is intended to expose script/export errors, not certify gameplay quality.

## Release gate

Do not charge players until all of these are true: Godot CI build passes; install/launch succeeds on the intended Android devices; a tester can finish the case without a softlock; Arabic RTL text and English fit the screen; save/restart works; movement and looking are comfortable; performance and thermal behaviour are acceptable; all purchased/third-party assets have documented licences; payment entitlement and restore purchases are verified server-side/store-side; privacy and store disclosures are complete; release AAB is signed with a private production key.

## Suggested art/audio brief

Modern realistic noir rather than fantasy horror. A believable archive building, practical lamps, restrained blue/amber palette, readable silhouettes, physically plausible evidence props, subtle room tone, rain/wind outside, footsteps, cloth/paper/metal interaction sounds. Avoid loud jumpscares. Mystery should be strange at first but fully explainable by the discovered evidence.

# Affaire Zéro — Design and implementation notes

## Product promise

A compact premium-style detective experience. The apparent impossible event is presented as strange and uncanny, but every decisive part of its solution must be explained by physical clues. Episode 01 is free and self-contained. Future episodes are intended to be sold individually once real paid content and store products exist.

## Case brief: The Impossible Absence

At 02:17 architect Elias Varen vanished from his locked study. The smart lock recorded no opening, a corridor camera showed an empty hall, and three witnesses heard footsteps move toward a blank wall. Three people have relevant information:

- **Nora Rahal**, house curator: claims she was in the kitchen; she feared the old service wing.
- **Malik Haddad**, security contractor: claims the hallway camera was live; he had access to the monitoring equipment, but access alone is not proof.
- **Elias Varen**, missing architect: his saved design review alludes to forgotten routes. The recovered voice note provides stronger evidence of advance planning.

## The evidence chain

- `locklog`: the study door remained locked; the exit did not use it.
- `camera_loop`: the corridor recording repeats an identical eleven-second sequence.
- `speaker`: a timed speaker routine played recorded footsteps.
- `breaker`: fresh tool marks and a 02:14 circuit reset establish a plausible way to isolate the camera.
- `footprints`: plaster dust connects the study-side bookcase to the old east-wing route.
- `notebook`: Elias's marked plan maps a service passage to the outer maintenance stair.
- `passage`: fresh latch scratches and one-directional dust prove the hatch was used recently.
- `voice_note`: Elias's draft connects the timed footsteps, clean lock log and audit files.

The valid conclusion requires at least three collected pieces of evidence, including: a route clue (`passage` or `notebook`), staging evidence (`camera_loop`, `speaker` or `voice_note`), and independent corroboration (`footprints`, `locklog` or `breaker`). This validates evidence-supported reasoning instead of a blind suspect guess.

## Resolution

Elias staged his own disappearance to leave through a concealed service passage with original audit files documenting a hidden surveillance system. He replayed the camera loop and scheduled footsteps through the speaker to create the impression of an impossible exit. The event is strange in appearance, ordinary in mechanism.

## Interaction / controls

- Android: left virtual joystick to move, drag on the right area to look, amber button to inspect a nearby clue.
- Editor/desktop: WASD/arrow keys; hold right mouse button to rotate the camera; click UI buttons.
- The screen can rotate between portrait and landscape. Canvas layout uses normalized anchors so controls reflow with screen dimensions.

## Architecture

- `GameBootstrap`: owns the case, initializes screen/build settings and routes gameplay events.
- `CaseManager`: validates clue IDs, tracks evidence, records the solved state and persists JSON locally in PlayerPrefs.
- `CaseCatalog`: all episode evidence definitions and the minimum evidence logic for a valid conclusion.
- `WorldBuilder`: creates walls, floors, lighting, furniture, evidence props and the first-person player from primitives.
- `PlayerController`: movement, camera look and nearby-interactable selection.
- `GameUI`: menu, bilingual HUD, collected evidence dossier, reactive interviews, theory selection and resolution panel.
- `ProjectSetup`: creates the first scene in the Unity Editor; `AndroidBuild` is the custom entry point called by GitHub Actions.

## Explicit non-goals of v0.1

No online account, cloud save, ads, analytics, live AI, or fabricated store purchase flow. A real commerce path is intentionally deferred until a second episode with a defined product ID exists. The current case does not require network access.

## Release acceptance checklist

- [ ] Unity 6.3 LTS import and script compilation succeed without errors.
- [ ] GameCI produces `BuildOutput/Android/AffaireZero.apk`.
- [ ] Fresh install starts in English and the user can switch to Arabic from the title menu.
- [ ] Arabic UI is readable in portrait and landscape on physical devices.
- [ ] Movement, look drag, interaction prompts and evidence inspection work on touchscreen.
- [ ] All eight clues can be reached and recorded; progress persists after closing/reopening.
- [ ] A conclusion with fewer than three clues is rejected.
- [ ] A wrong suspect or unsupported evidence set is rejected without corrupting the save.
- [ ] Elias plus a route clue, staging clue and independent corroboration resolves the case.
- [ ] Verify the generated APK on minimum/mid-range devices; tune shadows, effects and sensitivity.
- [ ] Add final app icon, screenshots, store disclosures and signing before Google Play release.

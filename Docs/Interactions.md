# Interaction Action System

This document outlines the modular interaction action graph introduced in the `feature/interaction-nodes` branch.

## Overview

Every interactive element in the scene exposes its hooks through the existing `Interactable` component. Designers attach one or more `InteractActionBase`-derived behaviours to respond to interaction events (Start, Progress, Complete, Cancel) without authoring new scripts.

### Event Flow

```
InteractionController (player)
   └── scans + focuses Interactable
            └── raises UnityEvents:
                • OnFocusEnter / Exit
                • OnInteractStart / Progress / Complete / Cancel
                        └── InteractActionBase listeners execute Action logic
```

The `InteractionController.Tick()` method already runs inside `AbilityController.Tick()` to keep the system event-driven—no new `Update()` loops were introduced.

## Core Components

- **InteractActionBase** – Abstract base that wires UnityEvents, optional actor filtering, and per-action cooldowns. Derived actions call `SetEventListening()` in `Awake()` when they need specific events.
- **InteractUtils** – Shared helpers for safe toggling of visuals, colliders, lights, particles, and UI canvas groups.
- **InteractGizmos** – Optional scene gizmo that visualises the Interactable range and current toggle state.
- **InteractionStateStore** – Lightweight static registry for sharing gate flags and counters across actions.

## Action Library

The following actions ship with default inspectors (all annotated and null-safe):

1. DoorAction – Toggle visual states & collider on complete.
2. MovingDoorAction – Animator-driven or manual moving door with cancel close.
3. PlatformToggleAction – Toggle any target hierarchy active state.
4. TimedSwitchAction – Enables a target for `durationMs` then reverts.
5. ElevatorCallAction – Fires an Animator trigger and optional SFX.
6. BridgeExtendAction – Toggle bridge visuals and colliders.
7. CircuitPatchAction – Updates emissive glow using a MaterialPropertyBlock.
8. ValveAction – Maps hold progress to rotation with cancel reset.
9. ColorKeyAction – Validates a controller `ColorKeyRing` against a required ID.
10. SequencePadAction – Broadcasts sequence step indices via UnityEvent.
11. WeightCheckAction – Evaluates rigidbody mass inside a trigger area.
12. TimeLeverAction – Scales `Time.timeScale` while held.
13. DialogueAction – Invokes start/advance dialogue events.
14. LogPickupAction – Adds collectibles and updates visuals once collected.
15. CutsceneTriggerAction – Plays or restarts a Timeline director.
16. MemoryFlashAction – Kicks an Animator trigger and updates overlay text.
17. ChoiceAction – Presents a CanvasGroup-driven choice and stores the result gate.
18. SfxAction – Plays configurable AudioSources on start/complete/cancel.
19. MusicSnapshotAction – Transitions to an AudioMixer snapshot.
20. LightToggleAction – Toggles light enable state & intensity.
21. FlickerAction – Periodic flicker during interaction.
22. ParticleBurstAction – Fires a particle burst on completion.
23. CameraShakeAction – Triggers a Cinemachine impulse.
24. ScreenFadeAction – Sets a CanvasGroup alpha for fades.
25. CheckpointAction – Emits spawn positions via UnityEvent.
26. SaveGameAction – Emits a generic save event.
27. QuestAdvanceAction – Emits quest progression payloads.
28. GateBoolAction – Writes boolean flags into the shared store.
29. MultiGateAction – Checks flag combinations before invoking.
30. CounterAction – Tracks repeated completes and fires when threshold reached.
31. PromptHintAction – Shows/hides hint UI on focus events.
32. ReticleSnapAction – Locks or releases aiming reticles via UnityEvents.
33. SlowTimeAction – Quick slow-motion toggle.
34. LogEventAction – Logs and profiles every interaction event.
35. GizmoPulseAction – Scales focus gizmos on focus.

## Editor Tooling

- **InteractableInspector** lists attached actions and supplies quick buttons to add common setups (Door, Platform Toggle, Dialogue panel).
- **Tools ▸ Interactions ▸ Create** menu spawns configured prefabs for Door, Moving Door, and Panel Trigger interactables.

## Prefabs & UI

Placeholder folders (`Assets/Prefabs/Interactions`, `Assets/UI/Interactions`) are ready for authoring reusable templates and UI overlays. Designers can duplicate objects from the demo scene into prefabs without extra wiring.

## Demo Scene

`Assets/Scenes/InteractionDemo.unity` (requires opening the project in Unity) showcases ten+ interaction nodes wired to `E` via the existing InteractionController. Each station demonstrates different action types (toggle, hold, timed switches, panels, gates, and FX).

## Performance Notes

- No per-frame allocations—the action base caches delegates and uses shared buffers/property blocks.
- Event-driven approach: only `InteractionController.Tick()` runs per-frame, feeding Interactable UnityEvents to actions.
- Utility methods guard against null references and redundant state changes to minimise work in the hot path.

## Smoke Test Checklist

1. Focus hints appear/disappear while stepping in/out of range.
2. Tap, toggle, hold, and panel interactions complete with matching responses.
3. Multiple interactables in range prioritise by `Interactable.priority`.
4. Cancelling hold interactions resets associated state (door closes, timescale restored).
5. Animator-driven doors keep collider state in sync with animation parameters.
6. Actions remain dormant while disabled, respecting local cooldown settings.

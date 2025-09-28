# Interaction System

This document describes the modular node-based interaction system that powers interactable objects in the project.

## Overview

* **Interactable** components expose UnityEvents for focus and interaction stages.
* **InteractionController** scans for nearby interactables, filters by actor requirements, and dispatches events while applying cooldown and hold logic.
* **InteractActionBase** enables modular behaviours by subscribing to any subset of the interactable events (focus enter/exit, start, progress, complete, cancel).
* Designers compose behaviour by stacking one or more `InteractActionBase` derived components on an interactable prefab. Each action is self-contained and event-driven.

## Event Flow

1. The controller discovers an interactable and fires `OnFocusEnter`.
2. When the player begins an interaction the controller fires `OnInteractStart` and periodically `OnInteractProgress`.
3. Completing the action invokes `OnInteractComplete`; releasing or interruption invokes `OnInteractCancel`.
4. Actions subscribe to any of these stages, perform their behaviour, and optionally set local cooldowns or actor filters.

```
Focus Enter → (optional) Focus Exit → Start → Progress → Complete/Cancel
```

## Creating Actions

All actions inherit from `InteractActionBase` and should:

* Call `SetDefaultListeners(...)` in `Reset()` to choose the events to subscribe to.
* Cache component references in `OnEnable`.
* Override whichever virtual hooks are required (`OnFocusEnter`, `OnStart`, etc.).
* Avoid allocations and per-frame updates; rely on Invoke/coroutines for timed behaviour.

### Local Cooldown

Set the `localCooldownMs` field to throttle an action independently of the interactable's cooldown. Internally the base class uses `Time.unscaledTimeAsDouble` for precision without allocations.

### Actor Filtering

Enable the actor filter to restrict the action to a specific `InteractionActor`. This allows shared interactables where only certain controllers trigger a behaviour.

## Utility Helpers

`InteractUtils` centralises reusable helpers for safely toggling GameObjects, sprites, animators, particles, and canvas groups. `InteractGizmos` renders focus/range debug visuals in the editor.

`InteractionStateRegistry` provides a lightweight key/value store for cross-action communication (e.g. gate flags, counters, quest progression). It avoids runtime allocations and supports bool/int values.

## Action Library

The project ships with 35 ready-to-use actions grouped by purpose:

* **World/Level** – `DoorAction`, `MovingDoorAction`, `PlatformToggleAction`, `TimedSwitchAction`, `ElevatorCallAction`, `BridgeExtendAction`.
* **Puzzles/System** – `CircuitPatchAction`, `ValveAction`, `ColorKeyAction`, `SequencePadAction`, `WeightCheckAction`, `TimeLeverAction`.
* **Narrative/UI** – `DialogueAction`, `LogPickupAction`, `CutsceneTriggerAction`, `MemoryFlashAction`, `ChoiceAction`.
* **FX/Audio/Camera** – `SfxAction`, `MusicSnapshotAction`, `LightToggleAction`, `FlickerAction`, `ParticleBurstAction`, `CameraShakeAction`, `ScreenFadeAction`.
* **Game State** – `CheckpointAction`, `SaveGameAction`, `QuestAdvanceAction`, `GateBoolAction`, `MultiGateAction`, `CounterAction`.
* **UX/Accessibility/Debug** – `PromptHintAction`, `ReticleSnapAction`, `SlowTimeAction`, `LogEventAction`, `GizmoPulseAction`.

Each action exposes clear inspector fields with tooltips to configure visuals, animators, events, or IDs. Multiple actions can coexist on the same GameObject to compose richer behaviour (e.g. toggle a door, play SFX, update quests).

## Editor Tooling

A custom inspector for `Interactable` lists attached actions and event listener counts. Use the "Quick Bind" button to connect the `DoorAction.Toggle` helper without manually dragging references. Menu items under **Tools → Interactions → Create** rapidly instantiate preconfigured prefabs with the required components and gizmos.

## Demo Scene

Open `Assets/Scenes/InteractionDemo.unity` to explore the full catalogue:

* Tap, toggle, hold, and panel interactions mapped to the **E** key.
* 10+ example stations demonstrating door toggles, timed switches, valves, puzzles, FX bursts, camera shakes, UI fades, and more.
* UI prompts and reticle snapping showcase focus hints and accessibility helpers.

## Performance Notes

* Actions avoid `Update`; they react exclusively to events.
* Component references are cached and helper methods guard against nulls.
* Lightweight timers rely on `Invoke` or coroutines for short-lived sequences.
* The state registry uses static dictionaries shared across actions; call `InteractionStateRegistry.ClearAll()` when resetting gameplay state.

## Testing Checklist

* Focus hints appear/disappear with `PromptHintAction`.
* Tap, toggle, hold, and panel interactions function across demo objects.
* Multiple interactables in range respect priority ordering.
* Cancelling hold interactions rewinds state (doors close, slow time resets).
* Animator-driven doors stay in sync with colliders and visuals.

## Extending

To add a new action:

1. Derive from `InteractActionBase` in `Assets/Scripts/Interactions/Actions`.
2. Select relevant events via `SetDefaultListeners`.
3. Expose inspector fields and tooltips for clarity.
4. Optionally add a menu item or documentation snippet.

This modular approach keeps gameplay logic granular, reusable, and designer-friendly.

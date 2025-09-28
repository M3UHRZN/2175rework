# 2175 Interaction Nodes

This branch introduces a modular, node-based interaction system layered on top of the existing `Interactable` and `InteractionController` components.

## Highlights

- 35 ready-to-use `InteractActionBase` behaviours covering world, puzzle, narrative, FX, game-state, and UX scenarios.
- Event-driven architecture with optional actor filtering and per-action cooldowns.
- Editor extensions listing attached actions with quick buttons plus menu items under **Tools ▸ Interactions ▸ Create** for rapid scene setup.
- Demo scene: `Assets/Scenes/InteractionDemo.unity` showcases tap, toggle, hold, timed, puzzle, and FX interactions triggered with the **E** key.
- Shared utilities (`InteractUtils`, `InteractionStateStore`, `InteractGizmos`) keep actions null-safe and allocation-free.

See [Docs/Interactions.md](Docs/Interactions.md) for full documentation, setup notes, and a smoke test checklist.

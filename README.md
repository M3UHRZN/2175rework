# 2175 Rework Interaction Nodes

This branch introduces a modular, node-driven interaction system built on top of the existing `Interactable` and `InteractionController` components.

* Stackable `InteractActionBase` behaviours let designers mix and match doors, switches, FX, dialogue, quest gates, and more without writing code.
* Editor tooling lists attached actions, shows UnityEvent bindings, and provides quick setup buttons under **Tools → Interactions → Create**.
* A demo scene showcases over ten interaction examples (tap, toggle, hold, and panel workflows) all triggered with the **E** key.

📄 See [`Docs/Interactions.md`](Docs/Interactions.md) for full documentation, event diagrams, and the testing checklist.

🎮 Open `Assets/Scenes/InteractionDemo.unity` to try the complete showcase.

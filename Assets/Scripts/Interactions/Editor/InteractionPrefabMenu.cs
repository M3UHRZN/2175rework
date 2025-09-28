using UnityEditor;
using UnityEngine;

public static class InteractionPrefabMenu
{
    [MenuItem("Tools/Interactions/Create/Door", priority = 10)]
    static void CreateDoor()
    {
        var root = new GameObject("DoorInteraction");
        Undo.RegisterCreatedObjectUndo(root, "Create Door Interaction");

        var interactable = root.AddComponent<Interactable>();
        interactable.interactionType = InteractionType.Toggle;

        var collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        var closed = new GameObject("Closed");
        closed.transform.SetParent(root.transform, false);
        var open = new GameObject("Open");
        open.transform.SetParent(root.transform, false);

        var door = root.AddComponent<DoorAction>();
        var serializedDoor = new SerializedObject(door);
        serializedDoor.FindProperty("closedVisual").objectReferenceValue = closed.transform;
        serializedDoor.FindProperty("openVisual").objectReferenceValue = open.transform;
        serializedDoor.FindProperty("blockingCollider").objectReferenceValue = collider;
        serializedDoor.ApplyModifiedProperties();

        Selection.activeGameObject = root;
    }

    [MenuItem("Tools/Interactions/Create/Moving Door", priority = 11)]
    static void CreateMovingDoor()
    {
        var root = new GameObject("MovingDoorInteraction");
        Undo.RegisterCreatedObjectUndo(root, "Create Moving Door Interaction");

        var interactable = root.AddComponent<Interactable>();
        interactable.interactionType = InteractionType.Toggle;

        root.AddComponent<BoxCollider2D>().isTrigger = true;
        root.AddComponent<MovingDoorAction>();

        Selection.activeGameObject = root;
    }

    [MenuItem("Tools/Interactions/Create/Panel Trigger", priority = 12)]
    static void CreatePanelTrigger()
    {
        var root = new GameObject("PanelTriggerInteraction");
        Undo.RegisterCreatedObjectUndo(root, "Create Panel Trigger Interaction");

        var interactable = root.AddComponent<Interactable>();
        interactable.interactionType = InteractionType.Panel;
        interactable.holdDurationMs = 1000f;

        root.AddComponent<BoxCollider2D>().isTrigger = true;
        root.AddComponent<SequencePadAction>();

        Selection.activeGameObject = root;
    }
}

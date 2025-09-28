#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class InteractionMenuItems
{
    [MenuItem("Tools/Interactions/Create/Door", priority = 0)]
    public static void CreateDoor()
    {
        var go = CreateBase("Door");
        var interactable = go.GetComponent<Interactable>();
        interactable.interactionType = InteractionType.Toggle;
        go.AddComponent<DoorAction>();
        FinalizeCreation(go);
    }

    [MenuItem("Tools/Interactions/Create/Moving Door", priority = 1)]
    public static void CreateMovingDoor()
    {
        var go = CreateBase("MovingDoor");
        var interactable = go.GetComponent<Interactable>();
        interactable.interactionType = InteractionType.Toggle;
        go.AddComponent<MovingDoorAction>();
        FinalizeCreation(go);
    }

    [MenuItem("Tools/Interactions/Create/Panel Toggle", priority = 2)]
    public static void CreatePanelToggle()
    {
        var go = CreateBase("PanelToggle");
        var interactable = go.GetComponent<Interactable>();
        interactable.interactionType = InteractionType.Toggle;
        go.AddComponent<PlatformToggleAction>();
        FinalizeCreation(go);
    }

    static GameObject CreateBase(string name)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.AddComponent<BoxCollider2D>().isTrigger = true;
        go.AddComponent<Interactable>();
        go.AddComponent<InteractGizmos>();
        Selection.activeGameObject = go;
        return go;
    }

    static void FinalizeCreation(GameObject go)
    {
        EditorUtility.SetDirty(go);
    }
}
#endif

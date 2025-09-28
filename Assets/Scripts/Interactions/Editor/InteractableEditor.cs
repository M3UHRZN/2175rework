using UnityEditor;
using UnityEngine;
using Interactions.Core;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var interactable = (Interactable)target;
        if (!interactable)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Interaction Actions", EditorStyles.boldLabel);

        var actions = interactable.GetComponents<InteractActionBase>();
        if (actions.Length == 0)
        {
            EditorGUILayout.HelpBox("No InteractActionBase components attached. Add one to author modular behaviour.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                if (!action)
                    continue;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(action.GetType().Name, action, action.GetType(), true);
                    if (GUILayout.Button("Select", GUILayout.Width(60f)))
                    {
                        Selection.activeObject = action;
                    }
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Create", EditorStyles.boldLabel);

        if (GUILayout.Button("Quick-Bind: Complete → DoorAction.Toggle()"))
        {
            EnsureAction<DoorAction>(interactable);
        }
        if (GUILayout.Button("Quick-Bind: Complete → PlatformToggleAction.Toggle()"))
        {
            EnsureAction<PlatformToggleAction>(interactable);
        }
        if (GUILayout.Button("Quick-Bind: Hold → DialogueAction"))
        {
            interactable.interactionType = InteractionType.Hold;
            EnsureAction<DialogueAction>(interactable);
        }
    }

    static void EnsureAction<T>(Interactable interactable) where T : Component
    {
        var action = interactable.GetComponent<T>();
        if (!action)
        {
            action = interactable.gameObject.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(action, "Add Interaction Action");
        }
        Selection.activeObject = action;
        EditorUtility.SetDirty(interactable);
    }
}

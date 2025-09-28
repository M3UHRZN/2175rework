#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor
{
    SerializedProperty scriptProp;

    void OnEnable()
    {
        scriptProp = serializedObject.FindProperty("m_Script");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
        {
            if (scriptProp != null)
                EditorGUILayout.PropertyField(scriptProp);
        }

        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        DrawActionsSection();
        EditorGUILayout.Space();
        DrawQuickBinders();
    }

    void DrawActionsSection()
    {
        var interactable = (Interactable)target;
        var actions = interactable.GetComponents<InteractActionBase>();
        EditorGUILayout.LabelField("Attached Actions", EditorStyles.boldLabel);
        if (actions.Length == 0)
        {
            EditorGUILayout.HelpBox("No InteractActionBase components attached.", MessageType.Info);
            return;
        }

        foreach (var action in actions)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField(action.GetType().Name, action, typeof(InteractActionBase), true);
                EditorGUILayout.LabelField(action.enabled ? "Enabled" : "Disabled", GUILayout.Width(70f));
            }
        }

        EditorGUILayout.Space();
        DrawEventStatus(interactable);
    }

    void DrawEventStatus(Interactable interactable)
    {
        EditorGUILayout.LabelField("Event Bindings", EditorStyles.boldLabel);
        DrawEventLine("OnFocusEnter", interactable.OnFocusEnter?.GetPersistentEventCount() ?? 0);
        DrawEventLine("OnFocusExit", interactable.OnFocusExit?.GetPersistentEventCount() ?? 0);
        DrawEventLine("OnInteractStart", interactable.OnInteractStart?.GetPersistentEventCount() ?? 0);
        DrawEventLine("OnInteractProgress", interactable.OnInteractProgress?.GetPersistentEventCount() ?? 0);
        DrawEventLine("OnInteractComplete", interactable.OnInteractComplete?.GetPersistentEventCount() ?? 0);
        DrawEventLine("OnInteractCancel", interactable.OnInteractCancel?.GetPersistentEventCount() ?? 0);
    }

    void DrawEventLine(string label, int count)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(160f));
            EditorGUILayout.LabelField(count.ToString(), GUILayout.Width(40f));
        }
    }

    void DrawQuickBinders()
    {
        var interactable = (Interactable)target;
        var doorAction = interactable.GetComponent<DoorAction>();
        if (doorAction)
        {
            EditorGUILayout.LabelField("Quick Bind", EditorStyles.boldLabel);
            if (GUILayout.Button("Complete → DoorAction.Toggle()"))
            {
                UnityAction<InteractionController> call = doorAction.Toggle;
                UnityEventTools.AddPersistentListener(interactable.OnInteractComplete, call);
                EditorUtility.SetDirty(interactable);
                EditorUtility.SetDirty(doorAction);
            }
        }
    }
}
#endif

using UnityEngine;

[DisallowMultipleComponent]
public class DualCharacterHUD : MonoBehaviour
{
    public DualCharacterController party;

    public Vector2 basePosition = new Vector2(20f, 20f);
    public float lineHeight = 22f;
    public float holdBarWidth = 160f;
    public float holdBarHeight = 16f;

    void OnGUI()
    {
        if (!party)
            return;

        int line = 0;
        float x = basePosition.x;
        float y = basePosition.y;

        var active = party.Active;
        string activeName = active ? active.name : "None";
        GUI.Label(new Rect(x, y + lineHeight * line++, 320f, lineHeight), $"Active: {activeName} | Merged: {party.IsMerged}");
        GUI.Label(new Rect(x, y + lineHeight * line++, 320f, lineHeight), $"Control Locked: {party.ControlLocked}");

        var interaction = active ? active.GetComponent<InteractionController>() : null;
        if (interaction)
        {
            string focusName = interaction.Focused ? interaction.Focused.name : "-";
            GUI.Label(new Rect(x, y + lineHeight * line++, 420f, lineHeight), $"Focus: {focusName} | Dist: {interaction.FocusedDistance:0.00} | Priority: {interaction.FocusedPriority}");

            if (interaction.Focused)
            {
                GUI.Label(new Rect(x, y + lineHeight * line++, 420f, lineHeight), $"E: Etkileşim");
                if (interaction.IsHolding)
                {
                    float progress = Mathf.Clamp01(interaction.HoldProgress);
                    Rect bar = new Rect(x, y + lineHeight * line, holdBarWidth, holdBarHeight);
                    GUI.Box(bar, GUIContent.none);
                    Rect fill = new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * progress, bar.height - 4f);
                    GUI.Box(fill, GUIContent.none);
                    line++;
                }
            }
        }
    }
}

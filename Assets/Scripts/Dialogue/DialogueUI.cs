using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogueUI : MonoBehaviour
{
    [Tooltip("Konuşmacı adını gösterecek Text bileşeni.")]
    public TMP_Text speakerLabel;

    [Tooltip("Konuşma metnini gösterecek Text bileşeni.")]
    public TMP_Text messageLabel;

    [Tooltip("Konuşma balonunun GameObject referansı.")]
    public GameObject bubbleRoot;

    void Awake()
    {
        if (bubbleRoot == null && messageLabel != null)
            bubbleRoot = messageLabel.gameObject;
    }

    public void ShowLine(string displayName, string fallbackId, string message)
    {
        if (bubbleRoot != null && !bubbleRoot.activeSelf)
            bubbleRoot.SetActive(true);

        if (speakerLabel != null)
            speakerLabel.text = !string.IsNullOrEmpty(displayName) ? displayName : fallbackId;

        if (messageLabel != null)
            messageLabel.text = message;
    }

    public void Hide()
    {
        if (messageLabel != null)
            messageLabel.text = string.Empty;

        if (speakerLabel != null)
            speakerLabel.text = string.Empty;

        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}

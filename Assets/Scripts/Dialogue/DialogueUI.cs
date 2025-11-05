using System;
using System.Collections;
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

    public event Action TypingCompleted;

    public bool IsTyping => typingRoutine != null;

    Coroutine typingRoutine;
    string fullText = string.Empty;

    void Awake()
    {
        if (bubbleRoot == null && messageLabel != null)
            bubbleRoot = messageLabel.gameObject;
    }

    public void ShowLine(string displayName, string fallbackId, string message, float charactersPerSecond)
    {
        if (bubbleRoot != null && !bubbleRoot.activeSelf)
            bubbleRoot.SetActive(true);

        if (speakerLabel != null)
            speakerLabel.text = !string.IsNullOrEmpty(displayName) ? displayName : fallbackId;

        StartTyping(message, charactersPerSecond);
    }

    public void Hide()
    {
        StopTyping();

        if (messageLabel != null)
        {
            messageLabel.text = string.Empty;
            messageLabel.maxVisibleCharacters = 0;
        }

        if (speakerLabel != null)
            speakerLabel.text = string.Empty;

        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }

    public void CompleteTyping()
    {
        if (!IsTyping)
            return;

        StopTyping();
        ApplyFullText();
        RaiseTypingCompleted();
    }

    void StartTyping(string message, float charactersPerSecond)
    {
        StopTyping();

        fullText = message ?? string.Empty;

        if (messageLabel == null)
        {
            RaiseTypingCompleted();
            return;
        }

        if (charactersPerSecond <= 0f)
        {
            messageLabel.text = fullText;
            messageLabel.maxVisibleCharacters = fullText.Length;
            RaiseTypingCompleted();
            return;
        }

        messageLabel.text = fullText;
        messageLabel.maxVisibleCharacters = 0;
        typingRoutine = StartCoroutine(TypeText(charactersPerSecond));
    }

    IEnumerator TypeText(float charactersPerSecond)
    {
        if (fullText.Length == 0)
        {
            typingRoutine = null;
            RaiseTypingCompleted();
            yield break;
        }

        float delay = 1f / charactersPerSecond;
        var wait = new WaitForSeconds(delay);
        for (int i = 0; i < fullText.Length; i++)
        {
            if (messageLabel != null)
                messageLabel.maxVisibleCharacters = i + 1;

            yield return wait;
        }

        typingRoutine = null;
        ApplyFullText();
        RaiseTypingCompleted();
    }

    void StopTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }
    }

    void ApplyFullText()
    {
        if (messageLabel != null)
        {
            messageLabel.text = fullText;
            messageLabel.maxVisibleCharacters = fullText.Length;
        }
    }

    void RaiseTypingCompleted()
    {
        TypingCompleted?.Invoke();
    }
}

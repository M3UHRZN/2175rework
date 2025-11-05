using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Sequence", fileName = "DialogueSequence")]
public class DialogueSequence : ScriptableObject
{
    [Tooltip("Konuşma sırasındaki diyalog cümleleri.")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Tooltip("Konuşma tamamlandığında başka bir davranış tetiklemek için.")]
    public UnityEngine.Events.UnityEvent onSequenceFinished;

    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Bu satırı kimin söylediğini belirtmek için benzersiz karakter kimliği.")]
        public string speakerId;

        [Tooltip("UI'da gösterilecek konuşmacı ismi. Boş bırakılırsa speakerId kullanılır.")]
        public string speakerDisplayName;

        [TextArea]
        [Tooltip("Sohbet baloncuğunda gösterilecek metin.")]
        public string text;

        [Tooltip("Bu satır sırasında hangi karakterlerin hareketinin kilitleneceği.")]
        public DialogueMovementLock movementLock = DialogueMovementLock.None;
    }
}

public enum DialogueMovementLock
{
    None,
    Speaker,
    AllBoundCharacters
}

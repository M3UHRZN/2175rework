using UnityEngine;

namespace Interactions.Core
{
    public class ColorKeyRing : MonoBehaviour
    {
        [Tooltip("Key identifier currently held by this actor.")]
        [SerializeField] string currentKeyId;

        public string CurrentKeyId => currentKeyId;

        public void SetKey(string keyId)
        {
            currentKeyId = keyId;
        }
    }
}

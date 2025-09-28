using UnityEngine;

namespace Interactions.Core
{
    [ExecuteAlways]
    public class InteractGizmos : MonoBehaviour
    {
        [Tooltip("Optional custom color for the interaction gizmos.")]
        public Color gizmoColor = new Color(0.25f, 0.9f, 1f, 0.75f);
        [Tooltip("Optional focus color when the object is highlighted.")]
        public Color focusColor = new Color(1f, 0.8f, 0.2f, 0.9f);

        Interactable cachedInteractable;

        void OnDrawGizmosSelected()
        {
            if (!cachedInteractable)
                cachedInteractable = GetComponent<Interactable>();

            if (!cachedInteractable)
                return;

            Vector3 position = cachedInteractable.transform.position;
            float range = Mathf.Max(0.01f, cachedInteractable.range);

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(position, range);

            if (cachedInteractable.ToggleState)
            {
                Gizmos.color = focusColor;
                Gizmos.DrawSphere(position, 0.05f);
            }
        }
    }
}

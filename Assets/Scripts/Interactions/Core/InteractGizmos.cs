using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Interactable))]
public class InteractGizmos : MonoBehaviour
{
    [SerializeField]
    Color focusColor = new Color(0.2f, 0.9f, 1f, 0.6f);
    [SerializeField]
    Color rangeColor = new Color(0.1f, 0.5f, 1f, 0.35f);

    Interactable interactable;

    void OnValidate()
    {
        if (!interactable)
            interactable = GetComponent<Interactable>();
    }

    void Awake()
    {
        if (!interactable)
            interactable = GetComponent<Interactable>();
    }

    void OnDrawGizmosSelected()
    {
        if (!interactable)
            interactable = GetComponent<Interactable>();
        if (!interactable)
            return;

        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(transform.position, interactable.range);

        Gizmos.color = focusColor;
        Gizmos.DrawWireSphere(transform.position, interactable.range * 0.35f);
    }
}

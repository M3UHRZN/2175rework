
using UnityEngine;

/// <summary>
/// Rotates a target transform based on the progress of a 'Hold' interaction.
/// It automatically calculates and sets the Interactable's hold duration based on
/// the desired angle and rotation speed.
/// If the hold is cancelled, it animates the transform back to its start rotation.
/// </summary>
[RequireComponent(typeof(Interactable))]
public class HoldRotateController : MonoBehaviour
{
    [Header("Rotation Target")]
    [Tooltip("The transform to rotate. If null, this GameObject's transform is used.")]
    public Transform targetTransform;

    [Header("Rotation Setup")]
    [Tooltip("The axis around which to rotate.")]
    public Vector3 rotationAxis = Vector3.forward;

    [Tooltip("The total angle to rotate in degrees.")]
    public float targetAngle = 180f;

    [Tooltip("The speed of rotation in degrees per second.")]
    public float rotationSpeed = 90f;

    [Header("Return Animation")]
    [Tooltip("How quickly the object returns to the start position after being cancelled.")]
    public float returnSpeed = 1.5f;

    [Tooltip("The curve defining the motion of the return animation.")]
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Interactable _interactable;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    private bool _isReturning = false;
    private float _returnProgress = 0f;
    private Quaternion _returnStartRotation;

    void Awake()
    {
        _interactable = GetComponent<Interactable>();

        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        if (_interactable.interactionType != InteractionType.Hold)
        {
            Debug.LogError("HoldRotateController requires the Interactable's Interaction Type to be 'Hold'.", this);
            enabled = false;
            return;
        }

        // Auto-synchronize the hold duration.
        if (rotationSpeed > 0)
        {
            float requiredDurationSecs = Mathf.Abs(targetAngle) / rotationSpeed;
            _interactable.holdDurationMs = requiredDurationSecs * 1000f;
        }
        else
        {
            _interactable.holdDurationMs = float.MaxValue; // Prevent completion if speed is zero
        }
    }

    void Start()
    {
        _startRotation = targetTransform.localRotation;
        _targetRotation = _startRotation * Quaternion.Euler(rotationAxis.normalized * targetAngle);
    }

    void OnEnable()
    {
        _interactable.OnInteractProgress.AddListener(HandleProgress);
        _interactable.OnInteractCancel.AddListener(HandleCancel);
        _interactable.OnInteractComplete.AddListener(HandleComplete);
    }

    void OnDisable()
    {
        _interactable.OnInteractProgress.RemoveListener(HandleProgress);
        _interactable.OnInteractCancel.RemoveListener(HandleCancel);
        _interactable.OnInteractComplete.RemoveListener(HandleComplete);
    }

    void Update()
    {
        if (_isReturning)
        {
            _returnProgress += Time.deltaTime * returnSpeed;
            float curveValue = returnCurve.Evaluate(_returnProgress);
            targetTransform.localRotation = Quaternion.SlerpUnclamped(_returnStartRotation, _startRotation, curveValue);

            if (_returnProgress >= 1f)
            {
                _isReturning = false;
            }
        }
    }

    private void HandleProgress(float progress)
    {
        _isReturning = false;
        float currentAngle = targetAngle * progress;
        targetTransform.localRotation = _startRotation * Quaternion.Euler(rotationAxis.normalized * currentAngle);
    }

    private void HandleCancel(InteractionController controller)
    {
        _isReturning = true;
        _returnProgress = 0f;
        _returnStartRotation = targetTransform.localRotation;
    }

    private void HandleComplete(InteractionController controller)
    {
        _isReturning = false;
        targetTransform.localRotation = _targetRotation;
    }
}

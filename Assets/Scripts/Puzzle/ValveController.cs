
using UnityEngine;

/// <summary>
/// Manages a valve-like interaction. It uses the progress of a 'Hold' interaction
/// to drive an animator parameter. If the hold is cancelled, it animates the parameter
/// back to zero over time.
/// </summary>
[RequireComponent(typeof(Animator), typeof(Interactable))]
public class ValveController : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The name of the float parameter in the Animator to control the valve's rotation.")]
    public string animatorParameterName = "TurnProgress";

    [Header("Return Animation")]
    [Tooltip("How quickly the valve returns to the start position after being cancelled.")]
    public float returnSpeed = 1.5f;

    [Tooltip("The curve defining the motion of the return animation.")]
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Animator _animator;
    private Interactable _interactable;

    private bool _isReturning = false;
    private float _returnProgress = 0f;
    private float _startValue = 0f;
    private int _animatorParamHash;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _interactable = GetComponent<Interactable>();

        if (_interactable.interactionType != InteractionType.Hold)
        {
            Debug.LogWarning("ValveController requires the Interactable's Interaction Type to be 'Hold'.", this);
        }

        if (!string.IsNullOrEmpty(animatorParameterName))
        {
            _animatorParamHash = Animator.StringToHash(animatorParameterName);
        }
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
            float currentValue = Mathf.Lerp(_startValue, 0f, curveValue);

            _animator.SetFloat(_animatorParamHash, currentValue);

            if (_returnProgress >= 1f)
            {
                _isReturning = false;
            }
        }
    }

    private void HandleProgress(float progress)
    {
        // When the user is actively holding, stop any return animation and sync with their progress.
        _isReturning = false;
        _animator.SetFloat(_animatorParamHash, progress);
    }

    private void HandleCancel(InteractionController controller)
    {
        // The user let go early. Start the return animation from the current value.
        _isReturning = true;
        _returnProgress = 0f;
        _startValue = _animator.GetFloat(_animatorParamHash);
    }

    private void HandleComplete(InteractionController controller)
    {
        // The user successfully held for the full duration. Ensure the animation is at 100%.
        _isReturning = false;
        _animator.SetFloat(_animatorParamHash, 1f);
    }
}

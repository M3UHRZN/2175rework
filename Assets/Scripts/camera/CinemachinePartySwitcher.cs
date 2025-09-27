using UnityEngine;
using Unity.Cinemachine;

public class CinemachinePartySwitcher : MonoBehaviour
{
    [Header("Party")]
    public DualCharacterController party;

    [Header("Cameras")]
    public CinemachineCamera vcamMerged;
    public CinemachineCamera vcamElior;
    public CinemachineCamera vcamSim;

    [Header("Profiles")]
    public CameraProfile mergedProfile;
    public CameraProfile eliorProfile;
    public CameraProfile simProfile;

    private void Awake()
    {
        if (!party)
        {
            party = FindObjectOfType<DualCharacterController>();
        }

        var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain != null)
        {
            brain.DefaultBlend.BlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            brain.DefaultBlend.Duration = 0.5f;
        }

        ApplyProfiles();
    }

    private void OnEnable()
    {
        if (party != null)
        {
            party.OnActiveCharacterChanged.AddListener(OnActiveChanged);
            party.OnMergedStateChanged.AddListener(OnMergedChanged);
        }

        ApplyState();
    }

    private void OnDisable()
    {
        if (party != null)
        {
            party.OnActiveCharacterChanged.RemoveListener(OnActiveChanged);
            party.OnMergedStateChanged.RemoveListener(OnMergedChanged);
        }
    }

    public void OnActiveChanged(string _)
    {
        ApplyState();
    }

    public void OnMergedChanged(bool _)
    {
        ApplyState();
    }

    public void ApplyProfiles()
    {
        void Setup(CinemachineCamera cam, CameraProfile profile)
        {
            if (cam == null || profile == null)
            {
                return;
            }

            var composer = cam.GetComponent<CinemachinePositionComposer>();
            if (composer == null)
            {
                composer = cam.gameObject.AddComponent<CinemachinePositionComposer>();
            }

            composer.TrackedObjectOffset = new Vector3(profile.followOffset.x, profile.followOffset.y, 0f);
            composer.Damping = new Vector3(profile.positionDamping, profile.positionDamping, profile.positionDamping);
            cam.Lens.FieldOfView = profile.fov;

            var transform = cam.transform;
            var position = transform.localPosition;
            position.z = profile.followOffset.z;
            transform.localPosition = position;
        }

        Setup(vcamMerged, mergedProfile);
        Setup(vcamElior, eliorProfile);
        Setup(vcamSim, simProfile);
    }

    private void ApplyState()
    {
        if (party == null)
        {
            return;
        }

        if (party.IsMerged)
        {
            SetPriorities(30, 5, 5);
            return;
        }

        var eliorActive = party.Active == party.elior;
        SetPriorities(5, eliorActive ? 30 : 10, eliorActive ? 10 : 30);
    }

    private void SetPriorities(int merged, int elior, int sim)
    {
        if (vcamMerged != null)
        {
            vcamMerged.Priority = merged;
        }

        if (vcamElior != null)
        {
            vcamElior.Priority = elior;
        }

        if (vcamSim != null)
        {
            vcamSim.Priority = sim;
        }
    }
}

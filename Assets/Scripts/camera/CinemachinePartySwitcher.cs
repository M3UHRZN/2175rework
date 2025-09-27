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

        var brain = Camera.main ? Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>() : null;
        if (brain)
        {
            brain.DefaultBlend.BlendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            brain.DefaultBlend.Duration = 0.5f;
        }

        ApplyProfiles();
    }

    private void OnEnable()
    {
        if (party)
        {
            party.OnActiveCharacterChanged.AddListener(OnActiveChanged);
            party.OnMergedStateChanged.AddListener(OnMergedChanged);
        }

        ApplyState();
    }

    private void OnDisable()
    {
        if (party)
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

    private void ApplyProfiles()
    {
        void Setup(CinemachineCamera cam, CameraProfile profile)
        {
            if (!cam || !profile)
            {
                return;
            }

            var composer = cam.GetComponent<CinemachinePositionComposer>();
            if (!composer)
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
        if (!party)
        {
            return;
        }

        if (party.IsMerged)
        {
            SetPriority(30, 5, 5);
            return;
        }

        bool eliorActive = party.Active == party.elior;
        SetPriority(5, eliorActive ? 30 : 10, eliorActive ? 10 : 30);
    }

    private void SetPriority(int merged, int elior, int sim)
    {
        if (vcamMerged)
        {
            vcamMerged.Priority = merged;
        }

        if (vcamElior)
        {
            vcamElior.Priority = elior;
        }

        if (vcamSim)
        {
            vcamSim.Priority = sim;
        }
    }
}

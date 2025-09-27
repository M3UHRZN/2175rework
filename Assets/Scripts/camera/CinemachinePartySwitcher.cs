using Unity.Cinemachine;
using UnityEngine;

public class CinemachinePartySwitcher : MonoBehaviour
{
    public DualCharacterController party;
    public CinemachineVirtualCamera vcamMerged;
    public CinemachineVirtualCamera vcamElior;
    public CinemachineVirtualCamera vcamSim;

    void Start()
    {
        if (!party)
            party = FindObjectOfType<DualCharacterController>();

        vcamMerged = EnsureCamera(vcamMerged, "vcam_Merged");
        vcamElior = EnsureCamera(vcamElior, "vcam_Elior");
        vcamSim = EnsureCamera(vcamSim, "vcam_Sim");

        ConfigureCameras();

        if (party)
        {
            party.OnActiveCharacterChanged.AddListener(OnActiveChanged);
            party.OnMergedStateChanged.AddListener(OnMergedChanged);
        }

        ApplyState();
    }

    CinemachineVirtualCamera EnsureCamera(CinemachineVirtualCamera cam, string name)
    {
        if (cam)
            return cam;

        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        cam = go.AddComponent<CinemachineVirtualCamera>();
        return cam;
    }

    void ConfigureCameras()
    {
        Transform eliorTarget = party && party.elior ? party.elior.transform : null;
        Transform simTarget = party && party.sim ? party.sim.transform : null;

        SetupCamera(vcamMerged, eliorTarget, 55f, 30, true);
        SetupCamera(vcamElior, eliorTarget, 50f, 10, false);
        SetupCamera(vcamSim, simTarget, 48f, 10, false);
    }

    void SetupCamera(CinemachineVirtualCamera cam, Transform follow, float fieldOfView, int defaultPriority, bool ensureFraming)
    {
        if (!cam)
            return;

        cam.m_Lens.FieldOfView = fieldOfView;
        cam.m_Lens.ModeOverride = LensSettings.OverrideModes.None;

        if (follow)
        {
            cam.Follow = follow;
            cam.LookAt = follow;
        }

        cam.Priority = defaultPriority;

        if (!ensureFraming)
            return;

        var body = cam.GetCinemachineComponent<CinemachineFramingTransposer>()
            ?? cam.AddCinemachineComponent<CinemachineFramingTransposer>();
        if (body != null)
        {
            body.m_CameraDistance = 8f;
            var offset = body.m_TrackedObjectOffset;
            offset.z = -8f;
            body.m_TrackedObjectOffset = offset;
        }
    }

    public void OnActiveChanged(string who)
    {
        ApplyState();
    }

    public void OnMergedChanged(bool merged)
    {
        ApplyState();
    }

    void ApplyState()
    {
        if (vcamMerged == null && vcamElior == null && vcamSim == null)
            return;

        ConfigureCameras();

        if (party == null)
        {
            SetPri(30, 10, 10);
            return;
        }

        if (party.IsMerged)
        {
            SetPri(30, 5, 5);
            return;
        }

        bool eliorActive = party.Active == party.elior;
        SetPri(5, eliorActive ? 30 : 10, eliorActive ? 10 : 30);
    }

    void SetPri(int merged, int elior, int sim)
    {
        if (vcamMerged)
            vcamMerged.Priority = merged;
        if (vcamElior)
            vcamElior.Priority = elior;
        if (vcamSim)
            vcamSim.Priority = sim;
    }
}

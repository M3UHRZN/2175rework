using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;

public static class CameraAutoSetup
{
    private const string MenuPath = "Tools/Camera/Auto-Create Party VCams";

    [MenuItem(MenuPath)]
    public static void AutoCreatePartyVCams()
    {
        var party = Object.FindFirstObjectByType<DualCharacterController>();
        if (!party)
        {
            Debug.LogWarning("No DualCharacterController found in the scene.");
            return;
        }

        var mainCamera = Camera.main;
        if (!mainCamera)
        {
            Debug.LogWarning("Main Camera not found in the scene.");
            return;
        }

        EnsureBrain(mainCamera);

        var elior = party.elior ? party.elior.transform : null;
        var sim = party.sim ? party.sim.transform : null;
        if (!elior || !sim)
        {
            Debug.LogWarning("Elior or Sim transforms not found on DualCharacterController.");
            return;
        }

        var mergedCam = FindOrCreateCamera("vcam_Merged", elior);
        var eliorCam = FindOrCreateCamera("vcam_Elior", elior);
        var simCam = FindOrCreateCamera("vcam_Sim", sim);

        mergedCam.Priority = 30;
        eliorCam.Priority = 10;
        simCam.Priority = 10;

        var switcher = EnsureSwitcher(party, mergedCam, eliorCam, simCam);
        AssignProfiles(switcher);

        EditorUtility.SetDirty(switcher);
        Debug.Log("Camera setup completed.");
    }

    private static CinemachineCamera FindOrCreateCamera(string name, Transform follow)
    {
        var existing = GameObject.Find(name);
        CinemachineCamera camera;
        if (existing)
        {
            camera = existing.GetComponent<CinemachineCamera>();
            if (!camera)
            {
                camera = existing.AddComponent<CinemachineCamera>();
            }
        }
        else
        {
            existing = new GameObject(name);
            camera = existing.AddComponent<CinemachineCamera>();
        }

        camera.Follow = follow;
        Debug.Log($"Camera {name} follow target set to: {follow.name} (Position: {follow.position})");
        return camera;
    }

    private static void EnsureBrain(Camera camera)
    {
        if (!camera.TryGetComponent(out Unity.Cinemachine.CinemachineBrain brain))
        {
            brain = camera.gameObject.AddComponent<Unity.Cinemachine.CinemachineBrain>();
        }

        brain.DefaultBlend = new Unity.Cinemachine.CinemachineBlendDefinition(
            Unity.Cinemachine.CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);
    }

    private static CinemachinePartySwitcher EnsureSwitcher(DualCharacterController party, CinemachineCamera merged, CinemachineCamera elior, CinemachineCamera sim)
    {
        const string switcherName = "CinemachinePartySwitcher";
        var switcherObj = GameObject.Find(switcherName);
        if (!switcherObj)
        {
            switcherObj = new GameObject(switcherName);
        }

        if (!switcherObj.TryGetComponent(out CinemachinePartySwitcher switcher))
        {
            switcher = switcherObj.AddComponent<CinemachinePartySwitcher>();
        }

        switcher.party = party;
        switcher.vcamMerged = merged;
        switcher.vcamElior = elior;
        switcher.vcamSim = sim;

        return switcher;
    }

    private static void AssignProfiles(CinemachinePartySwitcher switcher)
    {
        if (!switcher)
        {
            return;
        }

        var directory = "Assets/Configs/Camera";
        if (!AssetDatabase.IsValidFolder(directory))
        {
            CreateFolders("Assets/Configs", "Camera");
        }

        switcher.mergedProfile = FindOrCreateProfile(directory + "/CameraProfile_Merged.asset");
        switcher.eliorProfile = FindOrCreateProfile(directory + "/CameraProfile_Elior.asset");
        switcher.simProfile = FindOrCreateProfile(directory + "/CameraProfile_Sim.asset");
    }

    private static CameraProfile FindOrCreateProfile(string path)
    {
        var profile = AssetDatabase.LoadAssetAtPath<CameraProfile>(path);
        if (!profile)
        {
            profile = ScriptableObject.CreateInstance<CameraProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }

        return profile;
    }

    private static void CreateFolders(string basePath, string leaf)
    {
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            var parent = System.IO.Path.GetDirectoryName(basePath);
            if (!string.IsNullOrEmpty(parent))
            {
                CreateFolders(parent.Replace('\\', '/'), System.IO.Path.GetFileName(basePath));
            }
        }

        if (!AssetDatabase.IsValidFolder(basePath + "/" + leaf))
        {
            AssetDatabase.CreateFolder(basePath, leaf);
        }
    }
}

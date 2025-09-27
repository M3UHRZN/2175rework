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

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private void Awake()
    {
        DebugLog("CinemachinePartySwitcher başlatılıyor...");
        
        if (!party)
        {
            party = FindObjectOfType<DualCharacterController>();
            if (party)
                DebugLog("DualCharacterController bulundu: " + party.name);
            else
                DebugLogError("DualCharacterController bulunamadı!");
        }
        else
        {
            DebugLog("DualCharacterController referansı mevcut: " + party.name);
        }

        SetupCinemachineBrain();
        ApplyProfiles();
    }

    private void OnEnable()
    {
        if (party)
        {
            party.OnActiveCharacterChanged.AddListener(OnActiveChanged);
            party.OnMergedStateChanged.AddListener(OnMergedChanged);
            DebugLog("Event listener'lar bağlandı");
        }

        ApplyState();
    }

    private void OnDisable()
    {
        if (party)
        {
            party.OnActiveCharacterChanged.RemoveListener(OnActiveChanged);
            party.OnMergedStateChanged.RemoveListener(OnMergedChanged);
            DebugLog("Event listener'lar kaldırıldı");
        }
    }

    public void OnActiveChanged(string characterName)
    {
        DebugLog($"Aktif karakter değişti: {characterName}");
        ApplyState();
    }

    public void OnMergedChanged(bool isMerged)
    {
        DebugLog($"Merge durumu değişti: {(isMerged ? "Merged" : "Split")}");
        ApplyState();
    }

    private void SetupCinemachineBrain()
    {
        DebugLog("CinemachineBrain kurulumu kontrol ediliyor...");
        
        var mainCamera = Camera.main;
        if (!mainCamera)
        {
            DebugLogError("Main Camera bulunamadı!");
            return;
        }

        var brain = mainCamera.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (!brain)
        {
            DebugLog("CinemachineBrain Main Camera'ya ekleniyor...");
            brain = mainCamera.gameObject.AddComponent<Unity.Cinemachine.CinemachineBrain>();
            DebugLog("CinemachineBrain başarıyla eklendi");
        }
        else
        {
            DebugLog("CinemachineBrain zaten mevcut");
        }

        // Main Camera depth'ini düşür
        if (mainCamera.depth >= 0)
        {
            mainCamera.depth = -1;
            DebugLog("Main Camera depth -1 olarak ayarlandı");
        }

        // Cinemachine kameraları otomatik bul
        if (!vcamMerged || !vcamElior || !vcamSim)
        {
            DebugLog("Cinemachine kameraları otomatik bulunuyor...");
            
            var allCinemachineCameras = FindObjectsOfType<Unity.Cinemachine.CinemachineCamera>();
            DebugLog($"Sahnede {allCinemachineCameras.Length} Cinemachine kamerası bulundu");
            
            foreach (var cam in allCinemachineCameras)
            {
                string camName = cam.name.ToLower();
                DebugLog($"Kamera bulundu: {cam.name}");
                
                if (camName.Contains("merge") || camName.Contains("merged"))
                {
                    vcamMerged = cam;
                    DebugLog($"vcamMerged atandı: {cam.name}");
                }
                else if (camName.Contains("elior"))
                {
                    vcamElior = cam;
                    DebugLog($"vcamElior atandı: {cam.name}");
                }
                else if (camName.Contains("sim"))
                {
                    vcamSim = cam;
                    DebugLog($"vcamSim atandı: {cam.name}");
                }
            }
        }

        // Follow target'ları ayarla
        SetupFollowTargets();
    }

    private void SetupFollowTargets()
    {
        DebugLog("Follow target'ları ayarlanıyor...");
        
        if (!party)
        {
            DebugLogError("Party referansı yok!");
            return;
        }

        var elior = party.elior;
        var sim = party.sim;
        
        if (!elior)
        {
            DebugLogError("Elior karakteri bulunamadı!");
            return;
        }
        
        if (!sim)
        {
            DebugLogError("Sim karakteri bulunamadı!");
            return;
        }

        DebugLog($"Elior transform: {elior.transform.name} - Position: {elior.transform.position}");
        DebugLog($"Sim transform: {sim.transform.name} - Position: {sim.transform.position}");

        // Follow target'ları ayarla
        if (vcamMerged)
        {
            vcamMerged.Follow = elior.transform;
            DebugLog($"vcamMerged follow target: {elior.name} (Transform: {elior.transform.name})");
            
            // Follow target'ın doğru atandığını kontrol et
            if (vcamMerged.Follow == elior.transform)
            {
                DebugLog("vcamMerged follow target başarıyla atandı!");
            }
            else
            {
                DebugLogError($"vcamMerged follow target atanamadı! Beklenen: {elior.transform.name}, Atanan: {(vcamMerged.Follow ? vcamMerged.Follow.name : "null")}");
            }
        }

        if (vcamElior)
        {
            vcamElior.Follow = elior.transform;
            DebugLog($"vcamElior follow target: {elior.name} (Transform: {elior.transform.name})");
            
            if (vcamElior.Follow == elior.transform)
            {
                DebugLog("vcamElior follow target başarıyla atandı!");
            }
            else
            {
                DebugLogError($"vcamElior follow target atanamadı! Beklenen: {elior.transform.name}, Atanan: {(vcamElior.Follow ? vcamElior.Follow.name : "null")}");
            }
        }

        if (vcamSim)
        {
            vcamSim.Follow = sim.transform;
            DebugLog($"vcamSim follow target: {sim.name} (Transform: {sim.transform.name})");
            
            if (vcamSim.Follow == sim.transform)
            {
                DebugLog("vcamSim follow target başarıyla atandı!");
            }
            else
            {
                DebugLogError($"vcamSim follow target atanamadı! Beklenen: {sim.transform.name}, Atanan: {(vcamSim.Follow ? vcamSim.Follow.name : "null")}");
            }
        }

        DebugLog("Follow target'ları ayarlama tamamlandı!");
    }

    private void ApplyProfiles()
    {
        DebugLog("Camera profile'ları uygulanıyor...");
        
        SetupCameraProfile(vcamMerged, mergedProfile, "Merged");
        SetupCameraProfile(vcamElior, eliorProfile, "Elior");
        SetupCameraProfile(vcamSim, simProfile, "Sim");
    }

    private void SetupCameraProfile(Unity.Cinemachine.CinemachineCamera cam, CameraProfile profile, string name)
    {
        if (!cam)
        {
            DebugLogWarning($"{name} kamerası bulunamadı!");
            return;
        }

        if (!profile)
        {
            DebugLogWarning($"{name} profile'ı bulunamadı!");
            return;
        }

        DebugLog($"{name} kamerası için profile uygulanıyor...");

        // Lens ayarları (sadece FOV)
        cam.Lens.FieldOfView = profile.fov;
        DebugLog($"{name} FOV: {profile.fov}");

        // Kamera pozisyonunu ayarla (basit çözüm)
        var camTransform = cam.transform;
        var newPosition = camTransform.position;
        newPosition.x = profile.followOffset.x;
        newPosition.y = profile.followOffset.y;
        newPosition.z = profile.followOffset.z;
        camTransform.position = newPosition;
        
        DebugLog($"{name} kamera pozisyonu ayarlandı: {newPosition}");
    }

    private void ApplyState()
    {
        if (!party)
        {
            DebugLogError("Party referansı yok, kamera değişimi yapılamıyor!");
            return;
        }

        if (party.IsMerged)
        {
            DebugLog("Merged durumda - Merged kamera aktif ediliyor");
            SetPriority(30, 5, 5);
            return;
        }

        bool eliorActive = party.Active == party.elior;
        string activeChar = eliorActive ? "Elior" : "Sim";
        DebugLog($"Split durumda - Aktif karakter: {activeChar}");
        SetPriority(5, eliorActive ? 30 : 10, eliorActive ? 10 : 30);
    }

    private void SetPriority(int merged, int elior, int sim)
    {
        DebugLog($"Kamera öncelikleri ayarlanıyor - Merged: {merged}, Elior: {elior}, Sim: {sim}");
        
        if (vcamMerged)
        {
            vcamMerged.Priority = merged;
            DebugLog($"vcamMerged önceliği: {merged}");
        }
        else
        {
            DebugLogWarning("vcamMerged referansı yok!");
        }

        if (vcamElior)
        {
            vcamElior.Priority = elior;
            DebugLog($"vcamElior önceliği: {elior}");
        }
        else
        {
            DebugLogWarning("vcamElior referansı yok!");
        }

        if (vcamSim)
        {
            vcamSim.Priority = sim;
            DebugLog($"vcamSim önceliği: {sim}");
        }
        else
        {
            DebugLogWarning("vcamSim referansı yok!");
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[CinemachinePartySwitcher] {message}");
    }

    private void DebugLogWarning(string message)
    {
        if (enableDebugLogs)
            Debug.LogWarning($"[CinemachinePartySwitcher] {message}");
    }

    private void DebugLogError(string message)
    {
        if (enableDebugLogs)
            Debug.LogError($"[CinemachinePartySwitcher] {message}");
    }
}

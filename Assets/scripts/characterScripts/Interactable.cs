using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum InteractionActor
{
    Any,
    Elior,
    Sim
}

[System.Serializable]
public class InteractionControllerEvent : UnityEvent<InteractionController> { }

[System.Serializable]
public class InteractionProgressEvent : UnityEvent<float> { }

public enum PuzzleState
{
    Hazir,
    Calisiyor,
    Tamamlandi,
    Basarisiz
}

[System.Serializable]
public class PuzzleStateEvent : UnityEvent<PuzzleState> { }

[System.Serializable]
public class PuzzleStageEvent : UnityEvent<int> { }

public class Interactable : MonoBehaviour
{
    [Header("Etkileşim Ayarları")]
    public InteractionActor requiredActor = InteractionActor.Any;
    [FormerlySerializedAs("interactionType")]
    [SerializeField]
    InteractionType legacyInteractionType = InteractionType.Tap;
    [Tooltip("Bu etkileşimin kullanılabileceği maksimum mesafe.")]
    public float range = 1.2f;
    public int priority = 0;
    [Tooltip("Basılı tutma gerektiren etkileşimler için süre (ms cinsinden).")]
    public float holdDurationMs = 1000f;
    [Tooltip("Tamamlandıktan sonra yeniden kullanılmadan önceki bekleme süresi (ms cinsinden).")]
    public float cooldownMs = 300f;
    public bool isLocked = false;
    [Tooltip("Doğruysa etkileşim sahne yeniden yüklendiğinde de kilitsiz kalır (haricen yönetilir).")]
    public bool persistent = false;

    [Header("Eylem Yapılandırması")]
    public InteractionAction action;

    [Header("Bulmaca Durumu")]
    public PuzzleState baslangicBulmacaDurumu = PuzzleState.Hazir;
    [SerializeField]
    PuzzleState guncelBulmacaDurumu = PuzzleState.Hazir;
    [Tooltip("Toplam aşama sayısı (1 = aşama kullanılmıyor).")]
    public int toplamAsamaSayisi = 1;
    [Tooltip("Etkileşim başladığında kullanılacak başlangıç aşaması indexi.")]
    public int baslangicAsamaIndexi = 0;
    [SerializeField]
    int guncelAsamaIndexi = 0;

    [Header("Bulmaca Olayları")]
    public PuzzleStateEvent BulmacaDurumuDegisti;
    public UnityEvent BulmacaTamamlandi;
    public UnityEvent BulmacaBasarisiz;
    public UnityEvent BulmacaSifirlandi;
    public PuzzleStageEvent AsamaDegisti;

    [Header("Etkileşim Olayları")]
    [FormerlySerializedAs("OnFocusEnter")]
    public InteractionControllerEvent OdakGirdi;
    [FormerlySerializedAs("OnFocusExit")]
    public InteractionControllerEvent OdakCikti;
    [FormerlySerializedAs("OnInteractStart")]
    public InteractionControllerEvent EtkilesimBasladi;
    [FormerlySerializedAs("OnInteractProgress")]
    public InteractionProgressEvent EtkilesimIlerlemeGuncellendi;
    [FormerlySerializedAs("OnInteractComplete")]
    public InteractionControllerEvent EtkilesimTamamlandi;
    [FormerlySerializedAs("OnInteractCancel")]
    public InteractionControllerEvent EtkilesimIptalEdildi;

    float cooldownRemaining;
    bool toggleState;
    InteractionAction runtimeAction;
    InteractionController activeController;

    public bool IsLocked => isLocked;
    public bool IsOnCooldown => cooldownRemaining > 0f;
    public bool ToggleState => toggleState;

    void Awake()
    {
        ClampPuzzleConfiguration();
        ResetBulmacaDurumu(false);
    }

    void OnValidate()
    {
        ClampPuzzleConfiguration();
        guncelBulmacaDurumu = ClampPuzzleState(guncelBulmacaDurumu);
        guncelAsamaIndexi = Mathf.Clamp(guncelAsamaIndexi, 0, Mathf.Max(1, toplamAsamaSayisi) - 1);
    }

    public void Tick(float dtMs)
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - dtMs);
        }

        var instance = GetActionInstance();
        instance?.Tick(this, dtMs);
    }

    public void BeginCooldown()
    {
        cooldownRemaining = Mathf.Max(cooldownRemaining, cooldownMs);
    }

    public bool AllowsActor(InteractionActor actor)
    {
        return requiredActor == InteractionActor.Any || requiredActor == actor;
    }

    public void NotifyFocusEnter(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnFocusEnter(context);
        OdakGirdi?.Invoke(controller);
    }

    public void NotifyFocusExit(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnFocusExit(context);
        OdakCikti?.Invoke(controller);
    }

    public void NotifyStart(InteractionController controller)
    {
        if (!CanBeFocusedBy(controller))
            return;

        var context = CreateContext(controller);
        BeginBulmaca();
        GetActionInstance()?.OnStart(context);
        activeController = controller;
        EtkilesimBasladi?.Invoke(controller);
    }

    public void NotifyProgress(float t)
    {
        var context = CreateContext(activeController);
        GetActionInstance()?.OnProgress(context, t);
        GuncelleAsama(t);
        EtkilesimIlerlemeGuncellendi?.Invoke(t);
    }

    public void NotifyComplete(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnComplete(context);
        TamamlaBulmaca();
        EtkilesimTamamlandi?.Invoke(controller);
        if (activeController == controller)
            activeController = null;
    }

    public void NotifyCancel(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnCancel(context);
        BasarisizBulmaca();
        EtkilesimIptalEdildi?.Invoke(controller);
        if (activeController == controller)
            activeController = null;
    }

    public void ForceToggleState(bool value)
    {
        toggleState = value;
    }

    public InteractionActionMode ActionMode
    {
        get
        {
            var instance = GetActionInstance();
            return instance ? instance.Mode : InteractionActionMode.Instant;
        }
    }

    public InteractionAbilityRequirement AbilityRequirement
    {
        get
        {
            var instance = GetActionInstance();
            return instance ? instance.RequiredAbility : InteractionAbilityRequirement.Interact;
        }
    }

    public bool LocksMovement
    {
        get
        {
            var instance = GetActionInstance();
            return instance && instance.LocksMovement;
        }
    }

    public bool CanExecute(InteractionController controller)
    {
        if (!CanBeFocusedBy(controller))
            return false;

        var instance = GetActionInstance();
        if (!instance)
            return true;
        var context = CreateContext(controller);
        return instance.CanExecute(context);
    }

    public float GetRequiredHoldDurationMs(InteractionController controller)
    {
        var instance = GetActionInstance();
        if (!instance)
            return holdDurationMs;
        var context = CreateContext(controller);
        return instance.GetRequiredHoldDurationMs(context);
    }

    public PuzzleState GuncelBulmacaDurumu => guncelBulmacaDurumu;

    public int GuncelAsamaIndexi => guncelAsamaIndexi;

    public bool CanBeFocusedBy(InteractionController controller)
    {
        if (!controller)
            return true;

        if (guncelBulmacaDurumu == PuzzleState.Calisiyor && activeController && activeController != controller)
            return false;

        if (guncelBulmacaDurumu == PuzzleState.Tamamlandi && persistent)
            return false;

        return true;
    }

    public void ResetBulmacaDurumu(bool forceEvent = false)
    {
        bool stateChanged = SetBulmacaDurumu(baslangicBulmacaDurumu, forceEvent);
        bool stageChanged = SetAsamaIndexi(Mathf.Clamp(baslangicAsamaIndexi, 0, StageCount - 1), forceEvent);

        if ((stateChanged || stageChanged) || forceEvent)
        {
            BulmacaSifirlandi?.Invoke();
        }
    }

    public int StageCount => Mathf.Max(1, toplamAsamaSayisi);

    public void SetBulmacaDurumu(PuzzleState yeniDurum)
    {
        SetBulmacaDurumu(yeniDurum, false);
    }

    public void SetAsama(int yeniAsama)
    {
        SetAsamaIndexi(yeniAsama, false);
    }

    InteractionAction GetActionInstance()
    {
        if (!runtimeAction)
        {
            runtimeAction = CreateActionInstance();
            runtimeAction?.Initialize(this);
        }
        return runtimeAction;
    }

    InteractionAction CreateActionInstance()
    {
        if (action)
        {
            var instance = Instantiate(action);
            instance.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            return instance;
        }

        switch (legacyInteractionType)
        {
            case InteractionType.Toggle:
                return CreateRuntimeAction<ToggleInteractionAction>();
            case InteractionType.Hold:
                return CreateRuntimeAction<HoldInteractionAction>();
            case InteractionType.Panel:
                return CreateRuntimeAction<PanelInteractionAction>();
            default:
                return CreateRuntimeAction<TapInteractionAction>();
        }
    }

    InteractionAction CreateRuntimeAction<T>() where T : InteractionAction
    {
        var created = ScriptableObject.CreateInstance<T>();
        created.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        return created;
    }

    InteractionContext CreateContext(InteractionController controller)
    {
        return new InteractionContext(controller, this);
    }

    void BeginBulmaca()
    {
        bool wasInProgress = guncelBulmacaDurumu == PuzzleState.Calisiyor;
        SetBulmacaDurumu(PuzzleState.Calisiyor);
        if (!wasInProgress)
        {
            SetAsamaIndexi(Mathf.Clamp(baslangicAsamaIndexi, 0, StageCount - 1), false);
        }
    }

    void TamamlaBulmaca()
    {
        SetBulmacaDurumu(PuzzleState.Tamamlandi);
        BulmacaTamamlandi?.Invoke();

        if (!persistent)
        {
            ResetBulmacaDurumu();
        }
    }

    void BasarisizBulmaca()
    {
        SetBulmacaDurumu(PuzzleState.Basarisiz);
        BulmacaBasarisiz?.Invoke();

        if (!persistent)
        {
            ResetBulmacaDurumu();
        }
    }

    void GuncelleAsama(float normalizedProgress)
    {
        if (StageCount <= 1)
            return;

        var hedef = Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(normalizedProgress) * StageCount), 0, StageCount - 1);
        SetAsamaIndexi(hedef, false);
    }

    bool SetBulmacaDurumu(PuzzleState yeniDurum, bool forceEvent)
    {
        yeniDurum = ClampPuzzleState(yeniDurum);
        if (!forceEvent && guncelBulmacaDurumu == yeniDurum)
            return false;

        guncelBulmacaDurumu = yeniDurum;
        BulmacaDurumuDegisti?.Invoke(guncelBulmacaDurumu);
        return true;
    }

    bool SetAsamaIndexi(int yeniAsama, bool forceEvent)
    {
        int clamped = Mathf.Clamp(yeniAsama, 0, StageCount - 1);
        if (!forceEvent && guncelAsamaIndexi == clamped)
            return false;

        guncelAsamaIndexi = clamped;
        AsamaDegisti?.Invoke(guncelAsamaIndexi);
        return true;
    }

    PuzzleState ClampPuzzleState(PuzzleState durum)
    {
        switch (durum)
        {
            case PuzzleState.Hazir:
            case PuzzleState.Calisiyor:
            case PuzzleState.Tamamlandi:
            case PuzzleState.Basarisiz:
                return durum;
            default:
                return PuzzleState.Hazir;
        }
    }

    void ClampPuzzleConfiguration()
    {
        toplamAsamaSayisi = Mathf.Max(1, toplamAsamaSayisi);
        baslangicAsamaIndexi = Mathf.Clamp(baslangicAsamaIndexi, 0, toplamAsamaSayisi - 1);
    }
}

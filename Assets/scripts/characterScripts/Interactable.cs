using System.Collections.Generic;
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

public class Interactable : MonoBehaviour
{
    static readonly List<Interactable> aktifEtkilesimler = new List<Interactable>(64);

    [Header("Genel Ayarlar")]
    public InteractionActor requiredActor = InteractionActor.Any;
    [FormerlySerializedAs("interactionType")]
    [SerializeField]
    InteractionType legacyInteractionType = InteractionType.Tap;
    [Min(0f)]
    [Tooltip("Oyuncunun bu etkileşimi tetikleyebileceği azami mesafe.")]
    public float range = 1.5f;
    public int priority = 0;
    [Tooltip("Kilitli etkileşimler oyuncu tarafından kullanılamaz.")]
    public bool isLocked = false;
    [Tooltip("Basılı tutma gerektiren etkileşimler için gereken süre (ms).")]
    public float holdDurationMs = 1000f;
    [Tooltip("Tamamlandıktan sonra yeniden kullanılmadan önceki bekleme süresi (ms).")]
    public float cooldownMs = 250f;

    [Header("Eylem Yapılandırması")]
    public InteractionAction action;

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

    public bool ToggleState => toggleState;
    public bool IsLocked => isLocked;
    public bool IsOnCooldown => cooldownRemaining > 0f;

    public static void ToplananEtkilesimleriDoldur(List<Interactable> hedef)
    {
        if (hedef == null)
            return;

        hedef.Clear();
        for (int i = aktifEtkilesimler.Count - 1; i >= 0; i--)
        {
            var aday = aktifEtkilesimler[i];
            if (!aday)
            {
                aktifEtkilesimler.RemoveAt(i);
                continue;
            }

            if (!aday.isActiveAndEnabled)
                continue;

            hedef.Add(aday);
        }
    }

    void OnEnable()
    {
        if (!aktifEtkilesimler.Contains(this))
            aktifEtkilesimler.Add(this);
        cooldownRemaining = 0f;
        activeController = null;
        runtimeAction = null;
    }

    void OnDisable()
    {
        aktifEtkilesimler.Remove(this);
        activeController = null;
        if (runtimeAction)
        {
            DestroyImmediate(runtimeAction);
            runtimeAction = null;
        }
    }

    void OnDestroy()
    {
        aktifEtkilesimler.Remove(this);
        if (runtimeAction)
        {
            DestroyImmediate(runtimeAction);
            runtimeAction = null;
        }
    }

    void Update()
    {
        Tick(Time.deltaTime * 1000f);
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
        if (!CanExecute(controller))
            return;

        var context = CreateContext(controller);
        GetActionInstance()?.OnStart(context);
        activeController = controller;
        EtkilesimBasladi?.Invoke(controller);
    }

    public void NotifyProgress(float t)
    {
        var context = CreateContext(activeController);
        GetActionInstance()?.OnProgress(context, t);
        EtkilesimIlerlemeGuncellendi?.Invoke(t);
    }

    public void NotifyComplete(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnComplete(context);
        EtkilesimTamamlandi?.Invoke(controller);
        if (activeController == controller)
            activeController = null;
    }

    public void NotifyCancel(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnCancel(context);
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
        if (IsLocked || IsOnCooldown)
            return false;

        if (controller && !AllowsActor(controller.actor))
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

    public bool CanBeFocusedBy(InteractionController controller)
    {
        if (!controller)
            return true;
        if (!AllowsActor(controller.actor))
            return false;
        return !isLocked;
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

    void OnValidate()
    {
        range = Mathf.Max(0f, range);
        cooldownMs = Mathf.Max(0f, cooldownMs);
        holdDurationMs = Mathf.Max(0f, holdDurationMs);
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class MergeController : MonoBehaviour
{
    public enum TransitionKind
    {
        Merge,
        Split
    }

    [Header("Referanslar")]
    [Tooltip("Aktif ajan durumunu yöneten CharacterSwitcher.")]
    [SerializeField] CharacterSwitcher switcher;

    [Tooltip("Elior kök GameObject (boşsa bu nesne kullanılır).")]
    [SerializeField] GameObject eliorRoot;

    [Tooltip("Sim kök GameObject")]
    [SerializeField] GameObject simRoot;

    [Tooltip("Merge/Split tetiklerini taşıyan Animator.")]
    [SerializeField] Animator animator;

    [Header("Animasyon Ayarları")]
    [Tooltip("Merge akışını tetikleyen trigger adı.")]
    [SerializeField] string mergeTriggerName = "MergeTrigger";

    [Tooltip("Split akışını tetikleyen trigger adı.")]
    [SerializeField] string splitTriggerName = "SplitTrigger";

    [Tooltip("Yeni bir trigger ateşlenirken diğer trigger temizlensin mi?")]
    [SerializeField] bool resetOppositeTrigger = true;

    [Header("Split Çıkışı")]
    [Tooltip("Sim spawn edilirken Elior'a uygulanacak birincil ofset.")]
    [SerializeField] Vector2 primarySplitOffset = new(1.25f, 0f);

    [Tooltip("Birincil nokta doluysa denenecek alternatif ofset.")]
    [SerializeField] Vector2 secondarySplitOffset = new(-1.25f, 0f);

    [Tooltip("Spawn pozisyonu kontrolünde kullanılacak yarıçap.")]
    [SerializeField] float spawnCheckRadius = 0.45f;

    [Tooltip("Split spawn alanı için çakışma maskesi.")]
    [SerializeField] LayerMask spawnCollisionMask = ~0;

    [Header("Davranış")]
    [Tooltip("Merge sırasında Sim otomatik olarak devre dışı bırakılsın mı?")]
    [SerializeField] bool disableSimWhenMerged = true;

    [Tooltip("TransitionLock aktifken hareketli bileşenlerin hızı sıfırlansın mı?")]
    [SerializeField] bool zeroVelocityOnTransition = true;

    [Tooltip("Time.timeScale <= 0 olduğunda akış başlatma.")]
    [SerializeField] bool respectPauseState = true;

    [Tooltip("Akış başlangıç/bitişleri loglansın mı?")]
    [SerializeField] bool logTransitions = true;

    TransitionKind? currentTransition;
    bool warnedMissingAnimator;
    bool warnedMissingSim;

    void Awake()
    {
        switcher ??= GetComponent<CharacterSwitcher>();
        eliorRoot ??= gameObject;
        animator ??= GetComponentInChildren<Animator>();
    }

    public void RegisterSwitcher(CharacterSwitcher newSwitcher)
    {
        switcher = newSwitcher;
    }

    public bool BeginMerge()
    {
        if (respectPauseState && Time.timeScale <= 0f)
            return false;

        if (currentTransition.HasValue)
            return false;

        if (!simRoot)
        {
            if (!warnedMissingSim)
            {
                Debug.LogWarning("[MergeController] Sim referansı atanmamış.", this);
                warnedMissingSim = true;
            }
            return false;
        }

        if (disableSimWhenMerged)
        {
            SetActive(simRoot, false);
        }

        TriggerAnimator(mergeTriggerName, splitTriggerName);
        currentTransition = TransitionKind.Merge;

        if (zeroVelocityOnTransition)
        {
            FreezeMotion();
        }

        return true;
    }

    public bool BeginSplit()
    {
        if (respectPauseState && Time.timeScale <= 0f)
            return false;

        if (currentTransition.HasValue)
            return false;

        if (!simRoot)
        {
            if (!warnedMissingSim)
            {
                Debug.LogWarning("[MergeController] Sim referansı atanmamış.", this);
                warnedMissingSim = true;
            }
            return false;
        }

        Vector3 spawnPosition;
        if (!TryResolveSpawnPosition(out spawnPosition))
        {
            if (logTransitions)
                Debug.LogWarning("[MergeController] Split için güvenli spawn bulunamadı.", this);
            return false;
        }

        simRoot.transform.position = spawnPosition;
        SetActive(simRoot, true);
        ZeroVelocity(simRoot);

        TriggerAnimator(splitTriggerName, mergeTriggerName);
        currentTransition = TransitionKind.Split;

        if (zeroVelocityOnTransition)
        {
            FreezeMotion();
        }

        return true;
    }

    public void OnMergeAnimFinished()
    {
        if (currentTransition != TransitionKind.Merge)
            return;

        if (disableSimWhenMerged)
        {
            SetActive(simRoot, false);
        }

        currentTransition = null;
        switcher?.NotifyTransitionCompleted(TransitionKind.Merge);

        if (logTransitions)
            Debug.Log("[MergeController] Merge animasyonu tamamlandı.", this);
    }

    public void OnSplitAnimFinished()
    {
        if (currentTransition != TransitionKind.Split)
            return;

        currentTransition = null;
        switcher?.NotifyTransitionCompleted(TransitionKind.Split);

        if (logTransitions)
            Debug.Log("[MergeController] Split animasyonu tamamlandı.", this);
    }

    public void CancelActiveTransition()
    {
        if (!currentTransition.HasValue)
            return;

        currentTransition = null;
        switcher?.NotifyTransitionCancelled();
    }

    void TriggerAnimator(string triggerToFire, string triggerToReset)
    {
        if (!animator)
        {
            if (!warnedMissingAnimator)
            {
                Debug.LogWarning("[MergeController] Animator referansı bulunamadı.", this);
                warnedMissingAnimator = true;
            }
            return;
        }

        if (resetOppositeTrigger && !string.IsNullOrEmpty(triggerToReset))
        {
            animator.ResetTrigger(triggerToReset);
        }

        if (!string.IsNullOrEmpty(triggerToFire))
        {
            animator.SetTrigger(triggerToFire);
        }
    }

    bool TryResolveSpawnPosition(out Vector3 position)
    {
        Vector3 basePos = eliorRoot ? eliorRoot.transform.position : transform.position;
        Vector3 first = basePos + (Vector3)primarySplitOffset;
        Vector3 second = basePos + (Vector3)secondarySplitOffset;

        if (IsPositionClear(first))
        {
            position = first;
            return true;
        }

        if (IsPositionClear(second))
        {
            position = second;
            return true;
        }

        position = basePos;
        return false;
    }

    bool IsPositionClear(Vector2 world)
    {
        var hits = Physics2D.OverlapCircleAll(world, spawnCheckRadius, spawnCollisionMask);
        foreach (var hit in hits)
        {
            if (!hit)
                continue;

            if (IsPartOf(hit.transform.gameObject, eliorRoot))
                continue;

            if (IsPartOf(hit.transform.gameObject, simRoot))
                continue;

            return false;
        }

        return true;
    }

    void FreezeMotion()
    {
        ZeroVelocity(eliorRoot);
        ZeroVelocity(simRoot);
    }

    void ZeroVelocity(GameObject target)
    {
        if (!target)
            return;

        var rb = target.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
        }

        var motor = target.GetComponent<LocomotionMotor2D>();
        if (motor)
        {
            motor.RequestHorizontalIntent(0f);
        }
    }

    void SetActive(GameObject target, bool value)
    {
        if (!target)
            return;

        if (target.activeSelf != value)
            target.SetActive(value);
    }

    bool IsPartOf(GameObject candidate, GameObject root)
    {
        if (!candidate || !root)
            return false;

        if (candidate == root)
            return true;

        return candidate.transform.IsChildOf(root.transform);
    }
}

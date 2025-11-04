using UnityEngine;

/// <summary>
/// Parent GameObject'in sprite flip durumunu takip eder.
/// Parent'ın flipX'i değiştiğinde bu GameObject'in transform X scale'ini +'dan -'ye (veya tersine) çevirir.
/// </summary>
[DisallowMultipleComponent]
public class SpriteFlipMirror : MonoBehaviour
{
    [Header("Parent Settings")]
    [Tooltip("Takip edilecek parent SpriteRenderer. Buraya Elior'un SpriteRenderer'ını koyun.")]
    [SerializeField] SpriteRenderer parentSpriteRenderer;

    [Header("This Sprite Settings")]
    [Tooltip("Bu GameObject'teki SpriteRenderer. Boş bırakılırsa otomatik bulunur.")]
    [SerializeField] SpriteRenderer thisSpriteRenderer;

    Transform thisTransform;
    bool lastParentFlipX;

    void Awake()
    {
        thisTransform = transform;

        // Bu GameObject'teki SpriteRenderer'ı bul
        if (!thisSpriteRenderer)
        {
            thisSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        // Parent SpriteRenderer yoksa parent GameObject'te ara
        if (!parentSpriteRenderer)
        {
            Transform parent = transform.parent;
            if (parent)
            {
                parentSpriteRenderer = parent.GetComponent<SpriteRenderer>();
                if (!parentSpriteRenderer)
                {
                    parentSpriteRenderer = parent.GetComponentInChildren<SpriteRenderer>();
                }
            }
        }

        if (!parentSpriteRenderer)
        {
            Debug.LogError($"[SpriteFlipMirror] {gameObject.name}: Parent SpriteRenderer bulunamadı! Inspector'da Parent Sprite Renderer alanına Elior'un SpriteRenderer'ını sürükleyin.", this);
            enabled = false;
            return;
        }

        Debug.Log($"[SpriteFlipMirror] {gameObject.name}: Parent SpriteRenderer bulundu: {parentSpriteRenderer.gameObject.name}", this);

        // İlk durumu kaydet (oyun başlarkenki durumu değiştirme)
        lastParentFlipX = parentSpriteRenderer.flipX;
    }

    void LateUpdate()
    {
        if (!parentSpriteRenderer)
        {
            Debug.LogWarning($"[SpriteFlipMirror] {gameObject.name}: Parent SpriteRenderer null!", this);
            return;
        }

        // Parent'ın flipX'i değişti mi?
        bool currentFlipX = parentSpriteRenderer.flipX;
        if (currentFlipX != lastParentFlipX)
        {
            Debug.Log($"[SpriteFlipMirror] {gameObject.name}: Parent flipX değişti: {lastParentFlipX} -> {currentFlipX}", this);
            lastParentFlipX = currentFlipX;
            UpdateMirror();
        }
    }

    void UpdateMirror()
    {
        if (!parentSpriteRenderer || !thisTransform)
        {
            Debug.LogWarning($"[SpriteFlipMirror] {gameObject.name}: UpdateMirror - parentSpriteRenderer veya thisTransform null!", this);
            return;
        }

        // Position X'in işaretini değiştir (flipX değiştiğinde)
        Vector3 currentPos = thisTransform.localPosition;
        currentPos.x = -currentPos.x;
        Debug.Log($"[SpriteFlipMirror] {gameObject.name}: Position X işaret değiştiriliyor: {thisTransform.localPosition.x} -> {currentPos.x}", this);
        thisTransform.localPosition = currentPos;

        // Sprite'ı da flip et
        if (thisSpriteRenderer)
        {
            thisSpriteRenderer.flipX = parentSpriteRenderer.flipX;
        }
    }
}

using UnityEngine;

/// <summary>
/// Kameranın hareketine göre katmanı parallax hızında kaydırır.
/// Her katmana ayrı bir script ekleyin ve çarpanı ayarlayın.
/// </summary>
[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    [Header("References")]
    public Transform targetCamera; // Boş bırakılırsa otomatik Camera.main

    [Header("Parallax Settings")] 
    [Tooltip("Kameraya göre yatay kayma çarpanı (0 = sabit arka plan, 1 = kamera ile aynı hız)")]
    [Range(0f, 1f)] public float parallaxMultiplierX = 0.3f;

    [Tooltip("Kameraya göre dikey kayma çarpanı (0 = sabit arka plan, 1 = kamera ile aynı hız)")]
    [Range(0f, 1f)] public float parallaxMultiplierY = 0.0f;

    [Header("Mouse Parallax (Menü İçin)")]
    [Tooltip("Aktifken katman, fare konumuna göre kayar (menüler için ideal)")]
    public bool useMouseParallax = false;
    [Tooltip("Fareye göre maksimum yatay sapma (dünya birimi)")]
    public float mouseAmplitudeX = 1.5f;
    [Tooltip("Fareye göre maksimum dikey sapma (dünya birimi)")]
    public float mouseAmplitudeY = 0.8f;
    [Tooltip("Fare takibi yumuşatma hızı")]
    [Range(0.1f, 20f)] public float mouseFollowSpeed = 6f;

    [Header("Infinite Scroll (Opsiyonel)")]
    public bool infiniteScrollX = false;
    public bool infiniteScrollY = false;

    private Vector3 _lastCameraPosition;
    private Vector3 _startPosition;
    private float _textureUnitSizeX;
    private float _textureUnitSizeY;

    private void OnEnable()
    {
        EnsureCamera();
        CacheTextureUnitSizes();
        _startPosition = transform.position;
        if (targetCamera != null)
            _lastCameraPosition = targetCamera.position;
    }
    
    private void LateUpdate()
    {
        if (useMouseParallax)
        {
            // Ekran merkezine göre -1..1 aralığında normlanmış fare konumu
            Vector3 mouse = Input.mousePosition;
            Vector2 viewport = new Vector2(
                Mathf.Clamp01(mouse.x / Mathf.Max(1f, Screen.width)),
                Mathf.Clamp01(mouse.y / Mathf.Max(1f, Screen.height))
            );
            Vector2 centered = (viewport - new Vector2(0.5f, 0.5f)) * 2f; // -1..1

            float offsetX = centered.x * mouseAmplitudeX * parallaxMultiplierX;
            float offsetY = centered.y * mouseAmplitudeY * parallaxMultiplierY;
            Vector3 targetPos = new Vector3(_startPosition.x + offsetX, _startPosition.y + offsetY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * mouseFollowSpeed);
            return;
        }

        EnsureCamera();
        if (targetCamera == null) return;

        Vector3 delta = targetCamera.position - _lastCameraPosition;
        Vector3 move = new Vector3(delta.x * parallaxMultiplierX, delta.y * parallaxMultiplierY, 0f);
        transform.position += move;
        _lastCameraPosition = targetCamera.position;

        if (infiniteScrollX)
        {
            if (Mathf.Abs(targetCamera.position.x - transform.position.x) >= _textureUnitSizeX)
            {
                float offsetX = (targetCamera.position.x - transform.position.x) % _textureUnitSizeX;
                transform.position = new Vector3(targetCamera.position.x + offsetX, transform.position.y, transform.position.z);
            }
        }

        if (infiniteScrollY)
        {
            if (Mathf.Abs(targetCamera.position.y - transform.position.y) >= _textureUnitSizeY)
            {
                float offsetY = (targetCamera.position.y - transform.position.y) % _textureUnitSizeY;
                transform.position = new Vector3(transform.position.x, targetCamera.position.y + offsetY, transform.position.z);
            }
        }
    }

    private void EnsureCamera()
    {
        if (targetCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null) targetCamera = cam.transform;
        }
    }

    private void CacheTextureUnitSizes()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Sprite sprite = sr.sprite;
            _textureUnitSizeX = sprite.bounds.size.x * Mathf.Abs(transform.localScale.x);
            _textureUnitSizeY = sprite.bounds.size.y * Mathf.Abs(transform.localScale.y);
        }
        else
        {
            // Varsayılan makul değerler (infinite scroll kapalıysa önemli değil)
            _textureUnitSizeX = 10f;
            _textureUnitSizeY = 10f;
        }
    }
}



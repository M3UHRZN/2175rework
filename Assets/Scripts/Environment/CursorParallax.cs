using UnityEngine;

/// <summary>
/// Ana menü için cursor tabanlı parallax efekti.
/// Fare hareketine göre katmanları farklı hızlarda hareket ettirir.
/// </summary>
public class CursorParallax : MonoBehaviour
{
    [Header("Parallax Ayarları")]
    [SerializeField] private float parallaxFactor = 0.5f; // 0 = hiç hareket etmez, 1 = mouse ile aynı hızda
    [SerializeField] private float maxDistanceX = 2f; // Maksimum X hareket mesafesi (world units)
    [SerializeField] private float maxDistanceY = 50f; // Maksimum Y hareket mesafesi (pixels)
    [SerializeField] private float smoothTime = 0.3f; // Yumuşak geçiş süresi
    
    [Header("Eksen Kontrolü")]
    [SerializeField] private bool enableX = true; // X ekseninde hareket
    [SerializeField] private bool enableY = true; // Y ekseninde hareket
    [SerializeField] private bool invertX = false; // X eksenini ters çevir
    [SerializeField] private bool invertY = false; // Y eksenini ters çevir
    
    // Private değişkenler
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 velocity;
    
    void Start()
    {
        // Başlangıç pozisyonunu kaydet
        startPosition = transform.position;
        targetPosition = startPosition;
    }
    
    void Update()
    {
        CalculateTargetPosition();
        MoveToTarget();
    }
    
    /// <summary>
    /// Mouse pozisyonuna göre hedef pozisyonu hesapla
    /// </summary>
    private void CalculateTargetPosition()
    {
        // Mouse pozisyonunu al
        Vector3 mousePos = Input.mousePosition;
        
        // Ekran merkezine göre normalize et (-1 ile +1 arası)
        float normalizedX = (mousePos.x / Screen.width - 0.5f) * 2f;
        float normalizedY = (mousePos.y / Screen.height - 0.5f) * 2f;
        
        // Ters çevir (opsiyonel)
        if (invertX) normalizedX *= -1f;
        if (invertY) normalizedY *= -1f;
        
        // Offset hesapla
        float offsetX = enableX ? normalizedX * maxDistanceX * parallaxFactor : 0f;
        float offsetY = enableY ? normalizedY * maxDistanceY * parallaxFactor : 0f;
        
        // Y ekseni için pixel'den world unit'e çevir
        if (enableY && Camera.main != null)
        {
            // Pixel'den world unit'e çevir
            float pixelToWorldRatio = Camera.main.orthographicSize * 2f / Screen.height;
            offsetY *= pixelToWorldRatio;
        }
        
        // Hedef pozisyonu belirle
        targetPosition = new Vector3(
            startPosition.x + offsetX,
            startPosition.y + offsetY,
            startPosition.z
        );
    }
    
    /// <summary>
    /// Hedefe yumuşakça hareket et
    /// </summary>
    private void MoveToTarget()
    {
        // SmoothDamp ile yumuşak hareket
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            smoothTime
        );
    }
    
    /// <summary>
    /// Parallax faktörünü değiştir
    /// </summary>
    public void SetParallaxFactor(float factor)
    {
        parallaxFactor = Mathf.Clamp01(factor);
    }
    
    /// <summary>
    /// Maksimum X mesafesini değiştir
    /// </summary>
    public void SetMaxDistanceX(float distance)
    {
        maxDistanceX = Mathf.Max(0f, distance);
    }
    
    /// <summary>
    /// Maksimum Y mesafesini değiştir (pixel cinsinden)
    /// </summary>
    public void SetMaxDistanceY(float pixels)
    {
        maxDistanceY = Mathf.Max(0f, pixels);
    }
    
    /// <summary>
    /// Pozisyonu sıfırla
    /// </summary>
    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
        velocity = Vector3.zero;
    }
}

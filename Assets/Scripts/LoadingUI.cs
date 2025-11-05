using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Basit Loading ekranı yöneticisi - seviye geçişlerinde gösterilir
/// </summary>
public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }
    
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;
    public Image loadingIcon;
    
    [Header("Loading Settings")]
    public string defaultLoadingText = "Yükleniyor...";
    public float iconRotationSpeed = 90f;
    
    [Header("Animation")]
    public bool rotateIcon = true;
    public bool fadeInOut = true;
    public float fadeSpeed = 2f;
    
    // Components
    private CanvasGroup canvasGroup;
    private bool isLoading = false;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        SetupUI();
    }
    
    private void Update()
    {
        if (isLoading && rotateIcon && loadingIcon != null)
        {
            loadingIcon.transform.Rotate(0, 0, iconRotationSpeed * Time.deltaTime);
        }
    }
    
    #endregion
    
    #region Setup
    
    private void SetupUI()
    {
        // CanvasGroup ekle (fade için)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Başlangıçta gizle
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// Loading ekranını göster
    /// </summary>
    public void ShowLoading(string customText = "")
    {
        if (isLoading) return;
        
        isLoading = true;
        
        // Loading panel'i aktif et
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        // Loading text'i ayarla
        if (loadingText != null)
        {
            loadingText.text = string.IsNullOrEmpty(customText) ? defaultLoadingText : customText;
        }
        
        // Fade in animasyonu
        if (fadeInOut)
        {
            StartCoroutine(FadeIn());
        }
        
        Debug.Log($"�� Loading UI gösterildi: {loadingText?.text}");
    }
    
    /// <summary>
    /// Loading ekranını gizle
    /// </summary>
    public void HideLoading()
    {
        if (!isLoading) return;
        
        isLoading = false;
        
        // Fade out animasyonu
        if (fadeInOut)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            // Direkt gizle
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
        }
        
        Debug.Log("🔄 Loading UI gizlendi");
    }
    
    /// <summary>
    /// Loading text'ini güncelle
    /// </summary>
    public void UpdateLoadingText(string newText)
    {
        if (loadingText != null)
        {
            loadingText.text = newText;
        }
    }
    
    /// <summary>
    /// Loading durumunu kontrol et
    /// </summary>
    public bool IsLoading()
    {
        return isLoading;
    }
    
    #endregion
    
    #region Coroutines
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Loading panel'i gizle
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
    
    #endregion
    
    #region Static Methods
    
    /// <summary>
    /// Static method ile loading göster
    /// </summary>
    public static void Show(string customText = "")
    {
        if (Instance != null)
        {
            Instance.ShowLoading(customText);
        }
    }
    
    /// <summary>
    /// Static method ile loading gizle
    /// </summary>
    public static void Hide()
    {
        if (Instance != null)
        {
            Instance.HideLoading();
        }
    }
    
    #endregion
}
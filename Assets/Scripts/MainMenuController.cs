using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string defaultLevelSceneName = "level1";
    [SerializeField] private string levelsSceneName = "SampleScene";
    [SerializeField] private string settingsSceneName = "meutest";

    [Header("UI Layout")]
    [SerializeField] private Vector2 buttonSize = new Vector2(320f, 64f);
    [SerializeField] private float buttonSpacing = 72f;
    [SerializeField] private float startOffsetY = 130f;

    private void Awake()
    {
        EnsureProgressManager();
        BuildUI();
    }

    private void EnsureProgressManager()
    {
        if (LevelProgressManager.Instance != null)
        {
            return;
        }

        var manager = new GameObject("LevelProgressManager");
        manager.AddComponent<LevelProgressManager>();
    }

    private void BuildUI()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            canvas = CreateCanvas();
        }

        EnsureEventSystem();

        var actions = new (string label, UnityAction handler)[]
        {
            ("Oluştur", HandleCreatePressed),
            ("Oyna", HandlePlayPressed),
            ("Seviyeler", HandleLevelsPressed),
            ("Ayarlar", HandleSettingsPressed),
            ("Çıkış", HandleQuitPressed)
        };

        for (int i = 0; i < actions.Length; i++)
        {
            var rect = CreateButton(canvas.transform, actions[i].label, actions[i].handler);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, startOffsetY - i * buttonSpacing);
        }
    }

    private Canvas CreateCanvas()
    {
        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        var rectTransform = canvasGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = Vector2.zero;

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();
    }

    private RectTransform CreateButton(Transform parent, string label, UnityAction handler)
    {
        var buttonGO = new GameObject(label + "Button", typeof(RectTransform));
        buttonGO.transform.SetParent(parent, false);

        var rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = buttonSize;

        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.17f, 0.24f, 0.36f, 0.95f);

        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;
        if (handler != null)
        {
            button.onClick.AddListener(handler);
        }

        var textGO = new GameObject("Label", typeof(RectTransform));
        textGO.transform.SetParent(buttonGO.transform, false);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGO.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 32;

        return rect;
    }

    private void HandleCreatePressed()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ResetProgress();
            LevelProgressManager.Instance.SetCurrentLevel(0, defaultLevelSceneName);
        }

        LoadSceneIfAvailable(defaultLevelSceneName);
    }

    private void HandlePlayPressed()
    {
        string targetScene = defaultLevelSceneName;
        if (LevelProgressManager.Instance != null)
        {
            var storedScene = LevelProgressManager.Instance.CurrentLevelSceneName;
            if (!string.IsNullOrEmpty(storedScene))
            {
                targetScene = storedScene;
            }
            else
            {
                LevelProgressManager.Instance.SetCurrentLevelScene(defaultLevelSceneName);
            }
        }

        LoadSceneIfAvailable(targetScene);
    }

    private void HandleLevelsPressed()
    {
        LoadSceneIfAvailable(levelsSceneName);
    }

    private void HandleSettingsPressed()
    {
        LoadSceneIfAvailable(settingsSceneName);
    }

    private void HandleQuitPressed()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    private void LoadSceneIfAvailable(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Geçerli bir sahne adı belirtilmedi.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}

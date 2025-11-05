using Game.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Creates the main menu interface at runtime so the scene can remain lightweight in source control.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class MainMenuBootstrap : MonoBehaviour
    {
        [SerializeField] private string[] levelSceneNames = { "level1", "level2" };

        private void Start()
        {
            if (FindObjectOfType<MainMenuUI>() != null)
            {
                return;
            }

            EnsureEventSystem();
            EnsureAudioSettingsManager();
            BuildMenu();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void EnsureAudioSettingsManager()
        {
            if (AudioSettingsManager.Instance != null)
            {
                return;
            }

            var managerGo = new GameObject("Audio Settings Manager");
            managerGo.AddComponent<AudioSettingsManager>();
        }

        private void BuildMenu()
        {
            var canvasGo = new GameObject("Main Menu Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuUI));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var menu = canvasGo.GetComponent<MainMenuUI>();

            var mainPanel = CreatePanel("Main Panel", canvas.transform, new Color(0, 0, 0, 0.6f));
            var levelPanel = CreatePanel("Level Panel", canvas.transform, new Color(0, 0, 0, 0.6f));
            var settingsPanel = CreatePanel("Settings Panel", canvas.transform, new Color(0, 0, 0, 0.6f));

            menu.Initialize(mainPanel.gameObject, levelPanel.gameObject, settingsPanel.gameObject);

            SetupMainPanel(menu, mainPanel);
            SetupLevelPanel(menu, levelPanel);
            SetupSettingsPanel(menu, settingsPanel);

            mainPanel.gameObject.SetActive(true);
            levelPanel.gameObject.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
        }

        private RectTransform CreatePanel(string name, Transform parent, Color backgroundColor)
        {
            var panelGo = CreateUIObject(name, parent);
            var rect = panelGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500f, 500f);
            rect.anchoredPosition = Vector2.zero;

            var image = panelGo.AddComponent<Image>();
            image.color = backgroundColor;

            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 16f;

            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rect;
        }

        private void SetupMainPanel(MainMenuUI menu, RectTransform mainPanel)
        {
            CreateLabel(mainPanel, "Oyuna Hoş Geldiniz", 36);

            var startButton = CreateButton(mainPanel, "Start");
            startButton.onClick.AddListener(menu.ShowLevelPanel);

            var settingsButton = CreateButton(mainPanel, "Ayarlar");
            settingsButton.onClick.AddListener(menu.ShowSettingsPanel);

            var exitButton = CreateButton(mainPanel, "Çıkış");
            exitButton.onClick.AddListener(menu.QuitGame);
        }

        private void SetupLevelPanel(MainMenuUI menu, RectTransform levelPanel)
        {
            CreateLabel(levelPanel, "Seviye Seç", 32);

            foreach (var sceneName in levelSceneNames)
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    continue;
                }

                var button = CreateButton(levelPanel, sceneName.ToUpperInvariant());
                var localName = sceneName;
                button.onClick.AddListener(() => menu.LoadLevel(localName));
            }

            var backButton = CreateButton(levelPanel, "Geri");
            backButton.onClick.AddListener(menu.ShowMainPanel);
        }

        private void SetupSettingsPanel(MainMenuUI menu, RectTransform settingsPanel)
        {
            CreateLabel(settingsPanel, "Ses Ayarları", 32);

            var manager = AudioSettingsManager.Instance;
            var view = settingsPanel.gameObject.AddComponent<AudioSettingsView>();

            var masterSlider = CreateSlider(settingsPanel, "Ana Ses", manager?.MasterVolume ?? 0.8f);
            masterSlider.onValueChanged.AddListener(view.OnMasterChanged);

            var musicSlider = CreateSlider(settingsPanel, "Müzik", manager?.MusicVolume ?? 0.8f);
            musicSlider.onValueChanged.AddListener(view.OnMusicChanged);

            var sfxSlider = CreateSlider(settingsPanel, "SFX", manager?.SfxVolume ?? 0.8f);
            sfxSlider.onValueChanged.AddListener(view.OnSfxChanged);

            view.Initialize(masterSlider, musicSlider, sfxSlider);

            var backButton = CreateButton(settingsPanel, "Geri");
            backButton.onClick.AddListener(menu.ShowMainPanel);
        }

        private Slider CreateSlider(RectTransform parent, string label, float initialValue)
        {
            var container = CreateUIObject(label + " Container", parent);
            var rect = container.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 80f);

            var layout = container.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;

            CreateLabel(container.GetComponent<RectTransform>(), label, 24);

            var sliderGo = CreateUIObject(label + " Slider", container.transform);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(400f, 40f);

            var background = sliderGo.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.3f);

            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initialValue;

            var fillArea = CreateUIObject("Fill Area", sliderGo.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(10f, 0f);
            fillAreaRect.offsetMax = new Vector2(-10f, 0f);

            var fill = CreateUIObject("Fill", fillArea.transform);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.7f, 1f, 0.9f);

            var handleArea = CreateUIObject("Handle Slide Area", sliderGo.transform);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handle = CreateUIObject("Handle", handleArea.transform);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 40f);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 0.9f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;

            var valueText = CreateLabel(container.GetComponent<RectTransform>(), Mathf.RoundToInt(initialValue * 100f) + "%", 18);

            slider.onValueChanged.AddListener(v => valueText.text = Mathf.RoundToInt(v * 100f) + "%");
            slider.onValueChanged.Invoke(initialValue);

            return slider;
        }

        private Button CreateButton(RectTransform parent, string text)
        {
            var buttonGo = CreateUIObject(text + " Button", parent);
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 60f);

            var image = buttonGo.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.8f);

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;

            var labelGo = CreateUIObject("Text", buttonGo.transform);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var textComponent = labelGo.AddComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontSize = 28;
            textComponent.color = Color.black;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return button;
        }

        private Text CreateLabel(RectTransform parent, string text, int fontSize)
        {
            var labelGo = CreateUIObject(text + " Label", parent);
            var rect = labelGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, fontSize * 2);

            var textComponent = labelGo.AddComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return textComponent;
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return go;
        }
    }
}

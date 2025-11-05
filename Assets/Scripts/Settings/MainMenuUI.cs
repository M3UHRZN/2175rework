using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Settings
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject levelPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button levelBackButton;
        [SerializeField] private Button settingsBackButton;

        private LevelButton[] levelButtons = Array.Empty<LevelButton>();

        private void Awake()
        {
            CacheReferences();
            RegisterButtonCallbacks();
            ShowMainPanel();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        public void Initialize(GameObject main, GameObject level, GameObject settings)
        {
            mainPanel = main;
            levelPanel = level;
            settingsPanel = settings;
            CacheReferences();
            RegisterButtonCallbacks();
            ShowMainPanel();
        }

        public void ShowMainPanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
            }

            if (levelPanel != null)
            {
                levelPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void ShowLevelPanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }

            if (levelPanel != null)
            {
                levelPanel.SetActive(true);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Level panel açıldığında tüm level butonlarının unlock durumunu güncelle
            UpdateLevelButtonsState();
        }

        public void ShowSettingsPanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }

            if (levelPanel != null)
            {
                levelPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void LoadLevel(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CacheReferences()
        {
            levelButtons = GetComponentsInChildren<LevelButton>(true);

            mainPanel ??= FindChildGameObject(transform, "MainPanel", "Main Panel");
            levelPanel ??= FindChildGameObject(transform, "LevelPanel", "Level Panel");
            settingsPanel ??= FindChildGameObject(transform, "SettingsPanel", "Settings Panel");

            if (mainPanel != null)
            {
                startButton ??= FindChildButton(mainPanel.transform, "StartButton", "Start Button");
                settingsButton ??= FindChildButton(mainPanel.transform, "SettingsButton", "Settings Button");
                exitButton ??= FindChildButton(mainPanel.transform, "ExitButton", "Exit Button", "Çıkış Button");
            }

            if (levelPanel != null)
            {
                levelBackButton ??= FindChildButton(levelPanel.transform, "BackButton", "Back Button", "Geri Button");
            }

            if (settingsPanel != null)
            {
                settingsBackButton ??= FindChildButton(settingsPanel.transform, "BackButton", "Back Button", "Geri Button");
            }
        }

        private void RegisterButtonCallbacks()
        {
            ConfigureButton(startButton, ShowLevelPanel);
            ConfigureButton(settingsButton, ShowSettingsPanel);
            ConfigureButton(exitButton, QuitGame);
            ConfigureButton(levelBackButton, ShowMainPanel);
            ConfigureButton(settingsBackButton, ShowMainPanel);

            if (levelButtons == null || levelButtons.Length == 0)
            {
                levelButtons = GetComponentsInChildren<LevelButton>(true);
            }

            foreach (var levelButton in levelButtons)
            {
                levelButton?.BindToMenu(this);
            }

            // İlk yüklemede unlock durumlarını güncelle
            UpdateLevelButtonsState();
        }

        /// <summary>
        /// Tüm level butonlarının unlock durumunu günceller.
        /// </summary>
        private void UpdateLevelButtonsState()
        {
            if (levelButtons == null || levelButtons.Length == 0)
            {
                levelButtons = GetComponentsInChildren<LevelButton>(true);
            }

            foreach (var levelButton in levelButtons)
            {
                levelButton?.UpdateLockState();
            }
        }

        private static void ConfigureButton(Button button, UnityAction handler)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(handler);
            button.onClick.AddListener(handler);
        }

        internal void RegisterLevelButton(LevelButton button)
        {
            button?.BindToMenu(this);
        }

        private static GameObject FindChildGameObject(Transform parent, params string[] names)
        {
            foreach (var name in names)
            {
                var child = parent.Find(name);
                if (child != null)
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private static Button FindChildButton(Transform parent, params string[] names)
        {
            foreach (var name in names)
            {
                var child = parent.Find(name);
                if (child != null && child.TryGetComponent(out Button button))
                {
                    return button;
                }
            }

            return null;
        }
    }
}

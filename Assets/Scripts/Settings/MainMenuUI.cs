using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Settings
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject levelPanel;
        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            ShowMainPanel();
        }

        public void Initialize(GameObject main, GameObject level, GameObject settings)
        {
            mainPanel = main;
            levelPanel = level;
            settingsPanel = settings;
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
    }
}

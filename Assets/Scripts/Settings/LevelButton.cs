using UnityEngine;
using UnityEngine.UI;

namespace Game.Settings
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        private Button button;
        private MainMenuUI menu;

        public string SceneName
        {
            get => sceneName;
            set => sceneName = value;
        }

        private void Awake()
        {
            CacheComponents();
            menu?.RegisterLevelButton(this);
        }

        private void OnEnable()
        {
            BindToMenu(menu);
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void OnValidate()
        {
            CacheComponents();
        }

        internal void BindToMenu(MainMenuUI targetMenu)
        {
            menu = targetMenu ?? menu;
            CacheComponents();

            if (button == null || menu == null || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                menu?.LoadLevel(sceneName);
            }
        }

        private void CacheComponents()
        {
            button ??= GetComponent<Button>();
            menu ??= GetComponentInParent<MainMenuUI>();
        }
    }
}

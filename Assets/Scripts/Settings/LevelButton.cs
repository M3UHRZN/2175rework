using UnityEngine;
using UnityEngine.UI;

namespace Game.Settings
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        [Header("Lock Visual")]
        [Tooltip("Kilit ikonu (locked seviyeler için gösterilecek). Opsiyonel.")]
        [SerializeField] private GameObject lockIcon;

        [Tooltip("Locked seviyeler için buton rengi. Opsiyonel.")]
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private Button button;
        private MainMenuUI menu;
        private Image buttonImage;
        private Color originalColor;
        private bool isUnlocked;

        public string SceneName
        {
            get => sceneName;
            set => sceneName = value;
        }

        public bool IsUnlocked => isUnlocked;

        private void Awake()
        {
            CacheComponents();
            menu?.RegisterLevelButton(this);
        }

        private void OnEnable()
        {
            BindToMenu(menu);
            UpdateLockState();
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
            
            UpdateLockState();
        }

        /// <summary>
        /// Seviyenin unlock durumunu kontrol eder ve görsel geri bildirimi günceller.
        /// </summary>
        public void UpdateLockState()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            if (LevelProgressSaveManager.Instance == null)
            {
                isUnlocked = true; // SaveManager yoksa tüm seviyeler açık
            }
            else
            {
                isUnlocked = LevelProgressSaveManager.Instance.IsLevelUnlocked(sceneName);
            }

            // Buton durumunu güncelle
            if (button != null)
            {
                button.interactable = isUnlocked;
            }

            // Görsel geri bildirimi güncelle
            UpdateVisualFeedback();
        }

        private void UpdateVisualFeedback()
        {
            // Kilit ikonunu güncelle
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            // Buton rengini güncelle
            if (buttonImage != null)
            {
                if (isUnlocked)
                {
                    buttonImage.color = originalColor;
                }
                else
                {
                    buttonImage.color = lockedColor;
                }
            }
        }

        private void HandleClick()
        {
            if (!string.IsNullOrEmpty(sceneName) && isUnlocked)
            {
                menu?.LoadLevel(sceneName);
            }
        }

        private void CacheComponents()
        {
            button ??= GetComponent<Button>();
            menu ??= GetComponentInParent<MainMenuUI>();
            
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
                if (buttonImage != null)
                {
                    originalColor = buttonImage.color;
                }
            }
        }
    }
}

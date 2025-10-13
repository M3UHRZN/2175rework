using UnityEngine;

namespace MiniGames
{
    /// <summary>
    /// Manages the lifecycle of mini-games. Handles UI switching, player input disabling,
    /// and instantiation of mini-game prefabs.
    /// </summary>
    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("The parent GameObject for all mini-game UI. Will be activated when a game starts.")]
        [SerializeField] private GameObject miniGameContainer;

        [Tooltip("The main player HUD. Will be deactivated when a game starts.")]
        [SerializeField] private GameObject playerHud;

        private BaseMiniGame _currentMiniGame;
        private DualCharacterController _playerController;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Starts a new mini-game.
        /// </summary>
        /// <param name="miniGamePrefab">The prefab of the mini-game to start.</param>
        /// <param name="player">The player who triggered the mini-game.</param>
        public void StartMiniGame(GameObject miniGame, InteractionController player)
        {
            if (_currentMiniGame != null)
            {
                Debug.LogError("Cannot start a new mini-game while another is already running.");
                return;
            }

            // Find the player's main controller
            _playerController = Object.FindAnyObjectByType<DualCharacterController>();
            if (_playerController == null)
            {
                Debug.LogError("Could not find DualCharacterController on the player.", player);
                return;
            }

            // Disable player controls and switch UI
            _playerController.SendMessage("LockControl", SendMessageOptions.RequireReceiver);
            if (playerHud) playerHud.SetActive(false);
            if (miniGameContainer) miniGameContainer.SetActive(true);

            // Activate and setup the mini-game
            miniGame.SetActive(true);
            _currentMiniGame = miniGame.GetComponent<BaseMiniGame>();

            if (_currentMiniGame != null)
            {
                _currentMiniGame.StartMiniGame(player);
            }
            else
            {
                Debug.LogError("The provided GameObject does not contain a component derived from BaseMiniGame.", miniGame);
                CloseCurrentMiniGame(); // Clean up if setup fails
            }
        }

        /// <summary>
        /// Closes the currently active mini-game and returns to the main game view.
        /// This is typically called by the BaseMiniGame itself.
        /// </summary>
        public void CloseCurrentMiniGame()
        {
            if (_currentMiniGame == null) return;

            // Re-enable player controls and switch UI back
            if (_playerController != null)
            {
                _playerController.SendMessage("UnlockControl", SendMessageOptions.RequireReceiver);
            }

            if (playerHud) playerHud.SetActive(true);
            if (miniGameContainer) miniGameContainer.SetActive(false);

            // Deactivate the mini-game instance instead of destroying it
            _currentMiniGame.gameObject.SetActive(false);
            _currentMiniGame = null;
            _playerController = null;
        }
    }
}

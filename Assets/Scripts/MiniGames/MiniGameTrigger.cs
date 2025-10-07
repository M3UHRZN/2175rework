using UnityEngine;

namespace MiniGames
{
    /// <summary>
    /// This component sits on an Interactable object and launches a specified mini-game
    /// when the interaction is successfully completed.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class MiniGameTrigger : MonoBehaviour
    {
        [Header("Mini-Game Setup")]
        [Tooltip("The prefab of the mini-game UI to be launched.")]
        [SerializeField] private GameObject miniGamePrefab;

        private Interactable _interactable;

        void Awake()
        {
            _interactable = GetComponent<Interactable>();
            if (miniGamePrefab == null)
            {
                Debug.LogError("MiniGameTrigger on " + gameObject.name + " is missing a Mini-Game Prefab!", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _interactable.OnInteractComplete.AddListener(LaunchMiniGame);
        }

        private void OnDisable()
        {
            _interactable.OnInteractComplete.RemoveListener(LaunchMiniGame);
        }

        /// <summary>
        /// Called when the interaction is complete. Launches the mini-game.
        /// </summary>
        /// <param name="player">The player who completed the interaction.</param>
        private void LaunchMiniGame(InteractionController player)
        {
            if (!enabled) return;

            Debug.Log($"Launching mini-game from {gameObject.name} for player {player.name}.", this);
            MiniGameManager.Instance.StartMiniGame(miniGamePrefab, player);
        }
    }
}

using UnityEngine;
using UnityEngine.Events;

namespace MiniGames
{
    /// <summary>
    /// Base class for all mini-games. Provides common functionality and lifecycle management.
    /// </summary>
    public abstract class BaseMiniGame : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Fired when the mini-game is successfully completed.")]
        public UnityEvent OnMiniGameComplete;

        /// <summary>
        /// Called when the mini-game starts. Override this to implement game-specific logic.
        /// </summary>
        /// <param name="player">The player who triggered the mini-game.</param>
        public abstract void StartMiniGame(InteractionController player);

        /// <summary>
        /// Ends the mini-game and notifies the manager to clean up.
        /// </summary>
        /// <param name="wasCompleted">Whether the mini-game was completed successfully.</param>
        public virtual void EndMiniGame(bool wasCompleted)
        {
            if (wasCompleted)
            {
                Debug.Log($"[{GetType().Name}] Mini-game completed successfully.");
                OnMiniGameComplete?.Invoke();
            }
            else
            {
                Debug.Log($"[{GetType().Name}] Mini-game ended without completion.");
            }

            // Notify the manager to clean up and return to the main game.
            MiniGameManager.Instance.CloseCurrentMiniGame();
        }
    }
}

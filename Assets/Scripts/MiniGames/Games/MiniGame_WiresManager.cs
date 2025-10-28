using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Puzzle;

namespace MiniGames.Games
{
    public class MiniGame_WiresManager : BaseMiniGame, ILogicSource
    {
        [Header("Game Settings")]
       // [SerializeField] private int numberOfColors = 4;
        [SerializeField] private List<Color> availableColors = new List<Color>();
        
        [Header("UI Elements")]
        [SerializeField] private Button cancelButton;
        
        [Header("Manual Setup")]
        [SerializeField] private List<PlugUI> allPlugs = new List<PlugUI>();
        [SerializeField] private List<SocketUI> allSockets = new List<SocketUI>();
        
        private int connectionsMade = 0;
        private int totalConnections = 0;
        private bool isCompleted = false;
        
        // ILogicSource implementation
        public bool IsActive => isCompleted;
        
        public override void StartMiniGame(InteractionController player)
        {
            Debug.Log($"Starting Wires Mini Game with {allPlugs.Count} plugs and {allSockets.Count} sockets");
            
            InitializeColors();
            SetupManualElements();
            
            totalConnections = allPlugs.Count;
            connectionsMade = 0;
            isCompleted = false;
            
            Debug.Log($"Setup complete: {allPlugs.Count} plugs, {allSockets.Count} sockets");
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(() => EndMiniGame(false));
            }
        }
        
        private void InitializeColors()
        {
            if (availableColors.Count == 0)
            {
                availableColors = new List<Color>
                {
                    Color.red,
                    Color.blue,
                    Color.yellow,
                    Color.green
                };
            }
        }
        
        private void SetupManualElements()
        {
            // Initialize all manually placed plugs
            foreach (var plug in allPlugs)
            {
                if (plug != null)
                {
                    Debug.Log($"Setting up plug: {plug.name}");
                    // Plug'lar zaten sahne'de, sadece başlangıç pozisyonlarını kaydet
                }
            }
            
            // Initialize all manually placed sockets
            foreach (var socket in allSockets)
            {
                if (socket != null)
                {
                    Debug.Log($"Setting up socket: {socket.name}, isRightSide: {socket.isRightSide}");
                    // Socket'ler zaten sahne'de, sadece durumlarını sıfırla
                    socket.occupied = false;
                }
            }
        }
        
        
        public void OnConnectionMade()
        {
            connectionsMade++;
            
            Debug.Log($"Connection made! {connectionsMade}/{totalConnections}");
            
            if (connectionsMade >= totalConnections)
            {
                // All connections made - game completed!
                Debug.Log("All wires connected! Game completed!");
                isCompleted = true;
                
                // Find and trigger LogicTrigger on the same GameObject
                var logicTrigger = GetComponent<Puzzle.LogicTrigger>();
                if (logicTrigger != null)
                {
                    logicTrigger.Trigger();
                    Debug.Log("LogicTrigger activated! IsActive: " + logicTrigger.IsActive);
                }
                else
                {
                    Debug.LogWarning("No LogicTrigger found on this GameObject!");
                }
                
                EndMiniGame(true);
            }
        }
        
        void OnDestroy()
        {
            // Manual setup'ta temizleme gerekmiyor, sahne'deki objeler kalıyor
        }
    }
}

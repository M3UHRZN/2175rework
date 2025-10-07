using UnityEngine;
using UnityEngine.UI;

namespace MiniGames.Games
{
    /// <summary>
    /// Editor utility to quickly create SocketUI and PlugUI prefabs
    /// </summary>
    public class PrefabCreator : MonoBehaviour
    {
        [ContextMenu("Create SocketUI Prefab")]
        public void CreateSocketUIPrefab()
        {
            // Create SocketUI GameObject
            GameObject socketObj = new GameObject("SocketUI");
            socketObj.transform.SetParent(transform, false);
            
            // Add Image component
            var image = socketObj.AddComponent<Image>();
            image.color = Color.white;
            
            // Add RectTransform and set size
            var rectTransform = socketObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50f, 50f);
            
            // Add SocketUI script
            var socketUI = socketObj.AddComponent<SocketUI>();
            socketUI.colorId = 0;
            socketUI.isRightSide = false;
            socketUI.occupied = false;
            
            Debug.Log("SocketUI prefab created! Save it as a prefab.");
        }
        
        [ContextMenu("Create PlugUI Prefab")]
        public void CreatePlugUIPrefab()
        {
            // Create PlugUI GameObject
            GameObject plugObj = new GameObject("PlugUI");
            plugObj.transform.SetParent(transform, false);
            
            // Add Image component
            var image = plugObj.AddComponent<Image>();
            image.color = Color.white;
            
            // Add RectTransform and set size
            var rectTransform = plugObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(30f, 30f);
            
            // Add PlugUI script
            var plugUI = plugObj.AddComponent<PlugUI>();
            plugUI.colorId = 0;
            plugUI.originSocket = null; // Will be set at runtime
            
            Debug.Log("PlugUI prefab created! Save it as a prefab.");
        }
    }
}

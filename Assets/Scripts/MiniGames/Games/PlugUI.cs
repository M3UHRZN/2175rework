using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MiniGames.Games
{
    public class PlugUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Plug Properties")]
        public int colorId;
        public SocketUI originSocket;
        public Transform anchorPoint; // Kablonun başlayacağı nokta
        public bool isConnected = false;
        
        [Header("Visual Elements")]
        [SerializeField] private Image plugImage;
        [SerializeField] private GameObject dragProxy; // UI imleci takip eden imge
        [SerializeField] private GameObject activeLine; // Aktif çizgi UI objesi
        
        private Vector3 originalPosition;
        private bool isDragging = false;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (plugImage == null)
                plugImage = GetComponent<Image>();
                
            originalPosition = rectTransform.anchoredPosition;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isDragging) return;
            
            StartDragging();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            UpdateDragPosition(eventData);
            UpdateActiveLine(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            EndDragging(eventData);
        }
        
        private void StartDragging()
        {
            isDragging = true;
            
            // Create drag proxy if not exists
            if (dragProxy == null)
            {
                CreateDragProxy();
            }
            
            // Create active line if not exists
            if (activeLine == null)
            {
                CreateActiveLine();
            }
            
            // Disable raycast blocking for this plug
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
                
            // Show drag proxy and active line
            if (dragProxy != null)
                dragProxy.SetActive(true);
            if (activeLine != null)
                activeLine.SetActive(true);
        }
        
        private void UpdateDragPosition(PointerEventData eventData)
        {
            if (dragProxy != null)
            {
                dragProxy.transform.position = eventData.position;
            }
        }
        
        private void UpdateActiveLine(PointerEventData eventData)
        {
            if (activeLine == null) return;
            
            Vector3 startPos = anchorPoint != null ? anchorPoint.position : originalPosition;
            Vector3 endPos = eventData.position;
            
            UpdateLineVisual(activeLine, startPos, endPos);
        }
        
        private void EndDragging(PointerEventData eventData)
        {
            isDragging = false;
            
            // Re-enable raycast blocking
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = true;
            
            // Hide drag proxy and active line
            if (dragProxy != null)
                dragProxy.SetActive(false);
            if (activeLine != null)
                activeLine.SetActive(false);
            
            // Check for valid drop
            CheckForValidDrop(eventData);
        }
        
        private void CheckForValidDrop(PointerEventData eventData)
        {
            // Raycast to find socket under cursor
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                SocketUI targetSocket = result.gameObject.GetComponent<SocketUI>();
                if (targetSocket != null && IsValidDrop(targetSocket))
                {
                    // Valid drop - connect to socket
                    ConnectToSocket(targetSocket);
                    return;
                }
            }
            
            // Invalid drop - return to origin
            ReturnToOrigin();
        }
        
        private bool IsValidDrop(SocketUI targetSocket)
        {
            return targetSocket.isRightSide && 
                   !targetSocket.occupied && 
                   targetSocket.colorId == this.colorId;
        }
        
        private void ConnectToSocket(SocketUI targetSocket)
        {
            // Mark target as occupied
            targetSocket.SetOccupied(true);
            
            // Create permanent line BEFORE moving plug
            CreatePermanentLine(targetSocket);
            
            // Move plug to target position
            rectTransform.position = targetSocket.GetAnchorPosition();
            
            // Mark plug as connected
            isConnected = true;
            
            // Notify manager
            var manager = FindFirstObjectByType<MiniGame_WiresManager>();
            if (manager != null)
            {
                manager.OnConnectionMade();
            }
        }
        
        private void ReturnToOrigin()
        {
            rectTransform.anchoredPosition = originalPosition;
        }
        
        private void CreateDragProxy()
        {
            dragProxy = new GameObject("DragProxy");
            dragProxy.transform.SetParent(transform.parent, false);
            
            var proxyImage = dragProxy.AddComponent<Image>();
            proxyImage.sprite = plugImage.sprite;
            proxyImage.color = new Color(1f, 1f, 1f, 0.7f); // Semi-transparent
            
            var proxyRect = dragProxy.GetComponent<RectTransform>();
            proxyRect.sizeDelta = rectTransform.sizeDelta;
            
            var proxyCanvasGroup = dragProxy.AddComponent<CanvasGroup>();
            proxyCanvasGroup.blocksRaycasts = false;
            
            dragProxy.SetActive(false);
        }
        
        private void CreateActiveLine()
        {
            activeLine = new GameObject("ActiveLine");
            activeLine.transform.SetParent(transform.parent, false);
            
            var lineImage = activeLine.AddComponent<Image>();
            lineImage.color = GetColorFromId(colorId);
            lineImage.raycastTarget = false;
            
            var lineRect = activeLine.GetComponent<RectTransform>();
            lineRect.sizeDelta = new Vector2(100f, 4f); // Default size
            
            activeLine.SetActive(false);
        }
        
        private void CreatePermanentLine(SocketUI targetSocket)
        {
            GameObject permanentLine = new GameObject("PermanentLine");
            permanentLine.transform.SetParent(transform.parent, false);
            
            var lineImage = permanentLine.AddComponent<Image>();
            lineImage.color = GetColorFromId(colorId);
            lineImage.raycastTarget = false;
            
            // Use anchorPoint as start position, fallback to original position
            Vector3 startPos = anchorPoint != null ? anchorPoint.position : originalPosition;
            Vector3 endPos = targetSocket.GetAnchorPosition();
            
            Debug.Log($"Creating line from {startPos} to {endPos}. AnchorPoint: {(anchorPoint != null ? anchorPoint.name : "null")}");
            
            UpdateLineVisual(permanentLine, startPos, endPos);
        }
        
        private void UpdateLineVisual(GameObject lineObj, Vector3 startPos, Vector3 endPos)
        {
            if (lineObj == null) return;
            
            var lineRect = lineObj.GetComponent<RectTransform>();
            var lineImage = lineObj.GetComponent<Image>();
            
            // Calculate distance and angle
            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Set position to midpoint
            Vector3 midPoint = (startPos + endPos) / 2f;
            lineRect.position = midPoint;
            
            // Set size
            lineRect.sizeDelta = new Vector2(distance, 4f);
            
            // Set rotation
            lineRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        private Color GetColorFromId(int id)
        {
            switch (id)
            {
                case 0: return Color.red;
                case 1: return Color.blue;
                case 2: return Color.yellow;
                case 3: return Color.green;
                default: return Color.white;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MiniGames.Games
{
    public class SocketUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Socket Properties")]
        public int colorId;
        public bool isRightSide;
        public bool occupied = false;
        public Transform anchor; // Uç nokta için boş child
        
        [Header("Visual Elements")]
        [SerializeField] private Image socketImage;
        [SerializeField] private Image highlightImage;
        
        private Color originalColor;
        private bool isHighlighted = false;
        
        void Awake()
        {
            if (anchor == null)
            {
                // Create anchor point if not assigned
                GameObject anchorObj = new GameObject("Anchor");
                anchorObj.transform.SetParent(transform, false);
                anchor = anchorObj.transform;
            }
            
            if (socketImage == null)
                socketImage = GetComponent<Image>();
                
            if (highlightImage == null)
            {
                // Create highlight image
                GameObject highlightObj = new GameObject("Highlight");
                highlightObj.transform.SetParent(transform, false);
                highlightImage = highlightObj.AddComponent<Image>();
                highlightImage.color = new Color(1f, 1f, 0f, 0.3f); // Yellow highlight
                highlightImage.raycastTarget = false;
                highlightImage.enabled = false;
            }
            
            originalColor = socketImage.color;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!occupied && !isHighlighted)
            {
                ShowHighlight();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isHighlighted)
            {
                HideHighlight();
            }
        }
        
        public void ShowHighlight()
        {
            isHighlighted = true;
            if (highlightImage != null)
                highlightImage.enabled = true;
        }
        
        public void HideHighlight()
        {
            isHighlighted = false;
            if (highlightImage != null)
                highlightImage.enabled = false;
        }
        
        public void SetOccupied(bool occupied)
        {
            this.occupied = occupied;
            if (occupied)
            {
                socketImage.color = Color.gray;
                HideHighlight();
            }
            else
            {
                socketImage.color = originalColor;
            }
        }
        
        public Vector3 GetAnchorPosition()
        {
            return anchor.position;
        }
    }
}

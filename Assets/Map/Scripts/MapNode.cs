using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Map
{
    public enum NodeStates
    {
        Locked,
        Visited,
        Attainable
    }
}

namespace Map
{
    public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public SpriteRenderer sr;
        public Image image;
        public SpriteRenderer visitedCircle;
        public Image circleImage;
        public Image visitedCircleImage;

        public Node Node { get; private set; }
        public NodeBlueprint Blueprint { get; private set; }

        private float initialScale;
        private const float HoverScaleFactor = 1.2f;
        private float mouseDownTime;

        private const float MaxClickDuration = 0.5f;
        
        public Transform iconTransform;
        public float scaleFactor = 0.1f; // Scale factor to adjust the scaling speed
        public float pingPongSpeed = 1.0f; // Speed of the ping-pong animation
        private Vector3 initialScaleIcon;
        private bool isPingPongingScale = false;
        private Color BaseColor;
        public Color pingPongColor;
        private bool isPingPongingColor = false;
        
        private ToolTip toolTip;
        
        void Start()
        {
            iconTransform = this.transform;
            initialScaleIcon = iconTransform.localScale;
            BaseColor = image.color;
        }
        

        public void SetUp(Node node, NodeBlueprint blueprint)
        {
            Node = node;
            Blueprint = blueprint;
            if (sr != null) sr.sprite = blueprint.sprite;
            if (image != null) image.sprite = blueprint.sprite;
            if (node.nodeType == NodeType.Boss) transform.localScale *= 1.5f;
            if (sr != null) initialScale = sr.transform.localScale.x;
            if (image != null) initialScale = image.transform.localScale.x;

            if (visitedCircle != null)
            {
                visitedCircle.color = MapView.Instance.visitedColor;
                visitedCircle.gameObject.SetActive(false);
            }

            if (circleImage != null)
            {
                circleImage.color = MapView.Instance.visitedColor;
                circleImage.gameObject.SetActive(false);    
            }
            
            SetState(NodeStates.Locked);
            
            SetNodeToolTip(node);
        }

        public void SetState(NodeStates state)
        {
            if (visitedCircle != null) visitedCircle.gameObject.SetActive(false);
            if (circleImage != null) circleImage.gameObject.SetActive(false);
            
            Node.SetState(state);
            
            switch (state)
            {
                case NodeStates.Locked:
                    if (sr != null)
                    {
                        //sr.DOKill();
                        sr.color = MapView.Instance.lockedColor;
                        isPingPongingScale = false;
                        isPingPongingColor = false;
                    }

                    if (image != null)
                    {
                        //image.DOKill();
                        image.color = MapView.Instance.lockedColor;
                        isPingPongingScale = false;
                        isPingPongingColor = false;
                    }

                    break;
                case NodeStates.Visited:
                    if (sr != null)
                    {
                        //sr.DOKill();
                        sr.color = MapView.Instance.visitedColor;
                        isPingPongingScale = false;
                        isPingPongingColor = false;

                    }
                    
                    if (image != null)
                    {
                        //image.DOKill();
                        image.color = MapView.Instance.visitedColor;
                        isPingPongingScale = false;
                        isPingPongingColor = false;
                        
                    }
                    
                    if (visitedCircle != null) visitedCircle.gameObject.SetActive(true);
                    if (circleImage != null) circleImage.gameObject.SetActive(true);
                    break;
                case NodeStates.Attainable:
                    // start pulsating from visited to locked color:
                    if (sr != null)
                    {
                        sr.color = MapView.Instance.lockedColor;
                        isPingPongingScale = true;
                        isPingPongingColor = true;
                        //sr.DOKill();
                        //sr.DOColor(MapView.Instance.visitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                    
                    if (image != null)
                    {
                        image.color = MapView.Instance.lockedColor;
                        isPingPongingScale = true;
                        isPingPongingColor = true;
                        //image.DOKill();
                        //image.DOColor(MapView.Instance.visitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void OnPointerEnter(PointerEventData data)
        {
            if (sr != null)
            {
                //sr.transform.DOKill();
                //sr.transform.DOScale(initialScale * HoverScaleFactor, 0.3f);
            }

            if (image != null)
            {
                image.color = MapView.Instance.visitedColor;
                //image.transform.DOKill();
                //image.transform.DOScale(initialScale * HoverScaleFactor, 0.3f);
            }
        }

        public void OnPointerExit(PointerEventData data)
        {
            if (sr != null)
            {
                //sr.transform.DOKill();
                //sr.transform.DOScale(initialScale, 0.3f);
            }

            if (image != null)
            {
                if(Node.State == NodeStates.Visited)
                    image.color = MapView.Instance.visitedColor;
                if(Node.State == NodeStates.Locked)
                    image.color = MapView.Instance.lockedColor;

                //image.transform.DOKill();
                //image.transform.DOScale(initialScale, 0.3f);
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            mouseDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (!CombatController._instance.MapCanBeClicked || Node.State != NodeStates.Attainable)
            {
                return;
            }
            if (Time.time - mouseDownTime < MaxClickDuration)
            {
                // user clicked on this node:
                MapPlayerTracker.Instance.SelectNode(this);
                CombatController._instance.MapNodeClicked(this.Node.nodeType);
                UIController._instance.ToggleMapUI(0);
                UIController._instance.ToggleInventoryUI(0);
                UIController._instance.ToggleLootUI(0);
                UIController._instance.ToggleShopUI(0);
            }
        }

        public void ShowSwirlAnimation()
        {
            if (visitedCircleImage == null)
                return;

            const float fillDuration = 0.3f;
            visitedCircleImage.fillAmount = 0;

            //DOTween.To(() => visitedCircleImage.fillAmount, x => visitedCircleImage.fillAmount = x, 1f, fillDuration);
        }

        private void Update()
        {
            if (isPingPongingScale)
            {
                ScalePingPong();
            }

            if (isPingPongingColor)
            {
                ColorPingPong();
            }
        }


        private void ScalePingPong()
        {
            // Calculate the new scale using ping-pong function
            float scale = Mathf.PingPong(Time.time * pingPongSpeed, 1.0f) * scaleFactor + 1.0f;
            //Debug.Log(scale);
            // Apply the new scale
            iconTransform.localScale = initialScaleIcon * scale;
            
        }
        private void ColorPingPong()
        {
            var pingPong = Mathf.PingPong(Time.time, 1);
            var c = Color.Lerp(BaseColor, pingPongColor, pingPong);
            image.color = c;
        }

        private void SetNodeToolTip(Node n)
        {
            if (toolTip == null)
                toolTip = GetComponent<ToolTip>();

            toolTip.rarity = -1;
            switch (n.nodeType)
            {
                case NodeType.MinorEnemy:
                    toolTip.Title = "Combat";
                    toolTip.Message = "Duel against an adventurer";
                    break;
                case NodeType.EliteEnemy:
                    toolTip.Title = "Elite";
                    toolTip.Message = "Duel against a powerful adventurer, Defeating them will reward a relic";
                    break;
                case NodeType.Store:
                    toolTip.Title = "Shop";
                    toolTip.Message = "Spend your gold and sell your items here";
                    break;
                case NodeType.Mystery:
                    toolTip.Title = "Unknown";
                    toolTip.Message = "You dont know what will be here";
                    break;
                case NodeType.Boss:
                    toolTip.Title = "Dragon";
                    toolTip.Message = "A powerful dragon lives here, Defeating them will reward a dragon relic";
                    break;
                    
            }
        }
    }
}

// Simple Scroll-Snap
// Version: 1.0.1
// Author: Daniel Lochner

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DanielLochner.Assets.SimpleScrollSnap
{
    [AddComponentMenu("UI/Simple Scroll-Snap")]
    [RequireComponent(typeof(ScrollRect))]
    [Serializable]
    public class SimpleScrollSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Fields
        private int nearestPanel, targetPanel, currentPanel, numberOfPanels, numberOfToggles;
        private bool dragging, pressing, selected, setVelocity;
        private float releaseSpeed, planeDistance;
        private Vector2 contentSize, previousPosition;
        private Direction releaseDirection;
        private CanvasGroup canvasGroup;
        private GameObject[] panels;
        private Toggle[] toggles;

        public MovementType movementType = MovementType.Fixed;
        public MovementAxis movementAxis = MovementAxis.Horizontal;
        public bool automaticallyLayout = true;
        public Vector2 size = new Vector2(400, 250);
        public float automaticLayoutSpacing = 0.25f;
        public float leftMargin, rightMargin, topMargin, bottomMargin;
        public bool infinitelyScroll = false;
        public float infiniteScrollingEndSpacing = 0f;
        public int startingPanel = 0;
        public bool swipeGestures = true;
        public float thresholdSwipeSpeed = 0f;
        public Button previousButton = null;
        public Button nextButton = null;
        public GameObject pagination = null;
        public bool toggleNavigation = true;
        public SnapTarget snapTarget = SnapTarget.Nearest;
        public float snappingSpeed = 10f;
        public float thresholdSnappingSpeed = -1f;
        public bool hardSnap = true;
        public UnityEvent onPanelChanged, onPanelSelecting, onPanelSelected, onPanelChanging;
        public List<TransitionEffect> transitionEffects = new List<TransitionEffect>();        
        #endregion

        #region Properties
        public int CurrentPanel
        {
            get { return currentPanel; }
        }
        public int TargetPanel
        {
            get { return targetPanel; }
        }
        public int NearestPanel
        {
            get { return nearestPanel; }
        }
        public int NumberOfPanels
        {
            get { return numberOfPanels; }
        }
        public ScrollRect ScrollRect
        {
            get { return GetComponent<ScrollRect>(); }
        }
        public RectTransform Content
        {
            get { return GetComponent<ScrollRect>().content; }
        }
        public RectTransform Viewport
        {
            get { return GetComponent<ScrollRect>().viewport; }
        }
        public GameObject[] Panels
        {
            get { return panels; }
        }
        public Toggle[] Toggles
        {
            get { return toggles; }
        }
        #endregion

        #region Enumerators
        public enum MovementType
        {
            Fixed,
            Free
        }
        public enum MovementAxis
        {
            Horizontal,
            Vertical
        }
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
        public enum SnapTarget
        {
            Nearest,
            Previous,
            Next
        }
        #endregion

        #region Methods
        private void Awake()
        {
            if (Validate())
            {
                Setup();
            }
            else
            {
                throw new Exception("Invalid inspector input.");
            }
        }
        private void Update()
        {
            OnSelectingAndSnapping();
            OnInfiniteScrolling();
            OnTransitionEffects();
            OnSwipeGestures();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (swipeGestures)
            {
                pressing = true;
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (swipeGestures)
            {
                if (hardSnap)
                {
                    ScrollRect.inertia = true;
                }
                selected = false;
                dragging = true;
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (swipeGestures)
            {
                Vector2 position = eventData.position;
                if (position.x != previousPosition.x && position.y != previousPosition.y)
                {
                    if (movementAxis == MovementAxis.Horizontal)
                    {
                        releaseDirection = (position.x > previousPosition.x) ? Direction.Right : Direction.Left;
                    }
                    else if (movementAxis == MovementAxis.Vertical)
                    {
                        releaseDirection = (position.y > previousPosition.y) ? Direction.Up : Direction.Down;
                    }
                }
                previousPosition = eventData.position;
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (swipeGestures)
            {
                releaseSpeed = ScrollRect.velocity.magnitude;
                dragging = false;
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (swipeGestures)
            {
                pressing = false;
            }
        }

        private bool Validate()
        {
            numberOfPanels = Content.childCount;
            
            bool valid = true;
            if (numberOfPanels == 0)
            {
                Debug.LogError("<b>[SimpleScrollSnap]</b> You must have at least one panel in your content.", gameObject);
                valid = false;
            }

            if (pagination != null)
            {
                numberOfToggles = pagination.transform.childCount;
                if (numberOfToggles != numberOfPanels)
                {
                    Debug.LogError("<b>[SimpleScrollSnap]</b> The number of toggles should be equivalent to the number of panels. There are currently " + numberOfToggles + " toggles and " + numberOfPanels + " panels.", gameObject);
                    valid = false;
                }
            }

            return valid;
        }
        private void Setup()
        {
            // Canvas & Camera
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.planeDistance = (canvas.GetComponent<RectTransform>().rect.height / 2f) / Mathf.Tan((canvas.worldCamera.fieldOfView / 2f) * Mathf.Deg2Rad);
                if (canvas.worldCamera.farClipPlane < canvas.planeDistance)
                {
                    canvas.worldCamera.farClipPlane = Mathf.Ceil(canvas.planeDistance);
                }
            }

            // ScrollRect
            if (movementType == MovementType.Fixed)
            {
                ScrollRect.horizontal = (movementAxis == MovementAxis.Horizontal);
                ScrollRect.vertical = (movementAxis == MovementAxis.Vertical);
            }
            else
            {
                ScrollRect.horizontal = ScrollRect.vertical = true;
            }

            // Panels
            panels = new GameObject[numberOfPanels];
            for (int i = 0; i < numberOfPanels; i++)
            {
                panels[i] = ((RectTransform)Content.GetChild(i)).gameObject;

                if (movementType == MovementType.Fixed && automaticallyLayout)
                {
                    panels[i].GetComponent<RectTransform>().anchorMin = new Vector2(movementAxis == MovementAxis.Horizontal ? 0f : 0.5f, movementAxis == MovementAxis.Vertical ? 0f : 0.5f); ;
                    panels[i].GetComponent<RectTransform>().anchorMax = new Vector2(movementAxis == MovementAxis.Horizontal ? 0f : 0.5f, movementAxis == MovementAxis.Vertical ? 0f : 0.5f); ;

                    float x = (rightMargin + leftMargin) / 2f - leftMargin;
                    float y = (topMargin + bottomMargin) / 2f - bottomMargin;
                    Vector2 marginOffset = new Vector2(x / size.x, y / size.y);
                    panels[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f) + marginOffset;
                    panels[i].GetComponent<RectTransform>().sizeDelta = size - new Vector2(leftMargin + rightMargin, topMargin + bottomMargin);

                    float panelPosX = (movementAxis == MovementAxis.Horizontal) ? i * (automaticLayoutSpacing + 1f) * size.x + (size.x / 2f) : 0f;
                    float panelPosY = (movementAxis == MovementAxis.Vertical) ? i * (automaticLayoutSpacing + 1f) * size.y + (size.y / 2f) : 0f;
                    panels[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(panelPosX, panelPosY, 0f);
                }
            }

            // Content
            if (movementType == MovementType.Fixed)
            {
                // Automatic Layout
                if (automaticallyLayout)
                {
                    Content.anchorMin = new Vector2(movementAxis == MovementAxis.Horizontal ? 0f : 0.5f, movementAxis == MovementAxis.Vertical ? 0f : 0.5f);
                    Content.anchorMax = new Vector2(movementAxis == MovementAxis.Horizontal ? 0f : 0.5f, movementAxis == MovementAxis.Vertical ? 0f : 0.5f);
                    Content.pivot = new Vector2(movementAxis == MovementAxis.Horizontal ? 0f : 0.5f, movementAxis == MovementAxis.Vertical ? 0f : 0.5f);

                    Vector2 min = panels[0].transform.position;
                    Vector2 max = panels[numberOfPanels - 1].transform.position;

                    float contentWidth = (movementAxis == MovementAxis.Horizontal) ? (numberOfPanels * (automaticLayoutSpacing + 1f) * size.x) - (size.x * automaticLayoutSpacing) : size.x;
                    float contentHeight = (movementAxis == MovementAxis.Vertical) ? (numberOfPanels * (automaticLayoutSpacing + 1f) * size.y) - (size.y * automaticLayoutSpacing) : size.y;
                    Content.sizeDelta = new Vector2(contentWidth, contentHeight);
                }

                // Infinite Scrolling
                if (infinitelyScroll)
                {
                    ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
                    contentSize = ((Vector2)panels[numberOfPanels - 1].transform.localPosition - (Vector2)panels[0].transform.localPosition) + (panels[numberOfPanels - 1].GetComponent<RectTransform>().sizeDelta / 2f + panels[0].GetComponent<RectTransform>().sizeDelta / 2f) + (new Vector2(movementAxis == MovementAxis.Horizontal ? infiniteScrollingEndSpacing * size.x : 0f, movementAxis == MovementAxis.Vertical ? infiniteScrollingEndSpacing * size.y : 0f));
                }
            }

            // Starting Panel
            float xOffset = (ScrollRect.horizontal) ? Viewport.GetComponent<RectTransform>().rect.width / 2f : 0f;
            float yOffset = (ScrollRect.vertical) ? Viewport.GetComponent<RectTransform>().rect.height / 2f : 0f;
            Vector2 offset = new Vector2(xOffset, yOffset);
            Content.anchoredPosition = -(Vector2)panels[startingPanel].transform.localPosition + offset;
            currentPanel = targetPanel = nearestPanel = startingPanel;

            // Previous Button
            if (previousButton != null)
            {
                previousButton.onClick.AddListener(delegate { GoToPreviousPanel(); });
            }

            // Next Button
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(delegate { GoToNextPanel(); });
            }

            // Pagination
            if (pagination != null)
            {
                toggles = new Toggle[numberOfToggles];
                for (int i = 0; i < numberOfToggles; i++)
                {
                    toggles[i] = pagination.transform.GetChild(i).GetComponent<Toggle>();
                    toggles[i].isOn = (i == startingPanel);
                    toggles[i].interactable = (i != targetPanel);
                    int panelNum = i;
                    toggles[i].onValueChanged.AddListener(delegate
                    {
                        if (toggles[panelNum].isOn && toggleNavigation)
                        {
                            GoToPanel(panelNum);
                        }
                    });
                }
            }
        }

        private Vector2 DisplacementFromCenter(Vector2 position)
        {
            return position - (Vector2)Viewport.position;
        }
        private int DetermineNearestPanel()
        {
            int panelNumber = nearestPanel;
            float[] distances = new float[numberOfPanels];
            for (int i = 0; i < panels.Length; i++)
            {
                distances[i] = DisplacementFromCenter(panels[i].transform.position).magnitude;
            }
            float minDistance = Mathf.Min(distances);
            for (int i = 0; i < panels.Length; i++)
            {
                if(minDistance == distances[i])
                {
                    panelNumber = i;
                }
            }
            return panelNumber;
        }
        private void SelectTargetPanel()
        {
            nearestPanel = DetermineNearestPanel();
            if (snapTarget == SnapTarget.Nearest)
            {
                GoToPanel(nearestPanel);
            }
            else if (snapTarget == SnapTarget.Previous)
            {
                if (releaseDirection == Direction.Right)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).x < 0f)
                    {
                        GoToNextPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
                else if (releaseDirection == Direction.Left)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).x > 0f)
                    {
                        GoToPreviousPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
                else if (releaseDirection == Direction.Up)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).y < 0f)
                    {
                        GoToNextPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
                else if (releaseDirection == Direction.Down)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).y > 0f)
                    {
                        GoToPreviousPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
            }
            else if (snapTarget == SnapTarget.Next)
            {
                if (releaseDirection == Direction.Right)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).x > 0f)
                    {
                        GoToPreviousPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
                else if (releaseDirection == Direction.Left)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).x < 0f)
                    {
                        GoToNextPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
                else if (releaseDirection == Direction.Up)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).y > 0f)
                    {
                        GoToPreviousPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
                else if (releaseDirection == Direction.Down)
                {
                    if (DisplacementFromCenter(panels[nearestPanel].transform.position).y < 0f)
                    {
                        GoToNextPanel();
                    }
                    else
                    {
                        GoToPanel(nearestPanel);
                    }
                }
            }
        }
        private void SnapToTargetPanel()
        {
            float xOffset = (ScrollRect.horizontal) ? Viewport.GetComponent<RectTransform>().rect.width / 2f : 0f;
            float yOffset = (ScrollRect.vertical) ? Viewport.GetComponent<RectTransform>().rect.height / 2f : 0f;
            Vector2 offset = new Vector2(xOffset, yOffset);

            Vector2 targetPosition = (-(Vector2)panels[targetPanel].transform.localPosition + offset);
            Content.anchoredPosition = Vector2.Lerp(Content.anchoredPosition, targetPosition, Time.deltaTime * snappingSpeed);

            if (DisplacementFromCenter(panels[targetPanel].transform.position).magnitude < (panels[targetPanel].GetComponent<RectTransform>().rect.width / 10f) && targetPanel != currentPanel)
            {
                onPanelChanged.Invoke();
                currentPanel = targetPanel;
            }
            else if(ScrollRect.velocity != Vector2.zero)
            {
                onPanelChanging.Invoke();
            }
        }

        private void OnSelectingAndSnapping()
        {
            if (!dragging && !pressing)
            {
                // Snap/Select after Swiping
                if (releaseSpeed >= thresholdSwipeSpeed)
                {
                    if (ScrollRect.velocity.magnitude <= thresholdSnappingSpeed || thresholdSnappingSpeed == -1f)
                    {
                        if (selected)
                        {
                            SnapToTargetPanel();
                        }
                        else
                        {
                            SelectTargetPanel();
                        }
                    }
                    else
                    {
                        onPanelSelecting.Invoke();
                    }
                }
                // Snap/Select after Pressing Button/Pagination Toggle
                else
                {
                    if (selected)
                    {
                        SnapToTargetPanel();
                    }
                    else
                    {
                        GoToPanel(currentPanel);
                    }
                }
            }
        }
        private void OnInfiniteScrolling()
        {
            if (infinitelyScroll)
            {
                if (movementAxis == MovementAxis.Horizontal)
                {
                    for (int i = 0; i < numberOfPanels; i++)
                    {
                        if (DisplacementFromCenter(panels[i].transform.position).x > (contentSize.x) / 2f)
                        {
                            panels[i].transform.position += (contentSize.x) * Vector3.left;
                        }
                        else if (DisplacementFromCenter(panels[i].transform.position).x < -1f * (contentSize.x) / 2f)
                        {
                            panels[i].transform.position += (contentSize.x) * Vector3.right;
                        }
                    }
                }
                else if (movementAxis == MovementAxis.Vertical)
                {
                    for (int i = 0; i < numberOfPanels; i++)
                    {
                        if (DisplacementFromCenter(panels[i].transform.position).y > (contentSize.y) / 2f)
                        {
                            panels[i].transform.position += (contentSize.y) * Vector3.down;
                        }
                        else if (DisplacementFromCenter(panels[i].transform.position).y < -1f * (contentSize.y) / 2f)
                        {
                            panels[i].transform.position += (contentSize.y) * Vector3.up;
                        }
                    }
                }
            }
        }
        private void OnTransitionEffects()
        {
            foreach (GameObject panel in panels)
            {
                foreach (TransitionEffect transitionEffect in transitionEffects)
                {
                    // Displacement
                    float displacement = 0f;
                    if (movementType == MovementType.Fixed)
                    {
                        if (movementAxis == MovementAxis.Horizontal)
                        {
                            displacement = DisplacementFromCenter(panel.transform.position).x;
                        }
                        else
                        {
                            displacement = DisplacementFromCenter(panel.transform.position).y;
                        }
                    }
                    else
                    {
                        displacement = DisplacementFromCenter(panel.transform.position).magnitude;
                    }

                    // Value
                    switch (transitionEffect.Label)
                    {
                        case "localPosition.z":
                            panel.transform.localPosition = new Vector3(panel.transform.localPosition.x, panel.transform.localPosition.y, transitionEffect.GetValue(displacement));
                            break;
                        case "localScale.x":
                            panel.transform.localScale = new Vector2(transitionEffect.GetValue(displacement), panel.transform.localScale.y);
                            break;
                        case "localScale.y":
                            panel.transform.localScale = new Vector2(panel.transform.localScale.x, transitionEffect.GetValue(displacement));
                            break;
                        case "localRotation.x":
                            panel.transform.localRotation = Quaternion.Euler(new Vector3(transitionEffect.GetValue(displacement), panel.transform.localEulerAngles.y, panel.transform.localEulerAngles.z));
                            break;
                        case "localRotation.y":
                            panel.transform.localRotation = Quaternion.Euler(new Vector3(panel.transform.localEulerAngles.x, transitionEffect.GetValue(displacement), panel.transform.localEulerAngles.z));
                            break;
                        case "localRotation.z":
                            panel.transform.localRotation = Quaternion.Euler(new Vector3(panel.transform.localEulerAngles.x, panel.transform.localEulerAngles.y, transitionEffect.GetValue(displacement)));
                            break;
                        case "color.r":
                            panel.GetComponent<Image>().color = new Color(transitionEffect.GetValue(displacement), panel.GetComponent<Image>().color.g, panel.GetComponent<Image>().color.b, panel.GetComponent<Image>().color.a);
                            break;
                        case "color.g":
                            panel.GetComponent<Image>().color = new Color(panel.GetComponent<Image>().color.r, transitionEffect.GetValue(displacement), panel.GetComponent<Image>().color.b, panel.GetComponent<Image>().color.a);
                            break;
                        case "color.b":
                            panel.GetComponent<Image>().color = new Color(panel.GetComponent<Image>().color.r, panel.GetComponent<Image>().color.g, transitionEffect.GetValue(displacement), panel.GetComponent<Image>().color.a);
                            break;
                        case "color.a":
                            panel.GetComponent<Image>().color = new Color(panel.GetComponent<Image>().color.r, panel.GetComponent<Image>().color.g, panel.GetComponent<Image>().color.b, transitionEffect.GetValue(displacement));
                            break;
                    }
                }
            }
        }
        private void OnSwipeGestures()
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();

            if (!swipeGestures)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = false;
                }
                else
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.blocksRaycasts = false;
                }
            }
            else if (canvasGroup != null && !canvasGroup.blocksRaycasts)
            {     
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void GoToPanel(int panelNumber)
        {
            targetPanel = panelNumber;
            selected = true;
            onPanelSelected.Invoke();

            if (pagination != null)
            {
                for (int i = 0; i < toggles.Length; i++)
                {
                    toggles[i].isOn = (i == targetPanel);
                    toggles[i].interactable = (i != targetPanel);
                }
            }

            if (hardSnap)
            {
                ScrollRect.inertia = false;
            }
        }
        public void GoToPreviousPanel()
        {
            nearestPanel = DetermineNearestPanel();
            if (nearestPanel != 0)
            {
                GoToPanel(nearestPanel - 1);
            }
            else
            {
                if (infinitelyScroll)
                {
                    GoToPanel(numberOfPanels - 1);
                }
                else
                {
                    GoToPanel(nearestPanel);
                }
            }
        }
        public void GoToNextPanel()
        {
            nearestPanel = DetermineNearestPanel();
            if (nearestPanel != (numberOfPanels - 1))
            {
                GoToPanel(nearestPanel + 1);
            }
            else
            {
                if (infinitelyScroll)
                {
                    GoToPanel(0);
                }
                else
                {
                    GoToPanel(nearestPanel);
                }
            }
        }
        
        public void AddVelocity(Vector2 velocity)
        {
            ScrollRect.velocity += velocity;
            selected = false;
        }
        #endregion
    }
}
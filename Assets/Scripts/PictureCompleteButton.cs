using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PictureCompleteButton : MonoBehaviour
{
    [Header("Picture Parts")]
    [SerializeField] private GameObject[] pictureParts; // 4 pieces

    [Header("Button")]
    [SerializeField] private GameObject buttonVisual; // the button object
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Input")]
    [SerializeField] private InputActionProperty uiPressAction;

    [Header("Door Tags")]
    [SerializeField] private DoorTag[] doorTags;

    private bool isHovered;
    private bool prevPressed;
    private bool isActive;

    private void Awake()
    {
        if (!interactable)
            interactable = GetComponent<XRSimpleInteractable>();

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(_ => isHovered = true);
            interactable.hoverExited.AddListener(_ => isHovered = false);
        }

        // start hidden
        if (buttonVisual != null)
            buttonVisual.SetActive(false);
    }

    private void OnEnable()
    {
        if (uiPressAction.action != null)
            uiPressAction.action.Enable();
    }

    private void OnDisable()
    {
        if (uiPressAction.action != null)
            uiPressAction.action.Disable();
    }

    private void Update()
    {
        CheckIfComplete();

        if (!isActive) return;

        bool pressed = uiPressAction.action != null && uiPressAction.action.IsPressed();
        bool down = pressed && !prevPressed;
        prevPressed = pressed;

        if (down && isHovered)
            OnButtonPressed();
    }

    private void CheckIfComplete()
    {
        if (isActive) return;

        foreach (var part in pictureParts)
        {
            if (part == null || !part.activeSelf)
                return;
        }

        // all active → enable button
        isActive = true;

        if (buttonVisual != null)
            buttonVisual.SetActive(true);
    }

    private void OnButtonPressed()
    {
        ResetPictureAndDoorTags();
    }

    public bool CanResetPicture()
    {
        int activeCount = 0;

        foreach (var part in pictureParts)
        {
            if (part != null && part.activeSelf)
                activeCount++;
        }

        bool noneEnabled = activeCount == 0;
        bool allEnabled = activeCount == pictureParts.Length;

        return noneEnabled || allEnabled;
    }

    public void ResetPictureAndDoorTags()
    {
        foreach (var part in pictureParts)
        {
            if (part != null)
                part.SetActive(false);
        }

        if (doorTags != null)
        {
            foreach (var tag in doorTags)
            {
                if (tag != null)
                    tag.SetIncomplete();
            }
        }

        if (buttonVisual != null)
            buttonVisual.SetActive(false);

        isActive = false;
    }
}
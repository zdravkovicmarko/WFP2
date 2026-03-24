using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(RoomTeleporter))]
public class DoorUIPressInteractable : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Assign: XRI RightHand Interaction / UI Press")]
    [SerializeField] private InputActionProperty uiPressAction;

    [Header("Teleport")]
    [SerializeField] private bool useDefaultDelay = true;
    [SerializeField] private float customDelaySeconds = 0f;
    [SerializeField] private MinigameRoom roomToMarkComplete = MinigameRoom.None;

    private XRSimpleInteractable interactable;
    private RoomTeleporter teleporter;

    private bool isHovered;
    private bool prevPressed;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        teleporter = GetComponent<RoomTeleporter>();

        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
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

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void Update()
    {
        bool pressed = uiPressAction.action != null && uiPressAction.action.IsPressed();
        bool down = pressed && !prevPressed;
        prevPressed = pressed;

        if (!isHovered || !down || teleporter == null)
            return;

        Debug.Log($"[DoorUIPress] Triggered on {name}");

        if (useDefaultDelay)
            teleporter.TeleportWithDefaultDelay(roomToMarkComplete);
        else
            teleporter.TeleportWithDelay(customDelaySeconds, roomToMarkComplete);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(RoomTeleporter))]
public class DoorUIPressInteractable : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty leftUiPressAction;
    [SerializeField] private InputActionProperty rightUiPressAction;

    [Header("Controller Roots")]
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    [Header("Teleport")]
    [SerializeField] private bool useDefaultDelay = true;
    [SerializeField] private float customDelaySeconds = 0f;
    [SerializeField] private MinigameRoom roomToMarkComplete = MinigameRoom.None;

    private XRSimpleInteractable interactable;
    private RoomTeleporter teleporter;

    private bool leftHovered;
    private bool rightHovered;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        teleporter = GetComponent<RoomTeleporter>();

        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void OnEnable()
    {
        leftUiPressAction.action?.Enable();
        rightUiPressAction.action?.Enable();
    }

    private void OnDisable()
    {
        leftUiPressAction.action?.Disable();
        rightUiPressAction.action?.Disable();
    }

    private void Update()
    {
        bool leftPressed = leftUiPressAction.action != null &&
                           leftUiPressAction.action.WasPressedThisFrame();

        bool rightPressed = rightUiPressAction.action != null &&
                            rightUiPressAction.action.WasPressedThisFrame();

        if (leftHovered && leftPressed)
            TriggerTeleport("Left");

        if (rightHovered && rightPressed)
            TriggerTeleport("Right");
    }

    private void TriggerTeleport(string hand)
    {
        Debug.Log($"[DoorUIPress] {hand} triggered {name}");

        if (useDefaultDelay)
            teleporter.TeleportWithDefaultDelay(roomToMarkComplete);
        else
            teleporter.TeleportWithDelay(customDelaySeconds, roomToMarkComplete);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        Transform interactorTransform = args.interactorObject.transform;

        if (leftController != null && interactorTransform.IsChildOf(leftController))
            leftHovered = true;

        if (rightController != null && interactorTransform.IsChildOf(rightController))
            rightHovered = true;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        Transform interactorTransform = args.interactorObject.transform;

        if (leftController != null && interactorTransform.IsChildOf(leftController))
            leftHovered = false;

        if (rightController != null && interactorTransform.IsChildOf(rightController))
            rightHovered = false;
    }
}
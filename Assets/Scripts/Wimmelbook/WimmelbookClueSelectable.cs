using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class WimmelbookClueSelectable : MonoBehaviour
{
    [Header("Visual (child with SpriteRenderer)")]
    [SerializeField] private GameObject visual;

    [Header("XR")]
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Input")]
    [SerializeField] private InputActionProperty leftUiPressAction;
    [SerializeField] private InputActionProperty rightUiPressAction;

    [Header("Controller Roots")]
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    private WimmelbookManager manager;
    private string id;
    private bool isFound;

    private bool leftHovered;
    private bool rightHovered;

    private void Awake()
    {
        manager = GetComponentInParent<WimmelbookManager>();

        const string prefix = "clue_found_";
        id = gameObject.name.StartsWith(prefix)
            ? gameObject.name.Substring(prefix.Length)
            : gameObject.name;

        if (visual == null && transform.childCount > 0)
            visual = transform.GetChild(0).gameObject;

        if (!interactable)
            interactable = GetComponent<XRSimpleInteractable>();

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }

        ResetClue();
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
        bool leftPressed =
            leftUiPressAction.action != null &&
            leftUiPressAction.action.WasPressedThisFrame();

        bool rightPressed =
            rightUiPressAction.action != null &&
            rightUiPressAction.action.WasPressedThisFrame();

        if (leftHovered && leftPressed)
            OnSelected();

        if (rightHovered && rightPressed)
            OnSelected();
    }

    public void OnSelected()
    {
        if (isFound) return;
        if (manager == null) return;

        isFound = true;
        SetVisual(true);
        manager.OnClueFound(id);
    }

    public void ResetClue()
    {
        isFound = false;
        leftHovered = false;
        rightHovered = false;
        SetVisual(false);
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

    private void SetVisual(bool value)
    {
        if (visual != null)
            visual.SetActive(value);
    }
}
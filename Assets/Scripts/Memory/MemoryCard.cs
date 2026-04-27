using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MemoryCard : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Input")]
    [SerializeField] private InputActionProperty leftUiPressAction;
    [SerializeField] private InputActionProperty rightUiPressAction;

    [Header("Controller Roots")]
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform rightController;

    public int PairId { get; private set; }

    public bool IsRevealed { get; private set; }
    public bool IsMatched  { get; private set; }

    private float baseX;
    private float baseZ;
    private bool isInitialized;
    private MemoryBoard board;

    private bool leftHovered;
    private bool rightHovered;

    private void Awake()
    {
        if (!interactable)
            interactable = GetComponent<XRSimpleInteractable>();

        board = FindObjectOfType<MemoryBoard>();

        string[] parts = gameObject.name.Split('_');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int id))
            PairId = id;

        if (interactable != null)
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }
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

    public void Initialize(Vector3 localPosition)
    {
        baseX = localPosition.x;
        baseZ = localPosition.z;

        IsMatched = false;
        IsRevealed = false;
        isInitialized = true;

        leftHovered = false;
        rightHovered = false;

        Hide();

        if (interactable)
            interactable.enabled = true;
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
        if (!isInitialized || IsMatched || board == null)
            return;

        board.HandleCardSelected(this);
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

    public void Reveal()
    {
        IsRevealed = true;

        transform.localPosition = new Vector3(baseX, 0.0f, baseZ);
        transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
    }

    public void Hide()
    {
        IsRevealed = false;

        transform.localPosition = new Vector3(baseX, 0.05f, baseZ);
        transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsRevealed = true;

        transform.localPosition = new Vector3(baseX, 0.0f, baseZ);
        transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);

        if (interactable)
            interactable.enabled = false;
    }
}

using UnityEngine;


public class MemoryCard : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [Header("Input")]
    [SerializeField] private UnityEngine.InputSystem.InputActionProperty uiPressAction;

    public int PairId { get; private set; }

    public bool IsRevealed { get; private set; }
    public bool IsMatched  { get; private set; }

    private float baseX;
    private float baseZ;
    private bool  isInitialized;
    private MemoryBoard board;

    private bool isHovered = false;
    private bool prevPressed = false;

    private void Awake()
    {
        if (!interactable)
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        board = FindObjectOfType<MemoryBoard>();

        if (PairId == 0)
        {
            string[] parts = gameObject.name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int id))
                PairId = id;
        }

        interactable.hoverEntered.AddListener(_ => isHovered = true);
        interactable.hoverExited.AddListener(_ => isHovered = false);
    }

    public void Initialize(Vector3 localPosition)
    {
        baseX = localPosition.x;
        baseZ = localPosition.z;

        IsMatched = false;
        IsRevealed = false;
        isInitialized = true;

        Hide();
        if (interactable) interactable.enabled = true;
    }

    private void Update()
    {
        bool pressed = uiPressAction.action != null && uiPressAction.action.IsPressed();
        bool down = pressed && !prevPressed;
        prevPressed = pressed;

        if (!down || !isHovered)
            return;

        OnSelected();
    }

    public void OnSelected()
    {
        if (!isInitialized || IsMatched || board == null)
            return;

        board.HandleCardSelected(this);
    }

    public void Reveal()
    {
        IsRevealed = true;

        transform.localPosition = new Vector3(baseX, 0.0f,  baseZ);
        transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
    }

    public void Hide()
    {
        IsRevealed = false;

        transform.localPosition = new Vector3(baseX, 0.05f, baseZ);
        transform.localRotation = Quaternion.Euler( 90f, 180f, 0f);
    }

    public void SetMatched()
    {
        IsMatched  = true;
        IsRevealed = true;

        transform.localPosition = new Vector3(baseX, 0.0f, baseZ);
        transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);

        if (interactable) interactable.enabled = false;
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
}

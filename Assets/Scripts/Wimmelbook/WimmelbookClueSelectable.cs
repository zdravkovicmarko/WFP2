using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class WimmelbookClueSelectable : MonoBehaviour
{
    [Header("Visual (child with SpriteRenderer)")]
    [SerializeField] private GameObject visual;

    [Header("XR")]
    [SerializeField] private XRSimpleInteractable interactable;

    [Header("Input")]
    [SerializeField] private InputActionProperty uiPressAction;

    private bool isHovered;
    private bool prevPressed;

    private WimmelbookManager manager;
    private string id;
    private bool isFound;

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
            interactable.hoverEntered.AddListener(_ => isHovered = true);
            interactable.hoverExited.AddListener(_ => isHovered = false);
        }
        ResetClue();
    }

    public void OnSelected()
    {
        if (isFound) return;
        if (manager == null) return;

        isFound = true;
        SetVisual(true);

        // tell manager (counts progress + disables icon + checks win)
        manager.OnClueFound(id);
    }

    public void ResetClue()
    {
        isFound = false;
        SetVisual(false);
    }

    private void SetVisual(bool value)
    {
        if (visual != null)
            visual.SetActive(value);
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
        bool pressed = uiPressAction.action != null && uiPressAction.action.IsPressed();
        bool down = pressed && !prevPressed;
        prevPressed = pressed;

        if (!down || !isHovered)
            return;

        OnSelected();
    }
}

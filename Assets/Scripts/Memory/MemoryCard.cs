using UnityEngine;


public class MemoryCard : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    public int PairId { get; private set; }

    public bool IsRevealed { get; private set; }
    public bool IsMatched  { get; private set; }

    private float baseX;
    private float baseZ;
    private bool  isInitialized;
    private MemoryBoard board;

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
}

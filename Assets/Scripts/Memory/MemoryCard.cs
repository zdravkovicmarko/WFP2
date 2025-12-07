using UnityEngine;


public class MemoryCard : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    // Pair ID: same for both cards of a pair
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

        // Derive PairId from name: memory_card_3_1 → "3"
        // parts: ["memory", "card", "3", "1"]
        if (PairId == 0)
        {
            string[] parts = gameObject.name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int id))
                PairId = id;
        }
    }

    /// <summary>Called from MemoryBoard after shuffling.</summary>
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

    /// <summary>Hook this to XRSimpleInteractable → Select Entered.</summary>
    public void OnSelected()
    {
        if (!isInitialized || IsMatched || board == null)
            return;

        board.HandleCardSelected(this);
    }

    // ------------ visual states ------------

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

        // stay face-up
        transform.localPosition = new Vector3(baseX, 0.0f, baseZ);
        transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);

        // disable interaction so you can’t select matched cards again
        if (interactable) interactable.enabled = false;
    }
}

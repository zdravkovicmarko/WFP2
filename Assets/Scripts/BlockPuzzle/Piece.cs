using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class Piece : MonoBehaviour
{
    [Header("Board")]
    public BoardManager board;

    [Header("Pivots / Shape")]
    public Transform[] pivotPoints;

    public float snapDistance = 0.25f;

    public Vector3 snapOffset = Vector3.zero;

    [Header("XR")]
    public XRGrabInteractable grab;

    [Header("Game")]
    public BlockPuzzleGame game;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip wrongClip; 

    private Rigidbody rb;

    private bool isPlaced;
    private Vector2Int[] lastCells;

    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private Quaternion initRotRelativeToBoard;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!grab)
            grab = GetComponent<XRGrabInteractable>();

        spawnPos = transform.position;
        spawnRot = transform.rotation;

        rb.useGravity = true;

        if (audioSource == null)
        audioSource = GetComponent<AudioSource>();

        if (board == null)
        {
            Debug.LogError($"{name}: Board reference missing!");
        }
        else
        {
            initRotRelativeToBoard = Quaternion.Inverse(board.transform.rotation) * transform.rotation;
        }

        grab.selectEntered.AddListener(_ => OnGrab());
        grab.selectExited.AddListener(_ => OnRelease());
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveAllListeners();
            grab.selectExited.RemoveAllListeners();
        }
    }

    void OnGrab()
    {
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        if (isPlaced && lastCells != null && board != null)
        {
            Debug.Log($"[DEBUG] {name}: Removing occupancy for previous placement.");
            board.SetOccupiedCells(lastCells, false);
            isPlaced = false;
        }
    }

    void OnRelease()
    {
        TrySnap();
    }

    void TrySnap()
    {
        if (board == null || pivotPoints == null || pivotPoints.Length == 0)
        {
            ResetToSpawn();
            return;
        }

        //Debug.Log($"[DEBUG] ----- TRY SNAP for {name} -----");

        // 1) Map pivots to tiles
        if (!TryGetAlignedCells(out var cells, out var tileWorlds))
        {
            //Debug.Log($"[DEBUG] {name}: alignment failed → ResetToSpawn()");
            PlayWrongSound();
            ResetToSpawn();
            return;
        }

        //Debug.Log($"[DEBUG] {name}: All cubes aligned & mapped to UNIQUE tiles → checking occupancy...");

        // 2) Check occupancy
        if (!board.CanPlaceCells(cells))
        {
            //Debug.Log($"[DEBUG] {name}: target cells occupied → ResetToSpawn()");
            PlayWrongSound();
            ResetToSpawn();
            return;
        }

        // 3) Occupy cells
        lastCells = cells;
        board.SetOccupiedCells(cells, true);
        isPlaced = true;

        board.OnPiecePlaced();

        // 4) Final rotation: "flat" relative to board, like your prefab
        Quaternion finalRot = board.transform.rotation * initRotRelativeToBoard;

        // 5) Anchor = pivotPoints[0]
        Transform anchorPivot = pivotPoints[0];
        Vector3 localAnchor = transform.InverseTransformPoint(anchorPivot.position);
        Vector3 targetAnchorWorld = tileWorlds[0];

        // piecePos = tilePos0 - finalRot * localAnchor + finalRot * snapOffset
        Vector3 finalPos = targetAnchorWorld - finalRot * localAnchor + finalRot * snapOffset; // <-- offset added

        // 6) Apply transform / freeze physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.SetPositionAndRotation(finalPos, finalRot);

        // 7) Debug: post-snap error in XZ plane
        for (int i = 0; i < pivotPoints.Length; i++)
        {
            Transform p = pivotPoints[i];
            Vector3 w = p.position;
            Vector3 tw = tileWorlds[i];

            float err = Vector2.Distance(
                new Vector2(w.x, w.z),
                new Vector2(tw.x, tw.z)
            );
            //Debug.Log($"[DEBUG] {name}: post-snap error for cube '{p.name}' = {err:F4}");
        }

        //Debug.Log($"[DEBUG] {name}: SNAP COMPLETE at pivot cell ({cells[0].x}, {cells[0].y}).");

        PlayCorrectSound();

        if (game != null)
        {
            game.OnPieceSnapped(this);
        }
    }

    void ResetToSpawn()
    {
        Debug.Log($"[DEBUG] {name}: ResetToSpawn()");
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        transform.SetPositionAndRotation(spawnPos, spawnRot);
        isPlaced = false;
    }

    bool TryGetAlignedCells(out Vector2Int[] cells, out Vector3[] tileWorlds)
    {
        cells = null;
        tileWorlds = null;

        if (pivotPoints == null || pivotPoints.Length == 0 || board == null)
            return false;

        int n = pivotPoints.Length;
        cells = new Vector2Int[n];
        tileWorlds = new Vector3[n];

        var used = new System.Collections.Generic.HashSet<Vector2Int>();

        //Debug.Log($"[DEBUG] {name}: Checking {n} cube points...");

        for (int i = 0; i < n; i++)
        {
            Transform p = pivotPoints[i];
            if (p == null)
            {
                Debug.LogWarning($"{name}: pivotPoints[{i}] is null");
                return false;
            }

            Vector3 worldPos = p.position;

            if (!board.TryGetNearestCell(worldPos, out Vector2Int cell))
            {
                Debug.Log($"[DEBUG] {name}: pivot '{p.name}' has no nearest cell → fail");
                return false;
            }

            Vector3 tileWorld = board.GetTilePosition(cell);

            float planarDist = Vector2.Distance(
                new Vector2(worldPos.x, worldPos.z),
                new Vector2(tileWorld.x, tileWorld.z)
            );

            //Debug.Log($"[DEBUG] {name}: Cube '{p.name}' world {worldPos} → " + $"tile (x={cell.x}, y={cell.y}) at {tileWorld}, planarDist={planarDist:F3}");

            if (planarDist > snapDistance)
            {
                Debug.Log($"[DEBUG] {name}: cube '{p.name}' too far → fail");
                return false;
            }

            if (used.Contains(cell))
            {
                Debug.Log($"[DEBUG] {name}: duplicate cell ({cell.x}, {cell.y}) → fail");
                return false;
            }

            used.Add(cell);
            cells[i] = cell;
            tileWorlds[i] = tileWorld;
        }

        return true;
    }

    public void ForceResetToSpawn()
    {
        ResetToSpawn();
    }

    void PlayCorrectSound()
    {
        if (audioSource != null && correctClip != null)
            audioSource.PlayOneShot(correctClip);
    }

    void PlayWrongSound()
    {
        if (audioSource != null && wrongClip != null)
            audioSource.PlayOneShot(wrongClip);
    }
}
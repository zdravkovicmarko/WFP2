using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System;

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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip wrongClip; 

    public float autoResetDistanceFromSpawn = 0.75f;
    public bool enableAutoReset = true;

    [Header("Debug")]
    public bool debugPlacement = false;

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

        if (debugPlacement)
        {
            D($"SpawnPos={spawnPos}, SpawnRot={spawnRot.eulerAngles}");
            for (int i = 0; i < pivotPoints.Length; i++)
            {
                if (pivotPoints[i] == null)
                {
                    D($"pivot[{i}] = NULL");
                    continue;
                }

                Vector3 local = transform.InverseTransformPoint(pivotPoints[i].position);
                D($"pivot[{i}] '{pivotPoints[i].name}' local={local} world={pivotPoints[i].position}");
            }
        }

        grab.selectEntered.AddListener(_ => OnGrab());
        grab.selectExited.AddListener(_ => OnRelease());
    }

    void Update()
    {
        if (!enableAutoReset)
            return;

        if (isPlaced)
            return;

        if (grab != null && grab.isSelected)
            return;

        float distFromSpawn = Vector3.Distance(transform.position, spawnPos);
        if (distFromSpawn > autoResetDistanceFromSpawn)
        {
            PlayWrongSound();
            ResetToSpawn();
        }
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
            D($"Removing previous occupancy: {string.Join(", ", Array.ConvertAll(lastCells, c => $"({c.x},{c.y})"))}");
            board.SetOccupiedCells(lastCells, false);
            isPlaced = false;
        }
    }

    void OnRelease()
    {
        D("OnRelease() -> TrySnap()");
        SnapRotationToBoard();
        TrySnap();
    }

    void TrySnap()
    {
        if (board == null || pivotPoints == null || pivotPoints.Length == 0)
        {
            D("TrySnap failed: board or pivots missing.");
            PlayWrongSound();
            ResetToSpawn();
            return;
        }

        D("----- TRY SNAP START -----");

        if (!TryGetAlignedCells(out var cells, out var tileWorlds))
        {
            D("TryGetAlignedCells failed -> ResetToSpawn()");
            PlayWrongSound();
            ResetToSpawn();
            return;
        }

        D($"Mapped cells: {string.Join(", ", Array.ConvertAll(cells, c => $"({c.x},{c.y})"))}");

        if (!board.CanPlaceCells(cells))
        {
            D("CanPlaceCells returned FALSE -> ResetToSpawn()");
            PlayWrongSound();
            ResetToSpawn();
            return;
        }

        lastCells = cells;
        board.SetOccupiedCells(cells, true);
        isPlaced = true;

        board.OnPiecePlaced();

        Quaternion finalRot = transform.rotation;

        Transform anchorPivot = pivotPoints[0];
        Vector3 localAnchor = transform.InverseTransformPoint(anchorPivot.position);
        Vector3 targetAnchorWorld = tileWorlds[0];

        Vector3 finalPos = targetAnchorWorld - finalRot * localAnchor + finalRot * snapOffset;

        D($"Anchor pivot='{anchorPivot.name}' localAnchor={localAnchor}");
        D($"targetAnchorWorld={targetAnchorWorld}, finalRot={finalRot.eulerAngles}, finalPos={finalPos}");

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.SetPositionAndRotation(finalPos, finalRot);

        for (int i = 0; i < pivotPoints.Length; i++)
        {
            Transform p = pivotPoints[i];
            Vector3 w = p.position;
            Vector3 tw = tileWorlds[i];

            float err = Vector2.Distance(
                new Vector2(w.x, w.z),
                new Vector2(tw.x, tw.z)
            );

            D($"POST SNAP pivot '{p.name}' -> world={w}, targetTile={tw}, planarErr={err:F4}");
        }

        D("SNAP COMPLETE");
        PlayCorrectSound();
    }

    void ResetToSpawn()
    {
        D("ResetToSpawn()");
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
        {
            D("TryGetAlignedCells aborted: pivots or board missing.");
            return false;
        }

        int n = pivotPoints.Length;
        cells = new Vector2Int[n];
        tileWorlds = new Vector3[n];

        var used = new System.Collections.Generic.HashSet<Vector2Int>();

        D($"Checking {n} pivots...");

        for (int i = 0; i < n; i++)
        {
            Transform p = pivotPoints[i];
            if (p == null)
            {
                D($"pivot[{i}] is null -> fail");
                return false;
            }

            Vector3 worldPos = p.position;
            Vector3 localPos = transform.InverseTransformPoint(worldPos);

            if (!board.TryGetNearestCell(worldPos, out Vector2Int cell))
            {
                D($"pivot '{p.name}' world={worldPos} local={localPos} -> no nearest cell -> fail");
                return false;
            }

            Vector3 tileWorld = board.GetTilePosition(cell);

            float planarDist = Vector2.Distance(
                new Vector2(worldPos.x, worldPos.z),
                new Vector2(tileWorld.x, tileWorld.z)
            );

            D($"pivot '{p.name}' local={localPos} world={worldPos} -> cell=({cell.x},{cell.y}) tileWorld={tileWorld} planarDist={planarDist:F4}");

            if (planarDist > snapDistance)
            {
                D($"pivot '{p.name}' too far from nearest tile ({planarDist:F4} > {snapDistance}) -> fail");
                return false;
            }

            if (used.Contains(cell))
            {
                D($"pivot '{p.name}' maps to duplicate cell ({cell.x},{cell.y}) -> fail");
                return false;
            }

            used.Add(cell);
            cells[i] = cell;
            tileWorlds[i] = tileWorld;
        }

        D("All pivots mapped successfully.");
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

    void SnapRotationToBoard()
    {
        if (board == null)
            return;

        // Board basis
        Vector3 boardUp = board.transform.up;
        Vector3 boardForward = Vector3.ProjectOnPlane(board.transform.forward, boardUp).normalized;
        Vector3 boardRight = Vector3.ProjectOnPlane(board.transform.right, boardUp).normalized;

        if (boardForward.sqrMagnitude < 0.0001f || boardRight.sqrMagnitude < 0.0001f)
            return;

        // Take the piece's current forward and flatten it onto the board plane
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, boardUp);

        // Fallback if piece is pointing almost straight up/down
        if (flatForward.sqrMagnitude < 0.0001f)
            flatForward = boardForward;

        flatForward.Normalize();

        // Find signed angle relative to board forward
        float signedAngle = Vector3.SignedAngle(boardForward, flatForward, boardUp);

        // Snap to nearest 90°
        float snappedAngle = Mathf.Round(signedAngle / 90f) * 90f;

        // Build final flat board-aligned rotation
        Quaternion finalRot = Quaternion.AngleAxis(snappedAngle, boardUp) * Quaternion.LookRotation(boardForward, boardUp);

        transform.rotation = finalRot;

        D($"SnapRotationToBoard() -> signedAngle={signedAngle:F2}, snappedAngle={snappedAngle:F2}, finalRot={finalRot.eulerAngles}");
    }

    void D(string msg)
    {
        if (debugPlacement)
            Debug.Log($"[BLOCK DEBUG] {name}: {msg}");
    }
}
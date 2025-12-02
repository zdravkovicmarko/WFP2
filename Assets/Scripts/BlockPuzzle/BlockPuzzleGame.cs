using UnityEngine;

public class BlockPuzzleGame : MonoBehaviour
{
    [Header("Refs")]
    public BoardManager board;
    public Piece[] pieces;

    void Start()
    {
        // 1) Clear tile occupancy
        if (board != null)
            board.ClearOccupancy();

        // 2) Reset all pieces to their spawn pose
        if (pieces != null)
        {
            foreach (var p in pieces)
            {
                if (p == null) continue;
                p.ForceResetToSpawn();
            }
        }

    }

    public void OnPieceSnapped(Piece piece)
    {
        if (board == null) return;

        int used = board.CountOccupiedPlayableTiles();
        Debug.Log($"[GAME] After piece '{piece.name}' snapped: {used} tiles occupied.");

        if (board.AreAllPlayableTilesOccupied())
        {
            Debug.Log("[GAME] All tiles occupied – PUZZLE COMPLETE!");
        }
    }
}

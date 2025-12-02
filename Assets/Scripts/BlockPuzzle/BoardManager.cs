using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Size")]
    public int columns = 5;
    public int rows    = 5;

    [Header("Tiles (assign all Tile_ x_y objects)")]
    public SnapTile[] tiles;

    private SnapTile[,] grid;
    private bool[,] occupied;

    // NEW: world-space 2D bounds of the board (on X/Y plane)
    [HideInInspector] public Vector2 boardMin;
    [HideInInspector] public Vector2 boardMax;

    void Awake()
    {
        grid = new SnapTile[columns, rows];
        occupied = new bool[columns, rows];

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var t in tiles)
        {
            if (t == null) continue;

            if (t.x < 0 || t.x >= columns || t.y < 0 || t.y >= rows)
            {
                Debug.LogWarning($"SnapTile {t.name} index ({t.x},{t.y}) out of range.");
                continue;
            }

            grid[t.x, t.y] = t;

            Debug.Log($"[BOARD] Tile ({t.x},{t.y}) '{t.name}' at world {t.transform.position}");

            // Track board extents based on tile centers
            Vector3 p = t.transform.position;
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        // Approximate cell size from any two tiles in a row/column
        float spacing = 0f;
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = i + 1; j < tiles.Length; j++)
            {
                var a = tiles[i];
                var b = tiles[j];
                if (a == null || b == null) continue;

                // same row -> horizontal spacing
                if (a.y == b.y && a.x != b.x)
                {
                    spacing = Mathf.Abs(a.transform.position.x - b.transform.position.x);
                    break;
                }
                // same column -> vertical spacing
                if (a.x == b.x && a.y != b.y)
                {
                    spacing = Mathf.Abs(a.transform.position.y - b.transform.position.y);
                    break;
                }
            }
            if (spacing > 0f) break;
        }

        float halfCell = (spacing > 0f ? spacing * 0.5f : 0.25f);

        // Board bounds = outermost tile centers ± half a cell
        boardMin = new Vector2(minX - halfCell, minY - halfCell);
        boardMax = new Vector2(maxX + halfCell, maxY + halfCell);
    }

    public bool InBounds(Vector2Int c) =>
        c.x >= 0 && c.x < columns && c.y >= 0 && c.y < rows;

    public bool IsFree(Vector2Int c) =>
        InBounds(c) && grid[c.x, c.y] != null && !occupied[c.x, c.y];

    public bool CanPlace(Vector2Int pivot, Vector2Int[] offsets)
    {
        foreach (var o in offsets)
        {
            var c = pivot + o;
            if (!IsFree(c))
                return false;
        }
        return true;
    }

    public void SetOccupied(Vector2Int pivot, Vector2Int[] offsets, bool value)
    {
        foreach (var o in offsets)
        {
            var c = pivot + o;
            if (InBounds(c))
                occupied[c.x, c.y] = value;
        }
    }

    public Vector3 GetTilePosition(Vector2Int cell)
    {
        var tile = grid[cell.x, cell.y];
        return tile != null ? tile.transform.position : Vector3.zero;
    }

    public bool TryGetNearestCell(Vector3 pivotWorld, out Vector2Int cell)
    {
        float best = float.MaxValue;
        SnapTile bestTile = null;

        foreach (var t in tiles)
        {
            if (t == null) continue;
            float d = (t.transform.position - pivotWorld).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestTile = t;
            }
        }

        if (bestTile == null)
        {
            cell = default;
            return false;
        }

        cell = new Vector2Int(bestTile.x, bestTile.y);
        return true;
    }

    public bool CanPlaceCells(Vector2Int[] cells)
    {
        foreach (var c in cells)
        {
            if (!InBounds(c))
                return false;

            if (occupied[c.x, c.y])
                return false;
        }
        return true;
    }

    public void SetOccupiedCells(Vector2Int[] cells, bool value)
    {
        foreach (var c in cells)
        {
            if (InBounds(c))
                occupied[c.x, c.y] = value;
        }
    }

    // OPTIONAL helper if you want:
    public bool IsInsideBoardBounds(Vector2 pos)
    {
        return pos.x >= boardMin.x && pos.x <= boardMax.x &&
               pos.y >= boardMin.y && pos.y <= boardMax.y;
    }
}
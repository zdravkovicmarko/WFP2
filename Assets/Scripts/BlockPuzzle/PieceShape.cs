using UnityEngine;

[CreateAssetMenu(menuName = "BlockPuzzle/PieceShape")]
public class PieceShape : ScriptableObject
{
    [Tooltip("Offsets around pivot in grid coordinates. Include (0,0).")]
    public Vector2Int[] cells;
}

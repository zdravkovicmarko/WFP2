using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PinEndpoint : MonoBehaviour
{
    public int pairId = 1;

    [Header("Grid coord of this pin")]
    public int x;
    public int y;
    public string chainColorHex = "#FFFFFF";

    public Vector2Int Coord => new Vector2Int(x, y);

    public Color ChainColor
    {
        get
        {
            if (ColorUtility.TryParseHtmlString(chainColorHex, out var c))
                return c;

            Debug.LogWarning($"[PinEndpoint] Invalid HEX color on {name}: {chainColorHex}");
            return Color.white;
        }
    }

    public Renderer cachedRenderer { get; private set; }

    private void Awake()
    {
        cachedRenderer = GetComponentInChildren<Renderer>();
        if (cachedRenderer == null)
            cachedRenderer = GetComponent<Renderer>();
    }
}

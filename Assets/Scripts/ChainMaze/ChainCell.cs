using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class ChainCellInteractable : MonoBehaviour
{
    public int x;
    public int y;

    public Vector2Int Coord => new Vector2Int(x, y);

    private XRSimpleInteractable interactable;
    private ChainMazeManager manager;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        manager = FindFirstObjectByType<ChainMazeManager>();

        interactable.hoverEntered.AddListener(OnHoverEntered);
    }

    public Vector3 CenterWorld
    {
        get
        {
            var col = GetComponent<Collider>();
            return col != null ? col.bounds.center : transform.position;
        }
    }


    private void OnDestroy()
    {
        if (interactable == null) return;
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (manager == null) return;
        manager.OnCellHovered(Coord, CenterWorld, args.interactorObject);
    }
}
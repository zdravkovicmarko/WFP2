using UnityEngine;

public class WimmelbookClueSelectable : MonoBehaviour
{
    [Header("Visual (child with SpriteRenderer)")]
    [SerializeField] private GameObject visual;

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
}

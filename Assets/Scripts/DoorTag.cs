using UnityEngine;

public class DoorTag : MonoBehaviour
{
    [SerializeField] private Renderer tagRenderer;
    [SerializeField] private Material incompleteMaterial;
    [SerializeField] private Material completeMaterial;

    private bool isCompleted;

    private void Awake()
    {
        SetIncomplete();
    }

    public void SetComplete()
    {
        if (isCompleted) return;

        isCompleted = true;

        if (tagRenderer != null && completeMaterial != null)
            tagRenderer.material = completeMaterial;
    }

    public void SetIncomplete()
    {
        isCompleted = false;

        if (tagRenderer != null && incompleteMaterial != null)
            tagRenderer.material = incompleteMaterial;
    }
}
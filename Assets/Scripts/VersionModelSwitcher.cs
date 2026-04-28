using UnityEngine;

public class VersionModelSwitcher : MonoBehaviour
{
    [Header("Normal Version Objects")]
    [SerializeField] private GameObject[] normalObjects;

    [Header("Minimal Version Objects")]
    [SerializeField] private GameObject[] minimalObjects;

    private void Awake()
    {
        ApplyVersion();
    }

    private void OnEnable()
    {
        ApplyVersion();
    }

    public void ApplyVersion()
    {
        bool useMinimal = GameVersionManager.CurrentVersion == GameVersion.Minimal;

        foreach (var obj in normalObjects)
        {
            if (obj != null)
                obj.SetActive(!useMinimal);
        }

        foreach (var obj in minimalObjects)
        {
            if (obj != null)
                obj.SetActive(useMinimal);
        }
    }
}
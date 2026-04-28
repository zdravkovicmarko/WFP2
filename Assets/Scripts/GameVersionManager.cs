using UnityEngine;

public enum GameVersion
{
    None,
    Story,
    Minimal,
    Unimodal,
    Multimodal
}

public class GameVersionManager : MonoBehaviour
{
    public static GameVersion CurrentVersion = GameVersion.None;

    [Header("Normal Environment")]
    [SerializeField] private GameObject normalHubEnvironment;
    [SerializeField] private GameObject normalMiniGameEnvironment;

    [Header("Minimal Environment")]
    [SerializeField] private GameObject minimalHubEnvironment;
    [SerializeField] private GameObject minimalMiniGameEnvironment;

    public void SetStoryVersion()
    {
        CurrentVersion = GameVersion.Story;
        Debug.Log("Selected: Story Version");
    }

    public void SetMinimalVersion()
    {
        CurrentVersion = GameVersion.Minimal;
        ApplyEnvironmentVersion();

        Debug.Log("Selected: Minimal Version");
    }

    public void SetUnimodalVersion()
    {
        CurrentVersion = GameVersion.Unimodal;
        Debug.Log("Selected: Unimodal Version");
    }

    public void SetMultimodalVersion()
    {
        CurrentVersion = GameVersion.Multimodal;
        Debug.Log("Selected: Multimodal");
    }

    private void ApplyEnvironmentVersion()
    {
        bool useMinimal = CurrentVersion == GameVersion.Minimal;

        if (normalHubEnvironment != null)
            normalHubEnvironment.SetActive(!useMinimal);

        if (normalMiniGameEnvironment != null)
            normalMiniGameEnvironment.SetActive(!useMinimal);

        if (minimalHubEnvironment != null)
            minimalHubEnvironment.SetActive(useMinimal);

        if (minimalMiniGameEnvironment != null)
            minimalMiniGameEnvironment.SetActive(useMinimal);
    }
}
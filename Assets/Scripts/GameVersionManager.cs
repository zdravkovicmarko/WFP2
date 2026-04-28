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

    [SerializeField] private StorySequenceManager storySequenceManager;

    public void SetStoryVersion()
    {
        CurrentVersion = GameVersion.Story;
        ApplyAllVersionSwitchers();

        Debug.Log("Selected: Story Version");

        if (storySequenceManager != null)
            storySequenceManager.PlayIntro();
    }

    public void SetMinimalVersion()
    {
        CurrentVersion = GameVersion.Minimal;
        ApplyEnvironmentVersion();
        ApplyAllVersionSwitchers();

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

    private void ApplyAllVersionSwitchers()
    {
        var switchers = FindObjectsByType<VersionModelSwitcher>(FindObjectsSortMode.None);

        foreach (var switcher in switchers)
            switcher.ApplyVersion();
    }
}
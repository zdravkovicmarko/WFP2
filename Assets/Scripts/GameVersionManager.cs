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

    [Header("Story")]
    [SerializeField] private StorySequenceManager storySequenceManager;

    [Header("Picture Reset")]
    [SerializeField] private PictureCompleteButton pictureCompleteButton;

    public void SetStoryVersion()
    {
        if (!TryResetBeforeVersionSwitch()) return;

        CurrentVersion = GameVersion.Story;

        ApplyEnvironmentVersion();
        ApplyAllVersionSwitchers();

        Debug.Log("Selected: Story Version");

        if (storySequenceManager != null)
            storySequenceManager.ResetStoryProgress();
            storySequenceManager.PlayIntro();
    }

    public void SetMinimalVersion()
    {
        if (!TryResetBeforeVersionSwitch()) return;

        CurrentVersion = GameVersion.Minimal;

        ApplyEnvironmentVersion();
        ApplyAllVersionSwitchers();

        Debug.Log("Selected: Minimal Version");
    }

    public void SetUnimodalVersion()
    {
        if (!TryResetBeforeVersionSwitch()) return;

        CurrentVersion = GameVersion.Unimodal;

        ApplyEnvironmentVersion();
        ApplyAllVersionSwitchers();

        Debug.Log("Selected: Unimodal Version");
    }

    public void SetMultimodalVersion()
    {
        if (!TryResetBeforeVersionSwitch()) return;

        CurrentVersion = GameVersion.Multimodal;

        ApplyEnvironmentVersion();
        ApplyAllVersionSwitchers();

        Debug.Log("Selected: Multimodal Version");
    }

    private bool TryResetBeforeVersionSwitch()
    {
        if (pictureCompleteButton == null)
            return true;

        if (!pictureCompleteButton.CanResetPicture())
        {
            Debug.LogWarning("[VersionManager] Cannot switch version while picture is partially completed.");
            return false;
        }

        pictureCompleteButton.ResetPictureAndDoorTags();
        return true;
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
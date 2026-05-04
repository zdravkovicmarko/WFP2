using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

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

    // Story is active in Story, Unimodal, and Multimodal
    public static bool StoryEnabled { get; private set; }

    // Feedback is disabled only in Unimodal
    public static bool FeedbackEnabled { get; private set; } = true;

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

    [Header("Global Feedback")]
    [SerializeField] private bool muteAudioInUnimodal = true;
    [SerializeField] private HapticImpulsePlayer[] hapticPlayersToToggle;

    public void SetStoryVersion()
    {
        SelectVersion(GameVersion.Story, playIntro: true);
    }

    public void SetMinimalVersion()
    {
        SelectVersion(GameVersion.Minimal, playIntro: false);
    }

    public void SetUnimodalVersion()
    {
        SelectVersion(GameVersion.Unimodal, playIntro: true);
    }

    public void SetMultimodalVersion()
    {
        SelectVersion(GameVersion.Multimodal, playIntro: true);
    }

    private void SelectVersion(GameVersion version, bool playIntro)
    {
        if (StorySequenceManager.IsStoryPlaying) return;
        if (!TryResetBeforeVersionSwitch()) return;

        CurrentVersion = version;

        ApplyVersionFlags();
        ApplyEnvironmentVersion();
        ApplyAllVersionSwitchers();
        ApplyFeedbackSettings();

        Debug.Log($"Selected: {version} Version");

        if (StoryEnabled && storySequenceManager != null)
        {
            storySequenceManager.ResetStoryProgress();

            if (playIntro)
                storySequenceManager.PlayIntro();
        }
        else if (!StoryEnabled && storySequenceManager != null)
        {
            storySequenceManager.ResetStoryProgress();
        }
    }

    private void ApplyVersionFlags()
    {
        StoryEnabled =
            CurrentVersion == GameVersion.Story ||
            CurrentVersion == GameVersion.Unimodal ||
            CurrentVersion == GameVersion.Multimodal;

        FeedbackEnabled = CurrentVersion != GameVersion.Unimodal;
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

    private void ApplyFeedbackSettings()
    {
        if (muteAudioInUnimodal)
            AudioListener.volume = FeedbackEnabled ? 1f : 0f;

        if (!FeedbackEnabled)
        {
            InputSystem.ResetHaptics();
            InputSystem.PauseHaptics();
        }
        else
        {
            InputSystem.ResumeHaptics();
        }

        if (hapticPlayersToToggle != null)
        {
            foreach (var player in hapticPlayersToToggle)
            {
                if (player == null) continue;
                player.enabled = FeedbackEnabled;
            }
        }

        Debug.Log($"[VersionManager] Story enabled: {StoryEnabled}");
        Debug.Log($"[VersionManager] Feedback enabled: {FeedbackEnabled}");
    }
}
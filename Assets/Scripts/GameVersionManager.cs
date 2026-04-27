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

    public void SetStoryVersion()
    {
        CurrentVersion = GameVersion.Story;
        Debug.Log("Selected: Story Version");
    }

    public void SetMinimalVersion()
    {
        CurrentVersion = GameVersion.Minimal;
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
}
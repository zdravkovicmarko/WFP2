using UnityEngine;

public enum GameVersion
{
    None,
    Version1,
    Version2,
    Version3,
    Version4
}

public class GameVersionManager : MonoBehaviour
{
    public static GameVersion CurrentVersion = GameVersion.None;

    public void SetVersion1()
    {
        CurrentVersion = GameVersion.Version1;
        Debug.Log("Selected: Story Version");
    }

    public void SetVersion2()
    {
        CurrentVersion = GameVersion.Version2;
        Debug.Log("Selected: Minimal Version");
    }

    public void SetVersion3()
    {
        CurrentVersion = GameVersion.Version3;
        Debug.Log("Selected: Marko Ver A");
    }

    public void SetVersion4()
    {
        CurrentVersion = GameVersion.Version4;
        Debug.Log("Selected: Marko Ver B");
    }
}
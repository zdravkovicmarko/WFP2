using System;
using UnityEngine;

public enum GameLanguage
{
    German,
    English
}

public class LanguageManager : MonoBehaviour
{
    public static GameLanguage CurrentLanguage { get; private set; } = GameLanguage.German;

    public static event Action OnLanguageChanged;

    public void SetGerman()
    {
        SetLanguage(GameLanguage.German);
    }

    public void SetEnglish()
    {
        SetLanguage(GameLanguage.English);
    }

    public void SetLanguage(GameLanguage language)
    {
        if (StorySequenceManager.IsStoryPlaying)
            return;

        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        OnLanguageChanged?.Invoke();

        Debug.Log($"[LanguageManager] Language changed to {CurrentLanguage}");
    }
}
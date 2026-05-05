using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum StoryFragment
{
    Intro,
    Memory,
    BlockPuzzle,
    Wimmelbook,
    ChainMaze,
    Final
}

public class StorySequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class TimedSubtitle
    {
        public float startTime;

        [TextArea(1, 3)]
        public string text;
    }

    [System.Serializable]
    public class SubtitleStyle
    {
        public Color textColor = Color.white;
        public bool italic;
    }

    [System.Serializable]
    public class LocalizedStoryContent
    {
        public AudioClip clip;

        [Header("Subtitles")]
        public List<TimedSubtitle> subtitles = new();
    }

    [System.Serializable]
    public class StoryClipPart
    {
        [Header("Speaker")]
        public bool isGrandmotherVoice;

        [Header("German")]
        public LocalizedStoryContent german = new();

        [Header("English")]
        public LocalizedStoryContent english = new();
    }

    [System.Serializable]
    public class StoryLine
    {
        public StoryFragment fragment;

        [Header("Clips played in order")]
        public List<StoryClipPart> parts = new();
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Subtitles")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameObject subtitlePanel;

    [Header("Subtitle Styles")]
    [SerializeField] private SubtitleStyle tutorialStyle = new SubtitleStyle
    {
        textColor = Color.white,
        italic = false
    };

    [SerializeField] private SubtitleStyle grandmotherStyle = new SubtitleStyle
    {
        textColor = new Color(1f, 0.86f, 0.55f),
        italic = true
    };

    [Header("Story Lines")]
    [SerializeField] private List<StoryLine> storyLines = new();

    [Header("Interaction Lock")]
    [SerializeField] private Behaviour[] disableWhileStoryPlays;

    private readonly HashSet<StoryFragment> completedFragments = new();

    private bool finalPlayed;
    private Coroutine activeRoutine;

    public static bool IsStoryPlaying { get; private set; }

    private void Awake()
    {
        HideSubtitle();
    }

    public void PlayIntro()
    {
        PlayFragment(StoryFragment.Intro);
    }

    public void PlayMemory()
    {
        PlayFragment(StoryFragment.Memory);
    }

    public void PlayBlockPuzzle()
    {
        PlayFragment(StoryFragment.BlockPuzzle);
    }

    public void PlayWimmelbook()
    {
        PlayFragment(StoryFragment.Wimmelbook);
    }

    public void PlayChainMaze()
    {
        PlayFragment(StoryFragment.ChainMaze);
    }

    public void PlayFragment(StoryFragment fragment)
    {
        if (!GameVersionManager.StoryEnabled)
            return;

        if (fragment != StoryFragment.Intro && fragment != StoryFragment.Final)
        {
            if (completedFragments.Contains(fragment))
                return;

            completedFragments.Add(fragment);
        }

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(PlayRoutine(fragment));
    }

    private IEnumerator PlayRoutine(StoryFragment fragment)
    {
        StoryLine line = GetLine(fragment);
        if (line == null)
            yield break;

        SetInteractionLock(true);

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        ShowSubtitle("");

        if (line.parts == null || line.parts.Count == 0)
        {
            yield return new WaitForSeconds(4f);
        }
        else
        {
            foreach (var part in line.parts)
            {
                if (part == null)
                    continue;

                yield return PlayClipPart(part);
            }
        }

        HideSubtitle();
        SetInteractionLock(false);

        activeRoutine = null;

        if (fragment != StoryFragment.Intro && fragment != StoryFragment.Final)
            TryPlayFinal();
    }

    private IEnumerator PlayClipPart(StoryClipPart part)
    {
        LocalizedStoryContent content = GetLocalizedStoryContent(part);

        if (audioSource == null || content == null || content.clip == null)
        {
            yield return new WaitForSeconds(4f);
            yield break;
        }

        audioSource.Stop();
        audioSource.clip = content.clip;
        audioSource.Play();

        ApplySubtitleStyle(part.isGrandmotherVoice);

        int currentSubtitleIndex = -1;

        while (audioSource.isPlaying)
        {
            float time = audioSource.time;

            int newIndex = GetSubtitleIndex(content, time);

            if (newIndex != currentSubtitleIndex)
            {
                currentSubtitleIndex = newIndex;

                if (currentSubtitleIndex >= 0)
                    ShowSubtitle(content.subtitles[currentSubtitleIndex].text);
                else
                    ShowSubtitle("");
            }

            yield return null;
        }

        ShowSubtitle("");
    }

    private void TryPlayFinal()
    {
        if (finalPlayed)
            return;

        bool allMinigamesDone =
            completedFragments.Contains(StoryFragment.Memory) &&
            completedFragments.Contains(StoryFragment.BlockPuzzle) &&
            completedFragments.Contains(StoryFragment.Wimmelbook) &&
            completedFragments.Contains(StoryFragment.ChainMaze);

        if (!allMinigamesDone)
            return;

        finalPlayed = true;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(PlayRoutine(StoryFragment.Final));
    }

    private StoryLine GetLine(StoryFragment fragment)
    {
        foreach (var line in storyLines)
        {
            if (line.fragment == fragment)
                return line;
        }

        Debug.LogWarning($"[StorySequence] Missing story line for {fragment}");
        return null;
    }

    private LocalizedStoryContent GetLocalizedStoryContent(StoryClipPart part)
    {
        if (part == null)
            return null;

        return LanguageManager.CurrentLanguage == GameLanguage.German
            ? part.german
            : part.english;
    }

    private void ApplySubtitleStyle(bool isGrandmotherVoice)
    {
        if (subtitleText == null)
            return;

        SubtitleStyle style = isGrandmotherVoice ? grandmotherStyle : tutorialStyle;

        subtitleText.color = style.textColor;
        subtitleText.fontStyle = style.italic ? FontStyles.Italic : FontStyles.Normal;
    }

    private void ShowSubtitle(string text)
    {
        if (subtitlePanel != null)
            subtitlePanel.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = text;
    }

    private void HideSubtitle()
    {
        if (subtitleText != null)
            subtitleText.text = "";

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }

    private int GetSubtitleIndex(LocalizedStoryContent content, float time)
    {
        int index = -1;

        if (content == null || content.subtitles == null)
            return index;

        for (int i = 0; i < content.subtitles.Count; i++)
        {
            if (time >= content.subtitles[i].startTime)
                index = i;
            else
                break;
        }

        return index;
    }

    public void ResetStoryProgress()
    {
        completedFragments.Clear();
        finalPlayed = false;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        HideSubtitle();
        SetInteractionLock(false);
        IsStoryPlaying = false;

        Debug.Log("[StorySequence] Story progress reset.");
    }

    private void SetInteractionLock(bool locked)
    {
        IsStoryPlaying = locked;

        if (disableWhileStoryPlays == null)
            return;

        foreach (var behaviour in disableWhileStoryPlays)
        {
            if (behaviour == null)
                continue;

            behaviour.enabled = !locked;
        }
    }
}
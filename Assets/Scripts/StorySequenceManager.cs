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
    public class StoryLine
    {
        public StoryFragment fragment;
        public AudioClip clip;
        public List<TimedSubtitle> subtitles = new();
    }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Subtitles")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameObject subtitlePanel;

    [Header("Story Lines")]
    [SerializeField] private List<StoryLine> storyLines = new();

    private readonly HashSet<StoryFragment> completedFragments = new();
    private bool finalPlayed;

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
        if (GameVersionManager.CurrentVersion != GameVersion.Story)
            return;

        if (fragment != StoryFragment.Intro && fragment != StoryFragment.Final)
        {
            if (completedFragments.Contains(fragment))
                return;

            completedFragments.Add(fragment);
        }

        StartCoroutine(PlayRoutine(fragment));
    }

    private IEnumerator PlayRoutine(StoryFragment fragment)
    {
        StoryLine line = GetLine(fragment);
        if (line == null)
            yield break;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        ShowSubtitle("");

        if (audioSource != null && line.clip != null)
        {
            audioSource.clip = line.clip;
            audioSource.Play();

            int currentSubtitleIndex = -1;

            while (audioSource.isPlaying)
            {
                float time = audioSource.time;

                int newIndex = GetSubtitleIndex(line, time);

                if (newIndex != currentSubtitleIndex)
                {
                    currentSubtitleIndex = newIndex;

                    if (currentSubtitleIndex >= 0)
                        ShowSubtitle(line.subtitles[currentSubtitleIndex].text);
                }

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(4f);
        }

        HideSubtitle();

        if (fragment != StoryFragment.Intro && fragment != StoryFragment.Final)
            TryPlayFinal();
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
        StartCoroutine(PlayRoutine(StoryFragment.Final));
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

    private int GetSubtitleIndex(StoryLine line, float time)
    {
        int index = -1;

        for (int i = 0; i < line.subtitles.Count; i++)
        {
            if (time >= line.subtitles[i].startTime)
                index = i;
            else
                break;
        }

        return index;
    }
}
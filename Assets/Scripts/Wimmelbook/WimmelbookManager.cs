using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WimmelbookManager : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform clueIconsParent;   // "Clue Icons"
    [SerializeField] private Transform foundCluesParent;  // "Found Clues"

    [Header("End Game")]
    [SerializeField] private RoomTeleporter endGameTeleporter;
    [SerializeField] private int totalClues = 0;          // 0 = auto count
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClip;

    private readonly Dictionary<string, GameObject> iconById = new();
    public Image image;
    private int  foundCount;
    private bool gameFinished;

    private void Awake()
    {
        CacheChildren(clueIconsParent, iconById, "clue_icon_");
    }

    private void OnEnable()
    {
        // Reset UI
        foreach (var kv in iconById)
            if (kv.Value) kv.Value.SetActive(true);

        // Reset clues (visual + "already found" bool)
        if (foundCluesParent != null)
        {
            int autoCount = 0;

            foreach (Transform t in foundCluesParent)
            {
                var clue = t.GetComponent<WimmelbookClueSelectable>();
                if (clue != null)
                {
                    clue.ResetClue();
                    autoCount++;
                }
            }

            if (totalClues <= 0)
                totalClues = autoCount;
        }

        foundCount = 0;
        gameFinished = false;
    }

    public void OnClueFound(string id)
    {
        if (gameFinished) return;

        foundCount++;
        DisableIcon(id);
        PlayCorrectSound();

        Debug.Log($"[WIMMELBOOK] Found clue {id}. Progress: {foundCount}/{totalClues}");

        if (foundCount >= totalClues)
        {
            gameFinished = true;
            Debug.Log("[WIMMELBOOK] All clues found → puzzle complete!");
            image.gameObject.SetActive(true);
            if (endGameTeleporter != null)
                endGameTeleporter.TeleportWithDefaultDelay(MinigameRoom.Wimmelbook);
        }
    }

    public void DisableIcon(string id)
    {
        if (iconById.TryGetValue(id, out var iconGo) && iconGo != null)
            iconGo.SetActive(false);
    }

    private static void CacheChildren(Transform parent, Dictionary<string, GameObject> dict, string prefix)
    {
        dict.Clear();
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            string name = child.name;
            if (!name.StartsWith(prefix)) continue;

            string id = name.Substring(prefix.Length);
            dict[id] = child.gameObject;
        }
    }
    void PlayCorrectSound()
    {
        if (audioSource != null && correctClip != null)
            audioSource.PlayOneShot(correctClip);
    }
}

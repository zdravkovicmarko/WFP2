using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryBoard : MonoBehaviour
{
    [Header("Card Parent")]
    [SerializeField] private Transform cardsParent;

    [Header("End Game")]
    [SerializeField] private RoomTeleporter endGameTeleporter;
    [SerializeField] private int totalPairs = 9;   // 9 pairs in your setup

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip wrongClip; 

    public Image image;

    private readonly List<Transform>   cardTransforms = new List<Transform>();
    private readonly List<Vector3>     slotPositions  = new List<Vector3>();
    private readonly List<MemoryCard>  activeCards    = new List<MemoryCard>();

    private bool isCheckingPair;
    private int  matchedPairs;
    private bool gameFinished;

    void Awake()
    {
        foreach (Transform child in cardsParent)
        {
            cardTransforms.Add(child);
            slotPositions.Add(child.position);
        }
    }

    void OnEnable()
    {
        ShuffleCards();
        InitializeCards();
    }

    private void ShuffleCards()
    {
        List<Vector3> freeSlots = new List<Vector3>(slotPositions);

        foreach (Transform card in cardTransforms)
        {
            int index = Random.Range(0, freeSlots.Count);
            card.position = freeSlots[index];
            freeSlots.RemoveAt(index);
        }
    }

    private void InitializeCards()
    {
        foreach (Transform cardTransform in cardsParent)
        {
            var card = cardTransform.GetComponent<MemoryCard>();
            if (card != null)
                card.Initialize(cardTransform.localPosition);
        }

        activeCards.Clear();
        isCheckingPair = false;
        matchedPairs   = 0;
        gameFinished   = false;
    }

    // --------- called by MemoryCard.OnSelected ---------

    public void HandleCardSelected(MemoryCard card)
    {
        if (gameFinished) return;
        if (isCheckingPair) return;              // currently resolving a pair
        if (activeCards.Contains(card)) return;  // no unselect/cheating
        if (activeCards.Count >= 2) return;      // wait until current pair done

        card.Reveal();
        activeCards.Add(card);

        if (activeCards.Count == 2)
            StartCoroutine(CheckPairCoroutine());
    }

    private IEnumerator CheckPairCoroutine()
    {
        isCheckingPair = true;

        // small delay so the player can actually see the second card
        yield return new WaitForSeconds(1.0f);

        var cardA = activeCards[0];
        var cardB = activeCards[1];

        if (cardA.PairId == cardB.PairId)
        {
            // MATCH → keep them face-up & locked
            cardA.SetMatched();
            cardB.SetMatched();
            matchedPairs++;
            PlayCorrectSound();
            Debug.Log($"[MEMORY] Match found. Pairs: {matchedPairs}/{totalPairs}");

            if (matchedPairs >= totalPairs)
            {
                gameFinished = true;
                Debug.Log("[MEMORY] All pairs found → puzzle complete!");
                image.gameObject.SetActive(true);
                if (endGameTeleporter != null)
                {
                    endGameTeleporter.TeleportWithDefaultDelay();
                }
            }
        }
        else
        {
            // NO MATCH → flip them back down
            cardA.Hide();
            cardB.Hide();
            PlayWrongSound();
        }

        activeCards.Clear();
        isCheckingPair = false;

        // optional: here you can check if all cards are matched → game won
    }

    void PlayCorrectSound()
    {
        if (audioSource != null && correctClip != null)
            audioSource.PlayOneShot(correctClip);
    }

    void PlayWrongSound()
    {
        if (audioSource != null && wrongClip != null)
            audioSource.PlayOneShot(wrongClip);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    [System.Serializable]
    public class DoorTagEntry
    {
        public MinigameRoom room;
        public DoorTag doorTag;
    }

    [Header("XR Locomotion")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    public Transform destination;
    public float defaultDelaySeconds = 0f;
    public GameObject[] activateOnTeleport;
    public GameObject[] deactivateOnTeleport;

    [Header("Door Tags")]
    [SerializeField] private List<DoorTagEntry> doorTags = new List<DoorTagEntry>();

    [Header("Tutorial Reset")]
    [SerializeField] private Unity.VRTemplate.StepManager[] tutorialsToReset;

    public void TeleportNow()
    {
        StartCoroutine(TeleportRoutine(0f, MinigameRoom.None));
    }

    public void TeleportWithDefaultDelay()
    {
        StartCoroutine(TeleportRoutine(defaultDelaySeconds, MinigameRoom.None));
    }

    public void TeleportWithDelay(float delaySeconds)
    {
        StartCoroutine(TeleportRoutine(delaySeconds, MinigameRoom.None));
    }

    public void TeleportWithDefaultDelay(MinigameRoom room)
    {
        StartCoroutine(TeleportRoutine(defaultDelaySeconds, room));
    }

    public void TeleportWithDelay(float delaySeconds, MinigameRoom room)
    {
        StartCoroutine(TeleportRoutine(delaySeconds, room));
    }

    private IEnumerator TeleportRoutine(float delaySeconds, MinigameRoom room)
    {
        if (teleportationProvider == null)
        {
            Debug.LogWarning("[RoomTeleporter] Missing TeleportationProvider reference.");
            yield break;
        }

        if (destination == null)
        {
            Debug.LogWarning("[RoomTeleporter] Missing destination Transform.");
            yield break;
        }

        MarkDoorAsComplete(room);

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        ResetTutorials();
        ApplyActivationSets();

        var request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest
        {
            destinationPosition = destination.position,
            destinationRotation = destination.rotation
        };

        teleportationProvider.QueueTeleportRequest(request);
    }

    private void MarkDoorAsComplete(MinigameRoom room)
    {
        if (room == MinigameRoom.None)
            return;

        foreach (var entry in doorTags)
        {
            if (entry == null || entry.doorTag == null)
                continue;

            if (entry.room == room)
            {
                entry.doorTag.SetComplete();
                return;
            }
        }

        Debug.LogWarning($"[RoomTeleporter] No DoorTag assigned for room '{room}'.");
    }

    private void ApplyActivationSets()
    {
        if (activateOnTeleport != null)
        {
            foreach (var go in activateOnTeleport)
            {
                if (go == null) continue;
                go.SetActive(true);
            }
        }

        if (deactivateOnTeleport != null)
        {
            foreach (var go in deactivateOnTeleport)
            {
                if (go == null) continue;
                go.SetActive(false);
            }
        }
    }

    private void ResetTutorials()
    {
        if (tutorialsToReset == null) return;

        foreach (var tutorial in tutorialsToReset)
        {
            if (tutorial != null)
                tutorial.ResetToFirstPage();
        }
    }
}
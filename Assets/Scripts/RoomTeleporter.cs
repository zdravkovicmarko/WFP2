using System.Collections;
using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    [Header("XR Locomotion")]
    [Tooltip("TeleportationProvider from your XR Rig / LocomotionSystem.")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

    [Header("Destination")]
    [Tooltip("Where the player should end up (a spawn point transform in the target room).")]
    public Transform destination;

    [Header("Default Delay")]
    [Tooltip("Default delay before teleport (seconds). Use 0 for instant.")]
    public float defaultDelaySeconds = 0f;

    [Header("Activation on Teleport")]
    [Tooltip("These objects will be SetActive(true) when this teleporter is used.")]
    public GameObject[] activateOnTeleport;

    [Tooltip("These objects will be SetActive(false) when this teleporter is used.")]
    public GameObject[] deactivateOnTeleport;

    // Called from door interaction (no delay)
    public void TeleportNow()
    {
        StartCoroutine(TeleportRoutine(0f));
    }

    // Called from game logic (e.g. 3 seconds after puzzle finished)
    public void TeleportWithDefaultDelay()
    {
        StartCoroutine(TeleportRoutine(defaultDelaySeconds));
    }

    // Called if you want a custom delay from code
    public void TeleportWithDelay(float delaySeconds)
    {
        StartCoroutine(TeleportRoutine(delaySeconds));
    }

    private IEnumerator TeleportRoutine(float delaySeconds)
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

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        // 1) Handle activation/deactivation for this door
        ApplyActivationSets();

        // 2) Queue teleport
        var request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest
        {
            destinationPosition = destination.position,
            destinationRotation = destination.rotation
        };

        teleportationProvider.QueueTeleportRequest(request);
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
}
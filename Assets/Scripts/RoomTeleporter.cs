using System.Collections;
using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    [Header("XR Locomotion")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
    public Transform destination;
    public float defaultDelaySeconds = 0f;
    public GameObject[] activateOnTeleport;
    public GameObject[] deactivateOnTeleport;

    public void TeleportNow()
    {
        StartCoroutine(TeleportRoutine(0f));
    }

    public void TeleportWithDefaultDelay()
    {
        StartCoroutine(TeleportRoutine(defaultDelaySeconds));
    }

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

        ApplyActivationSets();

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
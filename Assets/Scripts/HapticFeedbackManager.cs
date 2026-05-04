using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticFeedbackManager : MonoBehaviour
{
    [Header("Haptic Players")]
    [SerializeField] private HapticImpulsePlayer leftHaptic;
    [SerializeField] private HapticImpulsePlayer rightHaptic;

    [Header("Correct Feedback")]
    [SerializeField, Range(0f, 1f)] private float correctAmplitude = 0.4f;
    [SerializeField] private float correctDuration = 0.125f;

    [Header("Wrong Feedback")]
    [SerializeField, Range(0f, 1f)] private float wrongAmplitude = 0.2f;
    [SerializeField] private float wrongPulseDuration = 0.09f;
    [SerializeField] private float wrongPauseBetweenPulses = 0.2f;
    [SerializeField] private int wrongPulseCount = 4;

    private Coroutine wrongRoutine;

    public void PlayCorrect()
    {
        // Stop wrong pattern if it is currently playing
        if (wrongRoutine != null)
        {
            StopCoroutine(wrongRoutine);
            wrongRoutine = null;
        }

        SendHaptics(correctAmplitude, correctDuration);
    }

    public void PlayWrong()
    {
        if (wrongRoutine != null)
            StopCoroutine(wrongRoutine);

        wrongRoutine = StartCoroutine(PlayWrongRoutine());
    }

    private IEnumerator PlayWrongRoutine()
    {
        for (int i = 0; i < wrongPulseCount; i++)
        {
            SendHaptics(wrongAmplitude, wrongPulseDuration);

            if (i < wrongPulseCount - 1)
                yield return new WaitForSeconds(wrongPauseBetweenPulses);
        }

        wrongRoutine = null;
    }

    private void SendHaptics(float amplitude, float duration)
    {
        if (leftHaptic != null)
            leftHaptic.SendHapticImpulse(amplitude, duration);

        if (rightHaptic != null)
            rightHaptic.SendHapticImpulse(amplitude, duration);
    }
}
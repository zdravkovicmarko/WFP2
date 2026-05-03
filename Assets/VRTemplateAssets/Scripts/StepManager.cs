using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls tutorial cards and plays the matching voice line for each card.
    /// </summary>
    public class StepManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            public GameObject stepObject;
            public string buttonText;
            public AudioClip voiceLine;
        }

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI m_StepButtonTextField;

        [Header("Steps")]
        [SerializeField] private List<Step> m_StepList = new List<Step>();

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        private int m_CurrentStepIndex = 0;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            ResetToFirstPage(true);
        }

        public void Next()
        {
            if (StorySequenceManager.IsStoryPlaying)
                return;

            if (m_StepList == null || m_StepList.Count == 0)
                return;

            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);

            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;

            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);

            if (m_StepButtonTextField != null)
                m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;

            PlayCurrentVoiceLine();
        }

        public void ResetToFirstPage(bool playVoiceLine = true)
        {
            if (m_StepList == null || m_StepList.Count == 0)
                return;

            for (int i = 0; i < m_StepList.Count; i++)
                m_StepList[i].stepObject.SetActive(i == 0);

            m_CurrentStepIndex = 0;

            if (m_StepButtonTextField != null)
                m_StepButtonTextField.text = m_StepList[0].buttonText;

            if (playVoiceLine)
                PlayCurrentVoiceLine();
            else if (audioSource != null)
                audioSource.Stop();
        }

        private void PlayCurrentVoiceLine()
        {
            if (audioSource == null)
                return;

            audioSource.Stop();

            AudioClip clip = m_StepList[m_CurrentStepIndex].voiceLine;

            if (clip == null)
                return;

            audioSource.clip = clip;
            audioSource.Play();
        }

        private void OnDisable()
        {
            if (audioSource != null)
                audioSource.Stop();
        }
    }
}
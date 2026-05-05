using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls tutorial cards, localized text, and localized voice lines.
    /// </summary>
    public class StepManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [Header("Card")]
            public GameObject stepObject;

            [Tooltip("Text field inside this card that displays the tutorial text.")]
            public TextMeshProUGUI stepTextField;

            [Header("German")]
            [TextArea(2, 5)] public string germanText;
            public string germanButtonText;
            public AudioClip germanVoiceLine;

            [Header("English")]
            [TextArea(2, 5)] public string englishText;
            public string englishButtonText;
            public AudioClip englishVoiceLine;
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
            LanguageManager.OnLanguageChanged += RefreshCurrentStepText;
            ResetToFirstPage(true);
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= RefreshCurrentStepText;

            if (audioSource != null)
                audioSource.Stop();
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

            RefreshCurrentStepText();
            PlayCurrentVoiceLine();
        }

        public void ResetToFirstPage(bool playVoiceLine = true)
        {
            if (m_StepList == null || m_StepList.Count == 0)
                return;

            for (int i = 0; i < m_StepList.Count; i++)
                m_StepList[i].stepObject.SetActive(i == 0);

            m_CurrentStepIndex = 0;

            RefreshCurrentStepText();

            if (playVoiceLine)
                PlayCurrentVoiceLine();
            else if (audioSource != null)
                audioSource.Stop();
        }

        private void RefreshCurrentStepText()
        {
            if (m_StepList == null || m_StepList.Count == 0)
                return;

            Step step = m_StepList[m_CurrentStepIndex];

            if (step.stepTextField != null)
                step.stepTextField.text = GetStepText(step);

            if (m_StepButtonTextField != null)
                m_StepButtonTextField.text = GetButtonText(step);
        }

        private void PlayCurrentVoiceLine()
        {
            if (audioSource == null)
                return;

            audioSource.Stop();

            AudioClip clip = GetVoiceLine(m_StepList[m_CurrentStepIndex]);

            if (clip == null)
                return;

            audioSource.clip = clip;
            audioSource.Play();
        }

        private string GetStepText(Step step)
        {
            return LanguageManager.CurrentLanguage == GameLanguage.German
                ? step.germanText
                : step.englishText;
        }

        private string GetButtonText(Step step)
        {
            return LanguageManager.CurrentLanguage == GameLanguage.German
                ? step.germanButtonText
                : step.englishButtonText;
        }

        private AudioClip GetVoiceLine(Step step)
        {
            return LanguageManager.CurrentLanguage == GameLanguage.German
                ? step.germanVoiceLine
                : step.englishVoiceLine;
        }
    }
}
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls the steps in the in coaching card.
    /// </summary>
    public class StepManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;

            [SerializeField]
            public string buttonText;
        }

        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        int m_CurrentStepIndex = 0;

        public void Next()
        {
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
        }

        public void ResetToFirstPage()
        {
            if (m_StepList == null || m_StepList.Count == 0)
                return;

            for (int i = 0; i < m_StepList.Count; i++)
                m_StepList[i].stepObject.SetActive(i == 0);

            m_CurrentStepIndex = 0;

            if (m_StepButtonTextField != null)
                m_StepButtonTextField.text = m_StepList[0].buttonText;
        }
    }
}

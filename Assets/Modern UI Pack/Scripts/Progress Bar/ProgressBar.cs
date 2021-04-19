using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Michsky.UI.ModernUIPack
{
    public class ProgressBar : MonoBehaviour
    {
        // Content
        public float currentValue = 0;
        public float maxValue = 0;

        // Resources
        public Image loadingBar;
        public TextMeshProUGUI text;
        void Start()
        {
            loadingBar.fillAmount = currentValue / maxValue;
            text.text = Convert.ToString(currentValue * 0.01) + "s/" + Convert.ToString(maxValue * 0.01) + "s";
        }

        void Update()
        {
            loadingBar.fillAmount = currentValue / maxValue;
            text.text = Convert.ToString(currentValue * 0.01) + "s/" + Convert.ToString(maxValue * 0.01) + "s";
        }
    }
}
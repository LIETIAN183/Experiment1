using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Michsky.UI.ModernUIPack
{
    public class ProgressBar : MonoBehaviour
    {
        // Content
        public float currentTime = 0;
        public float maxTime = 0;

        // Resources
        public Image loadingBar;
        public TextMeshProUGUI text;

        void Update()
        {
            loadingBar.fillAmount = currentTime / maxTime;
            text.text = currentTime.ToString("f2") + "s/" + maxTime.ToString("f2") + "s";
        }
    }
}
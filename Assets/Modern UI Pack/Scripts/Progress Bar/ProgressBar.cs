using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Michsky.UI.ModernUIPack
{
    public class ProgressBar : MonoBehaviour
    {
        // Content
        public float currentValue;
        public float maxValue = 0;

        // Resources
        public Image loadingBar;
        public TextMeshProUGUI text;
        void Start()
        {
            // if (isOn == false)
            // {
            loadingBar.fillAmount = currentValue / maxValue;
            // textPercent.text = ((int)currentPercent).ToString("F0") + "%";
            text.text = Convert.ToString(currentValue * 0.01) + "s/" + Convert.ToString(maxValue * 0.01) + "s";
            // }
        }

        void Update()
        {
            // if (isOn == true)
            // {
            //     if (currentPercent <= maxValue && invert == false)
            //         currentPercent += speed * Time.deltaTime;
            //     else if (currentPercent >= 0 && invert == true)
            //         currentPercent -= speed * Time.deltaTime;

            //     if (currentPercent >= maxValue && speed != 0 && restart == true && invert == false)
            //         currentPercent = 0;
            //     else if (currentPercent <= 0 && speed != 0 && restart == true && invert == true)
            //         currentPercent = maxValue;

            loadingBar.fillAmount = currentValue / maxValue;

            // if (isPercent == true)
            //     textPercent.text = ((int)currentPercent).ToString("F0") + "%";
            // else
            // textPercent.text = ((int)currentPercent).ToString("F0");
            text.text = Convert.ToString(currentValue * 0.01) + "s/" + Convert.ToString(maxValue * 0.01) + "s";
            // }
        }

        // public void UpdateUI()
        // {
        //     loadingBar.fillAmount = currentPercent / maxValue;

        //     if (isPercent == true)
        //         textPercent.text = ((int)currentPercent).ToString("F0") + "%";
        //     else
        //         // textPercent.text = ((int)currentPercent).ToString("F0");
        //         textPercent.text = Convert.ToString(currentPercent * 0.01) + "s/" + Convert.ToString(maxValue * 0.01) + "s";
        // }
    }
}
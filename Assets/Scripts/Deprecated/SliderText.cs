using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Deprecated
{
    public class SliderText : MonoBehaviour
    {
        Text sliderText;
        Slider slider;
        // Start is called before the first frame update
        void Start()
        {
            sliderText = GetComponentInChildren<Text>();
            slider = GetComponentInChildren<Slider>();

        }

        // Update is called once per frame
        // 更新 Text 内容与进度条同步
        void Update()
        {
            sliderText.text = Convert.ToString(slider.value * 0.01) + "s/" + Convert.ToString(slider.maxValue * 0.01) + "s";
        }
    }
}
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class Counter : MonoBehaviour
{
    public static Counter Instance { get; private set; }
    [ShowInInspector, ProgressBar(0, "max")]
    public int count
    {
        get;
        private set;
    }
    [HideInInspector]
    public int max
    {
        get;
        private set;
    }
    public CounterEvent onValueChanged { get; set; }
    public UnityEvent StopEarthquake { get; set; }

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Init();
    }

    void Init()
    {
        onValueChanged = new CounterEvent();// 初始化 UnityEvent
        StopEarthquake = new UnityEvent();
        this.enabled = false;// 不激活脚本
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// 初始化参数
    /// </summary>
    void OnEnable()
    {
        count = 0;
        max = EqManger.Instance.getTime();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        count = 0;
        max = 0;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// 每个 0.01s 计数
    /// </summary>
    void FixedUpdate()
    {
        if (count == max - 1)
        {
            // Stop Earthquake
            StopEarthquake.Invoke();
            // this.enabled = false;
        }
        onValueChanged.Invoke(count++);
    }


    public class CounterEvent : UnityEvent<int>
    {
        public CounterEvent() { }
    }
}

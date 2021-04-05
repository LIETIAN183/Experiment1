using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;


// onValueChanged 为 UnityEvent<int> ，必须先继承实现构造函数
public class CounterEvent : UnityEvent<int>
{
    public CounterEvent() { }
}

public class Counter : MonoBehaviour
{
    // 单例模式
    public static Counter Instance { get; private set; }

    [ShowInInspector, ProgressBar(0, "maxTime")]
    public int count
    {
        get;
        private set;
    }

    [ShowInInspector, ReadOnly]
    private int maxTime;

    // 监听计数器变化
    public CounterEvent onValueChanged { get; set; }

    void Awake()
    {
        // 单例模式判断
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始化 UnityEvent
        onValueChanged = new CounterEvent();
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        EqManger.Instance.startEarthquake.AddListener(() => this.enabled = true);
        EqManger.Instance.startEarthquake.AddListener(() => maxTime = EqManger.Instance.GetTime());
        EqManger.Instance.endEarthquake.AddListener(() => this.enabled = false);
        this.enabled = false;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// 每个 0.01s 计数
    /// </summary>
    void FixedUpdate()
    {
        if (count >= maxTime - 1)
        {
            // Stop Earthquake
            EqManger.Instance.endEarthquake.Invoke();
        }
        onValueChanged.Invoke(count++);
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        Reset();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        Reset();
    }

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's
    /// context menu or when adding the component the first time.
    /// </summary>
    void Reset()
    {
        count = 0;
        maxTime = 0;
    }
}
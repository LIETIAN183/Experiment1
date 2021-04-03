﻿using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class GroundMove : MonoBehaviour
{
    private Rigidbody rb;

    // 存储加速度数据
    //由于PhysicX不支持double精度，所以不可避免地造成精度损失
    private List<Vector3> acc;

    [ShowInInspector, ReadOnly]
    public Vector3 currentAcceleration { get; private set; }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;// 地面不受重力影响

    }
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        // 监听事件
        EqManger.Instance.StartEarthquake.AddListener(() => this.enabled = true);
        EqManger.Instance.EndEarthquake.AddListener(() => this.enabled = false);
        Counter.Instance.onValueChanged.AddListener(count => currentAcceleration = EqManger.Instance.getAcc(count));// 匿名函数 Counter 变化时，更新对应加速度
        this.enabled = false; // 不激活脚本
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        rb.AddForce(currentAcceleration, ForceMode.Acceleration);
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// 开始地面震动
    /// </summary>
    void OnEnable()
    {
        Reset();
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// 结束地面震动
    /// </summary>
    void OnDisable()
    {
        Reset();
    }

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's
    /// context menu or when adding the component the first time.
    /// 重置脚本
    /// </summary>
    void Reset()
    {
        currentAcceleration = Vector3.zero;
        rb.velocity = Vector3.zero;
        acc = null;
    }
}


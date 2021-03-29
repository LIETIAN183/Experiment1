using System.ComponentModel;
using System.Threading;
using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Earthquake : MonoBehaviour
{
    public Transform DataManager;
    private Rigidbody rb;
    public int timeLength;
    // Start is called before the first frame update
    public int timeCount = 0;
    //由于PhysicX不支持double精度，所以不可避免地造成精度损失
    public List<Vector3> acc;

    public Vector3 currentAcceleration;

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        currentAcceleration = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;// 地面不受重力影响
        DataManager.GetComponent<EqDataManger>().getData(out acc, out timeLength);
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity += currentAcceleration * Time.deltaTime;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// Time Step Set in Project Settings as 0.01 second
    /// </summary>
    void FixedUpdate()
    {
        // 每隔 0.01s更新加速度
        if (timeCount++ <= timeLength - 1)
        {
            currentAcceleration = acc[timeCount];
        }
        else
        {
            // 地震结束
            this.enabled = false;
        }

    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        reset();
    }

    // 重置脚本
    private void reset()
    {
        rb.velocity = Vector3.zero;
        DataManager = null;
        timeLength = 0;
        timeCount = 0;
        acc = null;
        currentAcceleration = Vector3.zero;
    }
}

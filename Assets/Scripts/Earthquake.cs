using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class Earthquake : MonoBehaviour
{
    private Rigidbody rb;

    // 地震运行时间
    private int timeLength;
    // Start is called before the first frame update
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, "timeLength")]
    private int timeCount;

    // 存储加速度数据
    //由于PhysicX不支持double精度，所以不可避免地造成精度损失
    private List<Vector3> acc;
    [ReadOnly]
    public Vector3 currentAcceleration;

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;// 地面不受重力影响
        reset();
        EqManger.Instance.getData(out acc, out timeLength);// 读取数据

    }

    // Update is called once per frame
    void Update()
    {

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
            rb.AddForce(currentAcceleration, ForceMode.Acceleration);
        }
        else
        {
            // 地震结束
            this.enabled = false;
        }

    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// 地震结束，执行地面静止代码
    /// </summary>
    void OnDisable()
    {
        reset();
    }

    // 重置脚本
    private void reset()
    {
        currentAcceleration = Vector3.zero;
        rb.velocity = Vector3.zero;
        timeLength = 0;
        timeCount = 0;
        acc = null;
    }
}

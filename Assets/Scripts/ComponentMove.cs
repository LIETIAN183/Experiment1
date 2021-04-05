using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// TODO: 创建一个移动 父物体 pivot 到子物体几何中心的脚本，父物体只是一个用来包括子物体的空物体
// [ExecuteInEditMode]
public class ComponentMove : MonoBehaviour
{
    // FIXME: Ground 目前还需要手动在 Insptor 中设置，目标能通过代码自动识别物体下方的 Ground
    public GroundMove ground;
    public Rigidbody[] rbs;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        // 获取质心
        rbs = GetComponentsInChildren<Rigidbody>();
        Physics.SyncTransforms();
        foreach (var rb in rbs)
        {
            rb.ResetCenterOfMass();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 注册监听事件
        EqManger.Instance.startEarthquake.AddListener(() => this.enabled = true);
        EqManger.Instance.endEarthquake.AddListener(() => this.enabled = false);
        this.enabled = false;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        // 地震对物体施加力
        foreach (var rb in rbs)
        {
            rb.AddForceAtPosition(ground.currentAcceleration * rb.mass, rb.centerOfMass, ForceMode.Force);
        }
    }
}

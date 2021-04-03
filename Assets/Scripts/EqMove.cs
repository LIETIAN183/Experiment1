using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// TODO: 创建一个移动 父物体 pivot 到子物体几何中心的脚本，父物体只是一个用来包括子物体的空物体
// [ExecuteInEditMode]
public class EqMove : MonoBehaviour
{
    // FIXME: Ground 目前还需要手动在 Insptor 中设置，目标能通过代码自动识别物体下方的 Ground
    public GroundMove ground;
    public Rigidbody[] rbs;

    public List<Vector3> mass;
    // Start is called before the first frame update
    void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        Physics.SyncTransforms();
        foreach (var rb in rbs)
        {
            rb.ResetCenterOfMass();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        foreach (var rb in rbs)
        {
            rb.AddForceAtPosition(ground.currentAcceleration * rb.mass, rb.centerOfMass, ForceMode.Force);
        }

        mass = rbs.Select(rb => rb.centerOfMass).ToList();
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        foreach (var rb in rbs)
        {
            // Gizmos.DrawSphere(rb.centerOfMass, 0.05f);
        }
        // Giz
    }
}

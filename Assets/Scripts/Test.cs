using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class Test : MonoBehaviour
{
    Rigidbody _rb;
    Vector3 angle;

    Vector3 lastVelocity;
    public Vector3 acc;
    public Vector3 vel;
    public Vector3 cur_vel;
    public Vector3 dis;

    float time = 0;
    [Button("forward")]
    void forward()
    {
        angle = Vector3.forward;

    }

    [Button("Right")]
    void right()
    {
        angle = Vector3.right;
    }

    [Button("Left")]
    void left()
    {
        angle = Vector3.left;
    }

    [Button("Back")]
    void back()
    {
        angle = Vector3.back;
    }
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        // _rb.AddForce(angle, ForceMode.Acceleration);


        dis.x = 0.5f * angle.x * time * time;
        dis.y = 0.5f * angle.y * time * time;
        dis.z = 0.5f * angle.z * time * time;

        vel.x = angle.x * time;
        vel.y = angle.y * time;
        vel.z = angle.z * time;
        cur_vel = _rb.velocity;

    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {

        acc = (_rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = _rb.velocity;
        _rb.AddForce(angle, ForceMode.Acceleration);
    }

}
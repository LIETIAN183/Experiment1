using System.Reflection.Emit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class Test : MonoBehaviour
{
    Rigidbody _rb;
    Vector3 angle;
    public int degree = 0;

    Vector3 lastVelocity;
    public Vector3 acc;
    public Vector3 cur_vel;
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

    [Button("Calculate Angle")]
    void CalculateAngle()
    {
        Vector3 temp = Quaternion.AngleAxis(degree, Vector3.up) * Vector3.forward;
        Debug.Log(temp);
    }
}
using UnityEngine;
using Sirenix.OdinInspector;
public class AccTest : MonoBehaviour
{
    Rigidbody _rb;
    Vector3 acc;

    Vector3 lastVelocity;
    public Vector3 verifyAcc;
    // public Vector3 cur_vel;
    // float time = 0;

    [Button("forward")]
    void forward()
    {
        acc = Vector3.forward;
    }

    [Button("Back")]
    void back()
    {
        acc = Vector3.back;
    }

    [Button("Right")]
    void right()
    {
        acc = Vector3.right;
    }

    [Button("Left")]
    void left()
    {
        acc = Vector3.left;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        verifyAcc = (_rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = _rb.velocity;
        _rb.AddForce(acc, ForceMode.Acceleration);
    }
}
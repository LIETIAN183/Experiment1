using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float torque;
    private Rigidbody rb;
    Quaternion target = Quaternion.Euler(30, 0, 0);
    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        // transform.Rotate(new Vector3(10, 0, 0), Space.Self);
        // transform.Rotate(20.0f, 0.0f, 0.0f, Space.World);
    }

    // Update is called once per frame
    void Update()
    {
        // float turn = Input.GetAxis("Vertical");
        // rb.AddTorque(transform.up * torque * turn);
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 10f);

    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        // float turn = Input.GetAxis("Horizontal");
        // rb.AddTorque(transform.up * torque * turn);
        float turn = Input.GetAxis("Vertical");
        // ConsoleProDebug.Watch("forward:", transform.right.ToString());
        // ConsoleProDebug.Watch("turn:", turn.ToString());
        rb.AddTorque(transform.right * torque * turn);
    }

}

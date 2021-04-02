using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// [ExecuteInEditMode]
public class EqMove : MonoBehaviour
{
    public Earthquake ground;
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

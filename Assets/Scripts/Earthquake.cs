using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Earthquake : MonoBehaviour
{
    public float angle = 5.0f;

    public float velocity = 1.0f;

    public float varietyangle = 1.0f;

    private Rigidbody rb;

    public Vector3 Force = new Vector3(1, 0, 0);

    // public enum Space = Space.World;
    // Start is called before the first frame update
    void Start()
    {
        // InvokeRepeating("earthQuake", 0, 0.1f);
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Mathf.Sin(Time.time * velocity) * (angle - Random.Range(0, varietyangle));
        float y = transform.position.y;
        float z = Mathf.Cos(Time.time * velocity) * (angle - Random.Range(0, varietyangle));

        transform.Rotate(x - transform.eulerAngles.x, y, z - transform.eulerAngles.z);
        rb.AddForce(Force);
    }

    void earthQuake()
    {

    }
    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        // ConsoleProDebug.Watch("Value:", Vector3.Lerp(transform.eulerAngles, end, 0.01f).ToString());
        // transform.Rotate(Vector3.Lerp(transform.eulerAngles, end, 0.01f), Space.World);
        // transform.Rotate(Angle, 0.0f, 0.0f, Space.World);
        // if (Angle == 30.0f)
        // {
        //     Angle = -30.0f;
        // }
        // else
        // {
        //     Angle = 30.0f;
        // }
    }
}

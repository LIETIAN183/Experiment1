using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Earthquake : MonoBehaviour
{
    public Transform DataManager;
    private Rigidbody rb;
    public int timeLength = 0;
    // Start is called before the first frame update
    public int timeCount = 0;
    //由于PhysicX不支持double精度，所以不可避免地造成精度损失
    public List<Vector3> acc = new List<Vector3>();
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        acc = DataManager.GetComponent<EqDataReader>().acceleration;
        timeLength = DataManager.GetComponent<EqDataReader>().timeLength;

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
        if (timeCount <= timeLength - 1)
        {
            rb.AddForce(acc[timeCount], ForceMode.Acceleration);
        }
        timeCount++;
    }
}

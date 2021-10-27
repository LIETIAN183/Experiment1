using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Size : MonoBehaviour
{
    public Vector3 x1, x2;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        x1 = GetComponent<Collider>().bounds.size;
        x2 = GetComponent<Renderer>().bounds.size;
    }
}

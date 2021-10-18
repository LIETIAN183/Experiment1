using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GetSize : MonoBehaviour
{
    public Vector3 size;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        size = transform.GetComponent<Renderer>().bounds.size;
    }
}

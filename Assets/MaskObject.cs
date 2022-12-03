using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=Blits1yymCw&t=218s
public class MaskObject : MonoBehaviour
{
    public GameObject[] maskObjects;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < maskObjects.Length; i++)
        {
            maskObjects[i].GetComponent<MeshRenderer>().material.renderQueue = 3002;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

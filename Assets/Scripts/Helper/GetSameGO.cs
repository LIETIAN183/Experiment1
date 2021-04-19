using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GetSameGO : MonoBehaviour
{
    public string _tag;

    public int number;
    // Start is called before the first frame update
    [Button("GetSame")]
    void GetSame()
    {
        this.transform.position = Vector3.zero;
        GameObject[] gos = GameObject.FindGameObjectsWithTag(_tag);
        foreach (var go in gos)
        {
            go.transform.parent = this.transform;
        }
        number = gos.Length;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ResetPosition : MonoBehaviour
{

    [Button("ResetPosition")]
    void ResetParPos()
    {
        Vector3 parPos = this.transform.position;

        Transform[] tfs = GetComponentsInChildren<Transform>();
        foreach (var tf in tfs)
        {
            if (tf == this.transform)
            {
                Debug.Log(transform.name);
                continue;
            }
            tf.position += parPos;
        }
        this.transform.position = Vector3.zero;
    }
}

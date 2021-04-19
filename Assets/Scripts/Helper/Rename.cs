using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Rename : MonoBehaviour
{
    public string childrensName;

    [Button("Rename")]
    void ResetParPos()
    {
        Transform[] tfs = GetComponentsInChildren<Transform>();
        foreach (var tf in tfs)
        {
            if (tf == this.transform)
            {
                continue;
            }
            tf.name = childrensName;
        }
    }
}

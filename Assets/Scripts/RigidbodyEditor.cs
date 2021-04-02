using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class RigidbodyEditor : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
// }
using UnityEditor;
// using UnityEngine;
[CustomEditor(typeof(Rigidbody))]
public class RigidbodyEditor : Editor
{
    void OnSceneGUI()
    {
        Rigidbody rb = target as Rigidbody;
        Handles.color = Color.red;
        // Handles.SphereCap(1, rb.transform.TransformPoint(rb.centerOfMass), rb.rotation, 1f);
        Handles.SphereHandleCap(1, rb.transform.TransformPoint(rb.centerOfMass), rb.rotation, 0.01f, EventType.Repaint);
    }
    public override void OnInspectorGUI()
    {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);
        DrawDefaultInspector();
    }
}
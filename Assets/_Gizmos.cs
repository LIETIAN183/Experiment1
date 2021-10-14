using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class _Gizmos : MonoBehaviour
{
    ConstraintsSystem lockRotationSystem;
    // Start is called before the first frame update
    void Start()
    {
        lockRotationSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ConstraintsSystem>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (lockRotationSystem != null)
        {
            lockRotationSystem.OnDrawGizmos();
        }
        else
        {
            lockRotationSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ConstraintsSystem>();
        }
    }
}

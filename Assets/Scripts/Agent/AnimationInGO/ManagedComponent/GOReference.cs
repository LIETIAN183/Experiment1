using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using FischlWorks;

public class GOReference : IComponentData, IDisposable
{
    public Transform transform;
    public Animator animator;
    public csHomebrewIK information;

    public FootInteraction ragdoll;

    public float aniSpeed;

    public void Dispose()
    {
        if (transform != null)
        {
            UnityEngine.Object.Destroy(transform.gameObject);
        }
    }
}

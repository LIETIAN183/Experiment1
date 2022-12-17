using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct MCData : IComponentData
{
    // 用于判断空中状态
    public float previousVelinY;
    public bool inAir;
}

public class MCDataAuthoring : MonoBehaviour { }

public class MCDataAuthoringBaker : Baker<MCDataAuthoring>
{
    public override void Bake(MCDataAuthoring authoring)
    {
        AddComponent<MCData>();
    }
}
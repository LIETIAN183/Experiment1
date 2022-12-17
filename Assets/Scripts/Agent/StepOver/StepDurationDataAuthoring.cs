using Unity.Entities;
using UnityEngine;

public struct StepDurationData : IComponentData
{
    public float Value;
}

public class StepDurationDataAuthoring : MonoBehaviour { }

public class StepDurationDataAuthoringBaker : Baker<StepDurationDataAuthoring>
{
    public override void Bake(StepDurationDataAuthoring authoring)
    {
        AddComponent<StepDurationData>();
    }
}
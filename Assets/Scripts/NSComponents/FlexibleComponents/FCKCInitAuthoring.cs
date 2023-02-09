using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
public class FCKCInitAuthoring : MonoBehaviour
{
    class Baker : Baker<FCKCInitAuthoring>
    {
        public override void Bake(FCKCInitAuthoring authoring)
        {
            AddComponent<FCKCInitData>();
        }
    }
}

public struct FCKCInitData : IComponentData
{
    public float k, c;
}

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[BurstCompile]
public partial struct FCKCBakingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new FCKCInitJob().ScheduleParallel(state.Dependency).Complete();
    }
}
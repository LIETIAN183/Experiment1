using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
public class CalculateCostFieldSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    public float accMax = 0;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;
        var accMagnitude = math.length(GetSingleton<AccTimerData>().acc) / 9.81f;
        if (accMax < accMagnitude) accMax = accMagnitude;
        var accTemp = accMax;

        Entities.ForEach((ref DynamicBuffer<CellBufferElement> buffer, ref FlowFieldSettingData flowFieldSettingData) =>
        {
            if (buffer.Length == 0) return;

            // Cost Field
            DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);
            for (int i = 0; i < cellBuffer.Length; i++)
            {
                CellData curCellData = cellBuffer[i];

                var pos = curCellData.worldPos;
                pos.y = 1;
                var halfExtents = flowFieldSettingData.cellRadius;
                halfExtents.y = 1;
                // 计算网格内障碍物
                outHits.Clear();
                physicsWorld.OverlapBox(pos, quaternion.identity, halfExtents, ref outHits, CollisionFilter.Default);

                // 计算前重置初始值
                // curCellData.cost = 1;

                float cost = 1;
                float max_y = 0;
                float sum_m = 0;
                int count = 0;
                foreach (var hit in outHits)
                {
                    if (hit.Material.CustomTags.Equals(2))//00000010
                    {
                        cost = byte.MaxValue;
                        count++;
                        // break;
                    }
                    else if (hit.Material.CustomTags.Equals(1))//00000001
                    {
                        if (-hit.Fraction > max_y) max_y = -hit.Fraction;
                        var component = GetComponentDataFromEntity<PhysicsMass>(true)[hit.Entity];
                        sum_m += 1 / component.InverseMass;
                        count++;
                    }
                }
                cost += math.exp(-accTemp) * sum_m * max_y * 2;
                if (cost > 255) cost = 255;

                curCellData.cost = (byte)cost;
                curCellData.count = count;

                // 计算 Integration Field 前重置 bestCost
                curCellData.bestCost = ushort.MaxValue;
                curCellData.bestDirection = int2.zero;
                cellBuffer[i] = curCellData;
            }

            outHits.Dispose();
        }).Run();

        World.DefaultGameObjectInjectionWorld.GetExistingSystem<CalculateFlowFieldSystem>().timer = 0.2f;
    }
}

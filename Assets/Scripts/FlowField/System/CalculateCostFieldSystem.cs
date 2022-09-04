using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;

[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
public partial class CalculateCostFieldSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        DynamicBuffer<CellData> cellBuffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>();

        var job = new CalculateCostJob()
        {
            cells = cellBuffer.AsNativeArray(),
            physicsWorld = buildPhysicsWorld.PhysicsWorld,
            pgaInms2 = GetSingleton<AccTimerData>().curPGA * 9.8f,
            cellRadius = GetSingleton<FlowFieldSettingData>().cellRadius,
            translationArray = GetComponentDataFromEntity<Translation>(true),
            massArray = GetComponentDataFromEntity<PhysicsMass>(true)
        };

        job.Schedule(cellBuffer.Length, 64).Complete();
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<CalculateFlowFieldSystem>().Enabled = true;
    }

    [BurstCompile]
    struct CalculateCostJob : IJobParallelFor
    {
        public NativeArray<CellData> cells;
        [ReadOnly] public PhysicsWorld physicsWorld;
        [ReadOnly] public float pgaInms2;
        [ReadOnly] public float3 cellRadius;
        [ReadOnly] public ComponentDataFromEntity<Translation> translationArray;
        [ReadOnly] public ComponentDataFromEntity<PhysicsMass> massArray;
        public void Execute(int index)
        {
            var curCell = cells[index];
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.Temp);
            physicsWorld.OverlapBox(curCell.worldPos, quaternion.identity, cellRadius, ref outHits, CollisionFilter.Default);
            float cost = 0, max_height = 0, sum_mass = 0;
            foreach (var hit in outHits)
            {
                if (cost < 255)
                {
                    if ((hit.Material.CustomTags & 0b_0000_1000) != 0)
                    {
                        cost = 255;
                        break;
                    }

                    var entityPos = translationArray[hit.Entity].Value;
                    if ((hit.Material.CustomTags & 0b_0000_0011) != 0)
                    {
                        // 小障碍物的customtags值为1，所以无影响，中等障碍物的customtags值为2，所以计算高度时为其坐标×2
                        if (entityPos.y * hit.Material.CustomTags > max_height) max_height = hit.Material.CustomTags * entityPos.y;

                        var component = massArray[hit.Entity];
                        sum_mass += 1 / component.InverseMass;
                    }
                }
            }
            cost += math.exp(-pgaInms2) * sum_mass * max_height * 2 + math.exp(max_height);
            if (cost > 255) cost = 255;
            curCell.cost = (byte)cost;
            curCell.bestCost = ushort.MaxValue;
            curCell.updated = false;
            cells[index] = curCell;
            outHits.Dispose();
        }
    }
}

// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Physics;
// using Unity.Physics.Systems;
// // using UnityEngine;
// using Unity.Transforms;

// [UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
// public partial class CalculateCostFieldSystem : SystemBase
// {
//     private BuildPhysicsWorld buildPhysicsWorld;

//     protected override void OnCreate() => buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
//     protected override void OnUpdate()
//     {
//         var physicsWorld = buildPhysicsWorld.PhysicsWorld;
//         // var accMagnitude = math.length(GetSingleton<AccTimerData>().acc);
//         var _pga = GetSingleton<AccTimerData>().curPGA * 9.8f;

//         Entities.ForEach((ref DynamicBuffer<CellBufferElement> buffer, ref FlowFieldSettingData flowFieldSettingData) =>
//         {
//             if (buffer.Length == 0) return;

//             // Cost Field
//             DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
//             NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);
//             for (int i = 0; i < cellBuffer.Length; i++)
//             {
//                 CellData curCellData = cellBuffer[i];
//                 // 计算网格内障碍物
//                 outHits.Clear();
//                 physicsWorld.OverlapBox(curCellData.worldPos, quaternion.identity, flowFieldSettingData.cellRadius, ref outHits, CollisionFilter.Default);

//                 // 计算前初始化
//                 float cost = 0, max_y = 0, sum_m = 0, count = 0;
//                 foreach (var hit in outHits)
//                 {
//                     // TODO: 区分小型障碍物、大型障碍物、不可通过区域的障碍物，考虑小障碍物可能在货架上
//                     if (cost < 255)
//                     {
//                         if ((hit.Material.CustomTags & 0b_0000_1100) != 0)
//                         {
//                             cost = 255;
//                         }

//                         // 判断检测到的物体是否可以被该网格计算
//                         var entityPos = GetComponentDataFromEntity<Translation>(true)[hit.Entity].Value;
//                         // TODO:
//                         // if (math.abs(entityPos.x - curCellData.worldPos.x) > 0.3f || math.abs(entityPos.z - curCellData.worldPos.z) > 0.3f) continue;


//                         if ((hit.Material.CustomTags & 0b_0000_0011) != 0)
//                         {
//                             // 小障碍物的customtags值为1，所以无影响，中等障碍物的customtags值为2，所以计算高度时为其坐标×2
//                             if (entityPos.y * hit.Material.CustomTags > max_y) max_y = hit.Material.CustomTags * entityPos.y;

//                             var component = GetComponentDataFromEntity<PhysicsMass>(true)[hit.Entity];
//                             sum_m += 1 / component.InverseMass;
//                             count++;
//                         }
//                     }
//                 }
//                 cost += math.exp(-_pga) * sum_m * max_y * 2 + math.exp(max_y);
//                 // cost += sum_m * max_y * 2 + math.exp(max_y);
//                 if (cost > 255) cost = 255;
//                 curCellData.cost = (byte)cost;

//                 // 计算 Integration Field 前重置 bestCost
//                 curCellData.bestCost = ushort.MaxValue;

//                 curCellData.updated = false;
//                 // curCellData.bestDirection = int2.zero;
//                 cellBuffer[i] = curCellData;
//             }
//             outHits.Dispose();
//         }).Run();

//         World.DefaultGameObjectInjectionWorld.GetExistingSystem<CalculateFlowFieldSystem>().timer = 0.2f;
//     }
// }
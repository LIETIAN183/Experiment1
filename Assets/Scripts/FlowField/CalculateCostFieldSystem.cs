using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

// [DisableAutoCreation]
[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
public class CalculateCostFieldSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var physicsWorld = buildPhysicsWorld.PhysicsWorld;

        // 通过鼠标设置目标点
        float3 worldMousePos = float3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        }

        Entities.ForEach((ref DynamicBuffer<CellBufferElement> buffer, ref FlowFieldSettingData flowFieldSettingData) =>
        {
            if (buffer.Length == 0) return;

            flowFieldSettingData.destination = worldMousePos.Equals(float3.zero) ? flowFieldSettingData.destination : worldMousePos;

            // Cost Field
            DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
            NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);
            int2 gridSize = flowFieldSettingData.gridSize;
            for (int i = 0; i < cellBuffer.Length; i++)
            {
                CellData curCellData = cellBuffer[i];

                // 计算网格内障碍物
                outHits.Clear();
                physicsWorld.OverlapBox(curCellData.worldPos, quaternion.identity, flowFieldSettingData.cellRadius, ref outHits, CollisionFilter.Default);

                // 计算前重置初始值
                curCellData.cost = 1;
                foreach (var hit in outHits)
                {
                    if (hit.Material.CustomTags.Equals(1))//00000001
                    {
                        curCellData.cost++;
                    }
                    else if (hit.Material.CustomTags.Equals(2))//00000010
                    {
                        curCellData.cost = byte.MaxValue;
                        break;
                    }
                }

                // 计算 Integration Field 前重置 bestCost
                curCellData.bestCost = ushort.MaxValue;
                cellBuffer[i] = curCellData;
            }

            outHits.Dispose();
        }).Run();

        World.DefaultGameObjectInjectionWorld.GetExistingSystem<CalculateFlowFieldSystem>().Enabled = true;
    }
}

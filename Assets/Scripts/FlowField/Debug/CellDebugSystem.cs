using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;
using Unity.Burst;
using Drawing;

[BurstCompile]
public partial struct CellDebugSystem : ISystem
{
    private float3 drawPos;
    private float3 cellSize;

    public CellData curDebugCell;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CameraRefData>();
        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (SystemAPI.ManagedAPI.GetSingleton<CameraRefData>().mainCamera == null) return;
            var screenRay = SystemAPI.ManagedAPI.GetSingleton<CameraRefData>().mainCamera.ScreenPointToRay(Input.mousePosition);
            var RaycastInput = new RaycastInput
            {
                Start = screenRay.origin,
                End = screenRay.GetPoint(100),
                Filter = CollisionFilter.Default
            };
            if (SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.CastRay(RaycastInput, out RaycastHit hit))
            {
                float3 worldMousePos = hit.Position;
                //判断鼠标点击的点是否在网格内
                FlowFieldSettingData data = SystemAPI.GetSingleton<FlowFieldSettingData>();
                if (!FlowFieldUtility.GetCellIndexFromWorldPos(worldMousePos, data.originPoint, data.gridSetSize, data.cellRadius * 2).Equals(Constants.notInGridSet))
                {
                    GetCellWorldPos(data, worldMousePos);
                }
            }
        }

        using (Draw.ingame.WithColor(Color.red * 0.5f))
        {
            Draw.ingame.SolidBox(drawPos, cellSize);
        }

    }

    [BurstCompile]
    public void GetCellWorldPos(FlowFieldSettingData data, float3 mousePos)
    {
        var flatIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(mousePos, data.originPoint, data.gridSetSize, data.cellRadius * 2);
        curDebugCell = SystemAPI.GetSingletonBuffer<CellBuffer>()[flatIndex].cell;
        drawPos = curDebugCell.worldPos + data.displayOffset;
        cellSize = data.cellRadius * 2;
        cellSize.y = 0.1f;
    }
}

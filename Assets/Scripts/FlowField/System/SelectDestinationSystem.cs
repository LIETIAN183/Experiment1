using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine;
using Unity.Burst;
using Drawing;

[BurstCompile]
public partial struct SelectDestinationSystem : ISystem
{
    private static readonly int displayTime = 3;
    private float timer;
    private float3 drawPos;
    private float3 cellSize;
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
        // 通过鼠标设置目标点
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
                    data.destination = worldMousePos;
                    SystemAPI.SetSingleton<FlowFieldSettingData>(data);
                    timer = displayTime;
                    GetCellWorldPos(data, worldMousePos);
                }
                else
                {
                    SystemAPI.SetSingleton(new MessageEvent
                    {
                        isActivate = true,
                        message = "Select Point Out of Range",
                        displayForever = false
                    });
                }
            }
        }

        if (timer > 0)
        {
            timer -= SystemAPI.Time.DeltaTime;
            var coefficent = timer / displayTime;
            var color = coefficent * Color.red + (1 - coefficent) * Color.clear;
            using (Draw.ingame.WithColor(color))
            {
                Draw.ingame.SolidBox(drawPos, cellSize);
            }
        }
    }

    [BurstCompile]
    public void GetCellWorldPos(FlowFieldSettingData data, float3 mousePos)
    {
        var flatIndex = FlowFieldUtility.GetCellFlatIndexFromWorldPos(mousePos, data.originPoint, data.gridSetSize, data.cellRadius * 2);
        drawPos = SystemAPI.GetSingletonBuffer<CellBuffer>()[flatIndex].cell.worldPos + data.displayOffset;
        cellSize = data.cellRadius * 2;
        cellSize.y = 0.1f;
    }
}

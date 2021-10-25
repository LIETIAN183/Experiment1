using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(AgentSimulationSystemGroup))]
public class AgentMovementSystem : SystemBase
{
    protected override void OnCreate()
    {
        this.Enabled = false;
    }
    protected override void OnUpdate()
    {
        DynamicBuffer<CellBufferElement> buffer = GetBufferFromEntity<CellBufferElement>(true)[GetSingletonEntity<FlowFieldSettingData>()];
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
        if (cellBuffer.Length == 0) return;
        var settingData = GetSingleton<FlowFieldSettingData>();

        float deltaTime = Time.DeltaTime;

        int2 destinationIndex = FlowFieldHelper.GetCellIndexFromWorldPos(settingData.originPoint, settingData.destination, settingData.gridSize, settingData.cellRadius * 2);

        Entities.WithReadOnly(cellBuffer).ForEach((ref PhysicsVelocity physVelocity, ref AgentMovementData movementData, in Translation translation) =>
        {
            if (movementData.state == AgentState.NotActive)
            {
                physVelocity.Linear = float3.zero;
                return;
            }
            // 获得当前所在位置的坐标
            int2 localCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(settingData.originPoint, translation.Value, settingData.gridSize, settingData.cellRadius * 2);

            // 到达目的地，停止运动
            if (localCellIndex.Equals(destinationIndex))
            {
                movementData.state = AgentState.NotActive;
                return;
            }

            // 获得当前的期望方向
            int flatLocalCellIndex = FlowFieldHelper.ToFlatIndex(localCellIndex, settingData.gridSize.y);
            float2 moveDirection = cellBuffer[flatLocalCellIndex].bestDirection;
            movementData.desireDirection = moveDirection;
            physVelocity.Linear += (new float3(moveDirection.x, 0, moveDirection.y) * movementData.desireSpeed - physVelocity.Linear) / 0.5f * deltaTime;

        }).ScheduleParallel();

        this.CompleteDependency();
    }
}

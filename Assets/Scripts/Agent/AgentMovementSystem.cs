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
    protected override void OnUpdate()
    {
        DynamicBuffer<CellBufferElement> buffer = GetBufferFromEntity<CellBufferElement>(true)[GetSingletonEntity<FlowFieldSettingData>()];
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();
        var settingData = GetSingleton<FlowFieldSettingData>();

        float deltaTime = Time.DeltaTime;

        if (cellBuffer.Length == 0) return;

        Entities.WithReadOnly(cellBuffer).ForEach((ref PhysicsVelocity physVelocity, ref AgentMovementData movementData, in Translation translation) =>
        {
            int2 localCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(settingData.originPoint, translation.Value, settingData.gridSize, settingData.cellRadius * 2);

            int flatLocalCellIndex = FlowFieldHelper.ToFlatIndex(localCellIndex, settingData.gridSize.y);
            float2 moveDirection = cellBuffer[flatLocalCellIndex].bestDirection;
            movementData.desireDirection = moveDirection;
            physVelocity.Linear.xz = moveDirection * movementData.desireSpeed * deltaTime;
        }).ScheduleParallel();

        this.CompleteDependency();
    }
}

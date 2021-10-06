﻿using Unity.Entities;

// [DisableAutoCreation]
[UpdateInGroup(typeof(FlowFieldSimulationSystemGroup))]
[UpdateAfter(typeof(CalculateFlowFieldSystem))]
public class DebugSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 重置GridDebug数据
        GridDebug.instance.ClearList();

        // 传输 DynamicBuffer 的 Cell 数据到 GridDebug
        DynamicBuffer<CellBufferElement> buffer = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>());
        DynamicBuffer<CellData> cellBuffer = buffer.Reinterpret<CellData>();

        for (int i = 0; i < cellBuffer.Length; i++)
        {
            GridDebug.instance.AddToList(cellBuffer[i]);
        }
    }
}
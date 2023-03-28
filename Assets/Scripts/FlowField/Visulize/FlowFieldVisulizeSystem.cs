using Unity.Entities;
using Drawing;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[BurstCompile]
// [DisableAutoCreation]
public partial struct FlowFieldVisulizeSystem : ISystem
{
    private float3 gridSizeInMeters, displayOffset, center;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.AddComponentData<FFVisTypeStateData>(state.SystemHandle, new FFVisTypeStateData { ffVisType = FlowFieldVisulizeType.None });
        gridSizeInMeters = displayOffset = center = 0;
        state.RequireForUpdate<FlowFieldSettingData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    // [BurstCompile]
    // DrawingManager.GetBuilder 为 managed mathod, 不可 BurstCompile
    public void OnUpdate(ref SystemState state)
    {
        // ----颜色刻度尺---
        // for (int i = 0; i <= Constants.T_c; ++i)
        // {
        //     Color drawColor = i >= Constants.T_c ? Color.black : Color.HSVToRGB((1 - i / Constants.T_c) / 3, 1, 1);
        //     Draw.ingame.SolidRectangle(new Rect(0, i / Constants.T_c * 50f, 5f, 1 / Constants.T_c * 50f), drawColor);
        // }
        //------------------
        // -------快速扫描法更新顺序可视化----------
        // using (Draw.ingame.WithLineWidth(3))
        // {
        //     Draw.ingame.WireGrid(new float3(0, 0, 0), Quaternion.identity, new int2(10, 10), new float2(10, 10), Color.black);
        // Draw.ingame.Arrow(new float3(-4.5f, 0, 4.5f), new float3(-4.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(-3.5f, 0, 4.5f), new float3(-3.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(-2.5f, 0, 4.5f), new float3(-2.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(-1.5f, 0, 4.5f), new float3(-1.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(-0.5f, 0, 4.5f), new float3(-0.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(0.5f, 0, 4.5f), new float3(0.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(1.5f, 0, 4.5f), new float3(1.5f, 0, -4.5f), math.up(), 0.1f, Color.blue);
        // Draw.ingame.Arrow(new float3(2.5f, 0, 4.5f), new float3(2.5f, 0, -2.5f), math.up(), 0.1f, Color.blue);
        // }
        //--------------------------------
        //-----------0-7刻度尺---------------
        // using (Draw.ingame.WithLineWidth(3))
        // {
        //     Draw.ingame.Arrow(float3.zero, new float3(0, 0, 8), math.up(), 0.3f, Color.black);
        //     Draw.ingame.Line(float3.zero, new float3(0.3f, 0, 0), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 1), new float3(0.3f, 0, 1), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 2), new float3(0.3f, 0, 2), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 3), new float3(0.3f, 0, 3), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 4), new float3(0.3f, 0, 4), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 5), new float3(0.3f, 0, 5), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 6), new float3(0.3f, 0, 6), Color.black);
        //     Draw.ingame.Line(new float3(0, 0, 7), new float3(0.3f, 0, 7), Color.black);
        // }
        //-----------------------------
        var ffVisType = SystemAPI.GetComponent<FFVisTypeStateData>(state.SystemHandle).ffVisType;
        if (ffVisType.Equals(FlowFieldVisulizeType.None)) return;

        var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
        if (gridSizeInMeters.Equals(float3.zero)) gridSizeInMeters = new float3(setting.cellRadius.x * 2 * setting.gridSetSize.x, 0, setting.cellRadius.z * 2 * setting.gridSetSize.y);
        if (!displayOffset.Equals(setting.displayOffset))
        {
            displayOffset = setting.displayOffset;
            center = setting.originPoint + gridSizeInMeters / 2 + displayOffset;
        }

        var builder = DrawingManager.GetBuilder(true);
        // var cameraRef = SystemAPI.ManagedAPI.GetSingleton<CameraRefData>();
        // builder.cameraTargets = new Camera[] { cameraRef.mainCamera, cameraRef.overHeadCamera };
        builder.Preallocate(setting.gridSetSize.x * setting.gridSetSize.y * 400);

        var drawJob = new DrawFlowFieldJob
        {
            ffVisType = ffVisType,
            gridSize = setting.gridSetSize,
            cellRadius = setting.cellRadius,
            gridSizeInMeters = gridSizeInMeters,
            displayOffset = displayOffset,
            center = center,
            cells = SystemAPI.GetSingletonBuffer<CellBuffer>(true).Reinterpret<CellData>().AsNativeArray(),
            builder = builder,
            dests = SystemAPI.GetSingletonBuffer<DestinationBuffer>(true).Reinterpret<int>().AsNativeArray()
        }.Schedule(state.Dependency);

        builder.DisposeAfter(drawJob);
        drawJob.Complete();
    }
}

[BurstCompile]
public struct DrawFlowFieldJob : IJob
{
    [ReadOnly] public FlowFieldVisulizeType ffVisType;
    [ReadOnly] public int2 gridSize;
    [ReadOnly] public float3 gridSizeInMeters, displayOffset, center, cellRadius;
    [ReadOnly] public NativeArray<CellData> cells;
    [ReadOnly] public NativeArray<int> dests;
    public CommandBuilder builder;
    public void Execute()
    {
        float3 drawSize = cellRadius * 2, drawPos;
        builder.PushLineWidth(2f);
        switch (ffVisType)
        {
            case FlowFieldVisulizeType.Grid:
                builder.WireGrid(center, Quaternion.identity, gridSize, gridSizeInMeters.xz, Color.black);
                break;
            case FlowFieldVisulizeType.CostField:
                builder.WireGrid(center, Quaternion.identity, gridSize, gridSizeInMeters.xz, Color.black);
                foreach (var cell in cells)
                {
                    FixedString32Bytes valueString = cell.localCost >= Constants.T_c ? "M" : $"{(int)cell.localCost}";
                    builder.Label3D(cell.worldPos + displayOffset, quaternion.Euler(1.57f, 1.57f, 0), ref valueString, 0.17f, LabelAlignment.Center);
                }
                drawSize.y = 0.1f;
                foreach (var flatIndex in dests)
                {
                    builder.WireBox(cells[flatIndex].worldPos + displayOffset, drawSize, Color.blue);
                }
                break;
            case FlowFieldVisulizeType.IntegrationField:
                builder.WireGrid(center, Quaternion.identity, gridSize, gridSizeInMeters.xz, Color.black);
                foreach (var cell in cells)
                {
                    FixedString32Bytes valueString = cell.integrationCost >= Constants.T_i ? "M" : $"{(int)cell.integrationCost}";
                    builder.Label3D(cell.worldPos + displayOffset, quaternion.Euler(1.57f, 1.57f, 0), ref valueString, 0.17f, LabelAlignment.Center);
                }
                drawSize.y = 0.1f;
                foreach (var flatIndex in dests)
                {
                    builder.WireBox(cells[flatIndex].worldPos + displayOffset, drawSize, Color.blue);
                }
                break;
            case FlowFieldVisulizeType.CostHeatMap:
                //https://stackoverflow.com/questions/10901085/range-values-to-pseudocolor
                // float maxCost = cells.SecondMaxCost();
                float maxCost = cells.SecondMaxCost();

                for (int i = 0; i < cells.Length; i++)
                {
                    Color drawColor = cells[i].localCost >= Constants.T_c ? Color.black : Color.HSVToRGB((1 - cells[i].localCost / maxCost) / 3, 1, 1);
                    var height = math.min(cells[i].localCost / maxCost, 1);
                    drawPos = cells[i].worldPos;
                    drawPos.y += height / 2;
                    drawSize.y = height;
                    if (dests.Contains(i))
                    {
                        builder.WireBox(drawPos + displayOffset, drawSize, Color.blue);
                    }
                    builder.SolidBox(drawPos + displayOffset, drawSize, drawColor);
                }
                break;
            case FlowFieldVisulizeType.IntegrationHeatMap:
                float maxBestCost = cells.SecondMaxTempCost();

                for (int i = 0; i < cells.Length; i++)
                {
                    Color drawColor = cells[i].integrationCost >= Constants.T_i ? Color.black : Color.HSVToRGB((1 - cells[i].integrationCost / maxBestCost) / 3, 1, 1);
                    var height = math.min(cells[i].integrationCost / maxBestCost, 1);
                    drawPos = cells[i].worldPos;
                    drawPos.y += height / 2;
                    drawSize.y = height;
                    if (dests.Contains(i))
                    {
                        builder.WireBox(drawPos + displayOffset, drawSize, Color.blue);
                    }
                    builder.SolidBox(drawPos + displayOffset, drawSize, drawColor);
                }
                break;
            case FlowFieldVisulizeType.GlobalFlowField:
                foreach (var cell in cells)
                {
                    if (dests.Contains(FlowFieldUtility.ToFlatIndex(cell.gridIndex, gridSize.y)))
                    {
                        //等于0的情况有两种，目标网格和不可行网格
                        builder.CircleXZ(cell.worldPos + displayOffset, cellRadius.x * 0.8f, Color.yellow);
                    }
                    else if (cell.globalDir.Equals(float2.zero)) builder.DrawCross45(cell.worldPos + displayOffset, cellRadius * 0.8f, Color.red);
                    else
                    {
                        var dir = math.normalize(new float3(cell.globalDir.x, 0, cell.globalDir.y));
                        var halfLength = cellRadius.x * 0.8f;
                        var originPos = cell.worldPos + displayOffset;
                        builder.Arrow(originPos - halfLength * dir, originPos + halfLength * dir, Color.black);
                    }
                }
                break;
            case FlowFieldVisulizeType.LocalFlowField:
                foreach (var cell in cells)
                {
                    if (dests.Contains(FlowFieldUtility.ToFlatIndex(cell.gridIndex, gridSize.y)))
                    {
                        //等于0的情况有两种，目标网格和不可行网格
                        builder.CircleXZ(cell.worldPos + displayOffset, cellRadius.x * 0.8f, Color.yellow);
                    }
                    else if (cell.localDir.Equals(float2.zero)) builder.DrawCross45(cell.worldPos + displayOffset, cellRadius * 0.8f, Color.red);
                    else
                    {
                        var temp = new float3(cell.localDir.x, 0, cell.localDir.y);
                        float3 dir = float3.zero;
                        if (math.lengthsq(temp) >= 1)
                        {
                            dir = math.normalizesafe(temp);
                        }
                        else
                        {
                            dir = temp;
                        }
                        var halfLength = cellRadius.x * 0.8f;
                        var originPos = cell.worldPos + displayOffset;
                        builder.Arrow(originPos - halfLength * dir, originPos + halfLength * dir, Color.black);
                    }
                }
                break;
            // case FlowFieldVisulizeType.TargetField:
            //     foreach (var cell in cells)
            //     {
            //         if (cell.integrationCost == 0) builder.CircleXZ(cell.worldPos + displayOffset, cellRadius.x * 0.8f, Color.yellow);
            //         // else if (cell.bestDirection.Equals(GridDirection.None)) drawCross45(draw, cell.worldPos + heightOffset, setting.cellRadius * 0.8f, Color.red);
            //         else if (cell.globalBestDir.Equals(float2.zero)) builder.DrawCross45(cell.worldPos + displayOffset, cellRadius * 0.8f, Color.red);
            //         else
            //         {
            //             var tdir = math.normalize(new float3(cell.targetDir.x, 0, cell.targetDir.y));
            //             var thalfLength = cellRadius.x * 0.8f;
            //             var toriginPos = cell.worldPos + displayOffset;
            //             builder.Arrow(toriginPos - thalfLength * tdir, toriginPos + thalfLength * tdir, Color.red);
            //         }
            //     }
            //     break;
            // case FlowFieldVisulizeType.DebugField1:

            //     builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
            //     // data.ForEach(cell => draw.Label2D(cell.worldPos + heightOffset, cell.bestCost == ushort.MaxValue ? "M" : cell.bestCost.ToString(), 40, LabelAlignment.Center));
            //     // 角度差异
            //     // foreach (var cell in cells)
            //     // {
            //     // FixedString32Bytes valueString = $"{(int)Vector2.Angle(cell.bestDir, cell.targetDir)}";
            //     //     builder.Label2D(cell.worldPos + heightOffset, ref valueString, 40, LabelAlignment.Center);
            //     // }

            //     foreach (var cell in cells)
            //     {
            //         FixedString32Bytes valueString = $"{(int)cell.debugField.x}";
            //         builder.Label2D(cell.worldPos + heightOffset, ref valueString, 40, LabelAlignment.Center);
            //     }

            //     // 距离差异
            //     // data.ForEach(cell => draw.Label2D(cell.worldPos + heightOffset, $"{(cell.targetCost):0.00}", 40, LabelAlignment.Center));
            //     break;
            // case FlowFieldVisulizeType.DebugField2:
            //     builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
            //     foreach (var cell in cells)
            //     {
            //         FixedString32Bytes valueString = $"{(int)cell.debugField.y}";
            //         builder.Label2D(cell.worldPos + heightOffset, ref valueString, 40, LabelAlignment.Center);
            //     }
            //     break;
            case FlowFieldVisulizeType.DebugField3:
                // builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
                // foreach (var cell in cells)
                // {
                //     FixedString32Bytes valueString = $"{(int)cell.debugField.z}";
                //     builder.Label2D(cell.worldPos + heightOffset, ref valueString, 40, LabelAlignment.Center);
                // }

                break;
            default:
                break;
        }
        builder.PopLineWidth();
    }
}
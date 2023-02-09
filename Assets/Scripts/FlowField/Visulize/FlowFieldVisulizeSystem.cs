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
    private float3 gridSetSize, displayOffset, center;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.AddComponentData<FFVisTypeStateData>(state.SystemHandle, new FFVisTypeStateData { ffVisType = FlowFieldVisulizeType.None });
        gridSetSize = displayOffset = center = 0;
        state.RequireForUpdate<FlowFieldSettingData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state) { }

    // [BurstCompile]
    // DrawingManager.GetBuilder 为 managed mathod, 不可 BurstCompile
    public void OnUpdate(ref SystemState state)
    {
        var ffVisType = SystemAPI.GetComponent<FFVisTypeStateData>(state.SystemHandle).ffVisType;
        if (ffVisType.Equals(FlowFieldVisulizeType.None)) return;

        var cells = SystemAPI.GetSingletonBuffer<CellBuffer>(true).Reinterpret<CellData>().AsNativeArray();
        var setting = SystemAPI.GetSingleton<FlowFieldSettingData>();
        if (gridSetSize.Equals(float3.zero)) gridSetSize = new float3(setting.cellRadius.x * 2 * setting.gridSetSize.x, 0, setting.cellRadius.z * 2 * setting.gridSetSize.y);
        if (!displayOffset.Equals(setting.displayOffset))
        {
            displayOffset = setting.displayOffset;
            center = setting.originPoint + gridSetSize / 2 + displayOffset;
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
            gridSetSize = gridSetSize,
            heightOffset = displayOffset,
            center = center,
            cells = cells,
            builder = builder,
            rotation = quaternion.Euler(setting.rotation)
        }.Schedule();

        builder.DisposeAfter(drawJob);
        drawJob.Complete();
    }
}

[BurstCompile]
public struct DrawFlowFieldJob : IJob
{
    [ReadOnly] public FlowFieldVisulizeType ffVisType;
    [ReadOnly] public int2 gridSize;
    [ReadOnly] public float3 gridSetSize, heightOffset, center, cellRadius;
    [ReadOnly] public NativeArray<CellData> cells;
    public CommandBuilder builder;

    [ReadOnly] public quaternion rotation;

    public void Execute()
    {
        builder.PushLineWidth(2f);
        switch (ffVisType)
        {
            case FlowFieldVisulizeType.Grid:
                builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
                break;
            case FlowFieldVisulizeType.CostField:
                builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
                foreach (var cell in cells)
                {
                    FixedString32Bytes valueString = cell.cost == 255 ? "M" : $"{cell.cost}";
                    builder.Label3D(cell.worldPos + heightOffset, quaternion.Euler(1.57f, 1.57f, 0), ref valueString, 0.5f, LabelAlignment.Center);
                }
                break;
            case FlowFieldVisulizeType.IntegrationField:
                builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
                foreach (var cell in cells)
                {
                    FixedString32Bytes valueString = cell.tempCost == float.MaxValue ? "M" : $"{(int)cell.tempCost}";
                    builder.Label3D(cell.worldPos + heightOffset, quaternion.Euler(1.57f, 1.57f, 0), ref valueString, 0.4f, LabelAlignment.Center);
                    // builder.Label2D(cell.worldPos + heightOffset,  ref valueString, 40, LabelAlignment.Center);
                }
                break;
            case FlowFieldVisulizeType.CostHeatMap:
                //https://stackoverflow.com/questions/10901085/range-values-to-pseudocolor
                float maxCost = cells.SecondMaxCost();

                foreach (var cell in cells)
                {
                    float costHeat = (maxCost - cell.cost) / maxCost;
                    Color drawColor = cell.cost == 255 ? Color.black : Color.HSVToRGB(costHeat / 3, 1, 1);
                    drawColor = cell.cost == 0 ? Color.blue : drawColor;
                    var height = cell.cost == 255 ? 1 : cell.cost / maxCost;
                    var tempPos = cell.worldPos;
                    tempPos.y += height / 2;
                    var tempSize = cellRadius * 2;
                    tempSize.y = height;
                    builder.SolidBox(tempPos + heightOffset, tempSize, drawColor);
                }
                break;
            case FlowFieldVisulizeType.IntegrationHeatMap:
                // float maxBestCost = data.Where(i => i.bestCost != ushort.MaxValue).Max(i => i.bestCost);

                // foreach (var cell in data)
                // {
                //     float intHeat = (maxBestCost - cell.bestCost) / maxBestCost;
                //     Color drawColor = cell.bestCost == ushort.MaxValue ? Color.black : Color.HSVToRGB(intHeat / 3, 1, 1);
                //     drawColor = cell.bestCost == 0 ? Color.blue : drawColor;
                //     var height = cell.bestCost == ushort.MaxValue ? 1 : cell.bestCost / maxBestCost;
                //     var tempPos = cell.worldPos;
                //     tempPos.y += height / 2;
                //     var tempSize = setting.cellRadius * 2;
                //     tempSize.y = height;
                //     draw.SolidBox(tempPos + heightOffset, tempSize, drawColor);
                // }
                float maxBestCost = cells.SecondMaxTempCost();

                foreach (var cell in cells)
                {
                    float intHeat = (maxBestCost - cell.tempCost) / maxBestCost;
                    Color drawColor = cell.tempCost == float.MaxValue ? Color.black : Color.HSVToRGB(intHeat / 3, 1, 1);
                    drawColor = cell.tempCost == 0 ? Color.blue : drawColor;
                    var height = cell.tempCost == float.MaxValue ? 1 : cell.tempCost / maxBestCost;
                    var tempPos = cell.worldPos;
                    tempPos.y += height / 2;
                    var tempSize = cellRadius * 2;
                    tempSize.y = height;
                    builder.SolidBox(tempPos + heightOffset, tempSize, drawColor);
                }
                break;
            case FlowFieldVisulizeType.FlowField:
                foreach (var cell in cells)
                {
                    if (cell.tempCost == 0) builder.CircleXZ(cell.worldPos + heightOffset, cellRadius.x * 0.8f, Color.yellow);
                    // else if (cell.bestDirection.Equals(GridDirection.None)) drawCross45(draw, cell.worldPos + heightOffset, setting.cellRadius * 0.8f, Color.red);
                    else if (cell.bestDir.Equals(float2.zero)) builder.drawCross45(cell.worldPos + heightOffset, cellRadius * 0.8f, Color.red);
                    else
                    {
                        var dir = math.normalize(new float3(cell.bestDir.x, 0, cell.bestDir.y));
                        var halfLength = cellRadius.x * 0.8f;
                        var originPos = cell.worldPos + heightOffset;
                        builder.Arrow(originPos - halfLength * dir, originPos + halfLength * dir, Color.black);

                        var tdir = math.normalize(new float3(cell.targetDir.x, 0, cell.targetDir.y));
                        var thalfLength = cellRadius.x * 0.8f;
                        var toriginPos = cell.worldPos + heightOffset;
                        builder.Arrow(toriginPos - thalfLength * tdir, toriginPos + thalfLength * tdir, Color.red);
                    }
                }
                break;
            case FlowFieldVisulizeType.DebugField:

                builder.WireGrid(center, Quaternion.identity, gridSize, gridSetSize.xz, Color.black);
                // data.ForEach(cell => draw.Label2D(cell.worldPos + heightOffset, cell.bestCost == ushort.MaxValue ? "M" : cell.bestCost.ToString(), 40, LabelAlignment.Center));
                // 角度差异
                foreach (var cell in cells)
                {
                    FixedString32Bytes valueString = $"{(int)Vector2.Angle(cell.bestDir, cell.targetDir)}";
                    builder.Label2D(cell.worldPos + heightOffset, ref valueString, 40, LabelAlignment.Center);
                }

                // 距离差异
                // data.ForEach(cell => draw.Label2D(cell.worldPos + heightOffset, $"{(cell.targetCost):0.00}", 40, LabelAlignment.Center));
                break;
            default:
                break;
        }
        builder.PopLineWidth();
    }
}
using Unity.Entities;
using Drawing;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using Unity.Collections;
using System.Collections.Generic;

public enum FlowFieldDisplayType { None, Grid, CostField, IntegrationField, CostHeatMap, IntegrationHeatMap, FlowField };
public class FlowFieldDebugSystem : SystemBase
{
    public FlowFieldDisplayType _curDisplayType;

    private float3 _gridSize;

    private List<CellData> data;

    private float3 drawOffset;

    protected override void OnCreate()
    {
        data = new List<CellData>();
        drawOffset = new float3(0, 2f, 0);
    }
    protected override void OnUpdate()
    {
        var settingComponent = GetSingleton<FlowFieldSettingData>();

        if (_gridSize.Equals(float3.zero)) _gridSize = new float3(settingComponent.cellRadius.x * 2 * settingComponent.gridSize.x, 0, settingComponent.cellRadius.z * 2 * settingComponent.gridSize.y);

        using (var draw = DrawingManager.GetBuilder(true))
        {
            draw.PushLineWidth(2f);
            switch (_curDisplayType)
            {
                case FlowFieldDisplayType.CostField:
                    draw.WireGrid(settingComponent.originPoint + _gridSize / 2 + drawOffset, Quaternion.identity, settingComponent.gridSize, _gridSize.xz, Color.black);
                    data.ForEach(cell => draw.Label2D(cell.worldPos + drawOffset, cell.cost == 255 ? "M" : cell.cost.ToString(), 50, LabelAlignment.Center));
                    break;
                case FlowFieldDisplayType.IntegrationField:
                    draw.WireGrid(settingComponent.originPoint + _gridSize / 2 + drawOffset, Quaternion.identity, settingComponent.gridSize, _gridSize.xz, Color.black);
                    data.ForEach(cell => draw.Label2D(cell.worldPos + drawOffset, cell.bestCost == ushort.MaxValue ? "M" : cell.bestCost.ToString(), 40, LabelAlignment.Center));
                    break;
                case FlowFieldDisplayType.CostHeatMap:
                    //https://stackoverflow.com/questions/10901085/range-values-to-pseudocolor
                    float maxCost = data.Where(i => i.cost != 255).Max(i => i.cost);

                    foreach (var cell in data)
                    {
                        float costHeat = (maxCost - cell.cost) / maxCost;
                        Color drawColor = cell.cost == 255 ? Color.black : Color.HSVToRGB(costHeat / 3, 1, 1);
                        drawColor = cell.cost == 0 ? Color.blue : drawColor;
                        var height = cell.cost == 255 ? 1 : cell.cost / maxCost;
                        var tempPos = cell.worldPos;
                        tempPos.y = height / 2;
                        var tempSize = settingComponent.cellRadius * 2;
                        tempSize.y = height;
                        draw.SolidBox(tempPos + drawOffset, tempSize, drawColor);
                        // draw.SolidPlane(cell.worldPos + drawOffset, math.up(), settingComponent.cellRadius.xz * 2, drawColor);
                    }
                    break;
                case FlowFieldDisplayType.IntegrationHeatMap:
                    float maxBestCost = data.Where(i => i.bestCost != ushort.MaxValue).Max(i => i.bestCost);

                    foreach (var cell in data)
                    {
                        float intHeat = (maxBestCost - cell.bestCost) / maxBestCost;
                        Color drawColor = cell.bestCost == ushort.MaxValue ? Color.black : Color.HSVToRGB(intHeat / 3, 1, 1);
                        drawColor = cell.bestCost == 0 ? Color.blue : drawColor;
                        var height = cell.bestCost == ushort.MaxValue ? 1 : cell.bestCost / maxBestCost;
                        var tempPos = cell.worldPos;
                        tempPos.y = height / 2;
                        var tempSize = settingComponent.cellRadius * 2;
                        tempSize.y = height;
                        draw.SolidBox(tempPos + drawOffset, tempSize, drawColor);
                        // draw.SolidPlane(cell.worldPos + drawOffset, math.up(), settingComponent.cellRadius.xz * 2, drawColor);
                    }
                    break;
                case FlowFieldDisplayType.FlowField:
                    foreach (var cell in data)
                    {
                        if (cell.cost == 0) draw.CircleXZ(cell.worldPos + drawOffset, settingComponent.cellRadius.x * 0.8f, Color.yellow);
                        else if (cell.bestDirection.Equals(GridDirection.None)) drawCross45(draw, cell.worldPos + drawOffset, settingComponent.cellRadius * 0.8f, Color.red);
                        // else draw.Arrowhead(cell.worldPos + drawOffset, new float3(cell.bestDirection.x, 0, cell.bestDirection.y), 0.2f, Color.blue);
                        else
                        {
                            var dir = math.normalize(new float3(cell.bestDirection.x, 0, cell.bestDirection.y));
                            var halfLength = settingComponent.cellRadius.x * 0.8f;
                            var originPos = cell.worldPos + drawOffset;
                            draw.Arrow(originPos - halfLength * dir, originPos + halfLength * dir, Color.black);
                        }
                    }
                    break;
                case FlowFieldDisplayType.Grid:
                    draw.WireGrid(settingComponent.originPoint + _gridSize / 2 + drawOffset, Quaternion.identity, settingComponent.gridSize, _gridSize.xz, Color.black);
                    break;
                default:
                    break;
            }
            draw.PopLineWidth();

            // Ruler Display for HeatMap
            // Color _drawColor;
            // for (int i = 0; i <= 100; i += 1)
            // {
            //     if (i == 0) _drawColor = Color.blue;
            //     else if (i == 100) _drawColor = Color.black;
            //     else _drawColor = Color.HSVToRGB((1 - i / 100f) / 3, 1, 1);
            //     draw.SolidPlane(new float3(20, 0, i / 10f), math.up(), new float2(1, 0.1f), _drawColor);
            // }
        }
    }

    void drawCross45(CommandBuilder builder, float3 position, float3 size, Color color)
    {
        builder.PushColor(color);
        builder.Line(position - new float3(size.x, 0, size.z), position + new float3(size.x, 0, size.z));
        builder.Line(position - new float3(size.x, 0, -size.z), position + new float3(size.x, 0, -size.z));
        builder.PopColor();
    }

    public void UpdateData() => data = GetBuffer<CellBufferElement>(GetSingletonEntity<FlowFieldSettingData>()).Reinterpret<CellData>().AsNativeArray().ToList();
}
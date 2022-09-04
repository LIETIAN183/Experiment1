using Unity.Entities;
using Drawing;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System.Collections.Generic;

public enum FlowFieldDisplayType { None, Grid, CostField, IntegrationField, CostHeatMap, IntegrationHeatMap, FlowField };
public partial class FlowFieldVisulizeSystem : SystemBase
{
    public FlowFieldDisplayType _curDisplayType;

    private float3 gridLength, heightOffset, center;

    private List<CellData> data;

    protected override void OnStartRunning()
    {
        data = new List<CellData>();
        var setting = GetSingleton<FlowFieldSettingData>();
        gridLength = new float3(setting.cellRadius.x * 2 * setting.gridSize.x, 0, setting.cellRadius.z * 2 * setting.gridSize.y);
        heightOffset = setting.displayHeightOffset;
        center = setting.originPoint + gridLength / 2 + heightOffset;
        _curDisplayType = FlowFieldDisplayType.None;
    }

    protected override void OnUpdate()
    {
        var setting = GetSingleton<FlowFieldSettingData>();
        heightOffset = setting.displayHeightOffset;
        center = setting.originPoint + gridLength / 2 + heightOffset;

        using (var draw = DrawingManager.GetBuilder(true))
        {
            draw.PushLineWidth(2f);
            switch (_curDisplayType)
            {
                case FlowFieldDisplayType.Grid:
                    draw.WireGrid(center, Quaternion.identity, setting.gridSize, gridLength.xz, Color.black);
                    break;
                case FlowFieldDisplayType.CostField:
                    draw.WireGrid(center, Quaternion.identity, setting.gridSize, gridLength.xz, Color.black);
                    data.ForEach(cell => draw.Label2D(cell.worldPos + heightOffset, cell.cost == 255 ? "M" : cell.cost.ToString(), 50, LabelAlignment.Center));
                    break;
                case FlowFieldDisplayType.IntegrationField:
                    draw.WireGrid(center, Quaternion.identity, setting.gridSize, gridLength.xz, Color.black);
                    data.ForEach(cell => draw.Label2D(cell.worldPos + heightOffset, cell.bestCost == ushort.MaxValue ? "M" : cell.bestCost.ToString(), 40, LabelAlignment.Center));
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
                        tempPos.y += height / 2;
                        var tempSize = setting.cellRadius * 2;
                        tempSize.y = height;
                        draw.SolidBox(tempPos + heightOffset, tempSize, drawColor);
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
                        tempPos.y += height / 2;
                        var tempSize = setting.cellRadius * 2;
                        tempSize.y = height;
                        draw.SolidBox(tempPos + heightOffset, tempSize, drawColor);
                    }
                    break;
                case FlowFieldDisplayType.FlowField:
                    foreach (var cell in data)
                    {
                        if (cell.cost == 0) draw.CircleXZ(cell.worldPos + heightOffset, setting.cellRadius.x * 0.8f, Color.yellow);
                        else if (cell.bestDirection.Equals(GridDirection.None)) drawCross45(draw, cell.worldPos + heightOffset, setting.cellRadius * 0.8f, Color.red);
                        else
                        {
                            var dir = math.normalize(new float3(cell.bestDirection.x, 0, cell.bestDirection.y));
                            var halfLength = setting.cellRadius.x * 0.8f;
                            var originPos = cell.worldPos + heightOffset;
                            draw.Arrow(originPos - halfLength * dir, originPos + halfLength * dir, Color.black);
                        }
                    }
                    break;
                default:
                    break;
            }
            draw.PopLineWidth();
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
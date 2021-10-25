using Unity.Entities;
using Drawing;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using Unity.Collections;
using System.Collections.Generic;

public enum FlowFieldDisplayType { None, CostField, IntegrationField, CostHeatMap, IntegrationHeatMap, FlowField };
public class FlowFieldDebugSystem : SystemBase
{
    public FlowFieldDisplayType _curDisplayType;

    private float3 _gridSize;

    private List<CellData> data;

    private float3 drawOffset;

    protected override void OnCreate()
    {
        data = new List<CellData>();
        drawOffset = new float3(0, 0, 0);
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
                    data.ForEach(cell => draw.Label2D(cell.worldPos + drawOffset, cell.cost.ToString(), 50, LabelAlignment.Center));
                    break;
                case FlowFieldDisplayType.IntegrationField:
                    draw.WireGrid(settingComponent.originPoint + _gridSize / 2 + drawOffset, Quaternion.identity, settingComponent.gridSize, _gridSize.xz, Color.black);
                    data.ForEach(cell => draw.Label2D(cell.worldPos + drawOffset, cell.bestCost.ToString(), 40, LabelAlignment.Center));
                    break;
                case FlowFieldDisplayType.CostHeatMap:
                    //https://stackoverflow.com/questions/10901085/range-values-to-pseudocolor
                    float maxCost = data.Where(i => i.cost != 255).Max(i => i.cost);

                    foreach (var cell in data)
                    {
                        float costHeat = (maxCost - cell.cost) / maxCost;
                        Color drawColor = cell.cost == 255 ? Color.black : Color.HSVToRGB(costHeat / 3, 1, 1);
                        draw.SolidPlane(cell.worldPos + drawOffset, math.up(), settingComponent.cellRadius.xz * 2, drawColor);
                    }
                    break;
                case FlowFieldDisplayType.IntegrationHeatMap:
                    float maxBestCost = data.Where(i => i.bestCost != ushort.MaxValue).Max(i => i.bestCost);

                    foreach (var cell in data)
                    {
                        float intHeat = (maxBestCost - cell.bestCost) / maxBestCost;
                        Color drawColor = cell.bestCost == ushort.MaxValue ? Color.black : Color.HSVToRGB(intHeat / 3, 1, 1);
                        draw.SolidPlane(cell.worldPos + drawOffset, math.up(), settingComponent.cellRadius.xz * 2, drawColor);
                    }
                    break;
                case FlowFieldDisplayType.FlowField:
                    foreach (var cell in data)
                    {
                        if (cell.cost == 0) draw.CircleXZ(cell.worldPos, settingComponent.cellRadius.x, Color.yellow);
                        else if (cell.bestDirection.Equals(GridDirection.None)) drawCross45(draw, cell.worldPos + drawOffset, settingComponent.cellRadius, Color.red);
                        else draw.Arrowhead(cell.worldPos, new float3(cell.bestDirection.x, 0, cell.bestDirection.y), 0.2f, Color.blue);
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
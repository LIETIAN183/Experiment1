using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;

// Fast Marching Method
public static class FMMExtension { }


public enum State { Dead, Open, Far };
// [BurstCompile]
public struct CalCulateIntegration_FMMJob : IJob
{
    public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;

    public void Execute()
    {
        var gridSetSize = settingData.gridSetSize;

        var destinationIndex = FlowFieldUtility.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSetSize, settingData.cellRadius * 2);
        // Update Destination Cell's cost and bestCost
        int flatDestinationIndex = FlowFieldUtility.ToFlatIndex(destinationIndex, gridSetSize.y);
        CellData destinationCell = cells[flatDestinationIndex];
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;
        destinationCell.tempCost = 0;
        // destinationCell.state = State.Open;
        cells[flatDestinationIndex] = destinationCell;

        List<State> stateList = new List<State>(cells.Length);

        for (int i = 0; i < cells.Length; i++)
        {
            stateList.Add(State.Far);
        }
        stateList[flatDestinationIndex] = State.Open;

        List<CellData> sortOpenList = new List<CellData>();
        List<int2> deadList = new List<int2>();
        sortOpenList.Add(destinationCell);
        while (sortOpenList.Count > 0)
        {

            sortOpenList = sortOpenList.OrderByDescending(i => i.tempCost).ToList();
            var current = sortOpenList.Last();
            sortOpenList.RemoveAt(sortOpenList.Count - 1);

            deadList.Add(current.gridIndex);
            // current.state = State.Dead;
            var currentIndex = FlowFieldUtility.ToFlatIndex(current.gridIndex, gridSetSize.y);
            stateList[currentIndex] = State.Dead;

            // cells[currentIndex] = current;

            foreach (int2 neighborIndex in FlowFieldUtility.GetBesideIndices(current.gridIndex, gridSetSize))
            {
                int flatNeighborIndex = FlowFieldUtility.ToFlatIndex(neighborIndex, gridSetSize.y);
                CellData neighborCellData = cells[flatNeighborIndex];

                var neighborState = stateList[flatNeighborIndex];
                if (neighborState == State.Dead) continue;
                else if (neighborState == State.Far)
                {
                    stateList[flatNeighborIndex] = State.Open;
                }
                else
                {
                    sortOpenList.Remove(neighborCellData);
                }

                var northIndex = FlowFieldUtility.GetIndexAtRelativePosition(neighborIndex, GridDirection.North, gridSetSize);
                var southIndex = FlowFieldUtility.GetIndexAtRelativePosition(neighborIndex, GridDirection.South, gridSetSize);
                var eastIndex = FlowFieldUtility.GetIndexAtRelativePosition(neighborIndex, GridDirection.East, gridSetSize);
                var westIndex = FlowFieldUtility.GetIndexAtRelativePosition(neighborIndex, GridDirection.West, gridSetSize);
                float t1, t2;
                if (northIndex.x < 0)
                {
                    t1 = cells[FlowFieldUtility.ToFlatIndex(southIndex, gridSetSize.y)].tempCost;
                }
                else if (southIndex.x < 0)
                {
                    t1 = cells[FlowFieldUtility.ToFlatIndex(northIndex, gridSetSize.y)].tempCost;
                }
                else
                {
                    t1 = math.min(cells[FlowFieldUtility.ToFlatIndex(southIndex, gridSetSize.y)].tempCost, cells[FlowFieldUtility.ToFlatIndex(northIndex, gridSetSize.y)].tempCost);
                }


                if (eastIndex.x < 0)
                {
                    t2 = cells[FlowFieldUtility.ToFlatIndex(westIndex, gridSetSize.y)].tempCost;
                }
                else if (westIndex.x < 0)
                {
                    t2 = cells[FlowFieldUtility.ToFlatIndex(eastIndex, gridSetSize.y)].tempCost;
                }
                else
                {
                    t2 = math.min(cells[FlowFieldUtility.ToFlatIndex(westIndex, gridSetSize.y)].tempCost, cells[FlowFieldUtility.ToFlatIndex(eastIndex, gridSetSize.y)].tempCost);
                }

                float t;
                if (math.abs(t1 - t2) < 0.5f)
                {
                    t = (t1 + t2 + math.sqrt(0.5f - (t1 - t2) * (t1 - t2))) / 2f;
                }
                else
                {
                    t = math.min(t1, t2) + 0.5f;
                }

                neighborCellData.tempCost = math.min(neighborCellData.tempCost, t);
                cells[FlowFieldUtility.ToFlatIndex(neighborCellData.gridIndex, gridSetSize.y)] = neighborCellData;
                sortOpenList.Add(neighborCellData);
            }
        }
    }
}
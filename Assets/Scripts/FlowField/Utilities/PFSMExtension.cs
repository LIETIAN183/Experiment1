using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// https://www.sciencedirect.com/science/article/pii/S002199911200722X
[BurstCompile]
public static class PFSMExtension
{
    // [BurstCompile]
    public static JobHandle CalculateIntegration_PFSM(JobHandle inputDependency, NativeArray<CellData> cells, FlowFieldSettingData settingData)
    {
        var updateJob = new UpdateDestinationJob()
        {
            cells = cells,
            settingData = settingData
        }.Schedule(inputDependency);

        int height = settingData.gridSetSize.x, width = settingData.gridSetSize.y;
        // double h = 0.5, f = 1.0;

        //  左下 level = i+j
        // int[I][J]  // I行J列
        // for level=0:I+J-2 do  step=1
        // I1 = max(0,level-J+1)
        // I2 = min(I-1,level)
        // parallel for i = I1:I2 do
        // j = level - i;
        // udpate value
        JobHandle firstLoopJobHandle = new JobHandle();
        for (int level = 0; level <= height + width - 2; level += 1)
        {
            int start = math.max(0, level - width + 1);
            int end = math.min(height - 1, level);
            var parallelFSPJob = new ParallelFastSweepingMethodJob()
            {
                cells = cells,
                gridSetSize = settingData.gridSetSize,
                level = level,
                start = start,
                type = 1
            }.Schedule(end - start + 1, 64, updateJob);
            firstLoopJobHandle = JobHandle.CombineDependencies(firstLoopJobHandle, parallelFSPJob);
        }

        // 右下 level = i-j
        // for level = -(J-1):I-1 do step=1
        // I1= max(0,level)
        // I2= min(I-1,J-1+level)
        // parallel for i=I1:I2 do
        // j=i-level
        // update value
        JobHandle secondLoopJobHandle = new JobHandle();
        for (int level = -width + 1; level <= height - 1; level += 1)
        {
            int start = math.max(0, level);
            int end = math.min(height - 1, level + width - 1);
            var parallelFSPJob = new ParallelFastSweepingMethodJob()
            {
                cells = cells,
                gridSetSize = settingData.gridSetSize,
                level = level,
                start = start,
                type = -1
            }.Schedule(end - start + 1, 64, firstLoopJobHandle);
            secondLoopJobHandle = JobHandle.CombineDependencies(secondLoopJobHandle, parallelFSPJob);
        }

        // 左上 level = i-j
        // for level = I-1:-(J-1) do step = -1
        // I1 = max(0,level)
        // I2 = min(I-1,J-1+level)
        // parallel for i=I1:I2 do
        // j=i-level
        // update value
        JobHandle thirdLoopJobHandle = new JobHandle();
        for (int level = height - 1; level >= -width + 1; level += -1)
        {
            int start = math.max(0, level);
            int end = math.min(height - 1, level + width - 1);
            var parallelFSPJob = new ParallelFastSweepingMethodJob()
            {
                cells = cells,
                gridSetSize = settingData.gridSetSize,
                level = level,
                start = start,
                type = -1
            }.Schedule(end - start + 1, 64, secondLoopJobHandle);
            thirdLoopJobHandle = JobHandle.CombineDependencies(thirdLoopJobHandle, parallelFSPJob);
        }

        // 右上 level = i+j
        // for level=I+J-2:0 do  step=-1
        // I1 = max(0,level-J+1)
        // I2 = min(I-1,level)
        // parallel for i = I1:I2 do
        // j = level - i;
        // udpate value
        JobHandle forthLoopJobHandle = new JobHandle();
        for (int level = height + width - 2; level >= 0; level += -1)
        {
            int start = math.max(0, level - width + 1);
            int end = math.min(height - 1, level);
            var parallelFSPJob = new ParallelFastSweepingMethodJob()
            {
                cells = cells,
                gridSetSize = settingData.gridSetSize,
                level = level,
                start = start,
                type = 1
            }.Schedule(end - start + 1, 64, thirdLoopJobHandle);
            forthLoopJobHandle = JobHandle.CombineDependencies(forthLoopJobHandle, parallelFSPJob);
        }
        return forthLoopJobHandle;
    }
}

[BurstCompile]
public struct ParallelFastSweepingMethodJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public int2 gridSetSize;

    [ReadOnly] public int level, start, type;// type=1=>level=i+j;type=-1=>level=i-j

    public void Execute(int index)
    {
        var i = index + start;
        var j = (level - i) * type;

        var currentIndex = FlowFieldUtility.ToFlatIndex(i, j, gridSetSize.y);
        var current = cells[currentIndex];
        float2 midValue = new float2(0, 0);

        if (i == 0 || i == (gridSetSize.x - 1))
        {
            if (i == 0)
            {
                midValue.y = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i + 1, j, gridSetSize.y)].tempCost);
            }
            if (i == (gridSetSize.x - 1))
            {
                midValue.y = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i - 1, j, gridSetSize.y)].tempCost);
            }
        }
        else
        {
            midValue.y = midValue.y = math.min(cells[FlowFieldUtility.ToFlatIndex(i - 1, j, gridSetSize.y)].tempCost, cells[FlowFieldUtility.ToFlatIndex(i + 1, j, gridSetSize.y)].tempCost);
        }

        if (j == 0 || j == (gridSetSize.y - 1))
        {
            if (j == 0)
            {
                midValue.x = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i, j + 1, gridSetSize.y)].tempCost);
            }
            if (j == (gridSetSize.y - 1))
            {
                midValue.x = math.min(current.tempCost, cells[FlowFieldUtility.ToFlatIndex(i, j - 1, gridSetSize.y)].tempCost);
            }
        }
        else
        {
            midValue.x = math.min(cells[FlowFieldUtility.ToFlatIndex(i, j - 1, gridSetSize.y)].tempCost, cells[FlowFieldUtility.ToFlatIndex(i, j + 1, gridSetSize.y)].tempCost);
        }

        float temp;
        if (math.abs(midValue.x - midValue.y) < 0.5f * current.cost)
        {
            temp = (midValue.x + midValue.y + math.sqrt(0.5f * current.cost * current.cost - (midValue.x - midValue.y) * (midValue.x - midValue.y))) * 0.5f;
        }
        else
        {
            temp = math.min(midValue.x, midValue.y) + 0.5f;
        }

        current.tempCost = math.min(temp, current.tempCost);
        cells[currentIndex] = current;
    }
}

[BurstCompile]
public struct UpdateDestinationJob : IJob
{
    public NativeArray<CellData> cells;
    [ReadOnly] public FlowFieldSettingData settingData;

    public void Execute()
    {
        var gridSize = settingData.gridSetSize;
        var destinationIndex = FlowFieldUtility.GetCellIndexFromWorldPos(settingData.destination, settingData.originPoint, gridSize, settingData.cellRadius * 2);
        // Update Destination Cell's cost and bestCost
        int flatDestinationIndex = FlowFieldUtility.ToFlatIndex(destinationIndex, gridSize.y);
        CellData destinationCell = cells[flatDestinationIndex];
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;
        destinationCell.tempCost = 0;
        cells[flatDestinationIndex] = destinationCell;
    }
}
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
        NativeQueue<byte> queue = new NativeQueue<byte>(Allocator.Persistent);
        int height = settingData.gridSetSize.x, width = settingData.gridSetSize.y;
        float widthInM = settingData.cellRadius.x * 2, twoWidth2 = settingData.cellRadius.x * settingData.cellRadius.x * 8;

        uint count = 0;
        do
        {
            count++;
            queue.Clear();

            // double h = 0.5, f = 1.0;

            //  左下 level = i+j
            // int[I][J]  // I行J列
            // for level=0:I+J-2 do  step=1
            // I1 = max(0,level-J+1)
            // I2 = min(I-1,level)
            // parallel for i = I1:I2 do
            // j = level - i;
            // udpate value
            for (int level = 0; level <= height + width - 2; level += 1)
            {
                int start = math.max(0, level - width + 1);
                int end = math.min(height - 1, level);
                inputDependency = new ParallelFastSweepingMethodJob()
                {
                    cells = cells,
                    gridSetSize = settingData.gridSetSize,
                    level = level,
                    start = start,
                    type = 1,
                    width = widthInM,
                    twoWidth2 = twoWidth2,
                    queueWriter = queue.AsParallelWriter()
                }.Schedule(end - start + 1, (end - start + 1) / 4, inputDependency);
            }

            // 右下 level = i-j
            // for level = -(J-1):I-1 do step=1
            // I1= max(0,level)
            // I2= min(I-1,J-1+level)
            // parallel for i=I1:I2 do
            // j=i-level
            // update value
            for (int level = -width + 1; level <= height - 1; level += 1)
            {
                int start = math.max(0, level);
                int end = math.min(height - 1, level + width - 1);
                inputDependency = new ParallelFastSweepingMethodJob()
                {
                    cells = cells,
                    gridSetSize = settingData.gridSetSize,
                    level = level,
                    start = start,
                    type = -1,
                    width = widthInM,
                    twoWidth2 = twoWidth2,
                    queueWriter = queue.AsParallelWriter()
                }.Schedule(end - start + 1, (end - start + 1) / 4, inputDependency);
            }

            // 左上 level = i-j
            // for level = I-1:-(J-1) do step = -1
            // I1 = max(0,level)
            // I2 = min(I-1,J-1+level)
            // parallel for i=I1:I2 do
            // j=i-level
            // update value
            for (int level = height - 1; level >= -width + 1; level += -1)
            {
                int start = math.max(0, level);
                int end = math.min(height - 1, level + width - 1);
                inputDependency = new ParallelFastSweepingMethodJob()
                {
                    cells = cells,
                    gridSetSize = settingData.gridSetSize,
                    level = level,
                    start = start,
                    type = -1,
                    width = widthInM,
                    twoWidth2 = twoWidth2,
                    queueWriter = queue.AsParallelWriter()
                }.Schedule(end - start + 1, (end - start + 1) / 4, inputDependency);
            }

            // 右上 level = i+j
            // for level=I+J-2:0 do  step=-1
            // I1 = max(0,level-J+1)
            // I2 = min(I-1,level)
            // parallel for i = I1:I2 do
            // j = level - i;
            // udpate value
            for (int level = height + width - 2; level >= 0; level += -1)
            {
                int start = math.max(0, level - width + 1);
                int end = math.min(height - 1, level);
                inputDependency = new ParallelFastSweepingMethodJob()
                {
                    cells = cells,
                    gridSetSize = settingData.gridSetSize,
                    level = level,
                    start = start,
                    type = 1,
                    width = widthInM,
                    twoWidth2 = twoWidth2,
                    queueWriter = queue.AsParallelWriter()
                }.Schedule(end - start + 1, (end - start + 1) / 4, inputDependency);
            }
            inputDependency.Complete();
        } while (queue.Count > 0);
        queue.Dispose();
        UnityEngine.Debug.Log(count);
        return inputDependency;
    }
}

[BurstCompile]
public struct ParallelFastSweepingMethodJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<CellData> cells;
    [ReadOnly] public int2 gridSetSize;
    [ReadOnly] public float width, twoWidth2;

    [ReadOnly] public int level, start, type;// type=1=>level=i+j;type=-1=>level=i-j

    public NativeQueue<byte>.ParallelWriter queueWriter;

    public void Execute(int index)
    {
        var i = index + start;
        var j = (level - i) * type;

        var currentIndex = FlowFieldUtility.ToFlatIndex(i, j, gridSetSize.y);
        var current = cells[currentIndex];

        if (current.localCost >= Constants.T_c) return;

        float2 midValue = float2.zero;
        float left, right;

        left = (i == 0 ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i - 1, j, gridSetSize.y)].integrationCost);
        right = (i == (gridSetSize.x - 1) ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i + 1, j, gridSetSize.y)].integrationCost);
        midValue.y = math.min(left, right);

        left = (j == 0 ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i, j - 1, gridSetSize.y)].integrationCost);
        right = (j == (gridSetSize.y - 1) ? current.integrationCost : cells[FlowFieldUtility.ToFlatIndex(i, j + 1, gridSetSize.y)].integrationCost);
        midValue.x = math.min(left, right);

        float newIntCost;
        if (math.abs(midValue.x - midValue.y) < width * current.localCost)
        {
            newIntCost = (midValue.x + midValue.y + math.sqrt(twoWidth2 * current.localCost * current.localCost - (midValue.x - midValue.y) * (midValue.x - midValue.y))) * 0.5f;
        }
        else
        {
            newIntCost = math.min(midValue.x, midValue.y) + width * current.localCost;
        }

        if (newIntCost < current.integrationCost)
        {
            current.integrationCost = newIntCost;
            queueWriter.Enqueue(0);
        }

        // current.integrationCost = math.min(newIntCost, current.integrationCost);
        cells[currentIndex] = current;
    }
}

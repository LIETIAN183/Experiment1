using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Unity.Physics;
using Unity.Jobs;
using System;
using System.Reflection;
using System.IO;
using Unity.Burst;


public struct Detail
{
    public int NO;
    public FixedString32Bytes SeismicName;
    public float time;
    public float xAcc;
    public float yAcc;
    public float zAcc;
    public int dropCount;
    public int escapedPedestrain;
    public int escapedPerSec;
}
public struct Summary
{
    public int NO;
    public FixedString32Bytes SeismicName;
    public float simulationPGA;
    public float fullEscapeTime;
    public int finalDropCount;
    public int itemCount;
    public float escapeTime_ave;
    public float escapeLength_ave;
    public float escapeVel_ave;
    public float reactionTime_ave;
}
[UpdateInGroup(typeof(AnalysisSystemGroup))]
[BurstCompile]
public partial struct SingleStatisticSystem : ISystem
{
    // 数据变量
    NativeList<Detail> details;
    NativeList<Summary> summaries;

    // 辅助变量
    private int escaped;
    private EntityQuery escapedQuery, mcQuery;
    private NativeQueue<byte> countQueue;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        details = new NativeList<Detail>(Allocator.Persistent);
        summaries = new NativeList<Summary>(Allocator.Persistent);
        countQueue = new NativeQueue<byte>(Allocator.Persistent);
        escapedQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Escaped>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(ref state);
        mcQuery = state.GetEntityQuery(ComponentType.ReadOnly<MCData>());
        state.Enabled = false;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = GetDetail(ref state);

        // 人数到达标准，结束仿真
        if (escaped.Equals(SystemAPI.GetSingleton<SpawnerData>().desireCount))//&& data.elapsedTime >= 50
        {
            GetSummary(ref state);

            // 结束本轮仿真
            SystemAPI.SetSingleton(new EndSeismicEvent { isActivate = true });
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        details.Dispose();
        summaries.Dispose();
        countQueue.Dispose();
    }
    [BurstCompile]
    public void ClearDataStorage()
    {
        details.Clear();
        summaries.Clear();
    }

    [BurstCompile]
    private JobHandle GetDetail(ref SystemState state)
    {
        countQueue.Clear();
        var countJob = new DroppedMCCountJob
        {
            counter = countQueue.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);

        escaped = escapedQuery.CalculateEntityCount();

        var recordJob = new RecordDetailJob
        {
            simulationRoundIndex = summaries.Length,
            details = details,
            dropped = countQueue,
            escaped = escaped,
            data = SystemAPI.GetSingleton<TimerData>(),
            deltaTime = SystemAPI.Time.DeltaTime
        }.Schedule(countJob);
        return recordJob;
    }

    [BurstCompile]
    public void GetSummary(ref SystemState state)
    {
        NativeQueue<float> timeQueue = new NativeQueue<float>(Allocator.TempJob);
        NativeQueue<float> lengthQueue = new NativeQueue<float>(Allocator.TempJob);
        NativeQueue<float> velQueue = new NativeQueue<float>(Allocator.TempJob);
        NativeQueue<float> recTimeQueue = new NativeQueue<float>(Allocator.TempJob);

        var calJob = new CalAgentAverageInfoJob
        {
            timeWriter = timeQueue.AsParallelWriter(),
            lengthWriter = lengthQueue.AsParallelWriter(),
            velWriter = velQueue.AsParallelWriter(),
            recTimeWriter = recTimeQueue.AsParallelWriter()
        }.ScheduleParallel(state.Dependency);

        new RecordSummaryJob
        {
            summaries = summaries,
            details = details,
            timeQueue = timeQueue,
            lengthQueue = lengthQueue,
            recTimeQueue = recTimeQueue,
            velQueue = velQueue,
            data = SystemAPI.GetSingleton<TimerData>(),
            itemCount = mcQuery.CalculateEntityCount()
        }.Schedule(calJob).Complete();

        timeQueue.Dispose();
        lengthQueue.Dispose();
        velQueue.Dispose();
        recTimeQueue.Dispose();
    }

    public void ExportData()
    {
        var savePath1 = Application.streamingAssetsPath + "/RecordData/Detail.txt";

        FileStream fs = new FileStream(@savePath1, FileMode.OpenOrCreate);
        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        fs.Close();

        fs = new FileStream(@savePath1, FileMode.Append);
        StreamWriter wr = null;
        wr = new StreamWriter(fs);
        wr.WriteLine(Struct2String(typeof(Detail)));
        for (int i = 0; i < details.Length; i++)
        {
            wr.WriteLine(Data2String(details[i]));
        }
        wr.Close();

        var savePath2 = Application.streamingAssetsPath + "/RecordData/Summary.txt";

        fs = new FileStream(@savePath2, FileMode.OpenOrCreate);
        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);
        fs.Close();

        fs = new FileStream(@savePath2, FileMode.Append);
        wr = null;
        wr = new StreamWriter(fs);
        wr.WriteLine(Struct2String(typeof(Summary)));
        for (int i = 0; i < summaries.Length; i++)
        {
            wr.WriteLine(Data2String(summaries[i]));
        }
        wr.Close();
    }

    public string Struct2String(Type type)
    {
        string result = null;
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            result += field.Name + ' ';
        }
        return result.TrimEnd();
    }

    public string Data2String(object data)
    {
        string result = null;
        foreach (var field in data.GetType().GetFields())
        {
            result += field.GetValue(data).ToString() + ' ';
        }
        return result.TrimEnd();
    }
}

// TODO: 考虑到存在破碎物体，计算不准确
[BurstCompile]
[WithAll(typeof(MCData))]
partial struct DroppedMCCountJob : IJobEntity
{
    public NativeQueue<byte>.ParallelWriter counter;
    void Execute(in MCData data, in LocalTransform localTransform)
    {
        // 最底层货架上的商品存在一定的错误计算，但误差可接受
        // 将地面下降2m统计则将为准确结果
        //     // 位于第二层货架上方的商品，低于0.5f且商品不在空中时，算掉落
        //     if (recoverData.originPosition.y > 0.5f && translation.Value.y < 0.5f && !data.inAir)
        //     {
        //         countWriter.Enqueue(true);
        //     }
        //     else if (recoverData.originPosition.y < 0.5f)
        //     {
        //         // 位于底层的货架上的商品，位移超过0.4m就算其掉落
        //         if (math.lengthsq(recoverData.originPosition - translation.Value) > 0.16f) countWriter.Enqueue(true);
        //     }

        // 位于第二层货架上方的商品，低于0.5f且商品不在空中时，算掉落
        if (localTransform.Position.y < 0.3f && !data.inAir)
        {
            counter.Enqueue(0);
        }
    }
}

[BurstCompile]
[WithAll(typeof(Escaped), typeof(AgentMovementData)), WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
partial struct CalAgentAverageInfoJob : IJobEntity
{
    public NativeQueue<float>.ParallelWriter timeWriter, lengthWriter, velWriter, recTimeWriter;
    void Execute(in RecordData recordData)
    {
        timeWriter.Enqueue(recordData.escapedTime);
        lengthWriter.Enqueue(recordData.escapedLength);
        velWriter.Enqueue(recordData.escapeAveVel);
        recTimeWriter.Enqueue(recordData.reactionTime);
    }
}

[BurstCompile]
partial struct RecordDetailJob : IJob
{
    public NativeList<Detail> details;

    [ReadOnly] public int simulationRoundIndex;
    [ReadOnly] public NativeQueue<byte> dropped;
    [ReadOnly] public int escaped;
    [ReadOnly] public TimerData data;
    [ReadOnly] public float deltaTime;
    public void Execute()
    {
        var curDetail = new Detail()
        {
            NO = simulationRoundIndex,
            SeismicName = data.seismicEventName,
            time = data.elapsedTime,
            xAcc = data.curAcc.x,
            yAcc = data.curAcc.y,
            zAcc = data.curAcc.z,
            dropCount = dropped.Count,
            escapedPedestrain = escaped,
            escapedPerSec = (int)((escaped - (details.Length <= 0 ? 0 : details[details.Length - 1].escapedPedestrain)) / deltaTime)
        };
        details.Add(curDetail);
    }
}

[BurstCompile]
partial struct RecordSummaryJob : IJob
{
    public NativeList<Summary> summaries;
    [ReadOnly] public NativeList<Detail> details;
    public NativeQueue<float> timeQueue, lengthQueue, velQueue, recTimeQueue;
    [ReadOnly] public TimerData data;
    [ReadOnly] public int itemCount;

    public void Execute()
    {
        var count = timeQueue.Count;
        float sumTime = 0, sumLength = 0, sumVel = 0, sumRecTime = 0;
        while (timeQueue.Count > 0) sumTime += timeQueue.Dequeue();
        while (lengthQueue.Count > 0) sumLength += lengthQueue.Dequeue();
        while (velQueue.Count > 0) sumVel += velQueue.Dequeue();
        while (recTimeQueue.Count > 0) sumRecTime += recTimeQueue.Dequeue();
        var curSummary = new Summary()
        {
            NO = summaries.Length,
            SeismicName = data.seismicEventName,
            simulationPGA = math.select(data.simPGA, data.eventPGA, data.simPGA.Equals(0)),
            fullEscapeTime = data.elapsedTime,
            // finalDropCount = countBridge[0],
            finalDropCount = details[details.Length - 1].dropCount,
            itemCount = itemCount,
            escapeTime_ave = sumTime / count,
            escapeLength_ave = sumLength / count,
            escapeVel_ave = sumVel / count,
            reactionTime_ave = sumRecTime / count
        };
        summaries.Add(curSummary);
    }
}
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.Physics;
using Unity.Jobs;
using System;
using System.Reflection;
using System.IO;
using System.Linq;


public struct Detail
{
    public int NO;
    public string SeismicName;
    public float simulationPGA;
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
    public string SeismicName;
    public float simulationPGA;
    public float fullEscapeTime;
    public float finalDropCount;
    public float itmeCount;
    public float escapeTime_ave;
    public float escapeLength_ave;
    public float escapeVel_ave;
}
[UpdateInGroup(typeof(AnalysisSystemGroup))]
public partial class SingleStatisticSystem : SystemBase
{
    List<Detail> details;

    List<Summary> summaries;

    int escapedPedestrain_Backup;
    protected override void OnCreate()
    {
        details = new List<Detail>();
        summaries = new List<Summary>();

        this.Enabled = false;
    }

    protected override void OnStartRunning()
    {
        escapedPedestrain_Backup = 0;
    }

    protected override void OnUpdate()
    {
        var data = GetSingleton<AccTimerData>();
        NativeQueue<bool> countQueue = new NativeQueue<bool>(Allocator.TempJob);
        var countWriter = countQueue.AsParallelWriter();

        // 最底层货架上的商品存在一定的错误计算，但误差可接受
        // 将地面下降2m统计则将为准确结果
        // Entities.WithAll<MCData>().ForEach((in Translation translation, in BackupData recoverData, in MCData data) =>
        // {
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
        // }).ScheduleParallel(Dependency).Complete();
        Entities.WithAll<MCData>().ForEach((in LocalTransform localTransform, in MCData data) =>
        {
            // 位于第二层货架上方的商品，低于0.5f且商品不在空中时，算掉落
            if (localTransform.Position.y < 0.5f && !data.inAir)
            {
                countWriter.Enqueue(true);
            }
        }).ScheduleParallel(Dependency).Complete();

        var query = new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Escaped>() },
            Options = EntityQueryOptions.IncludeDisabledEntities
        };
        var escaped = GetEntityQuery(query).CalculateEntityCount();
        var curDetail = new Detail()
        {
            NO = summaries.Count + 1,
            SeismicName = data.seismicName.ToString(),
            simulationPGA = math.select(data.targetPGA, data.eventPGA, data.targetPGA.Equals(0)),
            time = data.elapsedTime,
            xAcc = data.acc.x,
            yAcc = data.acc.y,
            zAcc = data.acc.z,
            dropCount = countQueue.Count,
            escapedPedestrain = escaped,
            escapedPerSec = escaped - escapedPedestrain_Backup
        };
        details.Add(curDetail);
        escapedPedestrain_Backup = escaped;
        countQueue.Dispose();

        // // 人数到达标准，结束仿真
        if (escaped.Equals(GetSingleton<SpawnerData>().desireCount))//&& data.elapsedTime >= 50
        {
            GetSummary();
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<AccTimerSystem>().Enabled = false;
        }
    }
    public void ClearDataStorage()
    {
        details.Clear();
        summaries.Clear();
    }

    public void GetSummary()
    {
        var accData = GetSingleton<AccTimerData>();

        NativeQueue<float> timeQueue = new NativeQueue<float>(Allocator.TempJob);
        NativeQueue<float> lengthQueue = new NativeQueue<float>(Allocator.TempJob);
        NativeQueue<float> velQueue = new NativeQueue<float>(Allocator.TempJob);

        var timeWriter = timeQueue.AsParallelWriter();
        var lengthWriter = lengthQueue.AsParallelWriter();
        var velWriter = velQueue.AsParallelWriter();

        Entities.WithAll<Escaped>().WithEntityQueryOptions(EntityQueryOptions.IncludeDisabledEntities).ForEach((in RecordData recordData) =>
        {
            timeWriter.Enqueue(recordData.escapeTime);
            lengthWriter.Enqueue(recordData.escapeLength);
            velWriter.Enqueue(recordData.escapeAveVel);
        }).ScheduleParallel(Dependency).Complete();

        var count = timeQueue.Count;
        float sumTime = 0, sumLength = 0, sumVel = 0;
        while (timeQueue.Count > 0) sumTime += timeQueue.Dequeue();
        while (lengthQueue.Count > 0) sumLength += lengthQueue.Dequeue();
        while (velQueue.Count > 0) sumVel += velQueue.Dequeue();
        var curSummary = new Summary()
        {
            NO = summaries.Count + 1,
            SeismicName = accData.seismicName.ToString(),
            simulationPGA = math.select(accData.targetPGA, accData.eventPGA, accData.targetPGA.Equals(0)),
            fullEscapeTime = accData.elapsedTime,
            // finalDropCount = countBridge[0],
            finalDropCount = details.Last().dropCount,
            itmeCount = GetEntityQuery(ComponentType.ReadOnly<MCData>()).CalculateEntityCount(),
            escapeTime_ave = sumTime / count,
            escapeLength_ave = sumLength / count,
            escapeVel_ave = sumVel / count
        };
        summaries.Add(curSummary);
        timeQueue.Dispose();
        lengthQueue.Dispose();
        velQueue.Dispose();

        var analysisSetting = GetSingleton<MultiRoundStatisticsData>();
        analysisSetting.curStage = AnalysisStage.Recover;
        SetSingleton<MultiRoundStatisticsData>(analysisSetting);
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
        for (int i = 0; i < details.Count; i++)
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
        for (int i = 0; i < summaries.Count; i++)
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

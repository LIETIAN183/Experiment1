using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using System;
using Drawing;

[BurstCompile]
public static class ExtensionMethod
{
    /// <summary>
    /// 计算地震事件的 PGA，单位 g
    /// </summary>
    /// <param name="accList">NatiList 中存储的数据类型为 float3，代表每一时刻的加速度</param>
    /// <returns>event PGA</returns>
    [BurstCompile]
    public static float maxPGA(this ref BlobArray<float3> accList)
    {
        float midRes = 0;
        for (int i = 0; i < accList.Length; i++)
        {
            midRes = math.max(midRes, math.lengthsq(accList[i]));
        }
        return math.sqrt(midRes) / Constants.gravity;
    }

    /// <summary>
    /// 判断 value 是否在区间 [min,max] 内，若超出范围范围则返回 defaultValue
    /// </summary>
    /// <param name="value">输入值</param>
    /// <param name="min">范围下界</param>
    /// <param name="max">范围上界</param>
    /// <param name="defaultValue">超时范围时使用的默认值</param>
    /// <returns>InRange: Value, OutRange: defaultValue</returns>
    [BurstCompile]
    public static float inRange(this float value, float min, float max, float defaultValue)
    {
        return (value < min | value > max) ? defaultValue : value;
    }

    /// <summary>
    /// 给 DynamicBuffer 添加 ToList 函数
    /// </summary>
    /// <param name="source">输入</param>
    /// <typeparam name="T">泛型</typeparam>
    /// <returns></returns>
    public static List<T> ToList<T>(this DynamicBuffer<T> source) where T : unmanaged
    {
        var res = new List<T>();
        foreach (var item in source)
        {
            res.Add(item);
        }
        return res;
    }

    public static string[] GetNameArry(this DynamicBuffer<BlobRefBuffer> source)
    {
        string[] eventNameArray = new string[source.Length];
        for (int i = 0; i < source.Length; ++i)
        {
            eventNameArray[i] = source[i].Value.Value.eventName.ToString();
        }
        return eventNameArray;
    }

    /// <summary>
    /// 返回非 MaxValue 的最大值，即第二大数值
    /// 泛型版本，有函数指针，不支持 BurstCompile
    /// </summary>
    /// <param name="source">Cell 集合</param>
    /// <returns>第二大数值</returns>
    public static TResult SecondMaxCost<CellData, TResult>(this List<CellData> source, Func<CellData, TResult> selector, TResult maxValue) where TResult : unmanaged, IComparable
    {
        TResult secondMaxCost = selector(source[0]);
        foreach (var cell in source)
        {
            var temp = selector(cell);
            // 因为 TResult.MaxValue不可用，因此从函数作为参数输入
            if (temp.Equals(maxValue)) continue;
            if (temp.CompareTo(secondMaxCost) > 0) secondMaxCost = temp;
        }
        return secondMaxCost;
    }

    /// <summary>
    /// 获得非 MaxValue 的最大 cost
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static float SecondMaxCost(this NativeArray<CellData> source)
    {
        float secondMaxCost = 0;
        foreach (var cell in source)
        {
            if (cell.localCost >= Constants.T_c) continue;
            secondMaxCost = math.max(secondMaxCost, cell.localCost);
        }
        return secondMaxCost;
    }

    public static float SecondMaxTempCost(this NativeArray<CellData> source)
    {
        float secondMaxTempCost = 0;
        foreach (var cell in source)
        {
            if (cell.integrationCost >= Constants.T_i) continue;
            secondMaxTempCost = math.max(secondMaxTempCost, cell.integrationCost);
        }
        return secondMaxTempCost;
    }

    /// <summary>
    /// 给 Drawing 添加绘制叉号的功能
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    public static void DrawCross45(this CommandBuilder builder, float3 position, float3 size, Color color)
    {
        builder.PushColor(color);
        builder.Line(position - new float3(size.x, 0, size.z), position + new float3(size.x, 0, size.z));
        builder.Line(position - new float3(size.x, 0, -size.z), position + new float3(size.x, 0, -size.z));
        builder.PopColor();
    }

    /// <summary>
    /// 复制目标 Transform 的 position, rotation 和 localScale
    /// </summary>
    /// <param name="targetTransform"></param>
    /// <param name="sourceTransform"></param>
    public static void CopyPosRotScale(this Transform targetTransform, Transform sourceTransform)
    {
        targetTransform.position = sourceTransform.position;
        targetTransform.rotation = sourceTransform.rotation;
        targetTransform.localScale = sourceTransform.localScale;
    }

    public static bool Contain(this DynamicBuffer<DestinationBuffer> source, int target)
    {
        foreach (var item in source)
        {
            if (item.desFlatIndex == target)
            {
                return true;
            }
        }
        return false;
    }

    public static bool TryGetIndex(this DynamicBuffer<DestinationBuffer> source, int target, out int index)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i].desFlatIndex == target)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public static float GetCellGridVolume(this FlowFieldSettingData data)
    {
        return data.cellRadius.x * data.cellRadius.y * data.cellRadius.z * 8;
    }
}
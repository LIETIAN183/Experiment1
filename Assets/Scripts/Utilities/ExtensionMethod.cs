using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Drawing;

public static class ExtensionMethod
{
    /// <summary>
    /// 计算地震事件的 PGA，单位 g
    /// </summary>
    /// <param name="accList">NatiList 中存储的数据类型为 float3，代表每一时刻的加速度</param>
    /// <returns>event PGA</returns>
    public static float maxPGA(this ref BlobArray<float3> accList)
    {
        float midRes = 0;
        for (int i = 0; i < accList.Length; i++)
        {
            midRes = math.max(midRes, math.lengthsq(accList[i]));
        }
        // foreach (var item in accList)
        // {
        //     midRes = math.max(midRes, math.lengthsq(item));
        // }
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
    public static byte SecondMaxCost(this NativeArray<CellData> source)
    {
        byte secondMaxCost = 0;
        foreach (var cell in source)
        {
            if (cell.cost == byte.MaxValue) continue;
            if (cell.cost > secondMaxCost) secondMaxCost = cell.cost;
        }
        return secondMaxCost;
    }

    /// <summary>
    /// 获得非 MaxValue 的最大 BestCost
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ushort SecondMaxBestCost(this List<CellData> source)
    {
        ushort secondMaxBestCost = 0;
        foreach (var cell in source)
        {
            if (cell.bestCost == ushort.MaxValue) continue;
            if (cell.bestCost > secondMaxBestCost) secondMaxBestCost = cell.bestCost;
        }
        return secondMaxBestCost;
    }

    public static float SecondMaxTempCost(this NativeArray<CellData> source)
    {
        float secondMaxTempCost = 0;
        foreach (var cell in source)
        {
            if (cell.tempCost == float.MaxValue) continue;
            if (cell.tempCost > secondMaxTempCost) secondMaxTempCost = cell.tempCost;
        }
        return secondMaxTempCost;
    }

    public static void drawCross45(this CommandBuilder builder, float3 position, float3 size, Color color)
    {
        builder.PushColor(color);
        builder.Line(position - new float3(size.x, 0, size.z), position + new float3(size.x, 0, size.z));
        builder.Line(position - new float3(size.x, 0, -size.z), position + new float3(size.x, 0, -size.z));
        builder.PopColor();
    }
}
// using UnityEngine;
// using Unity.Mathematics;
using UnityEngine;
// using Random = Unity.Mathematics.Random;

public static class NormalDistribution
{
    // 返回服从均值为mean和方差为sigma的正态分布的随机值
    public static float RandomGaussian(float mean, float sigma) => StandardNormalDistribution() * sigma + mean;

    // 返回范围在[minValue,maxValue]内服从正态分布的随机值
    public static float RandomGaussianInRange(float minValue, float maxValue)
    {
        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(StandardNormalDistribution() * sigma + mean, minValue, maxValue);
    }

    // 标准正态分布
    public static float StandardNormalDistribution()
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        return u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
    }
}

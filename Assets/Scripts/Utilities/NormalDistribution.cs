using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public static class NormalDistribution
{
    // 返回服从均值为mean和方差为sigma的正态分布的随机值
    [BurstCompile]
    public static float RandomGaussian(float mean, float sigma, uint seed) => StandardNormalDistribution(seed) * sigma + mean;

    // 返回范围在[minValue,maxValue]内服从正态分布的随机值
    [BurstCompile]
    public static float RandomGaussianInRange(float minValue, float maxValue, uint seed)
    {
        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return math.clamp(StandardNormalDistribution(seed) * sigma + mean, minValue, maxValue);
    }

    // 标准正态分布
    [BurstCompile]
    public static float StandardNormalDistribution(uint seed)
    {
        float u, v, S;
        var r = Random.CreateFromIndex(seed);
        do
        {
            u = 2.0f * r.NextFloat() - 1.0f;
            v = 2.0f * r.NextFloat() - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        return u * math.sqrt(-2.0f * math.log(S) / S);
    }
}

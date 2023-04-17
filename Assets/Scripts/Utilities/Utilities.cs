using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public static class Utilities
{
    [BurstCompile]
    public static void rotateAroundPoint(in float3 pivot, in quaternion targetRotation, in float3 itemPosition, in quaternion itemRotation, out float3 finalPostion, out quaternion finalRotation)
    {
        finalPostion = math.mul(targetRotation, itemPosition - pivot) + pivot;
        finalRotation = math.mul(targetRotation, itemRotation);
    }

    [BurstCompile]
    public static void rotateAroundOriginPoint(in quaternion targetRotation, in float3 itemPosition, in quaternion itemRotation, out float3 finalPostion, out quaternion finalRotation)
    {
        finalPostion = math.mul(targetRotation, itemPosition);
        finalRotation = math.mul(targetRotation, itemRotation);
    }

    public static (float3 position, quaternion rotation) rotateAroundPoint(float3 pivot, quaternion targetRotation, float3 itemPosition, quaternion itemRotation)
    {
        itemPosition = math.mul(targetRotation, itemPosition - pivot) + pivot;
        itemRotation = math.mul(targetRotation, itemRotation);
        return (itemPosition, itemRotation);
    }

    [BurstCompile]
    public static float GetStandardVelByPGA(float pga)
    {
        var temp = pga * Constants.gravity;
        if (temp < 0.222f)
        {
            return 1f;
        }
        else if (temp < 0.936f)
        {
            return 2f;
        }
        else
        {
            return 3f;
        }
    }
}

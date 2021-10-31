using Unity.Mathematics;

public static class MyExtensions
{
    public static float3 addFloat2(this float3 num1, float2 num2)
    {
        num1.x += num2.x;
        num1.z += num2.y;
        return num1;
    }
}

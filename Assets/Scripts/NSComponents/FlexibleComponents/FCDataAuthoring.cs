using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FCDataAuthoring : MonoBehaviour
{
    public float length;

    public float k, c, mass;

    public bool directionConstrain;

    public bool randomInitialize;

    class Baker : Baker<FCDataAuthoring>
    {
        public override void Bake(FCDataAuthoring authoring)
        {

            float modify_k, modify_c;
            if (authoring.randomInitialize)
            {
                // UnityEngine.Random.Range 可以返回不同的值，不可或缺
                uint seed = (uint)UnityEngine.Random.Range(uint.MinValue + 100, uint.MaxValue - 100);
                var random = Unity.Mathematics.Random.CreateFromIndex(seed);
                modify_k = random.NextFloat(-5, 5);
                modify_c = random.NextFloat(-0.2f, 0.2f);
            }
            else
            {
                modify_k = modify_c = 0;
            }
            AddComponent(new FCData
            {
                length = authoring.length,
                k = authoring.k + modify_k,
                c = authoring.c + modify_c,
                mass = authoring.mass,
                directionConstrain = authoring.directionConstrain
            });
        }
    }
}

public struct FCData : IComponentData
{
    public float length;

    public float topDis, topVel, topAcc;

    public float k, c, mass;

    public bool directionConstrain;

    public float3 forward;
}
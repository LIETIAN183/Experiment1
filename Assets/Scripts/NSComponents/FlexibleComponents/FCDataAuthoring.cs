using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// 挂载柔性构件端点组件
public class FCDataAuthoring : MonoBehaviour
{
    public float length;

    public float k, c, mass;

    public bool directionConstrain;

    public bool randomInitialize;

    public bool considerFriction;

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
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FCData
            {
                length = authoring.length,
                k = authoring.k + modify_k,
                c = authoring.c + modify_c,
                mass = authoring.mass,
                directionConstrain = authoring.directionConstrain,
                considerFriction = authoring.considerFriction
            });
        }
    }
}

// 柔性构件相关数据
public struct FCData : IComponentData
{
    // 柔性构件长度
    public float length;

    // 柔性构件端点位移、速度、加速度
    public float topDis, topVel, topAcc;

    // 柔性构件弹性系数、阻尼系数和质量
    public float k, c, mass;

    // 是否需要限制柔性构件振荡方向，若单侧货架背面靠墙则勾选该选项
    public bool directionConstrain;

    // 货架正面向量
    public float3 forward;

    // 判断是否需要考虑货架上物品的摩擦力
    public bool considerFriction;
}
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// 实现 AgentMovementData 通过 Inspector 挂载
public class AgentMovementDataAuthoring : MonoBehaviour
{
    public float standardVel;
    class Baker : Baker<AgentMovementDataAuthoring>
    {
        public override void Bake(AgentMovementDataAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new AgentMovementData
            {
                stdVel = authoring.standardVel,
                deltaHeight = 0,
                forceForFootInteraction = 0,
                desireSpeed = 0,
                curSpeed = 0
            });
        }
    }
}

// 行人相关的数据
public struct AgentMovementData : IComponentData
{
    // 基准速度、已弃用
    public float stdVel;
    // 行人数值位置
    public float3 originPosition;
    // 行人脚部交互力
    public float3 forceForFootInteraction;

    // 行人期望速度
    public float desireSpeed;

    // 行人当前速度
    public float curSpeed;
    // 行人垂直变化高度
    public float deltaHeight;

    // 行人对环境的熟悉程度
    public float familiarity;
    // 行人的反应时间系数
    public float reactionCofficient;

    // 判断行人是否知晓出口位置
    public bool SeeExit;

    // 行人上一时刻的自定义疏散方向
    public float2 lastSelfDir;

    // 判断行人是否摔倒
    public bool isFall;

    // 行人摔倒使用的时间
    public float fallTimer;

    // 行人站立后的恢复时间
    public float recoverTimer;
}
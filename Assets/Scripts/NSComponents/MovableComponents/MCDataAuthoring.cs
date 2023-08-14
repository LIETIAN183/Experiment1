using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;

// 实现 MCData 数据在编辑器界面的挂载功能，否则仅能通过代码挂载该数据
[RequireComponent(typeof(PhysicsShapeAuthoring), typeof(PhysicsBodyAuthoring))]
public class MCDataAuthoring : MonoBehaviour
{
    class Baker : Baker<MCDataAuthoring>
    {
        public override void Bake(MCDataAuthoring authoring)
        {
            // 添加数据到目标 Entity
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            AddComponent<MCData>(entity);
        }
    }
}

public struct MCData : IComponentData
{
    // 上一时刻垂直速度
    public float preVelinY;
    // 判断是否空中状态
    public bool inAir;
}
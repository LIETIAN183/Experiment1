using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Authoring;

// 用于实现在Inspector中挂载
[RequireComponent(typeof(PhysicsShapeAuthoring), typeof(PhysicsBodyAuthoring))]
public class SubFCDataAuthoring : MonoBehaviour
{
    public float height;

    public GameObject parent;

    class Baker : Baker<SubFCDataAuthoring>
    {
        public override void Bake(SubFCDataAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new SubFCData
            {
                height = authoring.height,
                parent = GetEntity(authoring.parent, TransformUsageFlags.Dynamic)
            });
        }
    }
}

// 实现柔性构件中间段的变形仿真
public struct SubFCData : IComponentData
{
    // 子组件的初始位置
    public float3 orgPos;

    // 子组件的初始旋转角度
    public quaternion orgRot;

    // 子组件在柔性构件中的高度
    public float height;

    // 保存父物体应用，获取 FCData 数据用于变形计算
    public Entity parent;
}
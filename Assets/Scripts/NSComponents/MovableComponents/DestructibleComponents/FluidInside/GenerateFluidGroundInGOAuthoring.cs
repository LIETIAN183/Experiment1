using UnityEngine;
using Unity.Entities;
using Unity.Physics.Authoring;
using Obi;

// 生成用于与流体交互的 GameObject 地板
[RequireComponent(typeof(PhysicsShapeAuthoring))]
public class GenerateFluidGroundInGOAuthoring : MonoBehaviour
{
    // TODO: 1. 在未来版本实现 “用于和流体交互的地板” 无需手动辅助的功能
    // Subscene 内的 Baking 在 Edit 模式下就会运行，所以会提前生成一个 GroundInGo 对象
    // 该对象在需要在 Build 前存在，因为目前测试 Build 后不生成该 GameObject
    // 注意：！！！ 还需要手动拖到 SubScene 之前，这是目前版本的问题，暂时无法解决
    class Baker : Baker<GenerateFluidGroundInGOAuthoring>
    {
        public static readonly string name = "GroundInGO(Move Above SubScene Manually)";
        public override void Bake(GenerateFluidGroundInGOAuthoring authoring)
        {
            // 检测有无已经生成，若已经生成则不再生成新的
            var go = GameObject.Find(name);
            if (go != null)
            {
                return;
            }

            //获取目标数据
            var sourceCollider = authoring.GetComponent<PhysicsShapeAuthoring>().GetBoxProperties();
            var sourceTransform = authoring.transform;

            // 创建物体并设置尺寸
            GameObject groundInGo = new GameObject(name);
            groundInGo.transform.CopyPosRotScale(sourceTransform);

            // 配置 Collider
            // 地板的 PhysicsShape 的 Orientation 必须设置为 0
            var colliderInGo = groundInGo.AddComponent<BoxCollider>();
            colliderInGo.center = sourceCollider.Center;
            colliderInGo.size = sourceCollider.Size;

            // 配置 ObiCollider
            var obiColliderInGo = groundInGo.AddComponent<ObiCollider>();
            obiColliderInGo.sourceCollider = colliderInGo;
        }
    }
}
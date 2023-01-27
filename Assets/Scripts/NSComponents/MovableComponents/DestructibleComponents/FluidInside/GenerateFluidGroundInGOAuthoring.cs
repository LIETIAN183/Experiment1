using UnityEngine;
using Unity.Entities;
using Unity.Physics.Authoring;
using Obi;

[RequireComponent(typeof(DestroyFluidGroundInEditMode), typeof(PhysicsShapeAuthoring))]
public class GenerateFluidGroundInGOAuthoring : MonoBehaviour
{
    // Subscene 内的 Baking 在 Edit 模式下就会运行，所以总会提前生成一个 GroundInGo 对象
    // 同时进入 PlayMode 后 Baker也会运行，生成的 GameObject 退出 PlayMode 后不消失，这个问题暂时不知道如何解决
    class Baker : Baker<GenerateFluidGroundInGOAuthoring>
    {
        public override void Bake(GenerateFluidGroundInGOAuthoring authoring)
        {
            // 删除所有存在的并重写生成
            // 还是需要再检测一次的
            var go = GameObject.Find("GroundInGo(Work in Play Mode)");
            while (go != null)
            {
                Object.DestroyImmediate(go);
                go = GameObject.Find("GroundInGo(Work in Play Mode)");
            }

            //获取目标数据
            var sourceCollider = authoring.GetComponent<PhysicsShapeAuthoring>().GetBoxProperties();
            var sourceTransform = authoring.transform;

            // 创建物体并设置尺寸
            GameObject groundInGo = new GameObject("GroundInGo(Work in Play Mode)");
            groundInGo.transform.position = sourceTransform.position;
            groundInGo.transform.rotation = sourceTransform.rotation;
            groundInGo.transform.localScale = sourceTransform.localScale;

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
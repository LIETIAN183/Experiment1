using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// 添加 RenderInGO 到 Entity 上，并关联 GameObejct
// 添加 RenderConfig 到 GameObject 上
// 实现 Entity 世界完成碰撞等逻辑，在 GameObejct 世界完成渲染
public class RenderConfig : MonoBehaviour
{
    public Entity renderEntity;

    // fix the problem of the  GameObject and Entity have the different center in Possition Property
    public float3 positionOffset = float3.zero;

    // To get the Entity in GameObject World
    private EntityManager manager;

    public Transform bone;

    public float height;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        if (renderEntity == null)
        {
            Debug.Log("Need Attach the Entity for Render");
            return;
        }

        // 同步物体的位置和旋转属性
        LocalToWorld worldCofficient = manager.GetComponentData<LocalToWorld>(renderEntity);
        transform.position = worldCofficient.Position + positionOffset;

        Rotation entRot = manager.GetComponentData<Rotation>(renderEntity);
        transform.rotation = entRot.Value;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        var data = manager.GetComponentData<ShakeData>(renderEntity);
        var gradient = -3 * data.endMovement * (math.pow(height, 2) - 2 * data.length * height) / (2 * math.pow(data.length, 3));
        var degree = math.degrees(math.atan(gradient));
        bone.rotation = Quaternion.Euler(degree, 0, 0);
    }
}

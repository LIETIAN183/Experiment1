using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;

[DisableAutoCreation]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EqControllerSystem))]
public class GroundMotionSystem : SystemBase
{
    // 指示选择的地震数据的编号
    // TODO: 新建一个脚本存储中间的index，否则不同 MotionSystem可能存在同步问题
    int gmIndex = 0;
    // 时间对应的加速度下标
    int timeCount = 0;
    // PhysicsWorld physicsWorld;
    protected override void OnCreate()
    {
        // base.OnCreate();
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        // float deltaTime = Time.DeltaTime;
        // Debug.Log(gmIndex);
        ref BlobArray<GroundMotion> gmArray = ref GroundMotionBlobAssetsConstructor.gmBlobRefs[gmIndex].Value.gmArray;
        // ref BlobString name = ref gmAsset.gmName;
        // Debug.Log(name.ToString());
        float3 acc = gmArray[timeCount].acceleration;
        // physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        Entities.WithAll<GroundTag>().ForEach((ref PhysicsVelocity physicsVelocity) =>
        {
            // DeltaTime 为 0.01f ,因为已经设置了 DeltaTime 为固定值，那就不用每次再获取 DeltaTime了
            physicsVelocity.Linear += acc * 0.01f;
        }).ScheduleParallel();

        // 更新 UI 界面 Progress
        ECSUIController.Instance.progress.currentValue = timeCount;

        ++timeCount;
        if (timeCount >= gmArray.Length)
        {
            // Debug.Log("End");
            // TODO: UI 显示仿真结束提示
            this.Enabled = false;
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<ComsMotionSystem>().Enabled = false;
        }
    }

    public void Active(int Index)
    {
        timeCount = 0;
        gmIndex = Index;
        // TODO: Convert to Manager
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<ComsMotionSystem>().Active(Index);
        this.Enabled = true;
    }
}

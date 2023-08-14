using Unity.Entities;

public struct ClearFluidEvent : IComponentData
{
    // 触发流体重置事件，用于多轮仿真重置场景时使用
    public bool isActivate;
}
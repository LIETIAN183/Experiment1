using Unity.Entities;
using Unity.Collections;

public struct MessageEvent : IComponentData
{
    public bool isActivate;

    public FixedString64Bytes message;

    // 为 0 显示 2s，为 1 持续显示
    public int displayType;
}
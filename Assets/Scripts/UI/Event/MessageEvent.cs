using Unity.Entities;
using Unity.Collections;

public struct MessageEvent : IComponentData
{
    public bool isActivate;

    public FixedString64Bytes message;
    public bool displayForever;
}
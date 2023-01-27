using Unity.Entities;

public struct LoadSceneEvent : IComponentData
{
    public bool isActivate;
    public int index;
    public float targetPGA;
}
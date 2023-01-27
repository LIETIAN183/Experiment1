using Unity.Entities;

public struct StartSeismicEvent : IComponentData
{
    public bool isActivate;
    public int index;
    public float targetPGA;
}
using Unity.Entities;

public struct EntityMovementData : IComponentData
{
    public float moveSpeed;
    public float destinationMoveSpeed;
    public bool destinationReached;
}
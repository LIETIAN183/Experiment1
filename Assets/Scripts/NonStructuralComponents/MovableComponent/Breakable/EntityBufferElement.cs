
using Unity.Entities;

[InternalBufferCapacity(10)]
public struct EntityBufferElement : IBufferElementData
{
    public Entity replacementItem;

    public static implicit operator Entity(EntityBufferElement entityBufferElement) => entityBufferElement.replacementItem;

    public static implicit operator EntityBufferElement(Entity e) => new EntityBufferElement { replacementItem = e };
}


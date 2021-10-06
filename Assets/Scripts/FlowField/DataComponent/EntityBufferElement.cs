using Unity.Entities;

[GenerateAuthoringComponent, InternalBufferCapacity(250)]
public struct CellBufferElement : IBufferElementData
{
    // public Entity entity;

    public CellData cell;

    public static implicit operator CellData(CellBufferElement entityBufferElement)
    {
        return entityBufferElement.cell;
    }

    public static implicit operator CellBufferElement(CellData e)
    {
        return new CellBufferElement { cell = e };
    }
}

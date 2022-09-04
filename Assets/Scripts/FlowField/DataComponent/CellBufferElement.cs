using Unity.Entities;

// [GenerateAuthoringComponent]
[InternalBufferCapacity(250)]
public struct CellBufferElement : IBufferElementData
{
    public CellData cell;

    public static implicit operator CellData(CellBufferElement cellBufferElement) => cellBufferElement.cell;

    public static implicit operator CellBufferElement(CellData e) => new CellBufferElement { cell = e };
}

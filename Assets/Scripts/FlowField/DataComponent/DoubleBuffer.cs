using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(250)]
public struct DoubleBuffer : IBufferElementData
{
    public CellData cell;

    public static implicit operator CellData(DoubleBuffer doubleBufferElement) => doubleBufferElement.cell;

    public static implicit operator DoubleBuffer(CellData e) => new DoubleBuffer { cell = e };
}

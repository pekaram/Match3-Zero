public struct SlotIndex
{
    public readonly int Row { get; }

    public readonly int Column { get; }

    public SlotIndex(in int row, in int column)
    {
        Row = row;
        Column = column;
    }
}

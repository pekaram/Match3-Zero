public readonly struct SlotIndex
{
    public int Row { get; }

    public int Column { get; }

    public SlotIndex(in int row, in int column)
    {
        Row = row;
        Column = column;
    }
}

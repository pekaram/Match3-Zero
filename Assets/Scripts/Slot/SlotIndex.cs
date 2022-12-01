public struct SlotIndex
{
    public int Row { get; }

    public int Column { get; }

    public SlotIndex(int row, int column)
    {
        Row = row;
        Column = column;
    }
}

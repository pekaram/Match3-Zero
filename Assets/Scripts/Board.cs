using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class Board 
{
    private readonly List<List<Slot>> _slots;

    private bool _awaitingVisuals = false;

    public Board(List<List<Slot>> slots)
    {
        _slots = slots;

        var allSlots = _slots.SelectMany(p => p);
        foreach (var slot in allSlots)
        {
            slot.OnSlotClicked += OnSlotClick;
        }
    }

    private async UniTask RemoveMatchingCells()
    {
        var visuals = new List<UniTask>();
        for (var row = _slots.Count - 1; row >= 0; row--)
        {
            var matchedSlots = GetRowMatches(row);
            foreach (var matchedSlot in matchedSlots)
            {
                matchedSlot.DestroyShape();
                visuals.Add(ShiftColumn(matchedSlot));
            }

            if (visuals.Count > 0)
            {  
                await UniTask.WhenAll(visuals);
                visuals.Clear();

                // Board changed, reset pointer to look if new matches formed
                row = _slots.Count - 1;
            }
        }
    }

    private async void OnSlotClick(Slot clickedSlot)
    {
        if (_awaitingVisuals)
        {
            UnityEngine.Debug.Log("Ignoring new user click, still updating visuals");
            return;
        }

        clickedSlot.DestroyShape();

        if (clickedSlot.Index.Row == _slots.Count - 1)
        {
            return;
        }

        _awaitingVisuals = true;

        await ShiftColumn(clickedSlot);
        await RemoveMatchingCells();

        _awaitingVisuals = false;
    }

    private async UniTask ShiftColumn(Slot fromSlot)
    {
        for (var row = fromSlot.Index.Row; row < _slots.Count; row++)
        {
            if (row == _slots.Count - 1)
            {
                _slots[row][fromSlot.Index.Column].RemoveShape();
                break;
            }

            _slots[row][fromSlot.Index.Column].ReceiveShape(_slots[row + 1][fromSlot.Index.Column].Shape);
        }

        var viewUpdates = new List<UniTask>();
        for (var row = fromSlot.Index.Row; row < _slots.Count; row++)
        {
            viewUpdates.Add(_slots[row][fromSlot.Index.Column].UpdateView());
        }
        await UniTask.WhenAll(viewUpdates);
    }

    private IEnumerable<Slot> GetRowMatches(int row)
    {
        var vanishingSlots = new HashSet<Slot>();

        for (var column = 1; column < _slots[row].Count - 1; column++)
        {
            if (Match3(_slots[row][column - 1], _slots[row][column], _slots[row][column + 1]))
            {
                vanishingSlots.Add(_slots[row][column - 1]);
                vanishingSlots.Add(_slots[row][column]);
                vanishingSlots.Add(_slots[row][column + 1]);
            }
        }

        return vanishingSlots; 
    }

    private bool Match3(Slot first, Slot second, Slot third)
    {
        return first.ContainmentType == second.ContainmentType &&
                second.ContainmentType == third.ContainmentType &&
                first.ContainmentType != -1;
    }

    public void Destroy()
    {
        var allSlots = _slots.SelectMany(p => p);
        foreach (var slot in allSlots)
        {
            slot.OnSlotClicked -= OnSlotClick;
            slot.DestroyShape();
        }

        _slots.Clear();
    }
}
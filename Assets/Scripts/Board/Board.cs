using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

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

    private async UniTask RemoveAllMatches()
    {
        var visualUpdates = ListPool<UniTask>.Get();
        for (var row = _slots.Count - 1; row >= 0; row--)
        {
            if (RemoveRowMatches(row, visualUpdates))
            {
                await UniTask.WhenAll(visualUpdates);
                visualUpdates.Clear();

                // Board changed, reset pointer to look if new matches formed
                row = _slots.Count - 1;
            }
        }
        ListPool<UniTask>.Release(visualUpdates);
    }

    private bool RemoveRowMatches(int row, List<UniTask> visualTasks)
    {
        var matches = HashSetPool<Slot>.Get();
        var anyMatches = GetRowMatchesNonAlloc(row, matches);
        foreach (var matchedSlot in matches)
        {
            matchedSlot.DestroyShape();
            visualTasks.Add(ShiftColumn(matchedSlot));
        }
        HashSetPool<Slot>.Release(matches);

        return anyMatches;
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
        await RemoveAllMatches();

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

        var viewUpdates = ListPool<UniTask>.Get();
        for (var row = fromSlot.Index.Row; row < _slots.Count; row++)
        {
            viewUpdates.Add(_slots[row][fromSlot.Index.Column].UpdateView());
        }
        await UniTask.WhenAll(viewUpdates);
        ListPool<UniTask>.Release(viewUpdates);
    }

    private bool GetRowMatchesNonAlloc(int row, HashSet<Slot> matches)
    {
        for (var column = 1; column < _slots[row].Count - 1; column++)
        {
            if (Match3(_slots[row][column - 1], _slots[row][column], _slots[row][column + 1]))
            {
                matches.Add(_slots[row][column - 1]);
                matches.Add(_slots[row][column]);
                matches.Add(_slots[row][column + 1]);
            }
        }

        return matches.Count > 0;
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
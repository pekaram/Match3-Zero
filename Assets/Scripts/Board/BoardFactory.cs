using System.Collections.Generic;
using UnityEngine;

public class BoardFactory : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private BoardSettings _settings;
    
    private Board _board;

    private ref readonly Board Board { get { return ref _board; } }

    private void Start()
    {
        _board = CreateBoard(_settings);

        ZoomFit(_camera, _settings);
    }

    private Board CreateBoard(in BoardSettings settings)
    {
        var slots = new List<List<Slot>>();
        for (var row = 0; row < settings.Rows; row++)
        {
            slots.Add(new List<Slot>());
            for (var column = 0; column < settings.Columns; column++)
            {
                var slot = CreateSlot(row, column, settings);
                slots[row].Add(slot);
            }
        }

        return new Board(slots);
    }

    private Slot CreateSlot(in int rowIndex, in int columnIndex, in BoardSettings boardSettings)
    {
        var slotGameObject = Instantiate(boardSettings.SlotPrefab, boardSettings.ParentTransform);
        slotGameObject.transform.localScale = new Vector3(boardSettings.SlotWidth, boardSettings.SlotHeight, boardSettings.SlotPrefab.transform.localScale.z);
        slotGameObject.transform.position = new Vector3(columnIndex * boardSettings.SlotWidth, rowIndex * boardSettings.SlotHeight, boardSettings.SlotPrefab.transform.position.z);
        slotGameObject.name = $"Slot {rowIndex} {columnIndex}";

        // TODO: Randomize 
        var random = new System.Random(rowIndex + columnIndex);
        var shapeIndex = random.Next(0, boardSettings.ShapesPrefabs.Count);
        var shape = Instantiate(boardSettings.ShapesPrefabs[shapeIndex], slotGameObject.transform);
        shape.name = $"Originated at {rowIndex} {columnIndex}";
        
        var slot = new Slot(slotGameObject, shape, new SlotIndex(rowIndex, columnIndex));
        return slot;
    }

    private void ZoomFit(Camera camera, in BoardSettings boardSettings)
    {
        var totalHeight = boardSettings.SlotHeight * boardSettings.Rows;
        var totalWidth = boardSettings.SlotWidth * boardSettings.SlotWidth;
        float maxExtent = Mathf.Max(totalHeight, totalWidth);

        var lastSlotX = boardSettings.SlotWidth * (boardSettings.Columns - 1);
        var lastSlotY = boardSettings.SlotHeight * (boardSettings.Rows - 1);
        var center = new Vector3(lastSlotX / 2, lastSlotY / 2);

        float visibilityRadius = Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView);       
        camera.transform.position = Vector3.back * (visibilityRadius * 2 * maxExtent) + center;
    }

    private void OnDestroy()
    {
        _board.Destroy();
    }
}
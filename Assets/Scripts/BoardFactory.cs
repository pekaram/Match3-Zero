using System.Collections.Generic;
using UnityEngine;

public class BoardFactory : MonoBehaviour
{
    [SerializeField, Range(3, 100)]
    private int _rows;

    [SerializeField, Range(3, 100)]
    private int _columns;

    [SerializeField]
    private float _slotWidth;

    [SerializeField]
    private float _slotHeight;

    [SerializeField]
    private GameObject _slotPrefab;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private List<Shape> ShapesPrefabs;

    private Board _board;

    private void Start()
    {
        _board = CreateBoard();

        ZoomFit(_camera);
    }

    private Board CreateBoard()
    {
        var slots = new List<List<Slot>>();
        var slotsContainer = new GameObject("Board");
        for (var row = 0; row < _rows; row++)
        {
            slots.Add(new List<Slot>());
            for (var column = 0; column < _columns; column++)
            {
                var slot = CreateSlot(row, column, slotsContainer.transform);
                slots[row].Add(slot);
            }
        }

        return new Board(slots);
    }

    private Slot CreateSlot(int rowIndex, int columnIndex, Transform parentContainer)
    {
        var slotGameObject = Instantiate(_slotPrefab, parentContainer);
        slotGameObject.transform.localScale = new Vector3(_slotWidth, _slotHeight, _slotPrefab.transform.localScale.z);
        slotGameObject.transform.position = new Vector3(columnIndex * _slotWidth, rowIndex * _slotHeight, _slotPrefab.transform.position.z);
        slotGameObject.name = $"Slot {rowIndex} {columnIndex}";

        // TODO: Randomize 
        var random = new System.Random(rowIndex + columnIndex);
        var shapeIndex = random.Next(0, ShapesPrefabs.Count);
        var shape = Instantiate(ShapesPrefabs[shapeIndex], slotGameObject.transform);
        shape.name = $"Originated at {rowIndex} {columnIndex}";
        
        var slot = new Slot(slotGameObject, shape, new SlotIndex(rowIndex, columnIndex));
        return slot;
    }

    private void ZoomFit(Camera camera)
    {
        var totalHeight = _slotHeight * _rows;
        var totalWidth = _slotWidth * _columns;
        float maxExtent = Mathf.Max(totalHeight, totalWidth);

        var lastSlotX = _slotWidth * (_columns - 1);
        var lastSlotY = _slotHeight * (_rows - 1);
        var center = new Vector3(lastSlotX / 2, lastSlotY / 2);

        float visibilityRadius = Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView);       
        camera.transform.position = Vector3.back * (visibilityRadius * 2 * maxExtent) + center;
    }

    private void OnDestroy()
    {
        _board.Destroy();
    }
}
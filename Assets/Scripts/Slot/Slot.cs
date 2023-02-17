using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class Slot
{
    private const float ShapeDistanceThreshold = 0.015f;

    private const float ShapeMoveSpeed = 0.15f;

    private readonly Vector3 _localPosition;

    private readonly GameObject _slotGameObject;

    public int ContainmentType => Shape ? Shape.TypeIndex : -1;

    public Shape Shape { get; private set; }

    public SlotIndex Index { get; }

    public event Action<Slot> OnSlotClicked;

    public Slot(GameObject slotGameObject, Shape shape, in SlotIndex slotIndex)
    {
        _slotGameObject = slotGameObject;
        Shape = shape;
        Index = slotIndex;

        _localPosition = shape.transform.localPosition;
        shape.OnShapeClicked += OnShapeClicked;
    }

    public void RemoveShape()
    {
        if (!Shape)
        {
            return;
        }

        Shape.OnShapeClicked -= OnShapeClicked;
        Shape = null;
    }

    public void ReceiveShape(Shape shape)
    {
        if (Shape)
        {
            Shape.OnShapeClicked -= OnShapeClicked;
        }

        Shape = shape;

        if (shape)
        {
            shape.OnShapeClicked += OnShapeClicked;
            shape.transform.SetParent(_slotGameObject.transform, true);
        }
    }

    public async UniTask UpdateView()
    {
        while (Shape && Vector3.Distance(Shape.transform.localPosition, _localPosition) > ShapeDistanceThreshold)
        {
            Shape.transform.localPosition = Vector3.Lerp(Shape.transform.localPosition, _localPosition, ShapeMoveSpeed);
            await UniTask.WaitForFixedUpdate();
        }
    }

    public void DestroyShape()
    {
        if (Shape == null)
        {
            return;
        }

        Shape.OnShapeClicked -= OnShapeClicked;
        GameObject.Destroy(Shape.gameObject);
        Shape = null;
    }

    private void OnShapeClicked()
    {
        OnSlotClicked?.Invoke(this);
    }
}
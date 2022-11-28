using System;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [SerializeField]
    public int TypeIndex;

    public event Action OnShapeClicked;

    public void OnMouseDown()
    {
        OnShapeClicked?.Invoke();
    }
}
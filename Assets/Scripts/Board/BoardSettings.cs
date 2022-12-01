using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoardSettings
{
    [Range(3, 100)]
    public int Rows;

    [Range(3, 100)]
    public int Columns;

    [SerializeField]
    public float SlotWidth;

    [SerializeField]
    public float SlotHeight;

    [SerializeField]
    public GameObject SlotPrefab;

    [SerializeField]
    public List<Shape> ShapesPrefabs;

    [SerializeField]
    public Transform ParentTransform;
}

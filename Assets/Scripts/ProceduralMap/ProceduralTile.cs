using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTile : PooledMonoBehaviour
{
    [SerializeField] private float baseChance = 1;
    [SerializeField] private Transform initialPoint;
    [SerializeField] private List<LastPoints> lastPoints = new List<LastPoints>();

    public List<LastPoints> LastPoints { get => lastPoints; }
    public Transform InitialPoint { get => initialPoint; }
    public float BaseChance { get => baseChance;}
}

[System.Serializable]
public class LastPoints
{
    public Transform point;
    public Directions direcion;
}

public enum Directions
{
    left = -1,
    center = 0,
    right = 1,
}
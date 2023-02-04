using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLane
{
    private int continuousTiles = 0;
    private float currentDistance;
    private Stack<ProceduralTile> internalTiles = new Stack<ProceduralTile>();

    public bool Open { get; set; } = false;
    public bool TemporalOpen { get; set; } = false;

    public Vector3 SpawnPoint { get; set; }
    public Vector3 TemporalSpawnPoint { get; set; }

    public int ContinuousTiles { get => continuousTiles; }
    public float CurrentDistance { get => currentDistance;}

    public bool AllowTerrainIntercept(float targetPos)
    {
        //Debug.Log(targetPos > currentDistance);
        return targetPos > currentDistance;
    }

    public void UpdateCurrentDisance(float distance)
    {       
        currentDistance = distance;
    }

    public void AddTile(ProceduralTile tile, float safeZoneDistance)
    {
        var centerPoint = tile.LastPoints.Find((_) => _.direcion == Directions.center);
        if (centerPoint == null)
        {
            continuousTiles = 0;
            UpdateCurrentDisance(tile.InitialPoint.position.z + (safeZoneDistance));
            TemporalOpen = false;
        }
        else
        {
            continuousTiles++;
            UpdateCurrentDisance(centerPoint.point.position.z + safeZoneDistance );
        }

        internalTiles.Push(tile);
    }

    public void ClearTiles(Vector3 target, float removeDistance, bool clearAll = false)
    {
        if (clearAll)
        {
            foreach (var item in internalTiles)
            {
                item.gameObject.SetActive(false);
            }
            internalTiles.Clear();
            return;
        }
        var tempInternalTiles = new Stack<ProceduralTile>(internalTiles).ToArray();
        internalTiles.Clear();
        foreach (var item in tempInternalTiles)
        {
            if (Vector3.Distance(target, item.transform.position) < removeDistance)
            {
                internalTiles.Push(item);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerator : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float initialLanePos;
    [SerializeField] private Vector3 initialTilePos;

    [SerializeField] private int maxNumberOfLanesToEachSide;
    [SerializeField] private float tileSafeZone;
    [SerializeField] private float laneSeparation = 1;

    [SerializeField] private float generationDistanceRadius;
    [SerializeField] private float destroyDistanceRadius;

    [SerializeField] private int initialGeneretion = 10;

    [SerializeField] private float updateInterval = 0.1f;

    [SerializeField] private List<ProceduralTile> tilesPrefab = new List<ProceduralTile>();

    private Dictionary<float, ProceduralLane> lanes = new Dictionary<float, ProceduralLane>();

    private List<ProceduralTile> tilesEndPrefab = new List<ProceduralTile>();

    private int generateLayersCount = 0;

    private bool generating = false;
    private float CurrentLanePos { get => Mathf.RoundToInt(target.position.x / laneSeparation) * laneSeparation; }

    private void Start()
    {
        laneSeparation = Mathf.Abs(laneSeparation);
        tilesEndPrefab = new List<ProceduralTile>(tilesPrefab);
        tilesEndPrefab.RemoveAll((_) => _.LastPoints.Count > 0);

        lanes.Add(initialLanePos, new ProceduralLane());
        lanes[initialLanePos].TemporalOpen = true;
        lanes[initialLanePos].TemporalSpawnPoint = initialTilePos;
        lanes[initialLanePos].UpdateCurrentDisance(initialTilePos.z + tileSafeZone);

        GenerateLanes();
        generateLayersCount = initialGeneretion;
        StartCoroutine(C_UpdateGeneration());
    }

    private ProceduralLane GetLane(float pos)
    {
        foreach (var item in lanes)
        {
            if (pos < (item.Key + (laneSeparation / 5f)) && pos > (item.Key - (laneSeparation / 5f)))
            {
                return item.Value;
            }
        }
        return null;
    }

    public void GenerateLanes()
    {
        List<float> unusedLanes = new List<float>();
        foreach (var item in lanes)
        {
            unusedLanes.Add(item.Key);
        }
        Dictionary<float, ProceduralLane> newLanes = new Dictionary<float, ProceduralLane>();

        for (int i = (-1 * maxNumberOfLanesToEachSide); i <= (maxNumberOfLanesToEachSide); i++)
        {
            float pos = CurrentLanePos + (i * laneSeparation);

            if (lanes.ContainsKey(pos))
            {
                newLanes.Add(pos, lanes[pos]);
                unusedLanes.Remove(pos);
            }
            else
            {
                newLanes.Add(pos, new ProceduralLane());
            }
        }

        foreach (var item in unusedLanes)
        {
            lanes[item].ClearTiles(target.position, destroyDistanceRadius, true);
        }

        lanes = new Dictionary<float, ProceduralLane>(newLanes);
    }

    public void GenerateLayer()
    {
        foreach (var item in lanes)
        {
            item.Value.ClearTiles(target.position, destroyDistanceRadius);
        }
        //StopCoroutine(c_GenerateLayer);
        //c_GenerateLayer = C_GenerateLayer();
        //StartCoroutine(c_GenerateLayer);
        generateLayersCount++;
    }

    private IEnumerator C_GenerateLayer()
    {
        if (generateLayersCount == 0) yield break;

        generating = true;

        int currentActiveLanes = 0;
        List<float> updateOrder = new List<float>();
        foreach (var item in lanes)
        {
            item.Value.Open = item.Value.TemporalOpen;
            item.Value.SpawnPoint = item.Value.TemporalSpawnPoint;
            if (item.Value.Open) currentActiveLanes++;
            updateOrder.Add(item.Key);
        }
        updateOrder.Sort((x, y) => Mathf.Abs(CurrentLanePos - x).CompareTo(Mathf.Abs(CurrentLanePos - y)));

        foreach (var item in updateOrder)
        {
            var currentLane = GetLane(item);

            if (!currentLane.Open) continue;
            if (Mathf.Abs(currentLane.SpawnPoint.x - target.position.x) >= generationDistanceRadius) continue;
            if (Mathf.Abs(currentLane.SpawnPoint.z - target.position.z) >= generationDistanceRadius) continue;

            var tempTilesPrefabs = new List<ProceduralTile>(tilesPrefab);

            tempTilesPrefabs.RemoveAll(RemoveConditions(item, currentLane.CurrentDistance, currentActiveLanes));

            Dictionary<float, ProceduralTile> probabilityTable = new Dictionary<float, ProceduralTile>();

            float probabilityCount = 0f;

            foreach (var _item in tempTilesPrefabs)
            {
                if (_item.BaseChance <= 0) continue;
                probabilityCount += _item.BaseChance;
                probabilityTable.Add(probabilityCount, _item);
            }

            float randomValue = Random.Range(0f, probabilityCount);

            ProceduralTile selectedTile = tilesEndPrefab[Random.Range(0, tilesEndPrefab.Count)];

            foreach (var _item in probabilityTable)
            {
                if (randomValue < _item.Key)
                {
                    selectedTile = _item.Value;
                    break;
                }
            }

            var tileInstance = selectedTile.Get<ProceduralTile>();
            tileInstance.transform.position = currentLane.SpawnPoint;

            if(tileInstance.LastPoints.Count <= 0)
            {
                currentActiveLanes--;
            }

            foreach (var _item in tileInstance.LastPoints)
            {
                ProceduralLane lane = null;

                lane = GetLane(item + (laneSeparation * (int)_item.direcion));
                if (lane == null) continue;
                lane.TemporalOpen = true;
                lane.TemporalSpawnPoint = _item.point.position;

                lane.UpdateCurrentDisance(lane.TemporalSpawnPoint.z + (tileSafeZone));

                lanes[item + (laneSeparation * (int)_item.direcion)] = lane;
            }


            currentLane.AddTile(tileInstance, tileSafeZone);

            yield return new WaitForEndOfFrame();
        }

        generateLayersCount--;
        if(generateLayersCount > 0)
        {
            yield return StartCoroutine(C_GenerateLayer());
        }
        else
        {
            generating = false;
        }
    }

    private IEnumerator C_UpdateGeneration()
    {
        float previousLanePos = CurrentLanePos;
        while (true)
        {
            if(previousLanePos != CurrentLanePos)
            {
                GenerateLanes();
            }

            GenerateLayer();

            if(!generating && generateLayersCount > 0)
            {
                StartCoroutine(C_GenerateLayer());
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private System.Predicate<ProceduralTile> RemoveConditions(float lanePos, float currentDistance, int currentActiveLanes)
    {
        return (_) =>
        {
            bool result = false;

            if (GetLane(lanePos - laneSeparation) == null)
            {
                result = _.LastPoints.Find((__) => __.direcion == Directions.left) != null ? true : result;
            }

            if (GetLane(lanePos + laneSeparation) == null)
            {
                result = _.LastPoints.Find((__) => __.direcion == Directions.right) != null ? true : result;
            }

            if (GetLane(lanePos - laneSeparation) != null 
            && !GetLane(lanePos - laneSeparation).AllowTerrainIntercept(currentDistance))
            {
                result = _.LastPoints.Find((__) => __.direcion == Directions.left) != null ? true : result;
            }

            if (GetLane(lanePos + laneSeparation) != null 
            && !GetLane(lanePos + laneSeparation).AllowTerrainIntercept(currentDistance))
            {
                result = _.LastPoints.Find((__) => __.direcion == Directions.right) != null ? true : result;
            }

            if (GetLane(lanePos + laneSeparation) != null 
            && GetLane(lanePos - laneSeparation) != null
            && GetLane(lanePos + laneSeparation).AllowTerrainIntercept(currentDistance)
            && GetLane(lanePos - laneSeparation).AllowTerrainIntercept(currentDistance))
            {
                result = _.LastPoints.Count <= 0 ? true : result;
            }

            if(currentActiveLanes < 2)
            {
                result = _.LastPoints.Count <= 0 ? true : result;
            }

            if(currentDistance < 120)
            {
                result = _.LastPoints.Find((__) => __.direcion == Directions.right) != null ? true : result;
                result = _.LastPoints.Find((__) => __.direcion == Directions.left) != null ? true : result;
            }

            return result;
        };
    }
}

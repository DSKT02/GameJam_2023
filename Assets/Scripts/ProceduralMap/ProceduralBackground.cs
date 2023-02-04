using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralBackground : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private Vector2 gridCellSize;
    [SerializeField] private Vector2 gridSize;

    [SerializeField] private Vector3 initialPos;

    [SerializeField] private ProceduralBackgroundTile backgroundtilePrefab;

    [SerializeField] private float tilesUpdateCycle = 0.5f;

    private Dictionary<Vector2, ProceduralBackgroundTile> tiles = new Dictionary<Vector2, ProceduralBackgroundTile>();

    private void Start()
    {
        StartCoroutine(C_UpdateTiles());
    }

    public List<Vector2> CalculateTiles()
    {
        List<Vector2> output = new List<Vector2>();
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Vector2 pos = new Vector2(i * gridCellSize.x, j * gridCellSize.y)
                    + new Vector2((int)(target.position.x / gridCellSize.x) * gridCellSize.x, (int)(target.position.z / gridCellSize.y) * gridCellSize.y)
                    + new Vector2(initialPos.x, initialPos.z)
                    - new Vector2(gridCellSize.x * gridSize.x / 2f, gridCellSize.y * gridSize.y / 2f)
                    + new Vector2(gridCellSize.x / 2f, gridCellSize.y / 2f);
                output.Add(pos);
            }
        }
        return output;
    }

    private void UpdateTiles()
    {
        var grid = CalculateTiles();

        List<Vector2> unusedCells = new List<Vector2>(tiles.Keys);

        foreach (var item in grid)
        {
            if (tiles.ContainsKey(item))
            {
                unusedCells.Remove(item);
            }
            else
            {
                tiles.Add(item, backgroundtilePrefab.Get<ProceduralBackgroundTile>());
                tiles[item].transform.position = new Vector3(item.x, initialPos.y, item.y);
            }
        }

        foreach (var item in unusedCells)
        {
            tiles[item].gameObject.SetActive(false);
            tiles.Remove(item);
        }
    }

    private IEnumerator C_UpdateTiles()
    {
        while (true)
        {
            UpdateTiles();
            yield return new WaitForSeconds(tilesUpdateCycle);
        }
    }
}

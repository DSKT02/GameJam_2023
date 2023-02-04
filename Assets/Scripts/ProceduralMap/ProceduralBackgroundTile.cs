using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralBackgroundTile : PooledMonoBehaviour
{
    [SerializeField] private List<GameObject> options;

    private void OnEnable()
    {
        int randomValue = Random.Range(0, options.Count);

        for (int i = 0; i < options.Count; i++)
        {
            options[i].SetActive(randomValue == i);
        }
    }
}

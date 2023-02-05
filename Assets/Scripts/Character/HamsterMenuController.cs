using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HamsterMenuController : MonoBehaviour
{
    [SerializeField] private float amp = 1;
    [SerializeField] private float frec = 1;

    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
    }

    private void Update()
    {
        transform.position = initialPos + ((amp * Mathf.Cos(frec * Time.time)) * new Vector3(1, 0.5f, 0));
    }
}

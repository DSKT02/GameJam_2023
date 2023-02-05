using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
    }

    private void FixedUpdate()
    {
        transform.position = new Vector3(target.position.x, initialPos.y, target.position.z);
    }
}

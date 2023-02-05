using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionIndicator : MonoBehaviour
{
    [SerializeField] private PlayerMovementInput _player;

    [SerializeField] private Transform _indicator;
    [SerializeField] private float maxRotation;

    private void Update()
    {
        _indicator.rotation = Quaternion.Euler(0, 0, -maxRotation * _player.RootPosition);
    }
}

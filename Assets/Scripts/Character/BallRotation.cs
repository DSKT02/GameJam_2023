using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRotation : MonoBehaviour
{
    [SerializeField] private bool _isRunning;
    [SerializeField] private Transform _sphereBone;
    [SerializeField] private float _speed;

    public void SetRunningState(bool isRunning)
    {
        _isRunning = isRunning;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _sphereBone.Rotate(new Vector3(_speed*Time.deltaTime, 0,0));
    }
}

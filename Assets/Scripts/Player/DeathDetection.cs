using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DeathTrap>(out var trap))
        {
            GameFlowManager.Instance.PlayerDied();
        }
        if (other.TryGetComponent<DeathZone>(out var zone))
        {
            GameFlowManager.Instance.PlayerDied();        
        }
    }
}

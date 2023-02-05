using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoInteractuableZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Acorn>(out var acorn))
        {
            acorn.Disable();
        }
        if(other.TryGetComponent<DeathTrap>(out var trap))
        {
            trap.Disable();
        }
    }
}

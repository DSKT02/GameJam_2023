using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCollectablesZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Acorn>(out var acorn))
        {
            acorn.Disable();
        }
    }
}

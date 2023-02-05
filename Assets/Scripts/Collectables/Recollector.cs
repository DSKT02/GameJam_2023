using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Acorn>(out var acorn))
        {
            acorn.Disable();
            CollectablesManager.Instance.CurrentAcorns++;
        }
    }
}

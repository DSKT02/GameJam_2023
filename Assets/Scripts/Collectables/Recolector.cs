using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recolector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Acorn>(out var acorn))
        {
            acorn.gameObject.SetActive(false);
            CollectablesManager.Instance.CurrentAcorns++;
        }
    }
}

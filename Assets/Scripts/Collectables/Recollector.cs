using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recollector : MonoBehaviour
{
    [SerializeField] private MenuController menu;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Acorn>(out var acorn))
        {
            acorn.Disable();
            CollectablesManager.Instance.CurrentAcorns++;
            menu.UpdateHighscores();
        }
    }
}

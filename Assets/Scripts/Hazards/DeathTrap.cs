using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrap : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> meshes;
    [SerializeField] private Collider collider;

    public void Disable()
    {
        foreach (var item in meshes)
        {
            item.enabled = false;
        }
        collider.enabled = false;
    }

    private void OnEnable()
    {
        foreach (var item in meshes)
        {
            item.enabled = true;
        }
        collider.enabled = true;
    }
}

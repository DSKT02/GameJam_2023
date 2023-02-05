using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acorn : MonoBehaviour
{
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private Collider collider;

    [SerializeField] private float frec;
    [SerializeField] private float amp;

    [SerializeField] private float rotationSpeed;

    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.localPosition;
    }

    public void Disable()
    {
        mesh.enabled = false;
        collider.enabled = false;
    }

    private void OnEnable()
    {
        mesh.enabled = true;
        collider.enabled = true;
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        transform.localPosition = initialPos + ((amp * Mathf.Cos(frec * Time.time)) * new Vector3(1, 0.5f, 0));
    }
}

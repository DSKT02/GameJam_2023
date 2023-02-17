using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acorn : MonoBehaviour
{
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private Collider m_collider;

    [SerializeField] private float frec;
    [SerializeField] private float amp;

    [SerializeField] private float rotationSpeed;

    [SerializeField] private AudioSource _audioSource;

    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.localPosition;
    }

    public void Disable(bool collect)
    {
        mesh.enabled = false;
        m_collider.enabled = false;

        if (collect)
        {
            _audioSource.Play();
        }
    }

    private void OnEnable()
    {
        mesh.enabled = true;
        m_collider.enabled = true;
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        transform.localPosition = initialPos + ((amp * Mathf.Cos(frec * Time.time)) * new Vector3(1, 0f, 0));
    }
}

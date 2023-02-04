using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementInput : MonoBehaviour
{
    private Transform _playerTransform;
    private float _screenWidth;
    private float _velocity = 0f;
    private float _smoothTime = 0.1f;
    [SerializeField] private float inertia = 10f;
    [SerializeField] private float radious = 1f;

    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
        _screenWidth = Screen.width;
#if UNITY_EDITOR
        print($"Touch Support: {Input.touchSupported}");
#endif
    }

    private void Update()
    {
        GetInputTouh();
    }

    private void GetInputTouh()
    {
        if (Input.touchSupported)
        {
            if (Input.touchCount > 0)
            {
                Vector2 touchPos = Input.GetTouch(0).position;

                if (touchPos.x > _screenWidth / 2)
                {
                    // Move right
                    _velocity = inertia;
                }
                else
                {
                    // Move left
                    _velocity = -inertia;
                }
            }
        }
        else
        {
            _velocity = Input.GetAxis("Horizontal") * inertia;
            
        }

        float newXPos = Mathf.Clamp( Mathf.SmoothDamp(_playerTransform.position.x, _playerTransform.position.x + _velocity * Time.deltaTime, ref _velocity, _smoothTime), -radious, radious);
        float newYPos = Mathf.Sqrt(Mathf.Pow(radious, 2) - Mathf.Pow(newXPos, 2));

        _playerTransform.localPosition = new Vector3(newXPos, newYPos, _playerTransform.position.z);
    }
}

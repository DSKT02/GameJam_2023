using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementInput : MonoBehaviour
{
    [SerializeField] private float lateralAceleration = 10f;
    [SerializeField] private float movementRadius = 3f;
    [SerializeField]
    [Range(3, 10)] private int rightFallThreshold = 7;
    [SerializeField]
    [Range(3, 10)] private int leftFallThreshold = 7;
    [SerializeField] private float forwardVelocity = 5f;

    private Transform _playerTransform;
    private Transform _rootPlayerTransform;
    private float _screenWidth;
    private float _lateralVelocity = 0f;
    private float _smoothTime = .1f;
    private float _initalXPos = 0f;
    private float _rightFallThreshold = 0f;
    private float _leftFallThreshold = 0f;

    // Test Rotation
    private float _rotateInterval = 3f;
    private float _timer;
    private float _rotationTime = .5f;
    private float _degreesToRotate = 90f;
    private float _rotationSpeed;

    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
        _rootPlayerTransform = _playerTransform.parent.GetComponent<Transform>();
        _screenWidth = Screen.width;
        _initalXPos = _playerTransform.localPosition.x;

        _rightFallThreshold = (_initalXPos + (movementRadius * (rightFallThreshold / 10f)));
        _leftFallThreshold = (_initalXPos - (movementRadius * (leftFallThreshold / 10f)));

        _rotationSpeed = _degreesToRotate / _rotationTime;

        print($"Right Threshold: {_rightFallThreshold}");
        print($"Left Threshold: {_leftFallThreshold}");

#if UNITY_EDITOR
        print($"Touch Support: {Input.touchSupported}");
#endif
    }

    private void Update()
    {
        LateralMovement();
        MoveForward();
    }

    private void LateralMovement()
    {
        if (Input.touchSupported)
        {
            if (Input.touchCount > 0)
            {
                Vector2 touchPos = Input.GetTouch(0).position;

                if (touchPos.x > _screenWidth / 2)
                {
                    // Move right
                    _lateralVelocity = lateralAceleration;
                }
                else
                {
                    // Move left
                    _lateralVelocity = -lateralAceleration;
                }
            }
            else
            {
                StartFalling();
            }
        }
        else
        {
            float input = Input.GetAxis("Horizontal");
            

            if (input == 0)
            {
                StartFalling();
            }
            else
            {
                _lateralVelocity = input * lateralAceleration;
            }
        }

        float newXPos = Mathf.Clamp( Mathf.SmoothDamp(_playerTransform.localPosition.x, _playerTransform.localPosition.x + _lateralVelocity * Time.deltaTime, ref _lateralVelocity, _smoothTime), -movementRadius, movementRadius);
        float newYPos = Mathf.Sqrt(Mathf.Pow(movementRadius, 2) - Mathf.Pow(newXPos, 2));

        _playerTransform.localPosition = new Vector3(newXPos, newYPos, _playerTransform.localPosition.z);
    }

    private void MoveForward()
    {
        _timer += Time.deltaTime;

        if (_timer >= _rotateInterval)
        {
            _timer = 0f;
            //_rootPlayerTransform.rotation *= Quaternion.Euler(0f, 90f, 0f);
            _rootPlayerTransform.RotateAround(_rootPlayerTransform.position, Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        _rootPlayerTransform.localPosition += _rootPlayerTransform.forward * forwardVelocity * Time.deltaTime;
    }

    private void StartFalling()
    {
        if (_playerTransform.localPosition.x > _rightFallThreshold && _playerTransform.localPosition.x < (_initalXPos + movementRadius))
        {
            // Falling right
            _lateralVelocity += lateralAceleration * Time.deltaTime;
#if UNITY_EDITOR
            print("Falling right");
#endif
        }

        if (_playerTransform.localPosition.x < _leftFallThreshold && _playerTransform.localPosition.x > (_initalXPos - movementRadius))
        {
            // Falling left
            _lateralVelocity -= lateralAceleration * Time.deltaTime;
#if UNITY_EDITOR
            print("Falling left");
#endif
        }
    }
}

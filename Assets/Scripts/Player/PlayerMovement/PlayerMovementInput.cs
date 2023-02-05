using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovementInput : MonoBehaviour
{
    [SerializeField] private float lateralAcceleration = 10f;
    [SerializeField] private float movementRadius = 3f;
    [SerializeField] private float forwardVelocity = 5f;
    [SerializeField] private bool useGyroscopeIfAvailable = true;

    private Transform _playerTransform;
    private Transform _rootPlayerTransform;
    private float _screenWidth;
    private float _lateralVelocity = 0f;
    private float _smoothTime = .1f;
    private float _touchTimer;
    private float _touchInput;
    private float _gyroVelocity = 0f;
    private float _friction = .9f;
    private float _gyroThreshold = .05f;

    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
        _rootPlayerTransform = _playerTransform.parent.GetComponent<Transform>();
        _screenWidth = Screen.width;

        Input.gyro.enabled = SystemInfo.supportsGyroscope && useGyroscopeIfAvailable;

#if UNITY_EDITOR
        print($"Touch Support: {Input.touchSupported}");
        print($"Gyro Support: {SystemInfo.supportsGyroscope}");
#endif
    }

    private void FixedUpdate()
    {
        LateralMovement();
        MoveForward();

        UpdatePlayerPosition();
    }

    private void UpdatePlayerPosition()
    {
        float velocitiy = (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable) ? _gyroVelocity : _lateralVelocity;
        float newXPos = Mathf.Clamp(Mathf.SmoothDamp(_playerTransform.localPosition.x, _playerTransform.localPosition.x + velocitiy * Time.deltaTime, ref _lateralVelocity, _smoothTime), -movementRadius, movementRadius);
        float newYPos = Mathf.Sqrt(Mathf.Pow(movementRadius, 2) - Mathf.Pow(newXPos, 2));

        _playerTransform.localPosition = new Vector3(newXPos, newYPos, _playerTransform.localPosition.z);
    }

    private void LateralMovement()
    {
        if (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable)
        {
            float horizontal = Input.gyro.rotationRateUnbiased.y;

            if (Input.gyro.rotationRateUnbiased.magnitude > _gyroThreshold)
            {
                _gyroVelocity += horizontal * lateralAcceleration;
                _gyroVelocity *= _friction;
            }
            else
            {
                Falling();
            }
        }
        else
        {
            if (Input.touchSupported)
            {
                if (Input.touchCount > 0)
                {
                    _touchTimer = 0;
                    Vector2 touchPos = Input.GetTouch(0).position;
                    _touchInput = Mathf.Clamp((touchPos.x > _screenWidth / 2) ? _touchInput + Time.deltaTime : _touchInput - Time.deltaTime, -1f, 1f);

                    _lateralVelocity = _touchInput * lateralAcceleration;
                }
                else
                {
                    // ramp it down to 0
                    if (_touchInput != 0)
                    {
                        _touchInput = Mathf.Lerp(_touchInput, 0, _touchTimer);
                        _touchTimer += 3f * Time.deltaTime;
                        _lateralVelocity = _touchInput * lateralAcceleration;
                    }
                    else
                    {
                        Falling();
                    }
                }
            }
            else
            {
                float input = Input.GetAxis("Horizontal");

                if (input == 0)
                {
                    Falling();
                }
                else
                {
                    _lateralVelocity = input * lateralAcceleration;
                }
            }
        }
    }

    private void MoveForward()
    {
        _rootPlayerTransform.localPosition += _rootPlayerTransform.forward * forwardVelocity * Time.deltaTime;
    }

    private void Falling()
    {
        if (_playerTransform.localPosition.x != 0)
        {
            if (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable)
            {
                _gyroVelocity += (lateralAcceleration * 2.75f) * _playerTransform.localPosition.x * Time.deltaTime;
            }
            else
            {
                _lateralVelocity += (lateralAcceleration * 2.75f) * _playerTransform.localPosition.x * Time.deltaTime;
            }

#if UNITY_EDITOR
            print($"Falling {(_playerTransform.localPosition.x > 0 ? "Right" : "Left")}");
#endif
        }
    }

    public void ToggleGyroscopeMovement(bool newValue)
    {
        useGyroscopeIfAvailable = newValue;
    }
}

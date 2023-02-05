using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerMovementInput : MonoBehaviour
{
    [SerializeField] private float lateralAcceleration = 10f;
    [SerializeField] private float movementRadius = 3f;
    [SerializeField] private float forwardVelocity = 5f;
    [SerializeField] private bool useGyroscopeIfAvailable = false;
    [SerializeField] private ProceduralGenerator proceduralGenerator;

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
    private float _maxLenghtLeftSubline = .6f;
    private float _maxLenghtCenterSubline = .3f;
    private float _maxLenghtRightSubline = .6f;
    private Directions _playerDirection;
    private Directions _lastTurn;
    private bool _isJumping = false;
    private Vector2 fingerDown;
    private Vector2 fingerUp;
    public bool detectSwipeOnlyAfterRelease = false;


    [SerializeField] private float _radiusPercentToLoose;
    public float RootPosition;

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
        Jump();

        LateralMovement();
        MoveForward();

        UpdatePlayerPosition();
        UpdatePlayerRotation();
        
        _playerDirection = GetPlayerDirection();

        CheckRootPosition();
        CheckLoose();
    }

    private void CheckRootPosition()
    {
        RootPosition = _playerTransform.localPosition.x / (movementRadius * _radiusPercentToLoose);
    }

    private void CheckLoose()
    {
        if (RootPosition <= -1 || RootPosition >= 1)
        {
            GameFlowManager.Instance.PlayerDied();
        }
    }

    private void UpdatePlayerPosition()
    {
        float velocitiy = (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable) ? _gyroVelocity : _lateralVelocity;
        float newXPos = Mathf.Clamp(Mathf.SmoothDamp(_playerTransform.localPosition.x, _playerTransform.localPosition.x + velocitiy * Time.deltaTime, ref _lateralVelocity, _smoothTime), -movementRadius, movementRadius);
        float newYPos = Mathf.Sqrt(Mathf.Pow(movementRadius, 2) - Mathf.Pow(newXPos, 2));

        _playerTransform.localPosition = new Vector3(newXPos, newYPos, _playerTransform.localPosition.z);
    }

    public void UpdatePlayerRotation()
    {
        Vector3 dir = _playerTransform.position - _rootPlayerTransform.position;
        _playerTransform.forward = _rootPlayerTransform.forward;
        Vector3 up = transform.TransformDirection(dir.normalized);
        _playerTransform.rotation = Quaternion.LookRotation(transform.forward, up);
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

    private Directions GetPlayerDirection()
    {
        if (_playerTransform.localPosition.x > (-movementRadius * _maxLenghtLeftSubline) &&
                _playerTransform.localPosition.x < (-movementRadius * _maxLenghtCenterSubline)) { return Directions.left; }

        if (_playerTransform.localPosition.x >= (-movementRadius * _maxLenghtCenterSubline) &&
            _playerTransform.localPosition.x <= (movementRadius * _maxLenghtCenterSubline)) { return Directions.center; }

        if (_playerTransform.localPosition.x > (movementRadius * _maxLenghtCenterSubline) &&
            _playerTransform.localPosition.x < (movementRadius * _maxLenghtRightSubline)) { return Directions.right; }

        Directions lastDirection = _playerDirection;
        return lastDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        ProceduralTile tile;
        if (other.transform.parent == null) { return; }

        if (other.transform.parent.TryGetComponent<ProceduralTile>(out tile))
        {

            if (other.name.ToLower().Contains("in"))
            {
                other.enabled = false;
                if (tile.LastPoints.Find((_) => _.direcion == _playerDirection) != null)
                {
                    _lastTurn = _playerDirection;
                    switch (_playerDirection)
                    {
                        case Directions.left:
                            TurnLeft();
                            break;
                        case Directions.right:
                            TurnRight();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (_playerDirection == Directions.center)
                    {
                        _lastTurn = Directions.right;
                        TurnRight();
                    }
                }
            }

            if (other.name.ToLower().Contains("out"))
            {
                other.enabled = false;

                if (_lastTurn == Directions.left)
                {
                    TurnRight();
                }

                if (_lastTurn == Directions.right)
                {
                    TurnLeft();
                }
            }
        }
    }

    public void ToggleGyroscopeMovement(bool newValue)
    {
        useGyroscopeIfAvailable = newValue;
    }

    private void TurnRight()
    {
        _rootPlayerTransform.rotation = Quaternion.AngleAxis(_rootPlayerTransform.rotation.eulerAngles.y + 41.78f, _rootPlayerTransform.up);
    }

    private void TurnLeft()
    {
        _rootPlayerTransform.rotation = Quaternion.AngleAxis(_rootPlayerTransform.rotation.eulerAngles.y - 41.78f, _rootPlayerTransform.up);
    }

    private void Jump()
    {
        if (_isJumping) { return; }

        if (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable)
        {
            // Check gryo input here
            _isJumping = true;
            StartCoroutine(C_StopJumpTest());
        }
        else
        {
            // Check swip gesture here
            if (Input.touchSupported)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    _isJumping = true;
                    if (touch.phase == TouchPhase.Began)
                    {
                        fingerUp = touch.position;
                        fingerDown = touch.position;
                    }

                    //Detects Swipe while finger is still moving
                    if (touch.phase == TouchPhase.Moved)
                    {
                        if (!detectSwipeOnlyAfterRelease)
                        {
                            fingerDown = touch.position;
                            checkSwipe();
                        }
                    }

                    //Detects swipe after finger is released
                    if (touch.phase == TouchPhase.Ended)
                    {
                        fingerDown = touch.position;
                        checkSwipe();
                    }
                }
            } else
            {
                // check vertial axis input here
                if (Input.GetAxis("Vertical") > 0 || Input.GetButtonDown("Jump"))
                {
                    _isJumping = true;
                    print("Salta puta !!!");
                    StartCoroutine(C_StopJumpTest());
                }
            }
        }
    }

    private void checkSwipe()
    {
        //Check if Vertical swipe
        if (verticalMove() > .1f && verticalMove() > horizontalValMove())
        {
            //Debug.Log("Vertical");
            if (fingerDown.y - fingerUp.y > 0)//up swipe
            {
                OnSwipeUp();
            }
            else if (fingerDown.y - fingerUp.y < 0)//Down swipe
            {
                OnSwipeDown();
            }
            fingerUp = fingerDown;
        }
    }

    private float verticalMove()
    {
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    private IEnumerator C_StopJumpTest()
    {
        yield return new WaitForSeconds(2f);
        _isJumping = false;
    }

    private float horizontalValMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    //Called when a swipe up movement is detected
    private void OnSwipeUp()
    {
        Debug.Log("Swipe Up");
        StartCoroutine(C_StopJumpTest());
    }

    //Called when a swipe down movement is detected
    private void OnSwipeDown()
    {
        Debug.Log("Swipe Down");
        StartCoroutine(C_StopJumpTest());
    }

    // para matar al jugar crear un metodo
    // que sea fallOffTheMap()
    // ejecutarlo cuando llegue a un punto muerto y cuando se pase del threshold laterales
}

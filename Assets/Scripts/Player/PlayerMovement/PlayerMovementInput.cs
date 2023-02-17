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
    [SerializeField] private Animator playerAnimator;

    private Transform _playerTransform;
    private Transform _rootPlayerTransform;
    private float _screenWidth;
    private float _lateralVelocity = 0f;
    private float _smoothTime = .1f;
    private float _touchTimer;
    private float _touchInput;
    private float _gyroVelocity = 0f;
    private float _friction = .9f;
    private float _gyroThreshold = 1.55f;
    private float _maxLenghtLeftSubline = .6f;
    private float _maxLenghtCenterSubline = .3f;
    private float _maxLenghtRightSubline = .6f;
    private Directions _playerDirection;
    private Directions _lastTurn;
    private bool _isJumping = false;
    private float _jumpElapsedTime;
    private float _jumpDuration;
    private float _y0;
    private float _rootStartXPos;
    [SerializeField] private float _maxJumpHeight;
    private Vector2 fingerDown;
    private Vector2 fingerUp;
    public bool detectSwipeOnlyAfterRelease = false;


    [SerializeField] private float _radiusPercentToLoose;
    private float _rootPosition;

    public float RootPosition { get => _rootPosition; }

    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
        _rootPlayerTransform = _playerTransform.parent.GetComponent<Transform>();
        _screenWidth = Screen.width;
        float v = Mathf.Sqrt(2 * -Physics.gravity.y) * 2;
        _jumpDuration = (v / -Physics.gravity.y) * 2;
        _y0 = movementRadius;

        Input.gyro.enabled = SystemInfo.supportsGyroscope && useGyroscopeIfAvailable;

#if UNITY_EDITOR
        print($"Touch Support: {Input.touchSupported}");
        print($"Gyro Support: {SystemInfo.supportsGyroscope}");
#endif
    }

    private void FixedUpdate()
    {
        MoveForward();

        Jump();
        LateralMovement();

        UpdatePlayerPosition();
        UpdatePlayerRotation();

        _playerDirection = GetPlayerDirection();

        CheckLoose();
    }

    private void CheckLoose()
    {
        _rootPosition = _playerTransform.localPosition.x / (movementRadius * _radiusPercentToLoose);
        if (_rootPosition <= -1 || _rootPosition >= 1)
        {
            GameFlowManager.Instance.PlayerDied();
        }
    }

    private void UpdatePlayerPosition()
    {
        if (_isJumping) { return; }

        float velocitiy = (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable) ? _gyroVelocity : _lateralVelocity;
        float newXPos = Mathf.Clamp(Mathf.SmoothDamp(_playerTransform.localPosition.x, _playerTransform.localPosition.x + velocitiy * Time.deltaTime, ref _lateralVelocity, _smoothTime), -movementRadius, movementRadius);
        float newYPos = Mathf.Sqrt(Mathf.Pow(movementRadius, 2) - Mathf.Pow(newXPos, 2));

        _playerTransform.localPosition = new Vector3(newXPos, newYPos, _playerTransform.localPosition.z);
    }

    public void UpdatePlayerRotation()
    {
        if (_isJumping) { return; }

        Vector3 dir = _playerTransform.position - _rootPlayerTransform.position;
        _playerTransform.forward = _rootPlayerTransform.forward;
        Vector3 up = transform.TransformDirection(dir.normalized);
        _playerTransform.rotation = Quaternion.LookRotation(transform.forward, up);
    }

    private void LateralMovement()
    {
        // Make the player fall to right by default
        if (_isJumping) { return; }

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
                        if (_playerTransform.localPosition.x >= 0)
                        {
                            _lastTurn = Directions.right;
                            TurnRight();
                        }
                        else
                        {
                            _lastTurn = Directions.left;
                            TurnLeft();
                        }
                    }
                }
            }

            if (other.name.ToLower().Contains("out"))
            {
                other.enabled = false;

                if (_lastTurn == Directions.left)
                {
                    _rootPlayerTransform.localPosition = new Vector3(_rootStartXPos - 12, _rootPlayerTransform.localPosition.y, _rootPlayerTransform.localPosition.z);
                    TurnRight();
                }

                if (_lastTurn == Directions.right)
                {
                    _rootPlayerTransform.localPosition = new Vector3(_rootStartXPos + 12, _rootPlayerTransform.localPosition.y, _rootPlayerTransform.localPosition.z);
                    TurnLeft();
                }
            }
        }
    }

    public void ToggleGyroscopeMovement(bool newValue)
    {
        useGyroscopeIfAvailable = newValue;
        Input.gyro.enabled = SystemInfo.supportsGyroscope && useGyroscopeIfAvailable;
    }

    private void TurnRight()
    {
        _rootStartXPos = _rootPlayerTransform.localPosition.x;
        _rootPlayerTransform.rotation = Quaternion.AngleAxis(_rootPlayerTransform.rotation.eulerAngles.y + 41.78f, _rootPlayerTransform.up);
    }

    private void TurnLeft()
    {
        _rootStartXPos = _rootPlayerTransform.localPosition.x;
        _rootPlayerTransform.rotation = Quaternion.AngleAxis(_rootPlayerTransform.rotation.eulerAngles.y - 41.78f, _rootPlayerTransform.up);
    }

    private void Jump()
    {
        print($"Can jump: {!_isJumping}");
        // While is jumping move the caracter and ignore new inputs
        if (_isJumping)
        {
            if (_jumpElapsedTime < _jumpDuration)
            {
                // Calculate
                print("Calculating jump");
                //float t = Mathf.Clamp01(elapsedTime / _jumpDuration);
                _jumpElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(_jumpElapsedTime / _jumpDuration);
                float y = (4 * _maxJumpHeight * t * (1 - t)) + _y0;
                //print($"Jump progress {t}");

                switch (_playerDirection)
                {
                    case Directions.center:
                        break;
                    case Directions.left:
                        print("Should move root to the left");
                        _rootPlayerTransform.localPosition = new Vector3(_rootStartXPos - (12 * t), _rootPlayerTransform.localPosition.y, _rootPlayerTransform.localPosition.z);
                        break;
                    case Directions.right:
                        print("Should move root to the right"); 
                        _rootPlayerTransform.localPosition = new Vector3(_rootStartXPos + (12 * t), _rootPlayerTransform.localPosition.y, _rootPlayerTransform.localPosition.z);
                        break;
                }

                _playerTransform.localPosition = new Vector3(_playerTransform.localPosition.x, y, _playerTransform.localPosition.z);
            }

            // End of Jump
            if (_jumpElapsedTime >= _jumpDuration)
            {
                _isJumping = false;
                playerAnimator.SetBool("jumping", false);
                _jumpElapsedTime = 0;

                Collider[] hitColliders = Physics.OverlapSphere(_rootPlayerTransform.position, 0.01f);
                bool isRoot = false;
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.tag == "Root")
                    {
                        isRoot = true;
                        break;
                    }
                    
                }

                if (!isRoot) { GameFlowManager.Instance.PlayerDied(); }

                switch (_playerDirection)
                {
                    case Directions.center:
                        break;
                    case Directions.left:
                        _rootPlayerTransform.localPosition = new Vector3(_rootStartXPos - 12, _rootPlayerTransform.localPosition.y, _rootPlayerTransform.localPosition.z);
                        break;
                    case Directions.right:
                        _rootPlayerTransform.localPosition = new Vector3(_rootStartXPos + 12, _rootPlayerTransform.localPosition.y, _rootPlayerTransform.localPosition.z);
                        break;
                }
                _rootStartXPos = _rootPlayerTransform.localPosition.x;
            }
            return;
        }
        // If not, check for inputs.

        // Check gryo input here
        if (SystemInfo.supportsGyroscope && useGyroscopeIfAvailable)
        {
            _isJumping = true;
            // TODO: Gyro input here
            _rootStartXPos = _rootPlayerTransform.localPosition.x;
            playerAnimator.SetBool("jumping", true);
            return;
        }

        // Check swipe gesture here
        if (Input.touchSupported /* && _useTouch */)
        {
            if (Input.touchCount == 0) { return; }

            Touch touch = Input.GetTouch(0);
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
            return;
        }

        // check vertial axis and jump input here
        if (Input.GetAxis("Vertical") > 0 || Input.GetButtonDown("Jump"))
        {
            _isJumping = true;
            playerAnimator.SetBool("jumping", true);
            _rootStartXPos = _rootPlayerTransform.localPosition.x;

            return;
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

    private float horizontalValMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    //Called when a swipe up movement is detected
    private void OnSwipeUp()
    {
        Debug.Log("Swipe Up");
        _isJumping = true;
        playerAnimator.SetBool("jumping", true);
        _rootStartXPos = _rootPlayerTransform.localPosition.x;
    }

    //Called when a swipe down movement is detected
    private void OnSwipeDown()
    {
        Debug.Log("Swipe Down");
    }
}

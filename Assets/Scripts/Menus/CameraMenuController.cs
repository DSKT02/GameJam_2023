using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMenuController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private CinemachineVirtualCamera gameplayCamera;
    [SerializeField] private CinemachineVirtualCamera menuCamera;

    private void Awake()
    {
        gameplayCamera.gameObject.SetActive(false);  
        menuCamera.gameObject.SetActive(true);  
    }

    public void SwitchToGameCamera()
    {
        gameplayCamera.gameObject.SetActive(true);
        menuCamera.gameObject.SetActive(false);
    }
}

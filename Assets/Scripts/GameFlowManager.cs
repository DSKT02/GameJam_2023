using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;
    [SerializeField] private PlayerMovementInput player;
    [SerializeField] private CameraMenuController cameras;
    [SerializeField] private MenuController menu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player.enabled = false;
    }

    public void InitGame()
    {
        cameras.SwitchToGameCamera();
        player.enabled = true;
    }

    public void PlayerDied()
    {
        player.enabled = false;
        menu.OpenPanelEnd();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

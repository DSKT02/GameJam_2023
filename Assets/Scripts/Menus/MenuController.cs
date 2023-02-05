using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject initGamePanel;
    [SerializeField] private GameObject endGamePanel;

    [SerializeField] private Toggle gyroToggle;

    [SerializeField] private PlayerMovementInput player;

    [SerializeField] private TextMeshProUGUI highscoreInit;
    [SerializeField] private TextMeshProUGUI highscoreEnd;

    [SerializeField] private Button playButton;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        gyroToggle.gameObject.SetActive(SystemInfo.supportsGyroscope);
        gyroToggle.onValueChanged.AddListener(ToggleGyro);
        playButton.onClick.AddListener(Play);
        continueButton.onClick.AddListener(Continue);
        OpenPanelInit();
    }

    public void ToggleGyro(bool value)
    {
        player.ToggleGyroscopeMovement(value);
    }

    public void Play()
    {
        ClosePanels();
        GameFlowManager.Instance.InitGame();
    }

    public void Continue()
    {
        GameFlowManager.Instance.RestartGame();
    }

    public void OpenPanelInit()
    {
        UpdateHighscores();
        initGamePanel.SetActive(true);
        endGamePanel.SetActive(false);
    }

    public void OpenPanelEnd()
    {
        UpdateHighscores();
        initGamePanel.SetActive(false);
        endGamePanel.SetActive(true);
    }

    public void ClosePanels()
    {
        UpdateHighscores();
        initGamePanel.SetActive(false);
        endGamePanel.SetActive(false);
    }

    public void UpdateHighscores()
    {
        string text = $"Acorn HighScore: {CollectablesManager.AcornHighscore}";
        highscoreEnd.text = text;
        highscoreInit.text = text;
    }
}

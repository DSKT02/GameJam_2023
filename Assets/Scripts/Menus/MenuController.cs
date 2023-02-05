using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject initGamePanel;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private GameObject inGamePanel;

    [SerializeField] private Toggle gyroToggle;

    [SerializeField] private PlayerMovementInput player;

    [SerializeField] private TextMeshProUGUI highscoreInit;
    [SerializeField] private TextMeshProUGUI highscoreEnd;
    [SerializeField] private TextMeshProUGUI highscoreIn;

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
        OpenPanelIn();
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
        inGamePanel.SetActive(false);
    }

    public void OpenPanelEnd()
    {
        UpdateHighscores();
        initGamePanel.SetActive(false);
        endGamePanel.SetActive(true);
        inGamePanel.SetActive(false);
    }

    public void OpenPanelIn()
    {
        UpdateHighscores();
        initGamePanel.SetActive(false);
        endGamePanel.SetActive(false);
        inGamePanel.SetActive(true);
    }

    public void UpdateHighscores()
    {
        string textHS = $"Acorn HighScore: {CollectablesManager.AcornHighscore}";
        highscoreEnd.text = textHS;
        highscoreInit.text = textHS;

        string textS = $"Acorn Score: {CollectablesManager.Instance.CurrentAcorns}";
        highscoreIn.text = textS;
    }
}

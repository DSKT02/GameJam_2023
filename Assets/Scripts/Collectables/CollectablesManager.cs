using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablesManager : MonoBehaviour
{
    public static CollectablesManager Instance;

    public static int AcornHighscore { get => PlayerPrefs.GetInt(nameof(AcornHighscore), 0); set => PlayerPrefs.SetInt(nameof(AcornHighscore), value); }

    public static float TimeHighscore { get => PlayerPrefs.GetFloat(nameof(TimeHighscore), 0); set => PlayerPrefs.SetFloat(nameof(TimeHighscore), value); }

    private int currentAcorns;
    private float currentTime;

    public int CurrentAcorns { get => currentAcorns; set  { currentAcorns = value; if (currentAcorns > AcornHighscore) AcornHighscore = currentAcorns; } }

    public float CurrentTime { get => currentTime; set { currentTime = value; if (currentTime > TimeHighscore) TimeHighscore = currentTime; } }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

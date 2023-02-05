using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    private float currentTime;

    public void StartTracking()
    {
        StartCoroutine(TimeTrack());
    }

    public void StopTracking()
    {
        StopAllCoroutines();
        CollectablesManager.Instance.CurrentTime = currentTime;
    }

    private IEnumerator TimeTrack()
    {
        while (true)
        {
            currentTime += Time.deltaTime;
            CollectablesManager.Instance.CurrentTime = currentTime;
            yield return new WaitForEndOfFrame();
        }
    } 
}

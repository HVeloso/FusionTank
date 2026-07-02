using Fusion;
using System;
using System.Collections;
using UnityEngine;

public class WaitingRoomTimerManager : NetworkBehaviour
{
    [Tooltip("Time in seconds")]
    [SerializeField, Min(0)] private int _timeToStart;
    [SerializeField] private WaitingRoomTimerView _timerView;

    [Networked, OnChangedRender(nameof(UpdateTimer))]
    private int CurrentTime { get; set; }

    private void UpdateTimer()
    {
        //if (CurrentTime )

        _timerView.SetTimerText(CurrentTime);
    }

    public void StartTimer()
    {

    }

    public void StopTimer()
    {

    }

    private IEnumerator Timer()
    {
        while(CurrentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            CurrentTime--;
        }
    }
}

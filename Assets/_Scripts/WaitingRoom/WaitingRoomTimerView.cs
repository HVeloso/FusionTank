using TMPro;
using UnityEngine;

public class WaitingRoomTimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;

    public void SetTimerText(int timeInSeconds)
    {
        if (timeInSeconds < 0)
        {
            DisableTimer();
            return;
        }

        int min = timeInSeconds / 60;
        int sec = timeInSeconds % 60;

        _timerText.text = $"{min}/{sec}s";
    }

    public void DisableTimer()
    {
        _timerText.text = "--/--s";
    }
}

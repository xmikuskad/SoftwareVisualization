using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;

    public bool IsPaused()
    {
        return isPaused;
    }

    public void SetPaused(bool paused)
    {
        this.isPaused = paused;
        if (isPaused)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void SetPausedWithTime(bool paused)
    {
        SetPaused(paused);
        Time.timeScale = paused ? 0 : 1;
    }
}

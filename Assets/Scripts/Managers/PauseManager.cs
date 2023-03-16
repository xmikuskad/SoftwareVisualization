using System.Collections;
using System.Collections.Generic;
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
    }
}

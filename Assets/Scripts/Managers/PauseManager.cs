using DG.Tweening;
using UnityEngine;

public class PauseManager : MonoBehaviour
{

    private bool isInteractionPaused = false;
    private bool isAnimationPaused = false;

    public bool IsAnimationPaused()
    {
        return isAnimationPaused;
    }
    
    public bool IsInteractionPaused()
    {
        return isInteractionPaused;
    }

    public bool IsSomethingPaused()
    {
        return isInteractionPaused || isAnimationPaused;
    }

    public void SetAnimationPaused(bool paused)
    {
        this.isAnimationPaused = paused;
        if (isAnimationPaused)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }
    
    public void SetInteractionPaused(bool paused)
    {
        this.isAnimationPaused = paused;
        if (isAnimationPaused)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void SetEverythingPaused(bool paused)
    {
        SetAnimationPaused(paused);
        SetInteractionPaused(paused);
    }

}

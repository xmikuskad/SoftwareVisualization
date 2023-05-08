using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [Header("References")] public DataRenderer dataRenderer;
    
    public Image pauseImg;
    
    public Sprite pauseIcon;
    public Sprite playIcon;

    public void PlayOrPause()
    {
        SingletonManager.Instance.pauseManager.SetAnimationPaused(!SingletonManager.Instance.pauseManager.IsAnimationPaused());
        pauseImg.sprite = SingletonManager.Instance.pauseManager.IsAnimationPaused() ? playIcon : pauseIcon;
    }

    public void RestartAnimation()
    {
        // dataRenderer.RerenderProject(1L, true);
        SingletonManager.Instance.pauseManager.SetAnimationPaused(false);
        pauseImg.sprite = pauseIcon;
    }
}

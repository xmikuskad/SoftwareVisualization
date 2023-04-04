using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private float speed = 0.8f;
    
    [Header("Animation settings")] 
    public float minSpawnAnimTime = 0.0001f;
    public float maxSpawnAnimTime = 1f;
    public float minMoveAnimTime = 0.0001f;
    public float maxMoveAnimTime = 1f;
    public float minColorChangeAnimTime = 0.0001f;
    public float maxColorChangeAnimTime = 1f;
    public float minLineDisappearAnimTime = 0.0001f;
    public float maxLineDisappearAnimTime = 0.2f;
    
    public float minWaitAnimTime = 0.0001f;
    public float maxWaitAnimTime = 0.3f;
    
    public void SetSpeed(float speed)
    {
        this.speed = speed/100f;
    }

    public float GetSpawnAnimTime()
    {
        return minSpawnAnimTime + (1f-speed)*maxSpawnAnimTime;
    }
    
    public float GetMoveAnimTime()
    {
        return minMoveAnimTime + (1f-speed)*maxMoveAnimTime;
    }
    
    public float GetColorChangeAnimTime()
    {
        return minColorChangeAnimTime + (1f-speed)*maxColorChangeAnimTime;
    }
    
    public float GetLineDisappearTime()
    {
        return minLineDisappearAnimTime + (1f-speed)*maxLineDisappearAnimTime;
    }
    
    public float GetWaitTime()
    {
        return minWaitAnimTime + (1f-speed)*maxWaitAnimTime;
    }
}

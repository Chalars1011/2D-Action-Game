using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public float Intensity;
    public float duration;
    public int frequen;
   public void shake() 
    {
        EffectPoolManager.Instance.ShakeScreen(Intensity, duration, frequen);
    }
}

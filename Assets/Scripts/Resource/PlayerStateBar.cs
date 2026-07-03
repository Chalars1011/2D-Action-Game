using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateBar : MonoBehaviour
{
    public Image Red;
    public Image Red_Slow;
    public Image Blued;
    public Image Blued_Slow;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Red_Slow.fillAmount > Red.fillAmount) 
        {
            Red_Slow.fillAmount -= (float)(Time.deltaTime * 0.3);
          //  Debug.Log("1111");
        }
        if (Red_Slow.fillAmount < Red.fillAmount)
        {
            Red_Slow.fillAmount += (float)(Time.deltaTime * 0.3);
            //  Debug.Log("1111");
        }
        if (Blued_Slow.fillAmount > Blued.fillAmount)
        {
            Red_Slow.fillAmount -= (float)(Time.deltaTime * 0.3);
            //  Debug.Log("1111");
        }
        if (Blued_Slow.fillAmount < Blued.fillAmount)
        {
            Red_Slow.fillAmount += (float)(Time.deltaTime * 0.3);
            //  Debug.Log("1111");
        }
    }

    public void OnHealthChange(float persent) 
    {
        Red.fillAmount = persent;
    }

    public void OnManaChange(float persent)
    {
        Blued.fillAmount = persent;
    }
}

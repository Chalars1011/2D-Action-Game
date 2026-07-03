using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveFalse : MonoBehaviour
{
    public float liveTime;
    void Start()
    {
       
    }
    private void OnEnable()
    {
        Invoke("SetActive", liveTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActive()
    {
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolPushObject : MonoBehaviour
{
    // Start is called before the first frame update
    public float pushTime;
    void Start()
    {
      //  Debug.Log($"Invoke PushObject for {gameObject.name} in {pushTime} seconds");
       
    }
    private void OnEnable()
    {
        Invoke("PushObject", pushTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PushObject() 
    {
        
        PoolManager.Instance.PushObj(gameObject.name, gameObject);
        
        // Debug.Log($"Pushing {gameObject.name} back to the pool");
       
    }
}

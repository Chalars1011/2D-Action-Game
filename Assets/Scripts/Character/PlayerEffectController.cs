using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    public GameObject attack_1_Effect;
    public GameObject attack_2_Effect;
    public GameObject attack_3_Effect;
   // public GameObject chongci_start_Effect;
    void Start()
    {
       // Attack1_Position = GameObject.Find("Player").transform.GetChild(1).transform.GetChild(0).transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack_1_Effect() 
    {
        attack_1_Effect.SetActive(true);
    }
    public void Attack_2_Effect()
    {
        attack_2_Effect.SetActive(true);
    }
    public void Attack_3_Effect()
    {
        attack_3_Effect.SetActive(true);
    }
    public void Chongci_start_Effect() 
    {
        EffectPoolManager.Instance.ChongCi_Start_Effect(transform.GetChild(0).transform,transform.GetComponent<PlayerController>().faceDir);
    }
    public void Chongci_stop_Effect()
    {
        EffectPoolManager.Instance.ChongCi_Stop_Effect(transform.GetChild(2).transform, transform.GetComponent<PlayerController>().faceDir);
    }
    public void RunningEffect()
    {
        EffectPoolManager.Instance.RunningEffect(transform.GetChild(1).transform, transform.GetComponent<PlayerController>().faceDir);
    }
    public void RunStopEffect()
    {
        EffectPoolManager.Instance.RunStopEffect(transform.GetChild(2).transform, transform.GetComponent<PlayerController>().faceDir);
    }
}

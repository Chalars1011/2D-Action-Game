using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DiuDiuGuai_Controller : EnemyBase
{
    [Header("丢丢怪参数")]
    public Transform throwPoint;        // 石头生成位置
    public GameObject ShiTou;

    protected override void Awake()
    {
        base.Awake();
       

    }

    // 重写受伤方法
    public override void OnTakeDamage(Transform attackTrans)
    {
        base.OnTakeDamage(attackTrans);

        // 触发受伤动画
        anim.SetTrigger("hurt");

        // 停止移动
        currentSpeed = 0;
    }

    public void CreatShiTou()
    {
       
        
        if (attckTarget == null || throwPoint == null)
            return;

        // 从对象池获取石头
        GameObject stoneObj = Instantiate(ShiTou);

        if (stoneObj == null)
            return;
      
        // 初始化石头
        stoneObj.transform.position = throwPoint.position;

        // 获取石头脚本并设置方向
        
        Stone stone = stoneObj.GetComponent<Stone>();
        if (stone != null)
        {
            // 计算目标方向（从投掷点到玩家）

            Vector2 targetDirection = attckTarget.position - throwPoint.position+new Vector3(0,1.5f,0);
            stone.Initialize(targetDirection);
        }
    }

    
}

using GameArchitecture.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Boss_KanMenRen : BossBase
{
    [Header("灯光控制")]
    public UnityEngine.Rendering.Universal.Light2D bossEffectLight; // 拖Boss身上的灯光进来

    // 动画事件调用——开灯
    public void LightOn()
    {
        if (bossEffectLight != null)
            bossEffectLight.enabled = true;
    }

    // 动画事件调用——关灯
    public void LightOff()
    {
        if (bossEffectLight != null)
            bossEffectLight.enabled = false;
    }
    [Header("��̲���")]
    public float dashForce = 15f;
    [HideInInspector]
    public float dashDirection = 1f;
    public bool isDashing = false;
    private int playerLayerIndex = 6;
    private Vector2 lastDirection = Vector2.right;

    [Header("�׵���Ч����")]
    public float lightningXRange = 3f;  // X�������Χ
    public float lightningYOffset = 2f; // Y��ƫ����
    public string playerTag = "Player"; // ���Tag
    private Transform playerTransform;  // ���Transform����
    public Transform throwPoint;  
    public GameObject FlyLight;

    protected override void Awake()
    {
        base.Awake();
        // �������Transform
        playerTransform = Blackboard.PlayerTransform;
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag(playerTag)?.transform;
    }

    // ��̿�ʼ
    public void StartDash()
    {
        if (isDashing) return;

        isDashing = true;
        rb.velocity = Vector2.zero;

        float facingMultiplier = transform.localScale.x;
        Vector2 dashVector = new Vector2(
            facingMultiplier * dashDirection * dashForce,
            0f
        );
        Physics2D.IgnoreLayerCollision(gameObject.layer, playerLayerIndex, true);
        rb.AddForce(dashVector, ForceMode2D.Impulse);
    }

    // ��̽���
    public void StopDash()
    {
        if (!isDashing) return;

        isDashing = false;
        rb.velocity = Vector2.zero;
        Physics2D.IgnoreLayerCollision(gameObject.layer, playerLayerIndex, false); // ������Ӧ����false
    }

    // ���������׵�
    public void GenerateMidleLightning()
    {
        if (playerTransform == null)
        {
            Debug.LogError("δ�ҵ���Ҷ���");
            return;
        }

        // �������λ�ã�X������Ҹ��������Y��̶�ƫ�ƣ�
        float randomX = playerTransform.position.x + Random.Range(-lightningXRange, lightningXRange);
        Vector3 spawnPos = new Vector3(randomX, playerTransform.position.y + lightningYOffset, 0);

        // ȷ������������ң�
        int faceDir = (playerTransform.position.x > randomX) ? 1 : -1;

        // ������Ч
        EffectPoolManager.Instance.Light_midleLight(spawnPos, faceDir);
    }

    // ���ɴ����׵�
    public void GenerateLongLightning()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y+ lightningYOffset, 0);
        // ������Ч
        EffectPoolManager.Instance.Light_long(spawnPos, 1);
    }
    public void CreatFlyLight() 
    {
        int faceDir;
        if (transform.localScale.x > 0) 
        {
            faceDir = 1;
        }
        else 
        {
            faceDir = -1;
        }

        if (playerTransform == null || throwPoint == null)
            return;

       
        GameObject Light = Instantiate(FlyLight);

        if (Light == null)
            return;
        Light.transform.position = throwPoint.position;

        FlyLightController LightContro = Light.GetComponent<FlyLightController>();
        if (LightContro != null)
        {
          
            LightContro.Initialize(faceDir);
        }

    }

    // ��Ļ�ζ�����
    public void ShakeScreen1()
    {
        EffectPoolManager.Instance.ShakeScreen(1.2f, 0.1f, 15);
    }

    public void ShakeScreen2()
    {
        EffectPoolManager.Instance.ShakeScreen(1.2f, 0.2f, 15);
    }

    public void ShakeScreen3()
    {
        EffectPoolManager.Instance.ShakeScreen(1.6f, 1f, 30);
    }
}
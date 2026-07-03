using GameArchitecture.Core;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;

/// <summary>
/// BOSS��۹������� - ������������ú�״̬��������������Ϊ�߼�
/// </summary>
public abstract class BossBase : MonoBehaviour
{
    // ����ʵ��
    public static BossBase Instance { get; private set; }

    [Header("�������")]
    public BehaviorTree behaviorTree;
    public Animator animator;
    public Rigidbody2D rb;
    public Character character; // ���Character���

    [Header("��������")]
    public float moveSpeed = 3f;
    public float attackRange = 2f;

    [Header("=== v2: SO Config ===")]
    [SerializeField] private CharacterStats_SO statsConfig;

    // ״̬����
    public Transform Target { get; protected set; }
    [Header("死亡效果")]
    public float deathFreezeDuration = 0.15f;
    public float deathShakeIntensity = 2f;
    public float deathShakeDuration = 0.3f;

    public bool IsAlive;
    public bool IsAttacking { get; protected set; }
    public int CurrentPhase { get; protected set; } = 1;

    #region ��������
    protected virtual void Awake()
    {
        // ȷ��ֻ��һ��ʵ��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeComponents();
        // ֱ��������ң��򻯰棬������С������
        Target = Blackboard.PlayerTransform;
        if (Target == null)
            Target = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (Target == null) Debug.LogError("δ�ҵ���Ҷ�����ȷ����Ҵ���'Player'��ǩ��");
    }

    protected virtual void InitializeComponents()
    {
        if (behaviorTree == null) behaviorTree = GetComponent<BehaviorTree>();
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (character == null) character = GetComponent<Character>();
    }
    #endregion


    private bool deathTriggered;

    private void Update()
    {
        if(character.currentHealth <= 0)
        {
            if (!deathTriggered)
            {
                deathTriggered = true;
                EffectPoolManager.Instance.FreezeTime(deathFreezeDuration, 0f);
                EffectPoolManager.Instance.ShakeScreen(deathShakeIntensity, deathShakeDuration, 30f);
                GameEndController endCtrl = FindObjectOfType<GameEndController>();
                if (endCtrl != null) endCtrl.TriggerEnd();
            }
            IsAlive = false;
            animator.SetBool("Death", true);

            // ���ø��壨��ֹ����ģ�⣩
            if (rb != null)
            {
                rb.simulated = false; // ��������ģ��
                rb.velocity = Vector2.zero; // ����ٶ�
            }

            // ����������ײ�壨��ֹ��ײ������
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // ��ѡ��������Ϊ����ֹͣAI�߼���
            if (behaviorTree != null)
            {
                behaviorTree.enabled = false;
            }
        }
        else 
        {
            IsAlive = true;
        }
    }

    public void Deastroy() 
    {
        Destroy(gameObject);
    }

    public void TreeOpen() 
    {
        behaviorTree.enabled = true;
    }
    public void TreeClose()
    {
        behaviorTree.enabled = false;
    }



    #region ״̬����
    /// <summary>
    /// ����ָ���׶Σ������ڶ�׶�BOSSս��
    /// </summary>
    public virtual void EnterPhase(int phase)
    {
        CurrentPhase = phase;
        if (behaviorTree != null)
            behaviorTree.SetVariableValue("CurrentPhase", phase);
        OnPhaseChanged?.Invoke(phase);
    }

    /// <summary>
    /// ��ʼ���������ƹ���״̬��֪ͨ��Ϊ����

    public virtual void StartAttack()
    {
        // 玩家倒地时不攻击
        if (Target != null)
        {
            PlayerController pc = Target.GetComponent<PlayerController>();
            if (pc != null && pc.isHurt) return;
        }
        IsAttacking = true;
        if (behaviorTree != null)
            behaviorTree.SetVariableValue("IsAttacking", true);
        animator.SetBool("IsAttacking", true);
        OnAttackStarted?.Invoke();
    }

    /// <summary>
    /// �������������ù���״̬��֪ͨ��Ϊ����
    /// </summary>
    public virtual void EndAttack()
    {
        IsAttacking = false;
        if (behaviorTree != null)
            behaviorTree.SetVariableValue("IsAttacking", false);
        animator.SetBool("IsAttacking", false);
        OnAttackEnded?.Invoke();
    }
    #endregion

    #region �¼�ϵͳ
    /// <summary>
    /// �׶α仯�¼���������UI���»����л���
    /// </summary>
    public delegate void PhaseChanged(int phase);
    public event PhaseChanged OnPhaseChanged;

    /// <summary>
    /// �����¼��������ڲ�����Ч����Ч��
    /// </summary>
    public delegate void AttackEvent();
    public event AttackEvent OnAttackStarted;
    public event AttackEvent OnAttackEnded;
    #endregion
}
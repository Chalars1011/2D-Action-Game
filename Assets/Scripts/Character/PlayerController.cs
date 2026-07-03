using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices.ComTypes;
using GameArchitecture.Core;
using GameArchitecture.Actor;
using GameArchitecture.Combat;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public  PlayerInputControl inputControl;
    CapsuleCollider2D playerCollider;
    Character character; // 角色组件引用
    Animator playerAnimator;
    Vector2 inputDirection;
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public int faceDir = 1;

    [Header("道具实例")]
    public GameObject bloodBladePrefab;
    public Transform bloodBladeSpawnPoint; // 血刃生成位置（空物体）


    [Header("召唤物设置")]
    public GameObject summonPrefab; // 召唤物预制体
    public Transform summonSpawnPoint; // 召唤物生成位置（空物体）


    [Header("无敌设置")]
    public bool isInvincible = false;
    public float invincibleDuration = 5f;
    public Color invincibleColor = new Color(1f, 0.8f, 0.2f, 1f);
    public SpriteRenderer playerSpriteRenderer;
    public UnityEngine.Rendering.Universal.Light2D invincibleLight; // 无敌时亮起的灯光
    private Color originalColor; // 原始颜色

    [Header("机关交互")]
    public Mechanism currentMechanism; // 当前靠近的机关

    [Header("受伤反弹")]
    public bool isHurt; //是否受伤
    public bool isDie;
    public float hurtForce; //受伤力度
    public float hurtDuration; //受伤时长


    [Header("攻击")]
    public bool isAttack;
    [HideInInspector]
    public bool canAttack;
    [HideInInspector]
    public bool isSkillActive; // 技能释放中，阻止跳跃和受伤动画

    [Header("技能蓝量消耗")]
    public float skill1ManaCost = 20f; // 技能1蓝量消耗
    public float skill2ManaCost = 30f; // 技能2蓝量消耗
    public float skill3ManaCost = 50f; // 技能3蓝量消耗



    [Header("闪避设置")]
    public float dodgeShortDistance = 3f;    // 短按闪避距离
    public float dodgeLongDistance = 6f;     // 长按闪避距离
    public float dodgeChargeThreshold = 0.08f;// 长按判定秒数
    public float dodgeDashDuration = 0.1f;   // 闪避冲刺耗时
    public float dodgeCooldown = 1.5f;       // 闪避冷却时间
    public float skill2DashDistance = 5f;    // 技能2冲刺距离
    public float skill2DashDuration = 0.12f; // 技能2冲刺耗时
    [SerializeField]
    public bool isDodging;
    private bool isDodgeCharging;
    private float dodgeChargeTimer;
    public bool canShanBi;
    private float cooldownTimer = 0f;
    private float jumpGraceTimer;
    // _coyoteTimer defined in v2 fields above
    //玩家闪避层级设置
    private int enemyLayerIndex = 7; // 敌人层索引
    public int dodgeLayerIndex = 9; // DodgeLayer 的索引
    private int originalLayer; // 存储原始层


    [Header("钉墙设置")]
    public bool isNailedToWall; // 是否钉在墙上
    public bool canNailToWall; // 是否可以钉墙
    public float nailSlideSpeed = 1f; // 钉墙下滑速度
    public float nailJumpForce = 8f; // 钉墙跳跃力度
    public Transform nailCheckTransform; // 钉墙检测点（带有碰撞体）
    public string nailWallTag = "NailWall"; // 可钉墙壁标签


    [Header("物理材质")]
    public PhysicsMaterial2D jumpPhysic;
    public PhysicsMaterial2D normalPhysic;


    [Header("移动参数")]
    public float Maxspeed;
    private float CurrentSpeed;
    [HideInInspector]
    public bool canRun;


    [Header("下蹲")]
    private Vector2 startOffset;
    private Vector2 startSize;
    public float startOffsetIntensity;
    public float startSizeIntensity;
    // private bool canWalk;
    private bool isXiaDun;




    [Header("跳跃检查")]
    public float jumpForce;
    private float defaultGravityScale; // 存储默认重力缩放

    private bool IsGround;
    public float CheckRadious;
    public Transform checkTransfrom;
    public LayerMask groundLayer;
    private int CurrentJumpCount;
    public int MaxJumpCount = 2;

    [Header("楼梯设置")]
    public bool isClimbing; // 是否在爬楼梯
    public float climbSpeed = 3f; // 爬楼梯速度
    public LayerMask stairsLayer; // 楼梯层
    public float stairsCheckRadius = 0.5f; // 楼梯检测半径
    public Transform stairsCheckTransform; // 楼梯检测点
    public LayerMask stairsEndLayer; // 楼梯端点层
    public Collider2D stairsEndCheckTop; // 上检测点（挂一个BoxCollider2D在玩家头顶）
    public Collider2D stairsEndCheckBottom; // 下检测点（挂一个BoxCollider2D在玩家脚底）

    [Header("边缘攀爬设置")]
    public bool isClimbingEdge; // 是否在攀爬边缘
    public bool canClimb; // 是否可以攀爬
    public float edgeClimbSpeed = 3f; // 边缘攀爬速度
    public Transform edgeCheckTransform; // 边缘检测点（带有碰撞体）
    public string edgeTag = "PanPaDian"; // 攀爬点标签
    private Transform currentClimbTarget; // 当前攀爬目标

    [Header("SO Config")]
    [SerializeField] private PlayerConfig_SO config;
    [SerializeField] private CharacterStats_SO statsConfig;

    // Architecture v2
    private ActionDispatcher _actionDispatcher = new ActionDispatcher();
    private InputBuffer _inputBuffer;
    private float _coyoteTimer;
    private InvincibleEffect _activeInvincibleEffect;

    private bool CanAct(ActionPriority priority)
    {
        if (isDie) return false;
        if (isHurt) return false;
        switch (priority)
        {
            case ActionPriority.Attack:
                if (isSkillActive || isDodging) return false; break;
            case ActionPriority.Skill:
                if (isAttack || isSkillActive || isDodging) return false; break;
            case ActionPriority.Dodge:
                if (isAttack || isSkillActive) return false; break;
            default:
                if (isAttack || isSkillActive || isDodging) return false; break;
        }
        return true;
    }

    private void ClearDispatcherState() { _actionDispatcher.EndCurrentAction(); }

    private void ApplyConfig()
    {
        if (config == null) return;
        Maxspeed = config.maxSpeed;
        jumpForce = config.jumpForce;
        MaxJumpCount = config.maxJumpCount;
        dodgeShortDistance = config.dodgeShortDistance;
        dodgeLongDistance = config.dodgeLongDistance;
        dodgeChargeThreshold = config.dodgeChargeThreshold;
        dodgeDashDuration = config.dodgeDashDuration;
        dodgeCooldown = config.dodgeCooldown;
        skill1ManaCost = config.skill1ManaCost;
        skill2ManaCost = config.skill2ManaCost;
        skill3ManaCost = config.skill3ManaCost;
        skill2DashDistance = config.skill2DashDistance;
        skill2DashDuration = config.skill2DashDuration;
        hurtForce = config.hurtForce;
        invincibleDuration = config.invincibleDuration;
        invincibleColor = config.invincibleColor;
        nailSlideSpeed = config.nailSlideSpeed;
        nailJumpForce = config.nailJumpForce;
        climbSpeed = config.climbSpeed;
        edgeClimbSpeed = config.edgeClimbSpeed;
        CheckRadious = config.groundCheckRadius;
        if (statsConfig != null && character != null)
        {
            character.maxHealth = statsConfig.maxHealth;
            character.maxMana = statsConfig.maxMana;
            character.manaRegenRate = statsConfig.manaRegenRate;
            character.WuDiTime = statsConfig.invincibleAfterHitTime;
        }
    }

    private void Awake()
    {
        _inputBuffer = new InputBuffer(0.15f);
        inputControl = new PlayerInputControl();
        inputControl.GamePlay.Jump.started += ctx => { _inputBuffer.RecordAction(GameAction.Jump); TryBufferedJump(); };
        inputControl.GamePlay.ShanBi.started += ctx => ShanBiAnimation();
        inputControl.GamePlay.Attack.started += ctx => PlayerAtatck();
        inputControl.GamePlay.Skill_1.started += ctx => PlayerSkill_1();
        inputControl.GamePlay.Skill_2.started += ctx => PlayerSkill_2();
        inputControl.GamePlay.Skill_3.started += ctx => PlayerSkill_3();
        inputControl.GamePlay.HuiXue.started += ctx => PlayerHuiXue();
        inputControl.GamePlay.Test_AddBloodBottle.started += IBindCtx => AddTestBottles();
        inputControl.GamePlay.Test_AddBloodBottle.started += IBindCtx => AddTestCurrency();
        inputControl.GamePlay.JiaoHu.started += ctx => Interact();
    }



    private void OnEnable()
    {
        inputControl.Enable();
    }

    private void OnDisable()
    {
        inputControl.Disable();
    }

    void Start()
    {
        character = GetComponent<Character>();
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerAnimator = GetComponent<Animator>();
        Blackboard.SetPlayer(transform);
        ApplyConfig();
        CurrentJumpCount = MaxJumpCount;

        // canWalk = false;
        playerCollider = GetComponent<CapsuleCollider2D>();
        startOffset = playerCollider.offset;
        startSize = playerCollider.size;
        rb.simulated = true;
        canRun = true;
        canAttack = true;
        canShanBi = true;
        isDodging = false;
        originalLayer = gameObject.layer;
        // 获取默认重力缩放值
        defaultGravityScale = rb.gravityScale;


        // 如果没有手动指定SpriteRenderer，自动查找
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Debug.Log("自动找到SpriteRenderer: " + (playerSpriteRenderer != null ? playerSpriteRenderer.gameObject.name : "未找到"));
        }

        // 保存原始颜色
        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
            Debug.Log("保存原始颜色: " + originalColor);
        }
        else
        {
            Debug.LogWarning("SpriteRenderer为空！");
        }
        // 检查是否从传送门重生
        if (PlayerPrefs.HasKey("SpawnPointName"))
        {
            Respawn();
        }


    }

    void Update()
    {
        inputDirection = inputControl.GamePlay.Move.ReadValue<Vector2>();
        _inputBuffer.Tick(Time.deltaTime);

        //闪避蓄力计时（按住闪避键持续计时）
        if (isDodgeCharging && inputControl.GamePlay.ShanBi.IsPressed())
        {
            dodgeChargeTimer += Time.deltaTime;
        }

        //闪避的冷却
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        // 起跳宽限期计时器
        if (jumpGraceTimer > 0)
        {
            jumpGraceTimer -= Time.deltaTime;
        }
        // 爬梯入口保护期
        if (climbEntryTimer > 0)
        {
            climbEntryTimer -= Time.deltaTime;
        }
        // 爬梯退出冷却
        if (climbExitCooldown > 0)
            climbExitCooldown -= Time.deltaTime;
        // 边缘攀爬冷却
        if (edgeClimbCooldown > 0)
            edgeClimbCooldown -= Time.deltaTime;
        // 处理钉墙状态
        HandleNailWallState();

        // 处理攀爬输入
        HandleClimbInput();

        // 楼梯检测和爬楼梯逻辑
        CheckStairs();
        HandleClimbing();

        // 处理道具使用
        HandleItemUsage();

        if (IsGround && !isAttack && !isSkillActive && !isDodging && !isHurt)
            TryBufferedJump();
    }

    void FixedUpdate()
    {
        PlayerAnimationBoolSet();

        bool canMove = !isAttack && !isSkillActive && !isDodging && !isHurt;
        if (canMove && !canRun && !isNailedToWall && !isClimbing && !isClimbingEdge) canRun = true;
        if (canMove && canRun && !isClimbing && !isClimbingEdge && !isNailedToWall) Move();


        checkIsGround();
        CheckState();

        if (isDodging && !isSkillActive)
        {
            gameObject.layer = dodgeLayerIndex;
            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, true);
        }
        else if (!isSkillActive && !isHurt)
        {
            gameObject.layer = originalLayer;
            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, false);
        }
    }

    void LateUpdate()
    {
        if (isInvincible && playerSpriteRenderer != null)
            playerSpriteRenderer.color = invincibleColor;
        if (!isDie) SyncFlagsWithAnimator();
    }

    void SyncFlagsWithAnimator()
    {
        if (playerAnimator == null) return;
        bool inAtk = playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Attack1")
                  || playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Attack2")
                  || playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Attack3");
        bool inDdg = playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_ShanBi");
        bool inSkl = playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Skill_1")
                  || playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Skill_2")
                  || playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Skill_3");
        if (isAttack && !inAtk) { isAttack = false; canRun = true; }
        if (isDodging && !inDdg) { isDodging = false; canRun = true; }
        if (isSkillActive && !inSkl) { isSkillActive = false; }
        if (!inAtk && !inDdg && !inSkl && !isHurt && !isDie && !isNailedToWall) canRun = true;
        if (IsGround && !isAttack && !isDodging && !isSkillActive && !isHurt) TryBufferedJump();
    }



    // 检测是否可以钉墙
    private bool CheckCanNailToWall()
    {
        // 使用OverlapCircle检测范围内是否有可钉墙壁
        Collider2D[] colliders = Physics2D.OverlapCircleAll(nailCheckTransform.position, 0.5f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(nailWallTag))
            {
                return true;
            }
        }

        return false;
    }

    // 进入钉墙状态
    private void EnterNailWallState()
    {
        isNailedToWall = true;
        canRun = false;
        rb.gravityScale = 0f; // 取消重力
        rb.velocity = Vector2.zero; // 停止移动

        // 触发钉墙动画
        playerAnimator.SetBool("IsNailedToWall", true);
        playerAnimator.SetTrigger("NailToWall");
    }

    // 处理钉墙状态
    private void HandleNailWallState()
    {
        if (isNailedToWall)
        {
            // 缓慢下滑
            rb.velocity = new Vector2(0, -nailSlideSpeed * Time.deltaTime);

            // 检测是否还在墙上
            bool isStillOnWall = false;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(nailCheckTransform.position, 0.5f);

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(nailWallTag))
                {
                    isStillOnWall = true;
                    break;
                }
            }

            if (!isStillOnWall)
            {
                ExitNailWallState();
            }
        }
    }

    // 退出钉墙状态
    private void ExitNailWallState()
    {
        isNailedToWall = false;
        canRun = true;
        rb.gravityScale = defaultGravityScale; // 恢复重力
        playerAnimator.SetBool("IsNailedToWall", false);

    }

    // 钉墙跳跃
    private void NailWallJump()
    {
        if (isNailedToWall)
        {
            ExitNailWallState();

            // 获取水平输入方向
            float horizontalInput = inputDirection.x;

            // 如果没有水平输入，默认向反方向跳
            if (Mathf.Abs(horizontalInput) < 0.1f)
            {
                horizontalInput = -faceDir;
            }

            // 应用跳跃力
            rb.velocity = new Vector2(horizontalInput * Maxspeed * Time.deltaTime, 0);
            rb.AddForce(Vector3.up * nailJumpForce, ForceMode2D.Impulse);

            // 更新朝向
            if (horizontalInput > 0)
            {
                faceDir = 1;
            }
            else if (horizontalInput < 0)
            {
                faceDir = -1;
            }
            transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);

            // 触发跳跃动画
            jumpGraceTimer = 0.15f;
            playerAnimator.SetBool("JumpNow", true);
            playerAnimator.SetFloat("JumpFloat", 1f);
            playerAnimator.SetTrigger("Jump");
        }
    }

    // 检测是否靠近楼梯
    private bool isClimbEnding;
    private int lastClimbDir;
    private float climbEntryTimer;
    private float climbExitCooldown; // 退出爬梯后的冷却，防止立刻再进

    private Coroutine centerCoroutine;

    private void CheckStairs()
    {
        Collider2D stairCol = Physics2D.OverlapCircle(stairsCheckTransform.position, stairsCheckRadius, stairsLayer);

        if (stairCol != null && (inputDirection.y > 0.4f || inputDirection.y < -0.4f) && !isClimbing && climbExitCooldown <= 0)
        {
            // 方向合法性：底部不能下、顶部不能上
            bool atBottom = stairsEndCheckBottom != null && stairsEndCheckBottom.IsTouchingLayers(stairsEndLayer);
            bool atTop = stairsEndCheckTop != null && stairsEndCheckTop.IsTouchingLayers(stairsEndLayer);
            bool pressingUp = inputDirection.y > 0.4f;
            bool pressingDown = inputDirection.y < -0.4f;

            if ((atBottom && pressingDown) || (atTop && pressingUp))
                return;

            isClimbing = true;
            isClimbEnding = false;
            climbEntryTimer = 0.3f;
            canRun = false;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            Physics2D.IgnoreLayerCollision(gameObject.layer, GroundLayerIndex(), true);

            float targetX = stairCol.bounds.center.x;
            if (centerCoroutine != null) StopCoroutine(centerCoroutine);
            centerCoroutine = StartCoroutine(SmoothCenterX(targetX));

            int climbDir = inputDirection.y > 0 ? 1 : -1;
            playerAnimator.SetInteger("ClimbDirection", climbDir);
            playerAnimator.SetBool("JumpNow", false);
            playerAnimator.SetBool("IsClimbing", true);
            playerAnimator.SetFloat("ClimbSpeed", climbDir);
            playerAnimator.SetTrigger("ClimbEnter");
        }
    }

    private IEnumerator SmoothCenterX(float targetX)
    {
        float startX = transform.position.x;
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            float newX = Mathf.Lerp(startX, targetX, elapsed / duration);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }


    // 处理爬楼梯逻辑
    private void HandleClimbing()
    {
        if (isClimbing)
        {
            if (CheckStairsEnd() && !isClimbEnding)
            {
                StartCoroutine(ExitClimbState());
                return;
            }

            if (isClimbEnding) return;

            Physics2D.IgnoreLayerCollision(gameObject.layer, GroundLayerIndex(), true);

            float climbDirection = inputDirection.y;
            int dir = climbDirection > 0.1f ? 1 : (climbDirection < -0.1f ? -1 : 0);
            if (dir != 0) lastClimbDir = dir;
            rb.velocity = new Vector2(0, climbDirection * climbSpeed * Time.deltaTime);
            playerAnimator.SetFloat("ClimbSpeed", dir != 0 ? dir : 0);
            playerAnimator.SetBool("IsClimbing", true);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, GroundLayerIndex(), false);
            playerAnimator.SetBool("IsClimbing", false);
            playerAnimator.SetFloat("ClimbSpeed", 0);
        }
    }

    private IEnumerator ExitClimbState()
    {
        isClimbEnding = true;
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        playerAnimator.SetFloat("ClimbSpeed", 0);

        if (lastClimbDir == 1)
            playerAnimator.SetTrigger("ClimbEndUp");
        else
            playerAnimator.SetTrigger("ClimbEndDown");

        yield return new WaitForSeconds(0.5f);

        rb.simulated = true;
        isClimbing = false;
        isClimbEnding = false;
        canRun = true;
        climbExitCooldown = 0.3f;
        rb.gravityScale = defaultGravityScale;
        Physics2D.IgnoreLayerCollision(gameObject.layer, GroundLayerIndex(), false);
        playerAnimator.SetBool("IsClimbing", false);
    }

    // 检测是否到达楼梯端点（分方向检测，带入口保护期）
    private bool CheckStairsEnd()
    {
        if (climbEntryTimer > 0) return false;

        Collider2D checkCol;
        if (lastClimbDir == 1)
            checkCol = stairsEndCheckTop;
        else
            checkCol = stairsEndCheckBottom;

        if (checkCol == null) return false;
        return checkCol.IsTouchingLayers(stairsEndLayer);
    }

    // LayerMask转层级索引
    private int GroundLayerIndex()
    {
        return (int)Mathf.Log(groundLayer.value, 2);
    }

    // 开始边缘攀爬
    private void StartEdgeClimb(Transform platform)
    {
        isClimbingEdge = true;
        canRun = false;
        canClimb = false;
        currentClimbTarget = platform;
        rb.gravityScale = 0f; // 攀爬时重力为0
        // 触发攀爬动画
        playerAnimator.SetTrigger("Climb");
        // 计算攀爬目标位置
        float platformTop = platform.position.y + platform.GetComponent<Collider2D>().bounds.extents.y;
        float playerTop = transform.position.y + playerCollider.size.y * transform.localScale.y * 0.5f;
        float climbDistance = platformTop - playerTop;

        // 开始攀爬协程
        StartCoroutine(EdgeClimbCoroutine(climbDistance, platformTop));
    }

    // 边缘攀爬协程
    private IEnumerator EdgeClimbCoroutine(float distance, float targetY)
    {
        float startTime = Time.time;
        float climbDuration = distance / edgeClimbSpeed;
        float startY = transform.position.y;

        while (Time.time < startTime + climbDuration)
        {
            float t = (Time.time - startTime) / climbDuration;
            float currentY = Mathf.Lerp(startY, targetY, t);
            transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
            yield return null;
        }

        // 攀爬结束
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        EndEdgeClimb();
    }

    private float edgeClimbCooldown;

    // 结束边缘攀爬
    private void EndEdgeClimb()
    {
        isClimbingEdge = false;
        canRun = true;
        currentClimbTarget = null;
        edgeClimbCooldown = 0.5f;
        rb.gravityScale = defaultGravityScale;
        IsGround = true;
        CurrentJumpCount = MaxJumpCount;

        // 往上推一小段，防止角色还蹭在触发器边缘反复触发
        transform.position += new Vector3(0, 0.3f, 0);
    }

    // 碰撞检测：进入攀爬点
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(edgeTag) && !IsGround && !isClimbingEdge && !isClimbing)
        {
            canClimb = true;
        }
    }

    // 碰撞检测：离开攀爬点
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(edgeTag))
        {
            canClimb = false;
        }
    }

    // 处理攀爬输入
    private void HandleClimbInput()
    {
        if (canClimb && inputDirection.y > 0 && !isClimbingEdge && !isClimbing && edgeClimbCooldown <= 0)
        {
            // 找到最近的攀爬点
            Collider2D[] colliders = Physics2D.OverlapCircleAll(edgeCheckTransform.position, 1f);
            Collider2D closestClimbPoint = null;
            float closestDistance = float.MaxValue;

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(edgeTag))
                {
                    float distance = Vector2.Distance(edgeCheckTransform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestClimbPoint = collider;
                    }
                }
            }

            if (closestClimbPoint != null)
            {
                StartEdgeClimb(closestClimbPoint.transform);
            }
        }
    }


    [SerializeField] private int testBottleAmount = 1; // 每次添加的血瓶数量
    [SerializeField] private int testCurrencyAmount = 100; // 每次添加的金币数量

    // 测试方法：添加血瓶
    private void AddTestBottles()
    {
        if (PlayerItemManager.Instance != null)
        {
            // 假设血瓶的键名为"BloodBottle"
            PlayerItemManager.Instance.AddBottle("BloodBottle", testBottleAmount);
            Debug.Log($"测试：添加了 {testBottleAmount} 个血瓶");
        }
        else
        {
            Debug.LogError("PlayerItemManager实例不存在！");
        }
    }
    // 测试方法：添加金币
    private void AddTestCurrency()
    {
        if (PlayerItemManager.Instance != null)
        {
            // 添加金币
            PlayerItemManager.Instance.AddCurrency("Gold", testCurrencyAmount);
            Debug.Log($"测试：添加了 {testCurrencyAmount} 金币");
        }
        else
        {
            Debug.LogError("PlayerItemManager实例不存在！");
        }
    }


    // E键交互
    private void Interact()
    {
        if (!CanAct(ActionPriority.Interact)) return;
        // 先检查拾取物
        Collider2D[] pickups = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D col in pickups)
        {
            WorldPickup pickup = col.GetComponent<WorldPickup>();
            if (pickup != null && pickup.playerInRange)
            {
                pickup.Collect();
                return;
            }
        }

        // 没拾取物就检查机关
        InteractWithMechanism();
    }

    // 与机关交互
    private void InteractWithMechanism()
    {
        // 检测附近的机关
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2f);

        foreach (Collider2D collider in colliders)
        {
            Mechanism mechanism = collider.GetComponent<Mechanism>();
            if (mechanism != null)
            {
                mechanism.Activate();
                break;
            }
        }
    }




    void Move()
    {
        CurrentSpeed = Maxspeed;
        isXiaDun = inputDirection.y < -0.4f && IsGround;

        float moveX = isXiaDun ? 0f : inputDirection.x;
        rb.velocity = new Vector2(moveX * CurrentSpeed * Time.deltaTime, rb.velocity.y);
        playerAnimator.SetFloat("RunSpeed", Mathf.Abs(rb.velocity.x));

        faceDir = (int)transform.localScale.x;
        if (inputDirection.x > 0)
        {
            faceDir = 1;
        }

        if (inputDirection.x < 0)
        {
            faceDir = -1;
        }
        if (isXiaDun)
        {
            playerCollider.offset = new Vector2(playerCollider.offset.x, startOffsetIntensity);
            playerCollider.size = new Vector2(playerCollider.size.x, startSizeIntensity);
        }
        else
        {
            playerCollider.offset = startOffset;
            playerCollider.size = startSize;
        }
        transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);
    }


    void Jump()
    {
        // 攻击、技能、闪避或受伤期间不能跳跃
        if (!CanAct(ActionPriority.Jump)) return;

        // 跳跃打断钉墙状态
        if (isNailedToWall)
        {
            NailWallJump();
            return;
        }
        // 跳跃打断爬楼梯状态
        if (isClimbing)
        {
            isClimbing = false;
            canRun = true;
            rb.gravityScale = defaultGravityScale; // 恢复默认重力
            // 恢复与地面的碰撞
            Physics2D.IgnoreLayerCollision(gameObject.layer, GroundLayerIndex(), false);
            // 重置动画状态
            playerAnimator.SetBool("IsClimbing", false);
            playerAnimator.SetFloat("ClimbSpeed", 0);

            // 应用跳跃力，就像在平地上跳跃一样
            jumpGraceTimer = 0.15f;
            playerAnimator.SetBool("JumpNow", true);
            playerAnimator.SetTrigger("Jump");

            // 应用水平方向的输入，实现左右跳跃
            float horizontalInput = inputDirection.x;
            rb.velocity = new Vector2(horizontalInput * Maxspeed * Time.deltaTime, 0);
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            CurrentJumpCount--;
        }
        else if (IsGround || _coyoteTimer > 0 || CurrentJumpCount > 0)
        {
            jumpGraceTimer = 0.15f;
            playerAnimator.SetBool("JumpNow", true);
            playerAnimator.SetTrigger("Jump");
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            CurrentJumpCount--; _coyoteTimer = 0;
        }
    }


    void ShanBiAnimation()
    {
        if (!CanAct(ActionPriority.Dodge)) return;
        if (cooldownTimer <= 0 && canShanBi && IsGround)
        {
            isDodgeCharging = true;
            dodgeChargeTimer = 0f;
            isDodging = true;
            playerAnimator.SetTrigger("ShanBi");
            cooldownTimer = dodgeCooldown;
        }
    }

    // 闪避出招，动画事件调用
    public void DodgeDash()
    {
        if (!isDodgeCharging) return;
        isDodgeCharging = false;

        float distance = dodgeChargeTimer >= dodgeChargeThreshold ? dodgeLongDistance : dodgeShortDistance;
        StartCoroutine(DodgeDashCoroutine(distance));
    }

    private IEnumerator DodgeDashCoroutine(float distance)
    {
        float startX = transform.position.x;
        float targetX = startX + faceDir * distance;
        float elapsed = 0f;

        while (elapsed < dodgeDashDuration)
        {
            float newX = Mathf.Lerp(startX, targetX, elapsed / dodgeDashDuration);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    // 闪避结束，动画事件调用
    public void DodgeEnd()
    {
        isDodging = false; ClearDispatcherState(); TryBufferedJump();
    }




    private void checkIsGround()
    {
        bool wasGrounded = IsGround;
        IsGround = Physics2D.OverlapCircle(checkTransfrom.position, CheckRadious, groundLayer);
        if (IsGround) _coyoteTimer = config != null ? config.coyoteTime : 0.15f;
        else _coyoteTimer -= Time.deltaTime;

        if (IsGround && jumpGraceTimer <= 0)
        {
            playerAnimator.SetBool("JumpNow", false);
            CurrentJumpCount = MaxJumpCount;
            if (!wasGrounded) { ClearDispatcherState(); TryBufferedJump(); }
        }
        else if (!IsGround) { playerAnimator.SetBool("JumpNow", true); }
    }

    private void TryBufferedJump()
    {
        if (_inputBuffer.HasAction(GameAction.Jump) && CanAct(ActionPriority.Jump))
        { _inputBuffer.ConsumeAction(GameAction.Jump); Jump(); }
    }
    public void PlayerHuiXue()
    {
        if (!CanAct(ActionPriority.Heal)) return;
        playerAnimator.SetTrigger("HuiXue");
    }
    public void PlayerHert()
    {
        if (isSkillActive) return;
        if (character.isHeavyHurt)
            playerAnimator.SetTrigger("HertHeavy");
        else
            playerAnimator.SetTrigger("Hert");
    }

    // 普通受伤动画结束，动画事件调用
    public void HurtEnd()
    {
        isHurt = false;
        character.isInHurtAnimation = false;
    }

    // 严重受伤落地，触发爬起动作，动画事件调用
    public void HurtHeavyEnd()
    {
        character.isInHurtAnimation = false;
        playerAnimator.SetTrigger("HertRecover");
    }

    private IEnumerator RespawnRecoverRoutine()
    {
        yield return new WaitForSeconds(1.2f);
        isHurt = false;
    }

    // 爬起动作结束，动画事件调用
    public void HurtRecoverEnd()
    {
        isHurt = false;
        gameObject.layer = originalLayer;
    }
    // 动画事件调用，攻击收尾预留取消窗口
    public void AttackCancelWindow()
    {
        isAttack = false; ClearDispatcherState(); TryBufferedJump();
    }

    public void SkillCancelWindow()
    {
        isSkillActive = false; ClearDispatcherState(); TryBufferedJump();
    }

    public void PlayerAtatck()
    {
        if (!CanAct(ActionPriority.Attack)) return;
        // 检测是否可以钉墙
        if (!IsGround && !isNailedToWall && CheckCanNailToWall())
        {
            EnterNailWallState();
            return;
        }
        playerAnimator.SetTrigger("attack");
    }
    public void PlayerSkill_1()
    {
        if (!CanAct(ActionPriority.Skill)) return;
        if (!character.ConsumeMana(skill1ManaCost)) return;

        isSkillActive = true;
        playerAnimator.SetTrigger("Skill_1");
    }

    // 技能1结束，动画事件调用
    public void Skill1End()
    {
        isSkillActive = false; ClearDispatcherState(); TryBufferedJump();
    }
    public void PlayerSkill_2()
    {
        if (isAttack || isSkillActive || isHurt || isDodging) return;
        if (!character.ConsumeMana(skill2ManaCost)) return;

        isSkillActive = true;
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, true);
        playerAnimator.SetTrigger("Skill_2");
    }

    // 技能2出剑冲刺，动画事件调用
    public void Skill2Dash()
    {
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        float startX = transform.position.x;
        float targetX = startX + faceDir * skill2DashDistance;
        float elapsed = 0f;

        while (elapsed < skill2DashDuration)
        {
            float newX = Mathf.Lerp(startX, targetX, elapsed / skill2DashDuration);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    // 技能2结束，动画事件调用
    public void Skill2End()
    {
        isSkillActive = false;
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, false);
    }

    public void PlayerSkill_3()
    {
        if (isAttack || isSkillActive || isHurt || isDodging) return;
        if (!character.ConsumeMana(skill3ManaCost)) return;

        isSkillActive = true;
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, true);
        playerAnimator.SetTrigger("Skill_3");
    }

    // 技能3结束，动画事件调用
    public void Skill3End()
    {
        isSkillActive = false;
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, false);
    }

   


    public void PlayerAnimationBoolSet()
    {
        playerAnimator.SetBool("canShanBi", canShanBi);
        playerAnimator.SetBool("canAttack", canAttack);
        playerAnimator.SetBool("canRun", canRun);
        playerAnimator.SetBool("IsXiaDun", isXiaDun);
        playerAnimator.SetFloat("JumpFloat", rb.velocity.y);
        playerAnimator.SetBool("IsDeath", isDie);
        playerAnimator.SetBool("isHurt", isHurt);
        if (isHurt || isDodging) playerAnimator.SetFloat("RunSpeed", 0);

    }

    public void playerDie()
    {
        isDie = true;
        inputControl.GamePlay.Disable();

        // 触发死亡过渡效果
        FadeManager.DeathFade(OnDeathFadeComplete);
    }

    // 死亡过渡效果完成后的回调
    private void OnDeathFadeComplete()
    {
        // 查找复活点
        GameObject respawnPoint = GameObject.Find("FuHuoDian");
        if (respawnPoint != null)
        {
            // 传送玩家到复活点
            transform.position = respawnPoint.transform.position;
        }

        // 重置玩家状态，起身期间锁移动
        isDie = false;
        isHurt = true;
        character.isInHurtAnimation = false;
        StartCoroutine(RespawnRecoverRoutine());

        // 复活后短暂无敌
        character.WuDiState = true;
        character.WuDiJiShiQi = 1f;

        float healthToRestore = character.maxHealth - character.currentHealth;
        float manaToRestore = character.maxMana - character.currentMana;
        character.HealHealth(healthToRestore);
        character.RestoreMana(manaToRestore);

        // 重置动画状态
        playerAnimator.SetBool("IsDeath", false);

        // 触发起身动画
        playerAnimator.SetTrigger("HertRecover");

        // 重新启用输入
        inputControl.GamePlay.Enable();
    }
    public void GetHurt(Transform attacker)
    {
        if (isSkillActive) return;

        // 爬梯中被攻击就掉下来
        if (isClimbing)
        {
            isClimbing = false;
            isClimbEnding = false;
            canRun = true;
            climbExitCooldown = 0.5f; // 防止立刻重进
            rb.gravityScale = defaultGravityScale;
            Physics2D.IgnoreLayerCollision(gameObject.layer, GroundLayerIndex(), false);
            playerAnimator.SetBool("IsClimbing", false);
            playerAnimator.SetFloat("ClimbSpeed", 0);
            playerAnimator.SetInteger("ClimbDirection", 0);
        }

        isHurt = true;
        character.isInHurtAnimation = true;
        rb.velocity = Vector2.zero;

        // 面朝攻击者
        float dirToAttacker = attacker.position.x - transform.position.x;
        if (Mathf.Abs(dirToAttacker) > 0.1f)
        {
            faceDir = dirToAttacker > 0 ? 1 : -1;
            transform.localScale = new Vector3(faceDir, transform.localScale.y, transform.localScale.z);
        }

        // 击退方向：背向攻击者
        Vector2 hurtDir = (transform.position - attacker.position).normalized;

        if (character.isHeavyHurt)
        {
            // 严重受伤：切到闪避层（该层已忽略敌人碰撞），抛物线飞出去
            gameObject.layer = dodgeLayerIndex;
            rb.AddForce(new Vector2(hurtDir.x * hurtForce * 1.5f, hurtForce * 1.2f), ForceMode2D.Impulse);
        }
        else
        {
            // 普通受伤：小击退
            rb.AddForce(hurtDir * hurtForce * 0.5f, ForceMode2D.Impulse);
        }
    }

    private void UseBloodBottle()
    {
        PlayerItemManager.Instance.UseBottle("BloodBottle");
    }

    private void CheckState()
    {
        if (isDie)
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        else
            gameObject.layer = LayerMask.NameToLayer("Player");
        playerCollider.sharedMaterial = IsGround ? normalPhysic : jumpPhysic;
    }

    private void Respawn()
    {
        // 获取保存的重生点名称
        string spawnPointName = PlayerPrefs.GetString("SpawnPointName");

        // 找到重生点
        GameObject spawnPoint = GameObject.Find(spawnPointName);
        if (spawnPoint != null)
        {
            // 移动玩家到重生点
            transform.position = spawnPoint.transform.position;

            // 设置玩家朝向 (使用localScale)
            int faceDirection = PlayerPrefs.GetInt("FaceDirection", 1);
            transform.localScale = new Vector3(faceDirection, transform.localScale.y, transform.localScale.z);
        }

        // 清理临时数据
        PlayerPrefs.DeleteKey("SpawnPointName");
        PlayerPrefs.DeleteKey("FaceDirection");
    }


    public void OpenInputController()
    {
        inputControl.Enable(); // 修改为方法调用形式
    }

    public void CloseInputController()
    {
        inputControl.Disable(); // 添加关闭输入的方法
    }



    // 处理道具使用
    private void HandleItemUsage()
    {
        // Y键使用远程攻击
        // U键使用远程攻击道具
        if (Input.GetKeyDown(KeyCode.U))
        {
            UseItem("RemoteAttack");
        }
        // I键使用无敌道具
        if (Input.GetKeyDown(KeyCode.I))
        {
            UseItem("Invincible");
        }
        // O键使用召唤道具
        if (Input.GetKeyDown(KeyCode.O))
        {
            UseItem("Summoning");
        }
        // P键清空金币（测试用）
        if (Input.GetKeyDown(KeyCode.P))
        {
            int currentGold = PlayerItemManager.Instance.GetCurrencyAmount("Gold");
            PlayerItemManager.Instance.SpendCurrency("Gold", currentGold);
            Debug.Log("金币已清零");
        }
    }

    // 使用道具
    private void UseItem(string itemName)
    {
        if (ShopManager.Instance == null) return;

        ShopItemData item = ShopManager.Instance.GetItemByName(itemName);
        if (item == null)
        {
            Debug.LogWarning($"没有找到道具: {itemName}");
            return;
        }

        if (item.currentAmount <= 0)
        {
            Debug.Log($"道具 {itemName} 数量不足!");
            return;
        }

        // 根据道具名称触发对应动作
        switch (itemName)
        {
            case "RemoteAttack":
                ShopManager.Instance.UseItem(item);
                playerAnimator.SetTrigger("RemoteAttack");
                break;

            case "Invincible":
                ShopManager.Instance.UseItem(item);
                playerAnimator.SetTrigger("Invincible");
                break;

            case "Summoning":
                ShopManager.Instance.UseItem(item);
                playerAnimator.SetTrigger("Summoning");
                break;
        }
    }

    // 生成血刃（由动画事件调用）
    public void SpawnBloodBlade()
    {
        if (bloodBladePrefab != null && bloodBladeSpawnPoint != null)
        {
            // 在空物体位置生成血刃
            Vector3 spawnPos = bloodBladeSpawnPoint.position;
            Quaternion spawnRot = Quaternion.Euler(0, 0, transform.localScale.x > 0 ? 0 : 180);
            GameObject blade = Instantiate(bloodBladePrefab, spawnPos, spawnRot);

            // 设置血刃方向（根据玩家朝向）
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            blade.transform.right = new Vector2(direction, 0);

            Debug.Log("生成血刃! 方向: " + direction);
        }
        else
        {
            if (bloodBladeSpawnPoint == null)
            {
                Debug.LogWarning("血刃生成点为空，请在玩家对象上创建一个空物体并赋值给bloodBladeSpawnPoint！");
            }
        }
    }

    // 开始无敌（由动画事件调用）
    public void StartInvincible()
    {
        isInvincible = true;
        character.WuDiState = true;
        character.WuDiJiShiQi = invincibleDuration;

        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, true);

        if (playerSpriteRenderer != null)
            playerSpriteRenderer.color = invincibleColor;

        if (invincibleLight != null)
            invincibleLight.enabled = true;

        _activeInvincibleEffect = InvincibleEffect.Create(invincibleDuration, character);
        character.effectManager.Add(_activeInvincibleEffect);

        Invoke("EndInvincible", invincibleDuration);
    }

    private void EndInvincible()
    {
        isInvincible = false;
        character.WuDiState = false;

        if (invincibleLight != null) invincibleLight.enabled = false;
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayerIndex, false);
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = originalColor;

        if (_activeInvincibleEffect != null)
        { character.effectManager.Remove(_activeInvincibleEffect); _activeInvincibleEffect = null; }
    }

    // 生成召唤物（由动画事件调用）
    public void SpawnSummon()
    {
        // 没拖就自动找玩家身上的"point_2"
        if (summonSpawnPoint == null)
            summonSpawnPoint = transform.Find("point_2");

        if (summonPrefab != null && summonSpawnPoint != null)
        {
            Vector3 spawnPos = summonSpawnPoint.position;
            float direction = transform.localScale.x > 0 ? 1f : -1f;

            GameObject summon = Instantiate(summonPrefab, spawnPos, Quaternion.identity);
            summon.transform.localScale = new Vector3(direction, 1f, 1f);

            // 设置跟随目标
            SummonCreature creature = summon.GetComponent<SummonCreature>();
            if (creature != null)
                creature.followTarget = transform;
        }
    }

}

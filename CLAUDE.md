# DuShen（赌神）— 2D 动作游戏框架

> 基于"水系架构模型"和"五河原则"的 Unity 2D 动作游戏模板

---

## 项目概述

**类型**：2D 横版动作平台游戏  
**引擎**：Unity（URP 2D）  
**语言**：C#  
**核心玩法**：探索 → 遇敌 → 资源博弈（闪避/蓝量/道具） → 击杀掉落 → Boss 终结

**参考游戏**：《渎神》(Blasphemous) 风格——黑暗奇幻、沉重手感、技能驱动战斗

---

## 架构：五河模型

```
                     ┌──────────────┐
                     │   EventBus   │  ← 河流间通信的唯一通道
                     │   QueryBus   │
                     │  Blackboard  │
                     └──┬───┬───┬──┘
        ┌───────────────┤   │   ├───────────────┐
        ▼               ▼   ▼   ▼               ▼
   Character        Combat   World   Narrative   Resource
   (角色河)         (战斗河)  (世界河)  (叙事河)    (资源河)
```

### 五条河流的职责边界

| 河流 | 文件夹 | 拥有的功能 | 绝不管的事 |
|---|---|---|---|
| **Core** | `Scripts/Core/` | EventBus, QueryBus, Blackboard, GameEvents | 不包含任何游戏逻辑 |
| **Character** | `Scripts/Character/` | 移动、动画、输入、物理状态、动作调度 | 伤害、UI、任务 |
| **Combat** | `Scripts/Combat/` | 伤害判定、HitTracker、Effect系统、攻防公式 | 动画细节、背包内容 |
| **World** | `Scripts/World/` | 场景加载、相机、交互物、对象池、音频 | 战斗公式、剧情 |
| **Narrative** | `Scripts/Narrative/` | 对话、任务、游戏结束、UI管理 | 战斗结果、角色位置 |
| **Resource** | `Scripts/Resource/` | 物品定义、背包、商店、配置SO | 战斗流程、动画 |

### 判断标准

> 如果一个"模块"的内部子模块之间需要用 EventBus 来通信，那它就不是一个模块——它是两条独立的河流。

---

## 目录结构

```
Assets/
├── _Game/                              # 游戏内容资产
│   ├── Scenes/                         # 场景文件 (.unity)
│   ├── Prefabs/                        # 预制体
│   │   ├── Player/       Boss/         Enemies/
│   │   ├── Environment/  Effects/      UI/
│   │   ├── Managers/     Items/        Lights/
│   ├── ScriptableObjects/
│   │   ├── Configs/      AudioEvents/  Economy/
│   │   └── Settings/
│   └── Settings/
├── Scripts/
│   ├── Core/          EventBus, QueryBus, Blackboard, GameEvents
│   ├── Character/     角色河：Player, Animation, StateMachine, Action
│   ├── Combat/        战斗河：Attack, Damage, Effect, Enemy, Boss
│   ├── World/         世界河：Audio, Camera, Portal, Pickup, Pool
│   ├── Narrative/     叙事河：Dialogue, Quest, UI, GameEnd
│   ├── Resource/      资源河：Config, Economy, Shop, Inventory
│   └── Editor/
├── Audio/             .wav/.mp3 文件
├── Art/               美术资源
├── Animations/        .anim/.controller 文件
├── Resources/         Unity 运行时加载目录
└── InputSystem/       Unity Input System 配置
```

---

## 核心系统

### 通信骨架

```csharp
// 广播——"发生了一件事，谁关心谁听"
EventBus.Emit(new EnemyDeathEvent { ... });
EventBus.On<EnemyDeathEvent>(OnEnemyDied);

// 查询——"我需要一个值，你们谁有"
QueryBus.Register<DamageModQuery, float>(myHandler);
var mods = QueryBus.Collect<DamageModQuery, float>(query);

// 全局状态——全游戏不超过 15 个字段
Blackboard.SetPhase(GamePhase.Normal);
if (Blackboard.CanPlayerAct) { ... }
```

### 动作优先级

```csharp
// 不再写: if(isAttack || isSkillActive || isHurt || isDodging) return;
// 改为:
if (!CanAct(ActionPriority.Jump)) return;

// 优先级: Cutscene(0) > Death(10) > HitStun(20) > Skill(40)
//          > Attack(50) > Item(55) > Dodge(60) > Jump(65)
//          > Heal(70) > Interact(75) > Move(80) > Idle(100)
```

### 伤害计算

```
伤害流程:
  AttackBase.OnTriggerEnter2D
    → HitTracker.TryHit() (命中去重)
    → Character.TakeDmage()
    → ApplyDamage()
        → DamageCalculator.Calculate()
            → QueryBus.Collect(AttackerBonus)    ← Buff/装备
            → QueryBus.Collect(DefenderResist)    ← 护甲/无敌
            → QueryBus.Collect(HitPart)           ← 部位
        → EventBus.Emit(PlayerDamagedEvent)
        → EventBus.Emit(EnemyDeathEvent)
```

### Effect 系统

```csharp
// 添加一个 Effect
character.effectManager.Add(new InvincibleEffect(5f));

// 创建自定义 Effect——不需要改任何现有代码
class MyBuff : IEffect, IQueryHandler<DamageModQuery, float> { ... }

// 5 种叠加规则: Independent / RefreshDuration / StackIntensity / Replace / StrongestFirst
```

### 配置分离

所有可调参数通过 ScriptableObject 管理：
- `PlayerConfig_SO` — 移动速度、跳跃力、闪避距离、技能蓝耗...
- `EnemyConfig_SO` — 巡逻速度、攻击范围、掉落数量...
- `CharacterStats_SO` — 生命值、蓝量、回蓝速度、无敌帧...

在 Inspector 中调参 → 不需要打开代码文件。

---

## 编码规范

### 命名空间

```
GameArchitecture.Core       → Core/ 下的文件
GameArchitecture.Character  → Character/ 下的文件
GameArchitecture.Combat     → Combat/ 下的文件
GameArchitecture.World      → World/ 下的文件
GameArchitecture.Narrative  → Narrative/ 下的文件
GameArchitecture.Resource   → Resource/ 下的文件
```

### 事件命名

```
正确: EnemyDeathEvent, PlayerDamagedEvent, ItemPickedUpEvent
错误: EnemyEvent, OnEnemyDeath, EnemyKilledMsg
```

### MonoBehaviour 使用原则

> MonoBehaviour 的唯一职责是连接 C# 逻辑和 Unity 引擎。它是胶水，不是逻辑容器。

以下情况**应该**用 MonoBehaviour：
- 需要在 Inspector 里拖拽引用
- 需要 Update/FixedUpdate/LateUpdate
- 需要协程

以下情况**不应该**用 MonoBehaviour：
- 纯数据计算（伤害公式、掉落概率）
- 状态机逻辑
- 事件处理
- 任何可以用纯 C# 类完成的事情

### 通信原则

```
错误: 河流 A 需要河流 B 的数据 → A 直接引用 B
正确: 河流 A 需要河流 B 的数据 → A 向 EventBus/QueryBus 查询
```

---

## 快速开始

1. 打开 Unity（URP 2D 项目）
2. 打开 `_Game/Scenes/1_start.unity` 起始场景
3. `_Game/Prefabs/Player/Player.prefab` — 玩家预制体
4. `_Game/ScriptableObjects/` — 所有配置资产

### 添加新敌人

1. 复制 `_Game/Prefabs/Enemies/QiangDao.prefab`
2. 创建 `_Game/ScriptableObjects/Configs/EnemyConfig_New.asset`
3. 拖 SO 到 Prefab 的 `EnemyBase.config` 字段
4. （可选）创建新 Controller 脚本继承 `EnemyBase`

### 添加新效果

```csharp
// 新建文件: Scripts/Combat/Effects/MyEffect.cs
public class MyEffect : IEffect, IQueryHandler<DamageModQuery, float>
{
    // 实现接口...
    public float Handle(DamageModQuery q)
    {
        if (q.phase == ModPhase.AttackerBonus)
            return 1.5f; // 50% 伤害加成
        return 1f;
    }
}
// 使用: character.effectManager.Add(new MyEffect(10f));
```

---

## 已知待改进

见 `/docs/ROADMAP.md`

---

## 参考

- `D:/unity_school/动作游戏架构设计教程_第一部分_总纲与通信.md`
- `D:/unity_school/动作游戏架构设计教程_第二部分_战斗系统.md`
- `D:/unity_school/动作游戏架构设计教程_第三部分_角色系统.md`
- `D:/unity_school/动作游戏架构设计教程_第四部分_世界系统.md`
- `D:/unity_school/动作游戏架构设计教程_第五部分_叙事与存档.md`

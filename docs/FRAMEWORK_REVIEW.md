# 框架评审与路线图 — DuShen v2.0 → v3.0

> 严格评估 + 业界对照 + 未来规划。不是吹捧文档，是直面问题的工程分析。

---

## 第一部分：当前框架评估

### 做得好的 ✅

#### 通信骨架 — 可以上生产线

`EventBus` / `QueryBus` / `Blackboard` 的设计是类型安全的、反递归的、带异常隔离的。这三样放在商业项目里也不掉价。

对比：Unreal Engine 用 Delegate + Event System 解决同类问题，Hollow Knight 内部也是类似的事件广播架构。你的实现比 Unity 原生的 `UnityEvent` 更轻、更快。

#### Effect 系统 — 设计思路对

`IEffect` + `QueryBus Handler` 的注册/注销模式，和 Hades 的 Boon 系统、Dead Cells 的变异系统、Slay the Spire 的遗物系统在架构上同构——"注册一个效果，自动影响伤害计算，过期自动清除"。扩展性很好。

#### 动作优先级 — 正确方向

`ActionDispatcher` + `CanAct` 把原来散落在 10 处的 `if(isAttack||isSkillActive||isHurt)` 统一了。这个模式和 Celeste 的动作系统、Hollow Knight 的状态机是同一种思路。

---

### 有问题的 ❌

#### 1. PlayerController 1396 行 — 核心槽点

```
业界标准:
  Celeste:    Player 拆成 ~10 个状态类，每个 ~100 行
  Dead Cells: 30+ 状态的有限状态机
  Hades:      每个武器/能力独立组件，Player 本身不超过 300 行

我们:
  PlayerController 包含跳跃/闪避/攻击/技能/攀爬/钉墙/道具/死亡/受击/动画/输入
```

《Code Complete》建议一个方法不超过 30 行，一个类不超过 500 行。1396 行是硬性结构缺陷——不是风格问题，是维护性问题。

#### 2. 两套系统并存 — 最严重的债务

```
旧系统（实际在跑，预制体在用）:
  Character.cs    → 血量/蓝量/伤害/死亡 全包
  AttackBase.cs   → 判定+震屏+音效+血特效 全包

新系统（代码写好了，预制体没接）:
  HealthComponent + PlayerStats + EnemyDeathHandler
  HitboxComponent + HitPresentation
```

这导致"文档架构"和"实际架构"脱节。学弟学妹打开项目看到两套血量系统，不知道该用哪个。在软件工程里这叫做 Dead Code + Speculative Generality——两个都是必须偿还的技术债。

#### 3. Singleton 地狱 — 可测试性为零

```csharp
// 遍布全项目的调用链:
AudioManager.Instance.PlaySound(...)
EffectPoolManager.Instance.ShakeScreen(...)
ShopManager.Instance.UseItem(...)
PlayerItemManager.Instance.AddCurrency(...)
```

测试一个 PlayerController 需要一连串单例全部初始化。任何一个不存在就是 `NullReferenceException`。教程第一部分明确批评了这种写法，但你项目里到处都是。这叫"知道但没做到"。

#### 4. 没有测试 — 不敢重构

改一行代码不知道会不会炸，所以不敢动。没有单元测试就没有重构的信心。EventBus、DamageCalculator、CanAct 这些纯逻辑完全可以脱离 Unity 测试——但你一个测试都没写。

#### 5. 命名空间覆盖率不足

120+ 个脚本中，只有 13 个有 `namespace`。其余 100+ 个在全局命名空间。编译器不会说，但 IDE 智能提示列出一百多个类，找不到谁属于哪个模块。

---

### 业界对照表

| 标准 | Celeste | Hollow Knight | Dead Cells | 我们的框架 |
|---|---|---|---|---|
| Player 拆模块 | ✅ 状态机10类 | ✅ 能力组件化 | ✅ 30状态机 | ❌ 1396行单体 |
| 通信解耦 | ✅ Event | ✅ 事件系统 | ✅ 信号系统 | ⚠️ 有但旧代码不用 |
| 数据驱动 | ✅ 关卡XML | ✅ SO资产 | ✅ 武器配置 | ⚠️ SO有但没全接 |
| 单元测试 | ✅ | ✅ | ✅ | ❌ 零 |
| DI/IoC | ❌ | ⚠️ 部分 | ⚠️ 部分 | ❌ Instance滥用 |
| 存档稳健 | ✅ 多槽+校验 | ✅ 集中存档 | ✅ 自动存档 | ⚠️ 设计好但分散 |
| 文档 | ✅ | ❌ | ❌ | ✅ CLAUDE.md |

---

## 第二部分：当前优先级

### 必须修（不修不能叫框架）

| # | 事项 | 影响 |
|---|---|---|
| 1 | **拆 PlayerController** | 消除最大槽点，每部分控制在 200 行以内 |
| 2 | **迁移到新组件** | 让 HealthComponent/HitboxComponent 成为唯一系统 |
| 3 | **加核心测试** | 至少 EventBus、DamageCalculator、CanAct 有 100% 覆盖 |
| 4 | **补齐命名空间** | 旧文件加 namespace，IDE 和编译器能分辨模块边界 |

---

## 第三部分：v3.0 展望

### 开发者体验

#### 1. 调试面板（半天工作量）

按 `~` 弹出半透明面板，实时显示：

```
状态:    Player | Action:Idle | Anim: Player_Run
         canRun=true | coyoteTimer=0.12s
事件:    [0.3s] EnemyDeathEvent → QuestSystem ✓
         [0.5s] ItemPickedUpEvent → PlayerItemManager ✓
Effect:  [Invincible 3.2s] [AttackUp 8.1s]
存档:    save_1.json | 85 commands | last @ Church
```

参考《Celeste》开发版内置控制台、《Dead Cells》Mod 调试面板。EventBus 已有，做这个只是读数据。

#### 2. 示例场景

```
Demo/
├── Demo_Movement.unity    ← 平台挑战，展示土狼/缓冲
├── Demo_Combat.unity      ← 竞技场，展示战斗
├── Demo_Effect.unity      ← Buff/Debuff 演示
└── Demo_Save.unity        ← 存档读档演示
```

学弟学妹打开即玩。参考 Unreal Engine 的 Content Examples。

#### 3. 热重载战斗数据

Play 模式下改 PlayerConfig_SO 的 maxSpeed → 立刻生效。调手感不用反复重启。参考《守望先锋》自定义房间。

### 游戏感系统

#### 4. 打击感增强包（HitStopConfig SO）

```
轻攻击 → 0.03s 时停 + 微震 + 白闪
重攻击 → 0.08s 时停 + 强震 + 红闪 + 粒子
暴击   → 定格帧 + 全屏震动 + 方向性爆炸
```

参考《怪物猎人》的 HitStop——命中瞬间暂停 0.05 秒，画面震一下，怪物闪白。这是"打击感"的核心公式。

#### 5. 屏幕特效管理器

```
Vignette（残血暗角）          — 已有雏形
ChromaticAberration（闪避色散）
RadialBlur（Boss登场径向模糊）
SlowMotion（大招慢动作）
ColorGrading（区域色调变化）
```

每个效果实现 `IScreenEffect`，和 EffectManager 同款设计。参考《Katana Zero》的失焦模糊。

### 内容生产工具

#### 6. 关卡编辑器基础

```
LevelBlock 预制体: 地面 + 敌人出生点 + 道具 + 门
LevelData SO:     关卡名 / BGM / 敌人列表 / 相机配置 / 通关条件

关卡 = LevelBlock 拼接 + LevelData 配置
```

《马里奥制造》思路：关卡 = 零件 + 配置。EnemyConfig_SO 已有，加关卡配置是自然延伸。

#### 7. 对话系统可视化

Unity GraphView 节点编辑器，对话树可视化。参考《极乐迪斯科》的技能检定对话系统。

---

## 版本计划

| 版本 | 目标 | 预计 |
|---|---|---|
| v2.0 | ✅ 完成 — 五河架构 + 核心系统 | 当前 |
| v2.1 | 拆 PlayerController + 新组件迁移 + 测试 | 下一步 |
| v2.2 | 调试面板 + 示例场景 + 打击感增强包 | |
| v2.3 | 热重载 + 屏幕特效 + 关卡编辑器基础 | |
| v3.0 | 对话系统 + 完整测试覆盖 + 官方示例游戏 | 远期 |

---

## 一句话总结

> 地基是对的。EventBus、QueryBus、Effect 系统放商业项目里不掉价。问题是盖在地上的楼有两层——旧的那层住了人，新的那层是毛坯。先把毛坯装修完、把旧住户迁过去。然后在这个干净基础上盖 v3.0 的新楼层。

# 最近一次会话记录

## 日期
2026-05-29

## 本轮完成
- PlayerController 从备份恢复 → 全部 Phase 2-4 功能加回
  - CanAct 统一入口 + ActionDispatcher
  - InputBuffer (150ms) + CoyoteTime (土狼时间)
  - TryBufferedJump 在各动作结束回调
  - SyncFlagsWithAnimator 安全网（LateUpdate）
  - FixedUpdate canRun 安全网
  - InvincibleEffect 集成（StartInvincible/EndInvincible）
  - SO 配置加载（ApplyConfig）
- PlayerController partial 拆分尝试失败（已回滚，勿再试）
- 创建 FRAMEWORK_REVIEW.md — 框架评审 + v3.0 路线图

## PlayerController 当前状态
- 1396 行，157/157 括号平衡，零编译错误
- 所有 Phase 2-4 功能已集成
- 需要拆分但不要再尝试 partial 方案

## 下一步（按优先级）
1. **拆 PlayerController** — 这次用状态机或组件模式，不用 partial
2. **迁移到新组件** — HealthComponent/HitboxComponent 真正跑起来
3. **加核心测试** — EventBus、DamageCalculator、CanAct
4. **补齐命名空间** — 旧文件加 namespace

## 关键文件位置
- CLAUDE.md — 框架说明书
- docs/FRAMEWORK_REVIEW.md — 评审 + 路线图
- docs/ROADMAP.md — 任务清单
- docs/SESSION_NOTES.md — 本文件

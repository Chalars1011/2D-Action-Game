# 框架完善路线图

## 已完成 ✅

- [x] EventBus / QueryBus / Blackboard 通信骨架
- [x] 五河目录结构重组
- [x] ActionPriority + ActionDispatcher 动作优先级
- [x] DamageCalculator 三阶段 Modifier Chain
- [x] HitTracker 命中去重
- [x] IEffect + EffectManager 效果框架
- [x] InvincibleEffect（首个完整 Effect 实现）
- [x] IPoolable 对象池接口
- [x] PlayerConfig_SO / EnemyConfig_SO / CharacterStats_SO
- [x] SO 资产一键生成（Tools → Generate All SO Assets）
- [x] InputBuffer 输入缓冲 + CoyoteTime 土狼时间
- [x] 旧代码标记（Character / AttackBase 废弃说明）
- [x] HealthComponent / HitboxComponent / HitPresentation
- [x] PlayerStats / EnemyDeathHandler
- [x] BloodBlade 伤害独立配置
- [x] CameraShaker + ShakeProfile 震动系统
- [x] CameraLookAhead 相机预瞄
- [x] HitboxManager + HitboxController + HurtboxComponent 帧驱动判定
- [x] SaveManager 命令模式存档 + IStateTracked 快照系统
- [x] PlayerController 拆分为 4 个 partial 文件
- [x] CLAUDE.md 项目文档

## 待开始 📋

### 短期
- [ ] 物理/视觉分离 (Physics Root + Visual Child)
- [ ] 框架示例场景（干净的新场景展示各模块用法）

### 长期
- [ ] 对话图系统
- [ ] 任务系统
- [ ] 单元测试覆盖核心系统
- [ ] Editor Gizmos（Hitbox/Hurtbox 可视化）

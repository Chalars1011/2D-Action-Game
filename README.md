# 2D-Action-Game · 横版动作RPG

> 毕业设计项目 | Unity 2D + C# | 独立开发

## 项目简介

一款横版动作游戏，实现了完整的角色控制、战斗系统、Boss 战和敌人 AI。

PlayerController 核心 1300+ 行，共计 100+ 个 C# 脚本。

## 主要功能

### 角色控制
- 多段跳跃（土狼时间）、蓄力闪避（短按短闪 / 长按长闪）
- 空中钉墙（检测 → 钉入 → 下滑 → 踢墙跳）、攀爬楼梯和边缘
- 蓝量系统（自然回复 + 3 个技能消耗）

### 战斗系统
- 3 个主动技能 + 血瓶 / 血刃 / 召唤物道具系统
- 对象池管理弹幕和特效，避免 GC 卡顿

### Boss 战
- Behavior Designer 行为树驱动 AI：条件节点（距离 / 血量 / 朝向 / 技能CD）+ 行为节点（轻击 / 重击 / 追踪 / 技能），多阶段切换
- 死亡演出：冻结帧 + 震屏

### 敌人系统
- 6 种小怪独立 AI（近战追踪 / 远程弹幕 / 高血量 / 冲撞 / 炮台 / 浮游追踪）

### 其他
- 音频系统（BGM / 环境 / 脚步 / 武器四类管理）
- 相机系统（跟随 + 前瞻 + 震屏）
- 商店、传送门、陷阱机关
- ScriptableObject 配置属性与数据

## 技术栈

Unity 2D + URP | C# | Behavior Designer | Fungus | InputSystem | ScriptableObject

## 运行方式

```bash
git clone https://github.com/Chalars1011/2D-Action-Game.git
```

Unity Hub 打开项目文件夹，Unity 版本 2022.3 LTS 或更高。

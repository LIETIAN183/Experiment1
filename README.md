# LOG

#### 2021.3.26

- 初始化项目
- 初步搭建场景
- 读取时程数据进行简单的地震模拟

#### 2021.3.28

- 修改时程数据读取方式
  - 每个地震建立单独文件，自动读取三个 txt 文件数据进行仿真，无需手动参与其中读取步骤

#### 2021.3.29

- 添加 EqDataManger 控制地震的开始与结束，分离数据读取
- 代码优化
- 添加 .gitignore

#### 2021.3.30

- 修改 Manger 为单例模式
- 验证 ForceMode.Acceleration 运行结果通过 Test.cs，此函数最好放在 FixedUpdate() 中使用

#### 2021.4.2

- 添加 UI 界面控制
- 添加全局计数器显示于 UI 界面
- Add Force on Nonstructural Component
- 清理 git,删除已跟踪的文件，使得 .gitignore 生效

#### 2021.4.3

- 修正模型缩放旋转，修正模型质心位置，修正模型坐标关系
- 修改 EqManger 的 Acceleration 为 private set
- 通过 Event 解耦 Script 间的联系 Progress 20%

#### 2021.4.5

- 完成重构，解耦不同组件的关系
- 添加 Pause/Continue Button

#### 2021.4.7

- 添加 UI 分辨率适配
- 添加测试脚本/场景
- 添加计划

#### 2021.4.9

- 添加 FPSCounter
- 选择使用 AdvancedPeoplePack2 作为人物模型
- Modify UI interface

#### 2021.4.10

- 更换 UI

#### 2021.4.11 - 2021.4.18

- 了解 ComputeShader
- 了解 PhysX GPU 运行是否可行，Unity 上不可行，无法在 GPU 上进行物理计算
- 了解 Unity DOTS

#### 2021.4.19

- 切换为 DOTS 架构

#### 2021.4.21

- 切换进度 80%

### 2021.5.11

- 完成 DOTS 架构转换
- 实现地震模拟原型

#### 2021.5.25

- 初步完成物体摇晃效果

#### 2021.5.31

- 终于成功实现了 DOTS Animation

#### 2021.6.1

- 实现 Build 后 Animation 继续生效

#### 2021.6.2

- 重构代码，添加全局控制

#### 2021.6.9

- 添加通知提醒，UI 显示与隐藏

#### 2021.6.11

- 添加 Cinemachine，自由视角摄像机

#### 2021.9.8

- 完成货架晃动算法以及场景效果展示

#### 2021.9.9

- 修复晃动算法 Bug
- 同步模型渲染行为以及位置旋转同步修复
- 调整惯性力算法

#### 2021.9.14

- 调整惯性力
- 添加相机运动

#### 2021.9.15

- 修复数据读取 Bug

#### 2021.9.18

- 场景结果参数分析-完成 Detail 表
- Todo： 路径算法、代码重置场景、完成 Summary 表

#### 2021.9.24

- 确定摇晃主要方程，参数待定
- 摇晃加入主场景

#### 2021.10.3

- 开始路径算法部分实验

#### 2021.10.6

- FlowField v1.0
- FlowField v2.0

#### 2021.10.7

- Add Agent Movement

#### 2021.10.11

- Improve Flow Field Algorithm

#### 2021.10.12

- Agent Walk over the obstacles

#### 2021.10.14

- Walk step up modify
- Select Destination of Flow Field Algorithm update

#### 2021.10.25

- Code Optimize

#### 2021.10.27

- SFM v1.0

#### 2021.10.31

- SFM finished
- Add Agent Spawner
- Add ScreenShot

#### 2021.11.17

- Part2 Analysis

#### 2022.4.2

- Stage Storage

#### 2022.9.3

- 阶段性重构
  - 完成环境仿真系统
  - 完成流场算法
  - 完成人群疏散算法
  - 完成数据统计系统
  - 重写 UI
  - 解耦系统，提升系统间独立性

#### 2022.11.7

- 添加物体破碎效果

#### 2022.12.3

- 墙体破碎备份

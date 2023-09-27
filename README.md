# 部分仿真结果
https://pan.baidu.com/s/1SGL38wK82ULBrTOChONPTA?pwd=gczf

# 代码模块
* 路径：Experiment1/Assets/Scripts
* Agent 实现地震疏散过程中行人的控制逻辑，如行人踩踏地面障碍物并发生物理交互、状态改变以及相关动画状态调整
* Camera 实现摄像机的位置变化，方便观察
* FlowField 实现路径算法，指导行人疏散过程，该过程需要评估地面障碍物对行人的危险程度，以及不同位置逃离至出口的快慢
* HelperComponent 实现代码计算过程中的辅助数据类等
* NScomponents 实现室内非结构构件在地震力作用下的运动，如超市场景下商品的掉落、货架的振荡
* SimControl 实现整个系统的全局控制，如多轮仿真中的状态重置等
* Stateful 来自Unity官方案例项目，用于判断物体碰撞过程中是否其他物体持续接触，用于计算货架的振荡效果
* Statistics 实现统计模块，统计每轮仿真结果的相关数据
* SystemUpdateRate 控制其他不同模块的更新频率，控制程序运行效率
* Timer 控制单轮仿真的时间、读取地震数据，并根据时间进度更新当前地震加速度
* UI 实现UI界面，并实现了路径算法的可视化Debug
* Utilities 实现其他一些辅助功能，如对象池、常量、输入控制、给现有数据类型添加扩展方法等


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

#### 2021.5.11

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

#### 2022.12.17

- 升级 ECS 1.0，未优化

#### 2023.1.27

- Flow Field 重构部分完成

#### 2023.2.10

- Bug fixs, 重构进度 85%

#### 2023.2.11

- 获取场景中已有流体的位置
- 修改流场向量的计算方法

#### 2023.2.13

- 流场可视化 Debug
- 流场集成场部分修改
- 场景切换-未完成

#### 2023.3.16

- 流场算法改进对比

#### 2023.3.22

- 流场算法模型对比

#### 2023.4.12

- 送审前版本实验完成

#### 2023.4.17

- 添加 DOTS 声音第一版本，目前为 2D 声音

#### 2023.6.18

- Final version

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

### 2021.4.21

- 切换进度 80%

### 2021.5.11

- 完成 DOTS 架构转换
- 实现地震模拟原型

### 2021.5.25

- 初步完成物体摇晃效果

---

# Unity 插件

- ~~Hierarchy 2~~
- ~~Advanced People Pack 2~~
- POLYGON Starter Pack - Low Poly 3D Art by Synty
- Modern UI Pack
- Odin - Inspector and Serializer
- Edeitor Console Pro
- ~~Play Mode Saver~~
- Sensor Toolkit
- ~~Peek~~
- **Maybe** Easy Character Movement

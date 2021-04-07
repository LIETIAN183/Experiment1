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

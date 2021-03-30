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
- 修改 .gitignore，清理不必要上传的文件

#### 2021.3.30

- 修改 Manger 为单例模式
- 验证 ForceMode.Acceleration 运行结果通过 Test.cs，此函数最好放在 FixedUpdate() 中使用

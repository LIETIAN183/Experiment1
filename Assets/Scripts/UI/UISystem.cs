using System;
using Unity.Entities;
using Unity.Mathematics;
using InitialPrefabs.NimGui;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using InitialPrefabs.NimGui.Text;
using System.Text;
using Unity.Collections;

using Unity.Transforms;
using Unity.Physics;
using Unity.Jobs;
using System.Reflection;
using System.IO;
using Unity.Burst;

// string 为 managedData，同时 FixedString32Bytes.ToString()也为 managed method，且 Burst 不支持 String Format 0.0，因此使用 SystemBase
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UISystem : SystemBase
{
    private World managedWorld;
    private WorldUnmanaged unmanagedWorld;

    // 存储通知内容
    private string message;
    private float messageResetTimer;

    // 控制字体大小
    private ImTextStyle textStyle;
    private ImSliderStyle sliderStyle;
    private ImDropDownStyle dropStyle;
    private ImButtonStyle buttonStyle;

    private bool debugFlag;

    // 地震事件名数组, FlowField可视化类型数组,辅助字符串数组
    private string[] eventNameArray, ffVisTypeArray, fontSizeArray, timeStepArray, pgaThresholdArray, pgaStepArray, spawnNumberArray, sceneNameArray;

    protected override void OnCreate()
    {
        managedWorld = World.DefaultGameObjectInjectionWorld;
        unmanagedWorld = managedWorld.Unmanaged;

        EntityManager.AddComponentData<UIDisplayStateData>(this.SystemHandle, new UIDisplayStateData { isDisplay = true });
        EntityManager.AddComponentData<MessageEvent>(this.SystemHandle, new MessageEvent());

        // 设置样式
        textStyle = ImTextStyle.New();
        sliderStyle = ImSliderStyle.New();
        dropStyle = ImDropDownStyle.New();
        buttonStyle = ImButtonStyle.New();
        textStyle.WithColumn(HorizontalAlignment.Left);

        // 初始化通知属性
        message = " ";// 空格占位
        messageResetTimer = 0;

        eventNameArray = new string[] { };
        sceneNameArray = new string[] { };
        // 存储 FlowField 可视化类型数组
        ffVisTypeArray = Enum.GetNames(typeof(FlowFieldVisulizeType));
        fontSizeArray = Enumerable.Range(5, 16).Select(x => (x * 2).ToString()).ToArray();
        timeStepArray = Enumerable.Range(1, 6).Select(x => (x * 0.01f).ToString()).ToArray();
        pgaThresholdArray = Enumerable.Range(0, 11).Select(x => (x * 0.1f).ToString()).ToArray();
        pgaStepArray = Enumerable.Range(0, 11).Select(x => (x * 0.01f).ToString()).ToArray();
        spawnNumberArray = new string[] { "1", "10", "50", "100", "200", "300" };

        debugFlag = false;
    }

    protected override void OnUpdate()
    {
        // 给事件名称数组赋值，放在这里是因为数据读取需要时间，不能一开始就初始化好
        CheckArrayData();

        // 控制 UI 显示
        if (!SystemAPI.GetSingleton<UIDisplayStateData>().isDisplay) return;
        // 显示 FPS
        ImGui.Label($"{SystemAPI.GetSingleton<FPSData>().curFPS} FPS", in textStyle);

        // 显示通知
        ImGui.SameLine(); // 显示在同一行
        ProcessMessage();
        ImGui.Label(message, in textStyle.WithColor(Color.red));
        textStyle.WithColor(DefaultStyles.Text);// 重置白色

        // Debug 用
        // TODO:注意运行仿真后，下方同类的另一个选项生效，会覆盖该选项
        // SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = (FlowFieldVisulizeType)ImGui.Dropdown("Visulize Type", ffVisTypeArray, in dropStyle) });

        // 判断是否开始仿真
        if (unmanagedWorld.GetExistingSystemState<TimerSystem>().Enabled | managedWorld.GetExistingSystemManaged<MultiRoundStatisticsSystem>().Enabled)
        {
            // 开始仿真时显示事件相关信息
            ShowEventInformation();
            SelectFlowFieldVisulizeTypeIfSimulateFlowField();
        }
        else
        {
            // 未仿真时显示配置选项
            int eventIndex = ImGui.Dropdown("Select Seismic", eventNameArray, in dropStyle);

            var pgaThresholdListIndex = ImGui.Dropdown("PGA Threshold", 0, pgaThresholdArray, in dropStyle);
            if (!float.TryParse(pgaThresholdArray[pgaThresholdListIndex], out var pgaThreshold)) pgaThreshold = 0;

            var pgaStepListIndex = ImGui.Dropdown("PGA Step", 0, pgaStepArray, in dropStyle);
            if (!float.TryParse(pgaStepArray[pgaStepListIndex], out var pgaStep)) pgaStep = 0;

            // 已修改 BeginCollapsible 函数中的 size，使得 CollapsibleArea 占据的区域为屏幕 x 轴的 1/10
            using (var collapse = new ImCollapsibleArea("Config", true, in buttonStyle))
            {
                if (collapse.IsVisible)
                {
                    SelectScene();

                    SetupFontSize();
                    // 已修改 LineInternal 函数中的 size，使得 Line 占据的区域为屏幕 x 轴的 1/10
                    ImGui.Line();

                    SetupFixedTimeStep();
                    ImGui.Line();

                    SetupSimConfig();
                    ImGui.Line();
                }
                else
                {
                    SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = FlowFieldVisulizeType.None });
                    ImGui.SameLine();
                }
            }

            //开始单次仿真
            if (ImGui.Button("Single", in buttonStyle))
            {
                // 获得选择的地震 Index. 开始仿真
                SystemAPI.SetSingleton(new StartSeismicEvent
                {
                    isActivate = true,
                    index = eventIndex,
                    targetPGA = pgaThreshold
                });
            }
            ImGui.SameLine();
            //开始多轮统计
            if (ImGui.Button("MultiRound", in buttonStyle))
            {
                // 获得选择的地震 Index. 开始仿真
                var cameraRef = SystemAPI.ManagedAPI.GetSingleton<CameraRefData>();
                cameraRef.mainCamera.enabled = false;
                cameraRef.overHeadCamera.enabled = true;
                managedWorld.GetExistingSystemManaged<MultiRoundStatisticsSystem>().StartMultiRoundStatistics(pgaThreshold, pgaStep);
            }
        }

        var analysisCircledata = SystemAPI.GetSingleton<MultiRoundStatisticsData>();
        var setting = SystemAPI.GetSingleton<SimConfigData>();
        ImGui.Label("c_f:" + setting.average + $"|SimIter:" + setting.simIter, in textStyle);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugField();
        // ImGui.Label($"Acc Equals 0", in textStyle);

        // ImGui.Label($"simType=0: Basic SFM", in textStyle);
        // ImGui.Label($"simType=1: Baisc Earthquake SFM", in textStyle);
        // ImGui.Label($"simType=2: Our Model no reaction,familarity", in textStyle);
        // ImGui.Label($"simType=3: Oue Model", in textStyle);
#endif
        if (ImGui.Button("Switch Camera", in buttonStyle))
        {
            var cameraRef = SystemAPI.ManagedAPI.GetSingleton<CameraRefData>();
            cameraRef.mainCamera.enabled = !cameraRef.mainCamera.enabled;
            cameraRef.overHeadCamera.enabled = !cameraRef.overHeadCamera.enabled;
        }

        if (ImGui.Button("Change Camera Render", in buttonStyle))
        {
            var cameraRef = SystemAPI.ManagedAPI.GetSingleton<CameraRefData>();
            if (cameraRef.mainCamera.enabled)
            {
                cameraRef.mainCamera.cullingMask = ~cameraRef.mainCamera.cullingMask;
            }
            if (cameraRef.overHeadCamera.enabled)
            {
                cameraRef.overHeadCamera.cullingMask = ~cameraRef.overHeadCamera.cullingMask;
            }
        }
    }

    /// <summary>
    /// 显示当前地震事件相关属性
    /// </summary>
    private void ShowEventInformation()
    {
        var eventData = SystemAPI.GetSingleton<TimerData>();
        // 显示当前地震名与仿真时间
        // TODO: 时间显示有时存在跳动异常, 插件问题，等待插件后续更新
        ImGui.Label(eventData.seismicEventName.ToString() + $" | Time: {eventData.elapsedTime:0.0}s/{eventData.eventDuration:0.0}s", in textStyle);
        // 显示当前地震加速度
        ImGui.Label($"CurAcc: {math.length(eventData.curAcc):0.00}m/s2", in textStyle);
        // 显示PGA
        ImGui.Label($"PGA: {eventData.curPGA:0.00}g", in textStyle);
    }

    /// <summary>
    /// 设置 UI 的字体大小
    /// </summary>
    private void SetupFontSize()
    {
        // 设置字体大小
        var fontSizeListIndex = ImGui.Dropdown("Font Size", 4, fontSizeArray, in dropStyle);
        if (!ushort.TryParse(fontSizeArray[fontSizeListIndex], out var fontSize)) fontSize = 18;
        // 更新相关 UI 组件的字体大小
        textStyle.WithFontSize(fontSize);
        sliderStyle.WithFontSize(fontSize);
        dropStyle.WithFontSize(fontSize);
        buttonStyle.WithFontSize(fontSize);
    }

    /// <summary>
    /// 设置物理系统的仿真时间间隔
    /// </summary>
    private void SetupFixedTimeStep()
    {
        var timeIndex = ImGui.Dropdown("Simulation TimeStep", 3, timeStepArray, in dropStyle);
        // 检查是否解析成功
        if (float.TryParse(timeStepArray[timeIndex], out var simDeltaTime))
        {
            // 检查是否超出范围
            simDeltaTime = simDeltaTime.inRange(0.01f, 0.06f, 0.04f);
            // 更新系统 Fixed TimeStep
            managedWorld.GetExistingSystemManaged<FixedStepSimulationSystemGroup>().Timestep = simDeltaTime;
            // 同步更新 TimerData
            var timerData = SystemAPI.GetSingleton<TimerData>();
            timerData.simDeltaTime = simDeltaTime;
            SystemAPI.SetSingleton(timerData);
        }
    }

    /// <summary>
    /// 设置仿真层次选项
    /// </summary>
    private void SetupSimConfig()
    {
        var simConfigData = SystemAPI.GetSingleton<SimConfigData>();
        // 配置环境参数
        if (simConfigData.simEnvironment = ImGui.Toggle("Simulate Environment", in buttonStyle, simConfigData.simEnvironment))
        {
            simConfigData.itemDestructible = ImGui.Toggle("Item Breakable", in buttonStyle, simConfigData.itemDestructible);
            ImGui.Line();
        }

        // 配置流场
        if (simConfigData.simFlowField = ImGui.Toggle("Simulate FlowField", in buttonStyle, simConfigData.simFlowField))
        {
            // TODO: 后续继续调整
            // 选择流场目标点
            if (ImGui.Toggle("Choose Destination", in buttonStyle))
            {
                SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = FlowFieldVisulizeType.GlobalFlowField });
                unmanagedWorld.GetExistingSystemState<SelectDestinationSystem>().Enabled = true;
            }
            else
            {
                unmanagedWorld.GetExistingSystemState<SelectDestinationSystem>().Enabled = false;
            }

            if (ImGui.Toggle("Modify Display Height", in buttonStyle))
            {
                SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = FlowFieldVisulizeType.GlobalFlowField });
                // 已修改 Slider 函数中的 size，使得 Slider 占据的区域为屏幕 x 轴的 1/10
                // 不添加空label，该slider不生效
                var heightOffset = ImGui.Slider("", -1f, 4f, in sliderStyle, 0.2f);
                var flowFieldData = SystemAPI.GetSingleton<FlowFieldSettingData>();
                flowFieldData.displayOffset.y = heightOffset;
                SystemAPI.SetSingleton(flowFieldData);
            }
            ImGui.Line();
        }

        // 配置人群参数
        if (simConfigData.simAgent = ImGui.Toggle("Simulate Agent", in buttonStyle, simConfigData.simAgent))
        {
            var spawnNumberListIndex = ImGui.Dropdown("Spawn Number", 2, spawnNumberArray, in dropStyle);
            var spawnerData = SystemAPI.GetSingleton<SpawnerData>();
            if (!int.TryParse(spawnNumberArray[spawnNumberListIndex], out spawnerData.desireCount)) spawnerData.desireCount = 50;
            SystemAPI.SetSingleton(spawnerData);

            simConfigData.displayTrajectories = ImGui.Toggle("Display Trajectory", in buttonStyle, simConfigData.displayTrajectories);
            ImGui.Line();
        }

        // 配属统计系统
        simConfigData.performStatistics = ImGui.Toggle("Perform Statistics", in buttonStyle, simConfigData.performStatistics);

        SystemAPI.SetSingleton(simConfigData);
    }

    private void SelectScene()
    {
        var index = ImGui.Dropdown("Select Scene", 0, sceneNameArray, in dropStyle);
        if (index != managedWorld.GetExistingSystemManaged<SimInitializeSystem>().curSceneIndex)
        {
            managedWorld.GetExistingSystemManaged<SimInitializeSystem>().ChangeScene(index);
        }
    }

    private void CheckArrayData()
    {
        if (eventNameArray.Length <= 0 && SystemAPI.GetSingleton<DataLoadStateData>().isLoadSuccessed)
        {
            eventNameArray = SystemAPI.GetSingletonBuffer<BlobRefBuffer>(true).GetNameArry();
        }
        if (sceneNameArray.Length <= 0 && managedWorld.GetExistingSystemManaged<SimInitializeSystem>().GetSceneNameArray(out var array))
        {
            sceneNameArray = array;
        }
    }

    private void ProcessMessage()
    {
        var messageEvent = SystemAPI.GetSingleton<MessageEvent>();
        if (messageEvent.isActivate)
        {
            this.message = messageEvent.message.ToString();
            messageEvent.isActivate = false;
            if (!messageEvent.displayForever)
            {
                messageResetTimer = 2;
            }
            SystemAPI.SetSingleton(messageEvent);
        }

        // 显示 2s 后清除信息
        if (messageResetTimer > 0)
        {
            messageResetTimer -= SystemAPI.Time.DeltaTime;
            if (messageResetTimer <= 0)
            {
                message = " ";
            }
        }
    }

    private void SelectFlowFieldVisulizeTypeIfSimulateFlowField()
    {
        if (SystemAPI.GetSingleton<SimConfigData>().simFlowField)
        {
            // 选择 FlowField 可视化效果
            SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = (FlowFieldVisulizeType)ImGui.Dropdown("FlowField", ffVisTypeArray, in dropStyle) });
        }
    }

    private void DebugField()
    {
        using (var collapse = new ImCollapsibleArea(debugFlag ? "Close Debug" : "Enter Debug", true, in buttonStyle))
        {
            if (debugFlag = collapse.IsVisible)
            {
                // if (ImGui.Toggle("Simulate FlowField", in buttonStyle))
                // {
                CellDebug();
                // }
            }
            else
            {
                unmanagedWorld.GetExistingSystemState<CellDebugSystem>().Enabled = debugFlag;
            }
        }
    }

    private void CellDebug()
    {
        SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = (FlowFieldVisulizeType)ImGui.Dropdown("Visulize Type", ffVisTypeArray, in dropStyle) });
        unmanagedWorld.GetExistingSystemState<CellDebugSystem>().Enabled = debugFlag;
        var handle = unmanagedWorld.GetExistingUnmanagedSystem<CellDebugSystem>();
        var curDebugCell = unmanagedWorld.GetUnsafeSystemRef<CellDebugSystem>(handle).curDebugCell;

        var structFields = typeof(CellData).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var dataFields = curDebugCell.GetType().GetFields();

        textStyle.WithColor(Color.red);// 设置红色
        ImGui.Label("FlatIndex:" + FlowFieldUtility.ToFlatIndex(curDebugCell.gridIndex, SystemAPI.GetSingleton<FlowFieldSettingData>().gridSetSize.y), in textStyle);
        for (int i = 0; i < structFields.Length; ++i)
        {
            ImGui.Label(structFields[i].Name + ":" + dataFields[i].GetValue(curDebugCell).ToString(), in textStyle);
        }
        textStyle.WithColor(DefaultStyles.Text);// 重置白色
    }
    private void CleanCellDebug()
    {
        SystemAPI.SetSingleton<FFVisTypeStateData>(new FFVisTypeStateData { ffVisType = FlowFieldVisulizeType.None });
    }
}
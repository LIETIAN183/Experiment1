using System;
using Unity.Entities;
using Unity.Mathematics;
using InitialPrefabs.NimGui;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using InitialPrefabs.NimGui.Text;
using System.Text;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class UISystem : SystemBase
{
    World simulation;
    // UI 字体大小
    ushort fontSize;
    // 存储地震事件列表, FlowField可视化类型
    string[] seismicEvents, flowFieldDisplayType;
    // 存储通知内容
    string notification;

    // 控制是否应该显示 UI
    public bool displayUI, showConfiguration;

    ImTextStyle textStyle;
    ImSliderStyle sliderStyle;
    ImDropDownStyle dropStyle;
    ImButtonStyle buttonStyle;


    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
        // 存储 FlowField 可视化类型列表
        flowFieldDisplayType = Enum.GetNames(typeof(FlowFieldDisplayType));

        // 设置样式
        textStyle = ImTextStyle.New();
        sliderStyle = ImSliderStyle.New();
        dropStyle = ImDropDownStyle.New();
        buttonStyle = ImButtonStyle.New();
        textStyle.WithColumn(HorizontalAlignment.Left);

        // 初始化通知属性
        notification = " ";// 空格占位

        displayUI = true;
        showConfiguration = false;
    }

    protected override void OnStartRunning()
    {
        // 存储地震事件列表
        if (simulation.GetExistingSystemManaged<SetupBlobSystem>().dataReadSuccessed)
        {
            seismicEvents = SetupBlobSystem.seismicBlobRefs.Select(item => item.Value.seismicName.ToString()).ToArray();
        }
        else
        {
            seismicEvents = new string[] { " " };
        }
    }

    protected override void OnUpdate()
    {
        // 控制左上角 UI 显示
        if (!displayUI) return;
        // 显示 FPS
        ImGui.Label($"{simulation.GetExistingSystemManaged<FPSSystem>().curFPS} FPS", in textStyle);


        // 显示通知
        ImGui.SameLine(); // 显示在同一行
        ImGui.Label(notification, in textStyle.WithColor(Color.red));

        // 显示地震事件名字
        var data = GetSingleton<AccTimerData>();
        // 显示当前地震名与仿真时间
        // TODO: 时间显示有时存在跳动异常
        ImGui.Label((string.IsNullOrEmpty(data.seismicName.ToString()) ? "Seismic Name" : data.seismicName.ToString()) + $"|Time: {data.elapsedTime:0.0}s/{data.seismicFinishTime:0.0}s", in textStyle.WithColor(DefaultStyles.Text));
        // 显示当前地震加速度
        ImGui.Label($"CurAcc:{math.length(data.acc):0.00}m/s2", in textStyle);

        // 显示PGA
        ImGui.Label($"PGA: {data.curPGA:0.00}g", in textStyle);

        // DisplayNotificationForever(simulation.GetExistingSystemManaged<MultiRoundStatisticsSystem>().Enabled.ToString());
        if (simulation.GetExistingSystemManaged<AccTimerSystem>().Enabled | simulation.GetExistingSystemManaged<MultiRoundStatisticsSystem>().Enabled)
        {
            showConfiguration = false;
            var simulationSetting = GetSingleton<SimulationLayerConfigurationData>();
            if (simulationSetting.isSimulateFlowField)
            {
                // 选择 FlowField 可视化效果
                simulation.GetExistingSystemManaged<FlowFieldVisulizeSystem>()._curDisplayType = (FlowFieldDisplayType)ImGui.Dropdown("FlowField", flowFieldDisplayType, in dropStyle);
            }
        }
        else
        {
            if (ImGui.Button(showConfiguration ? "Hide Configuration" : "Configuration", in buttonStyle))
            {
                showConfiguration = !showConfiguration;
            }

            if (showConfiguration)
            {
                var simulationSetting = GetSingleton<SimulationLayerConfigurationData>();
                // 配置环境参数
                if (simulationSetting.isSimulateEnvironment = ImGui.Toggle("Simulate Environment", in buttonStyle, true))
                {
                    simulationSetting.isItemBreakable = ImGui.Toggle("Item Breakable", in buttonStyle, true);
                    var accTimerData = GetSingleton<AccTimerData>();
                    var envList = Enumerable.Range(1, 6).Select(x => (x * 0.5f).ToString()).ToArray();
                    var envIndex = ImGui.Dropdown("Env Enhance Factor", 1, envList, in dropStyle);
                    if (!float.TryParse(envList[envIndex], out accTimerData.envEnhanceFactor)) accTimerData.envEnhanceFactor = 1;

                    var timeList = Enumerable.Range(1, 6).Select(x => (x * 0.01f).ToString()).ToArray();
                    var timeIndex = ImGui.Dropdown("Simulation TimeStep", 3, timeList, in dropStyle);
                    if (!float.TryParse(timeList[timeIndex], out accTimerData.simulationDeltaTime)) accTimerData.simulationDeltaTime = 0.04f;
                    SetSingleton(accTimerData);
                }
                ImGui.SkipLine();
                // 配置流场
                if (simulationSetting.isSimulateFlowField = ImGui.Toggle("Simulate FlowField", in buttonStyle, true))
                {
                    // 选择 FlowField 可视化效果
                    simulation.GetExistingSystemManaged<FlowFieldVisulizeSystem>()._curDisplayType = (FlowFieldDisplayType)ImGui.Dropdown("Visulize Type", flowFieldDisplayType, in dropStyle);
                    simulation.GetExistingSystemManaged<CalculateCostFieldSystem>().Enabled = true;
                    // 选择流场目标点
                    if (ImGui.Toggle("Choose Destination", in buttonStyle))
                    {
                        simulation.GetExistingSystemManaged<FlowFieldVisulizeSystem>()._curDisplayType = FlowFieldDisplayType.FlowField;
                        simulation.GetExistingSystemManaged<SelectDestinationSystem>().Enabled = true;
                    }
                    else
                    {
                        simulation.GetExistingSystemManaged<SelectDestinationSystem>().Enabled = false;
                    }

                    if (ImGui.Toggle("Modify Display Height", in buttonStyle))
                    {
                        if (simulation.GetExistingSystemManaged<FlowFieldVisulizeSystem>()._curDisplayType.Equals(FlowFieldDisplayType.None))
                        {
                            simulation.GetExistingSystemManaged<FlowFieldVisulizeSystem>()._curDisplayType = FlowFieldDisplayType.IntegrationHeatMap;
                        }
                        var heightOffset = ImGui.Slider("", -1f, 4f, in sliderStyle, 0.2f);// 不添加空label，该slider不生效
                        var flowFieldData = GetSingleton<FlowFieldSettingData>();
                        flowFieldData.displayHeightOffset.y = heightOffset;
                        SetSingleton(flowFieldData);
                    }
                }
                ImGui.SkipLine();
                if (simulationSetting.isSimulateAgent = ImGui.Toggle("Simulate Agent", in buttonStyle, true))
                {
                    var numberlist = new string[] { "1", "10", "50", "100", "200", "300" };
                    var numberIndex = ImGui.Dropdown("Spawn Number", 2, numberlist, in dropStyle);
                    var spawnerData = GetSingleton<SpawnerData>();
                    if (!int.TryParse(numberlist[numberIndex], out spawnerData.desireCount)) spawnerData.desireCount = 50;
                    if (!spawnerData.canSpawn)
                    {
                        if (ImGui.Button("Spawn Agents", in buttonStyle))
                        {
                            spawnerData.canSpawn = true;
                        }
                    }
                    SetSingleton(spawnerData);
                    simulationSetting.isDisplayTrajectories = ImGui.Toggle("Display Trajectory", in buttonStyle, true);
                }
                else
                {
                    simulationSetting.isDisplayTrajectories = false;
                }
                ImGui.SkipLine();
                simulationSetting.isPerformStatistics = ImGui.Toggle("Perform Statistics", in buttonStyle, true);
                SetSingleton(simulationSetting);

                ImGui.SkipLine();
            }

            // 选择单次仿真
            int index = ImGui.Dropdown("Select Seismic", seismicEvents, in dropStyle);
            float targetPGA = 0;
            var pgaList = Enumerable.Range(0, 11).Select(x => (x * 0.1f).ToString()).ToArray();
            var pgaIndex = ImGui.Dropdown("Target PGA", 0, pgaList, in dropStyle);
            if (!float.TryParse(pgaList[pgaIndex], out targetPGA)) targetPGA = 0;
            //开始仿真
            if (ImGui.Button("Start Single Simulation", in buttonStyle))
            {
                // 获得选择的地震 Index. 开始仿真
                simulation.GetExistingSystemManaged<AccTimerSystem>().StartSingleSimulation(index, targetPGA);
            }

            // 选择多轮统计
            float pgaThreshold = 0, pgaStep = 0;
            var thresholdList = Enumerable.Range(0, 11).Select(x => (x * 0.1f).ToString()).ToArray();
            var thresholdIndex = ImGui.Dropdown("PGA Threshold", 0, thresholdList, in dropStyle);
            if (!float.TryParse(thresholdList[thresholdIndex], out pgaThreshold)) pgaThreshold = 0;

            var stepList = Enumerable.Range(0, 6).Select(x => (x * 0.01f).ToString()).ToArray();
            var stepIndex = ImGui.Dropdown("PGA Step", 0, stepList, in dropStyle);
            if (!float.TryParse(stepList[stepIndex], out pgaStep)) pgaStep = 0;
            //开始多轮统计
            if (ImGui.Button("Start MultiRound Simulation", in buttonStyle))
            {
                // 获得选择的地震 Index. 开始仿真
                simulation.GetExistingSystemManaged<MultiRoundStatisticsSystem>().StartMultiRoundStatistics(pgaThreshold, pgaStep);
            }

            // 调整字体大小
            var fontList = Enumerable.Range(5, 16).Select(x => (x * 2).ToString()).ToArray();
            var fontIndex = ImGui.Dropdown("Font Size", 4, fontList, in dropStyle);
            if (!ushort.TryParse(fontList[fontIndex], out fontSize)) fontSize = 18;
            // 更新显示字体
            textStyle.WithFontSize(fontSize);
            sliderStyle.WithFontSize(fontSize);
            dropStyle.WithFontSize(fontSize);
            buttonStyle.WithFontSize(fontSize);
        }

        // 隐藏 UI 按钮
        if (ImGui.Button("Hide UI", in buttonStyle)) { displayUI = false; }
        ImGui.SameLine();

        // 暂停按钮
        if (ImGui.Button(UnityEngine.Time.timeScale == 0 ? "Continue" : "Pause", in buttonStyle))
        {
            UnityEngine.Time.timeScale = UnityEngine.Time.timeScale == 0 ? 1 : 0;
        }
        ImGui.SameLine();
        // 退出按钮
        if (ImGui.Button("Exit", in buttonStyle)) { System.Diagnostics.Process.GetCurrentProcess().Kill(); }
    }

    public void DisplayNotification2s(string notification)
    {
        this.notification = notification;
        // 2s后结束显示
        Task.Delay(2000).ContinueWith(delegate
        {
            this.notification = " ";
        });
    }

    public void DisplayNotificationForever(string notification)
    {
        this.notification = notification;
    }
}
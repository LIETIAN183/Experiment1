using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: 多分辨率适配
public class UIControl : MonoBehaviour
{
    public static UIControl Instance { get; private set; }
    public Dropdown selectEq;
    public Button startBtn;
    public Button stopBtn;
    public Button restartBtn;

    public Slider progress;
    // Start is called before the first frame update
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// use 'Awake' to configure the self. And 'Start' to communicate between objects
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Init();
    }

    // 初始化参数
    void Init()
    {
        // 关联变量
        selectEq = GetComponentInChildren<Dropdown>();
        // TODO: 性能优化，此查找方式可能太费性能
        startBtn = GameObject.Find("StartBtn").GetComponent<Button>();
        stopBtn = GameObject.Find("StopBtn").GetComponent<Button>();
        restartBtn = GameObject.Find("RestartBtn").GetComponent<Button>();
        progress = GetComponentInChildren<Slider>();
    }
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        // 关联 UI 和 EqManger
        selectEq.options = EqDataReader.EarthquakeFolders(Application.dataPath + "/Data/").Select(f => new Dropdown.OptionData(f)).ToList();//获取可选的地震，转换 IEnumerable<string> 为 List<Dropdown.OptionData>，显示在下拉框中
        selectEq.onValueChanged.AddListener(index => EqManger.Instance.earthquakes = selectEq.options[index].text);// 下拉框选择后，赋值 earthquakes
        startBtn.onClick.AddListener(EqManger.Instance.startEq);// 关联地震开始按钮
        startBtn.onClick.AddListener(() => progress.maxValue = EqManger.Instance.getTime());// 设置 Slider 最大值
        stopBtn.onClick.AddListener(EqManger.Instance.stopEq);// 关联地震结束按钮
        restartBtn.onClick.AddListener(EqManger.Instance.restart);// 关联重置场景按钮
        Counter.Instance.onValueChanged.AddListener(UpdateProgress);// 更新 Slider 滑动条进度
    }
    public void UpdateProgress(int i)
    {
        progress.value = i;
    }
}

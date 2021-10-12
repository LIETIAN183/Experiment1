using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using System.Linq;

public enum FlowFieldDisplayType { None, CostField, IntegrationField, FlowField, CostHeatMap, IntegrationHeatMap };

public class GridDebug : MonoBehaviour
{
    public static GridDebug instance;

    [SerializeField] private FlowFieldDisplayType _curDisplayType;

    public FlowFieldSettingData debugFlowFieldSetting { set; get; }

    private List<CellData> _gridCellData;

    // Direction Display
    [SerializeField] private Transform directionDisplayParent;
    private List<GameObject> _directionDisplay;

    public float3 drawOffset;
    private Sprite[] ffIcons;

    private void Awake()
    {
        instance = this;
        _gridCellData = new List<CellData>();
        _directionDisplay = new List<GameObject>();
    }

    private void Start()
    {
        ffIcons = Resources.LoadAll<Sprite>("Sprites/FFicons");
    }

    private void OnDrawGizmos()
    {
        if (_gridCellData == null || _gridCellData.Count == 0) { return; }

        // Direction Display
        while (_directionDisplay.Count < _gridCellData.Count)
        {
            GameObject iconGO = new GameObject();
            iconGO.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            iconGO.AddComponent<SpriteRenderer>();
            iconGO.transform.parent = directionDisplayParent;
            _directionDisplay.Add(iconGO);
        }

        directionDisplayParent.gameObject.SetActive(false);
        GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        Vector3 cubeSize = debugFlowFieldSetting.cellRadius * 2;
        cubeSize.y = 0f;
        switch (_curDisplayType)
        {
#if UNITY_EDITOR
            case FlowFieldDisplayType.CostField:
                DrawGridOnRuntime();

                foreach (CellData curCell in _gridCellData)
                {
                    Handles.Label(curCell.worldPos + drawOffset, curCell.cost.ToString(), style);
                }
                break;

            case FlowFieldDisplayType.IntegrationField:
                DrawGridOnRuntime();

                foreach (CellData curCell in _gridCellData)
                {
                    Handles.Label(curCell.worldPos + drawOffset, curCell.bestCost.ToString(), style);
                }
                break;

            case FlowFieldDisplayType.CostHeatMap:
                //https://stackoverflow.com/questions/10901085/range-values-to-pseudocolor

                float maxCost = _gridCellData.Where(i => i.cost != 255).Max(i => i.cost);

                foreach (CellData curCell in _gridCellData)
                {
                    float costHeat = (maxCost - curCell.cost) / maxCost;
                    Gizmos.color = Color.HSVToRGB(costHeat / 3, 1, 1);
                    if (curCell.cost == 255) Gizmos.color = Color.black;
                    Gizmos.DrawCube(curCell.worldPos + drawOffset, cubeSize);
                }
                break;

            case FlowFieldDisplayType.IntegrationHeatMap:

                float maxBestCost = _gridCellData.Where(i => i.bestCost != ushort.MaxValue).Max(i => i.bestCost);

                foreach (CellData curCell in _gridCellData)
                {
                    var intHeat = (maxBestCost - curCell.bestCost) / maxBestCost;
                    Gizmos.color = Color.HSVToRGB(intHeat / 3, 1, 1);
                    if (curCell.bestCost == ushort.MaxValue) Gizmos.color = Color.black;
                    Gizmos.DrawCube(curCell.worldPos + drawOffset, cubeSize);
                }
                break;
#endif
            case FlowFieldDisplayType.FlowField:
                directionDisplayParent.gameObject.SetActive(true);

                for (int i = 0; i < _gridCellData.Count; i++)
                {
                    DisplayDiretion(_gridCellData[i], i);
                }
                break;

            default:
                break;
        }
    }

    private void DrawGridOnRuntime()
    {
        Gizmos.color = Color.green;
        Vector3 size = Vector3.one * debugFlowFieldSetting.cellRadius * 2;
        foreach (var cell in _gridCellData)
        {
            Gizmos.DrawWireCube(cell.worldPos + drawOffset, size);
        }
    }

    public void ClearList() => _gridCellData.Clear();

    public void AddToList(CellData cellToAdd) => _gridCellData.Add(cellToAdd);

    private void DisplayDiretion(CellData cell, int index)
    {
        var iconGO = _directionDisplay[index];
        var iconSR = iconGO.GetComponent<SpriteRenderer>();
        iconGO.transform.position = cell.worldPos + drawOffset;

        Quaternion newRot;
        // 目标点
        if (cell.cost == 0)
        {
            iconSR.sprite = ffIcons[3];
            newRot = Quaternion.Euler(90, 0, 0);
        }
        // 不可行点
        else if (cell.bestDirection.Equals(GridDirection.None))
        {
            iconSR.sprite = ffIcons[2];
            newRot = Quaternion.Euler(90, 0, 0);
        }
        else
        {
            iconSR.sprite = ffIcons[0];
            newRot = Quaternion.Euler(90, 90 - math.degrees(math.atan2(cell.bestDirection.y, cell.bestDirection.x)), 0);
        }

        iconGO.transform.rotation = newRot;
    }
}

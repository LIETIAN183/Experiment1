using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

public enum FlowFieldDisplayType { None, CostField, IntegrationField, FlowField, CostHeatMap, IntegrationHeatMap };

public class GridDebug : MonoBehaviour
{
    public static GridDebug instance;

    [SerializeField] private FlowFieldDisplayType _curDisplayType;
    [SerializeField] private bool _displayGrid;

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
#if UNITY_EDITOR
        if (_displayGrid)
        {
            DrawGridOnRuntime();
        }
#endif
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
        switch (_curDisplayType)
        {
#if UNITY_EDITOR
            case FlowFieldDisplayType.CostField:

                foreach (CellData curCell in _gridCellData)
                {
                    Handles.Label(curCell.worldPos + drawOffset, curCell.cost.ToString(), style);
                }
                break;

            case FlowFieldDisplayType.IntegrationField:

                foreach (CellData curCell in _gridCellData)
                {
                    Handles.Label(curCell.worldPos + drawOffset, curCell.bestCost.ToString(), style);
                }
                break;

            // TODO: 颜色变化太少
            case FlowFieldDisplayType.CostHeatMap:

                foreach (CellData curCell in _gridCellData)
                {
                    float costHeat = curCell.cost / 255f;
                    Gizmos.color = new Color(0, 0, 1 - costHeat);
                    Gizmos.DrawCube(curCell.worldPos + drawOffset, cubeSize);
                }
                break;

            case FlowFieldDisplayType.IntegrationHeatMap:
                foreach (CellData curCell in _gridCellData)
                {
                    var intHeat = curCell.bestCost / (float)ushort.MaxValue;
                    Gizmos.color = new Color(0, 0, 1 - intHeat);
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

        // 目标点
        if (cell.cost == 0)
        {
            iconSR.sprite = ffIcons[3];
            Quaternion newRot = Quaternion.Euler(90, 0, 0);
            iconGO.transform.rotation = newRot;
            return;
        }
        // else if (cell.cost == byte.MaxValue)
        else if (cell.bestDirection.Equals(GridDirection.None))
        {
            iconSR.sprite = ffIcons[2];
            Quaternion newRot = Quaternion.Euler(90, 0, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.North))
        {
            iconSR.sprite = ffIcons[0];
            Quaternion newRot = Quaternion.Euler(90, 0, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.South))
        {
            iconSR.sprite = ffIcons[0];
            Quaternion newRot = Quaternion.Euler(90, 180, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.East))
        {
            iconSR.sprite = ffIcons[0];
            Quaternion newRot = Quaternion.Euler(90, 90, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.West))
        {
            iconSR.sprite = ffIcons[0];
            Quaternion newRot = Quaternion.Euler(90, 270, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.NorthEast))
        {
            iconSR.sprite = ffIcons[1];
            Quaternion newRot = Quaternion.Euler(90, 0, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.NorthWest))
        {
            iconSR.sprite = ffIcons[1];
            Quaternion newRot = Quaternion.Euler(90, 270, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.SouthEast))
        {
            iconSR.sprite = ffIcons[1];
            Quaternion newRot = Quaternion.Euler(90, 90, 0);
            iconGO.transform.rotation = newRot;
        }
        else if (cell.bestDirection.Equals(GridDirection.SouthWest))
        {
            iconSR.sprite = ffIcons[1];
            Quaternion newRot = Quaternion.Euler(90, 180, 0);
            iconGO.transform.rotation = newRot;
        }
        else
        {
            iconSR.sprite = ffIcons[2];
            Quaternion newRot = Quaternion.Euler(90, 0, 0);
            iconGO.transform.rotation = newRot;
        }
    }
}

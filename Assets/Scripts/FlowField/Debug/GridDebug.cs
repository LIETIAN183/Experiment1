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

    private FlowFieldSettingData _flowFieldData;
    public FlowFieldSettingData FlowFieldData
    {
        get => _flowFieldData;
        set => _flowFieldData = value;
    }

    private List<CellData> _gridCellData;

    private List<GameObject> _directionDisplay;

    private Vector2Int _gridSize;
    private float _cellRadius;

    public float3 drawOffset;
    private Sprite[] ffIcons;
    private Vector3 displayScale = new Vector3(0.25f, 0.25f, 0.25f);

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
        if (_displayGrid)
        {
            _gridSize = new Vector2Int { x = _flowFieldData.gridSize.x, y = _flowFieldData.gridSize.y };
            _cellRadius = _flowFieldData.cellRadius;

            DrawGrid(_gridSize, (_gridCellData == null || _gridCellData.Count == 0) ? Color.yellow : Color.green, _cellRadius);
        }

        if (_gridCellData == null || _gridCellData.Count == 0) { return; }

        while (_directionDisplay.Count < _gridCellData.Count)
        {
            GameObject iconGO = new GameObject();
            iconGO.transform.localScale = displayScale;
            iconGO.AddComponent<SpriteRenderer>();
            iconGO.transform.parent = transform;
            _directionDisplay.Add(iconGO);
        }

        GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

        switch (_curDisplayType)
        {
            case FlowFieldDisplayType.CostField:

                foreach (CellData curCell in _gridCellData)
                {
                    Handles.Label(curCell.worldPos, curCell.cost.ToString(), style);
                }
                break;

            case FlowFieldDisplayType.IntegrationField:

                foreach (CellData curCell in _gridCellData)
                {
                    Handles.Label(curCell.worldPos, curCell.bestCost.ToString(), style);
                }
                break;

            case FlowFieldDisplayType.CostHeatMap:
                foreach (CellData curCell in _gridCellData)
                {
                    float costHeat = curCell.cost / 255f;
                    Gizmos.color = new Color(costHeat, costHeat, costHeat);
                    Vector3 center = new Vector3(_cellRadius * 2 * curCell.gridIndex.x + _cellRadius, 0, _cellRadius * 2 * curCell.gridIndex.y + _cellRadius);
                    Vector3 size = Vector3.one * _cellRadius * 2;
                    Gizmos.DrawCube(center, size);
                }
                break;

            case FlowFieldDisplayType.FlowField:
                foreach (CellData curCell in _gridCellData)
                {
                    // Handles.Label(curCell.worldPos, curCell.bestDirection.ToString(), style);
                    DisplayDiretion(curCell, FlowFieldHelper.ToFlatIndex(curCell.gridIndex, _gridSize.y));
                }
                break;

            case FlowFieldDisplayType.None:
                break;

            default:
                Debug.LogWarning("Warning: Invalid Grid Debug Display Type", gameObject);
                break;
        }
    }

    private static void DrawGrid(Vector2Int drawGridSize, Color drawColor, float drawCellRadius)
    {
        Gizmos.color = drawColor;
        for (int x = 0; x < drawGridSize.x; x++)
        {
            for (int y = 0; y < drawGridSize.y; y++)
            {
                Vector3 center = new Vector3(drawCellRadius * 2 * x + drawCellRadius, 0, drawCellRadius * 2 * y + drawCellRadius);
                Vector3 size = Vector3.one * drawCellRadius * 2;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }

    public void ClearList() => _gridCellData.Clear();

    public void AddToList(CellData cellToAdd) => _gridCellData.Add(cellToAdd);

    private void DisplayDiretion(CellData cell, int flatIndex)
    {
        var iconGO = _directionDisplay[flatIndex];
        var iconSR = iconGO.GetComponent<SpriteRenderer>();
        iconGO.transform.position = cell.worldPos + drawOffset;

        // 目标点
        if (cell.cost == 0)
        {
            iconSR.sprite = ffIcons[3];
            Quaternion newRot = Quaternion.Euler(90, 0, 0);
            iconGO.transform.rotation = newRot;
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

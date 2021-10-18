using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class LineDebug : MonoBehaviour
{
    public static LineDebug instance;

    public LineRenderer line;

    public List<Vector3> positions;
    private void Awake()
    {
        instance = this;
        positions = new List<Vector3>();
    }

    public void addPosition(Vector3 position) => positions.Add(position);
    // {

    // }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // TODO: 一开始 foreach没有找到 instance，需要修复
        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray());
    }
}

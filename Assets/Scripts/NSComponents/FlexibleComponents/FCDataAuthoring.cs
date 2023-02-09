using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(FCKCInitAuthoring))]
public class FCDataAuthoring : MonoBehaviour
{
    public float length;

    public float k, c, mass;

    public bool directionConstrain;

    class Baker : Baker<FCDataAuthoring>
    {
        public override void Bake(FCDataAuthoring authoring)
        {
            AddComponent(new FCData
            {
                length = authoring.length,
                k = authoring.k,
                c = authoring.c,
                mass = authoring.mass,
                directionConstrain = authoring.directionConstrain
            });
        }
    }
}

public struct FCData : IComponentData
{
    public float length;

    public float topDis, topVel, topAcc;

    public float k, c, mass;

    public bool directionConstrain;

    public float3 forward;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RandomSeedAuthoring : MonoBehaviour
{
    class Baker : Baker<RandomSeedAuthoring>
    {
        public override void Bake(RandomSeedAuthoring authoring)
        {
            AddComponent<RandomSeed>(new RandomSeed { seed = (uint)System.DateTime.Now.Ticks });
        }
    }
}

public struct RandomSeed : IComponentData
{
    public uint seed;
}
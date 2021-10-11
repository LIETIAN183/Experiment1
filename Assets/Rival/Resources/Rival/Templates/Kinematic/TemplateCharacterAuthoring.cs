using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Rival;
using Unity.Physics;

namespace Rival.Templates.Kinematic //CODEGEN(Namespace)
{ //CODEGEN(NamespaceOpen)
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicsShapeAuthoring))]
    [UpdateAfter(typeof(EndColliderConversionSystem))]
    public class TemplateCharacterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public AuthoringKinematicCharacterBody CharacterBody = AuthoringKinematicCharacterBody.GetDefault();
        public TemplateCharacterComponent TemplateCharacter = TemplateCharacterComponent.GetDefault();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            KinematicCharacterUtilities.HandleConversionForCharacter(dstManager, entity, gameObject, CharacterBody);

            dstManager.AddComponentData(entity, TemplateCharacter);
            dstManager.AddComponentData(entity, new TemplateCharacterInputs());
        }
    }
} //CODEGEN(NamespaceClose)
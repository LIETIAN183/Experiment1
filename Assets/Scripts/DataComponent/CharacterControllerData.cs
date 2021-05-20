using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct CharacterControllerData : IComponentData
{
    // -------------------------------------------------------------------------------------
    // Current Movement
    // -------------------------------------------------------------------------------------

    /// <summary>
    /// The current direction that the character is moving.
    /// </summary>
    public float3 currentDirection;

    /// <summary>
    /// The current magnitude of the character movement.
    /// If <c>0.0</c>, then the character is not being directly moved by the controller but residual forces may still be active.
    /// 速度倍数
    /// </summary>
    public float currentMagnitude;

    /// <summary>
    /// Is the character requesting to jump?
    /// Used in conjunction with <see cref="IsGrounded"/> to determine if the <see cref="JumpStrength"/> should be used to make the entity jump.
    /// </summary>
    public bool jump;

    // -------------------------------------------------------------------------------------
    // Control Properties
    // -------------------------------------------------------------------------------------

    /// <summary>
    /// Gravity force applied to the character.
    /// </summary>
    public float3 gravity;// new float3(0,-9.81f,0)

    /// <summary>
    /// The maximum speed at which this character moves.
    /// </summary>
    public float maxSpeed;//7.5

    /// <summary>
    /// The current speed at which the player moves.
    /// </summary>
    public float speed;//5

    /// <summary>
    /// The jump strength which controls how high a jump is, in conjunction with <see cref="Gravity"/>.
    /// </summary>
    public float jumpStrength;//9

    /// <summary>
    /// The maximum height the character can step up, in world units.
    /// </summary>
    public float maxStep;//0.35

    /// <summary>
    /// Drag value applied to reduce the <see cref="VerticalVelocity"/>.
    /// </summary>
    public float drag;//0.2

    // -------------------------------------------------------------------------------------
    // Control State
    // -------------------------------------------------------------------------------------

    /// <summary>
    /// True if the character is on the ground.
    /// </summary>
    public bool isGrounded;

    /// <summary>
    /// The current horizontal velocity of the character.
    /// </summary>
    public float3 horizontalVelocity;

    /// <summary>
    /// The current jump velocity of the character.
    /// </summary>
    public float3 verticalVelocity;
}
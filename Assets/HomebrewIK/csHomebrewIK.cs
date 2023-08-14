/*
 * Created :    Spring 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    HomebrewIK
 * Filename :   csHomebrewIK.cs (non-static monobehaviour module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 */



using System;       // Convert
using UnityEngine;  // Monobehaviour
using UnityEditor;  // Handles
using Unity.Physics;
using Unity.Entities;
using Unity.Physics.Authoring;
using Drawing;


namespace FischlWorks
{



    public class csHomebrewIK : MonoBehaviour
    {
        private Animator playerAnimator = null;

        public Transform leftFootTransform = null;
        public Transform rightFootTransform = null;

        private Transform leftFootOrientationReference = null;
        private Transform rightFootOrientationReference = null;

        private Vector3 initialForwardVector = new Vector3();

        public float _LengthFromHeelToToes
        {
            get { return lengthFromHeelToToes; }
        }

        public float _RaySphereRadius
        {
            get { return raySphereRadius; }
        }

        public float _LeftFootProjectedAngle
        {
            get { return leftFootProjectedAngle; }
        }

        public float _RightFootProjectedAngle
        {
            get { return rightFootProjectedAngle; }
        }

        public Vector3 _LeftFootIKPositionTarget
        {
            get
            {
                if (Application.isPlaying == true)
                {
                    return leftFootIKPositionTarget;
                }
                else
                {
                    // This is being done because the IK target only gets updated during playmode
                    return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
                }
            }
        }

        public Vector3 _RightFootIKPositionTarget
        {
            get
            {
                if (Application.isPlaying == true)
                {
                    return rightFootIKPositionTarget;
                }
                else
                {
                    // This is being done because the IK target only gets updated during playmode
                    return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
                }
            }
        }

        public float _AnkleHeightOffset
        {
            get
            {
                return ankleHeightOffset;
            }
        }

        public float _WorldHeightOffset
        {
            get
            {
                if (giveWorldHeightOffset == true)
                {
                    return worldHeightOffset;
                }
                else
                {
                    return 0;
                }
            }
        }

        [BigHeader("Foot Properties")]

        [SerializeField]
        [Range(0, 0.25f)]
        private float lengthFromHeelToToes = 0.1f;
        [SerializeField]
        [Range(0, 60)]
        private float maxRotationAngle = 45;
        [SerializeField]
        [Range(-0.05f, 0.125f)]
        private float ankleHeightOffset = 0;

        [BigHeader("IK Properties")]

        [SerializeField]
        private bool enableIKPositioning = true;
        [SerializeField]
        private bool enableIKRotating = true;
        [SerializeField]
        [Range(0, 1)]
        private float globalWeight = 1;
        [SerializeField]
        [Range(0, 1)]
        private float leftFootWeight = 1;
        [SerializeField]
        [Range(0, 1)]
        private float rightFootWeight = 1;
        [SerializeField]
        [Range(0, 0.1f)]
        private float smoothTime = 0.075f;

        [BigHeader("Ray Properties")]

        [SerializeField]
        [Range(0.05f, 0.1f)]
        private float raySphereRadius = 0.05f;
        [SerializeField]
        [Range(0.1f, 2)]
        private float rayCastRange = 2;
        [SerializeField]
        private PhysicsCategoryTags detectLayers = PhysicsCategoryTags.Everything;
        [SerializeField]
        private bool ignoreTriggers = true;

        [BigHeader("Raycast Start Heights")]

        [SerializeField]
        [Range(0.1f, 1)]
        private float leftFootRayStartHeight = 0.5f;
        [SerializeField]
        [Range(0.1f, 1)]
        private float rightFootRayStartHeight = 0.5f;

        [BigHeader("Advanced")]

        [SerializeField]
        private bool enableFootLifting = true;
        [ShowIf("enableFootLifting")]
        [SerializeField]
        private float floorRange = 0;
        [SerializeField]
        private bool enableBodyPositioning = true;
        [ShowIf("enableBodyPositioning")]
        [SerializeField]
        private float crouchRange = 0.25f;
        [ShowIf("enableBodyPositioning")]
        [SerializeField]
        private float stretchRange = 0;
        [SerializeField]
        private bool giveWorldHeightOffset = false;
        [ShowIf("giveWorldHeightOffset")]
        [SerializeField]
        private float worldHeightOffset = 0;
        private ColliderCastHit leftFootCastHitInfo = new ColliderCastHit();
        private ColliderCastHit rightFootCastHitInfo = new ColliderCastHit();

        private float leftFootRayHitHeight = 0;
        private float rightFootRayHitHeight = 0;

        private Vector3 leftFootRayStartPosition = new Vector3();
        private Vector3 rightFootRayStartPosition = new Vector3();

        private Vector3 leftFootDirectionVector = new Vector3();
        private Vector3 rightFootDirectionVector = new Vector3();

        private Vector3 leftFootProjectionVector = new Vector3();
        private Vector3 rightFootProjectionVector = new Vector3();

        private float leftFootProjectedAngle = 0;
        private float rightFootProjectedAngle = 0;

        private Vector3 leftFootRayHitProjectionVector = new Vector3();
        private Vector3 rightFootRayHitProjectionVector = new Vector3();

        private float leftFootRayHitProjectedAngle = 0;
        private float rightFootRayHitProjectedAngle = 0;

        private float leftFootHeightOffset = 0;
        private float rightFootHeightOffset = 0;

        private Vector3 leftFootIKPositionBuffer = new Vector3();
        private Vector3 rightFootIKPositionBuffer = new Vector3();

        private Vector3 leftFootIKPositionTarget = new Vector3();
        private Vector3 rightFootIKPositionTarget = new Vector3();

        private float leftFootHeightLerpVelocity = 0;
        private float rightFootHeightLerpVelocity = 0;

        private Vector3 leftFootIKRotationBuffer = new Vector3();
        private Vector3 rightFootIKRotationBuffer = new Vector3();

        private Vector3 leftFootIKRotationTarget = new Vector3();
        private Vector3 rightFootIKRotationTarget = new Vector3();

        private Vector3 leftFootRotationLerpVelocity = new Vector3();
        private Vector3 rightFootRotationLerpVelocity = new Vector3();

        private GUIStyle helperTextStyle = null;

        // --- --- --- custom 
        private EntityQuery physicsWorldQuery;
        private CollisionFilter detectFilter = CollisionFilter.Default;
        private QueryInteraction detectInteraction = QueryInteraction.Default;

        [BigHeader("Custom")]


        [SerializeField]
        private bool isDraw = false;
        public float curHeight, basicHeight, deltaHeight;
        [Tooltip("重心降低比例，以减少因添加IK后导致的腿部僵直效果"), SerializeField]
        private float reductionPercentageOfCenterOfMass = 0.05f;

        private void Start()
        {
            InitializeVariables();

            CreateOrientationReference();

        }



        private void Update()
        {
            UpdateFootProjection();

            UpdateRayHitInfo();

            UpdateIKPositionTarget();
            UpdateIKRotationTarget();

            // if (leftFootCastHitInfo.Entity != Entity.Null)
            // {
            //     using (Draw.ingame.WithLineWidth(3))
            //     {
            //         Draw.ingame.WireSphere(leftFootRayStartPosition, 0.1f, Color.green);
            //         Draw.ingame.Line(leftFootRayStartPosition,
            //     leftFootRayStartPosition - rayCastRange * Vector3.up, Color.green);
            //         Draw.ingame.Label2D(leftFootTransform.position, "L", 80);
            //         Draw.ingame.WireSphere(leftFootCastHitInfo.Position, 0.05f, Color.green);
            //     }
            // }
            // else
            // {
            //     using (Draw.ingame.WithLineWidth(3))
            //     {
            //         Draw.ingame.WireSphere(leftFootRayStartPosition, 0.1f, Color.red);
            //         Draw.ingame.Line(leftFootRayStartPosition,
            //     leftFootRayStartPosition - rayCastRange * Vector3.up, Color.red);
            //         Draw.ingame.Label2D(leftFootTransform.position, "L", 80);
            //     }
            // }

            // if (rightFootCastHitInfo.Entity != Entity.Null)
            // {
            //     using (Draw.ingame.WithLineWidth(3))
            //     {
            //         Draw.ingame.WireSphere(rightFootRayStartPosition, 0.1f, Color.green);
            //         Draw.ingame.Line(rightFootRayStartPosition,
            //     rightFootRayStartPosition - rayCastRange * Vector3.up, Color.green);
            //         Draw.ingame.Label2D(rightFootTransform.position, "R", 80);
            //         Draw.ingame.WireSphere(rightFootCastHitInfo.Position, 0.05f, Color.green);
            //     }
            // }
            // else
            // {
            //     using (Draw.ingame.WithLineWidth(3))
            //     {
            //         Draw.ingame.WireSphere(rightFootRayStartPosition, 0.1f, Color.red);
            //         Draw.ingame.Line(rightFootRayStartPosition,
            //     rightFootRayStartPosition - rayCastRange * Vector3.up, Color.red);
            //         Draw.ingame.Label2D(rightFootTransform.position, "R", 80);
            //     }
            // }
        }



        private void OnAnimatorIK()
        {
            LerpIKBufferToTarget();

            ApplyFootIK();
            ApplyBodyIK();

        }



        // --- --- ---



        private void InitializeVariables()
        {
            playerAnimator = GetComponent<Animator>();

            leftFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

            // This is for faster development iteration purposes
            if (detectLayers.Value == PhysicsCategoryTags.Nothing.Value)
            {
                detectLayers = PhysicsCategoryTags.Everything;
            }

            // This is needed in order to wrangle with quaternions to get the final direction vector of each foot later
            initialForwardVector = transform.forward;

            // Initial value is given to make the first frames of lerping look natural, rotations should not need these
            leftFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();
            rightFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();

            // This is being done here due to internal unity reasons
            helperTextStyle = new GUIStyle()
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            helperTextStyle.normal.textColor = Color.yellow;

            //-----custom-----
            physicsWorldQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            detectFilter.CollidesWith = detectLayers.Value;
            if (ignoreTriggers)
            {
                detectInteraction = QueryInteraction.IgnoreTriggers;
            }
        }



        // This is being done to track bone orientation, since we cannot use footTransform's rotation in its own anyway
        private void CreateOrientationReference()
        {
            /* Just in case that this function gets called again... */

            if (leftFootOrientationReference != null)
            {
                Destroy(leftFootOrientationReference);
            }

            if (rightFootOrientationReference != null)
            {
                Destroy(rightFootOrientationReference);
            }

            /* These gameobjects hold different orientation values from footTransform.rotation, but the delta remains the same */

            leftFootOrientationReference = new GameObject("[RUNTIME] Normal_Orientation_Reference").transform;
            rightFootOrientationReference = new GameObject("[RUNTIME] Normal_Orientation_Reference").transform;

            leftFootOrientationReference.position = leftFootTransform.position;
            rightFootOrientationReference.position = rightFootTransform.position;

            leftFootOrientationReference.SetParent(leftFootTransform);
            rightFootOrientationReference.SetParent(rightFootTransform);
        }



        //This is being done because we want to know in what angle did the foot go underground
        private void UpdateFootProjection()
        {
            /* This is the only part in this script (except for those gizmos) that accesses footOrientationReference */

            leftFootDirectionVector = leftFootOrientationReference.rotation * initialForwardVector;
            rightFootDirectionVector = rightFootOrientationReference.rotation * initialForwardVector;

            /* World space based vector defines are used here for the representation of floor orientation */

            leftFootProjectionVector = Vector3.ProjectOnPlane(leftFootDirectionVector, Vector3.up);
            rightFootProjectionVector = Vector3.ProjectOnPlane(rightFootDirectionVector, Vector3.up);

            /* Cross is done in this order because we want the underground angle to be positive */

            leftFootProjectedAngle = Vector3.SignedAngle(
                leftFootProjectionVector,
                leftFootDirectionVector,
                Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) *
                // This is needed to cancel out the cross product's axis inverting behaviour
                Mathf.Sign(leftFootDirectionVector.y));

            rightFootProjectedAngle = Vector3.SignedAngle(
                rightFootProjectionVector,
                rightFootDirectionVector,
                Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) *
                // This is needed to cancel out the cross product's axis inverting behaviour
                Mathf.Sign(rightFootDirectionVector.y));
        }



        private void UpdateRayHitInfo()
        {
            /* Rays will be casted from above each foot, in the downward orientation of the world */

            leftFootRayStartPosition = leftFootTransform.position;
            leftFootRayStartPosition.y += leftFootRayStartHeight;

            rightFootRayStartPosition = rightFootTransform.position;
            rightFootRayStartPosition.y += rightFootRayStartHeight;

            /* SphereCast is used here just because we need a normal vector to rotate our foot towards */

            // Vector3.up is used here instead of transform.up to get normal vector in world orientation

            var physicsSingleton = physicsWorldQuery.GetSingleton<PhysicsWorldSingleton>();

            physicsSingleton.SphereCast(leftFootRayStartPosition,
                raySphereRadius,
                Vector3.up * -1,
                rayCastRange,
                out leftFootCastHitInfo,
                detectFilter,
                detectInteraction);

            // Vector3.up is used here instead of transform.up to get normal vector in world orientation
            physicsSingleton.SphereCast(rightFootRayStartPosition,
                raySphereRadius,
                Vector3.up * -1,
                rayCastRange,
                out rightFootCastHitInfo,
                detectFilter,
                detectInteraction);

            // Left Foot Ray Handling
            if (leftFootCastHitInfo.Entity != Entity.Null)
            {
                leftFootRayHitHeight = leftFootCastHitInfo.Position.y;
                leftFootRayHitProjectionVector = Vector3.ProjectOnPlane(
                    leftFootCastHitInfo.SurfaceNormal,
                    Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector));

                leftFootRayHitProjectedAngle = Vector3.Angle(
                    leftFootRayHitProjectionVector,
                    Vector3.up);
            }
            else
            {
                leftFootRayHitHeight = transform.position.y;
            }

            // Right Foot Ray Handling
            if (rightFootCastHitInfo.Entity != Entity.Null)
            {
                rightFootRayHitHeight = rightFootCastHitInfo.Position.y;

                /* Angle from the floor is also calculated to isolate the rotation caused by the animation */

                // We are doing this crazy operation because we only want to count rotations that are parallel to the foot
                rightFootRayHitProjectionVector = Vector3.ProjectOnPlane(
                    rightFootCastHitInfo.SurfaceNormal,
                    Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector));

                rightFootRayHitProjectedAngle = Vector3.Angle(
                    rightFootRayHitProjectionVector,
                    Vector3.up);
            }
            else
            {
                rightFootRayHitHeight = transform.position.y;

            }
        }



        private void UpdateIKPositionTarget()
        {
            /* We reset the offset values here instead of declaring them as local variables, since other functions reference it */

            leftFootHeightOffset = 0;
            rightFootHeightOffset = 0;

            /* Foot height correction based on the projected angle */

            float trueLeftFootProjectedAngle = leftFootProjectedAngle - leftFootRayHitProjectedAngle;

            if (trueLeftFootProjectedAngle > 0)
            {
                leftFootHeightOffset += Mathf.Abs(Mathf.Sin(
                    Mathf.Deg2Rad * trueLeftFootProjectedAngle) *
                    lengthFromHeelToToes);

                // There's no Abs here to support negative manual height offset
                leftFootHeightOffset += Mathf.Cos(
                    Mathf.Deg2Rad * trueLeftFootProjectedAngle) *
                    GetAnkleHeight();
            }
            else
            {
                leftFootHeightOffset += GetAnkleHeight();
            }

            /* Foot height correction based on the projected angle */

            float trueRightFootProjectedAngle = rightFootProjectedAngle - rightFootRayHitProjectedAngle;

            if (trueRightFootProjectedAngle > 0)
            {
                rightFootHeightOffset += Mathf.Abs(Mathf.Sin(
                    Mathf.Deg2Rad * trueRightFootProjectedAngle) *
                    lengthFromHeelToToes);

                // There's no Abs here to support negative manual height offset
                rightFootHeightOffset += Mathf.Cos(
                    Mathf.Deg2Rad * trueRightFootProjectedAngle) *
                    GetAnkleHeight();
            }
            else
            {
                rightFootHeightOffset += GetAnkleHeight();
            }

            /* Application of calculated position */

            leftFootIKPositionTarget.y = leftFootRayHitHeight + leftFootHeightOffset + _WorldHeightOffset;
            rightFootIKPositionTarget.y = rightFootRayHitHeight + rightFootHeightOffset + _WorldHeightOffset;
        }



        private void UpdateIKRotationTarget()
        {
            if (leftFootCastHitInfo.Entity != Entity.Null)
            {
                leftFootIKRotationTarget = Vector3.Slerp(
                    transform.up,
                    leftFootCastHitInfo.SurfaceNormal,
                    Mathf.Clamp(Vector3.Angle(transform.up, leftFootCastHitInfo.SurfaceNormal), 0, maxRotationAngle) /
                    // Addition of 1 is to prevent division by zero, not a perfect solution but somehow works
                    (Vector3.Angle(transform.up, leftFootCastHitInfo.SurfaceNormal) + 1));
            }
            else
            {
                leftFootIKRotationTarget = transform.up;
            }

            if (rightFootCastHitInfo.Entity != Entity.Null)
            {
                rightFootIKRotationTarget = Vector3.Slerp(
                    transform.up,
                    rightFootCastHitInfo.SurfaceNormal,
                    Mathf.Clamp(Vector3.Angle(transform.up, rightFootCastHitInfo.SurfaceNormal), 0, maxRotationAngle) /
                    // Addition of 1 is to prevent division by zero, not a perfect solution but somehow works
                    (Vector3.Angle(transform.up, rightFootCastHitInfo.SurfaceNormal) + 1));
            }
            else
            {
                rightFootIKRotationTarget = transform.up;
            }
        }



        private void LerpIKBufferToTarget()
        {
            /* Instead of wrangling with weights, we switch the lerp targets to make movement smooth */

            if (enableFootLifting == true &&
                playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y >=
                leftFootIKPositionTarget.y + floorRange)
            {
                leftFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    leftFootIKPositionBuffer.y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y,
                    ref leftFootHeightLerpVelocity,
                    smoothTime);
            }
            else
            {
                leftFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    leftFootIKPositionBuffer.y,
                    leftFootIKPositionTarget.y,
                    ref leftFootHeightLerpVelocity,
                    smoothTime);
            }

            if (enableFootLifting == true &&
                playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y >=
                rightFootIKPositionTarget.y + floorRange)
            {
                rightFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    rightFootIKPositionBuffer.y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y,
                    ref rightFootHeightLerpVelocity,
                    smoothTime);
            }
            else
            {
                rightFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    rightFootIKPositionBuffer.y,
                    rightFootIKPositionTarget.y,
                    ref rightFootHeightLerpVelocity,
                    smoothTime);
            }

            leftFootIKRotationBuffer = Vector3.SmoothDamp(
                leftFootIKRotationBuffer,
                leftFootIKRotationTarget,
                ref leftFootRotationLerpVelocity,
                smoothTime);

            rightFootIKRotationBuffer = Vector3.SmoothDamp(
                rightFootIKRotationBuffer,
                rightFootIKRotationTarget,
                ref rightFootRotationLerpVelocity,
                smoothTime);
        }



        private void ApplyFootIK()
        {
            /* Weight designation */

            playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, globalWeight * leftFootWeight);
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, globalWeight * rightFootWeight);

            playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, globalWeight * leftFootWeight);
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, globalWeight * rightFootWeight);

            /* Position handling */

            CopyByAxis(ref leftFootIKPositionBuffer, playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot),
                true, false, true);

            CopyByAxis(ref rightFootIKPositionBuffer, playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot),
                true, false, true);

            if (enableIKPositioning == true)
            {
                // if (leftFootIKPositionBuffer.y < 0.7f)
                // {
                playerAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPositionBuffer);
                // }

                // if (rightFootIKPositionBuffer.y < 0.7f)
                // {
                playerAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPositionBuffer);
                // }
            }

            /* Rotation handling */

            /* This part may be a bit tricky to understand intuitively, refer to docs for an explanation in depth */

            // FromToRotation is used because we need the delta, not the final target orientation
            Quaternion leftFootRotation =
                Quaternion.FromToRotation(transform.up, leftFootIKRotationBuffer) *
                playerAnimator.GetIKRotation(AvatarIKGoal.LeftFoot);

            // FromToRotation is used because we need the delta, not the final target orientation
            Quaternion rightFootRotation =
                Quaternion.FromToRotation(transform.up, rightFootIKRotationBuffer) *
                playerAnimator.GetIKRotation(AvatarIKGoal.RightFoot);

            if (enableIKRotating == true)
            {
                playerAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
                playerAnimator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
            }
        }



        private void ApplyBodyIK()
        {
            if (enableBodyPositioning == false)
            {
                return;
            }

            float minFootHeight = Mathf.Min(
                    playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y);
            // if (minFootHeight >= 0.7f)
            // {
            //     minFootHeight = transform.position.y;
            // }

            if (basicHeight == 0)
            {
                basicHeight = playerAnimator.bodyPosition.y - transform.position.y;
            }

            /* This part moves the body 'downwards' by the root gameobject's height */
            curHeight = playerAnimator.bodyPosition.y + LimitValueByRange(minFootHeight - transform.position.y, 0);
            deltaHeight = curHeight - basicHeight;
            if (deltaHeight < 0) { deltaHeight = 0; }
            curHeight -= basicHeight * reductionPercentageOfCenterOfMass;
            playerAnimator.bodyPosition = new Vector3(
            playerAnimator.bodyPosition.x,
            curHeight,
            playerAnimator.bodyPosition.z);
        }



        private float GetAnkleHeight()
        {
            return raySphereRadius + _AnkleHeightOffset;
        }



        private void CopyByAxis(ref Vector3 target, Vector3 source, bool copyX, bool copyY, bool copyZ)
        {
            target = new Vector3(
                Mathf.Lerp(
                    target.x,
                    source.x,
                    Convert.ToInt32(copyX)),
                Mathf.Lerp(
                    target.y,
                    source.y,
                    Convert.ToInt32(copyY)),
                Mathf.Lerp(
                    target.z,
                    source.z,
                    Convert.ToInt32(copyZ)));
        }



        private float LimitValueByRange(float value, float floor)
        {
            // 这里 stretchRange 和 crouchRange 是上升和下降超出范围后，移动人物高度
            if (value < floor - stretchRange)
            {
                return value + stretchRange;
            }
            else if (value > floor + crouchRange)
            {
                return value - crouchRange;
            }
            else
            {
                return floor;
            }
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {


            // Debug draw function relies on objects that are dynamically located during runtime
            if (Application.isPlaying == false)
            {
                return;
            }

            /* Left Foot */

            if (leftFootCastHitInfo.Entity != Entity.Null)
            {
                Handles.color = Color.yellow;

                // Just note that the normal vector of RayCastHit object is used here
                Handles.DrawWireDisc(
                    leftFootCastHitInfo.Position,
                    leftFootCastHitInfo.SurfaceNormal,
                    0.1f);
                Handles.DrawDottedLine(
                    leftFootTransform.position,
                    leftFootTransform.position + (Vector3)leftFootCastHitInfo.SurfaceNormal,
                    2);

                // Just note that the orientation of the parent transform is used here
                Handles.color = Color.green;
                Handles.DrawWireDisc(
                    leftFootCastHitInfo.Position,
                    transform.up, 0.25f);

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(
                    (Vector3)leftFootCastHitInfo.Position + transform.up * raySphereRadius,
                    raySphereRadius);
            }
            else
            {
                Gizmos.color = Color.red;
            }

            if (leftFootProjectedAngle > 0)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.red;
            }

            // Foot height correction related debug draws
            Handles.DrawWireDisc(
                leftFootTransform.position,
                leftFootOrientationReference.rotation * transform.up,
                0.15f);
            Handles.DrawSolidArc(
                leftFootTransform.position,
                Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) * -1,
                leftFootProjectionVector,
                // Abs is needed here because the cross product will deal with axis direction
                Mathf.Abs(leftFootProjectedAngle),
                0.25f);
            Handles.DrawDottedLine(
                leftFootTransform.position,
                leftFootTransform.position + leftFootDirectionVector.normalized,
                2);

            // SphereCast related debug draws
            Gizmos.DrawWireSphere(
                leftFootRayStartPosition,
                0.1f);
            Gizmos.DrawLine(
                leftFootRayStartPosition,
                leftFootRayStartPosition - rayCastRange * Vector3.up);



            // Indicator text
            Handles.Label(leftFootTransform.position, "L", helperTextStyle);

            /* Right foot */

            if (rightFootCastHitInfo.Entity != Entity.Null)
            {
                Handles.color = Color.yellow;

                // Just note that the normal vector of RayCastHit object is used here
                Handles.DrawWireDisc(
                    rightFootCastHitInfo.Position,
                    rightFootCastHitInfo.SurfaceNormal,
                    0.1f);
                Handles.DrawDottedLine(
                    rightFootTransform.position,
                    rightFootTransform.position + (Vector3)rightFootCastHitInfo.SurfaceNormal,
                    2);

                // Just note that the orientation of the parent transform is used here
                Handles.color = Color.green;
                Handles.DrawWireDisc(
                    rightFootCastHitInfo.Position,
                    transform.up, 0.25f);

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(
                    (Vector3)rightFootCastHitInfo.Position + transform.up * raySphereRadius,
                    raySphereRadius);
            }
            else
            {
                Gizmos.color = Color.red;
            }

            if (rightFootProjectedAngle > 0)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.red;
            }

            // Foot height correction related debug draws
            Handles.DrawWireDisc(
                rightFootTransform.position,
                rightFootOrientationReference.rotation * transform.up,
                0.15f);
            Handles.DrawSolidArc(
                rightFootTransform.position,
                Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) * -1,
                rightFootProjectionVector,
                // Abs is needed here because the cross product will deal with axis direction
                Mathf.Abs(rightFootProjectedAngle),
                0.25f);
            Handles.DrawDottedLine(
                rightFootTransform.position,
                rightFootTransform.position + rightFootDirectionVector.normalized,
                2);

            // SphereCast related debug draws
            Gizmos.DrawWireSphere(
                rightFootRayStartPosition,
                0.1f);
            Gizmos.DrawLine(
                rightFootRayStartPosition,
                rightFootRayStartPosition - rayCastRange * Vector3.up);

            // Indicator text
            Handles.Label(rightFootTransform.position, "R", helperTextStyle);

        }
#endif
    }



    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string _BaseCondition
        {
            get { return mBaseCondition; }
        }

        private string mBaseCondition = String.Empty;

        public ShowIfAttribute(string baseCondition)
        {
            mBaseCondition = baseCondition;
        }
    }



    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BigHeaderAttribute : PropertyAttribute
    {
        public string _Text
        {
            get { return mText; }
        }

        private string mText = String.Empty;

        public BigHeaderAttribute(string text)
        {
            mText = text;
        }
    }



}
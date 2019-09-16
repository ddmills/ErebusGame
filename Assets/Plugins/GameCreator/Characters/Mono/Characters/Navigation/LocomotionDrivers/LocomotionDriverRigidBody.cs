using System.Linq.Expressions;
namespace GameCreator.Characters
{
    using UnityEngine;

    // TODO: should this require tag on the `Character`.
    // Will the locomotion driver always be on the same
    // gameobject as the `Character` class?
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    [AddComponentMenu("Game Creator/Characters/Rigid Body Locomotion Driver", 100)]
    public class LocomotionDriverRigidBody : ILocomotionDriver
    {
        [HideInInspector] private Rigidbody rb;
        [HideInInspector] private CapsuleCollider cd;

        [Range(0, 90)]
        [SerializeField] private float slopeLimit = 45;

        private RaySensor sensor;
        [SerializeField] private RaySensor.CastType sensorType = RaySensor.CastType.Raycast;
        [SerializeField] private LayerMask sensorLayermask  = ~0;
        [SerializeField] private bool isInDebugMode = false;
        [SerializeField] private int sensorArrayRows = 1;
        [SerializeField] private int sensorArrayRayCount = 6;
        [SerializeField] private bool sensorArrayRowsAreOffset = false;
        [SerializeField] private float sensorRadiusModifier = 0.8f;

        private bool IsUsingExtendedSensorRange  = true;
        private float baseSensorRange = 0f;
        private bool isGrounded;
        private Vector3 currentGroundAdjustmentVelocity = Vector3.zero;
        private float safetyDistanceFactor = 0.001f;

        [SerializeField] private float height = 2f;
        [SerializeField] private Vector3 center = new Vector3(0, 1, 0);
        [SerializeField] private float radius = .5f;
        [SerializeField] private float stepHeight = .3f;

        public override void Setup(Character character)
        {
            this.rb = character.GetComponent<Rigidbody>();
            this.cd = character.GetComponent<CapsuleCollider>();
            this.sensor = new RaySensor(this.transform, this.cd);

            this.ConfigureCollider();
            this.ConfigureRigidBody();
            this.ConfigureSensor();
        }

        void FixedUpdate()
        {
            currentGroundAdjustmentVelocity = Vector3.zero;

            if (IsUsingExtendedSensorRange)
            {
                sensor.castLength = baseSensorRange + height + stepHeight;
            }
            else
            {
                sensor.castLength = baseSensorRange;
            }

            sensor.Cast();

            if (!sensor.HasDetectedHit())
            {
                isGrounded = false;
                return;
            }

            isGrounded = (Vector3.Angle(sensor.GetNormal(), transform.up) < slopeLimit);

            float groundDistance = sensor.GetDistance();
            float upperLimit = (height * (1f - stepHeight)) * 0.5f;
            float middle = upperLimit + height * stepHeight;
            float distanceToGo = middle - groundDistance;

            currentGroundAdjustmentVelocity = transform.up * (distanceToGo/Time.fixedDeltaTime);
        }

        private void ConfigureRigidBody()
        {
            rb.freezeRotation = true;
            rb.useGravity = false;
        }

        private void ConfigureCollider()
        {
            cd.radius = radius;
            cd.center = center + new Vector3(0, stepHeight * height / 2f, 0); // TODO: multiply by height ?
            cd.height = height * (1f - stepHeight);

            if (cd.height / 2 < cd.radius)
            {
                cd.radius = cd.height / 2f;
            }
        }

        private void ConfigureSensor()
        {
            sensor.SetCastOrigin(GetCenter());
            sensor.SetCastDirection(RaySensor.CastDirection.Down);

            sensor.castType = sensorType;
            sensor.layermask = sensorLayermask;

            float baseSensorRadius = radius * sensorRadiusModifier;
            float maxSensorRadius = (cd.height / 2) * (1f - safetyDistanceFactor);

            sensor.sphereCastRadius = Mathf.Clamp(baseSensorRadius, safetyDistanceFactor, maxSensorRadius);

            float castLength = 0f;
            castLength += (height * (1f - stepHeight)) * 0.5f;
            castLength += height * stepHeight;
            baseSensorRange = castLength * (1f + safetyDistanceFactor);
            sensor.castLength = castLength;

            sensor.ArrayRows = sensorArrayRows;
            sensor.arrayRayCount = sensorArrayRayCount;
            sensor.offsetArrayRows = sensorArrayRowsAreOffset;
            sensor.isInDebugMode = isInDebugMode;

            sensor.calculateRealDistance = true;
            sensor.calculateRealSurfaceNormal = true;

            sensor.RecalibrateRaycastArrayPositions();
        }

        public override void SetVelocity(Vector3 velocity)
        {
            rb.velocity = velocity + currentGroundAdjustmentVelocity;
        }

        public override void Move(Vector3 deltas)
        {
            rb.MovePosition(deltas);
            Debug.Log("!! Move invoked");
        }

        public override bool IsGrounded()
        {
            return this.isGrounded;
        }

        public override void SetCollisionDetection(bool isEnabled)
        {
            this.rb.detectCollisions = isEnabled;
        }

        public override void SetHeight(float value)
        {
            this.cd.height = value;
            this.cd.center = Vector3.up * (value / 2);
        }

        public override float GetSlopeAngleLimit()
        {
            return 45f;
        }

        public override float GetSkinWidth()
        {
            return 0;
        }

        public override float GetHeight()
        {
            return cd.height;
        }

        public override Vector3 GetCenter()
        {
            return cd.bounds.center;
        }

        public override float GetRadius()
        {
            return cd.radius;
        }

        public override Vector3 GetVelocity()
        {
            return rb.velocity;
        }
    }
}

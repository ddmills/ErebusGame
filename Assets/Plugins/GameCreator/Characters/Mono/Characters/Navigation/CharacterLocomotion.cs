namespace GameCreator.Characters
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Variables;
    using System;

    [System.Serializable]
    public class CharacterLocomotion
    {
        public enum ANIM_CONSTRAINT
        {
            NONE,
            KEEP_MOVEMENT,
            KEEP_POSITION
        }

        public enum LOCOMOTION_SYSTEM
        {
            LocomotionDriver,
            NavigationMeshAgent
        }

        public enum FACE_DIRECTION
        {
            MovementDirection,
            CameraDirection,
            Target,
            GroundPlaneCursor
        }

        public enum OVERRIDE_FACE_DIRECTION
        {
            None = -1,
            MovementDirection = FACE_DIRECTION.MovementDirection,
            CameraDirection = FACE_DIRECTION.CameraDirection,
            Target = FACE_DIRECTION.Target,
            GroundPlaneCursor = FACE_DIRECTION.GroundPlaneCursor,
        }

        public enum STEP
        {
            Any,
            Left,
            Right
        }

        // CONSTANTS: -----------------------------------------------------------------------------

        private const float JUMP_COYOTE_TIME = 0.3f;
        private const float MAX_GROUND_VSPEED = -9.8f;
        private const float GROUND_TIME_OFFSET = 0.1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool isControllable = true;
        public float runSpeed = 4.0f;
        public float acceleration = 10f;
        public float deceleration = 4f;

        [Range(0, 720f)]
        public float angularSpeed = 540f;
        public float gravity = -9.81f;
        public float maxFallSpeed = -100f;

        public bool canRun = true;
        public bool canJump = true;
        public float jumpForce = 6.0f;
        public int jumpTimes = 1;
        public float timeBetweenJumps = 0.5f;

        [HideInInspector] public Vector3 terrainNormal = Vector3.up;
        [HideInInspector] public float verticalSpeed = 0.0f;

        // ADVANCED PROPERTIES: -------------------------------------------------------------------

        public OVERRIDE_FACE_DIRECTION overrideFaceDirection = OVERRIDE_FACE_DIRECTION.None;
        public TargetPosition overrideFaceDirectionTarget = new TargetPosition();

        public FACE_DIRECTION faceDirection = FACE_DIRECTION.MovementDirection;
        public TargetPosition faceDirectionTarget = new TargetPosition();

        [Tooltip("Check this if you want to use Unity's NavMesh and have a map baked")]
        public bool canUseNavigationMesh = false;

        // INNER PROPERTIES: ----------------------------------------------------------------------

        private float lastGroundTime = 0f;
        private float lastJumpTime = 0f;
        private int jumpChain = 0;
        private Vector3 momentum = Vector3.zero;
        private Vector3 lastSavedVelocity = Vector3.zero;

        [HideInInspector] public Character character;

        [HideInInspector] public ANIM_CONSTRAINT animatorConstraint = ANIM_CONSTRAINT.NONE;
        [HideInInspector] public ILocomotionDriver locomotionDriver;
        [HideInInspector] public NavMeshAgent navmeshAgent;

        public LOCOMOTION_SYSTEM currentLocomotionType { get; private set; }
        public ILocomotionSystem currentLocomotionSystem { get; private set; }

        // INITIALIZERS: --------------------------------------------------------------------------

        public void Setup(Character character)
        {
            this.lastGroundTime = Time.time;
            this.lastJumpTime = Time.time;

            this.character = character;
            this.locomotionDriver = this.character.GetComponent<ILocomotionDriver>();

            this.currentLocomotionType = LOCOMOTION_SYSTEM.LocomotionDriver;

            this.locomotionDriver.Setup(this.character);
            this.GenerateNavmeshAgent();
            this.SetDirectionalDirection(Vector3.zero);
        }

        // UPDATE: --------------------------------------------------------------------------------

        public void Update()
        {
            this.UpdateCharacterState(this.currentLocomotionType);
            this.locomotionDriver.SetExtendSensorRange(this.character.IsGrounded());
            this.HandleGravity();

            this.currentLocomotionType = this.currentLocomotionSystem.Update();
            this.lastSavedVelocity = this.locomotionDriver.GetVelocity();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddMomentum(Vector3 value)
        {
            this.momentum += value;
        }

        public void Dash(Vector3 direction, float impulse, float duration, float drag)
        {
            this.SetDirectionalDirection(Vector3.zero);
            this.currentLocomotionSystem.Dash(direction, impulse, duration, drag);
        }

        public int Jump()
        {
            return this.Jump(this.jumpForce);
        }

        public int Jump(float jumpForce)
        {
            // bool isGrounded = (
            //     this.characterController.isGrounded ||
            //     Time.time < this.lastGroundTime + JUMP_COYOTE_TIME
            // );

            // bool jumpDelay = this.lastJumpTime + this.timeBetweenJumps < Time.time;
            // bool jumpNumber = isGrounded || this.jumpChain < this.jumpTimes;
            // if (this.canJump && jumpNumber && jumpDelay)
            // {
            //     this.verticalSpeed = jumpForce;
            //     this.lastJumpTime = Time.time;
            //     if (this.character.onJump != null)
            //     {
            //         this.character.onJump.Invoke(this.jumpChain);
            //     }

            //     this.jumpChain++;

            //     return this.jumpChain;
            // }

            return -1;
        }

        public void Teleport(Vector3 position)
        {
            this.character.transform.position = position;

            if (this.currentLocomotionType == CharacterLocomotion.LOCOMOTION_SYSTEM.NavigationMeshAgent)
            {
                this.character.characterLocomotion.navmeshAgent.Warp(position);
            }
        }

        public void SetAnimatorConstraint(ANIM_CONSTRAINT constraint)
        {
            this.animatorConstraint = constraint;
        }

        public void ChangeHeight(float height)
        {
            if (this.locomotionDriver != null)
            {
                this.locomotionDriver.SetHeight(height);
            }

            if (this.navmeshAgent != null)
            {
                this.navmeshAgent.height = height;
            }
        }

        public void SetIsControllable(bool isControllable)
        {
            if (isControllable == this.isControllable) return;
            this.isControllable = isControllable;

            if (!isControllable) this.SetDirectionalDirection(Vector3.zero);
            this.character.onIsControllable.Invoke(this.isControllable);
        }

        public Vector3 GetAimDirection()
        {
            return this.currentLocomotionSystem.aimDirection;
        }

        public Vector3 GetMovementDirection()
        {
            return this.currentLocomotionSystem.movementDirection;
        }

        private void OnGroundContactLost()
        {
            Vector3 currentVelocity = this.lastSavedVelocity;

             //Remove all vertical parts of 'currentVelocity';
            currentVelocity = VectorMath.RemoveDotVector(currentVelocity, this.locomotionDriver.transform.up);

             //Calculate magnitude and direction from 'currentVelocity';
            float magnitude = currentVelocity.magnitude;
            Vector3 direction = Vector3.zero;

             //Calculate velocity direction;
            if (magnitude != 0f)
            {
                direction = currentVelocity / magnitude;
            }

             // TODO: Subtract from 'magnitude', based on 'movementSpeed' and 'airControl', check for overshooting;
            // if (magnitude >= movementSpeed * airControl)
            // {
            //     magnitude -= movementSpeed * airControl;
            // }
            // else
            // {
            //     magnitude = 0f;
            // }

             // momentum = direction * magnitude;
            momentum = Vector3.zero;
        }

         private void OnGroundContactRegained(Vector3 collisionVelocity)
        {
            if (this.character.onLand != null)
            {
                this.character.onLand.Invoke(this.momentum.y); // TODO: send vector3
            }

             this.momentum = Vector3.zero;
        }

         private bool IsRisingOrFalling()
        {
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(this.momentum, this.locomotionDriver.transform.up);
            float limit = 0.001f;

             return verticalMomentum.magnitude > limit;
        }

         public Vector3 GetMomentum()
        {
            return this.momentum;
        }

        // PUBLIC LOCOMOTION METHODS: -------------------------------------------------------------

        public void SetDirectionalDirection(Vector3 direction, ILocomotionSystem.TargetRotation rotation = null)
        {
            this.ChangeLocomotionSystem<LocomotionSystemDirectional>();
            ((LocomotionSystemDirectional)this.currentLocomotionSystem).SetDirection(direction, rotation);
        }

        public void SetTarget(Ray ray, LayerMask layerMask, ILocomotionSystem.TargetRotation rotation,
            float stopThreshold, UnityAction callback = null)
        {
            this.ChangeLocomotionSystem<LocomotionSystemTarget>();
            ((LocomotionSystemTarget)this.currentLocomotionSystem)
                .SetTarget(ray, layerMask, rotation, stopThreshold, callback);
        }

        public void SetTarget(Vector3 position, ILocomotionSystem.TargetRotation rotation,
            float stopThreshold, UnityAction callback = null)
        {
            this.ChangeLocomotionSystem<LocomotionSystemTarget>();
            ((LocomotionSystemTarget)this.currentLocomotionSystem)
                .SetTarget(position, rotation, stopThreshold,  callback);
        }

        public void FollowTarget(Transform target, float minRadius, float maxRadius)
        {
            this.ChangeLocomotionSystem<LocomotionSystemFollow>();
            ((LocomotionSystemFollow)this.currentLocomotionSystem).SetFollow(target, minRadius, maxRadius);
        }

        public void Stop(ILocomotionSystem.TargetRotation rotation = null, UnityAction callback = null)
        {
            this.ChangeLocomotionSystem<LocomotionSystemTarget>();
            ((LocomotionSystemTarget)this.currentLocomotionSystem).Stop(rotation, callback);
        }

        public void SetRotation(Vector3 direction)
        {
            this.ChangeLocomotionSystem<LocomotionSystemRotation>();
            ((LocomotionSystemRotation)this.currentLocomotionSystem).SetDirection(direction);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GenerateNavmeshAgent()
        {
            if (!this.canUseNavigationMesh) return;

            if (this.navmeshAgent == null) this.navmeshAgent = this.character.gameObject.GetComponent<NavMeshAgent>();
            if (this.navmeshAgent == null) this.navmeshAgent = this.character.gameObject.AddComponent<NavMeshAgent>();

            this.navmeshAgent.updatePosition = false;
            this.navmeshAgent.updateRotation = false;
            this.navmeshAgent.updateUpAxis = false;
            this.navmeshAgent.radius = this.locomotionDriver.GetRadius();
            this.navmeshAgent.height = this.locomotionDriver.GetHeight();
            this.navmeshAgent.acceleration = this.acceleration;
        }

        private void ChangeLocomotionSystem<TLS>() where TLS : ILocomotionSystem, new()
        {
            if (this.currentLocomotionSystem != null && typeof(TLS) == this.currentLocomotionSystem.GetType()) return;
            if (this.currentLocomotionSystem != null) this.currentLocomotionSystem.OnDestroy();

            this.currentLocomotionSystem = new TLS();
            this.currentLocomotionSystem.Setup(this);
        }

        private void HandleGravity()
        {
            Vector3 verticalMomentum = Vector3.zero;
            Vector3 horizontalMomentum = Vector3.zero;

            if(this.momentum != Vector3.zero)
            {
                verticalMomentum = VectorMath.ExtractDotVector(this.momentum, this.locomotionDriver.transform.up);
                horizontalMomentum = this.momentum - verticalMomentum;
            }

            verticalMomentum += this.locomotionDriver.transform.up * gravity * Time.fixedDeltaTime;

            if (this.character.IsGrounded())
            {
                verticalMomentum = Vector3.zero;
            }

            this.momentum = horizontalMomentum + verticalMomentum;
        }

        private Vector3 GetVelocity()
        {
            return this.lastSavedVelocity;
        }

        private void UpdateCharacterState(LOCOMOTION_SYSTEM locomotionSystem)
        {
            bool isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(this.momentum, this.locomotionDriver.transform.up) > 0f);

             switch (this.character.characterState.locomotionState)
            {
                case Character.LocomotionState.Grounded:
                    if (isRising)
                    {
                        this.character.characterState.locomotionState = Character.LocomotionState.Rising;
                        break;
                    }
                    if (!this.locomotionDriver.IsGrounded())
                    {
                        this.character.characterState.locomotionState = Character.LocomotionState.Falling;
                        OnGroundContactLost();
                        break;
                    }
                    break;
                case Character.LocomotionState.Falling:
                    if (isRising)
                    {
                        this.character.characterState.locomotionState = Character.LocomotionState.Rising;
                        break;
                    }
                    if (this.locomotionDriver.IsGrounded())
                    {
                        this.character.characterState.locomotionState = Character.LocomotionState.Grounded;
                        OnGroundContactRegained(momentum);
                        break;
                    }
                    break;
                case Character.LocomotionState.Rising:
                    if (!isRising)
                    {
                        if (this.locomotionDriver.IsGrounded())
                        {
                            this.character.characterState.locomotionState = Character.LocomotionState.Grounded;
                            OnGroundContactRegained(momentum);
                        }
                        else
                        {
                            this.character.characterState.locomotionState = Character.LocomotionState.Falling;
                        }
                    }
                    break;
                default:
                    this.character.characterState.locomotionState = Character.LocomotionState.Falling;
                    break;
            }

            // TODO cleanup
            Vector3 worldVelocity = Vector3.zero;
            bool isSliding = this.currentLocomotionSystem.isSliding;
            bool isGrounded = true;

            switch (locomotionSystem)
            {
                case LOCOMOTION_SYSTEM.LocomotionDriver:
                    worldVelocity = this.locomotionDriver.GetVelocity();
                    isGrounded = (
                        this.locomotionDriver.IsGrounded() ||
                        Time.time - this.lastGroundTime < GROUND_TIME_OFFSET
                    );
                    break;

                case LOCOMOTION_SYSTEM.NavigationMeshAgent:
                    worldVelocity = (this.navmeshAgent.velocity == Vector3.zero
                        ? this.locomotionDriver.GetVelocity()
                        : this.navmeshAgent.velocity
                    );
                    isGrounded = (
                        !this.navmeshAgent.isOnOffMeshLink ||
                        Time.time - this.lastGroundTime < GROUND_TIME_OFFSET
                    );
                    break;
            }

            Vector3 localVelocity = this.character.transform.InverseTransformDirection(worldVelocity);
            this.character.characterState.forwardSpeed = localVelocity;
            this.character.characterState.sidesSpeed = Mathf.Atan2(localVelocity.x, localVelocity.z);
            this.character.characterState.verticalSpeed = worldVelocity.y;

            this.character.characterState.pivotSpeed = this.currentLocomotionSystem.pivotSpeed;

            this.character.characterState.isGrounded = isGrounded ? 1f : 0f;
            this.character.characterState.isSliding = isSliding ? 1f : 0f;
            this.character.characterState.isDashing = this.currentLocomotionSystem.isDashing ? 1f : 0f;
            this.character.characterState.normal = this.terrainNormal;
        }
    }
}

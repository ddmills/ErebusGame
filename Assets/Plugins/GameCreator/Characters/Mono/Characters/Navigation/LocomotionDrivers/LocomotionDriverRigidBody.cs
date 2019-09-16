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
        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public CapsuleCollider cd;
        [Range(0, 90)] public float slopeLimit = 45;

        public override void Setup(Character character)
        {
            this.rb = character.GetComponent<Rigidbody>();
            this.cd = character.GetComponent<CapsuleCollider>();
        }

        public override void SetVelocity(Vector3 velocity)
        {
            rb.velocity = velocity;
        }

        public override void Move(Vector3 deltas)
        {
            rb.MovePosition(deltas);
        }

        public override bool IsGrounded()
        {
            return true;
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
            return cd.center;
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

using System.Numerics;
namespace GameCreator.Characters
{
    using UnityEngine;

    // TODO: should this require tag on the `Character`.
    // Will the locomotion driver always be on the same
    // gameobject as the `Character` class?
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Game Creator/Characters/Unity Locomotion Driver", 100)]
    public class LocomotionDriverUnityController : ILocomotionDriver
    {
        [HideInInspector] private CharacterController characterController;

        private Vector3 velocity;

        public override void Setup(Character character)
        {
            this.velocity = Vector3.zero;
            this.characterController = character.GetComponent<CharacterController>();
        }

        void FixedUpdate()
        {
            this.Move(this.velocity * Time.fixedDeltaTime);
        }

        public override void SetVelocity(Vector3 value)
        {
            this.velocity = value;
        }

        public override void Move(Vector3 delta)
        {
            characterController.Move(delta);
            Debug.Log("!!! MOVE CALLED");
        }

        public override bool IsGrounded()
        {
            return characterController.isGrounded;
        }

        public override void SetCollisionDetection(bool isEnabled)
        {
            this.characterController.detectCollisions = isEnabled;
        }

        public override void SetHeight(float value)
        {
            characterController.height = value;
            characterController.center = Vector3.up * (value / 2);
        }

        public override float GetSlopeAngleLimit()
        {
            return this.characterController.slopeLimit;
        }

        public override float GetSkinWidth()
        {
            return characterController.skinWidth;
        }
        public override float GetHeight()
        {
            return characterController.height;
        }

        public override Vector3 GetCenter()
        {
            return characterController.center;
        }

        public override float GetRadius()
        {
            return characterController.radius;
        }

        public override Vector3 GetVelocity()
        {
            return characterController.velocity;
        }
    }
}

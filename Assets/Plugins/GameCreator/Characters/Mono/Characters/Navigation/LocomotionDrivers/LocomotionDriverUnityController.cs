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
        [HideInInspector] public CharacterController characterController;

        public override void Setup(Character character)
        {
            this.characterController = character.GetComponent<CharacterController>();
        }

        public override void Move(Vector3 deltas)
        {
            characterController.Move(deltas);
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

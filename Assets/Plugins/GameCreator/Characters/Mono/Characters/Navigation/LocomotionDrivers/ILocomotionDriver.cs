namespace GameCreator.Characters
{
    using UnityEngine;

    public abstract class ILocomotionDriver : MonoBehaviour
    {
        public virtual void Setup(Character character)
        {
        }

        public virtual void Move(Vector3 deltas)
        {
        }

        public virtual bool IsGrounded()
        {
            return false;
        }

        public virtual void SetVelocity(Vector3 value)
        {
        }

        public virtual void SetCollisionDetection(bool isEnabled)
        {
        }

        public virtual void SetHeight(float value)
        {
        }

        public virtual float GetSlopeAngleLimit()
        {
            return 45f;
        }

        public virtual float GetSkinWidth()
        {
            return 0.5f;
        }

        public virtual float GetHeight()
        {
            return 1.0f;
        }

        public virtual Vector3 GetCenter()
        {
            return new Vector3(0, 1, 0);
        }

        public virtual float GetRadius()
        {
            return 0.5f;
        }

        public virtual Vector3 GetVelocity()
        {
            return Vector3.zero;
        }
    }
}

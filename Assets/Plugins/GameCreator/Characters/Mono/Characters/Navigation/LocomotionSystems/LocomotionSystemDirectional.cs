namespace GameCreator.Characters
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;
	using GameCreator.Core.Hooks;
    using System;

    public class LocomotionSystemDirectional : ILocomotionSystem
	{
		// PROPERTIES: ----------------------------------------------------------------------------

		protected Vector3 desiredDirection = Vector3.zero;

		// OVERRIDE METHODS: ----------------------------------------------------------------------

		public override CharacterLocomotion.LOCOMOTION_SYSTEM Update()
		{
            base.Update();

			if (this.characterLocomotion.navmeshAgent != null)
			{
				this.characterLocomotion.navmeshAgent.updatePosition = false;
				this.characterLocomotion.navmeshAgent.updateUpAxis = false;
			}

			Vector3 targetDirection = this.desiredDirection;
            ILocomotionDriver locomotionDriver = this.characterLocomotion.locomotionDriver;
            Vector3 characterForward = locomotionDriver.transform.TransformDirection(Vector3.forward);

            float targetSpeed = this.CalculateSpeed(targetDirection, locomotionDriver.IsGrounded());

            if (targetDirection == Vector3.zero)
            {
                targetDirection = this.movementDirection;
                targetSpeed = 0f;
            }

            float speed = this.CalculateAccelerationFromSpeed(targetSpeed);
            Quaternion targetRotation = this.UpdateRotation(targetDirection);

            this.UpdateAnimationConstraints(ref targetDirection, ref targetRotation);
            targetDirection *= speed;

            this.UpdateSliding();

            if (this.isSliding) targetDirection = this.slideDirection;
            targetDirection += this.characterLocomotion.GetMomentum();

            if (this.isDashing)
            {
                targetDirection = this.dashVelocity;
                targetRotation = locomotionDriver.transform.rotation;
            }

            locomotionDriver.SetVelocity(targetDirection);
			locomotionDriver.transform.rotation = targetRotation;

			if (this.characterLocomotion.navmeshAgent != null &&
                this.characterLocomotion.navmeshAgent.isActiveAndEnabled)
			{
                this.characterLocomotion.navmeshAgent.enabled = false;
            }

            return CharacterLocomotion.LOCOMOTION_SYSTEM.LocomotionDriver;
		}

        public override void OnDestroy ()
		{
			return;
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetDirection(Vector3 direction, TargetRotation rotation = null)
		{
			this.desiredDirection = direction;
		}
    }
}

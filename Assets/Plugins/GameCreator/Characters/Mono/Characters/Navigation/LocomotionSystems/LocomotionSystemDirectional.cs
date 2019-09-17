﻿namespace GameCreator.Characters
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;
	using GameCreator.Core.Hooks;

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

            float speed = this.CalculateSpeed(targetDirection, this.characterLocomotion.locomotionDriver.IsGrounded());
			Quaternion targetRotation = this.UpdateRotation(targetDirection);

			this.UpdateAnimationConstraints(ref targetDirection, ref targetRotation);
            this.UpdateSliding();

            targetDirection = Vector3.ClampMagnitude(Vector3.Scale(targetDirection, HORIZONTAL_PLANE), 1.0f);
            targetDirection *= speed;

            if (this.isSliding) targetDirection = this.slideDirection;
            targetDirection += this.characterLocomotion.GetMomentum();

            if (this.isDashing)
            {
                targetDirection = this.dashVelocity;
                targetRotation = this.characterLocomotion.locomotionDriver.transform.rotation;
            }

						this.characterLocomotion.locomotionDriver.SetVelocity(targetDirection);
			this.characterLocomotion.locomotionDriver.transform.rotation = targetRotation;

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

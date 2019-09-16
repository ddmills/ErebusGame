namespace GameCreator.Characters
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using UnityEngine.AI;
	using GameCreator.Core;
	using GameCreator.Core.Hooks;

	public class LocomotionSystemFollow : ILocomotionSystem
	{
        // PROPERTIES: ----------------------------------------------------------------------------

        private bool isFollowing;
		private bool usingNavmesh;
		private NavMeshPath path;

        private Transform targetTransform;
        private float minRadius = 0.0f;
        private float maxRadius = 0.0f;

		// OVERRIDE METHODS: ----------------------------------------------------------------------

		public override CharacterLocomotion.LOCOMOTION_SYSTEM Update()
		{
            base.Update();

            Vector3 currPosition = this.characterLocomotion.character.transform.position;
            float distance = (this.targetTransform != null
                ? Vector3.Distance(currPosition, this.targetTransform.position)
                : -1.0f
            );

            bool stopConditions = this.targetTransform == null;
            stopConditions |= (this.isFollowing && distance <= this.minRadius);
            stopConditions |= (!this.isFollowing && distance <= this.maxRadius);

            if (stopConditions)
			{
                this.isFollowing = false;

                if (this.usingNavmesh)
                {
                    this.characterLocomotion.navmeshAgent.enabled = true;
                    this.characterLocomotion.navmeshAgent.isStopped = true;
                }
                else
                {
                    Vector3 defaultDirection = Vector3.up * this.characterLocomotion.verticalSpeed;
                    this.characterLocomotion.locomotionDriver.SetVelocity(defaultDirection);
                }

				return (this.usingNavmesh
					? CharacterLocomotion.LOCOMOTION_SYSTEM.NavigationMeshAgent
					: CharacterLocomotion.LOCOMOTION_SYSTEM.LocomotionDriver
				);
			}

            this.isFollowing = true;

            ILocomotionDriver locomotionDriver = this.characterLocomotion.locomotionDriver;

            if (this.usingNavmesh)
            {
                NavMeshAgent agent = this.characterLocomotion.navmeshAgent;
                agent.enabled = true;
                agent.updatePosition = true;
                agent.updateUpAxis = true;

                NavMeshHit hit = new NavMeshHit();
                NavMesh.SamplePosition(this.targetTransform.position, out hit, 1.0f, NavMesh.AllAreas);
                if (hit.hit) agent.SetDestination(hit.position);

                float remainingDistance = agent.remainingDistance;
                bool isGrounded = agent.isOnOffMeshLink;
                agent.speed = this.CalculateSpeed(locomotionDriver.transform.forward, isGrounded);
                agent.angularSpeed = this.characterLocomotion.angularSpeed;

                agent.isStopped = false;
                agent.updateRotation = true;

                this.UpdateNavmeshAnimationConstraints();
                return CharacterLocomotion.LOCOMOTION_SYSTEM.NavigationMeshAgent;
            }
            else
            {
                if (this.characterLocomotion.navmeshAgent != null)
                {
                    this.characterLocomotion.navmeshAgent.enabled = false;
                    //this.characterLocomotion.navmeshAgent.updatePosition = false;
                    //this.characterLocomotion.navmeshAgent.updateUpAxis = false;
                }

                Vector3 targetPosition = Vector3.Scale(this.targetTransform.position, HORIZONTAL_PLANE);
                targetPosition += Vector3.up * currPosition.y;
                Vector3 targetDirection = (targetPosition - currPosition).normalized;

                float speed = this.CalculateSpeed(targetDirection, locomotionDriver.IsGrounded());
                Quaternion targetRotation = this.UpdateRotation(targetDirection);

                this.UpdateAnimationConstraints(ref targetDirection, ref targetRotation);

                targetDirection = Vector3.Scale(targetDirection, HORIZONTAL_PLANE) * speed;
                targetDirection += Vector3.up * this.characterLocomotion.verticalSpeed;

                locomotionDriver.SetVelocity(targetDirection);
                locomotionDriver.transform.rotation = targetRotation;

                if (this.characterLocomotion.navmeshAgent != null && this.characterLocomotion.navmeshAgent.isOnNavMesh)
                {
                    Vector3 position = locomotionDriver.transform.position;
                    this.characterLocomotion.navmeshAgent.Warp(position);
                }

                return CharacterLocomotion.LOCOMOTION_SYSTEM.LocomotionDriver;
            }
		}

		public override void OnDestroy()
		{
            return;
		}

		// PRIVATE METHODS: -----------------------------------------------------------------------

		private void UpdateNavmeshAnimationConstraints()
		{
			NavMeshAgent agent = this.characterLocomotion.navmeshAgent;
			if (this.characterLocomotion.animatorConstraint == CharacterLocomotion.ANIM_CONSTRAINT.KEEP_MOVEMENT)
			{
				if (agent.velocity == Vector3.zero)
				{
					agent.Move(agent.transform.forward * agent.speed * Time.deltaTime);
				}
			}

			if (this.characterLocomotion.animatorConstraint == CharacterLocomotion.ANIM_CONSTRAINT.KEEP_POSITION)
			{
				agent.isStopped = true;
			}
		}

		// PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetFollow(Transform targetTransform, float minRadius, float maxRadius)
		{
            this.usingNavmesh = this.characterLocomotion.canUseNavigationMesh;
            this.targetTransform = targetTransform;
            this.minRadius = minRadius;
            this.maxRadius = maxRadius;
            this.isFollowing = false;
		}
	}
}

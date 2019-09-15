namespace GameCreator.Characters
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;

	[AddComponentMenu("")]
	public class ActionCharacterTeleport : IAction
	{
        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        [Space]
        public TargetPosition position = new TargetPosition(TargetPosition.Target.Position);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            Character targetCharacter = this.character.GetCharacter(target);
            if (targetCharacter == null) return true;

            Vector3 targetPosition = this.position.GetPosition(target);
            targetCharacter.characterLocomotion.Teleport(targetPosition);

            return true;
        }

		#if UNITY_EDITOR

        public static new string NAME = "Character/Teleport";
        private const string NODE_TITLE = "Teleport {0} to {1}";

        public override string GetNodeTitle()
        {
            return string.Format(NODE_TITLE, this.character, this.position);
        }

        #endif
    }
}

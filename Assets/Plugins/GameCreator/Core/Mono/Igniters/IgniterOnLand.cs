namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Characters;

	[AddComponentMenu("")]
    public class IgniterOnLand : Igniter 
	{
		#if UNITY_EDITOR
		public new static string NAME = "Character/On Land";
        #endif

        public TargetCharacter character = new TargetCharacter(TargetCharacter.Target.Player);

        private void Start()
        {
            Character target = this.character.GetCharacter(gameObject);
            if (target != null)
            {
                target.onLand.AddListener(this.OnLand);
            }
        }

        private void OnLand(float verticalSpeed)
        {
            base.ExecuteTrigger(this.character.GetCharacter(gameObject).gameObject);
        }
    }
}
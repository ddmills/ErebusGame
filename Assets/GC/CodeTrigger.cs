namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    [AddComponentMenu("")]
    public class CodeTrigger : Igniter
	{
		#if UNITY_EDITOR
        public new static string NAME = "My Igniters/Code Trigger";
        //public new static string COMMENT = "Uncomment to add an informative message";
        //public new static bool REQUIRES_COLLIDER = true; // uncomment if the igniter requires a collider
        #endif

        private bool triggered = false;

        public bool example = false;

        private void Update()
        {
            if (this.triggered)
            {
                this.ExecuteTrigger(gameObject);
                this.triggered = false;
            }
        }

        public void Trigger()
        {
            this.triggered = true;
        }
	}
}

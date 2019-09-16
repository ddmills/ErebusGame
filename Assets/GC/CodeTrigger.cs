namespace GameCreator.Core
{
    using System;
    using UnityEngine;

    [AddComponentMenu("")]
    public class CodeTrigger : Igniter
    {
        #if UNITY_EDITOR
        public new static string NAME = "Generic/Code";
        public new static string COMMENT = "Triggered manually via code";
        #endif

        private bool triggered = false;
        public CodeTriggerDelegate triggerDelegate;

        void Start()
        {
            triggerDelegate.OnTrigger += OnTrigger;
        }

        private void Update()
        {
            if (this.triggered)
            {
                this.ExecuteTrigger(gameObject);
                this.triggered = false;
            }
        }

        private void OnTrigger(object sender, EventArgs e)
        {
            this.triggered = true;
        }
    }
}

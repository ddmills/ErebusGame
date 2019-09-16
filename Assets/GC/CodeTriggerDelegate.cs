namespace GameCreator.Core
{
    using System;
    using UnityEngine;

    public class CodeTriggerDelegate : MonoBehaviour
    {
        public event EventHandler OnTrigger;

        public void CodeTrigger()
        {
            if (OnTrigger != null)
            {
                OnTrigger(this, new EventArgs());
            }
        }
    }
}

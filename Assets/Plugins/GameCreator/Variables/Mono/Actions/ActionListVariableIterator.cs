namespace GameCreator.Variables
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;

    [AddComponentMenu("")]
	public class ActionListVariableIterator : IAction
	{
        public HelperListVariable listVariables = new HelperListVariable();
        [Space]
        public NumberProperty pointer = new NumberProperty(0);

        // EXECUTE METHOD: ------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            ListVariables list = this.listVariables.GetListVariables(target);
            if (list == null || list.variables.Count == 0) return true;

            int value = this.pointer.GetInt(target);
            list.SetInterator(value);
            return true;
        }

        #if UNITY_EDITOR

        private const string NODE_TITLE = "Set List Variables {0} iterator";
        public static new string NAME = "Variables/List Variables Iterator";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.listVariables
            );
        }

        #endif
    }
}

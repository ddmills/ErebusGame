﻿namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core.Hooks;
    using GameCreator.Characters;
    using GameCreator.Variables;

	[System.Serializable]
	public class TargetCharacter
	{
		public enum Target
		{
			Player,
			Invoker,
			Character,
            LocalVariable,
            GlobalVariable,
            ListVariable
		}

		// PROPERTIES: ----------------------------------------------------------------------------

        public Target target = Target.Character;
        public Character character;
        public HelperLocalVariable local = new HelperLocalVariable();
        public HelperGlobalVariable global = new HelperGlobalVariable();
        public HelperGetListVariable list = new HelperGetListVariable();

        private int cacheInstanceID;
        private Character cacheCharacter;

        // INITIALIZERS: --------------------------------------------------------------------------

        public TargetCharacter() { }

        public TargetCharacter(TargetCharacter.Target target)
        {
            this.target = target;
        }

		// PUBLIC METHODS: ------------------------------------------------------------------------

        public Character GetCharacter(GameObject invoker)
		{
            switch (this.target)
			{
    			case Target.Player :
                    if (HookPlayer.Instance != null) this.cacheCharacter = HookPlayer.Instance.Get<Character>();
    				break;

    			case Target.Invoker:
                    this.cacheCharacter = invoker.GetComponentInChildren<Character>();
    				break;

                case Target.Character:
    				if (this.character != null) this.cacheCharacter = this.character;
    				break;

                case Target.LocalVariable:
                    GameObject localResult = this.local.Get(invoker) as GameObject;
                    if (localResult != null && localResult.GetInstanceID() != this.cacheInstanceID)
                    {
                        this.cacheCharacter = localResult.GetComponentInChildren<Character>();
                    }
                    break;

                case Target.GlobalVariable:
                    GameObject globalResult = this.global.Get(invoker) as GameObject;
                    if (globalResult != null && globalResult.GetInstanceID() != this.cacheInstanceID)
                    {
                        this.cacheCharacter = globalResult.GetComponentInChildren<Character>();
                    }
                    break;

                case Target.ListVariable:
                    GameObject listResult = this.list.Get(invoker) as GameObject;
                    if (listResult != null && listResult.GetInstanceID() != this.cacheInstanceID)
                    {
                        this.cacheCharacter = listResult.GetComponentInChildren<Character>();
                    }
                    break;
            }

            this.cacheInstanceID = (this.cacheCharacter == null
                ? 0
                : this.cacheCharacter.gameObject.GetInstanceID()
            );

			return this.cacheCharacter;
		}

		// UTILITIES: -----------------------------------------------------------------------------

		public override string ToString ()
		{
			string result = "(unknown)";
			switch (this.target)
			{
    			case Target.Player : result = "Player"; break;
    			case Target.Invoker: result = "Invoker"; break;
                case Target.Character:
                    result = (this.character == null 
                        ? "(none)" 
                        : this.character.gameObject.name
                    );
    				break;
                case Target.LocalVariable: result = this.local.ToString(); break;
                case Target.GlobalVariable: result = this.global.ToString(); break;
                case Target.ListVariable: result = this.list.ToString(); break;
            }

			return result;
		}
	}
}
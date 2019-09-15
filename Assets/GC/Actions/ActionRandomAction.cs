namespace GameCreator.Core
{
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Events;
  using GameCreator.Variables;

#if UNITY_EDITOR
  using UnityEditor;
#endif

  [AddComponentMenu("")]
  public class ActionRandomAction : IAction
  {
    public enum Source
    {
      Actions,
      Variable
    }

    public Source source = Source.Actions;
    public Actions actions;

    [VariableFilter(Variable.DataType.GameObject)]
    public VariableProperty variable = new VariableProperty(Variable.VarType.LocalVariable);

    public bool waitToFinish = false;
    private IAction action;

    // EXECUTABLE: ----------------------------------------------------------------------------

    public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
    {
      Actions actionsToChooseFrom = GetActionsList(target);

      if (actionsToChooseFrom == null)
      {
        yield return 0;
      }

      this.action = PickRandom(actionsToChooseFrom.actionsList);

      if (this.action == null)
      {
        yield return 0;
      }

      yield return this.action.InstantExecute(target, actions, index);
    }

    public override void Stop()
    {
      if (this.action != null)
      {
        this.action.Stop();
      }
    }

    private Actions GetActionsList(GameObject target)
    {
      if (this.source == Source.Actions)
      {
        return this.actions;
      }

      if (this.source == Source.Variable)
      {
          GameObject value = this.variable.Get(target) as GameObject;
          if (value != null)
          {
            return value.GetComponent<Actions>();
          }
      }

      return null;
    }

    private IAction PickRandom(IActionsList list)
    {
      int idx = Random.Range(0, list.actions.Length);

      return idx > -1 ? list.actions[idx] : null;
    }

    // +--------------------------------------------------------------------------------------+
    // | EDITOR																																							 |
    // +--------------------------------------------------------------------------------------+

#if UNITY_EDITOR

    public static new string NAME = "General/Execute Random Action";
    private const string NODE_TITLE = "Execute random action {0} {1}";

    // PROPERTIES: ----------------------------------------------------------------------------

    private SerializedProperty spSource;
    private SerializedProperty spActions;
    private SerializedProperty spVariable;
    private SerializedProperty spWaitToFinish;

    // INSPECTOR METHODS: ---------------------------------------------------------------------

    public override string GetNodeTitle()
    {
      string actionsName = (this.source == Source.Actions
        ? (this.actions == null ? "none" : this.actions.name)
        : this.variable.ToString()
      );

      return string.Format(
          NODE_TITLE,
          actionsName,
          (this.waitToFinish ? "and wait" : "")
      );
    }

    protected override void OnEnableEditorChild()
    {
      this.spSource = this.serializedObject.FindProperty("source");
      this.spVariable = this.serializedObject.FindProperty("variable");
      this.spActions = this.serializedObject.FindProperty("actions");
      this.spWaitToFinish = this.serializedObject.FindProperty("waitToFinish");
    }

    protected override void OnDisableEditorChild()
    {
      this.spSource = null;
      this.spVariable = null;
      this.spActions = null;
      this.spWaitToFinish = null;
    }

    public override void OnInspectorGUI()
    {
      this.serializedObject.Update();

      EditorGUILayout.PropertyField(this.spSource);
      switch (this.spSource.enumValueIndex)
      {
        case (int)Source.Actions:
          EditorGUILayout.PropertyField(this.spActions);
          break;

        case (int)Source.Variable:
          EditorGUILayout.PropertyField(this.spVariable);
          break;
      }

      EditorGUILayout.PropertyField(this.spWaitToFinish);

      this.serializedObject.ApplyModifiedProperties();
    }

#endif
  }
}

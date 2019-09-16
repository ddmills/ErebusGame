using UnityEngine;
using GameCreator.Core;

public class BigBoyAnimator : MonoBehaviour
{
    public CodeTriggerDelegate footTriggerDelegate;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Hit()
    {
        // Debug.Log("Hit");
    }

    void FootR()
    {
        if (footTriggerDelegate)
        {
            footTriggerDelegate.CodeTrigger();
        }
    }

    void FootL()
    {
        if (footTriggerDelegate)
        {
            footTriggerDelegate.CodeTrigger();
        }
    }

    void Land()
    {
        // Debug.Log("Land");
    }

    void WeaponSwitch()
    {
        // Debug.Log("WeaponSwitch");
    }
}

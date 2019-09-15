using UnityEngine;
using GameCreator.Core;

public class BigBoyAnimator : MonoBehaviour
{
    public CodeTrigger trigger;

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
        if (this.trigger)
        {
            trigger.Trigger();
        }
    }

    void FootL()
    {
        if (this.trigger)
        {
            trigger.Trigger();
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

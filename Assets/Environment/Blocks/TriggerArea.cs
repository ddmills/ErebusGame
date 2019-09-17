using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This script keeps track of all rigidbodies that enter and leave the trigger collider attached to this gameobject;
//It is used by 'MovingPlatform' to detect and move rigidbodies standing on top of it;
public class TriggerArea : MonoBehaviour {

	public List <Rigidbody> rigidbodiesInTriggerArea = new List<Rigidbody>();

	//Check if the collider that just entered the trigger has a rigidbody attached and add it to the list;
	void OnTriggerEnter(Collider col)
	{
		if(col.attachedRigidbody != null)
		{
			rigidbodiesInTriggerArea.Add(col.attachedRigidbody);
		}
	}

	//Check if the collider that just left the trigger has a rigidbody attached and remove it from the list;
	void OnTriggerExit(Collider col)
	{
		if(col.attachedRigidbody != null)
		{
			rigidbodiesInTriggerArea.Remove(col.attachedRigidbody);
		}
	}

}

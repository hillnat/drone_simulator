using System.Collections;
using UnityEngine;

public class Hoop : MonoBehaviour
{
	[HideInInspector]public Material targetMaterial;
	public int myHoopIndex = -1;
	void Start()
	{
		targetMaterial = GetComponent<MeshRenderer>().material;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<PlayerController>() == null) { return; }
        HoopManager.instance.SetCurrentHoop(myHoopIndex);
    }
}

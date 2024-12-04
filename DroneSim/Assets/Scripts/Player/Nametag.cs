using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nametag : MonoBehaviour
{
	void Update()
	{
		if (GameManager.instance != null && GameManager.instance.localPlayer!=null) { 
			float dist = Mathf.Abs(Vector3.Distance(transform.position, GameManager.instance.localPlayer.transform.position));
			transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one * 8, dist/300f);
			transform.localPosition = new Vector3(0, 1f + (dist / 100f), 0);
			transform.rotation = GameManager.instance.localPlayer.transform.rotation;
		}
	}
}

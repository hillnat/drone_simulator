using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nametag : MonoBehaviour
{
	private PlayerController localPlayer
	{
		get
		{
			if (_localPlayer == null)
			{
				PlayerController[] pcs = FindObjectsOfType<PlayerController>();
				for (int i = 0; i < pcs.Length; i++)
				{
					if (pcs[i].view.IsMine) { _localPlayer = pcs[i]; }
				}
			}
			return _localPlayer;
		}
		set
		{
			_localPlayer = value;
		}
	}
	private PlayerController _localPlayer;

	void Update()
	{
		if (localPlayer != null) { 
			float dist = Mathf.Abs(Vector3.Distance(transform.position, localPlayer.transform.position));
			transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one * 8, dist/300f);
			transform.localPosition = new Vector3(0, 1f + (dist / 100f), 0);
			transform.rotation = localPlayer.transform.rotation;
		}
	}
}

using System;
using System.Collections;
using UnityEngine;

public class Hoop : MonoBehaviour
{
	[Serializable] public enum HoopTypes {Normal, TimerStart, TimerEnd }
	public AudioClip hitClip;
	public HoopTypes hoopType;

	private void OnTriggerEnter(Collider other)
	{
		PlayerController pc;
        other.transform.root.gameObject.TryGetComponent<PlayerController>(out pc);
		if (pc == null || pc.lastHoopHit==this) { return; }
        pc.SetLastHitHoop(this);
        switch (hoopType)
		{
			case HoopTypes.Normal:
                AudioManager.instance.PlaySound(hitClip, transform.position);
                break;
			case HoopTypes.TimerStart:
				pc.SetTimer(true);
                AudioManager.instance.PlaySound(hitClip, transform.position);
                break; 
			case HoopTypes.TimerEnd:
                pc.SetTimer(false);
                AudioManager.instance.PlaySound(hitClip, transform.position);
                break;
			default:
				break;
		}	
    }
}

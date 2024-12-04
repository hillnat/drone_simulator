using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyCamManager : MonoBehaviour
{
    public static SkyCamManager instance;
    public Camera skyCam;
    private Vector2 fovLimits = new Vector2(1, 150);

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        skyCam =GetComponent<Camera>();
        transform.position = new Vector3(0, 75, 0);
    }

    private void FixedUpdate()
    {
        if(GameManager.instance.localPlayer.drone!=null)
        {
            if(!skyCam.enabled) { skyCam.enabled = true; }
            skyCam.fieldOfView = Mathf.Clamp(-(Vector3.Distance(transform.position, GameManager.instance.localPlayer.transform.position)) / 10, fovLimits.x, fovLimits.y);
            transform.LookAt(GameManager.instance.localPlayer.transform.position);
        }
        else if (skyCam.enabled) { skyCam.enabled = false; }
    }
    private void OnDisable()
    {
        skyCam.enabled = false;
    }
    private void OnEnable()
    {
        skyCam.enabled=true;
    }
}

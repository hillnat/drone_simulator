using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyCam : MonoBehaviour
{
    private Camera cam;
    private Vector2 fovLimits = new Vector2(1, 150);
    public PlayerController target;

    private void Awake()
    {
        cam=GetComponent<Camera>();      
    }
    private void Start()
    {
        cam.enabled = target != null;
    }
    private void FixedUpdate()
    {
        if(target != null && target.drone!=null)
        {
            if(!cam.enabled) { cam.enabled = true; }
            cam.fieldOfView = Mathf.Clamp(-(Vector3.Distance(transform.position, target.transform.position)) / 10, fovLimits.x, fovLimits.y);
            transform.LookAt(target.transform.position);
        }
        else if (cam.enabled) { cam.enabled = false; }
    }
    private void OnDisable()
    {
        cam.enabled = false;
    }
    private void OnEnable()
    {
        cam.enabled=true;
    }
}

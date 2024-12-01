using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public static Sun instance;
    private int[] sunPositions = new int[10];
    private int curPosition = 0;
    private float timer=0;
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
    }
    private void Start()
    {
        for (int i = 0;i<sunPositions.Length;i++)
        {
            sunPositions[i] = (360 / sunPositions.Length) * (i + 1);
        }
        curPosition = Random.Range(0, sunPositions.Length);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, sunPositions[curPosition], transform.eulerAngles.z);
    }
    void FixedUpdate()
    {
        timer = Mathf.Clamp01(timer + Time.fixedDeltaTime / 60);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Lerp(sunPositions[curPosition], (curPosition == sunPositions.Length - 1 ? sunPositions[curPosition] + (360 / sunPositions.Length) : sunPositions[curPosition + 1]), timer), transform.eulerAngles.z);
        if (timer >= 1) { timer = 0; curPosition++; if (curPosition >= sunPositions.Length) { curPosition = 0; } }
    }
    public void SkipTime()
    {
        curPosition++;
        if(curPosition >= sunPositions.Length) { curPosition = 0; }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, sunPositions[curPosition], transform.eulerAngles.z);
    }
}

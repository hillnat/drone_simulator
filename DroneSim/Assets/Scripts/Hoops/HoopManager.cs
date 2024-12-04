using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class HoopManager : MonoBehaviour
{
    public static HoopManager instance;
    private List<Hoop> hoops = new List<Hoop>();
    public AudioClip successClip;
    public AudioClip finishedClip;
    public Color successHoopColor = Color.green;
    public Color nextHoopColor = Color.yellow;
    public Color defaultHoopColor = Color.white;
    private int lastHitHoopIndex = -1;
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
    void Start()
    {
        hoops.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Hoop h = transform.GetChild(i).GetComponent<Hoop>();
            if (h == null) { transform.GetChild(i).AddComponent<Hoop>(); }
            h.myHoopIndex = i;
            hoops.Add(h);
        }
        UpdateHoopColors();
    }

    public void SetCurrentHoop(int index) {
        lastHitHoopIndex = index;
        if (lastHitHoopIndex == hoops.Count-1)
        {
            AudioManager.instance.PlaySound(finishedClip, hoops[lastHitHoopIndex].transform.position);
            StartCoroutine(DelayedReset());
        }
        else
        {
            AudioManager.instance.PlaySound(successClip, hoops[lastHitHoopIndex].transform.position);
        }
        UpdateHoopColors();
    }
    public void ResetHoops()
    {
        lastHitHoopIndex = -1;
        UpdateHoopColors();
    }
    public void UpdateHoopColors()
    {
        for (int i = 0; i < hoops.Count; i++)
        {
            if (hoops[i].targetMaterial == null) { continue; }
            if (i <= lastHitHoopIndex)
            {
                hoops[i].targetMaterial.color = successHoopColor;
            }
            else if (i == lastHitHoopIndex + 1)
            {
                hoops[i].targetMaterial.color = nextHoopColor;
            }
            else
            {
                hoops[i].targetMaterial.color = defaultHoopColor;
            }
        }
    }
    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(5);
        ResetHoops();
    }
}

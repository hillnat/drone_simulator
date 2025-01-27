using UnityEngine;

public class SetActiveOnStart : MonoBehaviour
{
    public bool state=false;
    void Start()
    {
        gameObject.SetActive(state);
    }
}

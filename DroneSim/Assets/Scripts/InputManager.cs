using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector3 directionalInputs = Vector3.zero;
    public float throttleInput = 0f;
    public static InputManager instance;
    public bool respawn = false;
    public bool toggleSkycam = false;
    public bool toggleTrail = false;
    public bool color = false;
    public bool clickUp = false;
    public bool openMenu = false;
    public bool setSpawn = false;
    public bool flip = false;
    public bool toggleChat = false;
    public Vector2 mousePosition=Vector2.zero;
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
    private void FixedUpdate()
    {
        if(respawn) { respawn = false; }
        if(toggleSkycam) { toggleSkycam = false; }
        if(toggleTrail) { toggleTrail = false; }
        if(color) { color = false; }
        if(clickUp) { clickUp = false; }
        if(openMenu) { openMenu = false; }
        if(setSpawn) { setSpawn = false; }
        if(flip) { flip = false; }
        if(toggleChat) { toggleChat = false; }
    }
    private void OnXYZ(InputValue iv)
    {
        directionalInputs = iv.Get<Vector3>();
    }
    private void OnThrottle(InputValue iv)
    {
        throttleInput = iv.Get<float>();
    }
    private void OnRespawn() { respawn = true; }
    private void OnToggleSkyCam() { toggleSkycam = true; }
    private void OnSkipTime() { Sun.instance.SkipTime(); }
    private void OnToggleTrail() { toggleTrail = true; }
    private void OnColor() { color = true; }
    private void OnMousePosition(InputValue iv) { mousePosition = iv.Get<Vector2>(); }
    private void OnMenu() { openMenu = true; }
    private void OnSetRespawn() { setSpawn = true; }
    private void OnFlip() { flip = true; }
    private void OnToggleChat() { toggleChat = true; }

}

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
    public bool mouse1 = false;
    public bool mouse1Hold = false;
    public bool mouse2Hold = false;
    public Vector2 mousePosition=Vector2.zero;
    public Vector2 mouseDelta=Vector2.zero;
    public Vector2 scrollDelta=Vector2.zero;
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
    private void LateUpdate()
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
        if(mouse1) { mouse1 = false; }
    }
    private void Update()
    {
        scrollDelta = Input.mouseScrollDelta;
        mouseDelta = Mouse.current.delta.ReadValue();
    }
    private void OnXYZ(InputValue iv)
    {
        directionalInputs = iv.Get<Vector3>();
        directionalInputs.x = Mathf.Clamp(directionalInputs.x, -1f, 1f);
        directionalInputs.y = Mathf.Clamp(directionalInputs.y, -1f, 1f);
        directionalInputs.z = Mathf.Clamp(directionalInputs.z, -1f, 1f);
    }
    private void OnThrottle(InputValue iv)
    {
        float f = iv.Get<float>();
        throttleInput = Mathf.Clamp01((f + 1f) / 2f);//Scale from -1 - 1 to 0 - 1
    }
    private void OnRespawn() { respawn = true; }
    private void OnToggleSkyCam() { toggleSkycam = true; }
    private void OnSkipTime() { SunManager.instance.SkipTime(); }
    private void OnToggleTrail() { toggleTrail = true; }
    private void OnColor() { color = true; }
    private void OnMousePosition(InputValue iv) { mousePosition = iv.Get<Vector2>(); }
    private void OnMenu() { openMenu = true; }
    private void OnSetRespawn() { setSpawn = true; }
    private void OnFlip() { flip = true; }
    private void OnToggleChat() { toggleChat = true; }
    private void OnMouse1() { mouse1 = true; }
    private void OnMouse1Hold(InputValue iv) { mouse1Hold = iv.Get<float>()>0; }
    private void OnMouse2Hold(InputValue iv) { mouse2Hold = iv.Get<float>()>0; }
}

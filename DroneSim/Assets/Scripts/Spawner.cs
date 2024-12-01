using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

public class Spawner : MonoBehaviour
{
    private string[] droneNames = new string[3] { "Basic Drone", "Race Drone", "Tiny Whoop" };

    private int selectedDroneType//index of which drone to spawn
    {
        get { return _typeToSpawn; }
        set { _typeToSpawn = value; UpdateShowcase(); }
    }
    private int _typeToSpawn=0;
    public static Spawner instance;
    private PlayerController pc;
    [SerializeField] private Animator showcaseCameraAnimator;
    [SerializeField]private TMP_Text droneNameText;
    void UpdateShowcase()
    {
        if (showcaseCameraAnimator != null) { showcaseCameraAnimator.SetInteger("showcaseIndex", _typeToSpawn); }
        droneNameText.text = droneNames[selectedDroneType];
    }
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Setup");
        }
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 50;
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        GameManager.spawn = transform.position;   
        selectedDroneType = 0;
        pc = PhotonNetwork.Instantiate("player", GameManager.spawn, GameManager.spawnRotation).GetComponent<PlayerController>();

    }
    public void UICALLBACK_SpawnDrone()
    {
        pc.view.RPC("SetDroneType", RpcTarget.AllBufferedViaServer, selectedDroneType);
        gameObject.SetActive(false);
    }
    public void UICALLBACK_ChangeDroneIndex(int delta)
    {
        int newIndex=selectedDroneType + delta;
        if (newIndex >= droneNames.Length) { newIndex = 0; }
        if (newIndex < 0) { newIndex = droneNames.Length-1; }
        selectedDroneType = newIndex;
    }
}

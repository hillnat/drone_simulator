using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public float time = 0f;
    public LevelRules levelRules;
    public Camera levelCamera;
    public PlayerController localPlayer;
    public GameObject spawnIndicator;
    #region Unity Callbacks
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Setup");
        }
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 50;
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
        SetSpawn(transform.position, transform.rotation);
        localPlayer = PhotonNetwork.Instantiate("player", levelRules.spawn, levelRules.spawnRotation).GetComponent<PlayerController>();
        SettingsManager.instance.SetDefaultValues();
    }

    private void Update()
    {
        time+= Time.deltaTime;
    }
    #endregion
    public void SetSpawn(Vector3 position, Quaternion rotation)
    {
        levelRules.spawn = position;
        levelRules.spawnRotation = rotation;
        spawnIndicator.transform.position = position;
        spawnIndicator.transform.rotation = rotation;
    }
}

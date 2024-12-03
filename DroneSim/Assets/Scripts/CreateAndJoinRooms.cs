using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    private string targetLevel = "LevelCity";
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Setup");
        }
    }
    public void UICALLBACK_JoinLevelField()
    {
        targetLevel = "LevelField";
        StartCoroutine(TryCreateJoin());
    }
    public void UICALLBACK_JoinLevelFactory()
    {
        targetLevel = "LevelFactory";
        StartCoroutine(TryCreateJoin());
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(targetLevel);
    }
    IEnumerator TryCreateJoin()
    {
        PhotonNetwork.CreateRoom(targetLevel);
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinRoom(targetLevel);
    }

}

using UnityEngine;
using Photon.Pun;

public class PlayerNameInputManager : MonoBehaviour
{
    public bool nameIsNull = true;
    public void SetPlayerName(string playername)
    {
        if (string.IsNullOrEmpty(playername))
        {
            Debug.Log("player name is empty");
            nameIsNull = true;
            return;
        }
        nameIsNull = false;
        PhotonNetwork.NickName = playername;
    } 
}

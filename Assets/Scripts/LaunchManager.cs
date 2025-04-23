using UnityEngine;
using Photon.Pun;

public class LaunchManager : MonoBehaviourPunCallbacks
{
    UIManager uIManager;
    [SerializeField] private PlayerNameInputManager playerNameInputManager;
    private void Start()
    {
        uIManager = GetComponent<UIManager>();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.NickName + " CONNECTED to the photon server");
        uIManager.GoToMainMenu();
    }
    public override void OnConnected()
    {
        Debug.Log("CONNECTED to internet"); 
    }

    public void ConnectToPhotonServer()
    {
        if (!playerNameInputManager.nameIsNull)
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

    }
}

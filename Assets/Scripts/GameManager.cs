using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab!=null)
            {
                int randomPoint = Random.Range(0,0);
                PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomPoint, 1f, randomPoint), Quaternion.identity);
            }
            else
            {
                Debug.Log("Place playerPrefab!");
            }
        }
    }


}

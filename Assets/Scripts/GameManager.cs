using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab!=null)
            {
                int randomPoint = Random.Range(-2, 2);
                PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomPoint, 1f, randomPoint), Quaternion.identity);
            }
            else
            {
                Debug.Log("Place playerPrefab!");
            }
        }       
    }

}

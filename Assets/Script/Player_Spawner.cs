using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Spawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private GameObject spawnedOperatorPrefab;
    private bool OperatorSpawned = false;
    // Start is called before the first frame update
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom <- PlayerSpawner");
        //spawn the player with the prefab
        base.OnJoinedRoom();

        Debug.Log(" #1 OperatorSpawned =" + OperatorSpawned.ToString());
        if(!OperatorSpawned){
            Debug.Log("Instantiation of Network Operator");
            spawnedOperatorPrefab = PhotonNetwork.Instantiate("Network Operator", new Vector3(0,0,0), transform.rotation);
            OperatorSpawned = !OperatorSpawned;
            Debug.Log(" #2 OperatorSpawned =" + OperatorSpawned.ToString());
        } else {
            Debug.Log("Instantiation of Network Player <- PlayerSpawner");
            spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", new Vector3(0,0,0), transform.rotation);
        }
    }

    public override void OnLeftRoom()
    {
        //destroy the operator & player prefab
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
        PhotonNetwork.Destroy(spawnedOperatorPrefab);
    }
}

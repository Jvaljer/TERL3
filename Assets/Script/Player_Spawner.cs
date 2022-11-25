using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Spawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private GameObject spawnedOperatorPrefab;
    // Start is called before the first frame update
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom <- PlayerSpawner");

        int countP = PhotonNetwork.LocalPlayer.ActorNumber;
        if(countP==1){
            //spawn the operator with the prefab
            Debug.Log("Instantiation of Network Operator");
            base.OnJoinedRoom();
            spawnedOperatorPrefab = PhotonNetwork.Instantiate("Network Operator", new Vector3(0,0,0), transform.rotation);
        } else {
            //spawn the player with the prefab
            base.OnJoinedRoom();
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

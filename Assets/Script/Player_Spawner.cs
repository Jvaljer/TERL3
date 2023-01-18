using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Spawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private GameObject spawnedOperatorPrefab;
    public bool withOperator  = false;

    // Start is called before the first frame update
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom <- PlayerSpawner");

        int ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("with Operator -> " + withOperator);
        if(ActorNumber==1){
            // here we wanna offer the choice of going inside the Room WITH or WITHOUT an operator
            if(withOperator){
                //spawn the operator with the prefab
                base.OnJoinedRoom();
                Debug.Log("Instantiation of Network Operator");
                spawnedOperatorPrefab = PhotonNetwork.Instantiate("Network Operator", new Vector3(0,0,0), transform.rotation);
            } else {
                //spawn the player with the prefab
                base.OnJoinedRoom();
                Debug.Log("Instantiation of Network Player <- PlayerSpawner");
                spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", new Vector3(0,0,0), transform.rotation);
            }
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

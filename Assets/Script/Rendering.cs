using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;

public class Rendering : MonoBehaviourPunCallBacks {

    public class MyCard : MonoBehaviour {
        //specific card attributes
        private GameObject go_card = null;
        private string pos_tag = "";
        private PhotonView pv;
        private Transform parent;

        public MyCard(Texture2D tex, Trasnform wall){
            GameObject go_card = PhotonNetwork.InstantiateRoomObject("Card", wall.position,wall.position,0,null);
            go_card.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
            parent = wall;
            pv = go_card.GetPhotonView();
            pos_tag = "onWall";
        }
    }

    //trash components
    private GameObject sphere1;
    private GameObject sphere2;
    private GameObject sphere3;
    private GameObject sphere4;

    //room walls
    private Transform BWall;
    private Tranform LWall;
    private Transform RWall;

    //room attributes
    private Trasnform card_area;

    //cards attributes
    private List<GameObject> card_list;
    private List<Vector3> card_init_pos;

    //cards informations & statements
    private object[] textures;
    private static int card_per_wall = 20;
    private bool cards_created = false;
    private bool cards_destroyed = false;

    //participant to load informations
    private string participant = "";
    private string group;
    private int fst_trial_nb;

    //experiment attributes & statements
    private Experiment experiment;
    private bool expe_running = false;
    private bool trial_running = false;

    //demo statements
    private bool demo_running = false;
    private bool demo_created = false;
    private bool demo_destroyed = false;
    
    //Awake Unity method that is called before anything else
    void Awake(){
        sphere1.SetActive(false);
        sphere2.SetActive(false);
        sphere3.SetActive(false);
        sphere4.SetActive(false);
    }

    //Update method from Unity, called once per frame
    void Update(){
        ExpeCheckup();
    }

    //every other method
    public void ExpeCheckup(){
        if(expe_running){
            expe_running = experiment.expe_running;
            trial_running = experiment.trial_running;
        }

        if(trial_running && expe_running){
            photonView.RPC("CurrentTrialConditionsCheck", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    //PunRPC methods
    [PunRPC]
    void CurrentTrialConditionsCheck(){
        experiment.current_trial.CheckConditions();
    }

    [PunRPC]
    public void NextTrial(){
        experiment.NextTrial();
    }
}
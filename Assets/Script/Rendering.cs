using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;

public class Rendering : MonoBehaviourPunCallbacks {

    public class MyCard : MonoBehaviour {
        //specific card attributes
        private GameObject go_card = null;
        private string pos_tag = "";
        public PhotonView pv;
        private Transform parent;

        public MyCard(Texture2D tex, Transform wall){
            GameObject go_card = PhotonNetwork.InstantiateRoomObject("Card", wall.position, wall.rotation, 0, null);
            go_card.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
            parent = wall;
            pv = go_card.GetPhotonView();
            pos_tag = "onWall";
        }
    }

    //trash components
    private GameObject trash_sphere1;
    private GameObject trash_sphere2;
    private GameObject trash_sphere3;
    private GameObject trash_sphere4;

    //room walls
    private Transform BackWall;
    private Transform LeftWall;
    private Transform RightWall;

    //room attributes
    public Transform card_area;

    //cards attributes
    public List<GameObject> card_list;
    private List<Vector3> card_init_pos;

    //cards informations & statements
    private object[] textures;
    public static int card_per_wall = 20;
    private bool cards_created = false;
    private bool cards_destroyed = false;

    //participant to load informations
    private string participant = "";
    private string group;
    private int fst_trial_nb;

    //experiment attributes & statements
    public Experiment experiment;
    private bool expe_running = false;
    private bool trial_running = false;
    private bool training = false;

    //demo statements
    private bool demo_running = false;
    private bool demo_created = false;
    private bool demo_destroyed = false;
    private bool with_ope = (GameObject.Find("Network Operator(Clone)")==null) ;
    
    //Awake Unity method that is called before anything else
    void Awake(){
        trash_sphere1.SetActive(false);
        trash_sphere2.SetActive(false);
        trash_sphere3.SetActive(false);
        trash_sphere4.SetActive(false);
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

    //operatoring method 
    public void SpacePressed(){
        if(!expe_running){
            if(demo_running || demo_created){
                demo_running = false;
                photonView.RPC("ResetCards", Photon.Pun.RpcTarget.All);
                photonView.RPC("ActivateCards", Photon.Pun.RpcTarget.All);
                photonView.RPC("StartExperiment", Photon.Pun.RpcTarget.All);
                expe_running = true;
            } else {
                InstantiateCards();
                CreateCards();
                photonView.RPC("StartExperiment", Photon.Pun.RpcTarget.All);
                expe_running = true;
            }
        } else if(expe_running && !trial_running){
            photonView.RPC("NextTrial", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    public void EPressed(){
        //ends the expe
        photonView.RPC("DeleteCards", Photon.Pun.RpcTarget.AllBuffered);
        photonView.RPC("EndExpe", Photon.Pun.RpcTarget.AllBuffered);
    }

    public void TPressed(){
        //we wanna teleport both player to the center of the room ?
        experiment.ctrl_tp.photon_view.RPC("TpToOther", Photon.Pun.RpcTarget.Others);
    }

    public void DPressed(){
        if(!demo_created){
            InstantiateCards();
            CreateCards();
            demo_created = true;
            demo_running = true;
        } else if(demo_running && !demo_destroyed){
            photonView.RPC("ResetCards", Photon.Pun.RpcTarget.All);
            demo_destroyed = true;
            demo_running = false;
        } else if(!demo_running && demo_created){
            photonView.RPC("ActivateCards", Photon.Pun.RpcTarget.All);
            demo_running = true;
        } else if(demo_running && demo_destroyed){
            photonView.RPC("ResetCards", Photon.Pun.RpcTarget.All);
            demo_running = false;
        }
    }

    public string SetParticipantName(){
        string str;
        switch (PhotonNetwork.LocalPlayer.ActorNumber){
            case 1:
                if(with_ope){
                    str = "ope";
                } else {
                    str = "p01";
                }
                break;
            case 2:
                if(with_ope){
                    str = "p01";
                } else {
                    str = "p02";
                }
                break;
            case 3:
                str = "p02";
                break;
            default:
                str = "";
                break;
        }
        return str;
    }

    public void InstantiateCards(){
        //gloabally you'll never have training==true it is in order to do some testing
        if(training){
            textures = Resources.LoadAll("dixit_training/", typeof(Texture2D));
        } else {
            textures = Resources.LoadAll("dixit_all/", typeof(Texture2D));
        }
    }

    public void CreateCards(){
        Transform wall_object;
        int on_wall_pos;
        string wall_label;

        for(int i=0; i< card_per_wall*3; i++){
            if(i< textures.Length){
                if(i< card_per_wall){
                    wall_object = LeftWall;
                    wall_label = "L";
                    on_wall_pos = i;
                } else if(i< 2*card_per_wall){
                    wall_object = BackWall;
                    wall_label = "B";
                    on_wall_pos = i - card_per_wall;
                } else {
                    wall_object = RightWall;
                    wall_label = "R";
                    on_wall_pos = i - 2*card_per_wall;
                }

                Texture2D texture = (Texture2D)textures[i];
                MyCard card = new MyCard(texture, wall_object);
                photonView.RPC("AddCardToList", Photon.Pun.RpcTarget.AllBuffered);
                card.pv.RPC("LoadCard", Photon.Pun.RpcTarget.AllBuffered, card.pv.ViewID, wall_object.GetComponent<PhotonView>().ViewID, on_wall_pos, i);
                photonView.RPC("AddPosToList", Photon.Pun.RpcTarget.AllBuffered);

                GameObject card_object = PhotonView.Find(card.pv.ViewID).gameObject;
                Vector3 card_scale = card_object.transform.localScale;
            } else {
                break;
            }
            cards_created = true;
        }
    }

    //Photon PunRPC methods
    [PunRPC]
    void CurrentTrialConditionsCheck(){
        experiment.current_trial.CheckConditions();
    }

    [PunRPC]
    public void NextTrial(){
        experiment.NextTrial();
    }

    [PunRPC]
    public void EndExpe(){
        experiment.End();
        expe_running = false;
        trial_running = false;
    }

    [PunRPC]
    public void DeleteCards(){
        PhotonView card_pv = null;
        //here we destroy all the room's card
        foreach(GameObject card in card_list){
            if(card != null){
                card_pv = card.GetComponent<PhotonView>();
                int card_owner = card_pv.Owner.ActorNumber;
                int master_id = PhotonNetwork.MasterClient.ActorNumber;

                if(card_owner != master_id){
                    if(PhotonNetwork.IsMasterClient){
                        card_pv.RequestOwnership();
                        card_pv.TransferOwnership(PhotonNetwork.MasterClient);
                    }
                }
            }
            PhotonNetwork.Destroy(card_pv);
            cards_destroyed = true;
        }
    }

    [PunRPC]
    public void ResetCards(){
        Transform wall;
        GameObject card;
        //PhotonView card_pv;
        //int pv;
        for(int i=0; i<60; i++){
            card = card_list[i];

            if(i < card_per_wall){
                wall = LeftWall;
            } else if(i < 2* card_per_wall){
                wall = BackWall;
            } else {
                wall = RightWall;
            }

            card.transform.parent = wall;
            card.transform.rotation = wall.rotation;
            card.transform.localScale = new Vector3(0.033f, 0.239f, 1.0f);

            if(card_init_pos[i]!=null){
                card.transform.localPosition = card_init_pos[i];
            }
    
            card.SetActive(false);
        }
    }

    [PunRPC]
    public void StartExperiment(string grp, int nb){
        participant = SetParticipantName();
        experiment = new Experiment(participant, grp, nb, card_list, with_ope);

        if(experiment.current_trial.collab_env == "C"){
            GameObject sound = GameObject.Find("Network Voice");
            sound.SetActive(false);
        }
    }

    [PunRPC]
    public void ActivateCards(){
        //GameObject card_pv;
        //int pv;
        foreach(GameObject card in card_list){
            card.SetActive(true);
        }
    }

    [PunRPC]
    public void AddPosToList(int PV){
        GameObject card = PhotonView.Find(PV).gameObject;
        if(demo_created){
            int index = 0;
            foreach(GameObject ob in card_list){
                if(ob==card){
                    break;
                }
                index++;
            }
            card_init_pos[index] = card.transform.localPosition;
        } else {
            card_init_pos.Add(card.transform.localPosition);
        }
    }

    [PunRPC]
    public void AddCardToList(int PV){
        GameObject card = PhotonView.Find(PV).gameObject;
        if(demo_created){
            int index = 0;
            foreach(GameObject ob in card_list){
                if(ob==null){
                    break;
                }
                index++;
            }
            card_list[index] = card;
        } else {
            card_list.Add(card);
        }
    }

    [PunRPC]
    public void UndoCard(int ob_id, int trash_nb){
        PhotonView.Find(ob_id).gameObject.SetActive(true);
        switch (trash_nb){
            case 0:
                trash_sphere1.SetActive(false);
                break;
            case 1:
                trash_sphere1.SetActive(false);
                break;
            case 2:
                trash_sphere2.SetActive(false);
                break;
            case 3:
                trash_sphere3.SetActive(false);
                break;
            case 4:
                trash_sphere4.SetActive(false);
                break;
            default:
                break;
        }
    }

    [PunRPC]
    public void DestroyCard(int ob_id, int trash_nb){
        PhotonView.Find(ob_id).gameObject.SetActive(false);
        switch (trash_nb){
            case 0:
                trash_sphere1.SetActive(true);
                break;
            case 1:
                trash_sphere1.SetActive(true);
                break;
            case 2:
                trash_sphere2.SetActive(true);
                break;
            case 3:
                trash_sphere3.SetActive(true);
                break;
            case 4:
                trash_sphere4.SetActive(true);
                break;
            default:
                break;
        }
    }
}
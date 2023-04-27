using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Valve.VR;
using Photon.Pun;

public class DragDrop : MonoBehaviourPun {
    //intersection ray/object attributes
    private GameObject pointer;
    private bool has_pos = false;
    private RaycastHit hit;

    //trigger boolean
    private SteamVR_Action_Boolean trigger = SteamVR_Input.GetBooleanAction("InteractUI");

    //pose
    private SteamVR_Behaviour_Pose pose;

    //machine state
    private bool moving = false;
    private bool wait = false;
    private bool long_click = false;
    public bool trial_start_cond = false;
    private float timer = 0;

    //click attributes
    private Vector3 click_coord;
    private Vector3 click_forward;

    //object to move
    private GameObject obj;

    //undoed objects
    private List<GameObject> undo_objects;

    //room attributes & experiment
    private GameObject room;
    private Rendering room_render;
    private Transform FrontWall;
    private Transform LeftWall;
    private Transform RightWall;
    private Experiment experiment;

    //card texture
    private Texture texture;

    //ray attributes
    private string ray_name;
    private string wall_name;

    //referred player & attributes
    private GameObject player;
    private Teleporter teleporter;

    //selected cards attributes
    private GameObject empty_to_move_card;
    private bool card_from_group = false;
    private Vector3 empty_local_scale;
    private string move_mode = "Drag";

    //class available predicate
    private bool available = true;

    //Unity Awake method, called before any 'Start' method, used here as an initializer
    void Awake(){
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        room = GameObject.Find("/Room");
        room_render = room.GetComponent<Rendering>();
        player = GameObject.Find("Network Player(Clone)");
        teleporter = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
    }

    //Unity Update method, called once per frame
    void Update(){
        if(experiment==null && room_render.experiment!=null){
            experiment = GameObject.Find("/Room").GetComponent<Rendering>().experiment;
            available = false;
        }

        if(available){
            has_pos = UpdatePointer();

            if(trigger.GetStateUp(pose.inputSource)){
                if(wait && obj!=null){
                    //just a click -> tagging object
                    player = GameObject.Find("Network Player(Clone)");
                    player.GetComponent<PhotonView>().RPC("ChangeTag", Photon.Pun.RpcTarget.AllBuffered, hit.transform.gameObject.GetComponent<PhotonView>().ViewID);
                }

                if(empty_to_move_card!=null){
                    EmptyUpdate();
                }

                moving = false;
                obj = null;
                wait = false;
                long_click = false;
                timer = 0;
            }

            if(trigger.GetStateDown(pose.inputSource) && has_pos){
                HitCheck(hit.transform.tag);

                click_coord = hit.transform.position;
                click_forward = transform.forward;
                wait = true;
                timer = Time.time;
            }

            if(obj!=null && wait && Vector3.Angle(click_forward, transform.forward) > 2){
                moving = true;
                wait = false;
            }

            has_pos = UpdatePointer();

            if(obj!=null && has_pos && hit.transform.tag=="trash"){
                TrashClickDestroy();
            }

            if(undo_objects!=null && has_pos && hit.transform.tag=="trash" && trigger.GetStateDown(pose.inputSource)){
                TrashClickUndo();
            }

            if(wait && (Time.time - timer > 1.5f)){
                long_click = true;
                wait = false;
            }

            has_pos = UpdatePointer();

            if(long_click && has_pos && (hit.transform.tag=="Wall" || hit.transform.tag=="Card")){
                MoveCardWithTag();
            }

            if(trigger.GetState(pose.inputSource)){
                MoveCard();
            }
        }
    }

    //other methods
    private bool UpdatePointer(){
        Ray ray = new Ray(transform.position, transform.forward);

        //we wanna check if there's a hit 
        if(Physics.Raycast(ray, out hit)){
            string hit_tag = hit.transform.tag;
            if(hit_tag=="MoveCtrlJoy" || hit_tag=="MoveCtrlDrag" || hit_tag=="MoveCtrlTP" || hit_tag=="Tp" || hit_tag=="TpLimit" || hit_tag=="Card" || hit_tag=="Wall" || hit_tag=="Tag" || hit_tag=="Trash"){
                pointer.transform.position = hit.point;
                return true;
            }
        }
        return false;
    }

    private void EmptyUpdate(){
        int children = empty_to_move_card.transform.childCount;

        for(int i=0; i<children; i++){
            if(empty_to_move_card.GetComponent<PhotonView>().IsMine){
                photonView.RPC("WallSwitch", Photon.Pun.RpcTarget.AllBuffered, empty_to_move_card.transform.parent.name, empty_to_move_card.transform.GetChild(0).GetComponent<PhotonView>().ViewID);
            }
        }

        photonView.RPC("DestroyEmpty", Photon.Pun.RpcTarget.All, empty_to_move_card.GetComponent<PhotonView>().ViewID);
        card_from_group = false;
    }

    private void HitCheck(string tag){
        switch (tag){
            case "Card":
                hit.transform.gameObject.GetComponent<PhotonView>().RequestOwnership();
                trial_start_cstr = true;
                StartCoroutine(SwitchConstraint());
                obj = hit.transform.gameObject;
                break;

            case "MoveCtrlTP":
                move_mode = "Tp";
                break;

            case "MoveCtrlJoy":
                move_mode = "Joy";
                break;
            
            case "MoveCtrlDrag":
                move_mode = "Drag";
                break;
            
            default:
                break;
        }
    }

    private void TrashClickDestroy(){
        photonView.GetComponent<PhotonView>().RPC("AddUndoObj", Photon.Pun.RpcTarget.All, obj.GetComponent<PhotonView>().ViewID);
        player.GetComponent<PhotonView>().RPC("RemoveTag", Photon.Pun.RpcTarget.All, obj.GetComponent<PhotonView>().ViewID, undo_objects.Count);
        room.GetComponent<PhotonView>().RPC("DestroyCard", Photon.Pun.RpcTarget.All, obj.GetComponent<PhotonView>().ViewID, undo_objects.Count);
        obj = null;
    }

    private void TrashClickUndo(){
        GameObject tmp = undo_objects[undo_objects.Count-1];
        room.GetComponent<PhotonView>().RPC("UndoCard", Photon.Pun.RpcTarget.All, tmp.GetComponent<PhotonView>().ViewID, undo_objects.Count);
        photonView.GetComponent<PhotonView>().RPC("RemoveUndoObj", Photon.Pun.RpcTarget.All, tmp.GetComponent<PhotonView>().ViewID);
    }

    private void MoveCardWithTag(){
        string wall_str = "";

        if(hit.transform.tag=="Card"){
            if(hit.transform.parent.tag!="Wall"){
                //here we wanna get the wall (not the empty) (that's why parent.parent)
                wall_str = hit.transform.parent.parent.name;
            } else {
                wall_str = hit.transform.parent.name;
            }
        } else {
            wall_str = hit.transform.name;
        }

        if(empty_to_move_card==null){
            empty_to_move_card = PhotonNetwork.Instantiate("EmptyToMoveCard", transform.position, transform.rotation);
            photonView.RPC("InitEmpty", Photon.Pun.RpcTarget.All, player.GetComponent<NetworkPlayer>().ray_name, wall_str, empty_to_move_card.GetComponent<PhotonView>().ViewID);
        }
        TeleportCard(player.GetComponent<NetworkPlayer>().ray_name, wall_str);
    }

    private void MoveCard(){
        float x, y, z;
        Vector3 vec = RightWall.localScale;
        Vector3 point = RightWall.position;

        x = pointer.transform.position.x / vec.x;
        y = (pointer.transform.position.y - point.y) / vec.y;
        z = -0.02f;

        if(!has_pos){
            return;
        }
        if(moving && obj!=null){
            string parent_name = obj.transform.parent.name;
            if(parent_name=="LeftWall"){
                if(hit.transform.name=="FrontWall"){
                    wall_name = hit.transform.name;
                }
                obj.transform.localPosition = new Vector3(pointer.transform.position.z / vec.x, y, z);
            } else if(parent_name=="FrontWall"){
                if(hit.transform.name=="LeftWall"){
                    wall_name = hit.transform.name;
                }
                if(hit.transform.name=="RightWall"){
                    wall_name = hit.transform.name;
                }
                obj.transform.localPosition = new Vector3(x, y, z);
            } else if(parent_name=="RightWall"){
                if(hit.transform.name=="FrontWall"){
                    wall_name = hit.transform.name;
                }
                obj.transform.localPosition = new Vector3(-pointer.transform.position.z / vec.x, y, z);
            }

            if(wall_name!= ""){
                photonView.RPC("WallSwitch", Photon.Pun.RpcTarget.All, wall_name, obj.GetComponent<PhotonView>().ViewID);
                wall_name = "";
            }
        }
    }

    private void TeleportCard(string ray_name, string wall_str){
        if(ray_name=="transparent (Instance)"){
            return;
        }
        List<GameObject> card_list = room.GetComponent<Rendering>().card_list;

        float width, height;
        float div = 2* 1000f;

        Transform wall = WallCheck(wall_str);

        Vector3 vec = wall.localScale;
        height = texture.height / div;
        width = (texture.width/div) * (vec.y/vec.x);
        Vector3 point = wall.position;

        card_from_group = true;
        if(wall_name!=empty_to_move_card.transform.parent.name){
            photonView.RPC("WallSwitch", Photon.Pun.RpcTarget.All, wall_str, empty_to_move_card.GetComponent<PhotonView>().ViewID);
        }

        foreach(GameObject card in card_list){
            if(card.transform.GetChild(0).GetComponent<Renderer>().material.name==ray_name){
                photonView.RPC("ScaleCard", Photon.Pun.RpcTarget.All, card.transform.GetComponent<PhotonView>().ViewID, width, height);
            }
        }

        float x = wall.InverseTransformPoint(pointer.transform.position).x;
        float y = wall.InverseTransformPoint(pointer.transform.position).y;

        photonView.RPC("MoveEmpty", Photon.Pun.RpcTarget.All, empty_to_move_card.GetComponent<PhotonView>().ViewID, x, y);

    }

    public Transform WallCheck(string str){
        Transform wall;
        switch (str){
            case "FrontWall":
                wall = FrontWall;
                break;
            case "LeftWall":
                wall = LeftWall;
                break;
            case "RightWall":
                wall = RightWall;
                break;
            default:
                wall = null;
                break;
        }
        return wall;
    }

    //IEnumerators 
    private IEnumerator SwitchConstraint(){
        yield return new WaitForSeconds(1);
        trial_start_cstr = false;
    }

    //PunRPC methods
    [PunRPC]
    public void WallSwitch(string tag_name, int ob_id){
        float width, height;
        float div = 2* 1000f;
        Transform wall = WallCheck(tag_name);

        Vector3 vec = wall.localScale;
        height = texture.height / div;
        width = (texture.width / div) * (vec.y / vec.x);

        PhotonView.Find(ob_id).gameObject.transform.parent = wall;
        PhotonView.Find(ob_id).gameObject.transform.rotation = wall.rotation;
        PhotonView.Find(ob_id).gameObject.transform.localScale = new Vector3(width, height, 1.0f);
    }

    [PunRPC]
    public void AddUndoObj(int ob_id){
        undo_objects.Add(PhotonView.Find(ob_id).gameObject);
    }

    [PunRPC]
    public void RemoveUndoObj(int ob_id){
        undo_objects.Remove(PhotonView.Find(ob_id).gameObject);
    }

    [PunRPC]
    public void ScaleCard(int ob_id, float w, float h){
        PhotonView.Find(ob_id).gameObject.transform.localScale = new Vector3(w, h, 1.0f);
    }

    [PunRPC]
    public void MoveEmpty(int ob_id, float x, float y){
        PhotonView.Find(ob_id).gameObject.transform.localPosition = new Vector3(x, y, 0f);
    }

    [PunRPC]
    public void InitEmpty(string r_name, string wall_str, int ob_id){
        Transform wall = WallCheck(wall_str);

        PhotonView.Find(ob_id).transform.parent = wall;
        PhotonView.Find(ob_id).transform.rotation = wall.rotation;
        PhotonView.Find(ob_id).transform.localPosition = new Vector3(0, 0, 0);
        PhotonView.Find(ob_id).transform.localScale = new Vector3(1, 1, 1);

        Vector3 vec = wall.localScale;
        float width, height;
        float div = 2* 1000f;

        height = texture.height/div;
        width = (texture.width/div) * (vec.y/vec.x);

        List<GameObject> card_list = room.GetComponent<Rendering>().card_list;
        int nb_card_to_tp = 0;

        foreach(GameObject card in card_list){
            if(card.transform.GetChild(0).GetComponent<Renderer>().material.name==r_name){
                card.GetComponent<PhotonView>().RequestOwnership();
            }
            float x, y = 0;

            if(nb_card_to_tp%2==0){
                y = -1.25f*height;
                x = 0;
            } else {
                x = width/2;
            }
            int card_id = card.GetComponent<PhotonView>().ViewID;
            PhotonView.Find(card_id).gameObject.transform.parent = PhotonView.Find(ob_id).transform;
            PhotonView.Find(card_id).gameObject.transform.rotation = PhotonView.Find(ob_id).transform.rotation;
            PhotonView.Find(card_id).gameObject.transform.localPosition = new Vector3(width*nb_card_to_tp/2+x, y, -0.02f);
        }
    }

    [PunRPC]
    public void DestroyEmpty(int ob_id){
        Destroy(PhotonView.Find(ob_id).gameObject);
    }
}
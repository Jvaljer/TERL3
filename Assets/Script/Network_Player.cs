using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;

public class Network_Player : MonoBehaviourPun
{
    // empty
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform torse;
    public Transform ray;
    public Transform pos;

    //avatar
    public Transform headSphere;
    public Transform leftHandSphere;
    public Transform rightHandSphere;
    public Transform palette;
    public Transform rayCast;
    public Transform circle;

    //Tag color
    public Material blue;
    public Material green;
    public Material white;
    public Material red;
    public Material none;
    public Material lightRed;

    //camera tracker
    private GameObject cameraRig;
    private GameObject headset;
    private GameObject right;
    private GameObject left;

    //room + wall
    private GameObject room;

    //operator's stuff
    private GameObject ope;
    private Network_Operator ope_Script;
    private rendering render;

    private bool punview = true;
    //private PhotonView photonView;

    private RaycastHit hit;
    public string nameR="";
    private string nameT="";
    private SteamVR_Behaviour_Pose m_pose = null;
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

    private bool synctag = true;
    private bool isOtherSynced;
    private string moveMode = "drag"; // "TP" | "joy" | "sync"

    Expe expe; // the aim is to remove this, and directly refers to ope.expe instead -> wanna centralize the expe management 
    private bool is_operator;

    void Start() {
        //getting the operator 
        if(GameObject.Find("/Network Operator(Clone)")!=null){
            Debug.Log("operator found -> implementing 3 computers model");
            ope = GameObject.Find("/Network Operator(Clone)");
            ope_Script = ope.GetComponent<Network_Operator>();


            //these 2 lines aren't working, returning NULL on both
            room = ope_Script.room;
            render = ope_Script.render;

            is_operator = false;
        } else {
            Debug.Log("no operator -> implement 2 players functionnability");
            room = GameObject.Find("/Salle");
            render = room.GetComponent<rendering>();
            if(photonView.IsMine && PhotonNetwork.LocalPlayer.ActorNumber==1){
                is_operator = true;
            } else {
                is_operator = false;
            }
        }
        
        //room + wall + camera
        cameraRig = GameObject.Find("/[CameraRig]");
        headset = GameObject.Find("Camera (eye)");
        right = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)");
        left = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (left)");

        m_pose = right?.GetComponent<SteamVR_Behaviour_Pose>();

        leftHand.gameObject.SetActive(true);
        
        palette.gameObject.SetActive(true); //abling / disabling the left hand palette 

        if (photonView.IsMine) {
            //don't show my avatar
            Debug.Log("PhotonView.IsMine (Start)" + PhotonNetwork.LocalPlayer.ActorNumber.ToString() );
            leftHand.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);
            torse.gameObject.SetActive(false);
        }
        nameR = rayCast.GetComponent<Renderer>().material.name.ToString();
        
    }

    // Update is called once per frame
    void Update() {
        if(is_operator){
            OperatorActions();
        }

        if (expe == null) {
            //expe = GameObject.Find("/Salle").GetComponent<rendering>().expe;
            expe = render?.expe;
            if(expe!=null){
                //we are disabling the possibility to change the MoveMode by itself (triggering the palette)
                palette.gameObject.SetActive(false);
                Debug.Log("expe has been found");
            }
        }

        //making it impossible to move in any possible way if the expe is paused
        /*if(expe.paused){
            right.gameObject.SetActive(false);
        } else {
            right.gameObject.SetActive(true);
        } */

        bool? synctagTest = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)")?.GetComponent<Teleporter>().synctag;
        bool? isOtherSyncedTest = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)")?.GetComponent<Teleporter>().isOtherSynced;

        synctag = synctagTest == null ? false : (bool)synctagTest;
        isOtherSynced = isOtherSyncedTest == null ? false : (bool)isOtherSyncedTest;

        Ray ray = new Ray(right.transform.position, right.transform.forward);

        if (photonView.IsMine) {
            // end the position and rotation over the network
            if(punview){
                Debug.Log("photonView.IsMine (Update)");
                punview = false;
            }
            //Ray ray = new Ray(right.transform.position, right.transform.forward);
            MapPosition();
        }

        if (Physics.Raycast(ray, out hit) /*&& !expe.paused */) {
            //change tag color of the ray cast
            if (interactWithUI.GetStateDown(m_pose.inputSource)) {

                if (hit.transform.tag == "Color tag") {
                    if (synctag) {
                        nameR = hit.transform.GetComponent<Renderer>().material.name;
                        photonView.RPC("ChangeRayColour", Photon.Pun.RpcTarget.All, nameR);
                        right.GetComponent<PhotonView>().RPC("RayColour", Photon.Pun.RpcTarget.All, nameR);
                        // Debug.Log("tag sync");
                    } else if (photonView.IsMine) {
                        nameR = hit.transform.GetComponent<Renderer>().material.name;
                        photonView.RPC("ChangeRayColour", Photon.Pun.RpcTarget.All, nameR);
                        right.GetComponent<PhotonView>().RPC("RayColour", Photon.Pun.RpcTarget.All, nameR);
                        // Debug.Log("tag not sync: photonView.IsMine");
                    }
                } else if (photonView.IsMine) {
                    if (hit.transform.tag == "MoveControlTP") {
                        palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = green;
                        moveMode = "TP";
                    } else if (hit.transform.tag == "MoveControlJoy") {
                        palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = green;
                        palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = red;
                        moveMode = "joy";
                    } else if (hit.transform.tag == "MoveControlDrag") {
                        palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = green;
                        palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = red;
                        moveMode = "drag";
                    } else if (hit.transform.tag == "MoveControlSync" && !isOtherSynced) {
                        palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = red;
                        palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = green;
                        palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = red;
                        moveMode = "sync";
                    }
                    photonView.RPC("ChangeMovement", Photon.Pun.RpcTarget.All, moveMode);
                }
            }

        }

    }

    void MapPosition() {
        // left hand 
        palette.position = left.transform.position;
        palette.rotation = left.transform.rotation;


        // right hand
        rightHand.position = right.transform.position;
        rightHand.rotation = right.transform.rotation;

        ray.position = right.transform.position;
        ray.rotation = right.transform.rotation;

        // head
        head.position = headset.transform.position;
        head.rotation = headset.transform.rotation;

        // circle
        pos.position = new Vector3(headset.transform.position.x, 0 , headset.transform.position.z);
        circle.rotation = new Quaternion(0, headset.transform.rotation.y, 0, headset.transform.rotation.w);

        // body
        torse.position = headset.transform.position;
    }

    [PunRPC]
    void ChangeMovement(string moveMode) {
        if (moveMode == "TP") {
            palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = green;
        } else if (moveMode == "joy") {
            palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = green;
            palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = red;
            moveMode = "joy";
        } else if (moveMode == "drag") {
            palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = green;
            palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = red;
            moveMode = "drag";
        } else if (moveMode == "sync") {
            palette.Find("GameObjectMoveJoy/CubeMoveModeJoy").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveDrag/CubeMoveModeDrag").GetComponent<Renderer>().material = red;
            palette.Find("GameObjectMoveSync/CubeMoveModeSync").GetComponent<Renderer>().material = green;
            palette.Find("GameObjectMoveTP/CubeMoveModeTP").GetComponent<Renderer>().material = red;
            moveMode = "sync";
        }
    }

    [PunRPC]
    void ChangeRayColour(string nameR) {
       // change the ray color 
        if (nameR == "blue (Instance)") {
            rayCast.GetComponent<Renderer>().material = blue;
        } else if(nameR == "green (Instance)") {
            rayCast.GetComponent<Renderer>().material = green;
        } else if(nameR == "red (Instance)") {
            rayCast.GetComponent<Renderer>().material = red;
        } else if (nameR == "white (Instance)") {
            rayCast.GetComponent<Renderer>().material = white;
        } else {
            rayCast.GetComponent<Renderer>().material = none;
        }
    }

    [PunRPC]
    void ChangeTag( int OB) {
        {
            // change the tag color of a picture
            if (PhotonView.Find(OB).gameObject.tag != "Card"){ return; }
           
            nameT = rayCast.GetComponent<Renderer>().material.name;
            if (expe != null && expe.curentTrial.canTagCard) {
                if (nameT == "blue (Instance)") {
                    PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = blue;
                } else if (nameT == "green (Instance)") {
                    PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = green;
                } else if (nameT == "red (Instance)") {
                    PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = red;
                } else if (nameT == "white (Instance)") {
                    PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = white;
                } else {
                    PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = none;
                }
            }
        }

    }

    [PunRPC]
    void removeTag(int OB) {
        if (PhotonView.Find(OB).gameObject.tag != "Card") { return; }

        PhotonView.Find(OB).gameObject.transform.GetChild(0).GetComponent<Renderer>().material = none;
     
    }

    [PunRPC]
    void tagMode(string tag) {
        Debug.Log("Change tag mode");
        if (tag == "syncro tag") {
            synctag = true;
        } else {
            synctag = false;
        }
        Debug.Log("tag mode"+synctag);
    }

    void OperatorActions(){
        if(Input.GetKeyDown(KeyCode.Space)){
            Debug.Log(".                Space was pressed (player one)");
            render.spacePressedOperator();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            Debug.Log(".                E was pressed (player one)");
            render.EPressedOperator();
        }
        if(Input.GetKeyDown(KeyCode.T)){
            Debug.Log(".                T was pressed (player one)");
            render.TPressedOperator();
        } 
        if(Input.GetKeyDown(KeyCode.D)){
            Debug.Log(".                D was pressed (player one)");
            render.DPressedOperator();
        }
        if(Input.GetKeyDown(KeyCode.I)){
            Debug.Log(".                I was pressed (player one)");
            render.IPressedOperator();
        }
    }

}

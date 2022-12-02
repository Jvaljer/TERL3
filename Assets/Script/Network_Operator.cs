using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

    
public class Network_Operator : MonoBehaviourPun {

    public float rotationX;
    public float rotationY;
    public float sensivity = 5f;
    

    private GameObject body;
    private GameObject room;

    private GameObject moving;

    private rendering render;
    Expe expe;
    private bool locked = true;

    //private PhotonView photonView;

    void Start(){
        Debug.Log("Start network_Operator");
        room = GameObject.Find("Salle");
        moving = GameObject.Find("/[movingCam]");
    }

    void Update(){
        //searching for an expe if there isn't already one 
        if(expe==null){
            //Debug.Log("Searching for an expe");
            expe = GameObject.Find("/Salle").GetComponent<rendering>().expe;
            render = GameObject.Find("/Salle").GetComponent<rendering>();
        }

        if(!locked){
            //the operator is free to move and watch how he wants

            transform.Translate(Vector3.forward * 0.01f * Time.fixedTime * Input.GetAxis("Vertical"));
            transform.Translate(Vector3.right * 0.01f * Time.fixedTime * Input.GetAxis("Horizontal")); 

            if(!isMouseOffScreen()){
                rotationX -= Input.GetAxis("Mouse Y") * sensivity;
                rotationY += Input.GetAxis("Mouse X") * sensivity;
                rotationX = Mathf.Clamp(rotationX, -90,90);
                transform.rotation = Quaternion.Euler(rotationX,rotationY,0);
            }
        }
        
        //else the operator can't do nothing 
            //maybe move him to a general overview camera 

        //all possible inputs  
        if(Input.GetKeyDown(KeyCode.L)){
            // L -> cam lock for op
            Debug.Log(".                     L was pressed (operator)");
            locked = !locked;
        }

        if(Input.GetKeyDown(KeyCode.I)){
            //printing wanted informations
            Debug.Log("render.expeEnCours : " + render.expeEnCours);
            Debug.Log("render.trialEnCours : " + render.trialEnCours);
            Debug.Log("expe.expeRunning : " + expe.expeRunning);
            Debug.Log("expe.trialRunning : " + expe.trialRunning);
        }
    }

    private bool isMouseOffScreen(){
        if(Input.mousePosition.x <= 2 || Input.mousePosition.y <= 2 || Input.mousePosition.x >= Screen.width -2 || Input.mousePosition.y >= Screen.height -2){
            return true;
        } 
        return false; 
    }
    
    private string[] getLogs(){
        string[] logs = new string[10];
        //the aim is to first get the position in real time of both players
        logs[0] = "position";
        return logs;
    }
}
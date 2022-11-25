using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

    
public class Network_Operator : MonoBehaviourPun {

    public float rotationX;
    public float rotationY;
    public float sensivity = 0.5f;
    

    private GameObject body;
    private GameObject room;

    private GameObject moving;


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
            expe = GameObject.Find("Salle").GetComponent<rendering>().expe;
        } else if(expe != null){
            Debug.Log("Now in an expe");
        }

        if(!locked){
            //the operator is free to move and watch how he wants

            transform.Translate(Vector3.forward * 0.1f * Time.fixedTime * Input.GetAxis("Vertical"));
            transform.Translate(Vector3.right * 0.1f * Time.fixedTime * Input.GetAxis("Horizontal")); 

            if(!isMouseOffScreen()){
                rotationX -= Input.GetAxis("Mouse Y") * sensivity;
                rotationY += Input.GetAxis("Mouse X") * sensivity;
                rotationX = Mathf.Clamp(rotationX, -90,90);
                transform.rotation = Quaternion.Euler(rotationX,rotationY,0);
            }
        }
        
        //else the operator can't do nothing 
            //maybe move him to a general overview camera 

        if(Input.GetKeyDown(KeyCode.L)){
            Debug.Log("L was pressed ...");
            locked = !locked;
        }
    }

    private bool isMouseOffScreen(){
        if(Input.mousePosition.x <= 2 || Input.mousePosition.y <= 2 || Input.mousePosition.x >= Screen.width -2 || Input.mousePosition.y >= Screen.height -2){
            return true;
        } 
        return false; 
    }
    
}
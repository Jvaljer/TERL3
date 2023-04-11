using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NetworkOperator : MonoBehaviourPun {
    //rotation & sensivity
    private float rotation_x;
    private float rotation_y;
    private float sensivity = 5f;

    //Materials components
    private Material blue;
    private Material green;
    private Material white;
    private Material red;
    private Material light_red;
    private Material none;
    
    //physical body & camera + attributes
    private GameObject body;
    private GameObject camera;
    private bool cam_locked = true;

    //room & corresponding script + experiment
    private GameObject room;
    private Rendering room_render;
    private Experiment experiment;


    //Unity Start method, used as an initializer
    void Start(){
        room = GameObject.Find("Room");
        moving = GameObject.Find("/[moving_camera]");
        if(!photonView.IsMine){
            moving.gameObject.SetActive(false);
        }
        room_render = room.GetComponent<Rendering>();
    }

    //Unity Update method, called once per frame
    void Update(){
        if(experiment==null && room_render.experiment!=null){
            experiment = room_render.experiment;
        }

        if(!cam_locked){
            transform.Translate(Vector3.forward * 0.01f * Input.GetAxis("Vertical"));
            transform.Translate(Vector3.right * 0.01f * Input.GetAxis("Horizontal"));
            if(!MouseOffScreen()){
                rotation_x -= Input.GetAxis("Mouse Y")*sensivity;
                rotation_y += Input.GetAxis("Mouse X") * sensivity;
                rotation_x = Mathf.Clamp(rotationX, -90,90);
                transform.rotation = Quaternion.Euler(rotation_x,rotation_y,0);
            }
        }

        //all operator's inputs for specifics actions
        if(Input.GetKeyDown(KeyCode.L)){
            //locking the camera
            cam_locked != cam_locked;
        }
        if(Input.GetKeyDown(KeyCode.Space)){
            //triggering the room's experiment
            room_render.SpacePressed();
        }
        if(Input.GetKeyDown(KeyCode.E)){
            //ending the room's experiment
            room_render.EPressed();
        }
        if(Input.GetKeyDown(KeyCode.T)){
            //teleporting both players to each other (+ to center of the room)
            room_render.TPressed();
        }
        if(Input.GetKeyDown(KeyCode.D)){
            //triggering the room's demo
            room_render.DPressed();
        }
    }

    //other methods
    private bool MouseOffScreen(){
        return (Input.mousePosition.x <= 2 || Input.mousePosition.y <= 2 || Input.mousePosition.x >= Screen.width -2 || Input.mousePosition.y >= Screen.height -2);
    }
}
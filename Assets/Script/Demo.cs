using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;

public class Demo {
    private Network_Operator operator;
    private Network_Player player;

    private Teleporter tp;
    private rendering render;
    public List<GameObject> cardList;

    public bool demoRunning;

    public Demo(bool b_, string s_ , List<GameObject> l_){
        if(b_ && s_ = "ope"){
            operator = GameObject.Find("Network Operator(Clone)").GetComponent<Network_Operator>();
            player = null;
            tp = null;
        } else if(b_ && s_ = "p_"){
            operator = null;
            player = GameObject.Find("Network Player(Clone)").GetComponent<Network_Player>();
            tp = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
        } else {
            player = null;
            operator = null;
            tp = null;
        }

        demoRunning = true;
        render = GameObject.Find("/Salle").GetComponent<rendering>();
        cardList = l_;

        //what a demo truly does ??? 
    }

    void Begin(){
        
    }

    void End(){

    }

}
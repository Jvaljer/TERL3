using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;

public class Expe {
    public string participant;
    private string group; // = "g01";
    public int startTrial = 1;
    public int trialNb = 0;

    public Teleporter teleport;
    private Network_Player player;
    private rendering render;

    private readonly string expeDescriptionFile = "Experiments/initialTrialFile";
    private string previousCardNum;
    //static string[] letters = {"H", "N", "K", "R"};
    static readonly string[] letters = { "evertnone", "ehornone" };
    private List<Trial> theTrials;
    public List<GameObject> cardList;
    public Trial curentTrial;
    private StreamWriter writer;
    private StreamWriter kineWriter;
    private readonly bool haveEyesCondition = false;
    public bool expeRunning = false;
    public bool trialRunning = false;

    static class ThreadSafeRandom {
        [ThreadStatic] private static System.Random Local;

        public static System.Random ThisThreadsRandom {
            get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }



    public Expe(string part, string grp, int startNb, List<GameObject> cardL) {
        Debug.Log("INSIDE EXPE CONSTRUCTOR");

        expeRunning = true;
        group = grp;
        participant = part;
        trialNb = startNb;
        cardList = cardL;
        Debug.Log("group : " + group  +"   participant : " + part + "   trialNb : " + startNb );

        teleport = GameObject.Find("/[CameraRig]/ControllerRotator/Controller (right)").GetComponent<Teleporter>();
        render = GameObject.Find("Network Operator(Clone)").GetComponent<Network_Operator>().GetRender();

        string mydate = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        //  Debug.Log("Goupe: " + trial.group + );
        // file name should look like  "class-PXX-2019-MM-DD-HH-MM-SS.csv"
        //string path = "Assets/Resources/expeLogs/class-" +  group + "-" + participant + "-" + mydate + ".csv";
        string path = "Assets/Resources/logs/class-" +  group + "-" + participant + "-" + mydate + ".csv";
        //File.Create(path);
        //Write some text to the test.txt file
            //Debug.Log("path : " + path);
        writer = new StreamWriter(path, false);

        writer.WriteLine(
            // "factor"
            "Group;Participant;CollabEnvironememnt;trialNb;training;MoveMode;Task;Wall;CardToTag;"
            // measure
            + "nbMove;nbMoveWall;nbDragWallFloor;distTotal;nbRotate;rotateTotal;"
            + "trialTime;moveTime");
        writer.Flush();
        path = "Assets/Resources/logs/class-" + participant + "-" + mydate + ".txt";
        kineWriter = new StreamWriter(path, false);
            //Debug.Log("VisExpe :" + expeDescriptionFile + " with participant : " + participant);

        TextAsset mytxtData = (TextAsset)Resources.Load(expeDescriptionFile);

        string txt = mytxtData.text;
        List<string> lines = new List<string>(txt.Split('\n'));        

        theTrials = new List<Trial>();

        foreach (string str in lines) {
            List<string> values = new List<string>(str.Split(';'));
            if (values[0] == "#pause" && theTrials.Count != 0 && theTrials[theTrials.Count - 1].group == group) {
                Debug.Log("#pause detected");
                theTrials.Add(new Trial(this, values[0], "", "", "", "", "", "", "", ""));
                //Debug.Log("Pause added to trials");
            } else if (values[0] == group && values[1] == participant) {
                Debug.Log("adding a Trial");
                theTrials.Add(new Trial(this,
                        values[0], values[1],
                        values[2], values[3], values[4], values[5], values[6], values[7], values[8]
                    ));
                //Debug.Log("Goupe: " + theTrials[theTrials.Count - 1].group + "; Participant: " + theTrials[theTrials.Count - 1].participant +
                  //        "; collabEnvironment: " + theTrials[theTrials.Count - 1].collabEnvironememnt + "; trialNb: " + theTrials[theTrials.Count - 1].trialNb + "; training: " + theTrials[theTrials.Count - 1].training + "; moveMode: " + theTrials[theTrials.Count - 1].moveMode + "; task: " + theTrials[theTrials.Count - 1].task + "; wall: " + theTrials[theTrials.Count - 1].wall + "; cardToTag: " + theTrials[theTrials.Count - 1].cardToTag);

                theTrials[theTrials.Count - 1].pathLog = path;

                theTrials[theTrials.Count - 1].kineWriter = kineWriter;
            }
        }
        Debug.Log(" foreach passed " + trialNb  + " " + theTrials.Count);

        curentTrial = theTrials[trialNb];
        kineWriter.WriteLine(curentTrial.group + " " + curentTrial.participant + " kine action");
        kineWriter.Flush();
    }

    // Re-doing the Expe Constructor to improve it (or at least try) + adapt it to new format
    public Expe(string partId, string grpId, int initNb, List<GameObject> cards){ //cannot be without operator. 
        //Expe launch for the assigned participant being apart of the selected group with the initNb
        return;
    }

    public void setInfoLocation(){
        teleport.menu.transform.position = teleport.cam.position + 1f*teleport.cam.forward;
        teleport.menu.transform.rotation = teleport.cam.rotation;
        //teleport.menu.transform.RotateAround(teleport.menu.transform.position, Vector3.up, teleport.cam.rotation.eulerAngles.y - teleport.menu.transform.rotation.eulerAngles.y);
    }

    public IEnumerator trialStarted(){
        curentTrial.startTrialTimer();
        teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "Trial started";
        yield return new WaitForSeconds(5);
        teleport.menu.SetActive(false);
    }

    public void nextTrial(){
        
        Debug.Log("Trial count" + theTrials.Count + " curent nb " + trialNb);
        if (!trialRunning){
            trialRunning = true;
            Debug.Log("update text info no trial running");
            setInfoLocation();
            teleport.menu.SetActive(true);
            if (theTrials[trialNb].task == "search"){
                if (theTrials[trialNb].training == "1"){
                    teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Search \n Training";
                } else {
                    teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Search";
                }
                teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "You are the one synchronized \n click to start the trial \n spot the card and tell the other";
            } else {
                if (theTrials[trialNb].training == "1") {
                    teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Navigate " + theTrials[trialNb].moveMode + "\n Training";
                } else {
                    teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Navigate " + theTrials[trialNb].moveMode;
                }
                teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "You are the one moving \n wait for the other to start \n let the other tell you where to go";
            }

            theTrials[trialNb].startTrial();
            curentTrial = theTrials[trialNb];
            //trialRunning = true;
        } else if (trialNb == theTrials.Count - 1) {
            write();
            theTrials[trialNb - 1].card.transform.GetChild(1).gameObject.SetActive(false);
            theTrials[trialNb].card.transform.GetChild(1).gameObject.SetActive(false);
            Finished();
        } else {
            write();
            incTrialNb();
            Debug.Log(theTrials[trialNb].group);
            if (theTrials[trialNb].group == "#pause") {
                trialRunning = false;
                teleport.photonView.RPC("resetPosition", Photon.Pun.RpcTarget.AllBuffered);
                teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "Pause";
                writer.WriteLine("#pause;");
                writer.Flush();
                incTrialNb();
                teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Next move " + theTrials[trialNb].moveMode;
                setInfoLocation();
                teleport.menu.SetActive(true);
            } else {
                Debug.Log("update text info");
                setInfoLocation();
                if (theTrials[trialNb].task == "search") {
                    if (theTrials[trialNb].training == "1") {
                        teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Search \n Training";
                    } else {
                        teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Search";
                    }
                    teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "You are the one synchronized \n click to start the trial \n spot the card and tell the other";
                } else {
                    if (theTrials[trialNb].training == "1") {
                        teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Navigate " + theTrials[trialNb].moveMode + "\n Training";
                    } else {
                        teleport.menu.transform.Find("moveModeText").GetComponent<TextMesh>().text = "Navigate " + theTrials[trialNb].moveMode;
                    }
                    teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "You are the one moving \n wait for the other to start \n let the other tell you where to go";
                }
                teleport.menu.SetActive(true);
                theTrials[trialNb].startTrial();
                curentTrial = theTrials[trialNb];
            }
        }
    }

    public void write() {
        writer.WriteLine(
            // "factor"
            theTrials[trialNb].group + ";" + theTrials[trialNb].participant + ";" + theTrials[trialNb].collabEnvironememnt + ";" + theTrials[trialNb].trialNb + ";" + theTrials[trialNb].training + ";" + theTrials[trialNb].moveMode + ";" + theTrials[trialNb].task + ";" + theTrials[trialNb].wall + ";" + theTrials[trialNb].cardToTag + ";"
            // measure
            + theTrials[trialNb].nbMove + ";" + theTrials[trialNb].nbMoveWall + ";" + theTrials[trialNb].nbDragWallFloor  + ";" + theTrials[trialNb].distTotal + ";" + theTrials[trialNb].nbRotate + ";" + theTrials[trialNb].rotateTotal + ";"
            + theTrials[trialNb].trialTime + ";" + theTrials[trialNb].moveTime
            );
        writer.Flush();
    }

    public void Finished() {
        write();
        teleport.menu.transform.Find("textInfo").gameObject.SetActive(true);
        teleport.menu.transform.Find("textInfo").GetComponent<TextMesh>().text = "Fin de l'exp�rience";
        trialRunning = false;
        expeRunning = false;
        writer.Close();
        kineWriter.Close();
    }

    public void incTrialNb() {
        trialNb += 1;
        if (trialNb - 2 >= 0 && theTrials[trialNb - 2].group != "#pause")
        {
            theTrials[trialNb - 2].card.transform.GetChild(1).gameObject.SetActive(false);
            theTrials[trialNb - 2].card.transform.GetChild(1).GetComponent<Renderer>().material = player.lightRed;
            theTrials[trialNb - 2].card.transform.GetChild(0).GetComponent<Renderer>().material = player.none;
        }
    }

}

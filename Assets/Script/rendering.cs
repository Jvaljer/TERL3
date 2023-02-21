using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class rendering : MonoBehaviourPunCallbacks {
    //Prefab card
    public GameObject pfCard;

    //trash
    public GameObject trash1;
    public GameObject trash2;
    public GameObject trash3;
    public GameObject trash4;

    //walls
    public Transform MurB;
    public Transform MurL;
    public Transform MurR;

    public Transform room;

    public Transform cardArea;

    //Card list
    public List<GameObject> cardList;
    public List<GameObject> cardListToTeleport;

    //List of textures
    public object[] textures;
    public bool card1 = true;
    public bool training = false;

    public static int cardPerWall = 20; //choisir le nombre de carte par mur, elles seront ensuite plac�es automatiquement (marche difficilement � plus de 40 cartes)

    //who to load
    public string participant = "";
    public string group;
    public int firstTrialNb;

    public GameObject m_Pointer;

    public bool expeEnCours = false;
    public bool trialEnCours = false;
    public Expe expe;

    public bool demoRunning = false;
    public bool demoHasBeenCreated = false;
    public bool demoHasBeenDestroyed = false;
    public bool cardsCreated = false;
    public bool cardsDestroyed = false;

    public bool expePaused = false;

    public class MyCard {
        // Creation of the card 
        public GameObject goCard = null;
        public string pos_tag = "";
        public PhotonView pv;
        public Transform parent;
        public int id_on_wall;

        public Transform initial_parent;

        public MyCard(Texture2D tex, Transform mur , int i) {
            GameObject goCard = PhotonNetwork.InstantiateRoomObject("Card", mur.position, mur.rotation, 0, null);
            goCard.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
            parent = mur;
            pv = goCard.GetPhotonView();
            //Debug.Log("MyCard created on Mur : " + parent);
            id_on_wall = i;
            pos_tag = "onWall";

            initial_parent = mur;
        }

        public void Reset(){
            //must implement
            //here we simply wanna put the card back on its original location on its initial parent.
            return;
        }
    }

    void Update() {
        if (expeEnCours) {
            expeEnCours = expe.expeRunning;
            trialEnCours = expe.trialRunning;
        }

        if (trialEnCours && expeEnCours) {
            photonView.RPC("curentTrialConditionCheck", Photon.Pun.RpcTarget.AllBuffered);
        }
    }

    // Start is called before the first frame update
    void Awake() {
        trash1.SetActive(false);
        trash2.SetActive(false);
        trash3.SetActive(false);
        trash4.SetActive(false);
    }

    public void Cards() {
        // recup jeu de carte et training depuis le csv
        //card1 = GameObject.Find("/[CameraRig]/Controller (right)").GetComponent<Teleporter>().card1;
        //bool training = GameObject.Find("/[CameraRig]/Controller (right)").GetComponent<Teleporter>().training;
        Debug.Log("card1 : " + card1);
        if (training) {
            textures = Resources.LoadAll("dixit_training/", typeof(Texture2D));
            Debug.Log("dixit_training/ is charged");
        }
        else {
            textures = Resources.LoadAll("dixit_all/", typeof(Texture2D));
            Debug.Log("dixit_all/ is charged");
        }
        Debug.Log(textures.Length);

    }

    public void CardCreation() { 
        Debug.Log("CardCreation with MasterClient : " + PhotonNetwork.IsMasterClient);

        Transform mur;
        int pos;
        string wall;
        for (int i = 0 ; i < cardPerWall*3 ; i++) {
            //Debug.Log("     i = " + i);
            if (i < textures.Length) {
                //Debug.Log("     i<textures.length (" + i + "<" + textures.Length + ")");
                // Slit the card over the 3 walls
                if (i < cardPerWall) {
                    //Debug.Log("         i < cardPerWall (" + i + "<" + cardPerWall + ")");
                    mur = MurL;
                    wall = "L";
                    pos = i;
                }
                else if (i < 2 * cardPerWall) {
                    //Debug.Log("         i < 2*cardPerWall (" + i + "<" + 2*cardPerWall + ")");
                    mur = MurB;
                    wall = "B";
                    pos = i - cardPerWall;
                }
                else {
                    //Debug.Log("         else (" + i + "," + cardPerWall + ")");
                    mur = MurR;
                    wall = "R";
                    pos = i - 2 * cardPerWall;
                }

                //Debug.Log("creating MyCard c : { textures[" + i + "] , Mur" + wall + " , id_on_wall : " + i + "}");
                MyCard c = new MyCard((Texture2D)textures[i], mur, i);
                //Debug.Log("adding the card to the list");
                //Debug.Log("cardList -> len : " + cardList.Capacity);
                photonView.RPC("addListCard", Photon.Pun.RpcTarget.AllBuffered, c.pv.ViewID, demoHasBeenCreated);
                c.pv.RPC("LoadCard", Photon.Pun.RpcTarget.AllBuffered, c.pv.ViewID, mur.GetComponent<PhotonView>().ViewID, pos, i);
            }
            else {
                break;
            }
        }
        cardsCreated = true;
    }

    [PunRPC]
    //Add card to the list of card
    void addListCard(int OB, bool b_) {
        if(b_){
            for(int i=0; i<60; i++){
                if(cardList[i]==null){
                    cardList[i] = PhotonView.Find(OB).gameObject;
                }
            }
        } else {
            cardList.Add(PhotonView.Find(OB).gameObject);
        }
    }

    [PunRPC]
    void startExpe(string grp, int nb, bool withOpe) {
        if(withOpe){
            if (PhotonNetwork.IsMasterClient) { 
                participant = "ope";
            } else if (PhotonNetwork.LocalPlayer.ActorNumber == 2) {
                participant = "p01";
            } else {
                participant = "p02";
            }
        } else {
            if(PhotonNetwork.IsMasterClient){
                participant = "p01";
            } else {
                participant = "p02";
            }
        }

        print("calling on new Expe() with ->\n  participant : " + participant + "\n  group : " + grp + "\n  nbTrial : " + nb  );
        expe = new Expe(participant, grp, nb, cardList, withOpe);
        print("\n\n    expe has well been instantiated !" + expe);

        if (expe.curentTrial.collabEnvironememnt == "C") {
            //desactiver son
            Debug.Log("Sound off" );
            GameObject sound = GameObject.Find("Network Voice");
            sound.SetActive(false);
        } else {
            Debug.Log(" Sound on");
        }
    }

    [PunRPC]
    void curentTrialConditionCheck(){
        expe.curentTrial.checkConditions();
    }

    [PunRPC]
    void endExpe() {
        expe.Finished();
        print("Expe End");
        //stop timing , stop expe ? 
        /*
        Debug.Log("nb tag card : " + expe.curentTrial.nbTag);
        Debug.Log("nb change tag color : " + expe.curentTrial.nbChangeTag);
        
        Debug.Log("nb sync TP : " + expe.curentTrial.nbSyncTp);
        Debug.Log("nb async TP : " + expe.curentTrial.nbAsyncTP);

        Debug.Log("nb sync TP W : " + expe.curentTrial.nbSyncTpWall);
        Debug.Log("nb async TP W: " + expe.curentTrial.nbAsyncTpWall);
        Debug.Log("nb sync TP G: " + expe.curentTrial.nbSyncTpGround);
        Debug.Log("nb async TP G: " + expe.curentTrial.nbAsyncTpGround);

        Debug.Log("nb DragCard : " + expe.curentTrial.nbDragCard);
        Debug.Log("nb GroupCardTP: " + expe.curentTrial.nbGroupCardTP);
        Debug.Log("nb DestroyCard: " + expe.curentTrial.nbDestroyCard);
        Debug.Log("nb UndoCard: " + expe.curentTrial.nbUndoCard);
        */
        expeEnCours = false;
        trialEnCours = false;
    }

    [PunRPC]
    public void nextTrial() {
        expe.nextTrial();
    }

    [PunRPC]
    //Add card to the list of card
    void DestroyCard(int OB, int nbTrashs) {
        // Undo.DestroyObjectImmediate(PhotonView.Find(OB).gameObject);
        PhotonView.Find(OB).gameObject.SetActive(false);
        if (nbTrashs >= 1) {
            trash1.SetActive(true);
        }
        if (nbTrashs >= 2) {
            trash2.SetActive(true);
        }
        if (nbTrashs >= 3) {
            trash3.SetActive(true);
        }
        if (nbTrashs >= 4) {
            trash4.SetActive(true);
        }
    }
    
    [PunRPC]
    //Add card to the list of card
    void UndoCard(int OB, int nbTrashs) {
        // Undo.DestroyObjectImmediate(PhotonView.Find(OB).gameObject);
        PhotonView.Find(OB).gameObject.SetActive(true);

        if (nbTrashs <= 1) {
            trash1.SetActive(false);
        }
        if (nbTrashs <= 2) {
            trash2.SetActive(false);
        }
        if (nbTrashs <= 3) {
            trash3.SetActive(false);
        }
        if (nbTrashs <=4) {
            trash4.SetActive(false);
        }
    }

    public void spacePressedOperator() {
        if (!expeEnCours){
            bool b_;
            if(GameObject.Find("Network Operator(Clone)")==null){
                b_ = false;
            } else {
                b_ = true;
            }
            if(demoRunning || demoHasBeenCreated){
                //wanna reset the cards & then let all recreate
                demoRunning = false;
                CardDeletion(); 
                //ResetCards();
                CardCreation();
                photonView.RPC("startExpe", Photon.Pun.RpcTarget.AllBuffered, group, firstTrialNb, b_);
                print("Expe Started succesfully !");
                expeEnCours = true;
            } else {
                Cards();
                CardCreation();
                photonView.RPC("startExpe", Photon.Pun.RpcTarget.AllBuffered, group, firstTrialNb, b_);
            
                print("Expe Started succesfully !");
                expeEnCours = true;
            }
        } else if (expeEnCours && !trialEnCours) {
            Debug.Log("expeEnCour && !trialEnCours ->");
            photonView.RPC("nextTrial", Photon.Pun.RpcTarget.AllBuffered);
            Debug.Log("     nextTrial called successfully");
        } 
    }

    public void EPressedOperator() {
        photonView.RPC("endExpe", Photon.Pun.RpcTarget.AllBuffered);
        CardDeletion();
    }

    public void TPressedOperator() {
        expe.teleport.photonView.RPC("tpToOther", Photon.Pun.RpcTarget.Others);
    }

    public void CardDeletion(){
        //we get every card that is IN the list and we destroy it using the PhotonNetwork 'Destroy()' method
        Debug.Log("cardList.Capacity -> " + cardList.Capacity);
        
        for (int i=0; i<60; i++){
            if(cardList[i] != null){
                GameObject ob = cardList[i];
                int ownerId = ob.GetComponent<PhotonView>().Owner.ActorNumber;
                //Debug.Log("card n°" + i + "is owned by :" + ownerId);
                if(ownerId != PhotonNetwork.MasterClient.ActorNumber){
                    Debug.Log("card n°" + i + "is owned by :" + ownerId);
                    //Debug.Log("Switching Ownership");
                    ownerId = PhotonNetwork.MasterClient.ActorNumber;
                    ob.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.MasterClient.ActorNumber);
                    //Debug.Log("now, card n°" + i + "is owned by :" + ownerId);
                }
                //Debug.Log("Destroying card n°" + i);
                PhotonNetwork.Destroy(ob);
                /*if(cardList[i] == null){
                    Debug.Log("Well Destroyed n°"+i);
                } */
            }
        }
        Debug.Log("all cards well destroyed");
        cardsDestroyed = true;
    }

    public void DPressedOperator(){

        if(!demoHasBeenCreated){
            Debug.Log("Initializing demo");
            Cards();
            CardCreation();
            demoHasBeenCreated = true;
            demoRunning = true;
        } else if(demoRunning && !demoHasBeenDestroyed){
            Debug.Log("initial Destruction of demo");
            CardDeletion();
            demoHasBeenDestroyed = true;
            demoRunning = false;
        } else if(!demoRunning && demoHasBeenCreated){
            Debug.Log("casual demo start");
            CardCreation();
            demoRunning = true;
        } else if(demoRunning && demoHasBeenDestroyed){
            Debug.Log("casual demo stop");
            CardDeletion();
            demoRunning = false;
        }
    } 

    public void IPressedOperator(){
        Debug.Log("current state : demoRunning(" + demoRunning + ") - demoHasBeenCreated(" + demoHasBeenCreated + ") - expeEnCours(" + expeEnCours + ") - demoHasBeenDestroyed(" + demoHasBeenDestroyed + ") - cardsCreated(" + cardsCreated + ")");
    }
    public void PauseExpe(){
        Debug.Log("render -> Pausing the current expe");
        expePaused = expe.paused;
        expe.Pause();
    }

    public void ResumeExpe(){
        Debug.Log("render -> Resuming the current expe");
        expePaused = expe.paused;
        expe.Resume();
    }


    public void ResetCards(){
        return;
    }
}

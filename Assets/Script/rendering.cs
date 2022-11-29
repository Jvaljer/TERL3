using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class rendering : MonoBehaviourPunCallbacks //, MonoBehaviourPun
{
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

    public class MyCard
    {
        // Creation of the card 
        public GameObject goCard = null;
        public string tag = "";
        public PhotonView pv;
        public Transform parent;
       

        public MyCard(Texture2D tex, Transform mur , int i)
        {
            GameObject goCard = PhotonNetwork.InstantiateRoomObject("Card", mur.position, mur.rotation, 0, null);
            goCard.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
            parent = mur;
            pv = goCard.GetPhotonView();
        }
    }

    void Update()
    {
        if (expeEnCours)
        {
            expeEnCours = expe.expeRunning;
            trialEnCours = expe.trialRunning;
        }

        if (trialEnCours && expeEnCours)
        {
            photonView.RPC("curentTrialConditionCheck", Photon.Pun.RpcTarget.AllBuffered);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !expeEnCours)
        {
            Cards();
            CardCreation();
            print("space key was pressed");
            photonView.RPC("startExpe", Photon.Pun.RpcTarget.AllBuffered, group, firstTrialNb);
            
            expeEnCours = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && expeEnCours && !trialEnCours)
        {
            photonView.RPC("nextTrial", Photon.Pun.RpcTarget.AllBuffered);
        }

        if (Input.GetKeyDown(KeyCode.E) && expeEnCours)
        {
            photonView.RPC("endExpe", Photon.Pun.RpcTarget.AllBuffered);
        }
        if (Input.GetKeyDown(KeyCode.T) && expeEnCours)
        {
            expe.teleport.photonView.RPC("tpToOther", Photon.Pun.RpcTarget.Others);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        trash1.SetActive(false);
        trash2.SetActive(false);
        trash3.SetActive(false);
        trash4.SetActive(false);
    }

    public void Cards()
    {
        // recup jeu de carte et training depuis le csv
        //card1 = GameObject.Find("/[CameraRig]/Controller (right)").GetComponent<Teleporter>().card1;
        //bool training = GameObject.Find("/[CameraRig]/Controller (right)").GetComponent<Teleporter>().training;
        Debug.Log(card1);
        if (training)
        {
            textures = Resources.LoadAll("dixit_training/", typeof(Texture2D));
        }
        else 
        {
            textures = Resources.LoadAll("dixit_all/", typeof(Texture2D));
        }
        Debug.Log(textures.Length);

    }

    public void CardCreation() { 
        
        Debug.Log("Creation carte " + PhotonNetwork.IsMasterClient);


        Transform mur;
        int pos;
        for (int i = 0 ; i < cardPerWall*3 ; i++)
        {
            if (i < textures.Length)
            {
                // Slit the cqrd over the 3 walls
                if (i < cardPerWall)
                {
                    mur = MurL;
                    pos = i;
                }
                else if (i < 2 * cardPerWall)
                {
                    mur = MurB;
                    pos = i - cardPerWall;
                }
                else
                {
                    mur = MurR;
                    pos = i - 2 * cardPerWall;
                }
                //Debug.Log(i);
                MyCard c = new MyCard((Texture2D)textures[i], mur, i);
                photonView.RPC("addListCard", Photon.Pun.RpcTarget.AllBuffered, c.pv.ViewID);
                c.pv.RPC("LoadCard", Photon.Pun.RpcTarget.AllBuffered, c.pv.ViewID, mur.GetComponent<PhotonView>().ViewID, pos, i);
            }
            else
            {
                break;
            }
        }
    }

    [PunRPC]
    //Add card to the list of card
    void addListCard(int OB)
    {
        cardList.Add(PhotonView.Find(OB).gameObject);
    }

    [PunRPC]
    void startExpe(string grp, int nb)
    {
        if (PhotonNetwork.IsMasterClient)
        { 
            participant = "ope";
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            participant = "p01";
        } else 
        {
            participant = "p02";
        }
        expe = new Expe(participant, grp, nb, cardList);
        
        if (expe.curentTrial.collabEnvironememnt == "C")
        {
            //desactiver son
            Debug.Log("Sound off" );
            GameObject sound = GameObject.Find("Network Voice");
            sound.SetActive(false);
        }
        else
        {
            Debug.Log(" Sound on");
        }
    }

    [PunRPC]
    void curentTrialConditionCheck()
    {
        expe.curentTrial.checkConditions();
    }

    [PunRPC]
    void endExpe()
    {
        expe.Finished();
        print("End");
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
    public void nextTrial()
    {
        expe.nextTrial();
    }

    [PunRPC]
    //Add card to the list of card
    void DestroyCard(int OB, int nbTrashs)
    {
        // Undo.DestroyObjectImmediate(PhotonView.Find(OB).gameObject);
        PhotonView.Find(OB).gameObject.SetActive(false);
        if (nbTrashs >= 1)
        {
            trash1.SetActive(true);
        }
        if (nbTrashs >= 2)
        {
            trash2.SetActive(true);
        }
        if (nbTrashs >= 3)
        {
            trash3.SetActive(true);
        }
        if (nbTrashs >= 4)
        {
            trash4.SetActive(true);
        }
    }
    
    [PunRPC]
    //Add card to the list of card
    void UndoCard(int OB, int nbTrashs)
    {
        // Undo.DestroyObjectImmediate(PhotonView.Find(OB).gameObject);
        PhotonView.Find(OB).gameObject.SetActive(true);

        if (nbTrashs <= 1)
        {
            trash1.SetActive(false);
        }
        if (nbTrashs <= 2)
        {
            trash2.SetActive(false);
        }
        if (nbTrashs <= 3)
        {
            trash3.SetActive(false);
        }
        if (nbTrashs <=4)
        {
            trash4.SetActive(false);
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LoadingCard : MonoBehaviour
{
    object[] textures;

    static float w = 0.033f;
    static float h = 0.239f;
    static int cardPerLine = Mathf.RoundToInt(rendering.cardPerWall/2);
    float minimalCardX = -0.35f -w;
    float cardEspacement;

    [PunRPC]
    void LoadCard( int OB, int wallViewID, int pos, int i)
    {
        if (textures == null) // load one time the texture
        {
            /*
            bool card1 = GameObject.Find("/Salle").GetComponent<rendering>().card1;
            bool training = GameObject.Find("/Salle").GetComponent<rendering>().training;
            if (training)
            {
                textures = Resources.LoadAll("dixit_training/", typeof(Texture2D));
            }
            else if (card1)
            {
            }
            else
            {
                textures = Resources.LoadAll("dixit_part2/", typeof(Texture2D));
            }
            */
            textures = Resources.LoadAll("dixit_all/", typeof(Texture2D));
        }


        if (rendering.cardPerWall % 2 == 0)
        {
            cardEspacement = 0.7f / (cardPerLine - 1);
        }
        else
        {
            cardEspacement = 0.7f / (cardPerLine);
        }

        // wall + card
        Transform mur = PhotonView.Find(wallViewID).transform;
        GameObject goCard = PhotonView.Find(OB).gameObject;


        //set the texture
        Texture2D tex = (Texture2D)textures[i];
        goCard.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);

        //height and width depending on the size of te wall
        
        Vector3 v = mur.localScale;
        //w = w * (v.y / v.x);

        //set parent, rotation , name , local scale
        goCard.transform.parent = mur;
        goCard.transform.rotation = mur.rotation;
        goCard.name = "Card " + i;
       
        goCard.transform.localScale = new Vector3(w, h, 1.0f);
        if (pos < cardPerLine) //10 card per ligne
        {
            goCard.transform.localPosition = new Vector3(minimalCardX + w + cardEspacement * pos,        -1 * h, -0.01f);
        }
        else
        {
            pos = pos - cardPerLine;
            goCard.transform.localPosition = new Vector3(minimalCardX + w + cardEspacement * pos,        1 * h, -0.01f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardsLoading : MonoBehaviour {
    //all cards' textures
    object[] textures;

    //size of cards
    private static float width = 0.033f;
    private static float height = 0.239f;

    //cards dispositions values
    private static int card_per_line = Mathf.RoundToInt(Rendering.card_per_wall/2);
    private float card_spacing;
    private float min_card_x = -0.35f - width;

    //card loading method
    [PunRPC]
    private void LoadCards(int card_pv_id, int wall_pv_id, int pos, int card_index){
        //loading textures
        if(textures==null){
            textures = Resources.LoadAll("dixit_all/", typeof(Texture2D));
        }
        //defining spacing attribute
        card_spacing = 0.7f / (card_per_line - ((int)(Rendering.card_per_wall%2)));

        //getting the concerned wall & card
        Transform wall = PhotonView.Find(wall_pv_id).transform;
        GameObject card = PhotonView.Find(card_pv_id).gameObject;

        //setting the texture
        Texture2D tex = (Texture2D)textures[card_index];
        card.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);

        //setting the card's attributes 
        card.transform.parent = wall;
        card.transform.rotation = wall.rotation;
        card.name = "Card " + card_index;
        card.transform.localScale = new Vector3(width, height, 1.0f);

        //setting card's position
        if(pos < card_per_line){
            card.transform.localPosition = new Vector3(min_card_x + width + card_spacing * pos, -height, -0.01f);
        } else {
            pos = pos - card_per_line;
            card.transform.localPosition = new Vector3(min_card_x + width + card_spacing * pos, height, -0.01f);
        }
    }
}
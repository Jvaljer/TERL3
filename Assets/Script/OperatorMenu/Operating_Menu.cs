using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operating_Menu : MonoBehaviour {

    private GameObject title;

    private GameObject Options; //Canvas which will be shown throughout the program

    private GameObject Canvas_Operator; //all components of "Canvas with Operator"
    private GameObject Canvas_Options; //all components of "Canvas Option"
    private GameObject Canvas_Demo; //all components of "Canvas Demo"
    private GameObject Canvas_Expe_1; //all components of "Canvas Expe 1"
    private GameObject Canvas_Expe_2; //all components of "Canvas Expe 2"

    private GameObject Current_Canvas; //actual canva we wanna show when needed
    private GameObject Canvas_name;

    // Start is called before the first frame update
    void Start(){
        Canvas_Operator = GameObject.Find("Canvas with Operator");
        Canvas_Options = GameObject.Find("Canvas Option");
        Canvas_Demo = GameObject.Find("Canvas Demo");
        Canvas_Expe_1 = GameObject.Find("Canvas Expe 1");
        Canvas_Expe_2 = GameObject.Find("Canvas Expe 2");

        Current_Canvas = Canvas_Operator;
        Current_Canvas.gameObject.SetActive(true);

        Canvas_Options.gameObject.SetActive(false);
        Canvas_Demo.gameObject.SetActive(false);
        Canvas_Expe_1.gameObject.SetActive(false);
        Canvas_Expe_2.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update(){
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Text gCost;
    public Text hCost;
    public Text fCost;

    private void Start()
    {
        //if debug mode is not turned on
        if(!GameManager.instance.debugMode)
        {
            //if we have a gCost text
            if(gCost != null)
            {
                //disable it
                gCost.enabled = false;
            }

            //if we have an hCost text
            if(hCost != null)
            {
                //diable it
                hCost.enabled = false;
            }

            //if we have an fCost text
            if(fCost != null)
            {
                //disable it
                fCost.enabled = false;
            }
        }
    }

    public void SetGText(string value)
    {
        //set the GText to the specified value
        gCost.text = value;
    }

    public void SetHText(string value)
    {
        //set the HText to the specified value
        hCost.text = value;
    }

    public void SetFText(string value)
    {
        //set the FText to a specified value
        fCost.text = value;
    }
}

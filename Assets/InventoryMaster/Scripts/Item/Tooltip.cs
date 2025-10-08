using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public Item item;

    //GUI
    [SerializeField]
    public Image tooltipBackground;
    [SerializeField]
    public Text tooltipNameText;
    [SerializeField]
    public Text tooltipDescText;

    //Tooltip Settings
    [SerializeField]
    private bool showTooltipIcon;
    [SerializeField]
    private bool showTooltipName;
    [SerializeField]
    private bool showTooltipDesc;

    //Tooltip Objects
    [SerializeField]
    private RectTransform tooltipRectTransform;
    [SerializeField]
    private GameObject tooltipTextName;
    [SerializeField]
    private GameObject tooltipTextDesc;
    [SerializeField]
    private GameObject tooltipImageIcon;

    void Start()
    {
        deactivateTooltip();
    }

    public void activateTooltip()               //if you activate the tooltip through hovering over an item
    {
        tooltipTextName.SetActive(true);
        tooltipImageIcon.SetActive(true);
        tooltipTextDesc.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(true);          //Tooltip getting activated
        transform.GetChild(1).GetComponent<Image>().sprite = item.itemIcon;         //and the itemIcon...
        transform.GetChild(2).GetComponent<Text>().text = item.itemName;            //,itemName...
        transform.GetChild(3).GetComponent<Text>().text = item.itemDesc;            //and itemDesc is getting set        
    }

    public void deactivateTooltip()             //deactivating the tooltip after you went out of a slot
    {
        tooltipTextName.SetActive(false);
        tooltipImageIcon.SetActive(false);
        tooltipTextDesc.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);
    }
}

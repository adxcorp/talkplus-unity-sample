using UnityEngine;
using TalkPlus;

public class TalkPlusSample : MonoBehaviour
{
    public GameObject loginPanel, mainPanel, menuPopup, channelPanel;
    
    void Awake()
    {
        TalkPlusApi.Init("875bd0c3-83eb-4086-b7ba-a1a8b05a26fe");
    }

    void Start()
    {
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
        menuPopup.SetActive(false);
        channelPanel.SetActive(false);
    }
}
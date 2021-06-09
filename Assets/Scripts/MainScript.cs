using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TalkPlus;

public class MainScript : MonoBehaviour
{
    public Button menuButton, closeButton;
    public RectTransform contentRect;

    public GameObject channelListItem;
    private List<GameObject> channelList = new List<GameObject>();

    public MenuPopup menuPopup;
    public LoginScript loginScript;
    public ChannelScript channelScript;

    private void Start()
    {
        menuButton.onClick.AddListener(() => { menuPopup.OpenMenuPopup(); });
        closeButton.onClick.AddListener(() => { Logout(); });
    }

    private void Logout()
    {
        TalkPlusApi.Logout(() =>
        {
            PlayerPrefs.SetString(CommonUtil.KEY_USER_ID, null);
            PlayerPrefs.SetString(CommonUtil.KEY_USER_NAME, null);

            ResetChannelList();

            gameObject.SetActive(false);
            loginScript.OpenLogin();

        }, (int statusCode, Exception e) => { });
    }

    public void OpenChannelList()
    {
        gameObject.SetActive(true);
        menuPopup.gameObject.SetActive(false);

        GetChannelList(null);
    }

    private void ResetChannelList()
    {
        foreach (GameObject channelItem in channelList)
        {
            Destroy(channelItem);
        }

        channelList.Clear();
    }

    private void GetChannelList(TPChannel lastChannel)
    {
        if (lastChannel == null)
        {
            ResetChannelList();
        }

        TalkPlusApi.GetChannelList(lastChannel, (List<TPChannel> tpChannels) =>
        {
            foreach (TPChannel tpChannel in tpChannels)
            {
                GameObject channelObject = Instantiate(channelListItem, contentRect.transform) as GameObject;
                ListItemScript channelItem = channelObject.GetComponent<ListItemScript>();

                string usersText = CommonUtil.GetAttendees(tpChannel);
                channelItem.userText.text = "참여자: " + usersText;

                TPMessage message = tpChannel.GetLastMessage();
                if (message != null && message.GetText().Length > 0)
                {
                    channelItem.messageText.text = message.GetText();
                    long createAt = message.GetCreatedAt();
                    string date = CommonUtil.GetFormattedTime(createAt);
                    channelItem.dateText.text = "date: " + date;
                }
                else
                {
                    channelItem.messageText.text = "no message";
                    channelItem.dateText.text = "date: ";
                }

                string unreadCount = "unread Count: " + tpChannel.GetUnreadCount().ToString();
                channelItem.unreadCountText.text = unreadCount;

                channelItem.actionButton.onClick.AddListener(() =>
                {
                    ResetChannelList();
                    gameObject.SetActive(false);

                    channelScript.OpenMessageList(tpChannel);
                });

                channelList.Add(channelObject);
            }

        }, (int statusCode, Exception e) => { });
    }
}

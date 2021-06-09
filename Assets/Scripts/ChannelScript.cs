using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TalkPlus;

public class ChannelScript : MonoBehaviour
{
    public Text titleText;
    public Button backButton, closeButton;
    public RectTransform contentRect;
    public Scrollbar scrollbar;
    public GameObject messageListItem, myMessageListItem;
    public InputField messageInputField;
    public Button sendButton;

    private List<GameObject> messageList = new List<GameObject>();
    private TPChannel channel;
    private string currentUserId;

    public MainScript mainScript;

    void Start()
    {
        backButton.onClick.AddListener(() =>
        {
            ResetMessageList();
            TalkPlusApi.OnMessageReceived -= OnMessageReceived;

            gameObject.SetActive(false);
            mainScript.OpenChannelList();
        });

        closeButton.onClick.AddListener(() => { LeaveChannel(); });
        sendButton.onClick.AddListener(() => { SendMessage(); });
    }

    public void OpenMessageList(TPChannel tpChannel)
    {
        gameObject.SetActive(true);

        channel = tpChannel;
        currentUserId = PlayerPrefs.GetString(CommonUtil.KEY_USER_ID);
        titleText.text = CommonUtil.GetAttendees(tpChannel);

        TalkPlusApi.OnMessageReceived += OnMessageReceived;

        MarkRead();
        GetMessageList(null);
    }

    private void MarkRead()
    {
        TalkPlusApi.MarkAsReadChannel(channel, (TPChannel tpChannel) =>
        {
            channel = tpChannel;
        }, (int statusCode, Exception e) => { });
    }

    private void ResetMessageList()
    {
        foreach (GameObject messageItem in messageList)
        {
            Destroy(messageItem);
        }
        messageList.Clear();
    }

    private void GetMessageList(TPMessage lastMessage)
    {
        if (channel != null)
        {
            if (lastMessage == null)
            {
                ResetMessageList();
            }

            TalkPlusApi.GetMessageList(channel, lastMessage, (List<TPMessage> tpMessages) =>
            {
                if (tpMessages != null && tpMessages.Count > 0)
                {
                    tpMessages.Reverse();
                    foreach (TPMessage tpMessage in tpMessages) { AddMessageToList(tpMessage); }

                    Invoke(nameof(ScrollToBottom), 0.05f);
                }
            }, (int statusCode, Exception e) => { });
        }
    }

    private void SendMessage()
    {
        string message = messageInputField.text;

        if (!string.IsNullOrEmpty(message))
        {
            TalkPlusApi.SendMessage(channel, message, TPMessage.TYPE_TEXT, null, (TPMessage tpMessage) =>
            {
                messageInputField.text = null;
                AddMessageToList(tpMessage);
                MarkRead();
                Invoke(nameof(ScrollToBottom), 0.05f);

            }, (int statusCode, Exception e) => { });
        }
    }

    private void AddMessageToList(TPMessage tpMessage)
    {
        if (tpMessage != null)
        {
            Debug.Log("AddMessageToList");
            string senderId = tpMessage.GetUserId();
            GameObject gameObject = currentUserId.Equals(senderId) ? myMessageListItem : messageListItem;
            GameObject messageObject = Instantiate(gameObject, contentRect.transform) as GameObject;
            ListItemScript messageItem = messageObject.GetComponent<ListItemScript>();

            if (!currentUserId.Equals(senderId))
            {
                messageItem.userText.text = tpMessage.GetUsername();
            }

            messageItem.messageText.text = tpMessage.GetText();
            messageItem.dateText.text = CommonUtil.GetFormattedTime(tpMessage.GetCreatedAt());
            int unreadCount = channel.GetMessageUnreadCount(tpMessage);
            messageItem.unreadCountText.text = unreadCount > 0 ? unreadCount.ToString() : null;

            LayoutRebuilder.ForceRebuildLayoutImmediate(messageItem.itemRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            messageList.Add(messageObject);
        }
    }

    private void LeaveChannel()
    {
        TalkPlusApi.LeaveChannel(channel, true, () =>
        {
            ResetMessageList();
            TalkPlusApi.OnMessageReceived -= null;

            gameObject.SetActive(false);
            mainScript.OpenChannelList();

        }, (int statusCode, Exception e) => { });
    }

    private void ScrollToBottom()
    {
        scrollbar.value = 0;
    }

    private void OnMessageReceived(object sender, TPChannelMessageArgs args)
    {
        Debug.Log("OnMessageReceived");

        if (args.tpChannel.GetChannelId().Equals(channel.GetChannelId())) {
            AddMessageToList(args.tpMessage);
            MarkRead();
            Invoke(nameof(ScrollToBottom), 0.05f);
        }
    }
}
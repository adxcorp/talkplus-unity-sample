using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TalkPlus;

public class MenuPopup : MonoBehaviour
{
    public Text titleText;
    public Button backButton, closeButton;

    public GameObject menuPanel;
    public Button privateButton, publicButton, invitationCodeButton, joinPublicButton, joinInvitationCodeButton;

    public GameObject invitePanel;
    public InputField channelIdInputField, invitationCodeInputField, userId1InputField, userId2InputField, userId3InputField;
    public Button doneButton;

    public MainScript mainScript;
    public ChannelScript channelScript;

    enum MENU_TYPE
    {
        PRIVATE, PUBLIC, INVITATION_CODE, JOIN_PUBLIC, JOIN_INVITATION_CODE
    }

    private MENU_TYPE menuType;

    void Start()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        backButton.gameObject.SetActive(false);
        menuPanel.SetActive(true);
        invitePanel.SetActive(false);

        backButton.onClick.AddListener(() =>
        {
            backButton.gameObject.SetActive(false);
            titleText.text = "Menu";
            menuPanel.SetActive(true);
            invitePanel.SetActive(false);
        });

        closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });
        privateButton.onClick.AddListener(() => { SetMenuType(MENU_TYPE.PRIVATE); });
        publicButton.onClick.AddListener(() => { SetMenuType(MENU_TYPE.PUBLIC); });
        invitationCodeButton.onClick.AddListener(() => { SetMenuType(MENU_TYPE.INVITATION_CODE); });
        joinPublicButton.onClick.AddListener(() => { SetMenuType(MENU_TYPE.JOIN_PUBLIC); });
        joinInvitationCodeButton.onClick.AddListener(() => { SetMenuType(MENU_TYPE.JOIN_INVITATION_CODE); });
        doneButton.onClick.AddListener(() =>
        {
            switch (menuType)
            {
                case MENU_TYPE.PRIVATE:
                    CreateChannel(TPChannel.TYPE_PRIVATE);
                    break;

                case MENU_TYPE.PUBLIC:
                    CreateChannel(TPChannel.TYPE_PUBLIC);
                    break;

                case MENU_TYPE.INVITATION_CODE:
                    CreateChannel(TPChannel.TYPE_INVITATION_ONLY);
                    break;

                case MENU_TYPE.JOIN_PUBLIC:
                    JoinChannel();
                    break;

                case MENU_TYPE.JOIN_INVITATION_CODE:
                    JoinChannel();
                    break;
            }
        });
    }

    public void OpenMenuPopup()
    {
        gameObject.SetActive(true);
        menuPanel.SetActive(true);
        invitePanel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    private void SetMenuType(MENU_TYPE type)
    {
        menuType = type;

        channelIdInputField.text = null;
        invitationCodeInputField.text = null;
        userId1InputField.text = null;
        userId2InputField.text = null;
        userId3InputField.text = null;

        backButton.gameObject.SetActive(true);
        menuPanel.SetActive(false);
        invitePanel.SetActive(true);

        switch (type)
        {
            case MENU_TYPE.PRIVATE:
                titleText.text = "Create Private Channel";
                channelIdInputField.gameObject.SetActive(false);
                invitationCodeInputField.gameObject.SetActive(false);
                userId1InputField.gameObject.SetActive(true);
                userId2InputField.gameObject.SetActive(true);
                userId3InputField.gameObject.SetActive(true);
                break;

            case MENU_TYPE.PUBLIC:
                titleText.text = "Create Public Channel";
                channelIdInputField.gameObject.SetActive(false);
                invitationCodeInputField.gameObject.SetActive(false);
                userId1InputField.gameObject.SetActive(true);
                userId2InputField.gameObject.SetActive(true);
                userId3InputField.gameObject.SetActive(true);
                break;

            case MENU_TYPE.INVITATION_CODE:
                titleText.text = "Create invitationCode Channel";
                channelIdInputField.gameObject.SetActive(false);
                invitationCodeInputField.gameObject.SetActive(true);
                userId1InputField.gameObject.SetActive(true);
                userId2InputField.gameObject.SetActive(true);
                userId3InputField.gameObject.SetActive(true);
                break;

            case MENU_TYPE.JOIN_PUBLIC:
                titleText.text = "Join Public Channel";
                channelIdInputField.gameObject.SetActive(true);
                invitationCodeInputField.gameObject.SetActive(false);
                userId1InputField.gameObject.SetActive(false);
                userId2InputField.gameObject.SetActive(false);
                userId3InputField.gameObject.SetActive(false);
                break;

            case MENU_TYPE.JOIN_INVITATION_CODE:
                titleText.text = "Join invitationCode Channel";
                channelIdInputField.gameObject.SetActive(true);
                invitationCodeInputField.gameObject.SetActive(true);
                userId1InputField.gameObject.SetActive(false);
                userId2InputField.gameObject.SetActive(false);
                userId3InputField.gameObject.SetActive(false);
                break;
        }
    }

    private void CreateChannel(string channelType)
    {
        List<string> userIds = new List<string>();

        string userId1 = userId1InputField.text;
        string userId2 = userId2InputField.text;
        string userId3 = userId3InputField.text;
        string invitationCode = invitationCodeInputField.text ?? null;

        if (channelType.Equals(TPChannel.TYPE_INVITATION_ONLY) && string.IsNullOrEmpty(invitationCode))
        {
            return;
        }

        if (!string.IsNullOrEmpty(userId1)) { userIds.Add(userId1); }
        if (!string.IsNullOrEmpty(userId2)) { userIds.Add(userId2); }
        if (!string.IsNullOrEmpty(userId3)) { userIds.Add(userId3); }

        if (userIds.Count > 0)
        {
            TalkPlusApi.CreateChannel(userIds, null, true, 20, false, channelType, null, invitationCode, null, null, (TPChannel tpChannel) =>
            {
                mainScript.OpenChannelList();

            }, (int statusCode, Exception e) => { });
        }
    }

    private void JoinChannel()
    {
        string channelId = channelIdInputField.text;
        string invitationCode = invitationCodeInputField.text;

        if (!string.IsNullOrEmpty(channelId))
        {
            if (!string.IsNullOrEmpty(invitationCode))
            {
                TalkPlusApi.JoinChannel(channelId, invitationCode, (TPChannel tpChannel) =>
                {
                    gameObject.SetActive(false);
                    mainScript.gameObject.SetActive(false);
                    channelScript.OpenMessageList(tpChannel);

                }, (int statusCode, Exception e) => { });
            }
            else
            {
                TalkPlusApi.JoinChannel(channelId, (TPChannel tpChannel) =>
                {
                    gameObject.SetActive(false);
                    mainScript.gameObject.SetActive(false);
                    channelScript.OpenMessageList(tpChannel);

                }, (int statusCode, Exception e) => { });
            }
        }
    }
}

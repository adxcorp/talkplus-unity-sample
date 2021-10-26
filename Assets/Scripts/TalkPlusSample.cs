//#define FIREBASE_MESSAGING

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TalkPlus;

#if FIREBASE_MESSAGING
using Firebase;
using Firebase.Messaging;
#endif

public class TalkPlusSample : MonoBehaviour {
    static readonly string KEY_USER_ID = "KeyUserId";
    static readonly string KEY_USER_NAME = "KeyUserName";

    enum PANEL_TYPE {
        LOGIN, MAIN, CHANNEL
    }

    public GameObject safeArea;
    Rect safeAreaRect;

    #region Login
    GameObject loginPanel;
    InputField userIdInputField;
    InputField nicknameInputField;
    Button loginButton;
    #endregion

    #region Main
    GameObject mainPanel;
    Button menuButton;
    Button closeButton;
    RectTransform mainContent;

    public GameObject channelItemPrefab;
    List<GameObject> channelList = new List<GameObject>();
    #endregion

    #region Channel
    GameObject channelPanel;
    Button channelBackButton;
    Button channelCloseButton;
    Text channelUserText;

    RectTransform channelContent;
    Scrollbar scrollbar;

    InputField messageInputField;
    Button fileButton, sendButton;

    public GameObject messageItemPrefab;
    List<GameObject> messageList = new List<GameObject>();
    TPChannel channel;
    #endregion

    #region Menu
    enum MENU_TYPE {
        PRIVATE, PUBLIC, INVITATION_CODE, JOIN_PUBLIC, JOIN_INVITATION_CODE
    }
    MENU_TYPE menuType;
    GameObject menuPopup;
    Text menuTitleText;

    GameObject menuPanel;
    Button menuBackButton;
    Button menuCloseButton;
    Button menu1Button;
    Button menu2Button;
    Button menu3Button;
    Button menu4Button;
    Button menu5Button;

    GameObject invitePanel;
    InputField channelIdInputField;
    InputField codeInputField;
    InputField userId1InputField;
    InputField userId2InputField;
    InputField userId3InputField;
    Button inviteDoneButton;
    #endregion

#if FIREBASE_MESSAGING
    static readonly string KEY_FCM_TOKEN = "KeyFCMToken";
    FirebaseApp app;
#endif

    void Start() {
        Initialize();
        InitializeComponent();
#if FIREBASE_MESSAGING
        InitializeFirebase();
#endif
    }

    void Update() {
        Rect rect = Screen.safeArea;

        if (rect != safeAreaRect) {
            ApplySafeArea(rect);
        }
    }

    void ApplySafeArea(Rect rect) {
        safeAreaRect = rect;

        Rect screenSafeArea = Screen.safeArea;

        Vector2 newAnchorMin = screenSafeArea.position;
        Vector2 newAnchorMax = screenSafeArea.position + screenSafeArea.size;
        newAnchorMin.x /= Screen.width;
        newAnchorMax.x /= Screen.width;
        newAnchorMin.y /= Screen.height;
        newAnchorMax.y /= Screen.height;

        RectTransform rectTransform = safeArea.GetComponent<RectTransform>();
        rectTransform.anchorMin = newAnchorMin;
        rectTransform.anchorMax = newAnchorMax;
    }

    void Initialize() {
        TalkPlusApi.Init("875bd0c3-83eb-4086-b7ba-a1a8b05a26fe");
    }

    void InitializeComponent() {
        #region Login UI
        loginPanel = GameObject.Find("TalkPlusSample/SafeArea/LoginPanel");
        userIdInputField = GameObject.Find("TalkPlusSample/SafeArea/LoginPanel/UserIdInputField").GetComponent<InputField>();
        nicknameInputField = GameObject.Find("TalkPlusSample/SafeArea/LoginPanel/NicknameInputField").GetComponent<InputField>();
        loginButton = GameObject.Find("TalkPlusSample/SafeArea/LoginPanel/LoginButton").GetComponent<Button>();
        loginButton.onClick.AddListener(() => { Login(userIdInputField.text, nicknameInputField.text); });
        #endregion

        #region Main UI
        mainPanel = GameObject.Find("TalkPlusSample/SafeArea/MainPanel");
        mainContent = GameObject.Find("TalkPlusSample/SafeArea/MainPanel/ScrollView/Viewport/Content").GetComponent<RectTransform>();
        menuButton = GameObject.Find("TalkPlusSample/SafeArea/MainPanel/Title/MenuButton").GetComponent<Button>();
        closeButton = GameObject.Find("TalkPlusSample/SafeArea/MainPanel/Title/CloseButton").GetComponent<Button>();

        menuButton.onClick.AddListener(() => { ShowMenuPopup(true); });
        closeButton.onClick.AddListener(() => { Logout(); });

        #endregion

        #region Channel UI
        channelPanel = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel");
        channelUserText = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/Title/ChannelUserText").GetComponent<Text>();
        channelBackButton = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/Title/BackButton").GetComponent<Button>();
        channelCloseButton = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/Title/CloseButton").GetComponent<Button>();
        channelContent = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/ScrollView/Viewport/Content").GetComponent<RectTransform>();
        scrollbar = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/ScrollView/Scrollbar").GetComponent<Scrollbar>();
        messageInputField = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/Send/MessageInputField").GetComponent<InputField>();
        fileButton = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/Send/FileButton").GetComponent<Button>();
        sendButton = GameObject.Find("TalkPlusSample/SafeArea/ChannelPanel/Send/SendButton").GetComponent<Button>();

        channelBackButton.onClick.AddListener(() => {
            ResetMessageList();
            TalkPlusApi.OnMessageReceived -= OnMessageReceived;
            SetActivePanel(PANEL_TYPE.MAIN);
        });
        channelCloseButton.onClick.AddListener(() => { LeaveChannel(); });
        fileButton.onClick.AddListener(() => { SendFileMessage(); });
        sendButton.onClick.AddListener(() => { SendMessage(); });
        #endregion


        #region Menu UI
        menuPopup = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup");
        menuTitleText = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Title/TitleText").GetComponent<Text>();
        menuBackButton = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Title/BackButton").GetComponent<Button>();
        menuCloseButton = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Title/CloseButton").GetComponent<Button>();

        menuPanel = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Menu");
        menu1Button = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Menu/PrivateButton").GetComponent<Button>();
        menu2Button = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Menu/PublicButton").GetComponent<Button>();
        menu3Button = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Menu/CodeButton").GetComponent<Button>();
        menu4Button = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Menu/JoinPublicButton").GetComponent<Button>();
        menu5Button = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Menu/JoinCodeButton").GetComponent<Button>();

        invitePanel = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite");
        channelIdInputField = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite/ChannelIdInputField").GetComponent<InputField>();
        codeInputField = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite/CodeInputField").GetComponent<InputField>();
        userId1InputField = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite/UserId1InputField").GetComponent<InputField>();
        userId2InputField = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite/UserId2InputField").GetComponent<InputField>();
        userId3InputField = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite/UserId3InputField").GetComponent<InputField>();
        inviteDoneButton = GameObject.Find("TalkPlusSample/SafeArea/MenuPopup/Popup/Content/Invite/InviteDoneButton").GetComponent<Button>();

        menuBackButton.onClick.AddListener(() => {
            menuBackButton.gameObject.SetActive(false);
            menuTitleText.text = "Menu";
            menuPanel.SetActive(true);
            invitePanel.SetActive(false);
        });
        menuCloseButton.onClick.AddListener(() => { ShowMenuPopup(false); });
        menu1Button.onClick.AddListener(() => { SetMenuType(MENU_TYPE.PRIVATE); });
        menu2Button.onClick.AddListener(() => { SetMenuType(MENU_TYPE.PUBLIC); });
        menu3Button.onClick.AddListener(() => { SetMenuType(MENU_TYPE.INVITATION_CODE); });
        menu4Button.onClick.AddListener(() => { SetMenuType(MENU_TYPE.JOIN_PUBLIC); });
        menu5Button.onClick.AddListener(() => { SetMenuType(MENU_TYPE.JOIN_INVITATION_CODE); });
        inviteDoneButton.onClick.AddListener(() => {
            switch (menuType) {
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
        #endregion

        SetActivePanel(PANEL_TYPE.LOGIN);
    }

    void SetActivePanel(PANEL_TYPE type) {
        switch (type) {
            case PANEL_TYPE.LOGIN:
                loginPanel.SetActive(true);
                mainPanel.SetActive(false);
                channelPanel.SetActive(false);
                menuPopup.SetActive(false);
                ShowLogin();
                break;

            case PANEL_TYPE.MAIN:
                loginPanel.SetActive(false);
                mainPanel.SetActive(true);
                channelPanel.SetActive(false);
                menuPopup.SetActive(false);
                GetChannelList();
                break;

            case PANEL_TYPE.CHANNEL:
                loginPanel.SetActive(false);
                mainPanel.SetActive(false);
                channelPanel.SetActive(true);
                menuPopup.SetActive(false);
                break;
        }
    }

    #region Login

    void ShowLogin() {
        string userId = PlayerPrefs.GetString(KEY_USER_ID);
        string userName = PlayerPrefs.GetString(KEY_USER_NAME);
        Login(userId, userName);
    }

    void Login(string userId, string userName) {
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName)) {
            TalkPlusApi.LoginWithAnonymous(userId, userName, null, null, (TPUser tpUser) => {
                if (tpUser != null) {
#if FIREBASE_MESSAGING
                    RegisterFCMToken();
#endif
                    PlayerPrefs.SetString(KEY_USER_ID, userId);
                    PlayerPrefs.SetString(KEY_USER_NAME, userName);

                    SetActivePanel(PANEL_TYPE.MAIN);
                }

            }, (int statusCode, Exception e) => { });
        }
    }

    #endregion

    #region Main
    void GetChannelList(TPChannel lastChannel = null) {
        if (lastChannel == null) {
            ResetChannelList();
        }

        TalkPlusApi.GetChannelList(lastChannel, (List<TPChannel> tpChannels) => {
            foreach (TPChannel tpChannel in tpChannels) {
                GameObject channelObject = Instantiate(channelItemPrefab, mainContent.transform) as GameObject;
                ListItem channelItem = channelObject.GetComponent<ListItem>();

                string usersText = GetAttendees(tpChannel);
                channelItem.userText.text = "참여자: " + usersText;

                TPMessage message = tpChannel.GetLastMessage();
                if (message != null && message.GetText().Length > 0) {
                    channelItem.messageText.text = message.GetText();
                    long createAt = message.GetCreatedAt();
                    string date = GetFormattedTime(createAt);
                    channelItem.dateText.text = "date: " + date;
                } else {
                    channelItem.messageText.text = "no message";
                    channelItem.dateText.text = "date: ";
                }

                string unreadCount = "unread Count: " + tpChannel.GetUnreadCount().ToString();
                channelItem.unreadCountText.text = unreadCount;
                channelList.Add(channelObject);

                channelObject.GetComponent<Button>().onClick.AddListener(() => {
                    ResetChannelList();
                    SetActivePanel(PANEL_TYPE.CHANNEL);
                    ShowChannel(tpChannel);
                });

                LayoutRebuilder.ForceRebuildLayoutImmediate(mainContent);
            }

        }, (int statusCode, Exception e) => { });
    }

    void ResetChannelList() {
        foreach (GameObject item in channelList) {
            Destroy(item);
        }

        channelList.Clear();
    }

    void Logout() {
        TalkPlusApi.Logout(() => {
            PlayerPrefs.SetString(KEY_USER_ID, null);
            PlayerPrefs.SetString(KEY_USER_NAME, null);

            ResetChannelList();
            SetActivePanel(PANEL_TYPE.LOGIN);

        }, (int statusCode, Exception e) => { });
    }

    #endregion

    #region Channel
    void ShowChannel(TPChannel tpChannel) {
        channel = tpChannel;
        channelUserText.text = GetAttendees(tpChannel);

        TalkPlusApi.OnMessageReceived += OnMessageReceived;

        MarkRead();
        GetMessageList();
    }

    void MarkRead() {
        TalkPlusApi.MarkAsReadChannel(channel, (TPChannel tpChannel) => {
            channel = tpChannel;

        }, (int statusCode, Exception e) => { });
    }

    void GetMessageList(TPMessage lastMessage = null) {
        if (channel != null) {
            if (lastMessage == null) {
                ResetMessageList();
            }

            TalkPlusApi.GetMessageList(channel, lastMessage, (List<TPMessage> tpMessages) => {
                if (tpMessages != null && tpMessages.Count > 0) {
                    tpMessages.Reverse();
                    foreach (TPMessage tpMessage in tpMessages) { AddMessageToList(tpMessage); }

                    Invoke(nameof(ScrollToBottom), 0.05f);
                }
            }, (int statusCode, Exception e) => { });
        }
    }

    void ResetMessageList() {
        foreach (GameObject item in messageList) {
            Destroy(item);
        }
        messageList.Clear();
    }

    async Task<Texture2D> GetRemoteTexture(string url) {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();

        while (asyncOperation.isDone == false)
            await Task.Delay(1000 / 30);

        if (request.isNetworkError || request.isHttpError) {
#if DEBUG
            Debug.Log($"{request.error}, URL:{request.url}");
#endif
            return null;
        } else {
            return DownloadHandlerTexture.GetContent(request);
        }
    }

    async void AddMessageToList(TPMessage tpMessage) {
        if (tpMessage != null) {
            GameObject messageObject = Instantiate(messageItemPrefab, channelContent.transform) as GameObject;
            ListItem messageItem = messageObject.GetComponent<ListItem>();

            messageItem.userText.text = tpMessage.GetUsername();
            messageItem.messageText.text = tpMessage.GetText();
            messageItem.dateText.text = GetFormattedTime(tpMessage.GetCreatedAt());
            int unreadCount = channel.GetMessageUnreadCount(tpMessage);
            messageItem.unreadCountText.text = unreadCount > 0 ? unreadCount.ToString() : null;

            string fileUrl = tpMessage.GetFileUrl();
            if (!string.IsNullOrEmpty(fileUrl)) {
                Texture2D texture = await GetRemoteTexture(fileUrl);
                messageItem.fileImage.texture = texture;
                messageItem.fileImage.SetNativeSize();
            }
            messageList.Add(messageObject);

            LayoutRebuilder.ForceRebuildLayoutImmediate(channelContent);
        }
    }

    void AddMessage(TPMessage tpMessage) {
        if (tpMessage != null) {
            AddMessageToList(tpMessage);
            MarkRead();
            Invoke(nameof(ScrollToBottom), 0.05f);
        }
    }

    void SendFileMessage() {
        string filePath = Application.dataPath + "/Resources/logo.png";
        string message = messageInputField.text;

        TalkPlusApi.SendFileMessage(channel, message, TPMessage.TYPE_TEXT, null, filePath, (TPMessage tpMessage) => {
            messageInputField.text = null;
            AddMessage(tpMessage);

        }, (int statusCode, Exception e) => { });
    }

    void SendMessage() {
        string message = messageInputField.text;

        if (!string.IsNullOrEmpty(message)) {
            TalkPlusApi.SendMessage(channel, message, TPMessage.TYPE_TEXT, null, (TPMessage tpMessage) => {
                messageInputField.text = null;
                AddMessage(tpMessage);

            }, (int statusCode, Exception e) => { });
        }
    }

    void LeaveChannel() {
        TalkPlusApi.LeaveChannel(channel, true, () => {
            ResetMessageList();
            TalkPlusApi.OnMessageReceived -= null;

            SetActivePanel(PANEL_TYPE.MAIN);

        }, (int statusCode, Exception e) => { });
    }

    void ScrollToBottom() {
        scrollbar.value = 0;
    }

    void OnMessageReceived(object sender, TPChannelMessageArgs args) {
        if (args.tpChannel.GetChannelId().Equals(channel.GetChannelId())) {
            AddMessage(args.tpMessage);
        }
    }

    #endregion

    #region Menu
    void ShowMenuPopup(bool isShow) {
        menuPopup.SetActive(isShow);

        if (isShow) {
            menuTitleText.text = "Menu";
            menuPanel.SetActive(true);
            invitePanel.SetActive(false);

            menuBackButton.gameObject.SetActive(false);
        }
    }

    void SetMenuType(MENU_TYPE type) {
        menuType = type;

        channelIdInputField.text = null;
        codeInputField.text = null;
        userId1InputField.text = null;
        userId2InputField.text = null;
        userId3InputField.text = null;

        menuBackButton.gameObject.SetActive(true);
        menuPanel.SetActive(false);
        invitePanel.SetActive(true);

        switch (type) {
            case MENU_TYPE.PRIVATE:
                menuTitleText.text = "Create Private Channel";
                channelIdInputField.gameObject.SetActive(false);
                codeInputField.gameObject.SetActive(false);
                userId1InputField.gameObject.SetActive(true);
                userId2InputField.gameObject.SetActive(true);
                userId3InputField.gameObject.SetActive(true);
                break;

            case MENU_TYPE.PUBLIC:
                menuTitleText.text = "Create Public Channel";
                channelIdInputField.gameObject.SetActive(false);
                codeInputField.gameObject.SetActive(false);
                userId1InputField.gameObject.SetActive(true);
                userId2InputField.gameObject.SetActive(true);
                userId3InputField.gameObject.SetActive(true);
                break;

            case MENU_TYPE.INVITATION_CODE:
                menuTitleText.text = "Create invitationCode Channel";
                channelIdInputField.gameObject.SetActive(false);
                codeInputField.gameObject.SetActive(true);
                userId1InputField.gameObject.SetActive(true);
                userId2InputField.gameObject.SetActive(true);
                userId3InputField.gameObject.SetActive(true);
                break;

            case MENU_TYPE.JOIN_PUBLIC:
                menuTitleText.text = "Join Public Channel";
                channelIdInputField.gameObject.SetActive(true);
                codeInputField.gameObject.SetActive(false);
                userId1InputField.gameObject.SetActive(false);
                userId2InputField.gameObject.SetActive(false);
                userId3InputField.gameObject.SetActive(false);
                break;

            case MENU_TYPE.JOIN_INVITATION_CODE:
                menuTitleText.text = "Join invitationCode Channel";
                channelIdInputField.gameObject.SetActive(true);
                codeInputField.gameObject.SetActive(true);
                userId1InputField.gameObject.SetActive(false);
                userId2InputField.gameObject.SetActive(false);
                userId3InputField.gameObject.SetActive(false);
                break;
        }
    }

    void CreateChannel(string channelType) {
        List<string> userIds = new List<string>();

        string userId1 = userId1InputField.text;
        string userId2 = userId2InputField.text;
        string userId3 = userId3InputField.text;
        string invitationCode = codeInputField.text ?? null;

        if (channelType.Equals(TPChannel.TYPE_INVITATION_ONLY) && string.IsNullOrEmpty(invitationCode)) {
            return;
        }

        if (!string.IsNullOrEmpty(userId1)) { userIds.Add(userId1); }
        if (!string.IsNullOrEmpty(userId2)) { userIds.Add(userId2); }
        if (!string.IsNullOrEmpty(userId3)) { userIds.Add(userId3); }

        if (userIds.Count > 0) {
            TalkPlusApi.CreateChannel(userIds, null, true, 20, false, channelType, null, invitationCode, null, null, (TPChannel tpChannel) => {
                SetActivePanel(PANEL_TYPE.MAIN);

            }, (int statusCode, Exception e) => { });
        }
    }

    void JoinChannel() {
        string channelId = channelIdInputField.text;
        string invitationCode = codeInputField.text;

        if (!string.IsNullOrEmpty(channelId)) {
            if (!string.IsNullOrEmpty(invitationCode)) {
                TalkPlusApi.JoinChannel(channelId, invitationCode, (TPChannel tpChannel) => {
                    SetActivePanel(PANEL_TYPE.CHANNEL);
                    ShowChannel(tpChannel);

                }, (int statusCode, Exception e) => { });
            } else {
                TalkPlusApi.JoinChannel(channelId, (TPChannel tpChannel) => {
                    SetActivePanel(PANEL_TYPE.CHANNEL);
                    ShowChannel(tpChannel);

                }, (int statusCode, Exception e) => { });
            }
        }
    }

    #endregion

    #region Common
    string GetAttendees(TPChannel tpChannel) {
        List<TPUser> users = tpChannel.GetMembers();
        if (users != null && users.Count > 0) {
            List<string> names = new List<string>();
            users.ForEach((TPUser user) => { names.Add(user.GetUsername()); });
            string attendees = string.Join(", ", names);

            return attendees;
        }

        return null;
    }

    string GetFormattedTime(long milliseconds) {
        if (milliseconds > 0) {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(milliseconds).ToLocalTime();
            return string.Format("{0:yyyy/MM/dd HH:mm}", date);
        } else {
            return null;
        }
    }
    #endregion

#if FIREBASE_MESSAGING
    #region Firebase
    void InitializeFirebase() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                app = FirebaseApp.DefaultInstance;
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;
            } else {
                Debug.Log("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void RegisterFCMToken() {
        string fcmToken = PlayerPrefs.GetString(KEY_FCM_TOKEN);

        if (!string.IsNullOrEmpty(fcmToken)) {
            TalkPlusApi.RegisterFCMToken(fcmToken, () => {

            }, (errorCode, error) => { });
        }
    }

    public static void OnTokenReceived(object sender, TokenReceivedEventArgs token) {
        string fcmToken = token.Token;
        PlayerPrefs.SetString(KEY_FCM_TOKEN, fcmToken);
    }

    public static void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
        if (e.Message.Data.ContainsKey("talkplus")) {
            TalkPlusApi.ProcessFirebaseCloudMessagingData(e.Message.Data);
        }
    }
    #endregion
#endif
}

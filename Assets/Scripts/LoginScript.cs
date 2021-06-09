using System;
using UnityEngine;
using UnityEngine.UI;
using TalkPlus;

public class LoginScript : MonoBehaviour
{
    public InputField userIdInputField;
    public InputField nicknameInputField;
    public Button loginButton;
    public MainScript mainScript;

    void Start()
    {
        loginButton.onClick.AddListener(() => { Login(userIdInputField.text, nicknameInputField.text); });

        string userId = PlayerPrefs.GetString(CommonUtil.KEY_USER_ID);
        string userName = PlayerPrefs.GetString(CommonUtil.KEY_USER_NAME);

        Login(userId, userName);
    }

    public void OpenLogin()
    {
        gameObject.SetActive(true);
        userIdInputField.text = null;
        nicknameInputField.text = null;
    }

    private void Login(string userId, string userName)
    {
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName))
        {
            TalkPlusApi.LoginWithAnonymous(userId, userName, null, null, (TPUser tpUser) =>
            {
                if (tpUser != null) {
                PlayerPrefs.SetString(CommonUtil.KEY_USER_ID, userId);
                PlayerPrefs.SetString(CommonUtil.KEY_USER_NAME, userName);

                userIdInputField.text = null;
                nicknameInputField.text = null;
                gameObject.SetActive(false);

                mainScript.OpenChannelList();
            }

            }, (int statusCode, Exception e) => { });
        }
    }
}

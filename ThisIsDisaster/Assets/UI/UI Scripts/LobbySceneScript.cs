﻿
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class LobbySceneScript : MonoBehaviour {

    //private string url = "http://api.thisisdisaster.com/user";
    private string result = null;

    public Text PlayerName;
    public Text PlayerLevel;
    public Text PlayerExp;


    public string nickname = "DefaultName";
    public int level = 1;
    public int exp = 0;
    public int gold = 0;

    int resultCode = 0;
    string resultMsg;

    // Use this for initialization
    void Start ()
    {
        StartCoroutine("LoadUser");
    }
	


	// Update is called once per frame
	void Update ()
    {
        SetUI();
    }

    Json.WebCommunicationManager WebManager
    {
        get
        {
            return Json.WebCommunicationManager.Manager;
        }
    }


    IEnumerator LoadUser()
    {
        while (true)
        {
            string email = GlobalParameters.Param.accountEmail;

            WebManager.SendRequest(Json.RequestMethod.GET, "user?email=" + email, "");

            yield return new WaitForSeconds(30);
            Debug.Log("Sending");
        }
    }

    void SetUI()
    {
            PlayerName.text = GlobalParameters.Param.accountName;
            PlayerLevel.text = GlobalParameters.Param.accountLevel.ToString();
            PlayerExp.text = GlobalParameters.Param.accountExp.ToString();
    }
}

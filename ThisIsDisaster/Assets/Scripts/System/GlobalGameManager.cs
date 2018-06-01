﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkComponents;

public enum GameState
{
    Lobby,
    Stage,
    None
}

public enum GameNetworkType {
Single,
Multi,
None//Lobby etc
}

public class GlobalParameters : ISavedData {
    public static GlobalParameters Param { get { return GlobalGameManager.Param; } }
    public int accountId = 0;
    public string accountName = "Player";
    public bool isHost = false;
    public bool isConnected = false;
    public bool isDisconnnected = false;

    public int AdditionalPortNum = 0;
    
    public void Init() { }

    public Dictionary<string, object> GetSavedData()
    {
        Dictionary<string, object> output = new Dictionary<string, object>();
        output.Add("accountId", accountId);
        output.Add("accountName", accountName);
        return output;
    }

    public void LoadData(Dictionary<string, object> data)
    {
        FileManager.TryGetValue(data, "accountId", ref accountId);
        FileManager.TryGetValue(data, "accountName", ref accountName);
    }

    public string GetPath()
    {
        return "globalParam";
    }
}

public class GlobalGameManager {

    private static GlobalGameManager _instance = null;
    public static GlobalGameManager Instance {
        get {
            if (_instance == null) {
                _instance = new GlobalGameManager();
                _instance.Init();
            }
            return _instance;
        }
    }

    public static GlobalParameters Param {
        get { return Instance.param; }
    }

    public DevelopmentConsole developmentConsole;
    public GlobalParameters param = null;
    
    [ReadOnly]
    public GameState GameState = GameState.Lobby;

    public GameNetworkType GameNetworkType {
        private set;
        get;
    }

    private void Init()
    {
        //if (Instance != null) {
        //    //Destroy(gameObject); return;
        //}
        //Instance = this;
        //DontDestroyOnLoad(gameObject);

        param = new GlobalParameters();
        param.Init();

        GameStaticDataLoader.Loader.LoaderInit();
        //LocalizeTextDataModel.Instance.LogAllData();

        GameObject networkObject = GameObject.Find("NetworkModule");
        if (networkObject) {
            NetworkModule network = networkObject.GetComponent<NetworkModule>();
            if (network) {
                
            }
        }

        if (FileManager.Instance.ExistFile(GlobalParameters.Param)) {
            FileManager.Instance.LoadData(GlobalParameters.Param);
        }

#if NET_DEV
        string texts = FileManager.Instance.GetLocalTextData(GlobalParameters.Param.GetPath(), ".txt");

        if (!string.IsNullOrEmpty(texts))
        {
            string[] split = texts.Split(' ', '\n');
            //split[1] - id
            //split[3] - name
            int id = GlobalParameters.Param.accountId;
            if (int.TryParse(split[1], out id))
            {
                GlobalParameters.Param.accountId = id;
            }
            GlobalParameters.Param.accountName = split[3];

            Debug.LogError("Set Global Param " + id + " " + GlobalParameters.Param.accountName);
        }
#endif
    }
    // Use this for initialization
    void Start () {
        //developmentConsole.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    if (developmentConsole.gameObject.activeInHierarchy)
        //    {
        //        developmentConsole.Close();
        //    }
        //    else
        //    {
        //        developmentConsole.Open();
        //    }
        //}
    }

    public void SetGameState(GameState state) {
        Debug.Log("Set State: " + state);
        GameState = state;
    }

    public void OnGameStart()
    {
        //syncrhonize game info
        //set game seed
        int randValue = UnityEngine.Random.Range(0, int.MaxValue);
        StageGenerator.Instance.SetSeed(randValue);
        //check game state is Mulitplay or singlePlay
        //in this case, assume that game state is Multiplay
        if (NetworkModule.Instance != null) {
            if (GameServer.Instance != null) {
                SendSessionStartNotice();
                GameServer.Instance.ConnectUDPServer();
                GenerateWorld();
            }
        }
    }

    void SendSessionStartNotice() {
        StartSessionNotice notice = new StartSessionNotice() {
            sessionId = 0,//by aws server
            stageRandomSeed = StageGenerator.Instance.GetSeed()
        };
        StartSessionNoticePacket packet = new StartSessionNoticePacket(notice);
        NetworkModule.Instance.SendReliableToAll(packet);
    }

    public void GenerateWorld() {
        if (GameManager.CurrentGameManager != null) {
            GameManager.CurrentGameManager.Init();

            GameServer.Instance.MakeRemotePlayer();
        }
    }
}

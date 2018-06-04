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

    public bool IsHost {
        private set;
        get;
    }

    private void Init()
    {
        param = new GlobalParameters();
        param.Init();

        GameStaticDataLoader.Loader.LoaderInit();

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
            int id = GlobalParameters.Param.accountId;
            if (int.TryParse(split[1], out id))
            {
                GlobalParameters.Param.accountId = id;
            }
            GlobalParameters.Param.accountName = split[3];

            Debug.LogError("Set Global Param " + id + " " + GlobalParameters.Param.accountName);
        }
#endif
        //testing
        SetGameNetworkType(GameNetworkType.Multi);
    }

    public void SetHost(bool isHost) {
        IsHost = isHost;
    }

    public void SetGameNetworkType(GameNetworkType type) {
        this.GameNetworkType = type;
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
        if (NetworkModule.Instance != null)
        {
            if (GameServer.Instance != null)
            {
                //SendSessionStartNotice();

                //GameServer.Instance.ConnectUDPServer();
            }
        }
        if (GameNetworkType == GameNetworkType.Multi) {
            GameServer.Instance.ConnectUDPServer();
            SendSessionStartNotice();
        }
        
        LoadGameScene();
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

    public void LoadGameScene()
    {
        LoadingSceneManager.LoadScene("NPCTestScene");
        
    }

    public void LoadLobbyScene()
    {
        LoadingSceneManager.LoadScene("Lobby Scene");
    }

    public Dictionary<int, int> _remotePlayers = new Dictionary<int, int>();
    //other data need
    public void AddRemotePlayer(int clientNode) {
        _remotePlayers.Add(clientNode, clientNode);
    }

    public void ClearRemotePlayer() {
        _remotePlayers.Clear();
    }

    
}

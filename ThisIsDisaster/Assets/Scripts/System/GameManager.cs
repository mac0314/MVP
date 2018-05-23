﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefab {
    /// <summary>
    /// Find Gameobject in Prefabs/
    /// </summary>
    /// <param name="prefabSrc"></param>
    /// <returns></returns>
    public static GameObject LoadPrefab(string prefabSrc) {
        GameObject load = Resources.Load<GameObject>("Prefabs/" + prefabSrc);
        if (load == null) {
#if UNITY_EDITOR
            Debug.LogError("Could not find prefab : " + prefabSrc);
#endif
        }
        GameObject copy = GameObject.Instantiate(load);
        return copy;
    }
}

public class GameManager : MonoBehaviour {
    public static GameManager CurrentGameManager {
        private set;
        get;
    }

    public Dictionary<int, UnitControllerBase> RemotePlayer
    {
        get
        {
            return _remotePlayer;
        }
    }

    private UnitControllerBase _localPlayer;
    private Dictionary<int, UnitControllerBase> _remotePlayer = null;

    public UnitControllerBase CommonPlayerObject;

    public ClimateType CurrentStageClimateTpye = ClimateType.Island;

    private void Awake()
    {
        //Init();
    }
    
    /// <summary>
    //  
    /// </summary>
    public void Init() {

        CurrentGameManager = this;
        _remotePlayer = new Dictionary<int, UnitControllerBase>();

        //generate world by input
        CurrentStageClimateTpye = StageGenerator.Instance.GetRandomClimateType();

        StageGenerator.ClimateInfo info = StageGenerator.Instance.GetClimateInfo(CurrentStageClimateTpye);
        CellularAutomata.Instance.MaxHeightLevel = info.MaxHeightLevel;
        List<string> tileSrc = new List<string>(info.tileSpriteSrc.Values);
        RandomMapGenerator.Instance.SetTileSprite(tileSrc);

        CellularAutomata.Instance.GenerateMap();
        try
        {
            foreach (var env in info.envInfoList)
            {
                if (!EnvironmentManager.Manager.IsValidateId(env.id)) continue;
                int count = StageGenerator.Instance.ReadNextValue(env.min, env.max);
                var coords = CellularAutomata.Instance.GetRoomsCoord(env.height, count);

                for (int i = 0; i < count; i++)
                {
                    var model = EnvironmentManager.Manager.MakeEnvironment(env.id);
                    TileUnit tile = RandomMapGenerator.Instance.GetTile(coords[i].tileX, coords[i].tileY);
                    model.UpdatePosition(tile.transform.position);
                }
            }
        }
        catch (System.Exception e) {
#if UNITY_EDITOR
            Debug.LogError(e);
#endif
        }

        NPCManager.Manager.SetNpcGenInfo(info.npcInfoList);
        NPCManager.Manager.CheckGeneration();

        //make other

        var localPlayer = MakePlayerCharacter(GlobalParameters.Param.accountName,
            GlobalParameters.Param.accountId, true);
        
    }

    // Use this for initialization
    void Start () {

        if (NetworkComponents.NetworkModule.Instance != null)
        {
            NetworkComponents.NetworkModule.Instance.RegisterReceiveNotification(
                NetworkComponents.PacketId.Coordinates, OnReceiveCharacterCoordinate);
        }

        GlobalGameManager.Instance.SetGameState(GameState.Stage);

        Init();
    }
	
	// Update is called once per frame
	void Update () {
        Notice.Instance.Send(NoticeName.Update);
	}

    public UnitControllerBase GetLocalPlayer() {
        return _localPlayer;
    }

    public static UnitControllerBase MakePlayerCharacter(string name, int id, bool isLocal) {
        UnitControllerBase output = null;
        if (!isLocal)
        {
            if (CurrentGameManager.RemotePlayer.TryGetValue(id, out output))
            {
                return output;
            }
        }
        else
        {
            if (CurrentGameManager._localPlayer != null) {
                return CurrentGameManager._localPlayer;
            }
        }

        GameObject copy = Instantiate(CurrentGameManager.CommonPlayerObject.gameObject);
        copy.transform.SetParent(CurrentGameManager.transform);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.Euler(Vector3.zero);
        copy.transform.localScale = Vector3.one;
        output = copy.GetComponent<UnitControllerBase>();
        output.SetUnitName(name);

        PlayerMoveController moveScript = copy.GetComponent<PlayerMoveController>();
        if (moveScript) {
            if (!isLocal) {
                moveScript.enabled = false;
            }
        }

        if (isLocal)
        {
            output.behaviour.IsRemoteCharacter = false;
            CurrentGameManager._localPlayer = output;
            Notice.Instance.Send(NoticeName.LocalPlayerGenerated);
            //attach chase
            CurrentGameManager.MakeCameraMoveScript(output.gameObject);
        }
        else {
            output.behaviour.IsRemoteCharacter = true;
            CurrentGameManager.RemotePlayer.Add(id, output);
        }

        return output;
    }

    void MakeCameraMoveScript(GameObject attach) {
        Chasing script = Camera.main.gameObject.GetComponent<Chasing>();
        if (!script) {
            script = Camera.main.gameObject.AddComponent<Chasing>();
        }
        script.Target = attach;
    }

    public void OnReceiveCharacterCoordinate(NetworkComponents.PacketId packetId, int packetSender, byte[] data) {
        UnitControllerBase controller = null;
        if (RemotePlayer.TryGetValue(packetSender, out controller))
        {
            NetworkComponents.CharacterMovingPacket packet = new NetworkComponents.CharacterMovingPacket(data);
            NetworkComponents.CharacterData charData = packet.GetPacket();

            //Debug.LogError("Position Info " + packetSender);
            controller.OnReceiveCharacterCoordinate(charData);
        }
    }
}

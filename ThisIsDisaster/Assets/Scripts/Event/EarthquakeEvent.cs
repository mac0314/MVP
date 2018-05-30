﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EarthquakeEvent : EventBase
{
    GameObject quakeObject = null;          // 공통2  (맵흔들림)
    GameObject crackObject = null;          // 공통1 (갈라짐)

    TileUnit _originTile = null;
    EarthquakeEffect _effect = null;

    public EarthquakeEvent()
    {
        type = WeatherType.Earthquake;
    }

    public override void OnGenerated()
    {
        quakeObject = null;
        crackObject = null;

        _effect = EventManager.Manager.GetEarthquakeEffect();

        SetOriginTile();
        MakeBirdEffect();
    }

    public void MakeBirdEffect() {
        GameObject effect = Prefab.LoadPrefab("Events/BirdFlyingEffect");
        effect.transform.SetParent(EventManager.Manager.transform);
        Vector3 pos = _originTile.transform.position;
        effect.transform.position = pos;

        //effect.transform.localRotation = Quaternion.Euler(30f, 90f, 0f);
        float value = pos.x / (Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y));
        float c = Mathf.Acos(value) * Mathf.Rad2Deg;
        Debug.Log(c);
        Vector3 current = new Vector3(0f, 0f, c - 180f);
        //current.y = 0f;
        effect.transform.localRotation = Quaternion.Euler(current);
        effect.transform.localScale = Vector3.one;
        Debug.Log("Make Bird");
    }

    public void SetOriginTile() {
        TileUnit originTile = RandomMapGenerator.Instance.GetRandomTileByHeight(0);
        _originTile = originTile;
        _effect.SetOriginTile(originTile);
    }

	public override void OnStart()
	{
		_effect.SetActive(true);

        //_effect.SetEarthquakeType(EarthquakeEffect.EarthquakeType.Main, 3);
        //_effect.StartWave();
        //synchronization need
        _effect.SetEndEvent(EndEvent);
        _effect.StartEarthquakeEffect(60f);
	}

    void EndEvent() {
        EventManager.Manager.EndEvent(this.type);
    }

	public override void OnEnd()
	{
		quakeObject = null;
		crackObject = null;
		_effect.ReturnTiles();
		_effect.SetActive(false);
	}

	public override void OnDestroy()
	{

	}
}  // 지진 이벤트



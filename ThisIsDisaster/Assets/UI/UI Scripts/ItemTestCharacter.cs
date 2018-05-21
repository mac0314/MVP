﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTestCharacter : MonoBehaviour {
    static long[] _weapons = { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17};
    static long[] _head = { 10001, 10002, 10003, 10004};
    static long[] _clothes = { 20001, 20002, 20003, 20004, 20005, 20006, 20007, 20008};
    static long[] _backpack = { 30001, 30002, 30003};
    static long[] _bottle = { 30010, 30020};
    static long[] _tool_equip = { 31001, 31002, 31003, 31004,31005, 31006, 31007, 31008};
    static long[] _tool_use = { 33001, 33002};
    static long[] _etc = { 40001, 40002, 40003, 41001, 41002, 41003, 41004, 41005};
    static long[] _norm = { 50001, 50002, 50003, 50004, 51001, 51002, 51003, 51004, 51005, 51006 };

    public GameObject PlayerCharacter;

    public CharacterModel CharacterUnit;

    public GameObject UIController;

    public GameObject Canvas;

    public void Awake()
    {
    }

    public void Start()
    {
        if (PlayerCharacter == null) {
            PlayerCharacter = GameManager.CurrentGameManager.GetLocalPlayer().gameObject;
        }
        CharacterUnit = PlayerCharacter.GetComponent<CharacterModel>();
        CharacterUnit.initialState();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)){
            GetEqip();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            GetToolUse();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            GetEtc();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            GetNorm();
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            CharacterUnit.SubtractHealth(20);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            EatFood();
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            CharacterUnit.SubtractStamina(20);
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            EatWater();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {

        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {

        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            RemoveFirstItem();
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            OpenCanvas();
        }
    }

    void OpenCanvas()
    {
        Canvas.SetActive(true);
    }
    void RemoveAllEquipType()
    {
        CharacterUnit.RemoveEquipment("weapon");
        CharacterUnit.RemoveEquipment("head");
        CharacterUnit.RemoveEquipment("clothes");
        CharacterUnit.RemoveEquipment("backpack");
        CharacterUnit.RemoveEquipment("bottle");
        CharacterUnit.RemoveEquipment("flash");
    }



    void EatFood()
    {
        ItemModel food = ItemManager.Manager.MakeItem(70000);
        CharacterUnit.UseExpendables(food);
    }
  
    void EatWater()
    {
        ItemModel water = ItemManager.Manager.MakeItem(70001);
        CharacterUnit.UseExpendables(water);
    }
    void GetEqip()
    {
        GetHead();
        GetWeapon();
        GetBackpack();
        GetBottle();
        GetToolEquip();            
    }

    void GetHead()
    {
        foreach (long id in _head)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }

    void GetWeapon()
    {
        foreach (long id in _weapons)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetClothes()
    {
        foreach (long id in _clothes)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetBackpack()
    {
        foreach (long id in _backpack)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetBottle()
    {
        foreach (long id in _bottle)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetToolEquip()
    {
        foreach (long id in _tool_equip)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetToolUse()
    {
        foreach (long id in _tool_use)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetEtc()
    {
        foreach (long id in _etc)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }
    void GetNorm()
    {
        foreach (long id in _norm)
        {
            ItemManager.Manager.AddItem(CharacterUnit, id, 1);
        }
    }



    void RemoveFirstItem()
    {
        CharacterUnit.RemoveItemAtIndex(0);
    }



}

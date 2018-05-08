﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUIController : MonoBehaviour {


    //상태창 텍스트
    public Text[] StatusPannel;
    public Text Health;
    public Text Stamina;
    public Text Defense;
    public Text Damage;

    public StatusUIController()
    {
        StatusPannel = new Text[] { Health, Stamina, Defense, Damage };
    }

    //캐릭터 모델로부터 스텟을 불러와 상태창에 반영
    public void GetStatus(GameObject PlayerCharacter)
    {
        string _maxHealth = PlayerCharacter.GetComponent<CharacterModel>().maxHealth.ToString();
        string _health = PlayerCharacter.GetComponent<CharacterModel>().health.ToString();
        string _healthInUI = _health + " / " + _maxHealth;

        string _maxStamina = PlayerCharacter.GetComponent<CharacterModel>().maxStamina.ToString();
        string _stamina = PlayerCharacter.GetComponent<CharacterModel>().stamina.ToString();
        string _staminaInUI = _stamina + " / " + _maxStamina;


        Health.GetComponent<Text>().text = _healthInUI;
        Stamina.GetComponent<Text>().text = _staminaInUI;
        Damage.GetComponent<Text>().text =
                PlayerCharacter.GetComponent<CharacterModel>().damage.ToString();
        Defense.GetComponent<Text>().text =
            PlayerCharacter.GetComponent<CharacterModel>().defense.ToString();
    }    
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class CharacterModel : UnitModel {

    //가방(인벤토리) 사이즈. 
    //util 아이템에 따라 사이즈 증가 가능하게 구현할 예정
    public int defaultBagSize = 30;
    
    //캐릭터 기본 스텟
    public float defaultHealth = 100.0f;
    public float defaultStamina = 100.0f;
    public float defaultDefense = 10.0f;
    public float defaultDamage = 10.0f;

    //캐릭터 맥스 스텟. 
    //맥스스텟 = 기본 스텟 + 아이템으로 증가하는 스텟
    public float maxHealth = 0.0f;
    public float maxStamina = 0.0f;

    //캐릭터 현재 스텟
    public float health = 0.0f;
    public float stamina = 0.0f;
    public float defense = 0.0f;
    public float damage = 0.0f;
    
    //아이템으로 증가하는 스텟
    public float itemHealth = 0.0f;
    public float itemStamina = 0.0f;
    public float itemDefense = 0.0f;
    public float itemDamage =0.0f;

    //아이템 착용 슬롯
    public ItemModel headSlot = null;
    public ItemModel weaponSlot = null;
    public ItemModel utilSlot1 = null;
    public ItemModel utilSlot2 = null;
    public ItemModel utilSlot3 = null;

    //초기화 
    public virtual void initialState()
    {

        UpdateStat();

        health = maxHealth;
        stamina = maxStamina;
    }


    //장비 착용.
    public virtual void WearEquipment(ItemModel equipment)
    {
        ItemType equipType = equipment.metaInfo.itemType;

        if (equipType.Equals(ItemType.Weapon))
        {
            if(weaponSlot == null){
                weaponSlot = equipment;
                AddStats(weaponSlot);
            }
            else
            {
                Debug.Log("Weapon Slot is full");
            }
        }
        else if (equipType.Equals(ItemType.Head))
        {
            if (headSlot == null)
            {
                headSlot = equipment;
                AddStats(headSlot);
            }
            else
            {
                Debug.Log("Head Slot is full");
            }
        }
        else if (equipType.Equals(ItemType.Util))
        {
            if (utilSlot1 == null)
            {
                utilSlot1 = equipment;
                AddStats(utilSlot1);
            }
            else if (utilSlot2 == null)
            {
                utilSlot2 = equipment;
                AddStats(utilSlot2);
            }
            else if (utilSlot3 == null)
            {
                utilSlot3 = equipment;
                AddStats(utilSlot3);
            }
            else
            {
                UnityEngine.Debug.Log("All UtilSlot is full");
            }//유틸 슬롯 풀
        }
    }
    
    //장비 착용. 착용할 슬롯과 아이템 모델을 변수로 
    public virtual void WearSlot(ItemModel Slot, ItemModel equipment)
    {
        if(Slot == null)
        {
            Slot = equipment;
            AddStats(Slot);
        }
        else
        {
            UnityEngine.Debug.Log("Slot is already full");
        }
    }

    //장비 제거 
    public virtual void RemoveEquipment(ItemModel Slot)
    {
        if(Slot == null)
        {
            Debug.Log("Slot is alread empty");
            return ;
        }

        ItemType slotType = Slot.metaInfo.itemType;
        
        

        if (slotType.Equals(ItemType.Weapon))
        {
            weaponSlot = null;
            SubtractStats(Slot);
        }
        else if (slotType.Equals(ItemType.Head))
        {
            headSlot = null;
            SubtractStats(Slot);
        }
        else if (slotType.Equals(ItemType.Util))
        {
            //
        }

        UpdateStat();

        if (health > maxHealth)
            health = maxHealth;
        if (stamina > maxStamina)
            stamina = maxStamina;
    }

    //장비 착용시 스텟 업데이트
    public void AddStats(ItemModel equip)
    {
        itemHealth += equip.GetHealth();
        health += equip.GetHealth();
        itemStamina += equip.GetStamina();
        stamina += equip.GetStamina();
        itemDefense += equip.GetDefense();
        itemDamage += equip.GetDamage();

        UpdateStat();
    }

    //장비 제거시 스텟 업데이트
    private void SubtractStats(ItemModel equip)
    {
        itemHealth -= equip.GetHealth();
        itemStamina -= equip.GetStamina();
        itemDefense -= equip.GetDefense();
        itemDamage -= equip.GetDamage();

        UpdateStat();
    }
    
    //장비 착용시 Max Stat을 업데이트
    private void UpdateStat()
    {
        maxHealth = defaultHealth + itemHealth;
        maxStamina = defaultStamina + itemStamina;
        defense = defaultDefense + itemDefense;
        damage = defaultDamage + itemDamage;
    }

    //소모품 사용. HP회복, Stamina회복
    public void UseExpendables(ItemModel etc)
    {
        float etcHealth = etc.GetHealth();
        if(etcHealth != 0f)
        {
            PlusHealth(etcHealth);
        }


        float etcStamina = etc.GetStamina();
        if (etcStamina != 0f)
        {
            PlusStamina(etcStamina);
        }

        UpdateStat();
    }

    //HP 감소
    public void SubtractHealth(float weight)
    {
        health -= weight;

        if(health <= 0f)
        {
            health = 0f;
            Debug.Log("Player Died");
        }
    }

    //소모품 사용시 체력 회복. MaxHealth 이상은 회복되지 않음
    public void PlusHealth(float weight)
    {
        if(health < maxHealth)
        {
            health += weight;
            if(health >= maxHealth)
            {
                health = maxHealth;
            }
        }
        else
        {
            Debug.Log("HP is Full");
        }
    }

    //Stamina 감소
    public void SubtractStamina(float weight)
    {
        stamina -= weight;

        if (stamina <= 0f)
        {
            stamina = 0f;
            Debug.Log("Stamina is 0");
        }
    }

    //아이템 사용시 Stamina회복. MaxStamina 이상은 회복 안됨
    public void PlusStamina(float weight)
    {
        if (stamina < maxStamina)
        {
            stamina += weight;
            if (stamina >= maxStamina)
            {
                stamina = maxStamina;
            }
        }
        else
        {
            Debug.Log("Stamina is Full");
        }
    }


    //테스트용. 현재 스텟 출력
    public virtual void PrintStats()
    {
        UnityEngine.Debug.Log(MaxStatsToString());
        UnityEngine.Debug.Log(CurrentStatsToString());

    }

    //테스트용. 현재 맥스 스텟 출력
    public virtual string MaxStatsToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("Max Stats : ");
        builder.Append(maxHealth);
        builder.Append(" , ");
        builder.Append(maxStamina);
        builder.Append(" , ");
        builder.Append(defense);
        builder.Append(" , ");
        builder.Append(damage);
        builder.AppendLine();
        string output = builder.ToString();

        return output;

    }


    //테스트용. 현재 스텟 출력 
    public virtual string CurrentStatsToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("Current Stats : ");
        builder.Append(health);
        builder.Append(" , ");
        builder.Append(stamina);
        builder.Append(" , ");
        builder.Append(defense);
        builder.Append(" , ");
        builder.Append(damage);
        builder.AppendLine();
        string output = builder.ToString();

        return output;

    }
}

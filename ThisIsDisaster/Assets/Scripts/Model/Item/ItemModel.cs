﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ItemModel {
    //Who is Owner

    //field will be initialized
    //Think which will be needed

    public ItemTypeInfo metaInfo;
    public long instanceId;

    public CharacterModel Owner = null;

    public virtual void OnItemAqquired(CharacterModel target) {
        //do smth
        Owner = target;
    }

    public virtual void OnItemRemoved(CharacterModel prevOwner) {
        //do smth
        Owner = null;
    }

    public virtual float GetDefense()
    {
        return GetStat("defense");
    }

    public virtual float GetHealth()
    {
        return GetStat("health");
    }
    public virtual float GetStamina()
    {
        return GetStat("stamina");
    }

    public virtual float GetDamage() {
        return GetStat("damage");
    }

    public virtual int GetAttackAnimType() {
        return (int)GetStat("AttackAnim");
    }

    public virtual float GetAttacRangeX()
    {
        return GetStat("range_x");
    }

    public virtual float GetAttacRangeY()
    {
        return GetStat("range_y");
    }

    public virtual int GetSize()
    {
        return (int)GetStat("size");
    }

    public virtual int GetHealthRegen()
    {
        return (int)GetStat("health_regen");
    }

    public virtual int GetStaminaRegen()
    {
        return (int)GetStat("stamina_regen");
    }

    public virtual int GetVision()
    {
        return (int)GetStat("vision");
    }

    public float GetStat(string statName) {
        float output = 0f;
        metaInfo.stats.TryGetValue(statName, out output);
        return output;
    }

}

public class WeaponModel : ItemModel {

    public void OnStartAttack()
    {
        //smth todo
        float damage = GetDamage();
    }

    public void OnGiveDamage() {
        //smth todo
    }

}

public class HeadModel : ItemModel {
}

public class UtilModel : ItemModel {

}

public class EtcModel : ItemModel {

}
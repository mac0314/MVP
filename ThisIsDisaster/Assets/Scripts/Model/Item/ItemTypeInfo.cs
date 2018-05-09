﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 아이템의 종류 -> 장비 혹은 소모품
/// </summary>
public enum ItemType
{
    /// <summary>
    /// 무기
    /// </summary>
    Weapon = 0,

    /// <summary>
    /// 모자
    /// </summary>
    Head,

    /// <summary>
    /// 유틸리티
    /// </summary>
    Util,

    /// <summary>
    /// 기타 소모품
    /// </summary>
    Etc
}

public class ItemTypeInfo
{
    public long metaId = 0;

    /// <summary>
    /// 아이템 이름
    /// </summary>
    public string Name;

    /// <summary>
    /// 소지가능한 최대 개수 : 장비의 경우 1개, 소모품의 경우 _defaultMaxCount를 반환
    /// </summary>
    public int MaxCount
    {
        get { return itemType == ItemType.Etc ? _defaultMaxCount : 1; }
    }

    private int _defaultMaxCount = 0;

    /// <summary>
    /// 아이템의 타입 - 장비, 소모품
    /// </summary>
    public ItemType itemType = ItemType.Weapon;

    public string spriteSrc = "";
    public Dictionary<string, float> stats = new Dictionary<string, float>();


    public List<string> tags = new List<string>();

    public static ItemType ParseType(string typeText) {
        switch (typeText.ToLower()) {
            case "head": return ItemType.Head;
            case "weapon": return ItemType.Weapon;
            case "util": return ItemType.Util;
            case "etc":
            default: return ItemType.Etc;
        }
    }

    public ItemTypeInfo(long metaId, string name, int maxCount, ItemType type, string[] tags) {
        this.metaId = metaId;
        this.Name = name;
        this._defaultMaxCount = maxCount;
        this.itemType = type;
        if (tags.Length > 0)
            this.tags.AddRange(tags);
    }

    //smth else info


    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} : {1} : {2}", metaId, itemType, Name);
        foreach (var kv in stats)
        {
            builder.AppendLine();
            builder.AppendFormat("   {0}:{1}", kv.Key, kv.Value);
        }


        return builder.ToString();
    }
}

// BattleContext.cs

using System.Collections.Generic;
using UnityEngine;
public static class BattleContext
{

    public static TextAsset PendingInkJSON;
    public static int PendingEnemySlotCount;

    public static List<string> PendingEnemyTags = new List<string>();
}

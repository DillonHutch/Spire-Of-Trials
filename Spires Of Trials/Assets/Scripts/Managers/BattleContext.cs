// BattleContext.cs

using UnityEngine;
public static class BattleContext
{
    // Will be set by the Overworld and read in Battle
    public static string PendingEnemyTag;
    public static TextAsset PendingInkJSON;
    public static int PendingEnemySlotCount;
}

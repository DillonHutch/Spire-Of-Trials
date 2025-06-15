using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Runtime.InteropServices;

/// <summary>
/// manages all fmod events 
/// </summary>
public class FMODEvents : MonoBehaviour
{

    #region Singleton Instance

    /// <summary>
    /// Singleton instance of FMODEvents to ensure only one exists in the game.
    /// </summary>
    public static FMODEvents instance { get; private set; }

    #endregion

    #region Music & Ambience

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; set; } // Background music event

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; set; } // Ambiance/background noise event

    #endregion

    #region Player Attack Sounds

    [field: Header("Player Attack Sounds")]

    [field: Header("Melee Attack")]
    [field: SerializeField] public EventReference meleeAttack { get; set; } // Sound effect for melee attacks

    [field: Header("Range Attack")]
    [field: SerializeField] public EventReference rangeAttack { get; set; } // Sound effect for ranged attacks

    [field: Header("Magic Attack")]
    [field: SerializeField] public EventReference magicAttack { get; set; } // Sound effect for magic attacks

    [field: Header("Heavy Attack")]
    [field: SerializeField] public EventReference heavyAttack { get; set; } // Sound effect for heavy attacks

    #endregion

    #region Enemy Attack Sounds

    [field: Header("Enemy Attack Sounds")]

    [field: Header("Goblin Attack")]
    [field: SerializeField] public EventReference gobAtk { get; set; } // Goblin attack sound

    [field: Header("Goblin Wind-Up")]
    [field: SerializeField] public EventReference gobWU { get; set; } // Goblin wind-up sound

    [field: Header("Skeleton Attack")]
    [field: SerializeField] public EventReference skeAtk { get; set; } // Skeleton attack sound

    [field: Header("Skeleton Wind-Up")]
    [field: SerializeField] public EventReference skeWU { get; set; } // Skeleton wind-up sound

    [field: Header("Slime Attack")]
    [field: SerializeField] public EventReference slimeAtk { get; set; } // Slime attack sound

    [field: Header("Slime Wind-Up")]
    [field: SerializeField] public EventReference slimeWU { get; set; } // Slime wind-up sound


    [field: Header("Wendingo Attack")]
    [field: SerializeField] public EventReference wenAtk { get; set; } // Goblin attack sound

    [field: Header("Wendingo Wind-Up")]
    [field: SerializeField] public EventReference wenWU { get; set; } // Goblin wind-up sound

    [field: Header("Thornbrute Attack")]
    [field: SerializeField] public EventReference thornAtk { get; set; } // Skeleton attack sound

    [field: Header("Thornbrute Wind-Up")]
    [field: SerializeField] public EventReference thornWU { get; set; } // Skeleton wind-up sound

    [field: Header("Serpant Attack")]
    [field: SerializeField] public EventReference serAtk { get; set; } // Slime attack sound

    [field: Header("Serpant Wind-Up")]
    [field: SerializeField] public EventReference serWU { get; set; } // Slime wind-up sound

    [field: Header("Cultist Attack")]
    [field: SerializeField] public EventReference cultistAtk { get; set; } // Slime attack sound

    [field: Header("Cultist Wind-Up")]
    [field: SerializeField] public EventReference CultistWU { get; set; } // Slime wind-up sound

    [field: Header("Demon Attack")]
    [field: SerializeField] public EventReference demonAtk { get; set; } // Slime attack sound

    [field: Header("Demon Wind-Up")]
    [field: SerializeField] public EventReference demonWU { get; set; } // Slime wind-up sound

    [field: Header("Vampire Attack")]
    [field: SerializeField] public EventReference vampireAtk { get; set; } // Slime attack sound

    [field: Header("Vampire Wind-Up")]
    [field: SerializeField] public EventReference vampireWU { get; set; } // Slime wind-up sound

    #endregion

    #region Knight Sounds

    [field: Header("Knight Sounds")]

    [field: Header("Knight Attack")]
    [field: SerializeField] public EventReference knightAttack { get; set; } // Knight attack sound

    [field: Header("Knight Wind-Up")]
    [field: SerializeField] public EventReference knightWU { get; set; } // Knight wind-up sound

    [field: Header("Knight Damage")]
    [field: SerializeField] public EventReference knightDamage { get; set; } // Knight damage sound

    [field: Header("Knight Death")]
    [field: SerializeField] public EventReference knightDeath { get; set; } // Knight damage sound

    #endregion

    #region Menu sounds

    [field: Header("Menu Sounds")]

    [field: Header("MenuClick")]
    [field: SerializeField] public EventReference menuClick { get; set; }

    [field: Header("MenuHover")]
    [field: SerializeField] public EventReference menuHover { get; set; }

    [field: Header("MenuStart")]
    [field: SerializeField] public EventReference menuStart { get; set; }


    #endregion

    #region Frog Sounds

    [field: Header("Frog Sounds")]

    [field: Header("Frog Attack")]
    [field: SerializeField] public EventReference frogAttack { get; set; } // Knight attack sound

    [field: Header("Frog Wind-Up")]
    [field: SerializeField] public EventReference frogWU { get; set; } // Knight wind-up sound

    [field: Header("Frog Damage")]
    [field: SerializeField] public EventReference frogDamage { get; set; } // Knight damage sound

    [field: Header("Frog Death")]
    [field: SerializeField] public EventReference frogDeath { get; set; } // Knight damage sound

    #endregion

    #region Final Boss Sounds

    [field: Header("Frog Sounds")]

    [field: Header("CollectorAttack")]
    [field: SerializeField] public EventReference collectorAttack { get; set; } // Knight attack sound

    [field: Header("CollectorWindup")]
    [field: SerializeField] public EventReference collectorWU { get; set; } // Knight wind-up sound

    [field: Header("CollectorDamage")]
    [field: SerializeField] public EventReference collectorDamage { get; set; } // Knight damage sound

    [field: Header("CollectorDeath")]
    [field: SerializeField] public EventReference collectorDeath { get; set; } // Knight damage sound

    [field: Header("CollectorSummon")]
    [field: SerializeField] public EventReference collectorSummon { get; set; } // Knight damage sound

    #endregion

    #region Player Damage & Defense Sounds

    [field: Header("Player Damage & Defense")]

    [field: Header("Player Hit")]
    [field: SerializeField] public EventReference playerHit { get; set; } // Sound effect when the player gets hit

    [field: Header("Player Metal Hit")]
    [field: SerializeField] public EventReference playerMetal { get; set; } // Sound effect when player is hit with a metallic attack

    [field: Header("Shield Block (Wood)")]
    [field: SerializeField] public EventReference shieldWood { get; set; } // Shield block sound (wooden shield)

    #endregion

    #region Combo Milestone Sounds

    [field: Header("Combo Milestones")]

    [field: Header("Combo Achieved")]
    [field: SerializeField] public EventReference combo { get; set; } // Sound effect for reaching a combo milestone

    [field: Header("Unstoppable Combo")]
    [field: SerializeField] public EventReference unstopable { get; set; } // Sound effect for high combo streak

    [field: Header("Legendary Combo")]
    [field: SerializeField] public EventReference legendary { get; set; } // Sound effect for max combo streak

    #endregion

    [field: Header("beep")]
    [field: SerializeField] public EventReference[] beep { get; set; } // Sound effect for reaching a combo milestone


    #region UnityMethods

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Implements the Singleton pattern to ensure only one FMODEvents instance exists.
    /// </summary>
    private void Awake()
    {
        if (instance != null)
        {
            // If an additional instance is found, you may want to log an error or handle it accordingly.
            // Debug.LogError("Found more than one FMODEvents instance. Ensure there is only one in the scene.");
        }

        // Assign this instance as the singleton instance
        instance = this;
    }

    #endregion

}

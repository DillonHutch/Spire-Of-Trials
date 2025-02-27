using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Runtime.InteropServices;

public class FMODEvents : MonoBehaviour
{

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; set; }


    [field: Header("MeleeAttack")]
    [field: SerializeField] public EventReference meleeAttack { get; set; }

    [field: Header("RangeAttack")]
    [field: SerializeField] public EventReference rangeAttack { get; set; }

    [field: Header("MagicAttack")]
    [field: SerializeField] public EventReference magicAttack { get; set; }

    [field: Header("HeavyAttck")]
    [field: SerializeField] public EventReference heavyAttack { get; set; }

    [field: Header("Shield")]
    [field: SerializeField] public EventReference shieldBlock { get; set; }

    [field: Header("GobAtk")]
    [field: SerializeField] public EventReference gobAtk { get; set; }

    [field: Header("GobWU")]
    [field: SerializeField] public EventReference gobWU { get; set; }

    [field: Header("SkeAtk")]
    [field: SerializeField] public EventReference skeAtk { get; set; }

    [field: Header("SkeWU")]
    [field: SerializeField] public EventReference skeWU { get; set; }

    [field: Header("SlimeAtk")]
    [field: SerializeField] public EventReference slimeAtk { get; set; }

    [field: Header("SlimeWU")]
    [field: SerializeField] public EventReference slimeWU { get; set; }

    [field: Header("PlayerHit")]
    [field: SerializeField] public EventReference playerHit { get; set; }



    public static FMODEvents instance { get; private set; }


    private void Awake()
    {
        if (instance != null)
        {

            //Debug.LogError("Found more than one fmod events");

        }

        instance = this;
    }
}

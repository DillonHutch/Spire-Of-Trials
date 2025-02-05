using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; set; }


    [field: Header("MeleeAttack")]
    [field: SerializeField] public EventReference meleeAttack { get; set; }

    [field: Header("RangeAttack")]
    [field: SerializeField] public EventReference rangeAttack { get; set; }

    [field: Header("MagicAttack")]
    [field: SerializeField] public EventReference magicAttack { get; set; }





    public static FMODEvents instance { get; private set; }


    private void Awake()
    {
        if (instance != null)
        {

            Debug.LogError("Found more than one fmod events");

        }

        instance = this;
    }
}

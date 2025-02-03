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

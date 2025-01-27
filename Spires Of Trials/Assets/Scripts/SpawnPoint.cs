using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    public enum SpawnPointNum
    {
        LEFT = 0,
        MIDDLE = 1,
        RIGHT = 3
    }


    [SerializeField] SpawnPointNum spawnPointNum;


    public int SpawnPointNumber { get { return (int)spawnPointNum; } }    




}

// CameraManager.cs
using UnityEngine;
using Cinemachine;
using System;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera npcCam;
    public CinemachineVirtualCamera thirdCam;

    [Tooltip("How much above the highest other priority")]
    public int priorityBoost = 10;

    private int _playerDefault;
    private int _npcDefault;
    private int _thirdDefault;
    private Transform _thirdOriginalFollow;


    private void OnEnable()
    {
        EventManager.Instance.StartListening<string>("focusCamera", OnFocusCamera);
        EventManager.Instance.StartListening("resetCamera", ResetToPlayerCam);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening<string>("focusCamera", OnFocusCamera);
        EventManager.Instance.StopListening("resetCamera", ResetToPlayerCam);
    }

    void Awake()
    {
        if (Instance != null) throw new Exception("Only one CameraManager allowed");
        Instance = this;

        _playerDefault = playerCam.Priority;
        _npcDefault = npcCam.Priority;
        _thirdDefault = thirdCam.Priority;
        _thirdOriginalFollow = thirdCam.Follow;

       
    }


    private void OnFocusCamera(string targetName)
    {
        var go = GameObject.Find(targetName);
        if (go == null)
        {
            Debug.LogWarning($"FocusCam: no GameObject named \"{targetName}\"");
            return;
        }

        // point thirdCam at it
        thirdCam.Follow = go.transform;


        // bump its priority above the others
        int highest = Mathf.Max(playerCam.Priority, npcCam.Priority);
        thirdCam.Priority = highest + priorityBoost;
    }

    /// <summary>
    /// Call this when you want to restore the original cameras
    /// (for example, after your dialogue knot ends).
    /// </summary>
    public void ResetToPlayerCam()
    {
        playerCam.Priority = 10;
        npcCam.Priority = 1;
        thirdCam.Priority = 1;
        thirdCam.Follow = _thirdOriginalFollow;
    }
}

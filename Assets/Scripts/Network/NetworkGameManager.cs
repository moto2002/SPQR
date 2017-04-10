﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkGameManager : Photon.PunBehaviour {

    public GameObject PlayerPrefab;
    protected GameObject PlayerAvatar;
    protected PhotonView PhotonView;
    public Dictionary<string, int> PlayerTeams;
    public int Team;
    public PlayerColors Color;

    void Start() {
        if (!PhotonNetwork.connected) return;
        object teams;
        PhotonNetwork.room.CustomProperties.TryGetValue("Teams", out teams);
        PlayerTeams = (Dictionary<string, int>) teams;
        Team = PlayerTeams[PhotonNetwork.playerName];
        Color = (PlayerColors)Team;
        string robotPrefabName = Color.ToString() + "Robot";
        Debug.Log(robotPrefabName);
        GameObject localPlayer = PhotonNetwork.Instantiate(
            robotPrefabName, 
            Vector3.left * (PhotonNetwork.room.PlayerCount * 2), 
            Quaternion.identity, 0
        );

        GameManager.Instance.LocalPlayer 
            = localPlayer.GetComponent<PlayerController>();
    }

    public override void OnLeftRoom() {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }

    void LoadArena() {
        if (!PhotonNetwork.isMasterClient) return;

        PhotonNetwork.LoadLevel("Sandbox");
    }
}

public enum PlayerColors
{
    White = 1, Black, Blue, Red, Green, Orange, Violet, Cyan
}

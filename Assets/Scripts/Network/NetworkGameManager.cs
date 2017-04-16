﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkGameManager : Photon.PunBehaviour {
	public static int nbPlayersForThisGame;
	public static bool instantiateAI = false;
	public GameObject AIPrefab;
    public GameObject PlayerPrefab;
    public GameObject[] MapPrefabs;
    protected GameObject PlayerAvatar;
    protected PhotonView PhotonView;
    public Dictionary<string, int> PlayerTeams;
    public int Team;
    public PlayerColors Color;
	public static NetworkGameManager Instance = null;


	void Awake() {
		if (NetworkGameManager.Instance == null) {
			NetworkGameManager.Instance = this;
		} else if (NetworkGameManager.Instance != this) {
			Destroy(gameObject);
		}
	}


    void Start() {
		
        if (!PhotonNetwork.connected) return;
        // Mode init

        // Map init
        object map;
        Vector3 position = new Vector3(0, -0.6f, 0);
        int randIndex = Random.Range(0, MapPrefabs.Length);
        GameObject newMap;
        if (!PhotonNetwork.offlineMode)
        {
            PhotonNetwork.room.CustomProperties.TryGetValue("Map", out map);
            int mapIndex = (int)map;
            switch (mapIndex)
            {
                case 0:
                    newMap = Instantiate(MapPrefabs[randIndex], position, Quaternion.identity);
                    break;
                case 1:
                    newMap = Instantiate(MapPrefabs[0], position, Quaternion.identity);
                    break;
                case 2:
                    newMap = Instantiate(MapPrefabs[1], position, Quaternion.identity);
                    break;
                case 3:
                    newMap = Instantiate(MapPrefabs[2], position, Quaternion.identity);
                    break;
                default:
                    newMap = Instantiate(MapPrefabs[2], position, Quaternion.identity);
                    break;
            }
        }
        else
        {
            newMap = Instantiate(MapPrefabs[randIndex], position, Quaternion.identity);
        }
        newMap.transform.localScale = new Vector3(100, 100, 100);
        // Teams init
        object teams;
		if (!PhotonNetwork.offlineMode) {
			PhotonNetwork.room.CustomProperties.TryGetValue ("Teams", out teams);
			PlayerTeams = (Dictionary<string, int>)teams;
			nbPlayersForThisGame = PlayerTeams.Count;
			Team = PlayerTeams [PhotonNetwork.playerName];
		} else {
			PlayerTeams =  new Dictionary<string, int>();
			nbPlayersForThisGame = 2;
            PlayerTeams.Add(PhotonNetwork.playerName, 1);
            PlayerTeams.Add("Bot1", 2);
		}

		//INSTANTIATE Players & AIs
	    if (PlayerTeams != null) {
            DistributePlayers();
	    }
    }

    private void DistributePlayers()
    {
        string team;
        string robotPrefabName;
        float radius = 6.5f;
        float angle = 0;
        float step = (2*Mathf.PI)/PlayerTeams.Count;
        float x, z;

        Vector3 spawnPos;
        foreach (string key in PlayerTeams.Keys)
        {
            x = radius * Mathf.Cos(angle);
            z = radius * Mathf.Sin(angle);
            spawnPos = new Vector3(x, 0, z);
            if (key.Contains("Bot") && (PhotonNetwork.isMasterClient || PhotonNetwork.offlineMode))
            {
                instantiateAI = true;
                team = ((PlayerColors)PlayerTeams[key]).ToString();
                robotPrefabName = team + "Robot";

                GameObject temp = PhotonNetwork.Instantiate(
                                     robotPrefabName,
                                     spawnPos,
                                     Quaternion.LookRotation(Vector3.zero - spawnPos), 0
                                 );
                temp.AddComponent<AI>();
                temp.AddComponent<AIFocus>();
                temp.transform.name = key + " " + robotPrefabName;
                temp.GetComponent<PlayerController>().Team = team;
                instantiateAI = false;
            }
            else if(key.Equals(PhotonNetwork.playerName))
            {
                Color = (PlayerColors)PlayerTeams[key];
                team = Color.ToString();
                robotPrefabName = team + "Robot";
                GameObject localPlayer = PhotonNetwork.Instantiate(
                    robotPrefabName,
                    spawnPos,
                    Quaternion.LookRotation(Vector3.zero - spawnPos), 0
                );
                localPlayer.GetComponent<PlayerController>().Team = Color.ToString();

                GameManager.Instance.LocalPlayer
                = localPlayer.GetComponent<PlayerController>();
            }
            angle += step;
        }
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
    Gray, White, Black, Blue, Red, Green, Orange, Violet, Cyan
}

using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    // public CinemachineVirtualCamera virtualCamera;

    [NonSerialized] public GameObject player;

    private static GameManager _instance;

    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Game Manager is null :(");
            }

            return _instance;
        }
    }

    public AudioClip deadSoudClip;

    public int collectedCoins = 0;

    public Image fader;

    private bool fading = false;
    private float fadingValue = 0f;

    private bool playerDeleted = false;
    private bool playerRecreated = false;

    public bool gameOver
    {
        set
        {
            _gameOver = value;
            if (_gameOver)
            {
                //! DO NOT PLAY SOUNDS WHEN TIMESCALE = 0
                // AudioManager.instance.PlaySfx(deadSoudClip);
                Time.timeScale = 0;
                if (!fading)
                {
                    fadingValue = 0f;
                }
                fading = true;
                fader.enabled = true;
                playerDeleted = false;
                playerRecreated = false;
                // virtualCamera.Follow = player.transform;
            }
        }
        get { return _gameOver; }
    }

    public int currentCheckpoint = 0;

    private bool _gameOver = false;

    bool tpToCheckpoint(int number)
    {
        bool found = false;
        var scene = SceneManager.GetActiveScene();
        GameObject[] rootGameObjects = scene.GetRootGameObjects();
        foreach (GameObject rootGameObject in rootGameObjects)
        {
            Checkpoint checkpoint;
            for (int i = 0; i < rootGameObject.transform.childCount; i++)
            {
                GameObject gameObject = rootGameObject.transform.GetChild(i).gameObject;
                if (gameObject.TryGetComponent<Checkpoint>(out checkpoint))
                {
                    if (checkpoint.checkpoint == number)
                    {
                        _gameOver = false;
                        player.transform.parent = rootGameObject.transform.parent;
                        player.transform.position = checkpoint.transform.position;
                        found = true;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (found)
            {
                break;
            }

            if (rootGameObject.TryGetComponent<Checkpoint>(out checkpoint))
            {
                if (checkpoint.checkpoint == number)
                {
                    _gameOver = false;
                    player.transform.parent = rootGameObject.transform.parent;
                    player.transform.position = checkpoint.transform.position;
                    break;
                }
            }
        }

        // if (found == false)
        // {
        //     GameObject newPlayer = Instantiate(playerPrefab);
        //     newPlayer.transform.parent = scene.GetRootGameObjects()[0].transform;
        // }
        return found;
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        fader.enabled = false;
        player = Instantiate(playerPrefab);
        tpToCheckpoint(0);
        // virtualCamera.FollowTargetAsVcam.Follow = player.transform;
    }


    private void Update()
    {
        if (fading)
        {
            if (fadingValue < 0.5)
            {
                fader.color = new(0, 0, 0, fadingValue * 2);
            }
            else if (fadingValue < 1)
            {
                if (playerRecreated == false)
                {
                    Destroy(player);
                    playerDeleted = true;
                    player = Instantiate(playerPrefab);
                    tpToCheckpoint(currentCheckpoint);
                    playerRecreated = true;
                }

                fader.color = new(0, 0, 0, 1 - (fadingValue - 0.5f) * 2);
            }
            else
            {
                fading = false;
                Time.timeScale = 1;
                fader.enabled = false;
            }

            fadingValue += Time.unscaledDeltaTime;
        }
    }
}
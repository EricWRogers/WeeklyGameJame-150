﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[Serializable]
public class GameSettings
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private const string gameSettingsFileName = "gameSettings.dat";

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string mainLevelScene = "Main";
    
    public InputDevice lastDetectedDevice = null;

    private Vector2 lastScreenSize;

    public event Action onScreenSizeChanged; 

    new void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        InputSystem.onEvent += (ptr, device) => { lastDetectedDevice = device; };
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (lastScreenSize != screenSize)
        {
            lastScreenSize = screenSize;
            onScreenSizeChanged?.Invoke();
        }
    }

    public void QuitGame()
    {
        ScreenFader.Instance.FadeOut(-1, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        });
    }

    public void GoToMainMenu()
    {
        GoToScene(mainMenuScene);
    }

    public void GoToMainLevel()
    {
        GoToScene(mainLevelScene);
    }

    public void RestartCurrentScene()
    {
        GoToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToScene(string sceneName)
    {
        ScreenFader.Instance.FadeOut(-1, () =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    public void DeleteAllSaveData()
    {
        string dir = Application.persistentDataPath;
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }

    public GameSettings GetGameSettings()
    {
        return SaveSystem.LoadData<GameSettings>(gameSettingsFileName) ?? new GameSettings()
        {
            masterVolume = 1f,
            musicVolume = 1f,
            sfxVolume = 1f,
        };
    }

    public void SaveGameSettings(GameSettings gameSettings)
    {
        SaveSystem.SaveData(gameSettings, gameSettingsFileName);
    }
}
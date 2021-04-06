﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void LoadScene(Define.Scene type)
    {
        Manager.Clear();

        string sceneName = GetSceneName(type);
        SceneManager.LoadScene(sceneName);
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }

    string GetSceneName(Define.Scene type)
    {
        // Reflaction
        return System.Enum.GetName(typeof(Define.Scene), type);
    }
}
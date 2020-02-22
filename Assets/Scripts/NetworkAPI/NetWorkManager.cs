﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetworkAPI
{
    public class NetWorkManager : MonoBehaviour
    {
        [SerializeField] private int _dataCollectionFreq = 8; // See neulog api documentation
        [SerializeField] private int _numberOfSamples = 10; // See neulog api documentation
        [SerializeField] private String _experimentName;

        private String _START_EXP_URL = "http://localhost:22002/NeuLogAPI?StartExperiment:[GSR],[1],[Pulse],[1]";
        private String _STOP_EXP_URL = "http://localhost:22002/NeuLogAPI?StopExperiment";
        private bool hasExperimentStarted;
        public float endTimer = 300;
        private Scene currentScene;
        public static float timeToStop;
        private float timeForNextExperiment;
        private PythonServerAPI _pythonServerApi;
        private NeuLogAPI _neuLogApi;

        private String RESTING = "REST";
        private String SIMULATION = "SIM";

        void Start()
        {
            _pythonServerApi = new PythonServerAPI();
            _neuLogApi = new NeuLogAPI();
            _pythonServerApi.Start();
            
            hasExperimentStarted = false;
            if (!hasExperimentStarted)
            {
                StartExperiment(_experimentName);
            }
        }

        private void FixedUpdate()
        {
            timeToStop = timeToStop + Time.deltaTime;
            timeForNextExperiment = timeForNextExperiment + Time.deltaTime;
            bool loadNextScene = Input.GetKeyDown(KeyCode.N);

            if (timeToStop >= endTimer && _pythonServerApi.hasDataProcessed)
            {
                LoadNextSceneAndStopExp(loadNextScene);
            }
        }

        private void LoadNextSceneAndStopExp(bool loadNextScene)
        {
            if (loadNextScene)
            {
                Logger.Log(LogLevel.INFO,
                    "Next key pressed. Terminating initial static scene.");
            }
            else
            {
                Logger.Log(LogLevel.INFO, "Initial static Scene finished. Total runtime: " + endTimer + "s");
            }

            StopExperiment();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        private void StartExperiment(String expName)
        {
            StartCoroutine(_neuLogApi.ClosePreviousExperiment(_STOP_EXP_URL)); // Close all previous started experiment
            StartCoroutine(_neuLogApi.StartExperiment(_START_EXP_URL, _dataCollectionFreq.ToString(),
                _numberOfSamples.ToString()));
            
            _pythonServerApi.SetMessageAndSend(expName);
            hasExperimentStarted = true;
        }

        private void StopExperiment()
        {
            StartCoroutine(_neuLogApi.StopExperiment(_STOP_EXP_URL));
            _pythonServerApi.hasDataProcessed = false;
            _pythonServerApi?.Stop();
        }
    }
}
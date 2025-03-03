﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CamperManager : SingletonMonoBehaviour<CamperManager>
{
    public GameObject camperPrefab;
    public Transform hidingSpotsContainer;

    public List<GameObject> camperModelPrefabs;

    public int campersCount = 6;

    public RangeFloat noiseHintIntervalRange;
    public AudioClip noiseClip;

    public AudioClip runningClip;

    [Header("Run to TargetBase Stats")] public RangeFloat initialRunToTargetBaseDelayRange;
    public RangeFloat runToTargetBaseIntervalRange;
    public RangeInt campersPerTargetRunRange;
    public float minTargetBaseToPlayerAngleToRun = 30f;

    [Header("Debug")] [SerializeField] [ReadOnly]
    private int _campersSafe = 0;

    public List<Camper> campers { get; } = new List<Camper>();
    public Randomizer<HidingSpot> hidingSpotsRandomizer { get; private set; }
    public Randomizer<GameObject> camperModelPrefabsRandomizer { get; private set; }

    private float timeSinceLastRunToTargetBase = 0;
    private float timeSinceLastNoiseHint = 0;

    public int campersSafe => _campersSafe;
    public event Action<Camper> onCamperSafe;

    new void Awake()
    {
        base.Awake();

        hidingSpotsRandomizer = new Randomizer<HidingSpot>(hidingSpotsContainer.GetComponentsInChildren<HidingSpot>());
        if (hidingSpotsRandomizer.count <= 0)
        {
            Debug.LogWarning("No Hiding Spots Found");
        }

        camperModelPrefabsRandomizer = new Randomizer<GameObject>(camperModelPrefabs);

        SpawnCampers();
    }

    // Start is called before the first frame update
    void Start()
    {
        initialRunToTargetBaseDelayRange.SelectRandom();
        noiseHintIntervalRange.SelectRandom();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastRunToTargetBase += Time.deltaTime;
        timeSinceLastNoiseHint += Time.deltaTime;

        var curRunToTargetBaseDelayRange = runToTargetBaseIntervalRange.selected.HasValue
            ? runToTargetBaseIntervalRange
            : initialRunToTargetBaseDelayRange;

        if (timeSinceLastRunToTargetBase >= curRunToTargetBaseDelayRange.selected)
        {
            timeSinceLastRunToTargetBase = 0;
            runToTargetBaseIntervalRange.SelectRandom();

            if (!TargetBase.Instance.isPlayerGuarding)
            {
                var targetToPlayer = PlayerModel.Instance.transform.position - TargetBase.Instance.transform.position;

                var runnableCampers = campers.Where(camper =>
                {
                    if (camper.curState != CamperState.Hiding)
                    {
                        return false;
                    }

                    var targetToCamper = camper.transform.position - TargetBase.Instance.transform.position;
                    if (targetToPlayer.magnitude > targetToCamper.magnitude)
                    {
                        return true;
                    }

                    if (Vector3.Angle(targetToPlayer, targetToCamper) < minTargetBaseToPlayerAngleToRun)
                    {
                        return false;
                    }

                    return true;
                }).ToList();

                var selectedCampersToRun = new HashSet<Camper>();
                var runnableCampersRandomizer = new Randomizer<Camper>(runnableCampers);

                campersPerTargetRunRange.SelectRandom();
                for (int i = 0; i < campersPerTargetRunRange.selected; i++)
                {
                    selectedCampersToRun.Add(runnableCampersRandomizer.GetRandomItem());
                }

                foreach (var camper in selectedCampersToRun)
                {
                    camper.RunToTargetBase();
                }
            }
        }

        if (LevelManager.Instance.campersRemaining > 0)
        {
            if (timeSinceLastNoiseHint > noiseHintIntervalRange.selected)
            {
                timeSinceLastNoiseHint = 0;
                noiseHintIntervalRange.SelectRandom();

                var availableCampers = CamperManager.Instance.campers.Where(camper =>
                    camper.curState == CamperState.Hiding || camper.curState == CamperState.Moving).ToList();

                Camper closestCamper = null;
                var closestDist = Mathf.Infinity;
                foreach (var availableCamper in availableCampers)
                {
                    var dist = Vector3.Distance(PlayerModel.Instance.transform.position,
                        availableCamper.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestCamper = availableCamper;
                    }
                }

                if (closestCamper)
                {
                    var playerToClosestCamper =
                        (closestCamper.transform.position - PlayerModel.Instance.transform.position).GetYLess();
                    
                    SoundEffectsManager.Instance.PlayAt(noiseClip, PlayerModel.Instance.mainCamera.transform.position + playerToClosestCamper.normalized);
                    NoiseDirectionIndicatorManager.Instance.IndicateNoiseFrom(closestCamper.transform.position);
                }
            }
        }
    }

    void SpawnCampers()
    {
        for (int i = 0; i < campersCount; i++)
        {
            var hidingSpot = hidingSpotsRandomizer.GetRandomItem();
            var camper = Instantiate(camperPrefab, hidingSpot.transform.position, hidingSpot.transform.rotation)
                .GetComponent<Camper>();

            campers.Add(camper);
        }
    }

    public void OnCamperSafe(Camper camper)
    {
        _campersSafe++;
        onCamperSafe?.Invoke(camper);
    }
}
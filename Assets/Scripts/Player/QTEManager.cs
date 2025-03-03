﻿using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class QTEManager : MonoBehaviour
{
    public enum QTEOptions
    {
        North,
        South,
        West,
        East,
        QTEOptionsSize
    }

    public QTEOptions currentKey;
    public float initialQTEBuffer = 1f;
    public RangeFloat maxQTEBuffer;
    public float QTEBuffer = 0;
    public float QTETimer;
    public RangeInt maxQuickTimeEvents;

    public GameObject QTETimerRoot;
    public UnityEngine.UI.Image QTETimerImage;
    public InputKeyUI northKey;
    public InputKeyUI southKey;
    public InputKeyUI westKey;
    public InputKeyUI eastKey;

    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip eatClip;

    public List<CinemachineVirtualCameraBase> qteVirtualCameras;

    [HideInInspector] public Camper camperInPossession = null;

    private float maxQTETimer = 0;
    private int currentPasses = 0;

    private bool canGetNextKey = true;
    private bool checkQTETimer = false;
    private bool isEating = false;

    private Randomizer<CinemachineVirtualCameraBase> qteVirtualCameraRandomizer;
    private CinemachineVirtualCameraBase curVirtualCamera = null;

    void Awake()
    {
        QTEBuffer = maxQTEBuffer.GetRandom();
        maxQTETimer = QTETimer;
    }

    void OnEnable()
    {
        // PlayerModel.Instance.thirdPersonVirtualCamera.enabled = false;

        qteVirtualCameraRandomizer = new Randomizer<CinemachineVirtualCameraBase>(qteVirtualCameras);
        SwitchToRandomQTEVirtualCamera();

        PlayerModel.Instance.animator.SetBool("captured", true);

        maxQuickTimeEvents.SelectRandom();

        canGetNextKey = true;
        checkQTETimer = false;

        QTEBuffer = initialQTEBuffer;
        QTETimer = maxQTETimer;

        currentPasses = 0;

        QTETimerRoot.SetActive(true);
        QTETimerImage.fillAmount = 0;

        isEating = false;
    }

    void OnDisable()
    {
        if (curVirtualCamera)
        {
            curVirtualCamera.enabled = false;
        }

        // PlayerModel.Instance.thirdPersonVirtualCamera.enabled = true;

        PlayerModel.Instance.animator.SetBool("captured", false);

        canGetNextKey = true;

        northKey.gameObject.SetActive(false);
        southKey.gameObject.SetActive(false);
        westKey.gameObject.SetActive(false);
        eastKey.gameObject.SetActive(false);
        QTETimerRoot.SetActive(false);
    }

    void Update()
    {
        QTETimerRoot.SetActive(QTETimerImage.fillAmount > 0);

        if (isEating)
        {
            return;
        }

        if (canGetNextKey)
        {
            GetNextKey();
        }

        if (checkQTETimer)
        {
            QTETimer -= Time.deltaTime;

            QTETimerImage.fillAmount = (((QTETimer - 0) * (1 - 0)) / (maxQTETimer - 0)) + 0;
            
            if (QTETimer <= 0)
            {
                Fail();
            }
        }
    }

    void SwitchToRandomQTEVirtualCamera()
    {
        if (curVirtualCamera)
        {
            curVirtualCamera.enabled = false;
        }

        curVirtualCamera = qteVirtualCameraRandomizer.GetRandomItem();
        curVirtualCamera.enabled = true;
    }

    void GetNextKey()
    {
        CheckPass();
        
        QTEBuffer -= Time.deltaTime;
        if (QTEBuffer <= 0)
        {
            currentKey = (QTEOptions)Random.Range(0, (int)QTEOptions.QTEOptionsSize);
            Debug.Log(currentKey);

            switch (currentKey)
            {
                case QTEOptions.North:
                    northKey.gameObject.SetActive(true);
                    break;
                case QTEOptions.South:
                    southKey.gameObject.SetActive(true);
                    break;
                case QTEOptions.West:
                    westKey.gameObject.SetActive(true);
                    break;
                case QTEOptions.East:
                    eastKey.gameObject.SetActive(true);
                    break;
            }

            checkQTETimer = true;
            QTETimer = maxQTETimer;
            QTETimerImage.fillAmount = 1;

            canGetNextKey = false;
        }
    }

    void Fail()
    {
        Debug.Log("Fail");
        SoundEffectsManager.Instance.Play(loseClip);

        camperInPossession.OnReleased();

        PlayerModel.Instance.ChangeState(PlayerModel.PlayerState.Moving);
        this.enabled = false;
    }

    void CheckPass()
    {
        if (isEating)
        {
            return;
        }

        if (currentPasses == maxQuickTimeEvents.selected)
        {
            Debug.Log("Passed!");
            SoundEffectsManager.Instance.Play(eatClip);

            camperInPossession.OnBeingEaten();
            PlayerModel.Instance.animator.SetTrigger("eat");
            isEating = true;
        }

        QTETimerImage.fillAmount = 0;
    }

    public void OnEatingAnimationCompleted()
    {
        this.enabled = false;
        isEating = false;
        camperInPossession = null;
    }

    void OnQuickTimeNorth(InputValue inputValue)
    {
        if (!canGetNextKey)
        {
            if (currentKey == QTEOptions.North)
            {
                currentPasses++;

                checkQTETimer = false;
                canGetNextKey = true;

                QTEBuffer = maxQTEBuffer.GetRandom();

                northKey.gameObject.SetActive(false);
                Debug.Log("Good!");

                SwitchToRandomQTEVirtualCamera();
                SoundEffectsManager.Instance.Play(winClip);
            }
            else
            {
                Fail();
            }
        }
    }

    void OnQuickTimeSouth(InputValue inputValue)
    {
        if (!canGetNextKey)
        {
            if (currentKey == QTEOptions.South)
            {
                currentPasses++;

                checkQTETimer = false;
                canGetNextKey = true;

                QTEBuffer = maxQTEBuffer.GetRandom();

                southKey.gameObject.SetActive(false);
                Debug.Log("Good!");

                SwitchToRandomQTEVirtualCamera();
                SoundEffectsManager.Instance.Play(winClip);
            }
            else
            {
                Fail();
            }
        }
    }

    void OnQuickTimeWest(InputValue inputValue)
    {
        if (!canGetNextKey)
        {
            if (currentKey == QTEOptions.West)
            {
                currentPasses++;

                checkQTETimer = false;
                canGetNextKey = true;

                QTEBuffer = maxQTEBuffer.GetRandom();

                westKey.gameObject.SetActive(false);
                Debug.Log("Good!");

                SwitchToRandomQTEVirtualCamera();
                SoundEffectsManager.Instance.Play(winClip);
            }
            else
            {
                Fail();
            }
        }
    }

    void OnQuickTimeEast(InputValue inputValue)
    {
        if (!canGetNextKey)
        {
            if (currentKey == QTEOptions.East)
            {
                currentPasses++;

                checkQTETimer = false;
                canGetNextKey = true;

                QTEBuffer = maxQTEBuffer.GetRandom();

                eastKey.gameObject.SetActive(false);
                Debug.Log("Good!");

                SwitchToRandomQTEVirtualCamera();
                SoundEffectsManager.Instance.Play(winClip);
            }
            else
            {
                Fail();
            }
        }
    }
}

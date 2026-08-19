using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.Enums;

public class TriggerBehavior : MonoBehaviour
{
    [SerializeField]
    GameObject uiTextElement;

    [SerializeField]
    string Label;

    [SerializeField]
    int frequency;

    [SerializeField]
    SO_GameSettings gameSettings;

    [SerializeField]
    SO_GameResources gameResources;

    [SerializeField]
    public Counter counterType;

    [SerializeField]
    GameManagerBehavior gameManagerBehavior;

    [SerializeField]
    AudioSource sfxAudioSource;

    [SerializeField]
    GameObject taskBar;

    [SerializeField] private Renderer Renderer;
    [SerializeField] private Color UncompletedTaskLvlColor;
    [SerializeField] private Color CompletedTaskLvlColor;
    [SerializeField] private string PropertyName = "Base Color";

    int totalCounter;
    int counter;
    int taskCounter;

    bool taskBarInCooldown = false;
    bool interactionEnabled = false;
    bool stopCoroutine = false;

    private MaterialPropertyBlock _mpbUncompletedTaskLvl;
    private MaterialPropertyBlock _mpbCompletedTaskLvl;

    Slider counterBar;

    AudioClip[] taskSoundLibrary;
    MeshRenderer[] taskBarLevels;

    WaitForSeconds countdownTime;
    WaitForSeconds cooldownTime;

    private void Awake()
    {
        _mpbUncompletedTaskLvl = new MaterialPropertyBlock();
        _mpbUncompletedTaskLvl.SetColor(PropertyName, UncompletedTaskLvlColor);

        _mpbCompletedTaskLvl = new MaterialPropertyBlock();
        _mpbCompletedTaskLvl.SetColor(PropertyName, CompletedTaskLvlColor);
    }

    private void OnDisable()
    {
        stopCoroutine = true;
    }

    private void OnEnable()
    {
        counterBar = uiTextElement.transform.Find("CounterBar").GetComponent<Slider>();

        stopCoroutine = false;
        taskCounter = 0;
        totalCounter = gameSettings.Selector(counterType);
        counter = totalCounter;
        UpdateCounterText();
        StartCoroutine(CounterCountdown());
    }

    private void Start()
    {
        countdownTime = new WaitForSeconds(frequency);
        cooldownTime = new WaitForSeconds(gameSettings.taskCooldownTime);

        taskBar = transform.Find("TaskBar").gameObject;
        if (taskBar == null)
        {
            throw new Exception("TaskBar GameObject not found as a child of the TriggerBehavior GameObject.");
        }
        taskBarLevels = taskBar.GetComponentsInChildren<MeshRenderer>();

        taskSoundLibrary = gameResources.GetClickClips(counterType);
    }

    private void Update()
    {
        CheckInteraction();
        if (taskCounter == 0 && !taskBarInCooldown && !taskBar.activeSelf)
        {
            taskBar.SetActive(true);
        }
    }

    private void CheckInteraction()
    {
        if (!interactionEnabled || taskBarInCooldown)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            var clip = taskSoundLibrary[UnityEngine.Random.Range(0, taskSoundLibrary.Length)];
            sfxAudioSource.clip = clip;
            sfxAudioSource.Play();
            taskCounter++;

            Renderer = taskBarLevels[taskCounter - 1];

            Renderer.SetPropertyBlock(_mpbCompletedTaskLvl);
            if (taskCounter < gameSettings.totalTaskCounterRequired)
            {
                return;
            }

            sfxAudioSource.clip = gameResources.GetTaskCompletedClip(counterType);
            sfxAudioSource.Play();

            foreach (var level in taskBarLevels)
            {
                level.SetPropertyBlock(_mpbUncompletedTaskLvl);
            }

            taskBar.SetActive(false);

            StartCoroutine(StartTaskBarCooldown());
            taskCounter = 0;
            counter = totalCounter;
            UpdateCounterText();
        }
    }

    public void RestartTaskBars()
    {
        foreach (var level in taskBarLevels)
        {
            level.SetPropertyBlock(_mpbUncompletedTaskLvl);
        }
        taskCounter = 0;
    }

    IEnumerator StartTaskBarCooldown()
    {
        taskBarInCooldown = true;
        var i = 0;
        while (i < 1)
        {
            i++;
            yield return cooldownTime;
        }
        taskBarInCooldown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        interactionEnabled = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        interactionEnabled = false;
    }

    void UpdateCounterText()
    {
        float normalizedValue = (float)counter / totalCounter;
        counterBar.value = Mathf.Clamp01(normalizedValue);
    }

    IEnumerator CounterCountdown()
    {
        while (!stopCoroutine)
        {
            if (!taskBarInCooldown)
            {
                counter--;
                CheckFailureState();
                UpdateCounterText();
            }

            yield return countdownTime;
        }
    }

    private void CheckFailureState()
    {
        if (counter <= 0)
        {
            if (!gameSettings.isTesting && !gameManagerBehavior.gameOver)
            {
                gameManagerBehavior.ShowDefeatScreen();
            }
            else
            {
                counter = totalCounter;
            }
        }
    }

    public void RestartCounterContdown()
    {
        counter = totalCounter;
        UpdateCounterText();
    }
}

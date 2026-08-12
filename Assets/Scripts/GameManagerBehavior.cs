using StarterAssets;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManagerBehavior : MonoBehaviour
{
    [SerializeField]
    public SO_GameSettings gameSettings;

    [SerializeField]
    SO_GameResources gameResources;

    [SerializeField]
    GameObject houseTriggersContainer;

    [SerializeField]
    GameObject clubTriggersContainer;

    [SerializeField]
    GameObject player;

    [SerializeField]
    AudioSource musicAudioSource;

    [SerializeField]
    Camera cam;

    [SerializeField]
    Canvas interLevelsUI;

    TextMeshProUGUI totalGameCounterUIElement;
    GameObject transitionPanel;
    Image transitionPanelImage;
    GameObject partyTimeElement;
    GameObject failureLayoutElement;
    GameObject dayOverElement;
    GameObject hiddenEndingElement;

    [SerializeField]
    Canvas HUD;

    TextMeshProUGUI hudDayCounter;
    GameObject hudHouseCounters;
    GameObject hudClubCounters;

    [Header("Corridor Settings")]
    [SerializeField]
    Vector3 playerCorridorPosition;

    [SerializeField]
    Vector3 playerCorridorRotation;

    [SerializeField]
    float camCorridorPositionZ;

    [Header("Club Settings")]
    [SerializeField]
    Vector3 playerClubPosition;

    [SerializeField]
    Vector3 playerClubRotation; 

    [SerializeField]
    float camClubPositionZ;

    [Header("House Settings")]
    [SerializeField]
    Vector3 playerHousePosition;

    [SerializeField]
    Vector3 playerHouseRotation; 

    [SerializeField]
    float camHousePositionZ;

    int totalGameCounter;
    int acumulatedHours;
    int dayCounter = 0;
    float hideEndingCounter = 0;

    enum CurrentStage
    {
        Day,
        Corridor,
        Night
    }

    PlayerInput playerInput;
    ThirdPersonController thirdPersonController;
    CharacterController characterController;
    CurrentStage currentStage = CurrentStage.Day;
    bool stopCoroutine = false;
    internal bool gameOver = false;
    internal bool isHideEndingOn = false;
    internal bool isGameStarted = false;
    int stageTotalHours = 12;

    private void Start()
    {
        #region Dependency Loading
        try
        {
            transitionPanel = interLevelsUI.transform.Find("TransitionWhitePanel").gameObject;
            if (transitionPanel == null)
            {
                throw new Exception("Missing TransitionWhitePanel object.");
            }
            transitionPanelImage = transitionPanel.GetComponent<Image>();
            dayOverElement = interLevelsUI.transform.Find("DayOverUIElement").gameObject;
            partyTimeElement = interLevelsUI.transform.Find("PartyTimeMessage").gameObject;
            failureLayoutElement = interLevelsUI.transform.Find("FailureLayout").gameObject;
            var totalGameCounterGO = failureLayoutElement.transform.Find("TotalGameCounterUIElement");
            if (totalGameCounterGO == null)
            {
                throw new Exception("TotalGameCounter object not found in interLevelsUI.");
            }
            totalGameCounterUIElement = totalGameCounterGO.GetComponent<TextMeshProUGUI>();
            hiddenEndingElement = interLevelsUI.transform.Find("HiddenEnding").gameObject;

            var hudDayCounterGO = HUD.transform.Find("DayCounter");
            if (hudDayCounterGO == null)
            {
                throw new Exception("DayCounter object not found in HUD.");
            }
            hudDayCounter = hudDayCounterGO.GetComponent<TextMeshProUGUI>();
            hudHouseCounters = HUD.transform.Find("HouseCounters").gameObject;
            hudClubCounters = HUD.transform.Find("ClubCounters").gameObject;

            if(player == null)
            {
                throw new Exception("Player object is not assigned in the inspector.");
            }
            playerInput = player.GetComponent<PlayerInput>();
            thirdPersonController = player.GetComponent<ThirdPersonController>();
            characterController = player.GetComponent<CharacterController>();
        }
        catch (Exception e)
        {
            throw e;
        }
        #endregion

        clubTriggersContainer.SetActive(false);
        acumulatedHours = 0;
        totalGameCounter = 0;
        StartCoroutine(TotalGameCounter());
        musicAudioSource.Play();
    }

    void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.F) && !isHideEndingOn)
        {
            RestartGame();
        }
    }

    IEnumerator TotalGameCounter()
    {
        var intervalDuration = gameSettings.stageDurationInSeconds / stageTotalHours;
        while (!stopCoroutine)
        {
            totalGameCounter++;
            hudDayCounter.text = $"{totalGameCounter}";

            if (totalGameCounter >= stageTotalHours)
            {
                FinishStage();
            }
            yield return new WaitForSeconds(intervalDuration);
        }
    }

    IEnumerator FadeInOrOut(bool isFadingIn)
    {
        float targetAlpha = isFadingIn ? 0f : 1f;

        while (!Mathf.Approximately(transitionPanelImage.color.a, targetAlpha))
        {
            var newAlpha = Mathf.MoveTowards(transitionPanelImage.color.a, targetAlpha, gameSettings.alphaFadeInOutIncrementalStep);
            transitionPanelImage.color = new Color(transitionPanelImage.color.r, transitionPanelImage.color.g, transitionPanelImage.color.b, newAlpha);
            yield return new WaitForSeconds(gameSettings.timeFadeInOutIncrementalStep);
        }

        transitionPanelImage.color = new Color(transitionPanelImage.color.r, transitionPanelImage.color.g, transitionPanelImage.color.b, targetAlpha);

        if (isFadingIn)
        {
            playerInput.enabled = true;
            thirdPersonController.enabled = true;
            characterController.enabled = true;
        }
    }

    private void FinishStage()
    {
        stopCoroutine = true;
        acumulatedHours += totalGameCounter;
        player.GetComponent<PlayerInput>().enabled = false;
        player.GetComponent<ThirdPersonController>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        HUD.gameObject.SetActive(false);
        transitionPanel.SetActive(true);

        switch (currentStage)
        {
            case CurrentStage.Day:
                StartCoroutine(TransitionToCorridorWhenDayFinished());
                break;
            case CurrentStage.Night:
                StartCoroutine(TransitionToHouseWhenNightFinished(false));
                break;
        }
    }

    IEnumerator TransitionToCorridorWhenDayFinished()
    {
        houseTriggersContainer.SetActive(false);
        yield return FadeInOrOut(false);

        musicAudioSource.Stop();
        musicAudioSource.clip = gameResources.corridorSong;
        musicAudioSource.Play();

        partyTimeElement.SetActive(true);

        player.transform.SetPositionAndRotation(playerCorridorPosition, new Quaternion(playerCorridorRotation.x, playerCorridorRotation.y, playerCorridorRotation.z, player.transform.rotation.w));
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camCorridorPositionZ);

        yield return new WaitForSeconds(2);
        partyTimeElement.SetActive(false);
        yield return FadeInOrOut(true);
    }

    IEnumerator TransitionToClub()
    {
        yield return FadeInOrOut(false);

        musicAudioSource.Stop();
        musicAudioSource.clip = gameResources.bolicheSong;
        musicAudioSource.Play();

        player.transform.SetPositionAndRotation(playerClubPosition, new Quaternion(playerClubRotation.x, playerClubRotation.y, playerClubRotation.z, player.transform.rotation.w));
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camClubPositionZ);

        yield return new WaitForSeconds(gameSettings.transitionBetweenStagesWaitingTimeInSeconds); 
        yield return FadeInOrOut(true);

        clubTriggersContainer.SetActive(true);

        HUD.gameObject.SetActive(true);
        hudHouseCounters.SetActive(false);
        hudClubCounters.SetActive(true);

        currentStage = CurrentStage.Night;

        stopCoroutine = false;

        totalGameCounter = 0;
        StartCoroutine(TotalGameCounter());
    }

    IEnumerator TransitionToHouseWhenNightFinished(bool isRestartedGame)
    {
        clubTriggersContainer.SetActive(false);
        yield return FadeInOrOut(false);

        musicAudioSource.Stop();
        musicAudioSource.clip = gameResources.houseSong;
        musicAudioSource.Play();

        if (!isRestartedGame)
        {
            dayOverElement.SetActive(true);
            dayCounter++;
            dayOverElement.GetComponent<TextMeshProUGUI>().text = $"DAY {dayCounter} IS OVER";
        }

        player.transform.SetPositionAndRotation(playerHousePosition, new Quaternion(playerHouseRotation.x, playerHouseRotation.y, playerHouseRotation.z, player.transform.rotation.w));
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camHousePositionZ);

        yield return new WaitForSeconds(gameSettings.transitionBetweenStagesWaitingTimeInSeconds);

        dayOverElement.SetActive(false);
        yield return FadeInOrOut(true);


        houseTriggersContainer.SetActive(true);
        HUD.gameObject.SetActive(true);
        hudHouseCounters.SetActive(true);
        hudClubCounters.SetActive(false);

        currentStage = CurrentStage.Day;

        stopCoroutine = false;

        totalGameCounter = 0;
        StartCoroutine(TotalGameCounter());
    }

    public void ShowDefeatScreen()
    {
        if (!gameOver)
        {
            stopCoroutine = true;

            musicAudioSource.Stop();
            var clip = gameResources.gameOverSong;
            musicAudioSource.PlayOneShot(clip);

            acumulatedHours += totalGameCounter;
            failureLayoutElement.SetActive(true);
            totalGameCounterUIElement.text = $"You survived the autodestructive loop: {dayCounter} Days and {acumulatedHours} Hours";
            gameOver = true;
        }
    }

    public void EnterClub()
    {
        player.GetComponent<PlayerInput>().enabled = false;
        player.GetComponent<ThirdPersonController>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        StartCoroutine(TransitionToClub());
    }

    public void RestartGame()
    {
        acumulatedHours = 0;
        gameOver = false;

        TriggerBehavior[] triggers;

        if (currentStage == CurrentStage.Day)
        {
            triggers = houseTriggersContainer.GetComponentsInChildren<TriggerBehavior>();
        }
        else
        {
            triggers = clubTriggersContainer.GetComponentsInChildren<TriggerBehavior>();
        }

        foreach (var trigger in triggers)
        {
            trigger.RestartCounterContdown();
        }

        failureLayoutElement.SetActive(false);
        StartCoroutine(TransitionToHouseWhenNightFinished(true));
    }

    internal void IncrementHiddenEndingCounter()
    {
        hideEndingCounter += Time.deltaTime;

        if (hideEndingCounter >= gameSettings.hideEndingLimit && !gameOver)
        {
            stopCoroutine = true;

            musicAudioSource.Stop();
            musicAudioSource.clip = gameResources.hiddenEndingSong;
            musicAudioSource.Play();

            HUD.gameObject.SetActive(false);
            player.GetComponent<ThirdPersonController>().enabled = false;
            hiddenEndingElement.SetActive(true);
            gameOver = true;
            isHideEndingOn = true;
        }
    }

    internal void RestartHiddenEndingCounter()
    {
        hideEndingCounter = 0;
    }
}

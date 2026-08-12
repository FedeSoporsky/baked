using StarterAssets;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManagerBehavior : MonoBehaviour
{
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
    TextMeshProUGUI dayOverElementText;
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
    int accumulatedHours;
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
    readonly int stageTotalHours = 12;
    int intervalDuration;

    WaitForSeconds timeFadeInOutIncrementalStep;
    WaitForSeconds transitionBetweenStagesTime;
    WaitForSeconds intervalTime;

    private void Start()
    {
        #region Dependency Loading
        //This approach was prefered over inspector references because of the size of the project
        //and the amount of references that would be needed to be set in the inspector.
        transitionPanel = interLevelsUI.transform.Find("TransitionWhitePanel").gameObject;
        if (transitionPanel == null)
        {
            throw new Exception("Missing TransitionWhitePanel object.");
        }
        transitionPanelImage = transitionPanel.GetComponent<Image>();
        dayOverElement = interLevelsUI.transform.Find("DayOverUIElement").gameObject;
        if (dayOverElement == null)
        {
            throw new Exception("Missing DayOverUIElement object.");
        }
        dayOverElementText = dayOverElement.GetComponent<TextMeshProUGUI>();
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

        if (player == null)
        {
            throw new Exception("Player object is not assigned in the inspector.");
        }
        playerInput = player.GetComponent<PlayerInput>();
        thirdPersonController = player.GetComponent<ThirdPersonController>();
        characterController = player.GetComponent<CharacterController>();
        #endregion

        intervalDuration = gameSettings.stageDurationInSeconds / stageTotalHours;

        intervalTime = new WaitForSeconds(intervalDuration);
        timeFadeInOutIncrementalStep = new WaitForSeconds(gameSettings.timeFadeInOutIncrementalStep);
        transitionBetweenStagesTime = new WaitForSeconds(gameSettings.transitionBetweenStagesWaitingTimeInSeconds);

        clubTriggersContainer.SetActive(false);
        accumulatedHours = 0;
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
        while (!stopCoroutine)
        {
            totalGameCounter++;
            hudDayCounter.text = $"{totalGameCounter}";

            if (totalGameCounter >= stageTotalHours)
            {
                FinishStage();
            }
            yield return intervalTime;
        }
    }

    IEnumerator FadeInOrOut(bool isFadingIn)
    {
        float targetAlpha = isFadingIn ? 0f : 1f;

        while (!Mathf.Approximately(transitionPanelImage.color.a, targetAlpha))
        {
            var newAlpha = Mathf.MoveTowards(transitionPanelImage.color.a, targetAlpha, gameSettings.alphaFadeInOutIncrementalStep);
            transitionPanelImage.color = new Color(transitionPanelImage.color.r, transitionPanelImage.color.g, transitionPanelImage.color.b, newAlpha);
            yield return timeFadeInOutIncrementalStep;
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
        accumulatedHours += totalGameCounter;
        playerInput.enabled = false;
        thirdPersonController.enabled = false;
        characterController.enabled = false;
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

        yield return transitionBetweenStagesTime;
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

        yield return transitionBetweenStagesTime;
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
            dayOverElementText.text = $"DAY {dayCounter} IS OVER";
        }

        player.transform.SetPositionAndRotation(playerHousePosition, new Quaternion(playerHouseRotation.x, playerHouseRotation.y, playerHouseRotation.z, player.transform.rotation.w));
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camHousePositionZ);

        yield return transitionBetweenStagesTime;

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

            accumulatedHours += totalGameCounter;
            failureLayoutElement.SetActive(true);
            totalGameCounterUIElement.text = $"You survived the autodestructive loop: {dayCounter} Days and {accumulatedHours} Hours";
            gameOver = true;
        }
    }

    public void EnterClub()
    {
        playerInput.enabled = false;
        thirdPersonController.enabled = false;
        characterController.enabled = false;
        StartCoroutine(TransitionToClub());
    }

    public void RestartGame()
    {
        accumulatedHours = 0;
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
            thirdPersonController.enabled = false;
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

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

    enum Stage
    {
        House,
        Corridor,
        Club
    }

    PlayerInput playerInput;
    ThirdPersonController thirdPersonController;
    CharacterController characterController;
    Stage currentStage = Stage.House;

    internal bool gameOver = false;
    internal bool isHideEndingOn = false;
    internal bool isGameStarted = false;
    readonly int stageTotalHours = 12;
    int intervalDuration;
    bool isRestartedGame = false;

    IEnumerator totalGameCounterCoroutine;

    WaitForSeconds timeFadeInOutIncrementalStep;
    WaitForSeconds intervalTime;
    WaitForSeconds readingTime;

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
        readingTime = new WaitForSeconds(gameSettings.ReadingTime);

        clubTriggersContainer.SetActive(false);
        accumulatedHours = 0;
        totalGameCounter = 0;

        totalGameCounterCoroutine = TotalGameCounter();
        StartCoroutine(totalGameCounterCoroutine);

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
        while (true)
        {
            totalGameCounter++;
            hudDayCounter.text = $"{totalGameCounter}";

            if (totalGameCounter >= stageTotalHours)
            {
                FinishStage();
                break;
            }
            yield return intervalTime;
        }
    }

    IEnumerator FadeInOrOutToWhite(bool isFadingIn)
    {
        float targetAlpha = isFadingIn ? 1f : 0f;

        while (!Mathf.Approximately(transitionPanelImage.color.a, targetAlpha))
        {
            var newAlpha = Mathf.MoveTowards(transitionPanelImage.color.a, targetAlpha, gameSettings.alphaFadeInOutIncrementalStep);
            transitionPanelImage.color = new Color(transitionPanelImage.color.r, transitionPanelImage.color.g, transitionPanelImage.color.b, newAlpha);
            yield return timeFadeInOutIncrementalStep;
        }

        transitionPanelImage.color = new Color(transitionPanelImage.color.r, transitionPanelImage.color.g, transitionPanelImage.color.b, targetAlpha);

        if (isFadingIn)
        {
            StartCoroutine(ShowTransitionUI());
        }
        else
        {
            FinishTransitionToStage();
        }
    }

    private void FinishStage()
    {
        if (currentStage != Stage.Corridor)
        {
            StopCoroutine(totalGameCounterCoroutine);
            accumulatedHours += totalGameCounter;
        }

        MoveOutFromStage();
    }

    void MoveOutFromStage()
    {
        IsEnablingPlayer(false);
        HidePreviousStageUI();
        StartCoroutine(FadeInOrOutToWhite(true));
    }

    IEnumerator ShowTransitionUI()
    {
        transitionPanel.SetActive(true);
        switch (currentStage)
        {
            case Stage.House:
                partyTimeElement.SetActive(true);
                break;
            case Stage.Corridor:
                break;
            case Stage.Club:
                if (!isRestartedGame)
                {
                    dayOverElement.SetActive(true);
                    dayCounter++;
                    dayOverElementText.text = $"DAY {dayCounter} IS OVER";
                }
                break;
            default:
                break;
        }

        if (currentStage != Stage.Corridor)
        {
            yield return readingTime;
        }

        ChangeStageMusicAndRepositionPlayer();
    }

    private void HidePreviousStageUI()
    {
        HUD.gameObject.SetActive(false);
        switch (currentStage)
        {
            case Stage.House:
                houseTriggersContainer.SetActive(false);
                hudHouseCounters.SetActive(false);
                break;
            case Stage.Corridor:
                break;
            case Stage.Club:
                clubTriggersContainer.SetActive(false);
                hudClubCounters.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void ChangeStageMusicAndRepositionPlayer()
    {
        musicAudioSource.Stop();

        switch (currentStage)
        {
            case Stage.House:
                musicAudioSource.clip = gameResources.corridorSong;

                player.transform.SetPositionAndRotation(playerCorridorPosition, new Quaternion(playerCorridorRotation.x, playerCorridorRotation.y, playerCorridorRotation.z, player.transform.rotation.w));
                cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camCorridorPositionZ);

                break;
            case Stage.Corridor:
                musicAudioSource.clip = gameResources.clubSong;

                player.transform.SetPositionAndRotation(playerClubPosition, new Quaternion(playerClubRotation.x, playerClubRotation.y, playerClubRotation.z, player.transform.rotation.w));
                cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camClubPositionZ);

                break;
            case Stage.Club:
                musicAudioSource.clip = gameResources.houseSong;

                player.transform.SetPositionAndRotation(playerHousePosition, new Quaternion(playerHouseRotation.x, playerHouseRotation.y, playerHouseRotation.z, player.transform.rotation.w));
                cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camHousePositionZ);
                break;
        }


        musicAudioSource.Play();
        StartCoroutine(FadeInOrOutToWhite(false));
        HideTransitionUI();
    }

    private void HideTransitionUI()
    {
        switch (currentStage)
        {
            case Stage.House:
                partyTimeElement.SetActive(false);
                break;
            case Stage.Corridor:
                break;
            case Stage.Club:
                dayOverElement.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void FinishTransitionToStage()
    {
        IsEnablingPlayer(true);
        switch (currentStage)
        {
            case Stage.House:
                currentStage = Stage.Corridor;
                break;
            case Stage.Corridor:
                HUD.gameObject.SetActive(true);
                clubTriggersContainer.SetActive(true);
                hudClubCounters.SetActive(true);

                currentStage = Stage.Club;

                totalGameCounter = 0;
                totalGameCounterCoroutine = TotalGameCounter();
                StartCoroutine(totalGameCounterCoroutine);
                break;
            case Stage.Club:
                HUD.gameObject.SetActive(true);
                houseTriggersContainer.SetActive(true);
                hudHouseCounters.SetActive(true);

                currentStage = Stage.House;

                totalGameCounter = 0;
                totalGameCounterCoroutine = TotalGameCounter();
                StartCoroutine(totalGameCounterCoroutine);
                break;
            default:
                break;
        }
    }

    void IsEnablingPlayer(bool state)
    {
        playerInput.enabled = state;
        thirdPersonController.enabled = state;
        characterController.enabled = state;
    }

    public void ShowDefeatScreen()
    {
        if (!gameOver)
        {
            StopCoroutine(totalGameCounterCoroutine);

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
        FinishStage();
    }

    public void RestartGame()
    {
        accumulatedHours = 0;
        gameOver = false;

        TriggerBehavior[] triggers;

        if (currentStage == Stage.House)
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
        isRestartedGame = true;
        //Set to the last one, so the transition will be to the house.
        currentStage = Stage.Club;
        FinishStage();
    }

    internal void IncrementHiddenEndingCounter()
    {
        hideEndingCounter += Time.deltaTime;

        if (hideEndingCounter >= gameSettings.hideEndingLimit && !gameOver)
        {
            StopCoroutine(totalGameCounterCoroutine);

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance = null;

    public string mainMenuScene;
    public string leaderboardKey;

    public Animator animator;
    public Button continueButton;
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public TMP_Text gameOverText;

    private bool newGame = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (GameManager.instance.dayNum > 0 || newGame)
        {
            newGame = false;
            continueButton.interactable = true;
            if (WorldMapManager.instance != null)
            {
                Debug.Log("b");
                WorldMapManager.instance.StartGame();
            }
        }
        else
        {
            animator.Play("MainMenuActive");
        }

        if (GameManager.instance.villageFood == 0)
        {
            GameManager.instance.playingGame = false;
            gameOverMenu.SetActive(true);
            gameOverText.text = $"The Village has Starved. You survived {GameManager.instance.dayNum} days.";
            StartCoroutine(SubmitScoreRoutine(GameManager.instance.dayNum));
            continueButton.interactable = false;
        }
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        bool done = false;
        string playerId = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerId, scoreToUpload, leaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully submitted score! :)");
            }
            else
            {
                Debug.LogWarning("Score submittion failed! ): " + response.Error);
            }
            done = true;
        });

        yield return new WaitWhile(() => !done);
    }

    public void StartGame()
    {
        if (!animator.IsInTransition(0) || !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayGame"))
        {
            animator.CrossFade("PlayGame", 0.1f);
        }
        GameManager.instance.playingGame = Time.timeScale > 0;
    }

    public void NewGame()
    {
        if (WorldMapManager.instance == null)
        {
            SceneManager.sceneLoaded += OnSceneLoadedStartGame;
            SceneManager.LoadScene(mainMenuScene);
            return;
        }

        StartNewGameOnWorldMap();
        WorldMapManager.instance.StartGame();
    }

    private void OnSceneLoadedStartGame(Scene arg0, LoadSceneMode arg1)
    {
        instance.newGame = true;
        instance.StartNewGameOnWorldMap();
        SceneManager.sceneLoaded -= OnSceneLoadedStartGame;
    }

    private void StartNewGameOnWorldMap()
    {
        Time.timeScale = 1;
        GameManager.instance.dayNum = 0;
        WorldMapManager.instance.CleanUp();
        GameManager.instance.CleanUp();
        GameManager.instance.playingGame = true;
        continueButton.interactable = true;

        if (newGame)
        {
            animator.Play("PlayGame");
            return;
        }

        if (!animator.IsInTransition(0) || !animator.GetCurrentAnimatorStateInfo(0).IsName("PlayGame"))
        {
            animator.CrossFade("PlayGame", 0.1f);
        }
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
        {
            if (WorldMapManager.instance != null)
            {
                if (GameManager.instance.playingGame)
                {
                    ActivateMainMenu();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        Time.timeScale = GameManager.instance.playingGame ? 0 : 1;
        pauseMenu.SetActive(GameManager.instance.playingGame);
        GameManager.instance.playingGame = !GameManager.instance.playingGame;
    }

    public void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        animator.CrossFade("MainMenu", 0.1f);
        GameManager.instance.playingGame = false;
    }

    private void OnDestroy()
    {
        instance = null;
    }
}

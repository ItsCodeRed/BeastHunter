using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;

public class MainMenu : MonoBehaviour
{
    public string leaderboardKey;

    public Animation anim;
    public Button continueButton;
    public GameObject mainMenu;
    public GameObject gameOverMenu;
    public TMP_Text gameOverText;

    private void Start()
    {
        if (GameManager.instance.dayNum > 0)
        {
            continueButton.interactable = true;
            WorldMapManager.instance.StartGame();
        }
        else
        {
            mainMenu.SetActive(true);
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
        anim.Blend("PlayGame");
        GameManager.instance.playingGame = true;
    }

    public void NewGame()
    {
        GameManager.instance.dayNum = 0;
        WorldMapManager.instance.CleanUp();
        GameManager.instance.CleanUp();
        WorldMapManager.instance.StartGame();
        anim.Blend("PlayGame");
        GameManager.instance.playingGame = true;
        continueButton.interactable = true;
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && GameManager.instance.playingGame)
        {
            ActivateMainMenu();
        }
    }

    public void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        anim.Blend("MainMenu");
        GameManager.instance.playingGame = false;
    }
}

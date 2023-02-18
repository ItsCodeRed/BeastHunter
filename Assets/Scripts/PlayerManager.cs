using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class PlayerManager : MonoBehaviour
{
    public TMP_InputField usernameField;
    public bool loggedIn = false;

    void Start()
    {
        usernameField.text = PlayerPrefs.GetString("Username");
        StartCoroutine(LoginRoutine());
    }

    public void SubmitUsername()
    {
        LootLockerSDKManager.SetPlayerName(usernameField.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set username! :)");
            }
            else
            {
                Debug.LogWarning("Failed to set username! :(" + response.Error);
            }
        });
        PlayerPrefs.SetString("Username", usernameField.text);
        PlayerPrefs.Save();
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Player was logged in! :)");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                PlayerPrefs.Save();
                loggedIn = true;
            }
            else
            {
                Debug.LogWarning("Player failed to log in! :( " + response.Error);
            }
            done = true;
        });
        yield return new WaitWhile(() => !done);
    }
}

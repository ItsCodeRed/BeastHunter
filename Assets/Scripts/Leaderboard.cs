using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class Leaderboard : MonoBehaviour
{
    public PlayerManager playerManager;

    public Animator boardAnim;
    public TextMeshProUGUI playerNames;
    public TextMeshProUGUI playerScores;


    private bool leaderboardShowing = false;

    public void ShowLeaderboard(string leaderboardKey)
    {
        if (boardAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            leaderboardShowing = !leaderboardShowing;

            if (leaderboardShowing)
            {
                boardAnim.Play("LeaderboardShow");
                StartCoroutine(FetchLeaderboardRoutine(leaderboardKey));
            }
            else
            {
                boardAnim.Play("LeaderboardClose");
            }
        }
    }

    public void HideLeaderboard()
    {
        if (leaderboardShowing)
        {
            leaderboardShowing = false;
            boardAnim.Play("LeaderboardClose");
        }
    }

    public void LoadLeaderboard(string leaderboardKey)
    {
        StartCoroutine(FetchLeaderboardRoutine(leaderboardKey));
    }

    public IEnumerator FetchLeaderboardRoutine(string key)
    {
        bool done = false;

        if (!playerManager.loggedIn)
        {
            Debug.LogWarning("Failed to generate leaderboard! ): LootLocker failed to log in.");
            done = true;
        }

        LootLockerSDKManager.GetScoreList(key, 100, 0, (response) =>
        {
            if (response.success)
            {
                string tempPlayerNames = "Name: \n";
                string tempPlayerScores = "Score: \n";

                LootLockerLeaderboardMember[] members = response.items;
                for (int i = 0; i < members.Length; i++)
                {
                    tempPlayerNames += members[i].rank + ". ";
                    if (members[i].player.name != "")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score + (members[i].score == 1 ? " Day\n" : " Days\n");
                    tempPlayerNames += "\n";
                }

                playerNames.text = tempPlayerNames;
                playerScores.text = tempPlayerScores;
            }
            else
            {
                Debug.LogWarning("Failed to generate leaderboard! ): " + response.Error);
            }

            done = true;
        });

        yield return new WaitWhile(() => !done);
    }
}

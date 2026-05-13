using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScreenDailyChallenge : UIScreen
{
    [SerializeField] private Transform modScroll;
    [SerializeField] private TextMeshProUGUI challengeTitle;
    [SerializeField] private TextMeshProUGUI challengeDescription;
    [SerializeField] private GameObject modDisplayPrefab;

    private TextMeshProUGUI TEMPOBJ;

    public void ActivateDailyChallenge()
    {
        List<List<object>> dailyChallenges = DataReader._instance.GetDailyChallengesTable();
        DateTime currentDateTime = DateTime.Now;

        int challengeID = currentDateTime.Day % 9;
        //challengeID = 8;
        List<int> mods = (List<int>)dailyChallenges[challengeID][3];
        List<int> descriptors = (List<int>)dailyChallenges[challengeID][4];

        foreach (Transform child in modScroll)
        {
            child.gameObject.SetActive(false);
        }
        // load modifiers
        foreach (int i in mods)
        {
            Modifiers._instance.AdjustMod((Mods)i, true);
        }

        PopulateDescriptors(descriptors);

        // load name
        challengeTitle.text = (string)dailyChallenges[challengeID][1];

        // load descritpion
        challengeDescription.text = (string)dailyChallenges[challengeID][2];
    }

    public void ClearDailyChallenge()
    {

        Modifiers._instance.ClearMods();

        //GoBack to title screen
    }

    public override void Activate(bool navigatableOnActivated = true)
    {
        UIController._instance.HideTitleScreen();
        ActivateDailyChallenge();
        base.Activate(navigatableOnActivated);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        gameObject.SetActive(false);
        UIController._instance.ActivateTitleScreen();
        ClearDailyChallenge();
    }

    public void StartDailyChallengeAdventure()
    {
        UIController._instance.StartAdventure();
    }

    private void PopulateDescriptors(List<int> descriptors)
    {
        foreach (int i in descriptors)
        {
            TextMeshProUGUI mod = Instantiate(TEMPOBJ, modScroll);
            mod.gameObject.SetActive(true);
            //mod.text = ParseHelper.CamelCaseToSpaced(((Descriptors)i).ToString());
        }
    }
}

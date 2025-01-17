using NorskaLibExamples.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    public SpreadsheetContainer spreadsheetContainer;
    public EventsData[] eventsData;
    public OptionsData[] optionsData;

    public void LoadData()
    {
        spreadsheetContainer = Resources.Load<SpreadsheetContainer>("SpreadsheetContainer");

        LoadEventsData();
        LoadOptionsData();
    }

    private void LoadEventsData()
    {
        eventsData = new EventsData[spreadsheetContainer.Content.EventsData.Count];

        for (int i = 0; i < eventsData.Length; i++)
        {
            EventsData eventData = spreadsheetContainer.Content.EventsData[i];
            eventsData[i] = new EventsData();

            if (eventData == null)
            {
                Debug.LogError("Event Data " + i + " is null in the SpreadsheetContainer");
                continue;
            }

            eventsData[i].ID = eventData.ID;
            eventsData[i].EEvent = eventData.EEvent;
            eventsData[i].DisplayName = eventData.DisplayName;
            eventsData[i].TrialMin = eventData.TrialMin;
            eventsData[i].Chance = eventData.Chance;
            eventsData[i].Text = eventData.Text;
            eventsData[i].Option0 = eventData.Option0;
            eventsData[i].Option1 = eventData.Option1;
            eventsData[i].Option2 = eventData.Option2;
            eventsData[i].Option3 = eventData.Option3;
        }
    }

    private void LoadOptionsData()
    {
        optionsData = new OptionsData[spreadsheetContainer.Content.OptionsData.Count];

        for (int i = 0; i < optionsData.Length; i++)
        {
            OptionsData optionData = spreadsheetContainer.Content.OptionsData[i];
            optionsData[i] = new OptionsData();

            if (optionData == null)
            {
                Debug.LogError("Option Data " + i + " is null in the SpreadsheetContainer");
                continue;
            }

            optionsData[i].ID = optionData.ID;
            optionsData[i].EOption = optionData.EOption;
            optionsData[i].DisplayName = optionData.DisplayName;
            optionsData[i].Outcome0 = optionData.Outcome0;
            optionsData[i].Value0 = optionData.Value0;
            optionsData[i].Chance0 = optionData.Chance0;
            optionsData[i].Text0 = optionData.Text0;
            optionsData[i].Outcome1 = optionData.Outcome1;
            optionsData[i].Value1 = optionData.Value1;
            optionsData[i].Chance1 = optionData.Chance1;
            optionsData[i].Text1 = optionData.Text1;
            optionsData[i].Outcome2 = optionData.Outcome2;
            optionsData[i].Value2 = optionData.Value2;
            optionsData[i].Chance2 = optionData.Chance2;
            optionsData[i].Text2 = optionData.Text2;
        }
    }
}


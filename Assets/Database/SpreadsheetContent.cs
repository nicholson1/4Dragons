using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventsData
{
    public int      ID;
    public EEvent   EEvent;
    public string   DisplayName;
    public int      TrialMin;
    public float    Chance;
    public string   Text;
    public EOption  Option0;
    public EOption  Option1;
    public EOption  Option2;
    public EOption  Option3;
}

[Serializable]
public class OptionsData
{
    public int      ID;
    public EOption  EOption;
    public string   DisplayName;
    public EOutcome Outcome0;
    public float    Value0;
    public float    Chance0;
    public string   Text0;
    public EOutcome Outcome1;
    public float    Value1;
    public float    Chance1;
    public string   Text1;
    public EOutcome Outcome2;
    public float    Value2;
    public float    Chance2;
    public string   Text2;
}

namespace NorskaLibExamples.Spreadsheets
{
    [Serializable]
    public class SpreadsheetContent
    {
        [SpreadsheetPage("Events")]
        public List<EventsData> EventsData;

        [SpreadsheetPage("Options")]
        public List<OptionsData> OptionsData;
    }

    [CreateAssetMenu(fileName = "SpreadsheetContainer", menuName = "SpreadsheetContainer")]
    public class SpreadsheetContainer : SpreadsheetsContainerBase
    {
        [SpreadsheetContent]
        [SerializeField] SpreadsheetContent content;
        public SpreadsheetContent Content => content;
    }
}
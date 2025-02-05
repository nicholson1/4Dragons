using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "Database", menuName = "Database")]
public class Database : ScriptableObject
{
    [SerializeField] bool saveToggle;
    [SerializeField] string speadsheetID = "1bPXe5Cy6gScdkFsFt2Mn0jbz0pZ0SgGnRjvqNUjot08";
    [SerializeField] string credentials = "AIzaSyA8s5Fik3T4s9of4awsu_WmQs8tSE4-N8w";
    
    public Tab eventsTab;
    public Tab optionsTab;
    public Tab outcomesTab;

    string _jsonRaw = "";
    string _eventsRaw = "";
    string _optionsRaw = "";
    string _outcomesRaw = "";

    public async void LoadTabs()
    {
        await ReadSheetDataAsync(GetRequestLink("Events"));
        _eventsRaw = _jsonRaw;
        eventsTab.SetEntries(JsonToDictionaryArray(_eventsRaw));
        LogEvents();

        await ReadSheetDataAsync(GetRequestLink("Options"));
        _optionsRaw = _jsonRaw;
        optionsTab.SetEntries(JsonToDictionaryArray(_optionsRaw));

        await ReadSheetDataAsync(GetRequestLink("Outcomes"));
        _outcomesRaw = _jsonRaw;
        outcomesTab.SetEntries(JsonToDictionaryArray(_outcomesRaw));
    }

    private void LogEvents()
    {
        Debug.Log("Log Events: ");

        for (int i = 0; i < eventsTab.rowEntries.Length; i++)
        {
            string row = "";

            for (int j = 0; j < eventsTab.rowEntries[i].columnList.Count; j++)
            {
                row += eventsTab.GetString(i, eventsTab.rowEntries[i].columnList[j].column);
                row += ", ";
            }

            Debug.Log(i + ": " + row);
        }
    }

    private Dictionary<string, string>[] JsonToDictionaryArray(string jsonRaw)
    {
        var parsedJson = JSON.Parse(jsonRaw);

        if (parsedJson == null || !parsedJson.HasKey("values"))
        {
            Debug.LogError("Invalid JSON format or missing 'values' key.");
            return new Dictionary<string, string>[0];
        }

        var valuesArray = parsedJson["values"].AsArray;
        if (valuesArray.Count < 2)
        {
            Debug.LogError("Insufficient data rows.");
            return new Dictionary<string, string> [0];
        }

        JSONArray headers = valuesArray[0].AsArray;
        Dictionary<string, string>[] source = new Dictionary<string, string>[valuesArray.Count - 1];

        for (int i = 1; i < valuesArray.Count; i++)
        {
            JSONArray rowData = valuesArray[i].AsArray;
            Dictionary<string, string> rowDict = new();

            for (int j = 0; j < headers.Count && j < rowData.Count; j++)
                if(headers[j] != "")
                    rowDict[headers[j]] = rowData[j];

            source[i - 1] = rowDict;
        }

        return source;
    }

    private string GetRequestLink(string tabName)
    {
        return $"https://sheets.googleapis.com/v4/spreadsheets/{speadsheetID}/values/{tabName}?key={credentials}";
    }

    private async Task ReadSheetDataAsync(string requestLink)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(requestLink))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield(); // Wait until the request completes.

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Web Request Error: {request.error}");
                return;
            }

            _jsonRaw = request.downloadHandler.text;
            Debug.Log($"Successfully accessed json data from {requestLink}");
            Debug.Log(_jsonRaw);
        }
    }
}

[System.Serializable]
public class RowEntry
{
    public List<ColumnValue> columnList = new List<ColumnValue>();
}

[System.Serializable]
public class ColumnValue
{
    public string column;
    public string value;
}

[System.Serializable]
public class Tab
{
    public RowEntry[] rowEntries = new RowEntry[0];

    public void SetEntries(Dictionary<string, string>[] source)
    {
        rowEntries = new RowEntry[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            Dictionary<string, string> rowEntry = source[i];
            rowEntries[i] = new RowEntry();

            foreach (KeyValuePair<string, string> cellEntry in rowEntry)
                rowEntries[i].columnList.Add(new ColumnValue { column = cellEntry.Key, value = cellEntry.Value });
        }
    }

    public string GetString(int row, string column)
    {
        if (row < 0 || row >= rowEntries.Length)
        {
            Debug.LogError("Invalid row: " + row);
            return "";
        }

        foreach (ColumnValue columnValue in rowEntries[row].columnList)
            if (columnValue.column == column)
                return columnValue.value;

        Debug.LogError("Invalid column: " + column);
        return "";
    }

    public bool GetBool(int row, string column)
    {
        return GetString(row, column) == "TRUE";
    }

    public int GetInt(int row, string column)
    {
        int result = 0;
        string cellData = GetString(row, column);

        if (!int.TryParse(cellData, out result))
            Debug.LogError("Invalid cellData: '" + cellData + "' did not parse into an int");

        return result;
    }

    public int GetEnum(Type enumType, int row, string column)
    {
        int result = 0;
        string cellData = GetString(row, column);

        if (Enum.TryParse(enumType, cellData, out object enumValue) && enumValue != null)
            result = (int)enumValue;
        else
            Debug.LogError("Invalid cellData: '" + cellData + "' did not parse into the Enum Type " + enumType);

        return result;
    }

    public float GetFloat(int row, string column)
    {
        float result = 0;
        string cellData = GetString(row, column);

        if (!float.TryParse(cellData, out result))
            Debug.LogError("Invalid cellData: '" + cellData + "' could not be parsed into a float.");

        return result;
    }

    public string[] GetStringColumn(string column)
    {
        string[] results = new string[rowEntries.Length];

        for (int i = 0; i < results.Length; i++)
            results[i] = GetString(i, column);

        return results;
    }

    public bool[] GetBoolColumn(string column)
    {
        bool[] results = new bool[rowEntries.Length];

        for (int i = 0; i < results.Length; i++)
            results[i] = GetBool(i, column);

        return results;
    }

    public int[] GetIntColumn(string column)
    {
        int[] results = new int[rowEntries.Length];

        for (int i = 0; i < results.Length; i++)
            results[i] = GetInt(i, column);

        return results;
    }

    public int[] GetEnumColumn(Type enumType, string column)
    {
        int[] results = new int[rowEntries.Length];

        for (int i = 0; i < results.Length; i++)
            results[i] = GetEnum(enumType, i, column);

        return results;
    }

    public float[] GetFloatColumn(string column)
    {
        float[] results = new float[rowEntries.Length];

        for (int i = 0; i < results.Length; i++)
            results[i] = GetFloat(i, column);

        return results;
    }

}
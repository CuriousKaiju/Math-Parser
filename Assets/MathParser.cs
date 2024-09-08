using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;


public class MathParser : MonoBehaviour
{
    [SerializeField] private List<StatComponent> _componentsList;
    [SerializeField] private List<string> _keyNames;
    [SerializeField] private List<string> _keyNamesTwinsF;
    [SerializeField] private List<string> _keyNamesTwinsD;
    [SerializeField] private string _path;

    private void Start()
    {
        ParseFile(_path);
        WriteToCSV();

    }
    public void ParseFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        StatComponent currentStatComponent = null;

        foreach (string line in lines)
        {
            // If the line contains "sum_values", stop parsing for the current StatComponent
            if (line.Contains("sum_values"))
            {
                currentStatComponent = null;  // Stop adding to the current StatComponent
                continue;
            }

            // Check if the line matches any distribution in _keyNames
            foreach (string key in _keyNames)
            {
                if (line.Contains(key))
                {
                    currentStatComponent = new StatComponent(key);  // Create new StatComponent
                    _componentsList.Add(currentStatComponent);

                    // Ensure that a SimpleValues object is added to _valueList
                    currentStatComponent._valueList.Add(new SimpleValues());

                    break;  // Only add one StatComponent per matching key
                }
            }

            // If we are parsing a StatComponent and the line contains valid data
            if (currentStatComponent != null)
            {
                string[] parts = line.Split(',');

                if (parts.Length == 3)
                {
                    // Add each part to its respective list in the SimpleValues object
                    string keyPart = parts[0].Trim();
                    string valuePart = parts[1].Trim();
                    string frequencyPart = parts[2].Trim();

                    currentStatComponent._valueList[0]._1_valueList.Add(keyPart);
                    currentStatComponent._valueList[0]._2_valueList.Add(valuePart);
                    currentStatComponent._valueList[0]._3_valueFrequencyList.Add(frequencyPart);
                }
            }
        }

        Debug.Log("Parsed components count: " + _componentsList.Count);
    }
    public void ParseFileTwins(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        StatComponent currentStatComponent = null;

        foreach (string line in lines)
        {
            // Check if a new section starts and handle ending the current section appropriately
            if (ShouldStartNewSection(line, currentStatComponent))
            {
                currentStatComponent = null;
            }

            foreach (string key in _keyNames)
            {
                if (line.StartsWith(key))
                {
                    string sectionType = line.Contains("frequency:") ? " Frequency" : " Distribution";
                    if (currentStatComponent == null || !currentStatComponent._nameOfTheKey.Equals(key + sectionType))
                    {
                        currentStatComponent = new StatComponent(key + sectionType);
                        _componentsList.Add(currentStatComponent);
                    }
                    break;
                }
            }

            // Parse data if we are within a valid section
            if (currentStatComponent != null)
            {
                ParseLineData(line, currentStatComponent);
            }
        }
    }

    private bool ShouldStartNewSection(string line, StatComponent current)
    {
        if (line.Contains("sum_values") || line.Contains("RTP_") || line.Contains("S_T_D_"))
        {
            return true;
        }

        foreach (string key in _keyNames)
        {
            if (line.StartsWith(key) && (line.Contains("frequency:") || line.Contains("distribution:")))
            {
                return current == null || !current._nameOfTheKey.StartsWith(key);
            }
        }
        return false;
    }

    private void ParseLineData(string line, StatComponent currentStatComponent)
    {
        if (!line.Contains(":")) return;

        var parts = line.Split(new[] { ':' }, 2);
        if (parts.Length < 2) return;

        string keyPart = parts[0].Trim();
        var valueParts = parts[1].Split(new[] { '=' }, 2);
        if (valueParts.Length < 2) return;

        string valuePart = valueParts[1].Trim().Split(' ')[0].Trim();
        SimpleValues values = new SimpleValues();
        values._1_valueList.Add(keyPart);
        values._3_valueFrequencyList.Add(valuePart);
        currentStatComponent._valueList.Add(values);
    }
    public void WriteToCSV()
    {
        StringBuilder sb = new StringBuilder();

        // Iterate over each component in the list
        foreach (StatComponent component in _componentsList)
        {
            // Title for each StatComponent
            sb.AppendLine(component._nameOfTheKey);

            // Headers for the columns, placed only once per component
            //sb.AppendLine("key;value_frequency");

            // Data for each SimpleValues in the component
            foreach (SimpleValues values in component._valueList)
            {
                for (int i = 0; i < values._1_valueList.Count; i++)
                {
                    // Convert frequency to string, replace dot with comma
                    string formattedFrequency = values._3_valueFrequencyList[i].ToString().Replace('.', ',');

                    // Append each line of data using semicolon as a delimiter
                    sb.AppendLine($"{values._1_valueList[i]};{formattedFrequency}");
                }
            }

            // Optionally, add an extra blank line after each component for clarity
            sb.AppendLine();
        }

        // Save the CSV content to a file
        string filePath = Path.Combine(Application.dataPath, "Data.csv");
        File.WriteAllText(filePath, sb.ToString());

        Debug.Log("Data written to CSV file at: " + filePath);
    }

}
[Serializable]  // This should now work
public class StatComponent
{
    [SerializeField] public string _nameOfTheKey;
    [SerializeField] public List<SimpleValues> _valueList;

    public StatComponent(string key)
    {
        _nameOfTheKey = key;
        _valueList = new List<SimpleValues>();
    }
}

[Serializable]  // Make sure SimpleValues is also serializable
public class SimpleValues
{
    [SerializeField] public List<string> _1_valueList = new List<string>();
    [SerializeField] public List<string> _2_valueList = new List<string>();
    [SerializeField] public List<string> _3_valueFrequencyList = new List<string>();
}

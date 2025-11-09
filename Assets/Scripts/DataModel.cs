using System.Collections.Generic;

[System.Serializable]
public class DataObject
{
    public string id;
    public string name;
    public Dictionary<string, string> data;
}

[System.Serializable]
public class CreateDataRequest
{
    public string name;
    public Dictionary<string, string> data;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class RestAPIManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent; 
    public GameObject dataItemPrefab;
    public Button refreshButton;
    public Button addButton;
    public GameObject addEditPanel;
    public TMP_InputField nameInput;
    public TMP_InputField dataInput;
    public Button sendButton;

    private string baseUrl = "https://api.restful-api.dev/objects";
    private List<DataObject> loadedData = new List<DataObject>();
    private bool isEditing = false;
    private string editingId = "";

    void Start()
    {
        refreshButton.onClick.AddListener(() => StartCoroutine(GetAllData()));
        addButton.onClick.AddListener(OpenAddPanel);
        sendButton.onClick.AddListener(OnSendData);
        addEditPanel.SetActive(false);

        StartCoroutine(GetAllData());
    }

    IEnumerator GetAllData()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"items\":" + request.downloadHandler.text + "}";
            var wrapper = JsonUtility.FromJson<DataWrapper>(json);
            loadedData = new List<DataObject>(wrapper.items);

            foreach (var obj in loadedData)
                CreateItemUI(obj);
        }
        else
        {
            Debug.LogError("GET failed: " + request.error);
        }
    }

    void CreateItemUI(DataObject obj)
    {
        GameObject item = Instantiate(dataItemPrefab, contentParent);
        item.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = obj.name;
        item.transform.Find("DataText").GetComponent<TextMeshProUGUI>().text = obj.data != null && obj.data.Count > 0
            ? string.Join(", ", obj.data)
            : "No data";

        item.transform.Find("DeleteButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(DeleteData(obj.id, item));
        });

        item.transform.Find("EditButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenEditPanel(obj);
        });
    }

    IEnumerator DeleteData(string id, GameObject item)
    {
        UnityWebRequest request = UnityWebRequest.Delete($"{baseUrl}/{id}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Destroy(item);
            Debug.Log("Deleted " + id);
        }
        else
        {
            Debug.LogError("DELETE failed: " + request.error);
        }
    }

    void OpenAddPanel()
    {
        isEditing = false;
        addEditPanel.SetActive(true);
        nameInput.text = "";
        dataInput.text = "";
    }

    void OpenEditPanel(DataObject obj)
    {
        isEditing = true;
        editingId = obj.id;
        addEditPanel.SetActive(true);
        nameInput.text = obj.name;
        dataInput.text = obj.data != null && obj.data.Count > 0 ? string.Join(",", obj.data) : "";
    }

    void OnSendData()
    {
        if (isEditing)
            StartCoroutine(PutData());
        else
            StartCoroutine(PostData());
    }

    IEnumerator PostData()
    {
        CreateDataRequest newData = new CreateDataRequest
        {
            name = nameInput.text,
            data = new Dictionary<string, string> { { "info", dataInput.text } }
        };

        string json = JsonUtility.ToJson(newData);
        UnityWebRequest request = new UnityWebRequest(baseUrl, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("POST Success");
            addEditPanel.SetActive(false);
            StartCoroutine(GetAllData());
        }
        else
        {
            Debug.LogError("POST failed: " + request.error);
        }
    }

    IEnumerator PutData()
    {
        CreateDataRequest updatedData = new CreateDataRequest
        {
            name = nameInput.text,
            data = new Dictionary<string, string> { { "info", dataInput.text } }
        };

        string json = JsonUtility.ToJson(updatedData);
        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/{editingId}", "PUT");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("PUT Success");
            addEditPanel.SetActive(false);
            StartCoroutine(GetAllData());
        }
        else
        {
            Debug.LogError("PUT failed: " + request.error);
        }
    }

    [System.Serializable]
    private class DataWrapper
    {
        public DataObject[] items;
    }
}

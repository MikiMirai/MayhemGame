using UnityEngine;

public class PumpkinPickup : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    public int value;

    private bool collected;

    public void OnTriggerEnter(Collider other)
    {
        if (!collected && other.tag == "Player")
        {
            CollectPupmkin();
        }
    }

    private void CollectPupmkin()
    {
        FindObjectOfType<GameManager>().AddPumpkin(value);
        collected = true;

        Destroy(gameObject);
    }

    public void LoadData(GameData data)
    {
        data.pumpkinsCollected.TryGetValue(id, out collected);
        if (collected)
        {
            gameObject.SetActive(false);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.pumpkinsCollected.ContainsKey(id))
        {
            data.pumpkinsCollected.Remove(id);
        }
        data.pumpkinsCollected.Add(id, collected);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]

    [SerializeField] private string fileName;
    
    private GameData gameData;

    private List<IDataPersistence> dataPersistencesObjects;

    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName); //Application.persistentDataPath gives operating system standard directory. https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html link to documentation if u want to change it later or check where it saves it miki.
        this.dataPersistencesObjects = FindAllDataPersistenceObjects();                 
        LoadGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public void NewGame()
    {
        this.gameData= new GameData();
    }

    public void LoadGame()
    {

        this.gameData = dataHandler.Load();// if there is no data it will return null

        if (this.gameData == null)
        {
            Debug.Log("No data found. Please create new Game");
            NewGame();// not sure if I want it
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistencesObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (IDataPersistence dataPersistenceObj in dataPersistencesObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

}

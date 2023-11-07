using UnityEngine;

[System.Serializable]
public class GameData 
{
    public int deathCount;

    public int carriedAmmo;

    public int magazineAmmo;

    public Vector3 playerPosition;

    public SerializableDictionary<string, bool> pumpkinsCollected;

    // The value defined in the constructor will be the default values
    // The game will start with when there is no save files
    public GameData()
    {
        this.deathCount = 0;
        this.carriedAmmo = 0;
        this.magazineAmmo = 0;
        playerPosition = Vector3.zero; //TODO: Change this to the defualt map starting point
        pumpkinsCollected = new SerializableDictionary<string, bool>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData 
{
    public int CarriedAmmo;

    public int MagazineAmmo;

    public GameData()
    {
        this.CarriedAmmo = 0;
        this.MagazineAmmo = 0;
    }
}

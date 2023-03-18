using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class DecalPainter : MonoBehaviour
{
    [SerializeField]
    private DecalTextureData[] decalData;

    [SerializeField]
    private GameObject decalPorjectorPrefab;

    [SerializeField]
    private int selectedDecalIndex;

    [SerializeField]
    private Image decalImage;

    [SerializeField]
    private InputAction decalSwitchAction;

    Material[] decalMaterials;

    private void Awake()
    {
        decalMaterials = new Material[decalData.Length];
        decalSwitchAction.Enable();
        //decalSwitchAction.performed += SwitchDecal;
        selectedDecalIndex = 0;
        //foreach (Image image in FindObjectsOfType<Image>())
        //{
        //    if (image.CompareTag("Decal"))
        //    {
        //        decalImage = image;
        //        break;
        //    }
        //}
    }
    //Setting up UI image to selected decal image
    //private void Start()
    //{
    //    decalImage.sprite = decalData[selectedDecalIndex].sprite;
    //}

    //private void SwitchDecal(InputAction.CallbackContext obj)
    //{
    //    selectedDecalIndex++;
    //    if (selectedDecalIndex >= decalData.Length)
    //        selectedDecalIndex = 0;
    //    decalImage.sprite = decalData[selectedDecalIndex].sprite;
    //}

    //You could get hit.point and hit.normal by shooting a ray in the direction of the wall
    //Ex:
    //RaycastHit hit;
    //if(Physics.Raycast(transform.position, cameraTransform.forward,out hit, 20)){ ...
    public void PaintDecal(Vector3 point, Vector3 normal, Collider collider)
    {
        //Prepare a decal
        GameObject decal = Instantiate(decalPorjectorPrefab, point, Quaternion.identity);
        DecalProjector projector = decal.GetComponent<DecalProjector>();
        if (decalMaterials[selectedDecalIndex] == null)
        {
            decalMaterials[selectedDecalIndex] = new Material(projector.material);
        }
        projector.material = decalMaterials[selectedDecalIndex];
        projector.material.SetTexture("Base_Map", decalData[selectedDecalIndex].sprite.texture);
        projector.size = decalData[selectedDecalIndex].size;
        decal.transform.forward = -normal;
    }

}

/// <summary>
/// Decal data to store sprite and size
/// </summary>
[Serializable]
public class DecalTextureData
{
    public Sprite sprite;
    public Vector3 size;
}

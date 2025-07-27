using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{

    public int attack, defense, skillCost, ability;

    [SerializeField]    
    private TMP_Text attackText, defenseText, skillCostText, abilityText;

    [SerializeField]
    private TMP_Text attackPreText, defensePreText, skillCostPreText, abilityPreText;

    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private GameObject selectedItemStats;

    [SerializeField]
    private GameObject selectedItemImage;


    // Start is called before the first frame update
    void Start()
    {
        UpdateEquipmentStats();
    }

    public void UpdateEquipmentStats()
    {
        attackText.text = attack.ToString();
        defenseText.text = defense.ToString();
        skillCostText.text = skillCost.ToString();
        abilityText.text = ability.ToString();
    }

    public void previewEquipmentStats(int attack, int defense, int skillCost, int ability, Sprite itemSprite)
    {
        
        attackPreText.text = attack.ToString();
        defensePreText.text = defense.ToString();
        skillCostPreText.text = skillCost.ToString();
        abilityPreText.text = ability.ToString();
       

        previewImage.sprite = itemSprite;

        selectedItemStats.SetActive(true);
        selectedItemImage.SetActive(true);
    }


    public void TurnOffPreviewStats()
    {
        selectedItemStats.SetActive(false);
        selectedItemImage.SetActive(false);
    }
}

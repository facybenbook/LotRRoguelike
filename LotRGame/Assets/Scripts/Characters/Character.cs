﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum that determines a character's gender and some stat modifiers
public enum Genders { Male, Female, Genderless };

[RequireComponent(typeof(RaceTypes))]
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Skills))]
[RequireComponent(typeof(PhysicalState))]
[RequireComponent(typeof(CombatStats))]
[RequireComponent(typeof(ActionList))]
[RequireComponent(typeof(CharacterSprites))]
[RequireComponent(typeof(PerkList))]
[System.Serializable]
public class Character : MonoBehaviour
{
    //This character's first name
    public string firstName = "";
    //This character's last name
    public string lastName = "";

    //This character's gender
    public Genders sex = Genders.Male;


    //References to each of the required components
    [HideInInspector]
    [System.NonSerialized]
    public RaceTypes charRaceTypes;
    [HideInInspector]
    [System.NonSerialized]
    public Inventory charInventory;
    [HideInInspector]
    [System.NonSerialized]
    public Skills charSkills;
    [HideInInspector]
    [System.NonSerialized]
    public PhysicalState charPhysState;
    [HideInInspector]
    [System.NonSerialized]
    public CombatStats charCombatStats;
    [HideInInspector]
    [System.NonSerialized]
    public ActionList charActionList;
    [HideInInspector]
    [System.NonSerialized]
    public CharacterSprites charSprites;
    [System.NonSerialized]
    public PerkList charPerks;


    //Function called when this character is created
    private void Awake()
    {
        //Setting the references to each of the required components
        this.charRaceTypes = this.GetComponent<RaceTypes>();
        this.charInventory = this.GetComponent<Inventory>();
        this.charSkills = this.GetComponent<Skills>();
        this.charPhysState = this.GetComponent<PhysicalState>();
        this.charCombatStats = this.GetComponent<CombatStats>();
        this.charActionList = this.GetComponent<ActionList>();
        this.charSprites = this.GetComponent<CharacterSprites>();
        this.charPerks = this.GetComponent<PerkList>();
    }


    //Function called externally from SaveLoadManager.cs to load this character's component data
    public void LoadCharacterFromSave(CharacterSaveData saveData_)
    {
        //Setting the Character.cs variables
        this.firstName = saveData_.firstName;
        this.lastName = saveData_.lastName;
        this.sex = saveData_.sex;
        
        //Setting the RaceTypes.cs variables
        this.charRaceTypes.race = saveData_.race;
        this.charRaceTypes.subtypeList = saveData_.subtypeList;
        
        //Setting all of the equipped items in Inventory.cs
        if (saveData_.helmObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.helmObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));
            
            itemObj.transform.SetParent(this.transform);
            this.charInventory.helm = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.helm = null;
        }
        if (saveData_.chestObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.chestObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.chestPiece = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.chestPiece = null;
        }
        if (saveData_.legObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.legObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.leggings = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.leggings = null;
        }
        if (saveData_.shoeObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.shoeObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.shoes = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.shoes = null;
        }
        if (saveData_.gloveObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.gloveObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.gloves = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.gloves = null;
        }
        if (saveData_.cloakObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.cloakObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.cloak = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.cloak = null;
        }
        if (saveData_.necklaceObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.necklaceObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.necklace = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.necklace = null;
        }
        if (saveData_.ringObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.ringObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.ring = itemObj.GetComponent<Armor>();
        }
        else
        {
            this.charInventory.ring = null;
        }
        if (saveData_.leftHandObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.leftHandObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.leftHand = itemObj.GetComponent<Weapon>();
        }
        else
        {
            this.charInventory.leftHand = null;
        }
        if (saveData_.rightHandObj != "")
        {
            PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.rightHandObj, typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));

            itemObj.transform.SetParent(this.transform);
            this.charInventory.rightHand = itemObj.GetComponent<Weapon>();
        }
        else
        {
            this.charInventory.rightHand = null;
        }
        
        //Looping through all of the inventory slot objects in the save data
        this.charInventory.itemSlots = new List<Item>();
        for(int i = 0; i < saveData_.inventorySlots.Count; ++i)
        {
            //If the current item is emtpy, we add an empty slot
            if(saveData_.inventorySlots[i] == "")
            {
                this.charInventory.itemSlots.Add(null);
            }
            //If the current item isn't empty, we add it's item component to our inventory
            else
            {
                PrefabIDTagData prefabID = JsonUtility.FromJson(saveData_.inventorySlots[i], typeof(PrefabIDTagData)) as PrefabIDTagData;
                GameObject itemObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(prefabID.objType, prefabID.iDNumber));
                
                itemObj.transform.SetParent(this.transform);
                this.charInventory.itemSlots.Add(itemObj.GetComponent<Item>());
            }
        }
        for(int s = 0; s < saveData_.stackedItems.Count; ++s)
        {
            //Making sure the item in this item stack matches the same item in the 
            //Getting the stack data
            InventoryItemStackData stackData = JsonUtility.FromJson(saveData_.stackedItems[s], typeof(InventoryItemStackData)) as InventoryItemStackData;
            GameObject stackedObj = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(stackData.objType, stackData.iDNumber));

            //Making sure the stacked object is actually an item
            if (stackedObj.GetComponent<Item>())
            {
                //Making sure the item in this stack matches the item in the designated inventory index
                if (stackData.itemStackIndex < this.charInventory.itemSlots.Count && 
                    this.charInventory.itemSlots[stackData.itemStackIndex].GetComponent<IDTag>().numberID == stackData.iDNumber)
                {
                    //Looping through every item that's in this stack
                    for (int si = 0; si < stackData.numberOfItemsInStack; ++si)
                    {
                        //Creating a new instance of the stacked item
                        GameObject stackedItem = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(stackData.objType, stackData.iDNumber));
                        //Parenting the stacked item to the one that's in the inventory slot
                        stackedItem.transform.SetParent(this.charInventory.itemSlots[stackData.itemStackIndex].transform);
                        //Increasing the stack size count in the inventory slot
                        this.charInventory.itemSlots[stackData.itemStackIndex].currentStackSize += 1;

                        //If the inventory slot has reached the max stack size, we stop
                        if(this.charInventory.itemSlots[stackData.itemStackIndex].currentStackSize >= this.charInventory.itemSlots[stackData.itemStackIndex].maxStackSize)
                        {
                            break;
                        }
                    }
                }
            }
        }
        this.charInventory.FindArmorStats();

        //Setting the variables in Skill.cs
        this.charSkills.LoadSkillValue(saveData_);
        
        //Setting the variables in PhysicalState.cs
        this.charPhysState.maxHealth = saveData_.maxHP;
        this.charPhysState.currentHealth = saveData_.currentHP;
        this.charPhysState.requiresFood = saveData_.requireFood;
        this.charPhysState.maxFood = saveData_.maxFood;
        this.charPhysState.currentFood = saveData_.currentFood;
        this.charPhysState.requiresWater = saveData_.requireWater;
        this.charPhysState.maxWater = saveData_.maxWater;
        this.charPhysState.currentWater = saveData_.currentWater;
        this.charPhysState.requiresSleep = saveData_.requireSleep;
        this.charPhysState.maxSleep = saveData_.maxSleep;
        this.charPhysState.currentSleep = saveData_.currentSleep;
        this.charPhysState.startingHealthCurve = saveData_.startingHealthCurve;
        this.charPhysState.healthCurveLevels = saveData_.healthCurveLevels;

        this.charPhysState.highestHealthPercent = saveData_.highestHealthPercent;
        this.charPhysState.highestFoodPercent = saveData_.highestFoodPercent;
        this.charPhysState.highestWaterPercent = saveData_.highestWaterPercent;
        this.charPhysState.highestSleepPercent = saveData_.highestSleepPercent;

        this.charPhysState.trackingHealthPercents = saveData_.trackingHealthPercents;
        this.charPhysState.trackingFoodPercents = saveData_.trackingFoodPercents;
        this.charPhysState.trackingWaterPercents = saveData_.trackingWaterPercents;
        this.charPhysState.trackingSleepPercents = saveData_.trackingSleepPercents;

        //Setting the variables in CombatStats.cs
        this.charCombatStats.currentInitiativeSpeed = saveData_.currentInitiativeSpeed;
        this.charCombatStats.startingPositionCol = saveData_.startingCol;
        this.charCombatStats.startingPositionRow = saveData_.startingRow;
        this.charCombatStats.accuracy = saveData_.accuracy;
        this.charCombatStats.evasion = saveData_.evasion;

        this.charCombatStats.combatEffects = new List<Effect>();
        for (int ce = 0; ce < saveData_.combatEffects.Count; ++ce)
        {
            this.charCombatStats.combatEffects.Add(JsonUtility.FromJson(saveData_.combatEffects[ce], typeof(Effect)) as Effect);
        }
        
        //Setting the variables in ActionList.cs
        this.charActionList.defaultActions = new List<Action>();
        for(int da = 0; da < saveData_.defaultActions.Count; ++da)
        {
            PrefabIDTagData actionData = JsonUtility.FromJson(saveData_.defaultActions[da], typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject actionObj = IDManager.globalReference.GetPrefabFromID(actionData.objType, actionData.iDNumber);
            this.charActionList.defaultActions.Add(actionObj.GetComponent<Action>());
        }
        this.charActionList.rechargingSpells = new List<SpellRecharge>();
        for (int rs = 0; rs < saveData_.rechargingSpells.Count; ++rs)
        {
            this.charActionList.rechargingSpells.Add(JsonUtility.FromJson(saveData_.rechargingSpells[rs], typeof(SpellRecharge)) as SpellRecharge);
        }
        
        //Setting the variables in CharacterSprites.cs
        this.charSprites.allSprites = saveData_.ourSprites;

        //Setting the variables in PerkList.cs
        this.charPerks.allPerks = new List<Perk>();
        for(int p = 0; p < saveData_.perkNames.Count; ++p)
        {
            PrefabIDTagData perkTagData = JsonUtility.FromJson(saveData_.perkNames[p], typeof(PrefabIDTagData)) as PrefabIDTagData;
            GameObject loadedPerk = GameObject.Instantiate(IDManager.globalReference.GetPrefabFromID(perkTagData.objType, perkTagData.iDNumber));
            this.charPerks.allPerks.Add(loadedPerk.GetComponent<Perk>());
        }
    }
}

//Class used in Character.cs and SaveLoadManager.cs to store all serialized character data
[System.Serializable]
public class CharacterSaveData
{
    //Variables in Character.cs
    public string firstName;
    public string lastName;
    public Genders sex;

    //Variables in RaceTypes.cs
    public RaceTypes.Races race;
    public List<RaceTypes.Subtypes> subtypeList;

    //Variables in Inventory.cs
    public string helmObj = "";
    public string chestObj = "";
    public string legObj = "";
    public string gloveObj = "";
    public string shoeObj = "";
    public string cloakObj = "";
    public string necklaceObj = "";
    public string ringObj = "";

    public string leftHandObj = "";
    public string rightHandObj = "";

    public List<string> inventorySlots;
    public List<string> stackedItems;

    //Variables in Skills.cs
    public int unarmed = 0;
    public int daggers = 0;
    public int swords = 0;
    public int mauls = 0;
    public int poles = 0;
    public int bows = 0;
    public int shields = 0;
    public int arcaneMagic = 0;
    public int holyMagic = 0;
    public int darkMagic = 0;
    public int fireMagic = 0;
    public int waterMagic = 0;
    public int windMagic = 0;
    public int electricMagic = 0;
    public int stoneMagic = 0;
    public int survivalist = 0;
    public int social = 0;

    //Variables in PhysicalState.cs
    public int maxHP = 0;
    public int currentHP = 0;
    public float maxFood = 0;
    public float currentFood = 0;
    public float maxWater = 0;
    public float currentWater = 0;
    public float maxSleep = 0;
    public float currentSleep = 0;
    public bool requireFood = true;
    public bool requireWater = true;
    public bool requireSleep = true;
    public HealthCurveTypes startingHealthCurve = HealthCurveTypes.Average;
    public int[] healthCurveLevels = new int[4];
    public float highestHealthPercent;
    public float highestFoodPercent;
    public float highestWaterPercent;
    public float highestSleepPercent;
    public List<float> trackingHealthPercents;
    public List<float> trackingFoodPercents;
    public List<float> trackingWaterPercents;
    public List<float> trackingSleepPercents;

    //Variables in CombatStats.cs
    public int startingCol = 0;
    public int startingRow = 0;
    public int accuracy = 0;
    public int evasion = 10;
    public float currentInitiativeSpeed = 0.01f;
    public List<string> combatEffects;

    //Variables in ActionList.cs
    public List<string> defaultActions;
    public List<string> rechargingSpells;

    //Variables in CharacterSprites.cs
    public CharSpritePackage ourSprites;

    //Variables in PerkList.cs
    public List<string> perkNames;


    //Constructor for this class
    public CharacterSaveData(Character characterToSave_)
    {
        //Setting variables from Character.cs
        this.firstName = characterToSave_.firstName;
        this.lastName = characterToSave_.lastName;
        this.sex = characterToSave_.sex;

        //Setting variables from RaceTypes.cs
        this.race = characterToSave_.charRaceTypes.race;
        this.subtypeList = characterToSave_.charRaceTypes.subtypeList;

        //Setting variables from Skills.cs
        this.unarmed = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Unarmed);
        this.daggers = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Daggers);
        this.swords = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Swords);
        this.mauls = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Mauls);
        this.poles = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Poles);
        this.bows = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Bows);
        this.shields = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Shields);
        this.arcaneMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.ArcaneMagic);
        this.holyMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.HolyMagic);
        this.darkMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.DarkMagic);
        this.fireMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.FireMagic);
        this.waterMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.WaterMagic);
        this.windMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.WindMagic);
        this.electricMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.ElectricMagic);
        this.stoneMagic = characterToSave_.charSkills.GetSkillLevelValue(SkillList.StoneMagic);
        this.survivalist = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Survivalist);
        this.social = characterToSave_.charSkills.GetSkillLevelValue(SkillList.Social);

        //Setting variables from PhysicalState.cs
        this.maxHP = characterToSave_.charPhysState.maxHealth;
        this.currentHP = characterToSave_.charPhysState.currentHealth;
        this.maxFood = characterToSave_.charPhysState.maxFood;
        this.currentFood = characterToSave_.charPhysState.currentFood;
        this.maxWater = characterToSave_.charPhysState.maxWater;
        this.currentWater = characterToSave_.charPhysState.currentWater;
        this.maxSleep = characterToSave_.charPhysState.maxSleep;
        this.currentSleep = characterToSave_.charPhysState.currentSleep;
        this.requireFood = characterToSave_.charPhysState.requiresFood;
        this.requireWater = characterToSave_.charPhysState.requiresWater;
        this.requireSleep = characterToSave_.charPhysState.requiresSleep;
        this.startingHealthCurve = characterToSave_.charPhysState.startingHealthCurve;
        this.healthCurveLevels = characterToSave_.charPhysState.healthCurveLevels;

        this.highestHealthPercent = characterToSave_.charPhysState.highestHealthPercent;
        this.highestFoodPercent = characterToSave_.charPhysState.highestFoodPercent;
        this.highestWaterPercent = characterToSave_.charPhysState.highestWaterPercent;
        this.highestSleepPercent = characterToSave_.charPhysState.highestSleepPercent;

        this.trackingHealthPercents = characterToSave_.charPhysState.trackingHealthPercents;
        this.trackingFoodPercents = characterToSave_.charPhysState.trackingFoodPercents;
        this.trackingWaterPercents = characterToSave_.charPhysState.trackingWaterPercents;
        this.trackingSleepPercents = characterToSave_.charPhysState.trackingSleepPercents;

        //Setting variables from CombatStats.cs
        this.currentInitiativeSpeed = characterToSave_.charCombatStats.currentInitiativeSpeed;
        this.startingCol = characterToSave_.charCombatStats.startingPositionCol;
        this.startingRow = characterToSave_.charCombatStats.startingPositionRow;
        this.accuracy = characterToSave_.charCombatStats.accuracy;
        this.evasion = characterToSave_.charCombatStats.evasion;

        this.combatEffects = new List<string>();
        for(int ce = 0; ce < characterToSave_.charCombatStats.combatEffects.Count; ++ce)
        {
            this.combatEffects.Add(JsonUtility.ToJson(characterToSave_.charCombatStats.combatEffects[ce]));
        }

        //Setting variables from ActionList.cs
        this.defaultActions = new List<string>();
        for(int da = 0; da < characterToSave_.charActionList.defaultActions.Count; ++da)
        {
            PrefabIDTagData actionIDData = new PrefabIDTagData(characterToSave_.charActionList.defaultActions[da].GetComponent<IDTag>());
            this.defaultActions.Add(JsonUtility.ToJson(actionIDData));
        }

        this.rechargingSpells = new List<string>();
        for(int rs = 0; rs < characterToSave_.charActionList.rechargingSpells.Count; ++rs)
        {
            this.rechargingSpells.Add(JsonUtility.ToJson(characterToSave_.charActionList.rechargingSpells[rs]));
        }

        //Setting variables from CharacterSprites.cs
        this.ourSprites = characterToSave_.charSprites.allSprites;

        //Setting all of the equipped object references
        if (characterToSave_.charInventory.helm != null)
        {
            this.helmObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.helm.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.chestPiece != null)
        {
            this.chestObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.chestPiece.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.leggings != null)
        {
            this.legObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.leggings.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.gloves != null)
        {
            this.gloveObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.gloves.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.shoes != null)
        {
            this.shoeObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.shoes.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.cloak != null)
        {
            this.cloakObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.cloak.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.necklace != null)
        {
            this.necklaceObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.necklace.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.ring != null)
        {
            this.ringObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.ring.GetComponent<IDTag>()));
        }

        if (characterToSave_.charInventory.leftHand != null)
        {
            this.leftHandObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.leftHand.GetComponent<IDTag>()));
        }
        if (characterToSave_.charInventory.rightHand != null)
        {
            this.rightHandObj = JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charInventory.rightHand.GetComponent<IDTag>()));
        }

        //Looping through all of the character inventory items to save their object references
        this.inventorySlots = new List<string>();
        this.stackedItems = new List<string>();
        for(int i = 0; i < characterToSave_.charInventory.itemSlots.Count; ++i)
        {
            //Making sure the current inventory object isn't null
            if (characterToSave_.charInventory.itemSlots[i] != null)
            {
                //Reference to the item's IDTag component
                IDTag itemTag = characterToSave_.charInventory.itemSlots[i].GetComponent<IDTag>();

                //Saving the IDTag info
                this.inventorySlots.Add(JsonUtility.ToJson(new PrefabIDTagData(itemTag)));

                //If the current item is a stack
                if(characterToSave_.charInventory.itemSlots[i].currentStackSize > 1)
                {
                    //Creating a new InventoryItemStackData class to store the item stack
                    InventoryItemStackData stack = new InventoryItemStackData(i, itemTag, characterToSave_.charInventory.itemSlots[i].currentStackSize);
                    //Adding a serialized version of the stack data to our list of stacked items
                    this.stackedItems.Add(JsonUtility.ToJson(stack));
                }
            }
            //If the current item is null, we set a null slot to keep the empty space
            else
            {
                this.inventorySlots.Add("");
            }
        }

        //Looping through all of the character perks to save their object references
        this.perkNames = new List<string>();
        for(int p = 0; p < characterToSave_.charPerks.allPerks.Count; ++p)
        {
            //Making sure the current perk isn't null
            if(characterToSave_.charPerks.allPerks[p] != null)
            {
                //Saving this perk's ID tag info
                this.perkNames.Add(JsonUtility.ToJson(new PrefabIDTagData(characterToSave_.charPerks.allPerks[p].GetComponent<IDTag>())));
            }
        }
    }
}


//Class used in Character.cs and SaveLoadManager.cs as a wrapper to store game object references for serialization
[System.Serializable]
public class GameObjectSerializationWrapper
{
    //Reference to the game object we're going to serialize
    public GameObject objToSave;

    //Constructor for this class
    public GameObjectSerializationWrapper(GameObject obj_)
    {
        this.objToSave = obj_;
    }
}


//Class used in Character.cs and SaveLoadManager.cs to store IDTag data
[System.Serializable]
public class PrefabIDTagData
{
    //The enum for what type of object this is
    public IDTag.ObjectType objType;
    //The int for the object's ID number
    public int iDNumber;

    //Constructor for this class
    public PrefabIDTagData(IDTag objIDTag_)
    {
        this.objType = objIDTag_.objType;
        this.iDNumber = objIDTag_.numberID;
    }
}


//Class used in Character.cs to store objects for serialization 
[System.Serializable]
public class InventoryItemStackData
{
    //Index where this item is stored in a character's inventory
    public int itemStackIndex = 0;
    //The enum for what type of object this is
    public IDTag.ObjectType objType;
    //The int for the object's ID number
    public int iDNumber;
    //The number of items in this stack
    public uint numberOfItemsInStack = 1;

    //Constructor for this class
    public InventoryItemStackData(int inventoryIndex_, IDTag objIDTag_, uint stackSize_)
    {
        this.itemStackIndex = inventoryIndex_;
        this.objType = objIDTag_.objType;
        this.iDNumber = objIDTag_.numberID;
        this.numberOfItemsInStack = stackSize_ - 1; //Removing one because the first one is already saved in the index position
    }
}
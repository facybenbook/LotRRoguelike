﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackAction : Action
{
    //The skill used for determining accuracy
    public SkillList weaponSkillUsed = SkillList.Unarmed;

    //Enum that determines how enemy evasion and armor affects the chance of this attack hitting
    public enum attackTouchType { Regular, IgnoreEvasion, IgnoreArmor, IgnoreEvasionAndArmor};
    public attackTouchType touchType = attackTouchType.Regular;

    //The percent chance that this attack will crit
    [Range(0, 1)]
    public float critChance = 0.05f;

    //The damage multiplier applied when this weapon crits
    public int critMultiplier = 2;

    //The amount added to the user's hit chance 
    public int accuracyBonus = 0;

    //The list of damage dice that are rolled when this attack deals damage
    public List<AttackDamage> damageDealt;

    //The list of effects that can proc when this attack hits
    public List<AttackEffect> effectsOnHit;

    //The list of projectiles that are spawned when this attack is used
    public List<ProjectileLauncher> projectilesToLaunch;

    //The object that's spawned with a timed activator component for our visual effect at the hit target
    public TimedActivator visualEffectOnHit;
    //Bool that determines if we spawn the visual effect on missed attacks or not
    public bool spawnVisualOnMiss = true;



    //Function inherited from Action.cs and called from CombatManager.cs so we can attack a target
    public override void PerformAction(CombatTile targetTile_)
    {
        //Calling the base function to start the cooldown time
        this.BeginActionCooldown();

        //Reference to the character performing this attack
        Character actingChar = CombatManager.globalReference.actingCharacters[0];
        //Reference to the character that's being attacked
        Character defendingChar;

        //Getting the tile that the acting character is on
        CombatTile actingCharTile = CombatManager.globalReference.FindCharactersTile(actingChar);
        CharacterSpriteBase cSprite = CombatManager.globalReference.GetCharacterSprite(actingChar);

        //Setting the direction the acting character faces
        this.SetDirectionFacing(targetTile_, actingCharTile, cSprite);

        //Looping through and triggering all combat effects on the acting character that happen on attack
        foreach(Effect e in actingChar.charCombatStats.combatEffects)
        {
            e.EffectOnAttack();

            //Checking to see if the character has died due to some effect. If so, we break the loop
            if(actingChar.charPhysState.currentHealth <= 0)
            {
                break;
            }
        }

        //Looping through and creating each of the launched projectiles for this attack
        Vector3 casterTile = CombatManager.globalReference.FindCharactersTile(CombatManager.globalReference.actingCharacters[0]).transform.position;
        foreach(ProjectileLauncher projectile in this.projectilesToLaunch)
        {
            GameObject newProjectile = GameObject.Instantiate(projectile.gameObject, casterTile, new Quaternion());
            //Parenting the projectile to the combat manager canvas
            newProjectile.transform.SetParent(CombatManager.globalReference.transform);
            //Telling the projectile to start moving
            newProjectile.GetComponent<ProjectileLauncher>().StartTravelPath(casterTile, targetTile_.transform.position);
        }

        //Making sure there's a character on the targeted tile
        if(targetTile_.objectOnThisTile != null && targetTile_.objectOnThisTile.GetComponent<Character>())
        {
            defendingChar = targetTile_.objectOnThisTile.GetComponent<Character>();
        }
        //If there isn't a character on the tile, nothing happens because it misses anything it could hit
        else
        {
            //If there are no attack damage rolls (like if the attack was just to inflict an effect) the "Miss" text isn't shown
            if (this.damageDealt.Count > 0)
            {
                CombatManager.globalReference.DisplayMissedAttack(this.timeToCompleteAction, targetTile_);
            }

            //If the visual effect doesn't require a hit to be spawned, we create it at the target tile
            if(this.spawnVisualOnMiss && this.visualEffectOnHit != null)
            {
                GameObject visual = GameObject.Instantiate(this.visualEffectOnHit.gameObject, targetTile_.transform.position, new Quaternion(), CombatManager.globalReference.transform);
            }

            //Looping through each effect that this attack can apply to see if any don't require the attack to land
            foreach(AttackEffect efc in this.effectsOnHit)
            {
                //If the effect doesn't require the attack to land, it's triggered
                if(!efc.requireHit)
                {
                    this.TriggerEffect(efc, targetTile_, actingChar);
                }
            }

            return;
        }

        //Before calculating damage, we need to find out if this attack hit. We start by rolling 1d100 to hit and adding this attack's accuracy bonus
        int hitRoll = this.FindAttackRoll(actingChar, defendingChar);
        
        //If the hit roll is still above 20%, they hit. If not, the attack misses
        if(hitRoll <= CombatManager.baseHitDC)
        {
            //If there are no attack damage rolls (like if the attack was just to inflict an effect) the "Miss" text isn't shown
            if (this.damageDealt.Count > 0)
            {
                //Miss
                CombatManager.globalReference.DisplayMissedAttack(this.timeToCompleteAction, targetTile_);
            }

            //If the visual effect doesn't require a hit to be spawned, we create it at the target tile
            if (this.spawnVisualOnMiss && this.visualEffectOnHit != null)
            {
                GameObject visual = GameObject.Instantiate(this.visualEffectOnHit.gameObject, targetTile_.transform.position, new Quaternion(), CombatManager.globalReference.transform);
            }

            //Looping through each effect that this attack can apply to see if any don't require the attack to land
            foreach (AttackEffect efc in this.effectsOnHit)
            {
                //If the effect doesn't require the attack to land, it's triggered
                if (!efc.requireHit)
                {
                    this.TriggerEffect(efc, targetTile_, actingChar);
                }
            }

            //Giving the attacking character skill EXP for a miss
            this.GrantSkillEXP(actingChar, this.weaponSkillUsed, true);

            return;
        }

        //Giving the attacking character skill EXP for a hit
        this.GrantSkillEXP(actingChar, this.weaponSkillUsed, false);

        //Checking to see if this attack crits
        float critMultiplier = 1; //Set to 1 in case we don't crit so it won't change anything
        float critRoll = Random.Range(0, 1);
        bool isCrit = false;

        //Float for the bonus damage multiplier from perks
        float critMultiplierBoost = 0;

        //Looping through all of the acting character's perks to see if they have perks for crit chance or multiplier
        foreach (Perk charPerk in actingChar.charPerks.allPerks)
        {
            //If the current perk increases crit chance, we need to see if it applies to this attack
            if(charPerk.GetType() == typeof(CritChanceBoostPerk))
            {
                CritChanceBoostPerk critPerk = charPerk.GetComponent<CritChanceBoostPerk>();

                //If the perk applies to this attack's required skill check
                if(critPerk.boostAllSkills || critPerk.skillCritToBoost == this.weaponSkillUsed)
                {
                    //Subtracting this perk's crit chance boost from the roll (since we're looking for lower numbers)
                    critRoll -= critPerk.critChanceBoost;
                }
            }
            //If the current perk increases crit damage multipliers, we see if it applies to this attack
            else if (charPerk.GetType() == typeof(CritMultiplierPerk))
            {
                CritMultiplierPerk critPerk = charPerk.GetComponent<CritMultiplierPerk>();

                //If the perk applies to this attack's required skill check
                if (critPerk.boostAllSkills || critPerk.skillCritToBoost == this.weaponSkillUsed)
                {
                    critMultiplierBoost += critPerk.critMultiplierBoost;
                }
            }
        }

        //If the crit roll is below the crit chance, the attack crits and we change the multiplier
        if(critRoll < this.critChance)
        {
            critMultiplier = this.critMultiplier + critMultiplierBoost;
            isCrit = true;
        }

        //Dictionary for the total amount of damage for each type that will be dealt with this attack
        Dictionary<CombatManager.DamageType, int> damageTypeTotalDamage = new Dictionary<CombatManager.DamageType, int>();
        //Dictionary for if all of the spell damage types for if the damage is completely negated
        Dictionary<CombatManager.DamageType, SpellResistTypes> spellResistDictionary = new Dictionary<CombatManager.DamageType, SpellResistTypes>();
        
        //Initializing the dictionaries correctly
        this.InitializeDamageDictionaries(damageTypeTotalDamage, spellResistDictionary);

        //Looping through each damage type for this attack
        foreach (AttackDamage atk in this.damageDealt)
        {
            //Int to hold all of the damage for the current attack
            int atkDamage = 0;

            //Adding the base damage
            atkDamage += atk.baseDamage;

            //Looping through each individual die rolled
            for(int d = 0; d < atk.diceRolled; ++d)
            {
                //Finding the value rolled on the current die
                atkDamage += Random.Range(1, atk.diceSides);
            }

            //Multiplying the damage by the crit multiplier
            atkDamage = Mathf.RoundToInt(atkDamage * critMultiplier);

            //Looping through the perks of the character that used this ability to see if they have any damage type boost perks
            foreach (Perk charPerk in actingChar.charPerks.allPerks)
            {
                //If the perk boosts a damage type that's the same as this damage type, we boost it
                if (charPerk.GetType() == typeof(DamageTypeBoostPerk) && atk.type == charPerk.GetComponent<DamageTypeBoostPerk>().damageTypeToBoost)
                {
                    atkDamage += charPerk.GetComponent<DamageTypeBoostPerk>().GetDamageBoostAmount(actingChar, isCrit, false, atk.type);
                }
            }

            //Looping through the defending character's perks to see if they have any spell resist or absorb perks
            foreach (Perk defPerk in defendingChar.charPerks.allPerks)
            {
                if (defPerk.GetType() == typeof(SpellResistAbsorbPerk))
                {
                    SpellResistAbsorbPerk resistPerk = defPerk.GetComponent<SpellResistAbsorbPerk>();

                    //Checking to see if the current damage type is the same as this spell resist perk
                    if(resistPerk.typeToResist == atk.type)
                    {
                        //Checking to see if the damage is negated entirely
                        if(resistPerk.negateAllDamage)
                        {
                            //If the resist type for this spell isn't on absorb, we can negate it. ALWAYS have preference to absorb because it heals
                            if (spellResistDictionary[atk.type] != SpellResistTypes.Absorb)
                            {
                                spellResistDictionary[atk.type] = SpellResistTypes.Negate;
                            }
                        }
                        //Checking to see if the damage is absorbed to heal the target
                        else if(resistPerk.absorbDamage)
                        {
                            spellResistDictionary[atk.type] = SpellResistTypes.Absorb;
                            //Applying the damage reduction so the defender isn't healed as much
                            damageTypeTotalDamage[atk.type] -= resistPerk.GetSpellResistAmount(defendingChar, isCrit, false);
                        }
                        //Otherwise we just get the amount that it normally resists
                        else
                        {
                            damageTypeTotalDamage[atk.type] -= resistPerk.GetSpellResistAmount(defendingChar, isCrit, false);
                        }
                    }
                }
            }

            //Adding the current attack's damage to the correct type
            damageTypeTotalDamage[atk.type] += atkDamage;
        }

        //Looping through the attacking character's perks to see if there's any bonus damage to add to this attack
        foreach (Perk charPerk in actingChar.charPerks.allPerks)
        {
            //If the perk is a damage boosting perk for the skill used to perform this action, we get the bonus damage from it
            if (charPerk.GetType() == typeof(SkillDamageBoostPerk) && this.weaponSkillUsed == charPerk.GetComponent<SkillDamageBoostPerk>().skillToBoost)
            {
                int perkDamage = charPerk.GetComponent<SkillDamageBoostPerk>().GetDamageBoostAmount(actingChar, isCrit, false);

                //If this perk applies the same damage type as this action, it's added to that damage type total
                if (charPerk.GetComponent<SkillDamageBoostPerk>().useActionDamageType && this.damageDealt.Count > 0)
                {
                    damageTypeTotalDamage[this.damageDealt[0].type] += perkDamage;
                }
                //Otherwise we apply the damage type to the perk's specified damage type total
                else
                {
                    damageTypeTotalDamage[charPerk.GetComponent<SkillDamageBoostPerk>().damageBoostType] += perkDamage;
                }
            }
        }

        //Subtracting the target's melee and spell damage resistance from our attack damage
        this.SubtractResistances(damageTypeTotalDamage, defendingChar);
        
        //Dealing damage to the target
        this.DealDamage(damageTypeTotalDamage, spellResistDictionary, defendingChar, targetTile_, isCrit);

        //Dealing threat to the target
        this.DealThreat(damageTypeTotalDamage, actingChar, defendingChar, isCrit);

        //Creating the visual effect at the target tile if it isn't null
        if (this.visualEffectOnHit != null)
        {
            GameObject visual = GameObject.Instantiate(this.visualEffectOnHit.gameObject, targetTile_.transform.position, new Quaternion(), CombatManager.globalReference.transform);
        }

        //Looping through each effect that this attack can cause and triggering them
        foreach (AttackEffect effect in this.effectsOnHit)
        {
            this.TriggerEffect(effect, targetTile_, actingChar);
        }
    }


    //Function called from TriggerEffect to find all targets within an effect's radius
    private List<Character> FindCharactersInAttackRange(CombatTile targetTile_, int attackRadius_)
    {
        //The list of characters that are returned
        List<Character> targets = new List<Character>();

        //If the attack radius is less than or equal to 0, it only includes the target tile
        if(attackRadius_ <= 0)
        {
            //If the target tile has an object on it with a character component, we return that character
            if(targetTile_.objectOnThisTile != null && targetTile_.objectOnThisTile.GetComponent<Character>())
            {
                targets.Add(targetTile_.objectOnThisTile.GetComponent<Character>());
                return targets;
            }
        }

        //If the radius is larger than 0, we have to find all tiles within range of the target
        List<CombatTile> tilesInRange = PathfindingAlgorithms.FindTilesInActionRange(targetTile_, attackRadius_);

        //Looping through each tile in range to find out if they have character objects on them
        foreach(CombatTile currentTile in tilesInRange)
        {
            if(currentTile.objectOnThisTile != null && currentTile.objectOnThisTile.GetComponent<Character>())
            {
                targets.Add(currentTile.objectOnThisTile.GetComponent<Character>());
            }
        }

        //Returning the list of characters
        return targets;
    }


    //Function called from PerformAction to set the direction this character faces when acting
    public virtual void SetDirectionFacing(CombatTile targetTile_, CombatTile actingCharTile_, CharacterSpriteBase cSprite_)
    {
        //If the difference in vertical space between the character tile and the target tile is greater than the difference in horizontal space
        if (Mathf.Abs(targetTile_.transform.position.y - actingCharTile_.transform.position.y) > Mathf.Abs(targetTile_.transform.position.x - actingCharTile_.transform.position.x))
        {
            //If the target tile is above the acting character's tile
            if (targetTile_.transform.position.y > actingCharTile_.transform.position.y)
            {
                //We make the character look up
                cSprite_.SetDirectionFacing(CharacterSpriteBase.DirectionFacing.Up);
            }
            //If the target tile is below the acting character's tile
            else
            {
                //We make the character look down
                cSprite_.SetDirectionFacing(CharacterSpriteBase.DirectionFacing.Down);
            }
        }
        //If the difference in horizontal space between the tiles is greater
        else
        {
            //If the target tile is right of the acting character's tile
            if (targetTile_.transform.position.x > actingCharTile_.transform.position.x)
            {
                //We make the character look right
                cSprite_.SetDirectionFacing(CharacterSpriteBase.DirectionFacing.Right);
            }
            //If the target tile is left of the acting character's tile
            else
            {
                //We make the character look left
                cSprite_.SetDirectionFacing(CharacterSpriteBase.DirectionFacing.Left);
            }
        }
    }


    //Function called from PerformAction and TriggerEffect to return an attack roll value
    public virtual int FindAttackRoll(Character actingChar_, Character defendingChar_)
    {
        //Int to hold the total attack roll
        int hitRoll = 0;

        //Rolling a random number between 1 and 100
        hitRoll += Random.Range(0, 100) + 1;

        //Adding this action's accuracy bonus
        hitRoll += this.accuracyBonus;

        //Adding the attacker's accuracy bonus
        hitRoll += actingChar_.charCombatStats.accuracy;

        //Adding the correct skill modifier of the acting character to their hit roll
        hitRoll += actingChar_.charSkills.GetSkillLevelValueWithMod(this.weaponSkillUsed);

        //Looping through the attacking character's perks to see if they have any accuracy boost perks
        foreach (Perk atkPerk in actingChar_.charPerks.allPerks)
        {
            if (atkPerk.GetType() == typeof(AccuracyBoostPerk))
            {
                AccuracyBoostPerk accuracyPerk = atkPerk.GetComponent<AccuracyBoostPerk>();
                //Making sure the perk either boosts all skill accuracy or the skill that this attack uses
                if (accuracyPerk.skillAccuracyToBoost == this.weaponSkillUsed || accuracyPerk.boostAllSkillAccuracy)
                {
                    hitRoll += accuracyPerk.baseAccuracyBoost;
                }
            }
        }

        //Looping through the defending character's perks to see if they have any armor or evasion boost perks
        int evasionPerkBoost = 0;
        int armorPerkBoost = 0;
        foreach (Perk charPerk in defendingChar_.charPerks.allPerks)
        {
            if (charPerk.GetType() == typeof(EvasionBoostPerk))
            {
                EvasionBoostPerk evasionPerk = charPerk.GetComponent<EvasionBoostPerk>();
                //Making sure the perk boosts evasion against this type of attack
                if (evasionPerk.blocksAllSkills || evasionPerk.skillToBlock == this.weaponSkillUsed)
                {
                    evasionPerkBoost += charPerk.GetComponent<EvasionBoostPerk>().evasionBoost;
                }
            }
            else if (charPerk.GetType() == typeof(ArmorBoostPerk))
            {
                ArmorBoostPerk armorPerk = charPerk.GetComponent<ArmorBoostPerk>();
                //Making sure the perk boosts armor against this type of attack
                if (armorPerk.blocksAllSkills || armorPerk.skillToBlock == this.weaponSkillUsed)
                {
                    armorPerkBoost += armorPerk.armorBoost;
                }
            }
        }

        //Finding the hit target's resistance and subtracting it from the attacker's hit roll
        switch (this.touchType)
        {
            case attackTouchType.Regular:
                hitRoll -= defendingChar_.charCombatStats.evasion;
                hitRoll -= defendingChar_.charInventory.totalPhysicalArmor;
                hitRoll -= evasionPerkBoost;
                hitRoll -= armorPerkBoost;
                break;
            case attackTouchType.IgnoreArmor:
                hitRoll -= defendingChar_.charCombatStats.evasion;
                hitRoll -= evasionPerkBoost;
                break;
            case attackTouchType.IgnoreEvasion:
                hitRoll -= defendingChar_.charInventory.totalPhysicalArmor;
                hitRoll -= armorPerkBoost;
                break;
            case attackTouchType.IgnoreEvasionAndArmor:
                //Nothing is subtracted
                break;
        }

        //Returning the total hit roll
        return hitRoll;
    }


    //Function called when an effect is triggered
    public virtual void TriggerEffect(AttackEffect effectToTrigger_, CombatTile targetTile_, Character actingChar_)
    {
        //Finding all targets within this effect's radius
        List<Character> targets = this.FindCharactersInAttackRange(targetTile_, effectToTrigger_.effectRadius);

        //If the effected race isn't "None", we need to loop through and remove all targets that don't qualify
        if(effectToTrigger_.effectedRace != RaceTypes.Races.None)
        {
            for(int r = 0; r < targets.Count; ++r)
            {
                //If the current character's race isn't the effected race and we only hit the effected type
                if(targets[r].charRaceTypes.race != effectToTrigger_.effectedRace && effectToTrigger_.hitEffectedType)
                {
                    //We remove the character from the targets list
                    targets.RemoveAt(r);
                    r -= 1;
                }
                //If the current character's race IS the effected race, but we hit everything except that type
                else if(targets[r].charRaceTypes.race == effectToTrigger_.effectedRace && !effectToTrigger_.hitEffectedType)
                {
                    //We remove the character from the targets list
                    targets.RemoveAt(r);
                    r -= 1;
                }
            }
        }

        //If the effected subtype isn't "None", we need to loop through and remove all targets that don't qualify
        if (effectToTrigger_.effectedType != RaceTypes.Subtypes.None)
        {
            for (int t = 0; t < targets.Count; ++t)
            {
                //If the current character's subtype isn't the effected subtype and we only hit the effected type
                if (!targets[t].charRaceTypes.subtypeList.Contains(effectToTrigger_.effectedType) && effectToTrigger_.hitEffectedType)
                {
                    //We remove the character from the targets list
                    targets.RemoveAt(t);
                    t -= 1;
                }
                //If the current character's subtype IS the effected subtype, but we hit everything except that type
                else if (targets[t].charRaceTypes.subtypeList.Contains(effectToTrigger_.effectedType) && !effectToTrigger_.hitEffectedType)
                {
                    //We remove the character from the targets list
                    targets.RemoveAt(t);
                    t -= 1;
                }
            }
        }

        //Bool to check if the acting character is a player character or an enemy
        bool isActingCharPlayer = true;
        if(CombatManager.globalReference.enemyCharactersInCombat.Contains(actingChar_))
        {
            isActingCharPlayer = false;
        }

        //Now we loop through and remove all targets that don't qualify based on the type of targets that are hit
        if(effectToTrigger_.effectedTargets != AttackEffect.EffectedTargets.Everyone)
        {
            for(int e = 0; e < targets.Count; ++e)
            {
                //If the current target is an enemy character
                if(CombatManager.globalReference.enemyCharactersInCombat.Contains(targets[e]))
                {
                    switch(effectToTrigger_.effectedTargets)
                    {
                        case AttackEffect.EffectedTargets.AlliesExceptAttacker:
                            //if the target is an enemy and the acting character is a player
                            if(isActingCharPlayer)
                            {
                                //This enemy is removed, because they aren't an ally of the player
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            //If the target is an enemy and the acting character is an enemy
                            else
                            {
                                //If this enemy is the acting character, they're removed
                                if (targets[e] == actingChar_)
                                {
                                    targets.RemoveAt(e);
                                    e -= 1;
                                }
                                //If this enemy is NOT the acting character, they're included in the effect
                            }
                            break;

                        case AttackEffect.EffectedTargets.AlliesOnly:
                            //if the target is an enemy and the acting character is a player
                            if (isActingCharPlayer)
                            {
                                //This enemy is removed, because they aren't an ally of the player
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            //If the target is an enemy and the acting character is an enemy
                            //They are included in the effect because they're allies
                            break;

                        case AttackEffect.EffectedTargets.Attacker:
                            //If this enemy character isn't the attacker, they're removed
                            if(targets[e] != actingChar_)
                            {
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.Defender:
                            //If this enemy character isn't the defending character, or if the target tile doesn't have a character on it, they are removed
                            if(targetTile_.objectOnThisTile == null || 
                                (targetTile_.objectOnThisTile != null && targetTile_.objectOnThisTile.GetComponent<Character>() && targetTile_.objectOnThisTile.GetComponent<Character>() != targets[e]))
                            {
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.EnemiesExceptDefender:
                            //if the target is an enemy and the acting character is a player
                            if (isActingCharPlayer)
                            {
                                //If this character is the defending character
                                if(targetTile_.objectOnThisTile != null && targetTile_.objectOnThisTile.GetComponent<Character>() && targetTile_.objectOnThisTile.GetComponent<Character>() == targets[e])
                                {
                                    //The target is removed, because we exclude the defending character
                                    targets.RemoveAt(e);
                                    e -= 1;
                                }
                                //If this character isn't the defending character, they are included, because they're enemies
                            }
                            //If the target is an enemy and the acting character is an enemy
                            else
                            {
                                //This enemy character is removed because they aren't an enemy
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.EnemiesOnly:
                            //if the target is an enemy and the acting character is a player
                                //They are included, because they're enemies
                            //If the target is an enemy and the acting character is an enemy
                            if(!isActingCharPlayer)
                            {
                                //This character is removed, because they're allies
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;
                    }
                }
                //If the current target is a player character
                else if(CombatManager.globalReference.playerCharactersInCombat.Contains(targets[e]))
                {
                    switch (effectToTrigger_.effectedTargets)
                    {
                        case AttackEffect.EffectedTargets.AlliesExceptAttacker:
                            //if the target is a player and the acting character is a player
                            if (isActingCharPlayer)
                            {
                                //If this player is the acting character, they're removed
                                if (targets[e] == actingChar_)
                                {
                                    targets.RemoveAt(e);
                                    e -= 1;
                                }
                                //If this player is NOT the acting character, they're included in the effect
                            }
                            //If the target is a player and the acting character is an enemy
                            else
                            {
                                //This player is removed, because they aren't an ally of the enemy
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.AlliesOnly:
                            //if the target is a player and the acting character is a player
                                //They are included in the effect because they're allies
                            //If the target is a player and the acting character is an enemy
                            if (!isActingCharPlayer)
                            {
                                //This player is removed, because they aren't an ally of the enemy
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.Attacker:
                            //If this player character isn't the attacker, they're removed
                            if (targets[e] != actingChar_)
                            {
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.Defender:
                            //If this player character isn't the defending character, or if the target tile doesn't have a character on it, they are removed
                            if (targetTile_.objectOnThisTile == null ||
                                (targetTile_.objectOnThisTile != null && targetTile_.objectOnThisTile.GetComponent<Character>() && targetTile_.objectOnThisTile.GetComponent<Character>() != targets[e]))
                            {
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            break;

                        case AttackEffect.EffectedTargets.EnemiesExceptDefender:
                            //if the target is a player and the acting character is a player
                            if (isActingCharPlayer)
                            {
                                //This player character is removed because they aren't an enemy
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            //If the target is a player and the acting character is an enemy
                            else
                            {
                                //If this character is the defending character
                                if (targetTile_.objectOnThisTile != null && targetTile_.objectOnThisTile.GetComponent<Character>() && targetTile_.objectOnThisTile.GetComponent<Character>() == targets[e])
                                {
                                    //The target is removed, because we exclude the defending character
                                    targets.RemoveAt(e);
                                    e -= 1;
                                }
                                //If this character isn't the defending character, they are included, because they're enemies
                            }
                            break;

                        case AttackEffect.EffectedTargets.EnemiesOnly:
                            //if the target is a player and the acting character is a player
                            if (isActingCharPlayer)
                            {
                                //This character is removed, because they're allies
                                targets.RemoveAt(e);
                                e -= 1;
                            }
                            //If the target is a player and the acting character is an enemy
                                //They are included, because they're enemies
                            break;
                    }
                }
            }
        }

        //Looping through all of the filtered targets in the list
        foreach (Character targetChar in targets)
        {
            //If the effect requires an additional hit chance, we need to roll for it
            bool effectHit = true;
            if(effectToTrigger_.requireSecondHitRoll)
            {
                //Rolling to see if we hit
                int hitRoll = this.FindAttackRoll(actingChar_, targetChar);

                //If the hit roll is below the required amount, the effect doesn't hit
                if(hitRoll <= CombatManager.baseHitDC)
                {
                    effectHit = false;
                }
            }

            //If the effect hit
            if (effectHit)
            {
                //Rolling to see if the effect hits the target or not
                float effectRoll = Random.Range(0, 1);

                //If the roll is less than the effect chance, it was sucessful
                if (effectRoll < effectToTrigger_.effectChance)
                {
                    //Creating an instance of the effect object prefab and triggering it's effect
                    GameObject effectObj = Instantiate(effectToTrigger_.effectToApply.gameObject, new Vector3(), new Quaternion());
                    effectObj.GetComponent<Effect>().TriggerEffect(actingChar_, targetChar, this.timeToCompleteAction);
                }
            }
        }
    }


    //Function called from PerformAction to initialize the damage type and spell type dictionaries
    public virtual void InitializeDamageDictionaries(Dictionary<CombatManager.DamageType, int> damageTypeTotalDamage_, Dictionary<CombatManager.DamageType, SpellResistTypes> spellResistDictionary_)
    {
        //Setting up all of the different damage types for the dictionary
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Slashing, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Stabbing, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Crushing, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Arcane, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Holy, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Dark, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Fire, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Water, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Wind, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Electric, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Nature, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Pure, 0);
        damageTypeTotalDamage_.Add(CombatManager.DamageType.Bleed, 0);

        //Setting up all of the spell resistance types
        spellResistDictionary_.Add(CombatManager.DamageType.Arcane, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Holy, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Dark, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Fire, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Water, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Wind, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Electric, SpellResistTypes.Normal);
        spellResistDictionary_.Add(CombatManager.DamageType.Nature, SpellResistTypes.Normal);
    }


    //Function called from PerformAction to subtract the target's resistances and armor from our attack damage
    public virtual void SubtractResistances(Dictionary<CombatManager.DamageType, int> damageTypeTotalDamage_, Character defendingChar_)
    {
        //Subtracing the defending character's physical armor resistances
        if (damageTypeTotalDamage_[CombatManager.DamageType.Slashing] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Slashing] -= defendingChar_.charInventory.totalSlashingArmor;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Stabbing] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Stabbing] -= defendingChar_.charInventory.totalStabbingArmor;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Crushing] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Crushing] -= defendingChar_.charInventory.totalCrushingArmor;
        }

        //Subtracting the defending character's magic resistances 
        if (damageTypeTotalDamage_[CombatManager.DamageType.Arcane] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Arcane] -= defendingChar_.charInventory.totalArcaneResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Fire] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Fire] -= defendingChar_.charInventory.totalFireResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Water] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Water] -= defendingChar_.charInventory.totalWaterResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Electric] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Electric] -= defendingChar_.charInventory.totalElectricResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Wind] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Wind] -= defendingChar_.charInventory.totalWindResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Nature] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Nature] -= defendingChar_.charInventory.totalNatureResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Holy] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Holy] -= defendingChar_.charInventory.totalHolyResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Dark] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Dark] -= defendingChar_.charInventory.totalDarkResist;
        }
        if (damageTypeTotalDamage_[CombatManager.DamageType.Bleed] > 0)
        {
            damageTypeTotalDamage_[CombatManager.DamageType.Bleed] -= defendingChar_.charInventory.totalBleedResist;
        }
    }


    //Function called from PerformAction to deal damage to this action's target
    public virtual void DealDamage(Dictionary<CombatManager.DamageType, int> damageTypeTotalDamage_, Dictionary<CombatManager.DamageType, SpellResistTypes> spellResistDictionary_, Character defendingChar_, CombatTile targetTile_, bool isCrit_)
    {
        //Dealing damage to the defending character and telling the combat manager to display how much was dealt
        defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Slashing]);
        CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Slashing], CombatManager.DamageType.Slashing, targetTile_, isCrit_);

        defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Stabbing]);
        CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Stabbing], CombatManager.DamageType.Stabbing, targetTile_, isCrit_);

        defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Crushing]);
        CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Crushing], CombatManager.DamageType.Crushing, targetTile_, isCrit_);

        //Dealing spell damage to the defending character based on their spell resist types
        if (spellResistDictionary_[CombatManager.DamageType.Arcane] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Arcane]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Arcane], CombatManager.DamageType.Arcane, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Arcane] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Arcane]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Arcane], CombatManager.DamageType.Arcane, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Arcane] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Arcane, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Arcane] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Fire] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Fire]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Fire], CombatManager.DamageType.Fire, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Fire] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Fire]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Fire], CombatManager.DamageType.Fire, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Fire] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Fire, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Fire] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Water] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Water]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Water], CombatManager.DamageType.Water, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Water] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Water]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Water], CombatManager.DamageType.Water, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Water] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Water, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Water] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Wind] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Wind]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Wind], CombatManager.DamageType.Wind, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Wind] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Wind]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Wind], CombatManager.DamageType.Wind, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Wind] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Wind, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Wind] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Electric] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Electric]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Electric], CombatManager.DamageType.Electric, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Electric] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Electric]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Electric], CombatManager.DamageType.Electric, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Electric] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Electric, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Electric] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Nature] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Nature]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Nature], CombatManager.DamageType.Nature, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Nature] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Nature]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Nature], CombatManager.DamageType.Nature, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Nature] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Nature, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Nature] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Holy] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Holy]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Holy], CombatManager.DamageType.Holy, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Holy] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Holy]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Holy], CombatManager.DamageType.Holy, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Holy] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Holy, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Holy] = 0;
        }

        if (spellResistDictionary_[CombatManager.DamageType.Dark] == SpellResistTypes.Normal)
        {
            defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Dark]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Dark], CombatManager.DamageType.Dark, targetTile_, isCrit_);
        }
        else if (spellResistDictionary_[CombatManager.DamageType.Dark] == SpellResistTypes.Absorb)
        {
            defendingChar_.charPhysState.HealCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Dark]);
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Dark], CombatManager.DamageType.Dark, targetTile_, isCrit_, true);
            damageTypeTotalDamage_[CombatManager.DamageType.Dark] = 0;
        }
        else
        {
            CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, 0, CombatManager.DamageType.Dark, targetTile_, isCrit_);
            damageTypeTotalDamage_[CombatManager.DamageType.Dark] = 0;
        }

        defendingChar_.charPhysState.DamageCharacter(damageTypeTotalDamage_[CombatManager.DamageType.Pure]);
        CombatManager.globalReference.DisplayDamageDealt(this.timeToCompleteAction, damageTypeTotalDamage_[CombatManager.DamageType.Pure], CombatManager.DamageType.Pure, targetTile_, isCrit_);
    }


    //Function called from PerformAction to deal threat to this action's target
    public virtual void DealThreat(Dictionary<CombatManager.DamageType, int> damageTypeTotalDamage_, Character actingChar_, Character defendingChar_, bool isCrit_)
    {
        //Increasing the threat to the target based on damage dealt
        int totalDamage = damageTypeTotalDamage_[CombatManager.DamageType.Slashing] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Stabbing] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Crushing] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Arcane] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Holy] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Dark] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Fire] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Water] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Wind] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Electric] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Nature] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Pure] +
                        damageTypeTotalDamage_[CombatManager.DamageType.Bleed];

        //Looping through the acting character's perks to see if they have any ThreatBoostPerk perks
        int bonusThreat = 0;
        foreach (Perk charPerk in actingChar_.charPerks.allPerks)
        {
            if (charPerk.GetType() == typeof(ThreatBoostPerk))
            {
                //Getting the threat boost perk component reference
                ThreatBoostPerk threatPerk = charPerk.GetComponent<ThreatBoostPerk>();

                //If the threat perk applies to all forms of damage
                if (threatPerk.threatenAllDamageTypes)
                {
                    bonusThreat += threatPerk.GetAddedActionThreat(totalDamage, isCrit_, false);
                }
                //Otherwise, we check for each damage type
                else
                {
                    switch (threatPerk.damageTypeToThreaten)
                    {
                        case CombatManager.DamageType.Slashing:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Slashing], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Stabbing:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Stabbing], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Crushing:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Crushing], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Arcane:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Arcane], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Holy:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Holy], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Dark:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Dark], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Fire:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Fire], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Water:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Water], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Wind:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Wind], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Electric:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Electric], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Nature:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Nature], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Pure:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Pure], isCrit_, false);
                            break;

                        case CombatManager.DamageType.Bleed:
                            bonusThreat += threatPerk.GetAddedActionThreat(damageTypeTotalDamage_[CombatManager.DamageType.Bleed], isCrit_, false);
                            break;
                    }
                }
            }
        }

        //If the attack crit, ALL enemies have their threat increased for 25% of the damage
        if (isCrit_)
        {
            //Getting 25% of the damage to pass to all enemies
            int threatForAll = (totalDamage + bonusThreat) / 4;
            CombatManager.globalReference.ApplyActionThreat(actingChar_, null, threatForAll, true);

            //Applying the rest of the threat to the defending character
            CombatManager.globalReference.ApplyActionThreat(actingChar_, defendingChar_, (totalDamage + bonusThreat) - threatForAll, false);
        }
        //If the attack wasn't a crit, only the defending character takes threat
        else
        {
            CombatManager.globalReference.ApplyActionThreat(actingChar_, defendingChar_, totalDamage + bonusThreat, false);
        }
    }


    //Function inherited from Action.cs to give the acting character skill EXP
    public override void GrantSkillEXP(Character abilityUser_, SkillList skillUsed_, bool abilityMissed_)
    {
        base.GrantSkillEXP(abilityUser_, skillUsed_, abilityMissed_);
    }
}



//Enum for the different spell resist types. Normal is taking damage, negate is taking 0 damage, absorb is healing
public enum SpellResistTypes { Normal, Negate, Absorb };

//Class used in AttackAction.cs to determine damage dealt when an attack hits
[System.Serializable]
public class AttackDamage
{
    //The type of damage that's inflicted
    public CombatManager.DamageType type = CombatManager.DamageType.Slashing;

    //The amount of damage inflicted before dice rolls
    public int baseDamage = 0;

    //The number of dice that are rolled
    public int diceRolled = 1;
    //The highest value of the type of die rolled
    public int diceSides = 6;
}

//Class used in AttackAction.cs to determine what effect can be applied when an attack happens and its chance of happening
[System.Serializable]
public class AttackEffect
{
    //The effect applied when this is triggered
    public Effect effectToApply;

    //If true, this effect only happens if the attack lands. If false, it will happen even if the initial attack misses
    public bool requireHit = true;

    //Bool where if true, this attack effect requires a separate roll to see if it hits
    public bool requireSecondHitRoll = false;

    //The percent chance that the effect on hit will proc
    [Range(0, 1)]
    public float effectChance = 0.2f;

    //The radius of effect from the hit target
    [Range(0, 10)]
    public int effectRadius = 0;

    //Determines if this damages specific enemy types
    public RaceTypes.Races effectedRace = RaceTypes.Races.None;
    public RaceTypes.Subtypes effectedType = RaceTypes.Subtypes.None;
    //Determines if the effected type is the only type hit or if it's the only type ignored
    public bool hitEffectedType = true;

    //Enum that determines who is effected
    public enum EffectedTargets
    {
        Defender,//The person being hit by the attack
        Attacker,//The person making the attack
        EnemiesOnly,//Hits all enemies in the radius and ignores allies
        EnemiesExceptDefender,//Hits all enemies, but doesn't include the defender
        AlliesOnly,//Hits all allies in the radius and ignores enemies
        AlliesExceptAttacker,//Hits all allies, but doesn't include the attacker
        Everyone//Hits every ally and enemy in the radius
    };

    //The type of targets that are effected
    public EffectedTargets effectedTargets = EffectedTargets.Defender;
}
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


public enum DamageStatus { None, Fire, Corrosive, Cryo, Electric, Explosive, Plasma, AP }
public class StatusEffectHandler : MonoBehaviour
{
    private Dictionary<DamageStatus, Coroutine> activeEffects = new();

    public void ApplyStatusEffect(StatusEffectData effectData, IDamage dmgTarget)
    {
        DamageStatus type = effectData.statusType;

        if (type == DamageStatus.None)
        {
            Debug.Log($"Skipping status {type}");
            return;
        }

        // Prevent duplicate effect of same type
        if (activeEffects.ContainsKey(type))
        {
            Debug.Log($"Refreshing status {type}");

            if (activeEffects[type] != null)
            {
                StopCoroutine(activeEffects[type]);
                activeEffects.Remove(type);
            }
            
        }

        Coroutine effectRoutine = StartCoroutine(RunEffect(effectData, dmgTarget));
        activeEffects[type] = effectRoutine;
    }

    private IEnumerator RunEffect(StatusEffectData data, IDamage target)
    {
        float timeElapsed = 0f;

        switch (data.statusType)
        {
            case DamageStatus.Fire:
                Debug.LogWarning("Fire status effect applied");
                break;

            case DamageStatus.Corrosive:
                Debug.LogWarning("Corrosive status effect applied");
                break;

            case DamageStatus.Cryo:

                Debug.LogWarning("Cryo status effect applied");
                target.slowDown(data.slowDownMagnitude, data.statusDuration);
                break;

            case DamageStatus.Electric:

                Debug.LogWarning("Electric status effect applied");
                target.slowDown(data.slowDownMagnitude, data.statusDuration);
                break;

            case DamageStatus.Explosive:

                Debug.LogWarning("Explosive status effect applied");
                
                break;

            default:
                Debug.LogWarning($"Unhandled status effect: {data.statusType}");
                yield break;
        }

        while (timeElapsed < data.statusDuration)
        {
            target.takeDamage(data.statusDamage, data);
            yield return new WaitForSeconds(data.statusTickRate);
            timeElapsed += data.statusTickRate;
        }

        activeEffects.Remove(data.statusType);
    }
}

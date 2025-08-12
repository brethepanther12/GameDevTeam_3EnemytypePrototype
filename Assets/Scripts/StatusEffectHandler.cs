using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


public enum DamageStatus { None, Fire, Corrosive, Cryo, Electric, Explosive, Plasma, AP }
public class StatusEffectHandler : MonoBehaviour
{
    public StatusEffectData currentEffectData;
    public List<StatusEffectData> currentStatuses = new List<StatusEffectData>();

    public void ApplyStatusEffect(StatusEffectData effectData, IDamage dmgTarget)
    {
        currentEffectData = effectData;
        DamageStatus type = effectData.statusType;

        if (type == DamageStatus.None || currentStatuses.Contains(effectData))
        {
            Debug.Log($"Skipping status {type}, already active or none");
            return;
        }

        currentStatuses.Add(effectData);

        switch (type)
        {

            case DamageStatus.None:

                Debug.LogWarning($"No status effect applied");
                break;

            case DamageStatus.Fire:

                Debug.LogWarning($"Fire status effect applied");
                StartCoroutine(ApplyBurnDamage(dmgTarget));
                break;

            case DamageStatus.Corrosive:

                Debug.LogWarning($"Corrosive status effect applied");
                StartCoroutine(ApplyCorrosiveDamage(dmgTarget));
                break;

            default:

                break;
        }
    }

    public IEnumerator ApplyBurnDamage(IDamage target)
    {

        float timeElapsed = 0f;
        StatusEffectData burnData = currentEffectData;

        while (timeElapsed < burnData.statusDuration)
        {
            
            target.takeDamage(burnData.statusDamage);

            yield return new WaitForSeconds(burnData.statusTickRate);

            timeElapsed += burnData.statusTickRate;
        }

        currentStatuses.Remove(currentEffectData);

    }

    public IEnumerator ApplyCorrosiveDamage(IDamage target)
    {

        float timeElapsed = 0f;
        StatusEffectData burnData = currentEffectData;

        while (timeElapsed < burnData.statusDuration)
        {

            target.takeDamage(burnData.statusDamage);

            yield return new WaitForSeconds(burnData.statusTickRate);

            timeElapsed += burnData.statusTickRate;
        }

        currentStatuses.Remove(currentEffectData);

    }
}

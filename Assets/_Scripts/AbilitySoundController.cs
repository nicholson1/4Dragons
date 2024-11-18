using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySoundController : MonoBehaviour
{
    public static AbilitySoundController i;
    
    [SerializeField] private AudioClip stabSFX;
    [SerializeField] private float stabVol;

    [SerializeField] private AudioClip gougeSFX;
    [SerializeField] private float gougeVol;

    [SerializeField] private AudioClip sliceSFX;
    [SerializeField] private float sliceVol;

    [SerializeField] private AudioClip bashSFX;
    [SerializeField] private float bashVol;

    [SerializeField] private AudioClip blockSFX;
    [SerializeField] private float blockVol;

    [SerializeField] private AudioClip barricadeSFX;
    [SerializeField] private float barricadeVol;

    [SerializeField] private AudioClip slashSFX;
    [SerializeField] private float slashVol;

    [SerializeField] private AudioClip whirlwindSFX;
    [SerializeField] private float whirlwindVol;

    [SerializeField] private AudioClip composedStrikeSFX;
    [SerializeField] private float composedStrikeVol;

    [SerializeField] private AudioClip hackSFX;
    [SerializeField] private float hackVol;

    [SerializeField] private AudioClip rendSFX;
    [SerializeField] private float rendVol;

    [SerializeField] private AudioClip severSFX;
    [SerializeField] private float severVol;

    [SerializeField] private AudioClip smashSFX;
    [SerializeField] private float smashVol;

    [SerializeField] private AudioClip pummelSFX;
    [SerializeField] private float pummelVol;

    [SerializeField] private AudioClip momentousSwingSFX;
    [SerializeField] private float momentousSwingVol;

    [SerializeField] private AudioClip rejuvenationSFX;
    [SerializeField] private float rejuvenationVol;

    [SerializeField] private AudioClip nourishSFX;
    [SerializeField] private float nourishVol;

    [SerializeField] private AudioClip thornsSFX;
    [SerializeField] private float thornsVol;
    
    [SerializeField] private AudioClip WrathImpactSFX;
    [SerializeField] private float WrathImpactVol;
    
    [SerializeField] private AudioClip wrathSFX;
    [SerializeField] private float wrathVol;

    [SerializeField] private AudioClip meditateSFX;
    [SerializeField] private float meditateVol;

    [SerializeField] private AudioClip smeltSFX;
    [SerializeField] private float smeltVol;

    [SerializeField] private AudioClip incinerateSFX;
    [SerializeField] private float incinerateVol;

    [SerializeField] private AudioClip fireballSFX;
    [SerializeField] private float fireballVol;
    
    [SerializeField] private AudioClip fireballImpactSFX;
    [SerializeField] private float fireballImpactVol;

    [SerializeField] private AudioClip pyroblastSFX;
    [SerializeField] private float pyroblastVol;

    [SerializeField] private AudioClip innerFireSFX;
    [SerializeField] private float innerFireVol;

    [SerializeField] private AudioClip iceBarrierSFX;
    [SerializeField] private float iceBarrierVol;

    [SerializeField] private AudioClip shatterSFX;
    [SerializeField] private float shatterVol;

    [SerializeField] private AudioClip blizzardSFX;
    [SerializeField] private float blizzardVol;

    [SerializeField] private AudioClip frostboltSFX;
    [SerializeField] private float frostboltVol;
    
    [SerializeField] private AudioClip frostboltImpactSFX;
    [SerializeField] private float frostboltImpactVol;

    [SerializeField] private AudioClip windchillSFX;
    [SerializeField] private float windchillVol;

    [SerializeField] private AudioClip lifeLeechSFX;
    [SerializeField] private float lifeLeechVol;

    [SerializeField] private AudioClip essenceDrainSFX;
    [SerializeField] private float essenceDrainVol;

    [SerializeField] private AudioClip crimsonVowSFX;
    [SerializeField] private float crimsonVowVol;

    [SerializeField] private AudioClip ritualSFX;
    [SerializeField] private float ritualVol;

    [SerializeField] private AudioClip purgeSFX;
    [SerializeField] private float purgeVol;

    [SerializeField] private AudioClip lifeTapSFX;
    [SerializeField] private float lifeTapVol;
    
    [SerializeField] private AudioClip shadowBoltSFX;
    [SerializeField] private float shadowBoltVol;
    
    [SerializeField] private AudioClip shadowBoltImpactSFX;
    [SerializeField] private float shadowBoltImpactVol;

    [SerializeField] private AudioClip curseOfSufferingSFX;
    [SerializeField] private float curseOfSufferingVol;

    [SerializeField] private AudioClip devilsDanceSFX;
    [SerializeField] private float devilsDanceVol;

    [SerializeField] private AudioClip curseOfWeaknessSFX;
    [SerializeField] private float curseOfWeaknessVol;
    
    [SerializeField] private AudioClip lacerateSFX;
    [SerializeField] private float lacerateVol;

    [SerializeField] private AudioClip burnSFX;
    [SerializeField] private float burnVol;

    [SerializeField] private AudioClip woundedSFX;
    [SerializeField] private float woundedVol;

    [SerializeField] private AudioClip weakenedSFX;
    [SerializeField] private float weakenedVol;

    [SerializeField] private AudioClip chilledSFX;
    [SerializeField] private float chilledVol;

    [SerializeField] private AudioClip exposedSFX;
    [SerializeField] private float exposedVol;

    [SerializeField] private AudioClip rejuvinateSFX;
    [SerializeField] private float rejuvinateVol;

    [SerializeField] private AudioClip empoweredSFX;
    [SerializeField] private float empoweredVol;

    [SerializeField] private AudioClip preparedSFX;
    [SerializeField] private float preparedVol;
    private void Awake()
    {
        if (i != null && i != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            i = this;
        }
    }

    public void PlayStabSound(float delay)
    {
        if (stabSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(stabSFX, delay, stabVol, 1, .05f);
    }

    public void PlayGougeSound(float delay)
    {
        if (gougeSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(gougeSFX, delay, gougeVol, 1, .05f);
    }

    public void PlaySliceSound(float delay)
    {
        if (sliceSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(sliceSFX, delay, sliceVol, 1, .05f);
    }

    public void PlayBashSound(float delay)
    {
        if (bashSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(bashSFX, delay, bashVol, 1, .05f);
    }

    public void PlayBlockSound(float delay)
    {
        if (blockSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(blockSFX, delay, blockVol, 1, .05f);
    }

    public void PlayBarricadeSound(float delay)
    {
        if (barricadeSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(barricadeSFX, delay, barricadeVol, 1, .05f);
    }

    public void PlaySlashSound(float delay)
    {
        if (slashSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(slashSFX, delay, slashVol, 1, .05f);
    }

    public void PlayWhirlwindSound(float delay)
    {
        if (whirlwindSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(whirlwindSFX, delay, whirlwindVol, 1, .05f);
    }

    public void PlayComposedStrikeSound(float delay)
    {
        if (composedStrikeSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(composedStrikeSFX, delay, composedStrikeVol, 1, .05f);
    }

    public void PlayHackSound(float delay)
    {
        if (hackSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(hackSFX, delay, hackVol, 1, .05f);
    }

    public void PlayRendSound(float delay)
    {
        if (rendSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(rendSFX, delay, rendVol, 1, .05f);
    }

    public void PlaySeverSound(float delay)
    {
        if (severSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(severSFX, delay, severVol, 1, .05f);
    }

    public void PlaySmashSound(float delay)
    {
        if (smashSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(smashSFX, delay, smashVol, 1, .05f);
    }

    public void PlayPummelSound(float delay)
    {
        if (pummelSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(pummelSFX, delay, pummelVol, 1, .05f);
    }

    public void PlayMomentousSwingSound(float delay)
    {
        if (momentousSwingSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(momentousSwingSFX, delay, momentousSwingVol, 1, .05f);
    }

    public void PlayRejuvenationSound(float delay)
    {
        if (rejuvenationSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(rejuvenationSFX, delay, rejuvenationVol, 1, .05f);
    }

    public void PlayNourishSound(float delay)
    {
        if (nourishSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(nourishSFX, delay, nourishVol, 1, .05f);
    }

    public void PlayThornsSound(float delay)
    {
        if (thornsSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(thornsSFX, delay, thornsVol, 1, .05f);
    }

    public void PlayWrathImpactSound(float delay)
    {
        if (WrathImpactSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(WrathImpactSFX, delay, WrathImpactVol, 1, .05f);
    }

    public void PlayWrathSound(float delay)
    {
        if (wrathSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(wrathSFX, delay, wrathVol, 1, .05f);
    }

    public void PlayMeditateSound(float delay)
    {
        if (meditateSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(meditateSFX, delay, meditateVol, 1, .05f);
    }

    public void PlaySmeltSound(float delay)
    {
        if (smeltSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(smeltSFX, delay, smeltVol, 1, .05f);
    }

    public void PlayIncinerateSound(float delay)
    {
        if (incinerateSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(incinerateSFX, delay, incinerateVol, 1, .05f);
    }

    public void PlayFireballSound(float delay)
    {
        if (fireballSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(fireballSFX, delay, fireballVol, 1, .05f);
    }
    public void PlayFireballImpactSound(float delay)
    {
        if (fireballImpactSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(fireballImpactSFX, delay, fireballImpactVol, 1, .05f);
    }

    public void PlayPyroblastSound(float delay)
    {
        if (pyroblastSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(pyroblastSFX, delay, pyroblastVol, 1, .05f);
    }

    public void PlayInnerFireSound(float delay)
    {
        if (innerFireSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(innerFireSFX, delay, innerFireVol, 1, .05f);
    }

    public void PlayIceBarrierSound(float delay)
    {
        if (iceBarrierSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(iceBarrierSFX, delay, iceBarrierVol, 1, .05f);
    }

    public void PlayShatterSound(float delay)
    {
        if (shatterSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(shatterSFX, delay, shatterVol, 1, .05f);
    }

    public void PlayBlizzardSound(float delay)
    {
        if (blizzardSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(blizzardSFX, delay, blizzardVol, 1, .05f);
    }

    public void PlayFrostboltSound(float delay)
    {
        if (frostboltSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(frostboltSFX, delay, frostboltVol, 1, .05f);
    }
    public void PlayFrostboltImpactSound(float delay)
    {
        if (frostboltImpactSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(frostboltImpactSFX, delay, frostboltImpactVol, 1, .05f);
    }

    public void PlayWindchillSound(float delay)
    {
        if (windchillSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(windchillSFX, delay, windchillVol, 1, .05f);
    }

    public void PlayLifeLeechSound(float delay)
    {
        if (lifeLeechSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(lifeLeechSFX, delay, lifeLeechVol, 1, .05f);
    }

    public void PlayEssenceDrainSound(float delay)
    {
        if (essenceDrainSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(essenceDrainSFX, delay, essenceDrainVol, 1, .05f);
    }

    public void PlayCrimsonVowSound(float delay)
    {
        if (crimsonVowSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(crimsonVowSFX, delay, crimsonVowVol, 1, .05f);
    }

    public void PlayRitualSound(float delay)
    {
        if (ritualSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(ritualSFX, delay, ritualVol, 1, .05f);
    }

    public void PlayPurgeSound(float delay)
    {
        if (purgeSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(purgeSFX, delay, purgeVol, 1, .05f);
    }

    public void PlayLifeTapSound(float delay)
    {
        if (lifeTapSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(lifeTapSFX, delay, lifeTapVol, 1, .05f);
    }

    public void PlayCurseOfWeaknessSound(float delay)
    {
        if (curseOfWeaknessSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(curseOfWeaknessSFX, delay, curseOfWeaknessVol, 1, .05f);
    }

    public void PlayShadowBoltSound(float delay)
    {
        if (shadowBoltSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(shadowBoltSFX, delay, shadowBoltVol, 1, .05f);
    }
    public void PlayShadowBoltImpactSound(float delay)
    {
        if (shadowBoltImpactSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(shadowBoltImpactSFX, delay, shadowBoltImpactVol, 1, .05f);
    }

    public void PlayCurseOfSufferingSound(float delay)
    {
        if (curseOfSufferingSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(curseOfSufferingSFX, delay, curseOfSufferingVol, 1, .05f);
    }

    public void PlayDevilsDanceSound(float delay)
    {
        if (devilsDanceSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(devilsDanceSFX, delay, devilsDanceVol, 1, .05f);
    }

    public void PlayLacerateSound(float delay)
    {
        if (lacerateSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(lacerateSFX, delay, lacerateVol, 1, .05f);
    }

    public void PlayBurnSound(float delay)
    {
        if (burnSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(burnSFX, delay, burnVol, 1, .05f);
    }

    public void PlayWoundedSound(float delay)
    {
        if (woundedSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(woundedSFX, delay, woundedVol, 1, .05f);
    }

    public void PlayWeakenedSound(float delay)
    {
        if (weakenedSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(weakenedSFX, delay, weakenedVol, 1, .05f);
    }

    public void PlayChilledSound(float delay)
    {
        if (chilledSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(chilledSFX, delay, chilledVol, 1, .05f);
    }

    public void PlayExposedSound(float delay)
    {
        if (exposedSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(exposedSFX, delay, exposedVol, 1, .05f);
    }

    public void PlayRejuvinateSound(float delay)
    {
        if (rejuvinateSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(rejuvinateSFX, delay, rejuvinateVol, 1, .05f);
    }

    public void PlayEmpoweredSound(float delay)
    {
        if (empoweredSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(empoweredSFX, delay, empoweredVol, 1, .05f);
    }

    public void PlayPreparedSound(float delay)
    {
        if (preparedSFX == null)
            return;
        SoundManager.Instance.Play2DSFXOnDelay(preparedSFX, delay, preparedVol, 1, .05f);
    }
}

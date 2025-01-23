using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager _instance;
    //prefabs
    [SerializeField] private ParticleControl NourishPrefab;
    [SerializeField] private ParticleControl RejuvPrefab;
    [SerializeField] private ParticleControl EmpowerPrefab;
    [SerializeField] private ParticleControl WeakenPrefab;
    [SerializeField] private ParticleControl BlizzardPrefab;
    [SerializeField] private ParticleControl ShatterPrefab;
    [SerializeField] private ParticleControl PreparedPrefab;
    [SerializeField] private ParticleControl ImmortalPrefab;
    [SerializeField] private ParticleControl InvulnerablePrefab;
    [SerializeField] private ParticleControl PurgePrefab;
    [SerializeField] private ParticleControl BleedPrefab;
    [SerializeField] private ParticleControl BurnPrefab;
    [SerializeField] private ParticleControl IceBlockPrefab;
    [SerializeField] private ParticleControl ExposedPrefab;
    [SerializeField] private ParticleControl ChillPrefab;
    [SerializeField] private ParticleControl WoundedPrefab;
    [SerializeField] private ParticleControl MeteorStrikePrefab;
    [SerializeField] private ParticleControl ThornsPrefab;
    [SerializeField] private ParticleControl LifeTapPrefab;
    [SerializeField] private ParticleControl SmeltPrefab;
    [SerializeField] private ParticleControl FireballPrefab;
    [SerializeField] private ParticleControl ShadowBoltPrefab;
    [SerializeField] private ParticleControl FrostboltPrefab;
    [SerializeField] private ParticleControl WrathPrefab;
    [SerializeField] private ParticleControl LifeLeechPrefab;
    [SerializeField] private ParticleControl BloodLifeTapPrefab;

    
    





    
    //list of pooled particles
    private List<ParticleControl> PooledParticles = new List<ParticleControl>();
    
    //for aura spells or static particles that dont move
    // spawn from pool at target location
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void SpawnParticle(CombatEntity caster, CombatEntity target, SpellTypes spell, float delayTimeOverRide = -1)
    {
        switch (spell)
        {
            case SpellTypes.Shield1:
                AbilitySoundController.i.PlayBashSound(.1f);
                break;
            case SpellTypes.Shield2:
                AbilitySoundController.i.PlayBlockSound(0);
                break;
            case SpellTypes.Shield3:
                SpawnStaticParticle(caster, PreparedPrefab, 0f, 5);
                AbilitySoundController.i.PlayBarricadeSound(0);
                AbilitySoundController.i.PlayPreparedSound(0);

                break;
            case SpellTypes.Nature2:
                SpawnStaticParticle(caster, NourishPrefab, .1f, 5);
                AbilitySoundController.i.PlayNourishSound(.1f);
                break;
            case SpellTypes.Nature1:
                SpawnStaticParticle(caster, RejuvPrefab, .1f, 5);
                AbilitySoundController.i.PlayRejuvenationSound(.1f);
                AbilitySoundController.i.PlayRejuvinateSound(.1f);
                break;
            case SpellTypes.Nature3:
                SpawnStaticParticle(caster, ThornsPrefab, .1f, 5);
                AbilitySoundController.i.PlayThornsSound(.1f);
                break;
            case SpellTypes.Nature4:
                SpawnMovingParticle(caster,target, WrathPrefab, .2f, 5);
                AbilitySoundController.i.PlayWrathSound(.2f);
                AbilitySoundController.i.PlayWrathImpactSound(.6f);

                break;
            case SpellTypes.Nature5:
                SpawnStaticParticle(caster, PreparedPrefab, 0f, 5);
                AbilitySoundController.i.PlayMeditateSound(0f);
                AbilitySoundController.i.PlayPreparedSound(0f);
                break;
            case SpellTypes.Blood1:
                SpawnMovingParticle(target, caster, LifeLeechPrefab, .1f, 5, 6);
                AbilitySoundController.i.PlayLifeLeechSound(0f);
                break;
            case SpellTypes.Blood2:
                SpawnMovingParticle(target, caster, LifeLeechPrefab, .1f, 5, 6);
                SpawnMovingParticle(target, caster, LifeLeechPrefab, .1f, 5, 6);
                AbilitySoundController.i.PlayLifeLeechSound(0f);
                AbilitySoundController.i.PlayLifeLeechSound(.1f);
                AbilitySoundController.i.PlayLifeLeechSound(.2f);


                break;
            case SpellTypes.Blood3:
                SpawnStaticParticle(caster, InvulnerablePrefab, 0f, 5);
                SpawnStaticParticle(caster, BloodLifeTapPrefab, .7f, 5);
                AbilitySoundController.i.PlayCrimsonVowSound(0f);
                AbilitySoundController.i.PlayLifeTapSound(.7f);
                break;
            case SpellTypes.Blood4:
                SpawnStaticParticle(caster, EmpowerPrefab, .1f, 5);
                AbilitySoundController.i.PlayEmpoweredSound(.3f);
                AbilitySoundController.i.PlayRitualSound(.1f);
                break;
            case SpellTypes.Blood5:
                SpawnStaticParticle(target, PurgePrefab, .2f, 5);
                AbilitySoundController.i.PlayPurgeSound(.2f);
                break;
            case SpellTypes.Hammer1:
                AbilitySoundController.i.PlaySmashSound(.1f);
                break;
            case SpellTypes.Hammer2:
                SpawnStaticParticle(target, ExposedPrefab, .5f, 5);
                AbilitySoundController.i.PlayPummelSound(.4f);
                AbilitySoundController.i.PlayExposedSound(.5f);
                break;
            case SpellTypes.Hammer3:
                SpawnStaticParticle(caster, EmpowerPrefab, .1f, 5);
                AbilitySoundController.i.PlayMomentousSwingSound(.5f);
                AbilitySoundController.i.PlayEmpoweredSound(.6f);
                break;
            case SpellTypes.Axe1:
                AbilitySoundController.i.PlayHackSound(.1f);
                break;
            case SpellTypes.Axe2:
                if (delayTimeOverRide == -1)
                {
                    SpawnStaticParticle(target, BleedPrefab, .4f, 5);
                    AbilitySoundController.i.PlayRendSound(.3f);
                    AbilitySoundController.i.PlayLacerateSound(.4f);
                }
                else
                {
                    SpawnStaticParticle(target, BleedPrefab, delayTimeOverRide, 5);
                    AbilitySoundController.i.PlayRendSound(.3f);
                    AbilitySoundController.i.PlayLacerateSound(delayTimeOverRide);
                }
                break;
            case SpellTypes.Axe3:
                SpawnStaticParticle(target, PurgePrefab, .5f, 5);
                AbilitySoundController.i.PlaySeverSound(.5f);
                AbilitySoundController.i.PlayPurgeSound(.5f);
                break;
            case SpellTypes.Fire1:
                SpawnStaticParticle(target, ExposedPrefab, .6f, 5);
                SpawnStaticParticle(target, SmeltPrefab, .25f, 5);
                AbilitySoundController.i.PlaySmeltSound(.25f);
                AbilitySoundController.i.PlayExposedSound(.6f);
                break;
            case SpellTypes.Fire2:
                if (delayTimeOverRide == -1)
                {
                    SpawnStaticParticle(target, BurnPrefab, .6f, 5);
                    AbilitySoundController.i.PlayBurnSound(.6f);
                    AbilitySoundController.i.PlayIncinerateSound(.4f);
                }
                else
                {
                    SpawnStaticParticle(target, BurnPrefab, delayTimeOverRide, 5);
                    AbilitySoundController.i.PlayBurnSound(delayTimeOverRide);
                    AbilitySoundController.i.PlayIncinerateSound(.4f);
                }
                break;
            case SpellTypes.Fire3:
                SpawnMovingParticle(caster,target, FireballPrefab, .2f, 5);
                AbilitySoundController.i.PlayFireballSound(.2f);
                AbilitySoundController.i.PlayFrostboltImpactSound(.6f);

                break;
            case SpellTypes.Fire4:
                SpawnStaticParticle(target, MeteorStrikePrefab, 0f, 5);
                AbilitySoundController.i.PlayPyroblastSound(0f);
                break;
            case SpellTypes.Fire5:
                SpawnStaticParticle(caster, EmpowerPrefab, .1f, 5);
                SpawnStaticParticle(caster, PreparedPrefab, 0f, 5);
                AbilitySoundController.i.PlayInnerFireSound(0f);
                AbilitySoundController.i.PlayPreparedSound(0f);
                AbilitySoundController.i.PlayEmpoweredSound(.3f);
                break;
            case SpellTypes.Shadow1:
                SpawnStaticParticle(caster, LifeTapPrefab, .7f, 5);
                AbilitySoundController.i.PlayLifeTapSound(.7f);
                AbilitySoundController.i.PlayShadowBoltImpactSound(0f);
                break;
            case SpellTypes.Shadow2:
                SpawnStaticParticle(target, WeakenPrefab, .1f, 5);
                AbilitySoundController.i.PlayWeakenedSound(.1f);
                AbilitySoundController.i.PlayCurseOfWeaknessSound(0f);
                break;
            case SpellTypes.Shadow3:
                SpawnMovingParticle(caster,target, ShadowBoltPrefab, .2f, 5);
                AbilitySoundController.i.PlayShadowBoltSound(.2f);
                AbilitySoundController.i.PlayShadowBoltImpactSound(.6f);

                break;
            case SpellTypes.Shadow4:
                SpawnStaticParticle(target, WoundedPrefab, .2f, 5);
                AbilitySoundController.i.PlayWoundedSound(.2f);
                AbilitySoundController.i.PlayCurseOfSufferingSound(.1f);
                break;
            case SpellTypes.Shadow5:
                SpawnStaticParticle(caster, ImmortalPrefab, 0f, 5);
                SpawnStaticParticle(caster, LifeTapPrefab, .7f, 5);
                AbilitySoundController.i.PlayDevilsDanceSound(0f);
                AbilitySoundController.i.PlayLifeTapSound(.7f);
                break;
            case SpellTypes.Dagger1:
                AbilitySoundController.i.PlayStabSound(0f);
                break;
            case SpellTypes.Dagger2:
                SpawnStaticParticle(target, WoundedPrefab, .2f, 5);
                AbilitySoundController.i.PlayGougeSound(0f);
                AbilitySoundController.i.PlayWoundedSound(.2f);
                break;
            case SpellTypes.Dagger3:
                SpawnStaticParticle(target, WeakenPrefab, .1f, 5);
                AbilitySoundController.i.PlaySliceSound(0);
                AbilitySoundController.i.PlayWeakenedSound(0);
                break;
            case SpellTypes.Ice5:
                SpawnStaticParticle(target, ExposedPrefab, .1f, 5);
                SpawnStaticParticle(target, ChillPrefab, .1f, 5);
                AbilitySoundController.i.PlayWindchillSound(0);
                AbilitySoundController.i.PlayExposedSound(.1f);
                AbilitySoundController.i.PlayChilledSound(.1f);
                break;
            case SpellTypes.Ice4:
                SpawnMovingParticle(caster,target, FrostboltPrefab, .2f, 5);
                SpawnStaticParticle(target, ChillPrefab, .75f, 5);
                AbilitySoundController.i.PlayFrostboltSound(.2f);
                AbilitySoundController.i.PlayChilledSound(.75f);
                AbilitySoundController.i.PlayFrostboltImpactSound(.6f);

                break;
            case SpellTypes.Ice3:
                SpawnStaticParticle(target, BlizzardPrefab, .6f, 5);
                SpawnStaticParticle(target, ChillPrefab, 1f, 5);
                AbilitySoundController.i.PlayChilledSound(1f);
                AbilitySoundController.i.PlayBlizzardSound(.3f);
                break;
            case SpellTypes.Ice2:
                SpawnStaticParticle(caster, ShatterPrefab, 0, 5);
                AbilitySoundController.i.PlayShatterSound(0f);
                break;
            case SpellTypes.Ice1:
                SpawnStaticParticle(caster, IceBlockPrefab, 0, 5);
                AbilitySoundController.i.PlayIceBarrierSound(0f);
                break;
            case SpellTypes.Sword1:
                AbilitySoundController.i.PlaySlashSound(0f);
                break;
            case SpellTypes.Sword2:
                AbilitySoundController.i.PlayWhirlwindSound(0f);
                break;
            case SpellTypes.Sword3:
                AbilitySoundController.i.PlaySlashSound(0f);
                AbilitySoundController.i.PlayBlockSound(.25f);
                break;
        }
        
    }
    

    private void SpawnStaticParticle(CombatEntity target, ParticleControl prefab, float delayTime, float duration)
    {
        ParticleControl pc = GetEffectPrefab(prefab.particleType);
        pc.Initialize(target.transform, delayTime, duration);
        
    }
    private void SpawnMovingParticle(CombatEntity caster, CombatEntity target, ParticleControl prefab, float delayTime, float duration, float moveSpeed = 8)
    {
        ParticleControl pc = GetEffectPrefab(prefab.particleType);
        pc.Initialize(caster.transform, target.transform, delayTime, duration, moveSpeed);
    }
    

    private ParticleControl GetEffectPrefab(ParticleControl.ParticleType type)
    {
        // check the pool
        foreach (var pc in PooledParticles)
        {
            if (pc.particleType == type)
            {
                pc.gameObject.SetActive(true);
                PooledParticles.Remove(pc);
                return pc;
            }
        }

        switch (type)
        {
            case ParticleControl.ParticleType.NatureCircle:
            {
                ParticleControl pc = Instantiate(NourishPrefab,this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.RejuvHealing:
            {
                ParticleControl pc = Instantiate(RejuvPrefab,this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Empower:
            {
                ParticleControl pc = Instantiate(EmpowerPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Weaken:
            {
                ParticleControl pc = Instantiate(WeakenPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Blizzard:
            {
                ParticleControl pc = Instantiate(BlizzardPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Shatter:
            {
                ParticleControl pc = Instantiate(ShatterPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Meditate:
            {
                ParticleControl pc = Instantiate(PreparedPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Immortal:
            {
                ParticleControl pc = Instantiate(ImmortalPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Invulnerable:
            {
                ParticleControl pc = Instantiate(InvulnerablePrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Purge:
            {
                ParticleControl pc = Instantiate(PurgePrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Bleed:
            {
                ParticleControl pc = Instantiate(BleedPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Burn:
            {
                ParticleControl pc = Instantiate(BurnPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.IceBlock:
            {
                ParticleControl pc = Instantiate(IceBlockPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Exposed:
            {
                ParticleControl pc = Instantiate(ExposedPrefab, this.transform);
                return pc;
            }case ParticleControl.ParticleType.Chill:
            {
                ParticleControl pc = Instantiate(ChillPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Wounded:
            {
                ParticleControl pc = Instantiate(WoundedPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.MeteorStrike:
            {
                ParticleControl pc = Instantiate(MeteorStrikePrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Thorns:
            {
                ParticleControl pc = Instantiate(ThornsPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Smelt:
            {
                ParticleControl pc = Instantiate(SmeltPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.LifeTap:
            {
                ParticleControl pc = Instantiate(LifeTapPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.LifeLeech:
            {
                ParticleControl pc = Instantiate(LifeLeechPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.FrostBolt:
            {
                ParticleControl pc = Instantiate(FrostboltPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.FireBall:
            {
                ParticleControl pc = Instantiate(FireballPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.Wrath:
            {
                ParticleControl pc = Instantiate(WrathPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.ShadowBolt:
            {
                ParticleControl pc = Instantiate(ShadowBoltPrefab, this.transform);
                return pc;
            }
            case ParticleControl.ParticleType.BloodLifeTap:
            {
                ParticleControl pc = Instantiate(BloodLifeTapPrefab, this.transform);
                return pc;
            }
        }

        Debug.LogWarning("No Particle Control Found");
        return null;

    }

    public void PoolParticle(ParticleControl pc)
    {
        PooledParticles.Add(pc);
        pc.gameObject.SetActive(false);
        
    }

}

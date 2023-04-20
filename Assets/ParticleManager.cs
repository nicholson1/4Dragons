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

    public void SpawnParticle(CombatEntity caster, CombatEntity target, Weapon.SpellTypes spell)
    {
        switch (spell)
        {
            case Weapon.SpellTypes.Shield3:
                SpawnStaticParticle(caster, PreparedPrefab, 0f, 5);
                break;
            case Weapon.SpellTypes.Nature2:
                SpawnStaticParticle(caster, NourishPrefab, .1f, 5);
                break;
            case Weapon.SpellTypes.Nature1:
                SpawnStaticParticle(caster, RejuvPrefab, .1f, 5);
                break;
            case Weapon.SpellTypes.Nature5:
                SpawnStaticParticle(caster, PreparedPrefab, 0f, 5);
                break;
            case Weapon.SpellTypes.Blood3:
                SpawnStaticParticle(caster, InvulnerablePrefab, 0f, 5);
                break;
            case Weapon.SpellTypes.Blood4:
                SpawnStaticParticle(caster, EmpowerPrefab, .1f, 5);
                break;
            case Weapon.SpellTypes.Blood5:
                SpawnStaticParticle(target, PurgePrefab, .2f, 5);
                break;
            case Weapon.SpellTypes.Hammer3:
                SpawnStaticParticle(caster, EmpowerPrefab, .1f, 5);
                break;
            case Weapon.SpellTypes.Axe2:
                SpawnStaticParticle(target, BleedPrefab, .4f, 5);
                break;
            case Weapon.SpellTypes.Axe3:
                SpawnStaticParticle(target, PurgePrefab, .5f, 5);
                break;
            case Weapon.SpellTypes.Fire5:
                SpawnStaticParticle(caster, EmpowerPrefab, .1f, 5);
                SpawnStaticParticle(caster, PreparedPrefab, 0f, 5);
                break;
            case Weapon.SpellTypes.Shadow2:
                SpawnStaticParticle(target, WeakenPrefab, 0f, 5);
                break;
            case Weapon.SpellTypes.Shadow5:
                SpawnStaticParticle(caster, ImmortalPrefab, 0f, 5);
                break;
            case Weapon.SpellTypes.Dagger3:
                SpawnStaticParticle(target, WeakenPrefab, .1f, 5);
                break;
            case Weapon.SpellTypes.Ice3:
                SpawnStaticParticle(target, BlizzardPrefab, .6f, 5);
                break;
            case Weapon.SpellTypes.Ice2:
                SpawnStaticParticle(caster, ShatterPrefab, 0, 5);
                break;
        }
        
    }

    private void SpawnStaticParticle(CombatEntity target, ParticleControl prefab, float delayTime, float duration)
    {
        ParticleControl pc = GetEffectPrefab(prefab.particleType);
        pc.Initialize(target.transform, delayTime, duration);
        
        
        
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

using Fusion;
using Projectiles;
using UnityEngine;
public class MonsterHealth : NetworkBehaviour
{
    [Header("Setup")]
    public Health healthRefer;
    public float DeathTime;

    [Header("References")]
    public GameObject VisualRoot;
    public GameObject DeathRoot;

    public bool IsAlive => healthRefer.IsAlive;
    public bool IsFinished => IsAlive == false && _deathCooldown.Expired(Runner);

   [Networked, HideInInspector, OnChangedRender(nameof(OnCurrentHealthChanged))]
    public float CurrentHealth { get; set; }

    [Networked]
    private TickTimer _deathCooldown { get; set; }

    /*   public bool TakeHit(float damage)
       {
           if (IsAlive == false)
               return false;

           CurrentHealth -= damage;

           if (IsAlive == false)
           {
               //Entity died,let's start death cooldown
               CurrentHealth = 0;
               _deathCooldown = TickTimer.CreateFromSeconds(Runner, DeathTime);
           }

           return true;
       }*/
    public override void FixedUpdateNetwork()
    {
        if(healthRefer.IsSpawned)
            CurrentHealth = healthRefer.CurrentHealth;

       /* if (healthRefer.IsAlive == false)
        {
            CurrentHealth = 0;
            _deathCooldown = TickTimer.CreateFromSeconds(Runner, DeathTime);
        }*/
    }
    public void Revive()
    {
        if (healthRefer.IsSpawned)
            CurrentHealth = healthRefer.MaxHealth;
        _deathCooldown = default;
    }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            //Set initial health
            CurrentHealth = healthRefer.MaxHealth;
        }
    }

    public override void Render()
    {
        // Use interpolated value when checking if entity is alive in Render.
        // This will ensure that death effects are played AFTER the death was "confirmed"
        // on the server in case of mispredictions (e.g. lost fire input) and also helps
        // with showing player visual at the correct position right away after respawn
        // (= player won't be visible before KCC teleport that is interpolated as well).
        // var interpolator = new NetworkBehaviourBufferInterpolator(this);
        // bool isAlive = interpolator.Float(nameof(CurrentHealth)) > 0;
        if (healthRefer.IsSpawned)
        {
            bool isAlive = healthRefer.IsAlive;

            VisualRoot.SetActive(isAlive);
            DeathRoot.SetActive(isAlive == false);
        }
    }

    private void OnCurrentHealthChanged()
    {
        Debug.Log("MonsterHealth CurrentHealth Changed>>" + CurrentHealth);
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            _deathCooldown = TickTimer.CreateFromSeconds(Runner, DeathTime);

            return; // Just health reset
        }

      /*  if (HasInputAuthority == false && ScalingRoot != null)
        {
            // Show hit reaction by simple scale. Scaling root
            // scale is lerped back to one in the Player script.
            ScalingRoot.localScale = new Vector3(0.85f, 1.15f, 0.85f);
        }*/
    }
}

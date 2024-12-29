using Fusion;
using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HPEvent : UnityEngine.Events.UnityEvent<int, int> { }

public class Status : NetworkBehaviour
{
    [HideInInspector]
    public HPEvent onHPEvent = new HPEvent();

    [Header("Walk, Run Speed")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [Header("HP")]
    [SerializeField]
    public int maxHP = 100;
    [SerializeField]
    public float currenthp;

    [SerializeField]
    public int attackdamage;
    [SerializeField]
    public int defense;
    [SerializeField]
    public float attack_distance;//¡Æ?¡Æ???¡Æ?¢¬?.

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;

    public float CurrentHP { get; set; }
    public float MaxHP => maxHP;

    [SerializeField] public float enemyCurrentfill;
    [SerializeField] public float currentManaPoint;
    [SerializeField] public float currentFillMana;
    [SerializeField] public float MaxManaPoints;
    [SerializeField] public string enemyName;
    [SerializeField] public int enemyLevel;

    private void Update()
    {
        UseMana();
    }
    public void UseMana()
    {
        if (MaxManaPoints == 0)
        {
            currentFillMana = 0;
        }
        else
        {
            currentFillMana = currentManaPoint / MaxManaPoints;
        }
    }
    void OnUIChange()
    {
        enemyCurrentfill = currenthp / maxHP;
    }

    private void Awake()
    {
        CurrentHP = maxHP;
        currenthp = maxHP;
        currentManaPoint = MaxManaPoints;
        enemyCurrentfill = currenthp / maxHP;
        currentFillMana = currentManaPoint / MaxManaPoints;
    }
    private void OnEnable()
    {
        CurrentHP = maxHP;
        currenthp = maxHP;
        currentManaPoint = MaxManaPoints;
        enemyCurrentfill = currenthp / maxHP;
        currentFillMana = currentManaPoint / MaxManaPoints;

        Debug.Log("ENEMY(air,ground,melee) OnEnable");
    }
    private void OnDisable()
    {
        Debug.Log("ENEMY(air,ground,melee) OnDisable");
    }
    public bool DecreaseHP(float damage)
    {
        float previousHP = CurrentHP;

        CurrentHP = CurrentHP - damage > 0 ? CurrentHP - damage : 0;

        //onHPEvent.Invoke(previousHP, CurrentHP);

        currenthp = CurrentHP;

        OnUIChange();

        if (CurrentHP <= 0)
        {
            return true;
        }

        return false;
    }

    public void IncreaseHP(int hp)
    {
        float previousHP = CurrentHP;

        CurrentHP = CurrentHP + hp > maxHP ? maxHP : CurrentHP + hp;

        currenthp = CurrentHP;

        // onHPEvent.Invoke(previousHP, CurrentHP);
    }
}
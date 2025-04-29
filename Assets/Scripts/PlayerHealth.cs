using System.Collections;
using UnityEngine;
using System;
using Photon.Pun;

public class PlayerHealth : MonoBehaviour
{
    public GameObject regenVFX;
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Armor Settings")]
    public int armor = 0;
    public float damageReductionPercent = 0.1f;

    [Header("Invincibility Settings")]
    public bool isInvincible = false;
    public float invincibilityDuration = 1f;

    [Header("Health Regeneration")]
    public bool canRegenerate = true;
    public int regenAmount = 1;
    public float regenInterval = 2f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    
    private CharacterController characterController;
}
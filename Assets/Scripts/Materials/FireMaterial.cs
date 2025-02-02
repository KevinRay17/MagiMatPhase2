﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class FireMaterial : MaterialClass
{
    [Header("Attack")]
    public bool attackUseFaceDirection;
    
    public float attackDamage;
    public float attackOffset;
    public bool attackHitMultipleTargets;
    public Vector2 attackSize;
    public GameObject hurtBoxPrefab;
    public GameObject atkFxAnimation;
    
    [Header("Special")]
    public float specialDashDistance;
    public float specialDashTime; //how long the dash takes

    private Rigidbody2D playerRB;
    
    private void Start()
    {
         playerRB = PlayerManager.instance.gameObject.GetComponent<Rigidbody2D>();
    }

    public override void Attack(GameObject player)
    {
        Debug.Log("Fire Attack");
        
        //See NoneMaterial Attack() for comments
        
        int attackDirection;
        Vector2 hitBoxSize = attackSize;
        
        if (attackUseFaceDirection)
        {
            attackDirection = PlayerManager.instance.playerMovement.faceDirection;
        }
        else
        {
            Vector2 direction = PlayerManager.instance.playerActions.aimDirection;

            //flip player sprite with mouse position
            if (direction.x < 0)
            {
                PlayerManager.instance.playerMovement.spriteRenderer.flipX = false;
                //this is so that the animation doesn't play in the direction of the mouse when the character is moving
                if (!PlayerManager.instance.playerMovement.anim.GetBool("Moving"))
                    PlayerManager.instance.playerMovement.faceDirection = 4;
            }
            else
            {
                PlayerManager.instance.playerMovement.spriteRenderer.flipX = true;
                if (!PlayerManager.instance.playerMovement.anim.GetBool("Moving"))
                    PlayerManager.instance.playerMovement.faceDirection = 2;
            }

            float directionAngle = GlobalFunctions.Vector2DirectionToAngle(direction);
            //Debug.Log(directionAngle + " " + direction.x);
            

            //MAKING THIS ONE BIG HITBOX (around player)
            /*
            attackDirection = Mathf.RoundToInt(directionAngle / 90);
            attackDirection += 1;
            attackDirection = attackDirection == 5 ? 1 : attackDirection;
            */
        }
        /*
        if (attackDirection == 1 || attackDirection == 3)
        {
            hitBoxSize = new Vector2(hitBoxSize.y, hitBoxSize.x);
        }
        */

        GameObject fx = Instantiate(atkFxAnimation, player.transform.position, Quaternion.identity);
        fx.transform.parent = player.transform;
        SpriteRenderer fxsr = fx.GetComponent<SpriteRenderer>();
        fxsr.flipX = PlayerManager.instance.playerMovement.spriteRenderer.flipX;

        Vector2 spawnpos = player.transform.position;
        spawnpos.y = player.transform.position.y + 0.5f;
        GameObject hurtBox = Instantiate(hurtBoxPrefab, spawnpos, Quaternion.identity);
        hurtBox.transform.parent = player.transform;
        HurtBox hbScript = hurtBox.GetComponent<HurtBox>();

        hbScript._spriteRenderer.size = hitBoxSize;
        hbScript._boxCollider.size = hitBoxSize;
        //hurtBox.transform.localPosition = GlobalFunctions.FaceDirectionToVector2(attackDirection) * attackOffset;

        hbScript.damage = attackDamage;
        hbScript.hitMultipleTargets = attackHitMultipleTargets;
    }
    
    public override void Special(GameObject player)
    {
        Debug.Log("Fire Special");
        
       // StartCoroutine(DashWait(player));
        var clip = Resources.Load<AudioClip>("Sounds/FireSpecial");
        AudioManager.instance.PlaySound(clip, 0.8f);
        
        Vector2 direction = new Vector2(PlayerManager.instance.playerMovement.inputVector.x, -PlayerManager.instance.playerMovement.inputVector.y);
        //flip player sprite to reflect direction
        //Positive direction = facing right; negative direction = facing left
        if (direction.x < 0)
            PlayerManager.instance.playerMovement.spriteRenderer.flipX = false;
        else
            PlayerManager.instance.playerMovement.spriteRenderer.flipX = true;
        
        StartCoroutine(FireSpecial(player, direction));
    }

    /*IEnumerator DashWait(GameObject player)
    {
       
        //Freeze movement until dash
        
        playerRB.velocity = new Vector2(0,0);
        playerRB.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(.5f);
        playerRB.constraints = RigidbodyConstraints2D.None;
        playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        Vector2 direction = new Vector2(PlayerManager.instance.playerMovement.inputVector.x, -PlayerManager.instance.playerMovement.inputVector.y);
        //flip player sprite to reflect direction
        //Positive direction = facing right; negative direction = facing left
        if (direction.x < 0)
            PlayerManager.instance.playerMovement.spriteRenderer.flipX = false;
        else
            PlayerManager.instance.playerMovement.spriteRenderer.flipX = true;
        StartCoroutine(FireSpecial(player, direction));
    }
    */
    IEnumerator FireSpecial(GameObject player, Vector3 direction)
    {
      
        //make the player ignore enemy collisions
       // Physics2D.IgnoreLayerCollision(player.layer, LayerMask.NameToLayer("Enemies"), true);
        
        //remove existing velocity so there isn't a strange jump/drop at the end of the dash
        playerRB.velocity = Vector2.zero;

        //temporarily remove gravity, so there isn't jitter when flying in the air
        playerRB.gravityScale = 0;

        //disable player movement
        PlayerManager.instance.playerMovement.canMove = false;

        //using MovePosition so the momentum of the dash isn't carried over
        //split over a few increments to make the dash seem smoother and not so sudden
        int upper = Mathf.RoundToInt(specialDashTime / 0.01f);
        for (int i = 0; i < upper; i++)
        {
            playerRB.MovePosition(player.transform.position + (direction * specialDashDistance/upper));
            yield return new WaitForSeconds(0.01f);
        }
        
        //add slight impulse force at the end for a little bit of momentum
        playerRB.AddForce(direction * specialDashDistance, ForceMode2D.Impulse);

        //reset gravity, collisions, and movement
        playerRB.gravityScale = PlayerManager.instance.playerMovement.gravityScale;
        //Physics2D.IgnoreLayerCollision(player.layer, LayerMask.NameToLayer("Enemies"), false);
        PlayerManager.instance.playerMovement.canMove = true;

        PlayerManager.instance.playerMovement.anim.SetBool("Special", false);
        PlayerManager.instance.playerActions.RC.resetCooldown(PlayerManager.instance.playerActions.RC.specialIndex);
    }
}

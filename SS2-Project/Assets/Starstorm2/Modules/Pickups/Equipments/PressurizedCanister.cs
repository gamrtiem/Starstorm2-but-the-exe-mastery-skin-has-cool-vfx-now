﻿using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Equipments
{
    public sealed class PressurizedCanister : SS2Equipment, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acPressurizedCannister", SS2Bundle.Equipments);

        public override bool Execute(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddItemBehavior<Behavior>(1);

                return true;
            }
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }


        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
        public sealed class PressurizedCanisterBehavior : BaseBuffBehaviour

        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdCanJump;

            protected override void OnFirstStackGained()
            {
                CharacterBody.baseJumpCount++;
                CharacterBody.characterMotor.onHitGroundAuthority += RemoveBuff;
            }

            public void FixedUpdate()
            {
                if (CharacterBody.inputBank.jump.justPressed) // server doesnt see client inputs so the buff cant be removed by the server. not gonna bother. too lazy. nemmerc must release.
                {
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), CharacterBody.transform.position, CharacterBody.transform.rotation, false);
                    CharacterBody.RemoveBuff(SS2Content.Buffs.bdCanJump.buffIndex);
                }
            }

            private void RemoveBuff(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                CharacterBody.RemoveBuff(SS2Content.Buffs.bdCanJump.buffIndex);
            }

            protected override void OnAllStacksLost()
            {
                CharacterBody.characterMotor.onHitGroundAuthority -= RemoveBuff;
                CharacterBody.baseJumpCount--;
            }
        }


        //notice how this is not added by this.AddItemBehavior() but the Fire Action
        public sealed class Behavior : CharacterBody.ItemBehavior
        {
            private static float duration = 0.8f;
            private static float thrustForce = 90f;
            private static int effectSpawnTotal = 10;


            private float stopwatch;
            private int effectInterval;
            private CharacterMotor characterMotor;

            private void Start()
            {
                characterMotor = body.characterMotor;
                if (characterMotor.isGrounded)
                    EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/SmokescreenEffect"), body.footPosition, Quaternion.identity, true);
                if (NetworkServer.active)
                {
                    characterMotor.Motor.ForceUnground();
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), body.transform.position, body.transform.rotation, true);
                    body.AddBuff(SS2Content.Buffs.bdCanJump.buffIndex);
                    body.characterMotor.Jump(1.2f, 2.2f, false); //it would be so awesome 
                }
            }

            private void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;

                //Gets removed if you switch equipments
                if (stopwatch > duration || body.equipmentSlot.equipmentIndex != EquipmentCatalog.FindEquipmentIndex("PressurizedCanister"))
                    Destroy(this);
            }
        }
    }
}

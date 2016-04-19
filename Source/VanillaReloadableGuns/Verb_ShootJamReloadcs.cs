using RimWorld;
using System;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaReloadableGuns
{
    public class Verb_ShootJamReload : Verb_LaunchProjectile
    {
        public bool isJammed = false;
        public SoundDef jamSound = SoundDef.Named("Misfire");
        private bool done;
        private CompReloader compAmmo;

        protected override int ShotsPerBurst
        {
            get
            {
                return this.verbProps.burstShotCount;
            }
        }

        protected override bool TryCastShot()
        {
            if (!this.done)
            {
                this.compAmmo = this.ownerEquipment.GetComp<CompReloader>();
                this.done = true;
            }
            bool result;
            if (this.compAmmo == null)
            {
                Log.ErrorOnce("No compAmmo found!", 12423);
                result = base.TryCastShot();
            }
            else
            {
                if (this.compAmmo.needReload || this.compAmmo.count <= 0)
                {
                    this.compAmmo.StartReload();
                    result = false;
                }
                else
                {
                    if (!base.TryCastShot())
                    {
                        result = false;
                    }
                    else
                    {
                        this.compAmmo.count--;
                        if (this.compAmmo.count <= 0)
                        {
                            this.compAmmo.StartReload();
                        }
                        result = true;
                    }
                }
            }
            return result;
        }

        public override void WarmupComplete()
        {
            if (isJammed)
            {
                System.Random random = new System.Random();
                int num = random.Next(0, 2);
                if (num == 1)
                {
                    isJammed = false;
                    Vector3 loc = new Vector3((float)this.caster.Position.x + 1f, (float)this.caster.Position.y, (float)this.caster.Position.z + 1f);
                    MoteThrower.ThrowText(loc, "Jam Cleared", Color.white);
                    this.ownerEquipment.def.soundInteract.PlayOneShot(caster.Position);
                    return;
                }
                else
                {
                    return;
                }
            }
            int jamChance = 0;
            QualityCategory qual;
            this.caster.TryGetQuality(out qual);
            switch (qual)
            {
                case QualityCategory.Awful:
                    jamChance = 30;
                    break;
                case QualityCategory.Poor:
                    jamChance = 40;
                    break;
                case QualityCategory.Shoddy:
                    jamChance = 50;
                    break;
                case QualityCategory.Normal:
                    jamChance = 60;
                    break;
                case QualityCategory.Good:
                    jamChance = 70;
                    break;
                case QualityCategory.Excellent:
                    jamChance = 80;
                    break;
                case QualityCategory.Superior:
                    jamChance = 90;
                    break;
                case QualityCategory.Masterwork:
                    jamChance = 100;
                    break;
                case QualityCategory.Legendary:
                    jamChance = 10000;
                    break;
                default:
                    jamChance = 60;
                    break;
            }

            if(Rand.Range(1, jamChance) == 1)
            {
                Vector3 loc = new Vector3((float)this.caster.Position.x + 1f, (float)this.caster.Position.y, (float)this.caster.Position.z + 1f);
                MoteThrower.ThrowText(loc, "Jammed", Color.white);
                SoundStarter.PlayOneShot(jamSound, caster.Position);
                isJammed = true;
                return;
            }

            base.WarmupComplete();
            if (base.CasterIsPawn && base.CasterPawn.skills != null)
            {
                float xp = 10f;
                if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn)
                {
                    if (this.currentTarget.Thing.HostileTo(this.caster))
                    {
                        xp = 240f;
                    }
                    else
                    {
                        xp = 50f;
                    }
                }
                base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp);
            }
        }
    }
}

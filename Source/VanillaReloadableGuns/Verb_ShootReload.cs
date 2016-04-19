using RimWorld;
using System;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaReloadableGuns
{
    public class Verb_ShootReload : Verb_LaunchProjectile
    {
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
    }
}

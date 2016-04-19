using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
namespace VanillaReloadableGuns
{
    public class JobDriver_Reload : JobDriver
    {
        private CompReloader CompReloader
        {
            get
            {
                return ThingCompUtility.TryGetComp<CompReloader>(base.TargetThingB);
            }
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnMentalState(this, TargetIndex.A);
            Toil waitToil = new Toil();
            waitToil.initAction = delegate
            {
                waitToil.actor.pather.StopDead();
            };
            waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
            waitToil.defaultDuration = this.CompReloader.reloaderProp.reloadTick;
            yield return waitToil;
            Toil toil = new Toil();
            toil.AddFinishAction(new Action(this.CompReloader.FinishReload));
            yield return toil;
            yield break;
        }
    }
}

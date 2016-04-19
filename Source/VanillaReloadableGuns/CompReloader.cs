using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VanillaReloadableGuns
{
    public class CompReloader : CompRangedGizmoGiver
    {
        private class GizmoAmmoStatus : Command
        {
            public CompReloader compAmmo;
            private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
            private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
            private static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG", true);
            public override float Width
            {
                get
                {
                    return 120f;
                }
            }
            public override GizmoResult GizmoOnGUI(Vector2 topLeft)
            {
                Rect rect = new Rect(topLeft.x, topLeft.y, this.Width, 75f);
                Widgets.DrawBox(rect, 1);
                GUI.DrawTexture(rect, BGTex);
                Rect rect2 = GenUI.ContractedBy(rect, 6f);
                Rect rect3 = rect2;
                rect3.height = rect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect3, this.compAmmo.parent.def.LabelCap);
                Rect rect4 = rect2;
                rect4.yMin = rect.y + rect.height / 2f;
                float num = (float)this.compAmmo.count / (float)this.compAmmo.reloaderProp.roundPerMag;
                Widgets.FillableBar(rect4, num, CompReloader.GizmoAmmoStatus.FullTex, CompReloader.GizmoAmmoStatus.EmptyTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, this.compAmmo.count + " / " + this.compAmmo.reloaderProp.roundPerMag);
                Text.Anchor = TextAnchor.UpperLeft;
                return new GizmoResult(0);
            }
        }
        public int count;
        public bool needReload;
        public CompProperties_Reloader reloaderProp;
        public CompEquippable CompEquippable
        {
            get
            {
                return this.parent.GetComp<CompEquippable>();
            }
        }
        public Pawn Wielder
        {
            get
            {
                return this.CompEquippable.PrimaryVerb.CasterPawn;
            }
        }
        public override void Initialize(CompProperties vprops)
        {
            base.Initialize(vprops);
            this.reloaderProp = (vprops as CompProperties_Reloader);
            if (this.reloaderProp != null)
            {
                this.count = this.reloaderProp.roundPerMag;
            }
            else
            {
                Log.Warning("Could not find a CompProperties_Reloader for CompReloader.");
                this.count = 9876;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<int>(ref this.count, "count", 1, false);
        }
        public void StartReload()
        {
            this.count = 0;
            this.needReload = true;
            if (this.Wielder == null)
            {
                Log.ErrorOnce("Wielder of " + this.parent + " is null!", 7381889);
                this.FinishReload();
            }
            else
            {
                if (this.reloaderProp.throwMote)
                {
                    MoteThrower.ThrowText(this.Wielder.Position.ToVector3Shifted(), Translator.Translate("CR_ReloadingMote"), -1);
                }
                Job job = new Job(DefDatabase<JobDef>.GetNamed("SkullyTweaksReloadWeapon", true), this.Wielder, this.parent)
                {
                    playerForced = true
                };
                if (this.Wielder.drafter != null)
                {
                    this.Wielder.drafter.TakeOrderedJob(job);
                }
                else
                {
                    ExternalPawnDrafter.TakeOrderedJob(this.Wielder, job);
                }
            }
        }
        public void FinishReload()
        {
            SoundStarter.PlayOneShot(this.parent.def.soundInteract, SoundInfo.InWorld(this.Wielder.Position, 0));
            if (this.reloaderProp.throwMote)
            {
                MoteThrower.ThrowText(this.Wielder.Position.ToVector3Shifted(), Translator.Translate("CR_ReloadedMote"), -1);
            }
            this.count = this.reloaderProp.roundPerMag;
            this.needReload = false;
        }
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            GizmoAmmoStatus gizmoAmmoStatus = new GizmoAmmoStatus
            {
                compAmmo = this
            };
            yield return gizmoAmmoStatus;
            if (this.Wielder != null)
            {
                Command_Action command_Action = new Command_Action
                {
                    action = new Action(this.StartReload),
                    defaultLabel = Translator.Translate("CR_ReloadLabel"),
                    defaultDesc = Translator.Translate("CR_ReloadDesc"),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/Reload", true)
                };
                yield return command_Action;
            }
            yield break;
        }
    }
}

using RimWorld;
using System;
using Verse;
using Verse.AI;
namespace VanillaReloadableGuns
{
    public static class ExternalPawnDrafter
    {
        public static bool CanTakeOrderedJob(Pawn pawn)
        {
            return !AttachmentUtility.HasAttachment(pawn, ThingDefOf.Fire) && (pawn.CurJob == null || pawn.CurJob.def.playerInterruptible);
        }
        public static void TakeOrderedJob(Pawn pawn, Job newJob)
        {
            if (pawn.jobs.debugLog)
            {
                pawn.jobs.DebugLogEvent("TakeOrderedJob " + newJob);
            }
            if (!ExternalPawnDrafter.CanTakeOrderedJob(pawn))
            {
                if (pawn.jobs.debugLog)
                {
                    pawn.jobs.DebugLogEvent("    CanTakePlayerJob is false. Returning.");
                }
            }
            else
            {
                GenJob.ValidateJob(newJob);
                if (pawn.jobs.curJob == null || !pawn.jobs.curJob.JobIsSameAs(newJob))
                {
                    pawn.stances.CancelBusyStanceSoft();
                    Find.PawnDestinationManager.UnreserveAllFor(pawn);
                    if (newJob.def == JobDefOf.Goto)
                    {
                        Find.PawnDestinationManager.ReserveDestinationFor(pawn, newJob.targetA.Cell);
                    }
                    if (pawn.jobs.debugLog)
                    {
                        pawn.jobs.DebugLogEvent("    Queueing job");
                    }
                    if (pawn.jobs.jobQueue == null)
                    {
                        pawn.jobs.jobQueue = new JobQueue();
                    }
                    pawn.jobs.jobQueue.Clear();
                    pawn.jobs.jobQueue.EnqueueFirst(newJob);
                    if (pawn.jobs.curJob != null)
                    {
                        pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
                    }
                }
            }
        }
    }
}

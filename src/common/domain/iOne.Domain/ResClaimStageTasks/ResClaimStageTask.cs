using System;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResClaimStages;
using iOne.ResTaskCategories;
using Volo.Abp.Domain.Entities;

namespace iOne.ResClaimStageTasks;

[Table("res_claim_stage_task")]
public class ResClaimStageTask : Entity
{
    public virtual Guid? ClaimStageId { get; private set; }

    public virtual Guid? TaskCategoryId { get; private set; }

    // Navigation
    public virtual ResClaimStage? ClaimStage { get; set; }
    public virtual ResTaskCategory? TaskCategory { get; set; }

    protected ResClaimStageTask()
    {
    }

    public ResClaimStageTask(Guid claimStageId, Guid taskCategoryId)
    {
        ClaimStageId = claimStageId;
        TaskCategoryId = taskCategoryId;
    }

    public override object[] GetKeys() => new object[] { ClaimStageId!, TaskCategoryId! };
}


using SFA.DAS.QnA.Api.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.QnA.Config.Preview.Types
{
    public class QnaWorkflows
    {
        public Project project { get; set; }
        public Workflow workflow { get; set; }
        public List<WorkflowSection> workflowSections { get; set; }
        public List<WorkflowSequence> workflowSequences { get; set; }
    }

    public class EntityBase
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
    }

    public class WorkflowStatus
    {
        public const string Live = "Live";
    }

}

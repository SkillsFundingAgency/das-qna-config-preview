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

}

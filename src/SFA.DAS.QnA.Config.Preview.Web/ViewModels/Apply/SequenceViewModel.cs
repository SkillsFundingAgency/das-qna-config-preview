using SFA.DAS.QnA.Api.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.QnA.Config.Preview.Web.ViewModels.Apply
{
    public class SequenceViewModel
    {
        public SequenceViewModel(Sequence sequence, Guid Id, string pageContext, List<Section> sections,
            List<QnA.Config.Preview.ApplyTypes.ApplySection> applySection, List<QnA.Config.Preview.ApplyTypes.ValidationErrorDetail> errorMessages)
        {
            this.Id = Id;
            Sections = sections;
            ApplySections = applySection;
            SequenceNo = sequence.SequenceNo;
            Status = sequence.Status;
            PageContext = pageContext;
            ErrorMessages = errorMessages;
        }

        public string Status { get; set; }
        public string PageContext { get;  }
        public List<Section> Sections { get; }
        public List<QnA.Config.Preview.ApplyTypes.ApplySection> ApplySections { get; }
        public Guid Id { get; }
        public int SequenceNo { get; }
        public List<QnA.Config.Preview.ApplyTypes.ValidationErrorDetail> ErrorMessages { get; }
    }
}

using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Config.Preview.ApplyTypes.CharityCommission;
using SFA.DAS.QnA.Config.Preview.ApplyTypes.CompaniesHouse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.QnA.Config.Preview.ApplyTypes
{
    public class ApplicationData
    {
        public string OrganisationReferenceId { get; set; }
        public string OrganisationName { get; set; }
        public string ReferenceNumber { get; set; }
        public string StandardName { get; set; }
        public string OrganisationType { get; set; }
        public int? StandardCode { get; set; }
        public string TradingName { get; set; }
        public bool UseTradingName { get; set; }
        public string ContactGivenName { get; set; }

        public CompaniesHouseSummary CompanySummary { get; set; }
        public CharityCommissionSummary CharitySummary { get; set; }
    }

    public class StandardApplicationData
    {
        public string StandardName { get; set; }
        public int StandardCode { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
    }

    public class ApplyData
    {
        public List<ApplySequence> Sequences { get; set; }
        public Apply Apply { get; set; }
    }


    public class ApplySequence
    {
        public Guid SequenceId { get; set; }
        public List<ApplySection> Sections { get; set; }
        public string Status { get; set; }
        public int SequenceNo { get; set; }
        public bool IsActive { get; set; }
        public bool NotRequired { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }

    }

    public class ApplySection
    {
        public Guid SectionId { get; set; }
        public int SectionNo { get; set; }
        public string Status { get; set; }
        public DateTime? ReviewStartDate { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? EvaluatedDate { get; set; }
        public string EvaluatedBy { get; set; }
        public bool NotRequired { get; set; }
        public bool? RequestedFeedbackAnswered { get; set; }
    }

    public class Feedback
    {
        public DateTime? Feedbackdate { get; set; }
        public string FeedbackBy { get; set; }
        public bool FeedbackAnswered { get; set; }
        public DateTime? FeedbackAnsweredDate { get; set; }
        public string FeedbackAnsweredBy { get; set; }
    }

    public class Apply
    {
        public string ReferenceNumber { get; set; }
        public int? StandardCode { get; set; }
        public string StandardReference { get; set; }
        public string StandardName { get; set; }
        public List<Submission> InitSubmissions { get; set; }
        public int InitSubmissionCount { get; set; }
        public DateTime? LatestInitSubmissionDate { get; set; }
        public DateTime? InitSubmissionFeedbackAddedDate { get; set; }
        public DateTime? InitSubmissionClosedDate { get; set; }
        public List<Submission> StandardSubmissions { get; set; }
        public int StandardSubmissionsCount { get; set; }
        public DateTime? LatestStandardSubmissionDate { get; set; }
        public DateTime? StandardSubmissionFeedbackAddedDate { get; set; }
        public DateTime? StandardSubmissionClosedDate { get; set; }
    }

    public class Submission
    {
        public DateTime SubmittedAt { get; set; }
        public Guid SubmittedBy { get; set; }
        public string SubmittedByEmail { get; set; }
    }

    public class ApplyTypeBase
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

    public class ApplicationSection : ApplyTypeBase
    {
        public Section Section { get; set; }
        public int SequenceNo { get; set; }
        public string PageContext { get; set; }

        public bool HasNewPageFeedback => Section.QnAData.Pages.Any(p => p.HasNewFeedback);
        public bool HasCompletedPageFeedback => Section.QnAData.Pages.Any(p => p.AllFeedbackIsCompleted);
    }

    public static class ApplicationSectionStatus
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string InProgress = "In Progress";
        public const string Graded = "Graded";
        public const string Evaluated = "Evaluated";
    }

    public static class SectionDisplayType
    {
        public const string Pages = "Pages";
        public const string Questions = "Questions";
        public const string PagesWithSections = "PagesWithSections";
    }

    public class DeliveryArea
    {
        public int Id { get; set; }
        public string Area { get; set; }
        public string Status { get; set; }
        public int Ordering { get; set; }
    }
}

﻿using Microsoft.AspNetCore.Http;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.QnA.Config.Preview.ApplyTypes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.QnA.Config.Preview.Api.Client
{
    public interface IQnaApiClient
    {
        Task<StartApplicationResponse> StartApplications(StartApplicationRequest startAppRequest);
        Task<ApplicationData> GetApplicationData(Guid applicationId);
        Task<Dictionary<string, object>> GetApplicationDataDictionary(Guid applicationId);
        Task<ApplicationData> UpdateApplicationData(Guid applicationId, ApplicationData applicationData);
        Task<Sequence> GetApplicationActiveSequence(Guid applicationId);
        Task<List<Sequence>> GetAllApplicationSequences(Guid applicationId);
        Task<Sequence> GetSequence(Guid applicationId, Guid sequenceId);
        Task<Sequence> GetSequenceBySequenceNo(Guid applicationId, int sequenceNo);
        Task<List<Section>> GetSections(Guid applicationId, Guid sequenceId);
        Task<Section> GetSection(Guid applicationId, Guid sectionId);
        Task<Section> GetSectionBySectionNo(Guid applicationId, int sequenceNo, int sectionNo);
        Task<Page> GetPage(Guid applicationId, Guid sectionId, string pageId);
        Task<Page> GetPageBySectionNo(Guid applicationId, int sequenceNo, int sectionNo, string pageId);
        Task<SetPageAnswersResponse> SetPageAnswers(Guid applicationId, Guid sectionId, string pageId, List<Answer> answer);
        Task<AddPageAnswerResponse> AddAnswersToMultipleAnswerPage(Guid applicationId, Guid sectionId, string pageId, List<Answer> answer);
        Task<SetPageAnswersResponse> Upload(Guid applicationId, Guid sectionId, string pageId, IFormFileCollection files);
        Task<HttpResponseMessage> DownloadFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string fileName);
        Task DeleteFile(Guid applicationId, Guid sectionId, string pageId, string questionId, string fileName);
        Task<Page> RemovePageAnswer(Guid applicationId, Guid sectionId, string pageId, Guid answerId);
        Task<bool> AllFeedbackCompleted(Guid applicationId, Guid sequenceId);
        Task<List<Project>> GetProjects();
        Task<List<Workflow>> GetWorkflows(Guid projectId);
        Task<Project> UpsertProject(Guid projectId, Project project);
        Task<Workflow> UpsertWorkflow(Guid projectId, Guid workflowId, Workflow workflow);
        Task<WorkflowSection> UpsertWorkflowSection(Guid projectId, Guid sectionId, WorkflowSection section);
        Task<WorkflowSequence> UpsertWorkflowSequence(Guid workflowId, Guid sequenceId, WorkflowSequence sequence);
    }
}

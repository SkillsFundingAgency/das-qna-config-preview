
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SFA.DAS.QnA.Api.Types;
using SFA.DAS.QnA.Api.Types.Page;
using SFA.DAS.QnA.Config.Preview.Api.Client;
using SFA.DAS.QnA.Config.Preview.ApplyTypes;
using SFA.DAS.QnA.Config.Preview.Web.ViewModels.Apply;
using SFA.DAS.QnA.Config.Preview.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.QnA.Config.Preview.Web.ViewModels;
using SFA.DAS.QnA.Config.Preview.Session;
using System.Net;
using SFA.DAS.QnA.Config.Preview.Types;

namespace SFA.DAS.QnA.Config.Preview.Web.Controllers
{
    /// <summary>
    /// Main application controller
    /// </summary>
    public class ApplicationController : Controller
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IQnaApiClient _qnaApiClient;
        private readonly ISessionService _sessionService;
        private const string UserReference = "8477e176-fe4b-4435-94a9-08d75ba10b41";
        private const string OrganisationName = "Preview Organisation";
        private readonly Guid OrganisationId = Guid.NewGuid();
        private const string OrganisationType = "Trade Body";

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="qnaApiClient"></param>
        /// <param name="sessionService"></param>
        /// <param name="logger"></param>
        public ApplicationController(IQnaApiClient qnaApiClient, ISessionService sessionService,
             ILogger<ApplicationController> logger)
        {
            _logger = logger;
            _sessionService = sessionService;
            _qnaApiClient = qnaApiClient;
        }

        [HttpGet("/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return RedirectToAction("StartApplication", "Application");
        }

        [HttpGet("/Application/StartApplication")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult StartApplication()
        {
            var previewViewModel = new PreviewViewModel();
            if (_sessionService.Exists("viewPreviewData"))
                previewViewModel = _sessionService.Get<PreviewViewModel>("viewPreviewData");
            return View(previewViewModel);
        }

        /// <summary>
        /// Inject Workflows
        /// </summary>
        /// <param name="qnaWorkflows"></param>
        /// <returns></returns>
        [HttpPost("/Application/UpsertQnaWorkflows")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> UpsertQnaWorkflows([FromBody] QnaWorkflows qnaWorkflows)
        {
            //Todo: Add input validations, better error handling and refactor into a service, mid you the qna always returns a null even when 
            //the call succeeds.
            var projectId = qnaWorkflows?.project?.Id;
            if (projectId != null)
            {
                var project = await _qnaApiClient.UpsertProject(qnaWorkflows.project.Id, qnaWorkflows.project);
                var upsertWorkflow = await _qnaApiClient.UpsertWorkflow(qnaWorkflows.project.Id, qnaWorkflows.workflow.Id, qnaWorkflows.workflow);
                foreach (var workflowSection in qnaWorkflows.workflowSections)
                {
                    var upsertWorkflowSection = await _qnaApiClient.UpsertWorkflowSection(qnaWorkflows.project.Id, workflowSection.Id, workflowSection);
                }
                foreach (var workflowSequence in qnaWorkflows.workflowSequences)
                {
                    await _qnaApiClient.UpsertWorkflowSequence(qnaWorkflows.workflow.Id, workflowSequence.Id, workflowSequence);
                }
            }
            else
            {
                return BadRequest(new BadRequestError($"Failed to insert project"));
            }

            return Ok();
        }

        [HttpPost("/Application/StartApplication")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> StartApplication(PreviewViewModel previewViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(previewViewModel);
            }
          
            var applicationStartRequest = new StartApplicationRequest
            {
                UserReference = UserReference,
                WorkflowType = previewViewModel.ProjectType,
                ApplicationData = previewViewModel.ApplicationData
            };

            var qnaResponse = new StartApplicationResponse();
            try
            {
                qnaResponse = await _qnaApiClient.StartApplications(applicationStartRequest);
            }
            catch (HttpRequestException ex) {
                dynamic obj = JToken.Parse(ex.Message);
                if(obj.statusCode == "400")
                {
                    ModelState.AddModelError(nameof(previewViewModel.ApplicationData), (string)obj.message);
                    return View(previewViewModel);
                }
            }

            if (_sessionService.Exists("viewPreviewData"))
                _sessionService.Remove("viewPreviewData");
            _sessionService.Set("viewPreviewData", JsonConvert.SerializeObject(previewViewModel));

            var allApplicationSequences = await _qnaApiClient.GetAllApplicationSequences(qnaResponse.ApplicationId);
            var sequenceNos = string.Join(',',allApplicationSequences.Select(x => x.SequenceNo));
            foreach (var _ in allApplicationSequences.Where(seq => seq.SequenceNo == previewViewModel.SequenceNo).Select(seq => new { }))
            {
                var sections = allApplicationSequences.Select(async sequence => await _qnaApiClient.GetSections(qnaResponse.ApplicationId, sequence.Id)).Select(t => t.Result).ToList();
                return RedirectToAction("Sequence", new { Id = qnaResponse.ApplicationId, sequenceNo = previewViewModel.SequenceNo });
            }

            ModelState.AddModelError(nameof(previewViewModel.SequenceNo),$"Sequence number not found. Valid sequences are: {sequenceNos}");
            return View(previewViewModel);
        }

        [HttpGet("/Application/{Id}/Sequence/{sequenceNo}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Sequence(Guid Id, int sequenceNo)
        {
            var sequence = await _qnaApiClient.GetSequenceBySequenceNo(Id, sequenceNo);
            var sections = await _qnaApiClient.GetSections(Id, sequence.Id);
            var applySection = new List<ApplySection>();
            foreach (var section in sections)
            {
                applySection.Add(new ApplySection
                {
                    SectionId = section.Id,
                    SectionNo = section.SectionNo,
                    Status = section.Status
                });
            }
            var sequenceVm = new SequenceViewModel(sequence, Id, OrganisationName, sections, applySection, null);

            return View(sequenceVm);
        }

        [HttpGet("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionNo}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Section(Guid Id, int sequenceNo, int sectionNo)
        {
            var section = await _qnaApiClient.GetSectionBySectionNo(Id, sequenceNo, sectionNo);
            var applicationSection = new ApplicationSection { Section = section, Id = Id };
            applicationSection.SequenceNo = sequenceNo;
            applicationSection.PageContext = OrganisationName;

            switch (section?.DisplayType)
            {
                case null:
                case SectionDisplayType.Pages:
                    return View("~/Views/Application/Section.cshtml", applicationSection);
                case SectionDisplayType.Questions:
                    return View("~/Views/Application/Section.cshtml", applicationSection);
                case SectionDisplayType.PagesWithSections:
                    return View("~/Views/Application/PagesWithSections.cshtml", applicationSection);
                default:
                    throw new BadRequestException("Section does not have a valid DisplayType");
            }
        }

        [HttpGet("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionNo}/Pages/{pageId}"), ModelStatePersist(ModelStatePersist.RestoreEntry)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Page(Guid Id, int sequenceNo, int sectionNo, string pageId, string __redirectAction, string __summaryLink = "Show")
        {
            var sequence = await _qnaApiClient.GetSequenceBySequenceNo(Id, sequenceNo);
            var section = await _qnaApiClient.GetSectionBySectionNo(Id, sequenceNo, sectionNo);

            PageViewModel viewModel = null;
            var returnUrl = Request.Headers["Referer"].ToString();
            var pageContext = OrganisationName;
            if (!ModelState.IsValid)
            {

                var page = JsonConvert.DeserializeObject<Page>((string)TempData["InvalidPage"]);
                page = GetDataFedOptions(page);

                var errorMessages = !ModelState.IsValid
                    ? ModelState.SelectMany(k => k.Value.Errors.Select(e => new ValidationErrorDetail()
                    {
                        ErrorMessage = e.ErrorMessage,
                        Field = k.Key
                    })).ToList()
                    : null;

                if (page.ShowTitleAsCaption)
                {
                    page.Title = section.Title;
                }

                UpdateValidationDetailsForAddress(page, errorMessages);
                viewModel = new PageViewModel(Id, sequenceNo, sectionNo, pageId, page, pageContext, __redirectAction,
                    returnUrl, errorMessages, __summaryLink);
            }
            else
            {
                try
                {
                    var page = await _qnaApiClient.GetPageBySectionNo(Id, sequenceNo, sectionNo, pageId);

                    if (page != null && (!page.Active))
                    {
                        var nextPage = page.Next.FirstOrDefault(p => p.Conditions is null || p.Conditions.Count == 0);

                        if (nextPage?.ReturnId != null && nextPage?.Action == "NextPage")
                        {
                            pageId = nextPage.ReturnId;
                            return RedirectToAction("Page",
                                new { Id, sequenceNo, sectionNo, pageId, __redirectAction });
                        }
                        else
                        {
                            return RedirectToAction("Section", new { Id, sequenceNo, sectionNo });
                        }
                    }

                    page = GetDataFedOptions(page);

                    if (page.ShowTitleAsCaption)
                    {
                        page.Title = section.Title;
                    }

                    viewModel = new PageViewModel(Id, sequenceNo, sectionNo, page.PageId, page, pageContext, __redirectAction,
                        returnUrl, null, __summaryLink);

                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("Could not find the page", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Applications");
                    throw ex;
                }

            }

            if (viewModel.AllowMultipleAnswers)
            {
                return View("~/Views/Application/Pages/MultipleAnswers.cshtml", viewModel);
            }

            return View("~/Views/Application/Pages/Index.cshtml", viewModel);
        }

        [HttpPost("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionNo}/Pages/{pageId}/multiple"), ModelStatePersist(ModelStatePersist.Store)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SaveMultiplePageAnswers(Guid Id, int sequenceNo, int sectionNo, string pageId, string __redirectAction, string __formAction)
        {
            try
            {
                string __summaryLink = HttpContext.Request.Form["__summaryLink"];

                var page = await _qnaApiClient.GetPageBySectionNo(Id, sequenceNo, sectionNo, pageId);

                if (page.AllowMultipleAnswers)
                {
                    var answers = GetAnswersFromForm(page);
                    var pageAddResponse = await _qnaApiClient.AddAnswersToMultipleAnswerPage(Id, page.SectionId.Value, page.PageId, answers);
                    if (pageAddResponse?.Success != null && pageAddResponse.Success)
                    {
                        if (__formAction == "Add")
                        {
                            return RedirectToAction("Page", new
                            {
                                Id,
                                sequenceNo,
                                sectionNo,
                                pageId = pageAddResponse.Page.PageId,
                                __redirectAction,
                                __summaryLink
                            });
                        }

                        if (__redirectAction == "Feedback")
                            return RedirectToAction("Feedback", new { Id });

                        var nextAction = pageAddResponse.Page.Next.SingleOrDefault(x => x.Action == "NextPage");

                        if (!string.IsNullOrEmpty(nextAction.Action))
                            return RedirectToNextAction(Id, sequenceNo, sectionNo, __redirectAction, nextAction.Action, nextAction.ReturnId);
                    }
                    else if (page.PageOfAnswers?.Count > 0 && __formAction != "Add")
                    {
                        if (page.HasFeedback && page.HasNewFeedback && !page.AllFeedbackIsCompleted && __redirectAction == "Feedback")
                        {
                            page = StoreEnteredAnswers(answers, page);
                            SetResponseValidationErrors(pageAddResponse?.ValidationErrors, page);
                            if (!page.AllFeedbackIsCompleted || pageAddResponse?.ValidationErrors.Count > 0)
                            {
                                return RedirectToAction("Page", new { Id, sequenceNo, sectionNo, pageId, __redirectAction, __summaryLink });
                            }
                            return RedirectToAction("Feedback", new { Id });
                        }
                        else if (pageAddResponse.ValidationErrors?.Count == 0 || answers.All(x => x.Value == string.Empty || Regex.IsMatch(x.Value, "^[,]+$")))
                        {
                            var nextAction = page.Next.SingleOrDefault(x => x.Action == "NextPage");

                            if (!string.IsNullOrEmpty(nextAction.Action))
                            {
                                if (__redirectAction == "Feedback")
                                {
                                    foreach (var answer in answers)
                                    {
                                        if (page.Next.Exists(y => y.Conditions.Exists(x => x.QuestionId == answer.QuestionId || x.QuestionTag == answer.QuestionId)))
                                        {
                                            return RedirectToNextAction(Id, sequenceNo, sectionNo, __redirectAction, nextAction.Action, nextAction.ReturnId, "Hide");
                                        }
                                        break;
                                    }

                                    return RedirectToAction("Feedback", new { Id });
                                }

                                return RedirectToNextAction(Id, sequenceNo, sectionNo, __redirectAction, nextAction.Action, nextAction.ReturnId);
                            }
                        }
                    }

                    if (!page.PageOfAnswers.Any())
                    {
                        page.PageOfAnswers = new List<PageOfAnswers>() { new PageOfAnswers() { Answers = new List<Answer>() } };
                    }

                    page = StoreEnteredAnswers(answers, page);

                    SetResponseValidationErrors(pageAddResponse?.ValidationErrors, page);

                }
                else
                {
                    return BadRequest("Page is not of a type of Multiple Answers");
                }

                return RedirectToAction("Page", new { Id, sequenceNo, sectionNo, pageId, __redirectAction, __summaryLink });
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Could not find the page", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Applications");
                throw ex;
            }
        }

        [HttpPost("/Application/{Id}/Sequences/{sequenceNo}/Sections/{sectionNo}/Pages/{pageId}"), ModelStatePersist(ModelStatePersist.Store)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SaveAnswers(Guid Id, int sequenceNo, int sectionNo, string pageId, string __redirectAction)
        {
            var updatePageResult = new SetPageAnswersResponse();

            try
            {
                string __summaryLink = HttpContext.Request.Form["__summaryLink"];
                var page = await _qnaApiClient.GetPageBySectionNo(Id, sequenceNo, sectionNo, pageId);
                var fileupload = page.Questions?.Any(q => q.Input.Type == "FileUpload");
                var answers = GetAnswersFromForm(page);


                if (HttpContext.Request.Form.Files.Count == 0 && fileupload == false)
                {
                    if (__redirectAction == "Feedback" && !HasAtLeastOneAnswerChanged(page, answers) && !page.AllFeedbackIsCompleted)
                        SetAnswerNotUpdated(page);
                    else
                        updatePageResult = await _qnaApiClient.AddPageAnswer(Id, page.SectionId.Value, page.PageId, answers);
                }


                if (fileupload == true)
                {
                    updatePageResult = new SetPageAnswersResponse();
                    var errorMessages = new List<ValidationErrorDetail>();
                    if (HttpContext.Request.Form.Files.Count == 0 && NothingToUpload(updatePageResult, answers))
                    {

                        if (__redirectAction == "Feedback")
                        {
                            if (page.HasFeedback && page.HasNewFeedback && !page.AllFeedbackIsCompleted)
                            {
                                SetAnswerNotUpdated(page);
                                return RedirectToAction("Page", new { Id, sequenceNo, sectionNo, pageId, __redirectAction, __summaryLink });
                            }
                            if (FileValidator.FileValidationPassed(answers, page, errorMessages, ModelState, HttpContext.Request.Form.Files))
                                return RedirectToAction("Feedback", new { Id });
                        }
                        else
                        {
                            if (FileValidator.FileValidationPassed(answers, page, errorMessages, ModelState, HttpContext.Request.Form.Files))
                                return ForwardToNextSectionOrPage(page, Id, sequenceNo, sectionNo, __redirectAction);
                        }
                    }
                    else
                    {
                        if (FileValidator.FileValidationPassed(answers, page, errorMessages, ModelState, HttpContext.Request.Form.Files))
                            updatePageResult = await UploadFilesToStorage(Id, page.SectionId.Value, page.PageId, page);
                    }

                }

                if (updatePageResult?.ValidationPassed == true)
                {
                    if (__redirectAction == "Feedback")
                    {
                        foreach (var answer in answers)
                        {
                            if (page.Next.Exists(y => y.Conditions.Exists(x => x.QuestionId == answer.QuestionId || x.QuestionTag == answer.QuestionId)))
                            {
                                return RedirectToNextAction(Id, sequenceNo, sectionNo, __redirectAction, updatePageResult.NextAction, updatePageResult.NextActionId, "Hide");
                            }
                            break;
                        }

                        return RedirectToAction("Feedback", new { Id });
                    }

                    if (!string.IsNullOrEmpty(updatePageResult.NextAction))
                        return RedirectToNextAction(Id, sequenceNo, sectionNo, __redirectAction, updatePageResult.NextAction, updatePageResult.NextActionId);
                }

                if (!page.PageOfAnswers.Any())
                {
                    page.PageOfAnswers = new List<PageOfAnswers>() { new PageOfAnswers() { Answers = new List<Answer>() } };
                }

                page = StoreEnteredAnswers(answers, page);

                SetResponseValidationErrors(updatePageResult?.ValidationErrors, page);

                return RedirectToAction("Page", new { Id, sequenceNo, sectionNo, pageId, __redirectAction, __summaryLink });
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Could not find the page", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Applications");

                _logger.LogError(ex, ex.Message);

                throw ex;
            }
        }

        [HttpPost("/Application/DeleteAnswer")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteAnswer(Guid Id, int sequenceNo, int sectionNo, string pageId, Guid answerId, string __redirectAction, string __summaryLink = "False")
        {

            var page = await _qnaApiClient.GetPageBySectionNo(Id, sequenceNo, sectionNo, pageId);
            try
            {
                await _qnaApiClient.RemovePageAnswer(Id, page.SectionId.Value, page.PageId, answerId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Page answer removal errored : {ex} ");
            }

            return RedirectToAction("Page", new { Id, sequenceNo, sectionNo, pageId, __redirectAction, __summaryLink });
        }

        [HttpGet("Application/{Id}/Section/{sectionId}/Page/{pageId}/Question/{questionId}/{filename}/Download")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Download(Guid Id, Guid sectionId, string pageId, string questionId, string filename)
        {
            var response = await _qnaApiClient.DownloadFile(Id, sectionId, pageId, questionId, filename);

            var fileStream = await response.Content.ReadAsStreamAsync();

            return File(fileStream, response.Content.Headers.ContentType.MediaType, filename);

        }

        [HttpGet("Application/{Id}/SequenceNo/{sequenceNo}/Section/{sectionId}/Page/{pageId}/Question/{questionId}/Filename/{filename}/RedirectAction/{__redirectAction}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteFile(Guid Id, int sequenceNo, Guid sectionId, string pageId, string questionId, string filename, string __redirectAction)
        {
            var section = await _qnaApiClient.GetSection(Id, sectionId);

            await _qnaApiClient.DeleteFile(Id, section.Id, pageId, questionId, filename);

            return RedirectToAction("Page", new { Id, sequenceNo, section.SectionNo, pageId, __redirectAction });
        }


        private Page GetDataFedOptions(Page page)
        {
            if (page != null)
            {
                foreach (var question in page.Questions)
                {
                    if (question.Input.Type.StartsWith("DataFed_"))
                    {
                        var deliveryAreas = new List<DeliveryArea>();
                        deliveryAreas.Add(new DeliveryArea
                        {
                            Id = 8,
                            Area = "West Midlands",
                            Ordering = 0,
                            Status = "Live"
                        });
                        var questionOptions = deliveryAreas.Select(da => new Option() { Label = da.Area, Value = da.Area }).ToList();

                        question.Input.Options = questionOptions;
                        question.Input.Type = question.Input.Type.Replace("DataFed_", "");
                    }
                }
            }

            return page;
        }


        private void SetAnswerNotUpdated(Page page)
        {
            var validationErrors = new List<KeyValuePair<string, string>>();
            var updatePageResult = new SetPageAnswersResponse();
            foreach (var question in page.Questions)
                validationErrors.Add(new KeyValuePair<string, string>(question.QuestionId, "Unable to save as you have not updated your answer"));

            updatePageResult.ValidationPassed = false;
            updatePageResult.ValidationErrors = validationErrors;
            SetResponseValidationErrors(updatePageResult?.ValidationErrors, page);
        }

        private static bool HasAtLeastOneAnswerChanged(Page page, List<Answer> answers)
        {
            var atLeastOneAnswerChanged = page.Questions.Any(q => q.Input.Type == "FileUpload");

            foreach (var question in page.Questions)
            {
                var answer = answers.FirstOrDefault(a => a.QuestionId == question.QuestionId);
                var existingAnswer = page.PageOfAnswers.SelectMany(poa => poa.Answers).FirstOrDefault(a => a.QuestionId == question.QuestionId);

                atLeastOneAnswerChanged = atLeastOneAnswerChanged
                    ? true
                    : !answer?.Value.Equals(existingAnswer.Value, StringComparison.OrdinalIgnoreCase) ?? answer != existingAnswer;

                if (question.Input.Options != null)
                {
                    foreach (var option in question.Input.Options)
                    {
                        if (answer?.Value == option.Value.ToString())
                        {
                            if (option.FurtherQuestions != null)
                            {
                                var atLeastOneFutherQuestionAnswerChanged = page.Questions.Any(q => q.Input.Type == "FileUpload");

                                foreach (var furtherQuestion in option.FurtherQuestions)
                                {
                                    var furtherAnswer = answers.FirstOrDefault(a => a.QuestionId == furtherQuestion.QuestionId);
                                    var existingFutherAnswer = page.PageOfAnswers.SelectMany(poa => poa.Answers).FirstOrDefault(a => a.QuestionId == furtherQuestion.QuestionId);

                                    atLeastOneFutherQuestionAnswerChanged = atLeastOneFutherQuestionAnswerChanged
                                        ? true
                                        : !furtherAnswer?.Value.Equals(existingFutherAnswer.Value, StringComparison.OrdinalIgnoreCase) ?? furtherAnswer != existingFutherAnswer;
                                }

                                atLeastOneAnswerChanged = atLeastOneAnswerChanged ? true : atLeastOneFutherQuestionAnswerChanged;
                            }
                        }
                    }
                }
            }

            return atLeastOneAnswerChanged;
        }

        private static bool NothingToUpload(SetPageAnswersResponse updatePageResult, List<Answer> answers)
        {
            return updatePageResult.ValidationErrors == null && !updatePageResult.ValidationPassed
                    && answers.Count > 0;
        }

        private RedirectToActionResult ForwardToNextSectionOrPage(Page page, Guid Id, int sequenceNo, int sectionNo, string __redirectAction)
        {
            var next = page.Next.FirstOrDefault(x => x.Action == "NextPage");
            if (next != null)
                return RedirectToNextAction(Id, sequenceNo, sectionNo, __redirectAction, next.Action, next.ReturnId);
            return RedirectToAction("Section", new { Id, sequenceNo, sectionNo });
        }



        private static Page StoreEnteredAnswers(List<Answer> answers, Page page)
        {
            if (answers != null && answers.Any() && page.PageOfAnswers != null)
                page.PageOfAnswers.Add(new PageOfAnswers { Answers = answers });

            return page;
        }


        private RedirectToActionResult RedirectToNextAction(Guid Id, int sequenceNo, int sectionNo, string redirectAction, string nextAction, string nextActionId, string __summaryLink = "Show")
        {
            if (nextAction == "NextPage")
            {
                return RedirectToAction("Page", new
                {
                    Id,
                    sequenceNo,
                    sectionNo,
                    pageId = nextActionId,
                    __redirectAction = redirectAction,
                    __summaryLink
                });
            }

            return nextAction == "ReturnToSection"
                ? RedirectToAction("Section", "Application", new { Id, sequenceNo, sectionNo })
                : RedirectToAction("Sequence", "Application", new { Id, sequenceNo });
        }

        private void SetResponseValidationErrors(List<KeyValuePair<string, string>> validationErrors, Page page)
        {
            if (validationErrors != null)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
            }

            TempData["InvalidPage"] = JsonConvert.SerializeObject(page);
        }

        private async Task<SetPageAnswersResponse> UploadFilesToStorage(Guid applicationId, Guid sectionId, string pageId, Page page)
        {
            SetPageAnswersResponse response = new SetPageAnswersResponse(null);
            if (HttpContext.Request.Form.Files.Any() && page != null)
                response = await _qnaApiClient.Upload(applicationId, sectionId, pageId, HttpContext.Request.Form.Files);
            return response;
        }

        private List<Answer> GetAnswersFromForm(Page page)
        {
            List<Answer> answers = new List<Answer>();

            foreach (var keyValuePair in HttpContext.Request.Form.Where(f => !f.Key.StartsWith("__")))
            {
                if (!keyValuePair.Key.EndsWith("Search"))
                {
                    answers.Add(new Answer() { QuestionId = keyValuePair.Key, Value = keyValuePair.Value });
                }
            }

            #region FurtherQuestion_Processing
            // Get the first Question that has FutherQuestions
            var questionWithFutherQuestions = page.Questions.Where(x => x.Input.Type == "ComplexRadio" && x.Input.Options != null && x.Input.Options.Any(o => o.FurtherQuestions.Any())).FirstOrDefault();

            if (questionWithFutherQuestions != null)
            {
                var questionIdContainingFutherQuestions = questionWithFutherQuestions.QuestionId;

                // Get a list of values which would show FutherQuestions
                var valuesThatShowFurtherQuestions = questionWithFutherQuestions.Input.Options.Where(o => o.FurtherQuestions != null && o.FurtherQuestions.Any()).Select(opt => opt.Value).ToList();

                // If supplied answer results in no FutherQuestions being shown then remove those that have slipped in. This is because the HTML Posts all form inputs to us.
                if (!answers.Any(a => a.QuestionId == questionIdContainingFutherQuestions && valuesThatShowFurtherQuestions.Contains(a.Value)))
                {
                    for (int i = answers.Count - 1; i >= 0; i--)
                    {
                        if (answers[i].QuestionId.Contains(questionIdContainingFutherQuestions + "."))
                        {
                            answers.RemoveAt(i);
                        }
                    }
                }
            }
            # endregion FurtherQuestion_Processing

            var questionId = page.Questions.Where(x => x.Input.Type == "ComplexRadio" || x.Input.Type == "Radio").Select(y => y.QuestionId).FirstOrDefault();

            if (!answers.Any() && !string.IsNullOrEmpty(questionId))
            {
                answers.Add(new Answer { QuestionId = questionId, Value = "" });
            }
            else if (questionId != null && answers.Any(y => y.QuestionId.Contains(questionId)))
            {
                if (answers.All(x => x.Value == "" || Regex.IsMatch(x.Value, "^[,]+$")))
                {
                    foreach (var answer in answers.Where(y => y.QuestionId.Contains(questionId + ".") && (y.Value == "" || Regex.IsMatch(y.Value, "^[,]+$"))))
                    {
                        answer.QuestionId = questionId;
                        break;
                    }
                }
                else if (answers.Count(y => y.QuestionId.Contains(questionId) && (y.Value == "" || Regex.IsMatch(y.Value, "^[,]+$"))) == 1
                    && answers.Count(y => y.QuestionId == questionId && y.Value != "") == 0)
                {
                    foreach (var answer in answers.Where(y => y.QuestionId.Contains(questionId + ".") && (y.Value == "" || Regex.IsMatch(y.Value, "^[,]+$"))))
                    {
                        answer.QuestionId = questionId;
                    }
                }
            }

            if (page.Questions.Any(x => x.Input.Type == "Address"))
                return ProcessPageVmQuestionsForAddress(page, answers);

            return answers;
        }


        private static List<Answer> ProcessPageVmQuestionsForAddress(Page page, List<Answer> answers)
        {

            if (page.Questions.Any(x => x.Input.Type == "Address"))
            {
                Dictionary<string, JObject> answerValues = new Dictionary<string, JObject>();

                foreach (var formVariable in answers.Where(x => x.QuestionId.Contains("_Key_")))
                {
                    var answerKey = formVariable.QuestionId.Split("_Key_");
                    if (!answerValues.ContainsKey(answerKey[0]))
                    {
                        answerValues.Add(answerKey[0], new JObject());
                    }

                    answerValues[answerKey[0]].Add(
                        answerKey.Count() == 1 ? string.Empty : answerKey[1],
                        formVariable.Value.ToString());
                }

                answers = answers.Where(x => !x.QuestionId.Contains("_Key")).ToList();

                foreach (var answer in answerValues)
                {
                    if (answer.Value.Count > 1)
                    {
                        answers.Add(new Answer() { QuestionId = answer.Key, Value = answer.Value.ToString() });
                    }
                }

            }

            return answers;
        }


        //Todo: Remove this function if and when the _Address.cshtml is refactored or the qna modelstate 
        //reflects the keys that are set in the _Address.cshtml. Currently the ValidationErrorDetailTagHelper will not 
        //update the address fields because of the keys mismatch.
        private static void UpdateValidationDetailsForAddress(Page page, List<ValidationErrorDetail> errorMessages)
        {
            var question = page.Questions.SingleOrDefault(x => x.Input.Type == "Address");
            if (question != null)
            {
                foreach (var error in errorMessages)
                {
                    switch (error.ErrorMessage)
                    {
                        case "Enter building and street":
                            error.Field = $"{question.QuestionId}_Key_AddressLine1";
                            break;
                        case "Enter town or city":
                            error.Field = $"{question.QuestionId}_Key_AddressLine3";
                            break;
                        case "Enter postcode":
                            error.Field = $"{question.QuestionId}_Key_Postcode";
                            break;
                    }
                }
            }

        }
    }
    /// <summary>
    /// Bad Request Exception
    /// </summary>
    public sealed class BadRequestException : Exception
    {
       /// <summary>
       /// Constructor
       /// </summary>
        public BadRequestException() : base("") { }
        
        /// <summary>
        /// Bad Request Exception
        /// </summary>
        /// <param name="message"></param>
        public BadRequestException(string message) : base(message) { }
    }

}

   
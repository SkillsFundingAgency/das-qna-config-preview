using FluentValidation;
using SFA.DAS.QnA.Config.Preview.Api.Client;
using SFA.DAS.QnA.Config.Preview.Web.ViewModels;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SFA.DAS.QnA.Config.Preview.Web.Validators
{
    public class PreviewViewModelValidator: AbstractValidator<PreviewViewModel>
    {
        private const string ProjectTypeFieldEmptyError = "Enter a project type";
        private const string ApplicaitonDataEmptyError = "Enter application data";
        private const string ProjectTypeFieldDoesNotExistError = "Project type does not exist in QnA workflow";
        private const string SequenceNoFieldEmptyError = "Enter a sequence number";
        private const string InvalidJson = "Enter application data in a correct json format";

        public PreviewViewModelValidator(IQnaApiClient qnaApiClient)
        {
            RuleFor(vm => vm).Custom((vm, context) =>
            {  
                if (vm.SequenceNo == null)
                    context.AddFailure(nameof(vm.SequenceNo), SequenceNoFieldEmptyError);
                if(string.IsNullOrEmpty(vm.ApplicationData))
                    context.AddFailure(nameof(vm.ApplicationData), ApplicaitonDataEmptyError);
                else
                {
                    if (!IsValidJson(vm.ApplicationData))
                        context.AddFailure(nameof(vm.ApplicationData), InvalidJson);
                }

                if (string.IsNullOrEmpty(vm.ProjectType))
                    context.AddFailure(nameof(vm.ProjectType), ProjectTypeFieldEmptyError);
                else
                {
                    bool projectTypeExists = false;
                    var projects = qnaApiClient.GetProjects().Result;
                    foreach (var _ in from project in projects
                                      let workflow = qnaApiClient.GetWorkflows(project.Id).Result
                                      where workflow != null && workflow.Exists(x => x.Type.Equals(vm.ProjectType, System.StringComparison.OrdinalIgnoreCase))
                                      select new { })
                    {
                        projectTypeExists = true;
                        break;
                    }

                    if (!projectTypeExists)
                        context.AddFailure(nameof(vm.ProjectType), ProjectTypeFieldDoesNotExistError);
                }
            });
        }

        private static bool IsValidJson(string strJson)
        {
            strJson = strJson.Trim();
            if ((strJson.StartsWith("{") && strJson.EndsWith("}")) ||
                (strJson.StartsWith("[") && strJson.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(strJson);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}

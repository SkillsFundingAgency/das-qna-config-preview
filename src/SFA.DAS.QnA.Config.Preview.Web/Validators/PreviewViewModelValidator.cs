using FluentValidation;
using SFA.DAS.QnA.Config.Preview.Api.Client;
using SFA.DAS.QnA.Config.Preview.Web.ViewModels;
using System.Linq;

namespace SFA.DAS.QnA.Config.Preview.Web.Validators
{
    public class PreviewViewModelValidator: AbstractValidator<PreviewViewModel>
    {
        private const string ProjectTypeFieldEmptyError = "Project type cannot be empty";
        private const string ProjectTypeFieldDoesNotExistError = "Project type does not exist in QnA Workflow";
        private const string SequenceNoFieldEmptyError = "Sequence number cannot be empty or zero";
        public PreviewViewModelValidator(IQnaApiClient qnaApiClient)
        {
            RuleFor(vm => vm).Custom((vm, context) =>
            {  
                if (vm.SequenceNo == 0)
                    context.AddFailure(nameof(vm.SequenceNo), SequenceNoFieldEmptyError);
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
    }
}

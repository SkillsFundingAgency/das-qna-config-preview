using FluentValidation;
using SFA.DAS.QnA.Config.Preview.Web.ViewModels;

namespace SFA.DAS.QnA.Config.Preview.Web.Validators
{
    public class PreviewViewModelValidator: AbstractValidator<PreviewViewModel>
    {
        private const string ProjectTypeFieldEmptyError = "Project type cannot be empty";
        private const string SequenceNoFieldEmptyError = "Sequence number cannot be empty or zero";
        public PreviewViewModelValidator()
        {
            RuleFor(vm => vm).Custom((vm, context) =>
            {
                if (string.IsNullOrEmpty(vm.ProjectType))
                    context.AddFailure(nameof(vm.ProjectType), ProjectTypeFieldEmptyError);
                if (vm.SequenceNo == 0)
                    context.AddFailure(nameof(vm.SequenceNo), SequenceNoFieldEmptyError);
            });
        }
    }
}

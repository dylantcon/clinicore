using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Physicians
{
    /// <summary>
    /// ViewModel for editing existing physicians.
    /// Uses UpdatePhysicianProfileCommand for saving.
    /// </summary>
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    public class PhysicianEditViewModel : PhysicianFormViewModelBase
    {
        private readonly CommandInvoker _commandInvoker;
        private Guid _physicianId;

        public string PhysicianIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    _physicianId = guid;
                    LoadPhysicianCommand.Execute(null);
                }
            }
        }

        public MauiCommand LoadPhysicianCommand { get; }

        public PhysicianEditViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            CommandInvoker commandInvoker)
            : base(commandFactory, navigationService, sessionManager)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            Title = "Edit Physician";

            // Load command for populating form with existing data
            var viewCoreCommand = _commandFactory.CreateCommand(ViewPhysicianProfileCommand.Key);
            LoadPhysicianCommand = new MauiCommandAdapter(
                _commandInvoker,
                viewCoreCommand!,
                parameterBuilder: BuildLoadParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadResult
            );
        }

        protected override MauiCommandAdapter CreateSaveCommand()
        {
            var coreCommand = _commandFactory.CreateCommand(UpdatePhysicianProfileCommand.Key);
            return new MauiCommandAdapter(
                _commandInvoker,
                coreCommand!,
                parameterBuilder: BuildParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult
            );
        }

        private CommandParameters BuildLoadParameters()
        {
            return new CommandParameters()
                .SetParameter(ViewPhysicianProfileCommand.Parameters.ProfileId, _physicianId);
        }

        private void HandleLoadResult(CommandResult result)
        {
            if (result.Success && result.Data is PhysicianProfile physician)
            {
                Name = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                Address = physician.GetValue<string>("address") ?? string.Empty;
                BirthDate = physician.GetValue<DateTime>("birthdate");
                LicenseNumber = physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty;
                GraduationDate = physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey());

                // Load specializations
                var selectedSpecs = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? new List<MedicalSpecialization>();
                foreach (var item in SpecializationItems)
                {
                    item.IsSelected = selectedSpecs.Contains(item.Specialization);
                }

                ClearValidation();
            }
        }

        private CommandParameters BuildParameters()
        {
            return new CommandParameters()
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.ProfileId, _physicianId)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.Name, Name)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.Address, Address)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.BirthDate, BirthDate)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.LicenseNumber, LicenseNumber)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.GraduationDate, GraduationDate)
                .SetParameter(UpdatePhysicianProfileCommand.Parameters.Specializations, GetSelectedSpecializations());
        }

        protected override async Task NavigateBackAsync()
        {
            // After editing, go back to detail page
            await _navigationService.NavigateToAsync($"PhysicianDetailPage?physicianId={_physicianId}");
        }
    }
}

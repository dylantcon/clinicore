using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users.Concrete;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;
using GUI.CliniCore.Views.Patients;
using MauiCommand = System.Windows.Input.ICommand;

namespace GUI.CliniCore.ViewModels.Patients
{
    /// <summary>
    /// ViewModel for editing existing patients.
    /// Uses UpdatePatientProfileCommand for saving.
    /// </summary>
    [QueryProperty(nameof(PatientIdString), "patientId")]
    public class PatientEditViewModel : PatientFormViewModelBase
    {
        private readonly CommandInvoker _commandInvoker;
        private Guid _patientId;

        public string PatientIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    _patientId = guid;
                    LoadPatientCommand.Execute(null);
                }
            }
        }

        public MauiCommand LoadPatientCommand { get; }

        public PatientEditViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            CommandInvoker commandInvoker)
            : base(commandFactory, navigationService, sessionManager)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            Title = "Edit Patient";

            // Load command for populating form with existing data
            var viewCoreCommand = _commandFactory.CreateCommand(ViewPatientProfileCommand.Key);
            LoadPatientCommand = new MauiCommandAdapter(
                _commandInvoker,
                viewCoreCommand!,
                parameterBuilder: BuildLoadParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleLoadResult
            );
        }

        protected override MauiCommandAdapter CreateSaveCommand()
        {
            var coreCommand = _commandFactory.CreateCommand(UpdatePatientProfileCommand.Key);
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
                .SetParameter(ViewPatientProfileCommand.Parameters.ProfileId, _patientId);
        }

        private void HandleLoadResult(CommandResult result)
        {
            if (result.Success && result.Data is PatientProfile patient)
            {
                Name = patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty;
                Address = patient.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty;
                BirthDate = patient.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey());
                SelectedGender = patient.GetValue<Gender>(PatientEntryType.Gender.GetKey());
                Race = patient.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty;

                ClearValidation();
            }
        }

        private CommandParameters BuildParameters()
        {
            return new CommandParameters()
                .SetParameter(UpdatePatientProfileCommand.Parameters.ProfileId, _patientId)
                .SetParameter(UpdatePatientProfileCommand.Parameters.Name, Name)
                .SetParameter(UpdatePatientProfileCommand.Parameters.Address, Address)
                .SetParameter(UpdatePatientProfileCommand.Parameters.BirthDate, BirthDate)
                .SetParameter(UpdatePatientProfileCommand.Parameters.Gender, SelectedGender.ToString())
                .SetParameter(UpdatePatientProfileCommand.Parameters.Race, Race);
        }

        protected override async Task NavigateBackAsync()
        {
            // After editing, go back to detail page
            await _navigationService.NavigateToAsync($"{nameof(PatientDetailPage)}?patientId={_patientId}");
        }
    }
}

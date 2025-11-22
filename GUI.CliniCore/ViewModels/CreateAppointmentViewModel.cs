using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Scheduling;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;

namespace GUI.CliniCore.ViewModels
{
    /// <summary>
    /// ViewModel for creating new appointments.
    /// Uses ScheduleAppointmentCommand for saving.
    /// </summary>
    [QueryProperty(nameof(PatientIdString), "patientId")]
    [QueryProperty(nameof(PhysicianIdString), "physicianId")]
    public class CreateAppointmentViewModel : AppointmentFormViewModelBase
    {
        public string PatientIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == guid);
                }
            }
        }

        public string PhysicianIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == guid);
                }
            }
        }

        public CreateAppointmentViewModel(
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager)
            : base(commandFactory, navigationService, sessionManager)
        {
            Title = "Schedule Appointment";
        }

        protected override MauiCommandAdapter CreateSaveCommand()
        {
            var coreCommand = _commandFactory.CreateCommand(ScheduleAppointmentCommand.Key);
            return new MauiCommandAdapter(
                coreCommand!,
                parameterBuilder: BuildParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult,
                viewModel: this
            );
        }

        private CommandParameters BuildParameters()
        {
            var parameters = new CommandParameters()
                .SetParameter(ScheduleAppointmentCommand.Parameters.PatientId, SelectedPatient?.Id ?? Guid.Empty)
                .SetParameter(ScheduleAppointmentCommand.Parameters.PhysicianId, SelectedPhysician?.Id ?? Guid.Empty)
                .SetParameter(ScheduleAppointmentCommand.Parameters.StartTime, GetStartDateTime())
                .SetParameter(ScheduleAppointmentCommand.Parameters.DurationMinutes, DurationMinutes)
                .SetParameter(ScheduleAppointmentCommand.Parameters.Reason, Reason ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(Notes))
            {
                parameters.SetParameter(ScheduleAppointmentCommand.Parameters.Notes, Notes);
            }

            return parameters;
        }
    }
}

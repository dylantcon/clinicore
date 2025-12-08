using Core.CliniCore.Commands;
using Core.CliniCore.Commands.Scheduling;
using Core.CliniCore.Scheduling;
using Core.CliniCore.Service;
using GUI.CliniCore.Commands;
using GUI.CliniCore.Services;

namespace GUI.CliniCore.ViewModels.Appointments
{
    /// <summary>
    /// ViewModel for editing existing appointments.
    /// Uses UpdateAppointmentCommand for saving.
    /// </summary>
    [QueryProperty(nameof(AppointmentIdString), "appointmentId")]
    public partial class AppointmentEditViewModel : AppointmentFormViewModelBase
    {
        private readonly CommandInvoker _commandInvoker;
        private Guid _appointmentId;
        private AppointmentTimeInterval? _appointment;

        public string AppointmentIdString
        {
            set
            {
                if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
                {
                    _appointmentId = guid;
                    LoadAppointment();
                }
            }
        }

        public AppointmentEditViewModel(
            CommandInvoker commandInvoker,
            CommandFactory commandFactory,
            INavigationService navigationService,
            SessionManager sessionManager,
            ProfileService profileService,
            SchedulerService schedulerService)
            : base(commandFactory, navigationService, sessionManager, profileService, schedulerService)
        {
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            Title = "Edit Appointment";
        }

        protected override MauiCommandAdapter CreateSaveCommand()
        {
            var coreCommand = _commandFactory.CreateCommand(UpdateAppointmentCommand.Key);
            return new MauiCommandAdapter(
                _commandInvoker,
                coreCommand!,
                parameterBuilder: BuildParameters,
                sessionProvider: () => _sessionManager.CurrentSession,
                resultHandler: HandleSaveResult
            );
        }

        private CommandParameters BuildParameters()
        {
            var parameters = new CommandParameters()
                .SetParameter(UpdateAppointmentCommand.Parameters.AppointmentId, _appointmentId)
                .SetParameter(UpdateAppointmentCommand.Parameters.NewStartTime, GetStartDateTime())
                .SetParameter(UpdateAppointmentCommand.Parameters.DurationMinutes, DurationMinutes)
                .SetParameter(UpdateAppointmentCommand.Parameters.ReasonForVisit, Reason ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(Notes))
            {
                parameters.SetParameter(UpdateAppointmentCommand.Parameters.Notes, Notes);
            }

            return parameters;
        }

        private void LoadAppointment()
        {
            try
            {
                var allAppointments = _scheduleManager.GetAllAppointments();
                _appointment = allAppointments.FirstOrDefault(a => a.Id == _appointmentId);

                if (_appointment == null)
                {
                    ValidationErrors.Add("Appointment not found");
                    return;
                }

                // Populate form with existing appointment data
                SelectedPatient = AvailablePatients.FirstOrDefault(p => p.Id == _appointment.PatientId);
                SelectedPhysician = AvailablePhysicians.FirstOrDefault(p => p.Id == _appointment.PhysicianId);
                SelectedDate = _appointment.Start.Date;
                SelectedTime = _appointment.Start.TimeOfDay;
                DurationMinutes = (int)(_appointment.End - _appointment.Start).TotalMinutes;
                Reason = _appointment.ReasonForVisit ?? "General Consultation";
                Notes = _appointment.Notes ?? string.Empty;
            }
            catch (Exception ex)
            {
                AddValidationError($"Error loading appointment: {GetExceptionMessage(ex)}");
            }
        }
    }
}

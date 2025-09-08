namespace Core.CliniCore.Domain.Enumerations
{
    public enum Permission
    {
        // patient permissions
        ViewOwnProfile,
        EditOwnProfile,
        ViewOwnAppointments,
        ScheduleOwnAppointment,
        ViewOwnClinicalDocuments,

        // physician permissions
        ViewAllPatients,
        CreatePatientProfile,
        ViewPatientProfile,
        CreateClinicalDocument,
        ViewAllAppointments,
        ScheduleAnyAppointment,
        EditOwnAvailability,

        // admin permissions
        CreatePhysicianProfile,
        ViewSystemReports,
        EditFacilitySettings
    }
}
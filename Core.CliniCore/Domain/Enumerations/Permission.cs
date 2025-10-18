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
        UpdatePatientProfile,
        DeletePatientProfile,
        ViewPhysicianProfile,
        CreateClinicalDocument,
        UpdateClinicalDocument,
        DeleteClinicalDocument,
        ViewAllAppointments,
        ScheduleAnyAppointment,
        EditOwnAvailability,

        // admin permissions
        CreatePhysicianProfile,
        UpdatePhysicianProfile,
        DeletePhysicianProfile,
        ViewAdministratorProfile,
        UpdateAdministratorProfile,
        ViewAllProfiles,
        ViewSystemReports,
        EditFacilitySettings
    }
}
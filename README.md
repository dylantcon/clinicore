# The CliniCore System
## Introduction
CliniCore is intended to act as an EMR and practice management software, handling major medical practice business logic. This includes patient management with support for flexible onboarding procedures, quick storage and retrieval of medical notes and records, and an intuitive and dependable prescription tracking system. 

CliniCore was built around the physicians who use it, with an easy and reliable scheduler ensuring every appointment goes smoothly. The terminal interface has received special care and attention; interaction with the system is extremely straightforward. Regardless of whether you're a system administrator, physician, or patient, navigating and using CliniCore will be a breeze.

## Table of Contents
- [`API.CliniCore/`](#apiclinicore)
  - [`Controllers/`](#controllers)
    - [WeatherForecastController.cs](#weatherforecastcontrollercs)
  - [Program.cs](#programcs)
  - [`Properties/`](#properties)
  - [WeatherForecast.cs](#weatherforecastcs)
- [`CLI.CliniCore/`](#cliclinicore)
  - [`Documentation/`](#documentation)
  - [Program.cs](#programcs)
  - [`Service/`](#service)
    - [AbstractConsoleEngine.cs](#abstractconsoleenginecs)
    - [ConsoleCommandParser.cs](#consolecommandparsercs)
    - [ConsoleMenuBuilder.cs](#consolemenubuildercs)
    - [ConsoleSessionManager.cs](#consolesessionmanagercs)
    - [`Editor/`](#editor)
      - [ClinicalDocumentEditor.cs](#clinicaldocumenteditorcs)
      - [DocumentTreeView.cs](#documenttreeviewcs)
      - [EditorKeyHandler.cs](#editorkeyhandlercs)
      - [EditorRenderer.cs](#editorrenderercs)
      - [EditorState.cs](#editorstatecs)
      - [RenderZone.cs](#renderzonecs)
      - [StatusBarInputHandler.cs](#statusbarinputhandlercs)
      - [ZonedRenderer.cs](#zonedrenderercs)
    - [IConsoleEngine.cs](#iconsoleenginecs)
    - [ServiceContainer.cs](#servicecontainercs)
    - [ThreadSafeConsoleManager.cs](#threadsafeconsolemanagercs)
    - [TTYConsoleEngine.cs](#ttyconsoleenginecs)
    - [UserInputCancelledException.cs](#userinputcancelledexceptioncs)
- [`Core.CliniCore/`](#coreclinicore)
  - [`Bootstrap/`](#bootstrap)
    - [CoreServiceBootstrapper.cs](#coreservicebootstrappercs)
  - [`ClinicalDoc/`](#clinicaldoc)
    - [AbstractClinicalEntry.cs](#abstractclinicalentrycs)
    - [AssessmentEntry.cs](#assessmententrycs)
    - [ClinicalDocument.cs](#clinicaldocumentcs)
    - [DiagnosisEntry.cs](#diagnosisentrycs)
    - [ObservationEntry.cs](#observationentrycs)
    - [PlanEntry.cs](#planentrycs)
    - [PrescriptionEntry.cs](#prescriptionentrycs)
  - [`Commands/`](#commands)
    - [AbstractCommand.cs](#abstractcommandcs)
    - [`Admin/`](#admin)
      - [CreateFacilityCommand.cs](#createfacilitycommandcs)
      - [ManageUserRolesCommand.cs](#manageuserrolescommandcs)
      - [SystemMaintenanceCommand.cs](#systemmaintenancecommandcs)
      - [UpdateFacilitySettingsCommand.cs](#updatefacilitysettingscommandcs)
      - [ViewAuditLogCommand.cs](#viewauditlogcommandcs)
    - [`Authentication/`](#authentication)
      - [ChangePasswordCommand.cs](#changepasswordcommandcs)
      - [LoginCommand.cs](#logincommandcs)
      - [LogoutCommand.cs](#logoutcommandcs)
    - [`Clinical/`](#clinical)
      - [AddAssessmentCommand.cs](#addassessmentcommandcs)
      - [AddDiagnosisCommand.cs](#adddiagnosiscommandcs)
      - [AddObservationCommand.cs](#addobservationcommandcs)
      - [AddPlanCommand.cs](#addplancommandcs)
      - [AddPrescriptionCommand.cs](#addprescriptioncommandcs)
      - [CreateClinicalDocumentCommand.cs](#createclinicaldocumentcommandcs)
      - [DeleteClinicalDocumentCommand.cs](#deleteclinicaldocumentcommandcs)
      - [ListClinicalDocumentsCommand.cs](#listclinicaldocumentscommandcs)
      - [UpdateAssessmentCommand.cs](#updateassessmentcommandcs)
      - [UpdateClinicalDocumentCommand.cs](#updateclinicaldocumentcommandcs)
      - [UpdateDiagnosisCommand.cs](#updatediagnosiscommandcs)
      - [UpdateObservationCommand.cs](#updateobservationcommandcs)
      - [UpdatePlanCommand.cs](#updateplancommandcs)
      - [UpdatePrescriptionCommand.cs](#updateprescriptioncommandcs)
      - [ViewClinicalDocumentCommand.cs](#viewclinicaldocumentcommandcs)
    - [CommandFactory.cs](#commandfactorycs)
    - [CommandInvoker.cs](#commandinvokercs)
    - [CommandParameters.cs](#commandparameterscs)
    - [CommandResult.cs](#commandresultcs)
    - [CommandValidationResult.cs](#commandvalidationresultcs)
    - [ICommand.cs](#icommandcs)
    - [`Profile/`](#profile)
      - [AssignPatientToPhysicianCommand.cs](#assignpatienttophysiciancommandcs)
      - [CreateAdministratorCommand.cs](#createadministratorcommandcs)
      - [CreatePatientCommand.cs](#createpatientcommandcs)
      - [CreatePhysicianCommand.cs](#createphysiciancommandcs)
      - [DeleteProfileCommand.cs](#deleteprofilecommandcs)
      - [ListPatientsCommand.cs](#listpatientscommandcs)
      - [ListPhysiciansCommand.cs](#listphysicianscommandcs)
      - [ListProfileCommand.cs](#listprofilecommandcs)
      - [UpdateAdministratorProfileCommand.cs](#updateadministratorprofilecommandcs)
      - [UpdatePatientProfileCommand.cs](#updatepatientprofilecommandcs)
      - [UpdatePhysicianProfileCommand.cs](#updatephysicianprofilecommandcs)
      - [UpdateProfileCommand.cs](#updateprofilecommandcs)
      - [ViewAdministratorProfileCommand.cs](#viewadministratorprofilecommandcs)
      - [ViewPatientProfileCommand.cs](#viewpatientprofilecommandcs)
      - [ViewPhysicianProfileCommand.cs](#viewphysicianprofilecommandcs)
      - [ViewProfileCommand.cs](#viewprofilecommandcs)
    - [`Query/`](#query)
      - [FindPhysiciansByAvailabilityCommand.cs](#findphysiciansbyavailabilitycommandcs)
      - [FindPhysiciansBySpecializationCommand.cs](#findphysiciansbyspecializationcommandcs)
      - [GetScheduleCommand.cs](#getschedulecommandcs)
      - [ListAllUsersCommand.cs](#listalluserscommandcs)
      - [SearchClinicalNotesCommand.cs](#searchclinicalnotescommandcs)
      - [SearchPatientsCommand.cs](#searchpatientscommandcs)
    - [`Reports/`](#reports)
      - [GenerateAppointmentReportCommand.cs](#generateappointmentreportcommandcs)
      - [GenerateFacilityReportCommand.cs](#generatefacilityreportcommandcs)
      - [GeneratePatientReportCommand.cs](#generatepatientreportcommandcs)
      - [GeneratePhysicianReportCommand.cs](#generatephysicianreportcommandcs)
    - [`Scheduling/`](#scheduling)
      - [CancelAppointmentCommand.cs](#cancelappointmentcommandcs)
      - [CheckConflictsCommand.cs](#checkconflictscommandcs)
      - [DeleteAppointmentCommand.cs](#deleteappointmentcommandcs)
      - [GetAvailableTimeSlotsCommand.cs](#getavailabletimeslotscommandcs)
      - [ListAppointmentsCommand.cs](#listappointmentscommandcs)
      - [ScheduleAppointmentCommand.cs](#scheduleappointmentcommandcs)
      - [SetPhysicianAvailabilityCommand.cs](#setphysicianavailabilitycommandcs)
      - [UpdateAppointmentCommand.cs](#updateappointmentcommandcs)
      - [ViewAppointmentCommand.cs](#viewappointmentcommandcs)
  - [`Domain/`](#domain)
    - [AbstractUserProfile.cs](#abstractuserprofilecs)
    - [AdministratorProfile.cs](#administratorprofilecs)
    - [`Authentication/`](#authentication)
      - [AuthenticationException.cs](#authenticationexceptioncs)
      - [BasicAuthenticationService.cs](#basicauthenticationservicecs)
      - [IAuthenticationService.cs](#iauthenticationservicecs)
      - [RoleBasedAuthorizationService.cs](#rolebasedauthorizationservicecs)
      - [SessionContext.cs](#sessioncontextcs)
    - [`Enumerations/`](#enumerations)
      - [AppointmentStatus.cs](#appointmentstatuscs)
      - [EntryKeyResolver.cs](#entrykeyresolvercs)
      - [`EntryTypes/`](#entrytypes)
        - [AdministratorEntryType.cs](#administratorentrytypecs)
        - [CommonEntryType.cs](#commonentrytypecs)
        - [PatientEntryType.cs](#patiententrytypecs)
        - [PhysicianEntryType.cs](#physicianentrytypecs)
      - [`Extensions/`](#extensions)
        - [AdministratorEntryTypeExtensions.cs](#administratorentrytypeextensionscs)
        - [CommonEntryTypeExtensions.cs](#commonentrytypeextensionscs)
        - [GenderExtensions.cs](#genderextensionscs)
        - [MedicalSpecializationExtensions.cs](#medicalspecializationextensionscs)
        - [PatientEntryTypeExtensions.cs](#patiententrytypeextensionscs)
        - [PhysicianEntryTypeExtensions.cs](#physicianentrytypeextensionscs)
      - [Gender.cs](#gendercs)
      - [MedicalSpecialization.cs](#medicalspecializationcs)
      - [Permission.cs](#permissioncs)
      - [UserRole.cs](#userrolecs)
    - [IIdentifiable.cs](#iidentifiablecs)
    - [IUserProfile.cs](#iuserprofilecs)
    - [PatientProfile.cs](#patientprofilecs)
    - [PhysicianProfile.cs](#physicianprofilecs)
    - [ProfileBuilder.cs](#profilebuildercs)
    - [ProfileEntry.cs](#profileentrycs)
    - [ProfileEntryFactory.cs](#profileentryfactorycs)
    - [`ProfileTemplates/`](#profiletemplates)
      - [AbstractProfileTemplate.cs](#abstractprofiletemplatecs)
      - [AdministratorProfileTemplate.cs](#administratorprofiletemplatecs)
      - [IProfileTemplate.cs](#iprofiletemplatecs)
      - [PatientProfileTemplate.cs](#patientprofiletemplatecs)
      - [PhysicianProfileTemplate.cs](#physicianprofiletemplatecs)
    - [`Validation/`](#validation)
      - [AbstractValidator.cs](#abstractvalidatorcs)
      - [CompositeValidator.cs](#compositevalidatorcs)
      - [IValidator.cs](#ivalidatorcs)
      - [ValidatorFactory.cs](#validatorfactorycs)
      - [`Validators/`](#validators)
        - [DateRangeValidator.cs](#daterangevalidatorcs)
        - [EnumValidator.cs](#enumvalidatorcs)
        - [ListValidator.cs](#listvalidatorcs)
        - [RegexValidator.cs](#regexvalidatorcs)
        - [RequiredValidator.cs](#requiredvalidatorcs)
        - [StringLengthValidator.cs](#stringlengthvalidatorcs)
        - [UsernameUniquenessValidator.cs](#usernameuniquenessvalidatorcs)
  - [`DTO/`](#dto)
    - [PatientDTO.cs](#patientdtocs)
  - [`Facilities/`](#facilities)
    - [Facility.cs](#facilitycs)
    - [HealthSystem.cs](#healthsystemcs)
  - [`Repositories/`](#repositories)
    - [`InMemory/`](#inmemory)
      - [InMemoryAdministratorRepository.cs](#inmemoryadministratorrepositorycs)
      - [InMemoryAppointmentRepository.cs](#inmemoryappointmentrepositorycs)
      - [InMemoryClinicalDocumentRepository.cs](#inmemoryclinicaldocumentrepositorycs)
      - [InMemoryPatientRepository.cs](#inmemorypatientrepositorycs)
      - [InMemoryPhysicianRepository.cs](#inmemoryphysicianrepositorycs)
      - [InMemoryRepositoryBase.cs](#inmemoryrepositorybasecs)
    - [IAdministratorRepository.cs](#iadministratorrepositorycs)
    - [IAppointmentRepository.cs](#iappointmentrepositorycs)
    - [IClinicalDocumentRepository.cs](#iclinicaldocumentrepositorycs)
    - [IPatientRepository.cs](#ipatientrepositorycs)
    - [IRepository.cs](#irepositorycs)
  - [`Scheduling/`](#scheduling)
    - [AbstractTimeInterval.cs](#abstracttimeintervalcs)
    - [AppointmentTimeInterval.cs](#appointmenttimeintervalcs)
    - [`BookingStrategies/`](#bookingstrategies)
      - [FirstAvailableBookingStrategy.cs](#firstavailablebookingstrategycs)
      - [IBookingStrategy.cs](#ibookingstrategycs)
    - [ITimeInterval.cs](#itimeintervalcs)
    - [`Management/`](#management)
      - [ScheduleConflictDetector.cs](#scheduleconflictdetectorcs)
      - [SchedulingService.cs](#schedulingservicecs)
    - [PhysicianSchedule.cs](#physicianschedulecs)
    - [UnavailableTimeInterval.cs](#unavailabletimeintervalcs)
  - [`Services/`](#services)
    - [ClinicalDocumentService.cs](#clinicaldocumentservicecs)
    - [ProfileService.cs](#profileservicecs)
    - [SchedulerService.cs](#schedulerservicecs)
- [`GUI.CliniCore/`](#guiclinicore)
  - [App.xaml](#appxaml)
  - [App.xaml.cs](#appxamlcs)
  - [AppShell.xaml](#appshellxaml)
  - [AppShell.xaml.cs](#appshellxamlcs)
  - [`Commands/`](#commands)
    - [CommandParameterConverter.cs](#commandparameterconvertercs)
    - [MauiCommandAdapter.cs](#mauicommandadaptercs)
    - [RelayCommand.cs](#relaycommandcs)
  - [`Converters/`](#converters)
    - [PhysicianAssignmentConverter.cs](#physicianassignmentconvertercs)
  - [MainPage.xaml](#mainpagexaml)
  - [MainPage.xaml.cs](#mainpagexamlcs)
  - [MauiProgram.cs](#mauiprogramcs)
  - [`Properties/`](#properties)
  - [`Services/`](#services)
    - [HomeViewModelFactory.cs](#homeviewmodelfactorycs)
    - [IHomeViewModelFactory.cs](#ihomeviewmodelfactorycs)
    - [INavigationService.cs](#inavigationservicecs)
    - [NavigationService.cs](#navigationservicecs)
    - [SessionManager.cs](#sessionmanagercs)
  - [`ViewModels/`](#viewmodels)
    - [AdministratorEditViewModel.cs](#administratoreditviewmodelcs)
    - [AdministratorHomeViewModel.cs](#administratorhomeviewmodelcs)
    - [AppointmentDetailViewModel.cs](#appointmentdetailviewmodelcs)
    - [AppointmentEditViewModel.cs](#appointmenteditviewmodelcs)
    - [AppointmentFormViewModelBase.cs](#appointmentformviewmodelbasecs)
    - [AppointmentListViewModel.cs](#appointmentlistviewmodelcs)
    - [BaseViewModel.cs](#baseviewmodelcs)
    - [ClinicalDocumentDetailViewModel.cs](#clinicaldocumentdetailviewmodelcs)
    - [ClinicalDocumentEditViewModel.cs](#clinicaldocumenteditviewmodelcs)
    - [ClinicalDocumentListViewModel.cs](#clinicaldocumentlistviewmodelcs)
    - [CreateAppointmentViewModel.cs](#createappointmentviewmodelcs)
    - [InvertedBoolConverter.cs](#invertedboolconvertercs)
    - [IsNotNullConverter.cs](#isnotnullconvertercs)
    - [IsStringNotNullOrEmptyConverter.cs](#isstringnotnulloremptyconvertercs)
    - [LoginViewModel.cs](#loginviewmodelcs)
    - [PatientDetailViewModel.cs](#patientdetailviewmodelcs)
    - [PatientEditViewModel.cs](#patienteditviewmodelcs)
    - [PatientHomeViewModel.cs](#patienthomeviewmodelcs)
    - [PatientListViewModel.cs](#patientlistviewmodelcs)
    - [PhysicianDetailViewModel.cs](#physiciandetailviewmodelcs)
    - [PhysicianEditViewModel.cs](#physicianeditviewmodelcs)
    1- [PhysicianHomeViewModel.cs](#physicianhomeviewmodelcs)
    - [PhysicianListViewModel.cs](#physicianlistviewmodelcs)
    - [SpecializationsConverter.cs](#specializationsconvertercs)
    - [StubViewModel.cs](#stubviewmodelcs)
    - [UserListViewModel.cs](#userlistviewmodelcs)
  - [`Views/`](#views)
    - [AdministratorEditPage.xaml](#administratoreditpagexaml)
    - [AdministratorEditPage.xaml.cs](#administratoreditpagexamlcs)
    - [AppointmentDetailPage.xaml](#appointmentdetailpagexaml)
    - [AppointmentDetailPage.xaml.cs](#appointmentdetailpagexamlcs)
    - [AppointmentEditPage.xaml](#appointmenteditpagexaml)
    - [AppointmentEditPage.xaml.cs](#appointmenteditpagexamlcs)
    - [AppointmentListPage.xaml](#appointmentlistpagexaml)
    - [AppointmentListPage.xaml.cs](#appointmentlistpagexamlcs)
    - [ClinicalDocumentDetailPage.xaml](#clinicaldocumentdetailpagexaml)
    - [ClinicalDocumentDetailPage.xaml.cs](#clinicaldocumentdetailpagexamlcs)
    - [ClinicalDocumentEditPage.xaml](#clinicaldocumenteditpagexaml)
    - [ClinicalDocumentEditPage.xaml.cs](#clinicaldocumenteditpagexamlcs)
    - [ClinicalDocumentListPage.xaml](#clinicaldocumentlistpagexaml)
    - [ClinicalDocumentListPage.xaml.cs](#clinicaldocumentlistpagexamlcs)
    - [CreateAppointmentPage.xaml](#createappointmentpagexaml)
    - [CreateAppointmentPage.xaml.cs](#createappointmentpagexamlcs)
    - [HomePage.xaml](#homepagexaml)
    - [HomePage.xaml.cs](#homepagexamlcs)
    - [LoginPage.xaml](#loginpagexaml)
    - [LoginPage.xaml.cs](#loginpagexamlcs)
    - [PatientDetailPage.xaml](#patientdetailpagexaml)
    - [PatientDetailPage.xaml.cs](#patientdetailpagexamlcs)
    - [PatientEditPage.xaml](#patienteditpagexaml)
    - [PatientEditPage.xaml.cs](#patienteditpagexamlcs)
    - [PatientListPage.xaml](#patientlistpagexaml)
    - [PatientListPage.xaml.cs](#patientlistpagexamlcs)
    - [PhysicianDetailPage.xaml](#physiciandetailpagexaml)
    - [PhysicianDetailPage.xaml.cs](#physiciandetailpagexamlcs)
    - [PhysicianEditPage.xaml](#physicianeditpagexaml)
    - [PhysicianEditPage.xaml.cs](#physicianeditpagexamlcs)
    - [PhysicianListPage.xaml](#physicianlistpagexaml)
    - [PhysicianListPage.xaml.cs](#physicianlistpagexamlcs)
    - [RoleBasedContentTemplateSelector.cs](#rolebasedcontenttemplateselectorcs)
    - [StubPage.xaml](#stubpagexaml)
    - [StubPage.xaml.cs](#stubpagexamlcs)
    - [UserListPage.xaml](#userlistpagexaml)
    - [UserListPage.xaml.cs](#userlistpagexamlcs)
## API.CliniCore
The API.CliniCore project is a RESTful web API built with ASP.NET Core 8.0 that serves as the HTTP interface for the CliniCore system. Currently in initial development stage with template scaffolding, this project is designed to expose CliniCore's medical practice management functionality through REST endpoints with Swagger/OpenAPI documentation support.

### Program.cs
Entry point for the ASP.NET Core Web API that configures the application builder with controller support, Swagger/OpenAPI documentation generation for API exploration, and authorization middleware. This minimal API configuration serves as the foundation for future RESTful endpoints that will expose Core.CliniCore services for patient management, scheduling, clinical documentation, and authentication over HTTP, enabling integration with third-party systems and mobile applications.

## Controllers
The Controllers directory contains API endpoint implementations following the MVC pattern, exposing RESTful HTTP operations for CliniCore functionality.

### WeatherForecastController.cs
A template controller demonstrating basic REST API structure with a single GET endpoint at /WeatherForecast that returns randomly generated weather data. This placeholder controller serves as scaffolding from the ASP.NET Core Web API template and will be replaced with actual CliniCore controllers for patients, physicians, appointments, and clinical documents. It demonstrates dependency injection for ILogger, attribute-based routing with [ApiController] and [Route], and standard REST GET operation patterns that future controllers will follow.

### WeatherForecast.cs
A simple data transfer object (DTO) representing weather forecast data with properties for date, temperatures (Celsius and Fahrenheit), and weather summary. This template model class demonstrates the DTO pattern that will be used for API request/response serialization when integrating with Core.CliniCore domain models. Future DTOs will handle patient profiles, appointment details, clinical documents, and authentication tokens for secure data transfer between the API and client applications.

## Properties
The Properties directory contains launch configuration and environment settings for running the API during development.

### launchSettings.json
Defines development environment launch profiles for the API including HTTP settings (port 5059), IIS Express configuration (port 5143), and Swagger UI as the default launch URL. This configuration file specifies that the application runs in Development mode with automatic browser launch to the Swagger documentation interface, anonymous authentication enabled, and detailed ASP.NET Core logging for debugging. The settings ensure consistent local development experience and easy API testing through the interactive Swagger interface.

## Configuration Files

### appsettings.json
Main application configuration file defining logging levels (Information for general logs, Warning for ASP.NET Core framework logs) and allowed hosts set to wildcard for development flexibility. This file will be extended to include database connection strings, authentication settings, CORS policies, and Core.CliniCore service configuration as the API implementation progresses beyond the template stage.

### appsettings.Development.json
Development-specific configuration override that maintains the same logging configuration as the base appsettings.json file. This file is loaded only in Development environment and can be used to override production settings with development-specific values such as detailed logging, test database connections, or relaxed security policies without affecting production configuration.

## API.CliniCore.csproj
Project file targeting .NET 8.0 with nullable reference types enabled and implicit usings for cleaner code. Currently includes only the Swashbuckle.AspNetCore package (version 6.6.2) for Swagger/OpenAPI documentation generation. Future package references will include Entity Framework Core for data persistence, authentication libraries for JWT token handling, and a project reference to Core.CliniCore for business logic integration.
## CLI.CliniCore
The CLI.CliniCore project provides a terminal-based interface for the CliniCore medical practice management system. It implements a service-oriented architecture with menu-driven navigation, interactive command parsing, session management, and a comprehensive text-based document editor for clinical records.

### Program.cs
Entry point for the CLI application that initializes the ServiceContainer with optional development data in DEBUG mode and starts the console engine. This file handles fatal error catching, ensures proper resource cleanup on shutdown, and provides a clear separation between debug and production environments through conditional compilation.

**Inheritance:** None

**Properties:** None

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `static void Main(string[] args)` | void | Entry point method that creates ServiceContainer, handles fatal errors with console output, and ensures cleanup via Dispose |

## Service
The Service layer implements the core console UI infrastructure, command orchestration, and session management for the CLI application.

### IConsoleEngine.cs
Defines the contract for console engines with core operations including menu display, user input handling (both standard and secure password input), message display with color-coded types, table rendering, confirmation dialogs, and breadcrumb navigation. This interface also includes helper types (MessageType, ConsoleMenu, ConsoleMenuItem) that standardize console UI patterns across the application and ensure consistent user experience.

**Inheritance:** Interface

**Enumerations:**
- **MessageType:** Info, Success, Warning, Error, Debug

**Helper Classes:**
- **ConsoleMenu:** Properties include Title, Subtitle, Items (List&lt;ConsoleMenuItem&gt;), HelpText, ShowBackOption, Prompt
- **ConsoleMenuItem:** Properties include Key, Label, Description, Action, SubMenuFactory, IsVisible, IsEnabled, Color

**Methods:**

| Signature                                                                                                   | Returns | Description                                    |
| ----------------------------------------------------------------------------------------------------------- | ------- | ---------------------------------------------- |
| `void Start()`                                                                                              | void    | Starts the console engine main loop            |
| `void Stop()`                                                                                               | void    | Stops the console engine and shuts down        |
| `void DisplayMenu(ConsoleMenu menu)`                                                                        | void    | Displays a menu and handles user selection     |
| `string? GetUserInput(string prompt)`                                                                       | string? | Prompts for and returns user input             |
| `string? GetSecureInput(string prompt)`                                                                     | string? | Prompts for and returns masked password input  |
| `void DisplayMessage(string message, MessageType type = MessageType.Info)`                                  | void    | Displays a color-coded message                 |
| `void Clear()`                                                                                              | void    | Clears the console screen                      |
| `void DisplayTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> ValueGetter)[] columns)` | void    | Renders a formatted table with dynamic columns |
| `bool Confirm(string prompt, bool defaultValue = false)`                                                    | bool    | Displays a yes/no confirmation prompt          |
| `void DisplayHeader(string text)`                                                                           | void    | Displays a formatted header with border        |
| `void DisplaySeparator()`                                                                                   | void    | Displays a horizontal separator line           |
| `void Pause(string message = "Press any key to continue...")`                                               | void    | Pauses execution until key press               |
| `void SetColor(ConsoleColor foreground, ConsoleColor? background = null)`                                   | void    | Sets console colors                            |
| `void ResetColor()`                                                                                         | void    | Resets console colors to default               |
| `string GetBreadcrumb()`                                                                                    | string  | Returns current navigation breadcrumb path     |
| `void PushBreadcrumb(string crumb)`                                                                         | void    | Adds a breadcrumb to navigation stack          |
| `void PopBreadcrumb()`                                                                                      | void    | Removes last breadcrumb from navigation stack  |

### AbstractConsoleEngine.cs
Abstract base implementation of IConsoleEngine providing common console UI functionality shared across different console engine types. This class implements the Template Method pattern, handling menu navigation with breadcrumb tracking, color-coded message display, table rendering with dynamic column widths, confirmation prompts, and a welcome screen display. It coordinates with ConsoleSessionManager for authentication state, ConsoleMenuBuilder for menu generation, and CommandInvoker for executing user commands while managing the application lifecycle and error handling.

**Inheritance:** IConsoleEngine, IDisposable

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _breadcrumbs | Stack&lt;string&gt; | protected | Navigation breadcrumb stack |
| _sessionManager | ConsoleSessionManager | protected | Manages user authentication and session state |
| _menuBuilder | ConsoleMenuBuilder | protected | Builds role-based menus |
| _commandInvoker | CommandInvoker | protected | Executes commands |
| _console | ThreadSafeConsoleManager | protected | Thread-safe console I/O wrapper |
| _cancellationTokenSource | CancellationTokenSource | protected | Cancellation token for shutdown |
| _isRunning | bool | protected | Indicates if engine is currently running |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `protected AbstractConsoleEngine(ConsoleSessionManager sessionManager, ConsoleMenuBuilder? menuBuilder, CommandInvoker commandInvoker)` | - | Constructor accepting core dependencies |
| `void SetMenuBuilder(ConsoleMenuBuilder menuBuilder)` | void | Sets menu builder after construction (resolves circular dependency) |
| `virtual void Start()` | void | Starts main application loop with menu display |
| `virtual void Stop()` | void | Stops the engine and cancels operations |
| `virtual void DisplayMenu(ConsoleMenu menu)` | void | Displays menu, handles selection, and executes actions/submenus |
| `abstract string? GetUserInput(string prompt)` | string? | Abstract method for getting user input (implemented by derived classes) |
| `abstract string? GetSecureInput(string prompt)` | string? | Abstract method for getting secure password input |
| `virtual void DisplayMessage(string message, MessageType type = MessageType.Info)` | void | Displays color-coded message based on type |
| `virtual void Clear()` | void | Clears console screen |
| `virtual void DisplayTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> ValueGetter)[] columns)` | void | Renders formatted table with dynamic column widths |
| `virtual bool Confirm(string prompt, bool defaultValue = false)` | bool | Shows yes/no confirmation dialog |
| `virtual void DisplayHeader(string text)` | void | Displays bordered header with cyan color |
| `virtual void DisplaySeparator()` | void | Displays horizontal line separator |
| `virtual void Pause(string message = "Press any key to continue...")` | void | Waits for key press |
| `virtual void SetColor(ConsoleColor foreground, ConsoleColor? background = null)` | void | Sets console foreground and optional background color |
| `virtual void ResetColor()` | void | Resets console colors to defaults |
| `virtual string GetBreadcrumb()` | string | Returns breadcrumb navigation path |
| `virtual void PushBreadcrumb(string crumb)` | void | Pushes breadcrumb onto navigation stack |
| `virtual void PopBreadcrumb()` | void | Pops breadcrumb from navigation stack |
| `protected virtual void DisplayWelcome()` | void | Displays ASCII art welcome banner |
| `protected virtual (ConsoleColor foreground, ConsoleColor? background) GetMessageColors(MessageType type)` | (ConsoleColor, ConsoleColor?) | Maps message type to console colors |
| `virtual void Dispose()` | void | Disposes resources and cancels operations |

### TTYConsoleEngine.cs
Concrete console engine implementation for TTY (terminal) environments that extends AbstractConsoleEngine with interactive keyboard input handling. This class implements character-by-character input capture with support for Escape key cancellation, backspace editing, and secure password masking, while respecting input redirection for automated testing scenarios. It works with ConsoleCommandParser to translate user input into command parameters and manages console mode transitions between menu navigation, text editing, and input collection states.

**Inheritance:** AbstractConsoleEngine

**Properties:**

| Name           | Type                 | Access  | Description                           |
| -------------- | -------------------- | ------- | ------------------------------------- |
| _commandParser | ConsoleCommandParser | private | Parses interactive command parameters |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `TTYConsoleEngine(ConsoleMenuBuilder? menuBuilder, CommandInvoker commandInvoker, ConsoleSessionManager sessionManager, ConsoleCommandParser? commandParser)` | - | Constructor accepting dependencies (some nullable for late binding) |
| `void SetCommandParser(ConsoleCommandParser commandParser)` | void | Sets command parser after construction (resolves circular dependency) |
| `override string? GetUserInput(string prompt)` | string? | Interactive input with Escape cancellation, backspace support, character-by-character capture |
| `override string? GetSecureInput(string prompt)` | string? | Masked password input with asterisks, Escape cancellation, and backspace support |
| `override void DisplaySeparator()` | void | Renders separator with dynamic width based on console dimensions |

### ServiceContainer.cs
Dependency injection container using Microsoft.Extensions.DependencyInjection that integrates with CoreServiceBootstrapper to provide consistent service configuration across CLI, GUI, and API applications. This sealed class manages the lifecycle of all CLI-specific services (ConsoleSessionManager, ConsoleMenuBuilder, ConsoleCommandParser, TTYConsoleEngine) and core CliniCore services (authentication, scheduling, commands), resolving circular dependencies through late binding and providing centralized service access. It includes factory methods for creating configured containers with optional development data initialization.

**Inheritance:** IDisposable

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _serviceProvider | ServiceProvider | private | Microsoft DI service provider |
| _authService | IAuthenticationService | private | Authentication service instance |
| _scheduleManager | SchedulingService | private | Schedule management service |
| _commandFactory | CommandFactory | private | Creates command instances |
| _commandInvoker | CommandInvoker | private | Executes commands |
| _sessionManager | ConsoleSessionManager | private | Manages CLI session state |
| _commandParser | ConsoleCommandParser | private | Parses interactive commands |
| _menuBuilder | ConsoleMenuBuilder | private | Builds role-based menus |
| _consoleEngine | TTYConsoleEngine | private | TTY console engine implementation |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `private ServiceContainer(ServiceProvider serviceProvider, IAuthenticationService authService, SchedulingService scheduleManager, CommandFactory commandFactory, CommandInvoker commandInvoker, ConsoleSessionManager sessionManager, ConsoleCommandParser commandParser, ConsoleMenuBuilder menuBuilder, TTYConsoleEngine consoleEngine)` | - | Private constructor for dependency injection setup |
| `static ServiceContainer Create(bool includeDevelopmentData = false)` | ServiceContainer | Factory method creating configured container with optional dev data |
| `TTYConsoleEngine GetConsoleEngine()` | TTYConsoleEngine | Returns the console engine instance |
| `IAuthenticationService GetAuthenticationService()` | IAuthenticationService | Returns the authentication service instance |
| `void Dispose()` | void | Disposes service provider and console engine |

### ConsoleSessionManager.cs
Manages user session state including authentication status, session timeouts (30-minute default), activity tracking, and permission validation for the CLI application. This class wraps the Core library's SessionContext and provides CLI-specific session operations like automatic expiration checking, role-based authorization, permission validation, and formatted session information display. It enforces security policies by validating session state before critical operations and provides helper methods for role and permission requirements.

**Inheritance:** None

**Properties:**

| Name              | Type            | Access           | Description                             |
| ----------------- | --------------- | ---------------- | --------------------------------------- |
| _currentSession   | SessionContext? | private          | Current active session context          |
| _lastActivityTime | DateTime?       | private          | Timestamp of last user activity         |
| _sessionTimeout   | TimeSpan        | private readonly | Session timeout duration (30 minutes)   |
| CurrentSession    | SessionContext? | get              | Returns current session context         |
| IsAuthenticated   | bool            | get              | True if session exists and not expired  |
| CurrentUsername   | string          | get              | Current user's username or "Guest"      |
| CurrentUserRole   | string          | get              | Current user's role as string or "None" |
| CurrentUserId     | Guid?           | get              | Current user's ID                       |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `void StartSession(SessionContext session)` | void | Initiates a new session with given context |
| `void EndSession()` | void | Terminates current session |
| `void UpdateActivity()` | void | Updates last activity timestamp |
| `bool IsSessionExpired()` | bool | Checks if session has exceeded timeout |
| `void ValidateSession()` | void | Validates session exists, not expired, updates activity |
| `bool HasPermission(Permission permission)` | bool | Checks if current session has specified permission |
| `string GetSessionInfo()` | string | Returns formatted session information string |
| `void RequireAuthentication()` | void | Throws UnauthorizedAccessException if not authenticated |
| `void RequireRole(UserRole role)` | void | Throws UnauthorizedAccessException if role doesn't match |
| `void RequirePermission(Permission permission)` | void | Throws UnauthorizedAccessException if permission not granted |

### ConsoleMenuBuilder.cs
Dynamically constructs role-based hierarchical menus for the CLI application based on authenticated user roles (Administrator, Physician, Patient). This class generates context-appropriate menu structures with options for user management, patient/physician management, scheduling, clinical documentation, and system administration, delegating command execution to ConsoleCommandParser and CommandInvoker. It implements menu factories for each functional area and manages special workflows like document editor launching, profile viewing, and report generation while respecting user permissions and session state.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _commandInvoker | CommandInvoker | private readonly | Executes commands |
| _commandFactory | CommandFactory | private readonly | Creates command instances |
| _sessionManager | ConsoleSessionManager | private readonly | Manages session state |
| _commandParser | ConsoleCommandParser | private readonly | Parses interactive parameters |
| _console | IConsoleEngine | private readonly | Console engine for display |
| _commandKeyCache | Dictionary&lt;Type, string&gt; | private | Caches command keys by type |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ConsoleMenuBuilder(CommandInvoker commandInvoker, CommandFactory commandFactory, ConsoleSessionManager sessionManager, ConsoleCommandParser commandParser, IConsoleEngine console)` | - | Constructor accepting all dependencies |
| `ConsoleMenu BuildMainMenu()` | ConsoleMenu | Builds main menu based on authentication state and user role |
| `private void AddAdministratorMenuItems(ConsoleMenu menu)` | void | Adds administrator-specific menu options |
| `private void AddPhysicianMenuItems(ConsoleMenu menu)` | void | Adds physician-specific menu options |
| `private void AddPatientMenuItems(ConsoleMenu menu)` | void | Adds patient-specific menu options |
| `private ConsoleMenu BuildUserManagementMenu()` | ConsoleMenu | Creates user management submenu |
| `private ConsoleMenu BuildPatientManagementMenu()` | ConsoleMenu | Creates patient management submenu |
| `private ConsoleMenu BuildPhysicianManagementMenu()` | ConsoleMenu | Creates physician management submenu |
| `private ConsoleMenu BuildSchedulingMenu()` | ConsoleMenu | Creates scheduling submenu |
| `private ConsoleMenu BuildPhysicianSchedulingMenu()` | ConsoleMenu | Creates physician-specific scheduling submenu |
| `private ConsoleMenu BuildClinicalDocumentsMenu()` | ConsoleMenu | Creates clinical documents submenu with editor option |
| `private ConsoleMenu BuildSOAPMenu()` | ConsoleMenu | Creates SOAP note entry submenu |
| `private ConsoleMenu BuildSOAPUpdateMenu()` | ConsoleMenu | Creates SOAP entry update submenu |
| `private ConsoleMenu BuildReportsMenu()` | ConsoleMenu | Creates reports submenu (currently unutilized) |
| `private ConsoleMenu BuildPhysicianReportsMenu()` | ConsoleMenu | Creates physician reports submenu (currently unutilized) |
| `private ConsoleMenu BuildSystemAdminMenu()` | ConsoleMenu | Creates system administration submenu (currently unutilized) |
| `private void ExecuteCommand(string commandName)` | void | Executes command with interactive parameter parsing and result handling |
| `private void ViewOwnProfile()` | void | Displays current user's profile |
| `private void SetOwnAvailability()` | void | Sets current physician's availability |
| `private void GenerateOwnPhysicianReport()` | void | Generates report for current physician |
| `private void LaunchDocumentEditor()` | void | Launches clinical document editor for selected document |
| `private Guid GetClinicalDocumentSelection()` | Guid | Prompts user to select a clinical document |
| `private void ExecuteUpdateClinicalDocument()` | void | Updates clinical document chief complaint |
| `private void ExecuteFinalizeClinicalDocument()` | void | Finalizes and completes clinical document |

### ConsoleCommandParser.cs
Translates user interactions into CommandParameters objects by prompting for required command inputs through a series of type-specific input methods. This class implements comprehensive interactive parsing for all Core library commands, providing specialized input handlers for dates, times, GUIDs, enumerations (Gender, MedicalSpecialization, UserRole), and complex selections (patient/physician profiles, appointments, clinical documents) with visual table displays. It leverages Core library registries (Profileservice, ClinicalDocumentService, SchedulingService) to present contextual selection lists and validates input formats while supporting Escape key cancellation through UserInputCancelledException.

**Inheritance:** None (Primary constructor)

**Properties:**

| Name                      | Type                     | Access           | Description                         |
| ------------------------- | ------------------------ | ---------------- | ----------------------------------- |
| _console                  | IConsoleEngine           | private readonly | Console engine for user interaction |
| _profileservice          | Profileservice          | private readonly | service for user profiles          |
| _clinicalDocumentservice | ClinicalDocumentService | private readonly | service for clinical documents     |

**Methods:**

| Signature                                                                    | Returns                           | Description                                                 |
| ---------------------------------------------------------------------------- | --------------------------------- | ----------------------------------------------------------- |
| `ConsoleCommandParser(IConsoleEngine console)`                               | -                                 | Primary constructor accepting console engine                |
| `CommandParameters ParseInteractive(ICommand command)`                       | CommandParameters                 | Parses parameters for given command via interactive prompts |
| `private string GetStringInput(string prompt)`                               | string                            | Prompts for required string input                           |
| `private string? GetOptionalStringInput(string prompt)`                      | string?                           | Prompts for optional string input                           |
| `private string GetSecureInput(string prompt)`                               | string                            | Prompts for secure password input                           |
| `private DateTime GetDateInput(string prompt)`                               | DateTime                          | Prompts for date input with validation                      |
| `private DateTime? GetOptionalDateInput(string prompt)`                      | DateTime?                         | Prompts for optional date input                             |
| `private DateTime GetDateTimeInput(string prompt)`                           | DateTime                          | Prompts for date and time input                             |
| `private TimeOnly GetTimeInput(string prompt)`                               | TimeOnly                          | Prompts for time-only input                                 |
| `private Guid GetGuidInput(string prompt)`                                   | Guid                              | Prompts for GUID input with validation                      |
| `private Guid? GetOptionalGuidInput(string prompt)`                          | Guid?                             | Prompts for optional GUID input                             |
| `private int GetIntInput(string prompt, int defaultValue, int min, int max)` | int                               | Prompts for integer input with range validation             |
| `private bool GetBoolInput(string prompt)`                                   | bool                              | Prompts for yes/no boolean input                            |
| `private Gender GetGenderInput()`                                            | Gender                            | Prompts for gender selection from enumeration               |
| `private Gender? GetOptionalGenderInput()`                                   | Gender?                           | Prompts for optional gender selection                       |
| `private string GetRaceInput()`                                              | string                            | Prompts for race/ethnicity input                            |
| `private MedicalSpecialization GetSpecializationInput()`                     | MedicalSpecialization             | Prompts for medical specialization selection                |
| `private List<MedicalSpecialization> GetSpecializationListInput()`           | List&lt;MedicalSpecialization&gt; | Prompts for multiple specialization selections              |
| `private UserRole GetUserRoleInput()`                                        | UserRole                          | Prompts for user role selection                             |
| `private DayOfWeek GetDayOfWeekInput()`                                      | DayOfWeek                         | Prompts for day of week selection                           |
| `private string GetReportTypeInput()`                                        | string                            | Prompts for report type selection                           |
| `private Guid GetProfileSelection(UserRole? roleFilter = null)`              | Guid                              | Displays profile table and prompts for selection            |
| `private Guid GetAppointmentSelection(Guid? patientId = null)`               | Guid                              | Displays appointment table and prompts for selection        |
| `private Guid GetClinicalDocumentSelection()`                                | Guid                              | Displays clinical document table and prompts for selection  |
| `private void PromptForProfileUpdateFields(CommandParameters parameters)`    | void                              | Prompts for profile-specific update fields                  |

### ThreadSafeConsoleManager.cs
Thread-safe singleton that wraps all System.Console operations using ReaderWriterLockSlim to prevent race conditions between UI threads and background operations. This class implements dimension caching with timeout-based refresh to minimize lock contention, manages three console modes (Menu, Editor, Input) for coordinating exclusive console access, and provides safe cursor positioning with boundary checking. All console I/O operations (Write, WriteLine, ReadKey, ReadLine, color management) are synchronized through this manager to ensure visual consistency in multi-threaded scenarios.

**Inheritance:** IDisposable (sealed singleton)

**Enumerations:**
- **ConsoleMode:** Menu, Editor, Input

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _instanceLock | object | private static readonly | Lock for singleton instance creation |
| _instance | ThreadSafeConsoleManager? | private static | Singleton instance |
| _consoleLock | ReaderWriterLockSlim | private readonly | Read/write lock for console operations |
| _dimensionCacheLock | object | private readonly | Lock for dimension cache |
| _cachedWidth | int | private | Cached console width |
| _cachedHeight | int | private | Cached console height |
| _lastDimensionUpdate | DateTime | private | Timestamp of last dimension refresh |
| _cacheTimeout | TimeSpan | private readonly | Cache timeout (50ms) |
| _currentMode | ConsoleMode | private | Current console mode |
| Instance | ThreadSafeConsoleManager | get | Returns singleton instance |
| CurrentMode | ConsoleMode | get | Returns current console mode |
| IsInputRedirected | bool | get | Returns true if console input is redirected |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `private ThreadSafeConsoleManager()` | - | Private constructor for singleton |
| `void SetMode(ConsoleMode mode)` | void | Sets current console mode with write lock |
| `(int Width, int Height) GetDimensions()` | (int, int) | Returns cached dimensions, refreshes if cache expired |
| `(int Width, int Height) GetDimensionsForceRefresh()` | (int, int) | Forces dimension refresh and returns new values |
| `private void RefreshDimensions()` | void | Refreshes cached console dimensions |
| `void Clear()` | void | Clears console with write lock |
| `void Write(string text)` | void | Writes text to console with write lock |
| `void WriteLine(string text = "")` | void | Writes line to console with write lock |
| `ConsoleKeyInfo ReadKey(bool intercept = false)` | ConsoleKeyInfo | Reads key with write lock |
| `string? ReadLine()` | string? | Reads line with write lock |
| `void SetForegroundColor(ConsoleColor color)` | void | Sets foreground color with write lock |
| `void SetBackgroundColor(ConsoleColor color)` | void | Sets background color with write lock |
| `void ResetColor()` | void | Resets colors with write lock |
| `void SetCursorPosition(int left, int top)` | void | Sets cursor position with boundary checking and write lock |
| `(int Left, int Top) GetCursorPosition()` | (int, int) | Returns cursor position with read lock |
| `void Dispose()` | void | Disposes reader/writer lock |

### UserInputCancelledException.cs
Custom exception thrown when users press the Escape key to cancel input operations in the CLI application. This exception enables graceful cancellation of command input workflows, propagating up through ConsoleCommandParser to ConsoleMenuBuilder where it is caught and handled with appropriate user feedback. It provides a clear semantic distinction between validation errors and intentional user cancellations in the command execution pipeline.

**Inheritance:** Exception

**Properties:** None (inherits from Exception)

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `UserInputCancelledException()` | - | Default constructor with message "User cancelled input operation" |
| `UserInputCancelledException(string message)` | - | Constructor with custom message |
| `UserInputCancelledException(string message, Exception innerException)` | - | Constructor with custom message and inner exception |

## Editor
The Editor subsystem provides a comprehensive split-pane clinical document editor with tree navigation and real-time editing.

### ClinicalDocumentEditor.cs
Comprehensive text editor for clinical documents providing split-pane interface with tree navigation and real-time editing capabilities.

**Inheritance:** AbstractConsoleEngine

**Properties:**

| Name                        | Type                     | Access           | Description                                |
| --------------------------- | ------------------------ | ---------------- | ------------------------------------------ |
| _renderer                   | EditorRenderer           | private readonly | Handles rendering of editor interface      |
| _keyHandler                 | EditorKeyHandler         | private readonly | Processes keyboard input                   |
| _commandInvoker             | CommandInvoker           | private readonly | Executes commands                          |
| _commandParser              | ConsoleCommandParser     | private readonly | Parses command parameters                  |
| _editorState                | EditorState?             | private          | Current editor state                       |
| _resizeListenerTask         | Task?                    | private          | Background task monitoring terminal resize |
| _resizeListenerCancellation | CancellationTokenSource? | private          | Cancellation for resize listener           |
| _editorActive               | bool                     | private          | Indicates if editor is active              |
| _isPrompting                | bool                     | private          | Indicates if prompt dialog is active       |
| _currentPrompt              | string                   | private          | Current prompt message                     |
| _promptCallback             | Action&lt;bool&gt;?      | private          | Callback for prompt result                 |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ClinicalDocumentEditor(ConsoleSessionManager sessionManager, CommandInvoker commandInvoker, ConsoleCommandParser commandParser)` | - | Constructor accepting dependencies |
| `void EditDocument(ClinicalDocument document)` | void | Launches editor for specified clinical document |
| `private void EnterEditorLoop()` | void | Main editor input/rendering loop |
| `private bool TryReadKeyWithTimeout(out ConsoleKeyInfo key, int timeoutMs)` | bool | Non-blocking key read with timeout |
| `private void StartResizeListener()` | void | Starts background task monitoring terminal resize |
| `private void ExitEditor()` | void | Cleans up editor resources and restores console mode |
| `private void DisplayWelcomeMessage()` | void | Displays editor welcome message |
| `private bool IsSystemKey(ConsoleKeyInfo key)` | bool | Checks if key is a system key (Ctrl+X, etc.) |
| `private void HandleEditorResult(EditorKeyResult result, ref bool shouldExit)` | void | Processes result from key handler |
| `private void HandlePromptInput(ConsoleKeyInfo key)` | void | Handles input during prompt dialogs |

### DocumentTreeView.cs
Renders the document tree view showing SOAP entries in hierarchical structure.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _console | ThreadSafeConsoleManager | private readonly | Thread-safe console manager |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `DocumentTreeView(ThreadSafeConsoleManager console)` | - | Constructor accepting console manager |
| `void RenderTree(EditorState state, Region region)` | void | Renders tree view based on current view mode |
| `private void RenderEmptyTree(Region region)` | void | Renders placeholder for empty document |
| `private void RenderGroupedTree(EditorState state, Region region)` | void | Renders SOAP-grouped hierarchical view |
| `private void RenderFlatList(EditorState state, Region region)` | void | Renders flat list of all entries |
| `private void RenderDetailedView(EditorState state, Region region)` | void | Renders detailed view of selected entry |
| `private int RenderSOAPGroup(string letter, string title, IList<AbstractClinicalEntry> entries, Region region, int startLine, int selectedIndex, ref int currentEntryIndex)` | int | Renders a SOAP category group |
| `private void RenderTreeEntry(AbstractClinicalEntry entry, Region region, int line, int indent, bool isSelected)` | void | Renders individual tree entry |

### EditorKeyHandler.cs
Handles keyboard input processing for the clinical document editor.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _editor | ClinicalDocumentEditor | private readonly | Reference to parent editor |
| _console | ThreadSafeConsoleManager | private readonly | Thread-safe console manager |
| _inputHandler | StatusBarInputHandler | private readonly | Handles text input in status bar |
| _renderer | EditorRenderer | private readonly | Editor renderer |
| _isEditingText | bool | private | Indicates if currently editing text |
| _entryBeingEdited | AbstractClinicalEntry? | private | Entry currently being edited |
| _editMode | string? | private | Type of edit operation |
| _isAddingEntry | bool | private | Indicates if adding new entry |
| _addEntryStep | string | private | Current step in add entry workflow |
| _addEntryData | ConcurrentDictionary&lt;string, string&gt; | private | Data collected during add entry |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `EditorKeyHandler(ClinicalDocumentEditor editor, ThreadSafeConsoleManager console, StatusBarInputHandler inputHandler, EditorRenderer renderer)` | - | Constructor accepting dependencies |
| `EditorKeyResult HandleKeyInput(ConsoleKeyInfo keyInfo, EditorState state)` | EditorKeyResult | Main key input dispatcher |
| `private EditorKeyResult HandleControlKeys(ConsoleKeyInfo keyInfo, EditorState state)` | EditorKeyResult | Handles Ctrl+key combinations |
| `private EditorKeyResult HandleNavigationUp(EditorState state)` | EditorKeyResult | Moves selection up |
| `private EditorKeyResult HandleNavigationDown(EditorState state)` | EditorKeyResult | Moves selection down |
| `private EditorKeyResult HandleAddEntry(EditorState state)` | EditorKeyResult | Initiates add entry workflow |
| `private EditorKeyResult HandleEditEntry(EditorState state)` | EditorKeyResult | Initiates edit entry workflow |
| `private EditorKeyResult HandleDeleteEntry(EditorState state)` | EditorKeyResult | Deletes selected entry |
| `private EditorKeyResult HandleSaveDocument(EditorState state)` | EditorKeyResult | Saves document changes |
| `private EditorKeyResult HandleToggleView(EditorState state)` | EditorKeyResult | Cycles through view modes |
| `private void ApplyEditToEntry(AbstractClinicalEntry entry, string newText, string? editMode)` | void | Applies edited text to entry |

### EditorRenderer.cs
Handles rendering of the split-pane editor interface with document tree on left and content view on right.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _console | ThreadSafeConsoleManager | private readonly | Thread-safe console manager |
| _treeView | DocumentTreeView | private readonly | Tree view renderer |
| _inputHandler | StatusBarInputHandler | private readonly | Status bar input handler |
| _zonedRenderer | ZonedRenderer | private readonly | Zone-based renderer |
| _layoutInvalid | bool | private | Indicates if layout needs recalculation |
| _layout | EditorLayout | private | Current editor layout |
| _statusBarHeight | int | private | Dynamic status bar height |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `EditorRenderer(ThreadSafeConsoleManager console)` | - | Constructor accepting console manager |
| `StatusBarInputHandler InputHandler { get; }` | StatusBarInputHandler | Returns input handler instance |
| `bool LayoutInvalid { get; }` | bool | Returns true if layout needs recalculation |
| `void InvalidateLayout()` | void | Marks layout for recalculation |
| `void InvalidateTreeZone()` | void | Marks tree zone as dirty |
| `void InvalidateContentZone()` | void | Marks content zone as dirty |
| `void InvalidateStatusZone()` | void | Marks status zone as dirty |
| `void RenderEditor(EditorState state)` | void | Main render method for entire editor |
| `void ShowHelpOverlay()` | void | Displays help overlay |
| `void ShowStatusMessage(string message, MessageType type)` | void | Shows status message |
| `private void InitializeZones()` | void | Initializes rendering zones |
| `private void CalculateLayout()` | void | Calculates layout regions |
| `private void RenderBorders()` | void | Renders split-pane borders |
| `private void RenderStatusBar(EditorState state)` | void | Renders status bar with controls or input |

### EditorState.cs
Manages the state of the clinical document editor including current document, selection, cursor position, and view mode.

**Inheritance:** None

**Enumerations:**
- **EditorViewMode:** Tree, List, Details

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _flattenedEntries | List&lt;AbstractClinicalEntry&gt; | private | Flattened list of all entries |
| _selectedIndex | int | private | Currently selected entry index |
| _isDirty | bool | private | Indicates if document has unsaved changes |
| Document | ClinicalDocument | get | The clinical document being edited |
| ViewMode | EditorViewMode | get/set | Current view mode (Tree/List/Details) |
| IsDirty | bool | get/set | Document dirty state |
| HasEntries | bool | get | True if document has entries |
| SelectedIndex | int | get/set | Selected entry index with bounds checking |
| SelectedEntry | AbstractClinicalEntry? | get | Currently selected entry |
| FlattenedEntries | IReadOnlyList&lt;AbstractClinicalEntry&gt; | get | Read-only flattened entry list |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `EditorState(ClinicalDocument document)` | - | Constructor accepting document to edit |
| `void RefreshFlattenedEntries()` | void | Rebuilds flattened entry list from document |
| `void MoveSelectionUp()` | void | Moves selection up by one |
| `void MoveSelectionDown()` | void | Moves selection down by one |
| `void MoveToFirst()` | void | Moves selection to first entry |
| `void MoveToLast()` | void | Moves selection to last entry |
| `void MarkDirty()` | void | Marks document as having unsaved changes |
| `void MarkClean()` | void | Marks document as saved |
| `(IList<AbstractClinicalEntry> Subjective, IList<AbstractClinicalEntry> Objective, IList<AbstractClinicalEntry> Assessment, IList<AbstractClinicalEntry> Plan) GetGroupedEntries()` | tuple | Returns entries grouped by SOAP category |
| `private int GetEntrySortOrder(AbstractClinicalEntry entry)` | int | Returns sort order for SOAP categories |

### RenderZone.cs
Represents a rectangular rendering zone within the console interface with dirty state tracking and coordinate boundaries for partial screen updates.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Bounds | Region | get/set | Spatial boundaries in console coordinates |
| IsDirty | bool | get/set | True if zone needs redraw |
| LastUpdate | DateTime | get private | Timestamp of last render |
| ZoneId | string | get | Unique identifier for this zone |
| RenderPriority | int | get/set | Render order (lower renders first) |
| HasLayoutDependencies | bool | get/set | True if zone depends on other zones' layouts |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `RenderZone(string zoneId, Region bounds, int renderPriority = 100)` | - | Constructor with zone ID, bounds, and priority |
| `void Invalidate()` | void | Marks zone as needing redraw |
| `void MarkRendered()` | void | Marks zone as clean and updates timestamp |
| `void UpdateBounds(Region newBounds)` | void | Updates boundaries and invalidates if changed |
| `bool IntersectsWith(RenderZone other)` | bool | Checks if zones overlap spatially |
| `Region? GetIntersection(RenderZone other)` | Region? | Calculates intersection region |

### StatusBarInputHandler.cs
Handles multi-line text input integrated into the status bar area.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| _console | ThreadSafeConsoleManager | private readonly | Thread-safe console manager |
| _currentText | StringBuilder | private | Current input text |
| _cursorPosition | int | private | Cursor position in text |
| _wrappedLines | List&lt;string&gt; | private | Text wrapped to display width |
| _currentLineIndex | int | private | Current line index |
| _currentColumnIndex | int | private | Current column index |
| _isActive | bool | private | True if input is active |
| _prompt | string | private | Input prompt message |
| _maxWidth | int | private | Maximum input width |
| IsActive | bool | get | Returns true if input active |
| RequiredHeight | int | get | Height required for wrapped lines |
| CurrentText | string | get | Returns current input text |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `StatusBarInputHandler(ThreadSafeConsoleManager console)` | - | Constructor accepting console manager |
| `void StartEditing(string prompt, string initialText = "", int maxWidth = 0)` | void | Initiates text input with prompt |
| `bool ProcessKey(ConsoleKeyInfo keyInfo)` | bool | Processes key input, returns false when done |
| `void StopEditing()` | void | Stops input and clears state |
| `void Render(Region region)` | void | Renders input area with wrapped text |
| `private void InsertCharacter(char c)` | void | Inserts character at cursor position |
| `private void UpdateWrappedLines()` | void | Recalculates wrapped line breaks |
| `private void UpdateCursorIndices()` | void | Updates line/column indices from cursor position |

### ZonedRenderer.cs
Manages multiple rendering zones to enable efficient partial screen updates with zone dependencies and cascading layout invalidation.

**Inheritance:** None

**Properties:**

| Name              | Type                                                                  | Access           | Description                          |
| ----------------- | --------------------------------------------------------------------- | ---------------- | ------------------------------------ |
| _console          | ThreadSafeConsoleManager                                              | private readonly | Thread-safe console manager          |
| _zones            | ConcurrentDictionary&lt;string, RenderZone&gt;                        | private readonly | service of render zones             |
| _zoneRenderers    | ConcurrentDictionary&lt;string, Action&lt;RenderZone, object?&gt;&gt; | private readonly | Render callbacks for each zone       |
| _dependencies     | ConcurrentDictionary&lt;string, HashSet&lt;string&gt;&gt;             | private readonly | Zone dependency graph                |
| _isRendering      | bool                                                                  | private volatile | True during render operation         |
| ZoneLayoutChanged | event Action&lt;string, Region, Region&gt;?                           |                  | Event fired when zone layout changes |

**Methods:**

| Signature                                                                                                         | Returns                       | Description                                       |
| ----------------------------------------------------------------------------------------------------------------- | ----------------------------- | ------------------------------------------------- |
| `ZonedRenderer(ThreadSafeConsoleManager console)`                                                                 | -                             | Constructor accepting console manager             |
| `void RegisterZone(string zoneId, Region bounds, Action<RenderZone, object?> renderer, int renderPriority = 100)` | void                          | Registers new rendering zone with callback        |
| `void AddDependency(string dependentZone, string dependsOnZone)`                                                  | void                          | Adds layout dependency between zones              |
| `void UpdateZoneBounds(string zoneId, Region newBounds)`                                                          | void                          | Updates zone boundaries and triggers cascades     |
| `void InvalidateZone(string zoneId)`                                                                              | void                          | Marks zone as needing redraw                      |
| `void InvalidateAllZones()`                                                                                       | void                          | Marks all zones for redraw                        |
| `void RenderAll(object? context = null)`                                                                          | void                          | Renders all dirty zones in priority order         |
| `void RenderZone(string zoneId, object? context = null)`                                                          | void                          | Renders specific zone                             |
| `private void InvalidateDependents(string zoneId)`                                                                | void                          | Invalidates all zones depending on specified zone |
| `private IEnumerable<RenderZone> GetRenderOrder()`                                                                | IEnumerable&lt;RenderZone&gt; | Returns zones sorted by render priority           |
## Core.CliniCore
## Bootstrap
### CoreServiceBootstrapper.cs
Provides dependency injection registration for all core CliniCore services, ensuring consistent service configuration across different client applications (CLI, GUI, API). This class serves as the central bootstrapper, registering authentication services, domain registries, scheduling management components, and command infrastructure with the DI container. It includes methods for initializing development data and sample credentials for testing purposes.
## ClinicalDoc
### AbstractClinicalEntry.cs
Base class for all clinical documentation entries that provides common properties and functionality shared across different entry types.

**Inheritance:** None (abstract base class)

**Properties:**

| Name       | Type              | Access             | Description                                                  |
| ---------- | ----------------- | ------------------ | ------------------------------------------------------------ |
| Id         | Guid              | get, protected set | Unique identifier for this entry, auto-generated on creation |
| AuthorId   | Guid              | get, protected set | The physician who authored this entry                        |
| CreatedAt  | DateTime          | get, protected set | When this entry was created                                  |
| ModifiedAt | DateTime?         | get/set            | When this entry was last modified                            |
| Content    | string            | get/set            | The actual content/text of the entry                         |
| IsActive   | bool              | get/set            | Whether this entry is active (not deleted/superseded)        |
| Code       | string?           | get/set            | Optional ICD-10 or other coding                              |
| Severity   | EntrySeverity     | get/set            | Severity or priority level (defaults to Routine)             |
| EntryType  | ClinicalEntryType | get                | Abstract property defining the type of clinical entry        |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AbstractClinicalEntry(Guid authorId, string content)` | - | Protected constructor initializing ID, author, content, timestamps, and active status |
| `IsValid()` | bool | Virtual method validating that content exists, author is valid, and entry is active |
| `GetValidationErrors()` | List&lt;string&gt; | Virtual method returning list of validation error messages |
| `GetDisplayString()` | string | Virtual method creating formatted display string with severity and code |
| `Update(string newContent)` | void | Updates content and marks entry as modified with current timestamp |
| `ToString()` | string | Returns formatted string with entry type, display string, and creation timestamp |

**Enumerations:**

**ClinicalEntryType:**

| Value | Description |
|-------|-------------|
| ChiefComplaint | Patient's primary reason for visit |
| Observation | Clinical observation or finding |
| Assessment | Clinical assessment or impression |
| Diagnosis | Medical diagnosis |
| Plan | Treatment plan item |
| Prescription | Medication prescription |
| ProgressNote | Progress note entry |
| Procedure | Medical procedure |
| LabResult | Laboratory test result |
| VitalSigns | Vital signs measurement |

**EntrySeverity:**

| Value | Description |
|-------|-------------|
| Routine | Standard severity level |
| Moderate | Moderate severity |
| Urgent | Requires prompt attention |
| Critical | Critical severity |
| Emergency | Emergency severity level |
### AssessmentEntry.cs
Represents a clinical assessment or impression entry within a medical document.

**Inheritance:** AbstractClinicalEntry

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| EntryType | ClinicalEntryType | get | Returns ClinicalEntryType.Assessment |
| ClinicalImpression | string | get/set | Clinical impression summary (alias for Content property) |
| Condition | PatientCondition | get/set | Patient's overall condition (defaults to Stable) |
| Prognosis | Prognosis | get/set | Prognosis assessment (defaults to Good) |
| DifferentialDiagnoses | List&lt;string&gt; | get (private set) | List of differential diagnoses being considered |
| RiskFactors | List&lt;string&gt; | get (private set) | Identified risk factors |
| RequiresImmediateAction | bool | get/set | Whether immediate intervention is needed |
| Confidence | ConfidenceLevel | get/set | Confidence level in the assessment (defaults to Moderate) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AssessmentEntry(Guid authorId, string assessment)` | - | Constructor initializing the assessment with author and clinical impression |
| `GetDisplayString()` | string | Overridden method formatting assessment with condition, action flags, differentials, and risk factors |
| `GetValidationErrors()` | List&lt;string&gt; | Overridden method validating severity matches action requirements and condition/prognosis consistency |

**Enumerations:**

**PatientCondition:**

| Value | Description |
|-------|-------------|
| Stable | Patient condition is stable |
| Improving | Patient is improving |
| Unchanged | No change in condition |
| Worsening | Condition is deteriorating |
| Critical | Patient is in critical condition |

**Prognosis:**

| Value | Description |
|-------|-------------|
| Excellent | Excellent expected outcome |
| Good | Good expected outcome |
| Fair | Fair expected outcome |
| Guarded | Uncertain outcome |
| Poor | Poor expected outcome |

**ConfidenceLevel:**

| Value | Description |
|-------|-------------|
| Low | Low confidence in assessment |
| Moderate | Moderate confidence level |
| High | High confidence in assessment |
| Certain | Certain of assessment |
### ClinicalDocument.cs
Implements a composite pattern for complete medical encounter documentation following the SOAP (Subjective, Objective, Assessment, Plan) format.

**Inheritance:** None

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Id | Guid | get | Unique identifier for this document, auto-generated on creation |
| PatientId | Guid | get | ID of the patient this document belongs to |
| PhysicianId | Guid | get | ID of the physician who created this document |
| AppointmentId | Guid | get | ID of the associated appointment |
| CreatedAt | DateTime | get | When this document was created |
| CompletedAt | DateTime? | get (private set) | When this document was completed (null if incomplete) |
| IsCompleted | bool | get | Whether this document has been completed |
| Entries | IReadOnlyList&lt;AbstractClinicalEntry&gt; | get | All entries in this document (read-only) |
| ChiefComplaint | string? | get/set | Chief complaint/reason for visit |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ClinicalDocument(Guid patientId, Guid physicianId, Guid appointmentId)` | - | Constructor initializing document with patient, physician, and appointment IDs |
| `AddEntry(AbstractClinicalEntry entry)` | void | Adds an entry to the document with validation for completed status and diagnosis-prescription linking |
| `GetEntries<T>() where T : AbstractClinicalEntry` | IEnumerable&lt;T&gt; | Gets all entries of a specific type using generics |
| `GetObservations()` | IEnumerable&lt;ObservationEntry&gt; | Gets all observation entries |
| `GetAssessments()` | IEnumerable&lt;AssessmentEntry&gt; | Gets all assessment entries |
| `GetDiagnoses()` | IEnumerable&lt;DiagnosisEntry&gt; | Gets all diagnosis entries |
| `GetPrescriptions()` | IEnumerable&lt;PrescriptionEntry&gt; | Gets all prescription entries |
| `GetPlans()` | IEnumerable&lt;PlanEntry&gt; | Gets all plan entries |
| `GetPrescriptionsForDiagnosis(Guid diagnosisId)` | IEnumerable&lt;PrescriptionEntry&gt; | Gets all prescriptions linked to a specific diagnosis |
| `IsComplete()` | bool | Validates document meets minimum requirements (chief complaint + observations + assessments + diagnoses + plans) |
| `GetValidationErrors()` | List&lt;string&gt; | Returns list of all validation errors for document and entries |
| `Complete()` | void | Marks document as completed after validation, preventing further modifications |
| `GenerateSOAPNote()` | string | Generates formatted SOAP note string with all document sections |
### DiagnosisEntry.cs
Represents a clinical diagnosis with support for ICD-10 coding and diagnosis lifecycle tracking.

**Inheritance:** AbstractClinicalEntry

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| EntryType | ClinicalEntryType | get | Returns ClinicalEntryType.Diagnosis |
| Type | DiagnosisType | get/set | Type of diagnosis (differential, working, final, ruled out), defaults to Working |
| ICD10Code | string? | get/set | ICD-10 diagnosis code (alias for Code property) |
| IsPrimary | bool | get/set | Whether this is the primary diagnosis |
| OnsetDate | DateTime? | get/set | Date of onset for this condition |
| RelatedPrescriptions | List&lt;Guid&gt; | get (private set) | IDs of prescriptions linked to this diagnosis |
| SupportingObservations | List&lt;Guid&gt; | get (private set) | IDs of observations supporting this diagnosis |
| Status | DiagnosisStatus | get/set | Clinical status of the diagnosis, defaults to Active |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `DiagnosisEntry(Guid authorId, string diagnosisDescription)` | - | Constructor initializing diagnosis with author and description |
| `AddRelatedPrescription(Guid prescriptionId)` | void | Adds a prescription ID that treats this diagnosis (prevents duplicates) |
| `AddSupportingObservation(Guid observationId)` | void | Adds an observation ID that supports this diagnosis (prevents duplicates) |
| `GetDisplayString()` | string | Overridden method formatting diagnosis with type, primary flag, and status indicators |
| `GetValidationErrors()` | List&lt;string&gt; | Overridden method validating final diagnoses have ICD-10 codes |

**Enumerations:**

**DiagnosisType:**

| Value | Description |
|-------|-------------|
| Differential | Possible diagnosis under consideration |
| Working | Probable diagnosis being treated |
| Final | Confirmed diagnosis |
| RuledOut | Excluded diagnosis |

**DiagnosisStatus:**

| Value | Description |
|-------|-------------|
| Active | Currently active condition |
| Resolved | Condition has resolved |
| Chronic | Ongoing chronic condition |
| Remission | In remission |
| Recurrence | Recurrence of previous condition |
### ObservationEntry.cs
Captures clinical observations including subjective patient reports and objective physical findings.

**Inheritance:** AbstractClinicalEntry

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| EntryType | ClinicalEntryType | get | Returns ClinicalEntryType.Observation |
| Type | ObservationType | get/set | Type of observation, defaults to PhysicalExam |
| BodySystem | string? | get/set | Body system or area examined |
| IsAbnormal | bool | get/set | Whether this is a normal or abnormal finding |
| VitalSigns | Dictionary&lt;string, string&gt; | get (private set) | Vital signs measurements (BP, HR, Temp, etc.) |
| NumericValue | double? | get/set | Numeric value if this is a measurement |
| Unit | string? | get/set | Unit of measurement |
| ReferenceRange | string? | get/set | Reference range for this observation |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ObservationEntry(Guid authorId, string observation)` | - | Constructor initializing observation with author and content |
| `AddVitalSign(string name, string value)` | void | Adds or updates a vital sign measurement |
| `GetDisplayString()` | string | Overridden method formatting observation with abnormal flag, body system, and numeric values |
| `GetVitalSignsDisplay()` | string | Formats vital signs as comma-separated display string |

**Enumerations:**

**ObservationType:**

| Value | Description |
|-------|-------------|
| ChiefComplaint | Patient's primary complaint |
| HistoryOfPresentIllness | Patient's description of current illness |
| PhysicalExam | Physical examination findings |
| VitalSigns | Vital signs measurements |
| LabResult | Laboratory test results |
| ImagingResult | Imaging study results |
| ReviewOfSystems | Systematic review of body systems |
| SocialHistory | Patient's social history |
| FamilyHistory | Family medical history |
| Allergy | Known allergies |
### PlanEntry.cs
Represents treatment plan entries with completion tracking and priority management.

**Inheritance:** AbstractClinicalEntry

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| EntryType | ClinicalEntryType | get | Returns ClinicalEntryType.Plan |
| Type | PlanType | get/set | Type of plan item, defaults to Treatment |
| TargetDate | DateTime? | get/set | When this plan item should be completed |
| IsCompleted | bool | get/set | Whether this plan item has been completed |
| CompletedDate | DateTime? | get/set | When the plan item was completed |
| Priority | PlanPriority | get/set | Priority of this plan item, defaults to Routine |
| RelatedDiagnoses | List&lt;Guid&gt; | get (private set) | Diagnoses this plan addresses |
| FollowUpInstructions | string? | get/set | Follow-up instructions for this plan item |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `PlanEntry(Guid authorId, string planDescription)` | - | Constructor initializing plan with author and description |
| `MarkCompleted()` | void | Marks plan as completed with current timestamp and updates ModifiedAt |
| `GetDisplayString()` | string | Overridden method formatting plan with priority, type, status, and target date |
| `GetValidationErrors()` | List&lt;string&gt; | Overridden method validating completion date exists when completed and target date is valid |

**Enumerations:**

**PlanType:**

| Value | Description |
|-------|-------------|
| Treatment | Medical treatment plan |
| Diagnostic | Diagnostic tests to order |
| Referral | Referral to specialist |
| FollowUp | Follow-up appointment needed |
| PatientEducation | Education provided/needed |
| Procedure | Procedure to perform |
| Monitoring | Ongoing monitoring required |
| Prevention | Preventive care measures |

**PlanPriority:**

| Value | Description |
|-------|-------------|
| Routine | Routine priority |
| High | High priority |
| Urgent | Urgent priority |
| Emergency | Emergency priority |
### PrescriptionEntry.cs
Manages medication prescriptions with comprehensive details and diagnosis linkage requirements.

**Inheritance:** AbstractClinicalEntry

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| EntryType | ClinicalEntryType | get | Returns ClinicalEntryType.Prescription |
| DiagnosisId | Guid | get (private set) | REQUIRED: The diagnosis this prescription treats |
| MedicationName | string | get/set | Name of the medication |
| Dosage | string? | get/set | Dosage amount and unit (e.g., "500mg") |
| Frequency | string? | get/set | Frequency of administration (e.g., "twice daily") |
| Route | string? | get/set | Route of administration (defaults to "Oral") |
| Duration | string? | get/set | Duration of treatment |
| Refills | int | get/set | Number of refills authorized (defaults to 0) |
| GenericAllowed | bool | get/set | Whether generic substitution is allowed (defaults to true) |
| DEASchedule | int? | get/set | DEA schedule if controlled substance (1-5) |
| ExpirationDate | DateTime? | get/set | Date prescription expires |
| Instructions | string? | get/set | Special instructions or warnings |
| NDCCode | string? | get/set | National Drug Code if available (alias for Code property) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `PrescriptionEntry(Guid authorId, Guid diagnosisId, string medicationName)` | - | Constructor initializing prescription with author, linked diagnosis, and medication name |
| `GenerateSig()` | string | Generates standard prescription signature (Sig) from dosage, route, frequency, duration, and instructions |
| `GetDisplayString()` | string | Overridden method formatting prescription with medication name, Sig, refills, and DEA schedule |
| `IsValid()` | bool | Overridden method validating diagnosis linkage and medication name in addition to base validation |
| `GetValidationErrors()` | List&lt;string&gt; | Overridden method validating diagnosis linkage, medication name, dosage, frequency, and DEA schedule range |
## Commands
### AbstractCommand.cs
Base implementation for all commands in the CliniCore system, providing common infrastructure for command execution, validation, authorization, and undo functionality. This class implements the Command Pattern with support for parameter validation, session management, execution tracking, audit logging, and state capture for undoable operations. It enforces a consistent execution workflow including authorization checks, session expiration validation, performance measurement, and exception handling across all command types.
## Admin
### CreateFacilityCommand.cs
Unimplemented command stub for creating new medical facilities within the health system.

**Inheritance:** AbstractCommand

**Command Key:** `createfacility`

**Parameters:**
- `facility_name` - Name of the facility
- `facility_address` - Physical address
- `facility_phone` - Contact phone number

**Status:** Not implemented (throws NotImplementedException)

### ManageUserRolesCommand.cs
Unimplemented command stub for managing user role assignments and permissions.

**Inheritance:** AbstractCommand

**Command Key:** `manageuserroles`

**Parameters:**
- `userid` - ID of the user
- `userrole` - New role to assign

**Status:** Not implemented (throws NotImplementedException)

### SystemMaintenanceCommand.cs
Unimplemented command stub for performing system maintenance operations.

**Inheritance:** AbstractCommand

**Command Key:** `systemmaintenance`

**Parameters:**
- `maintenancetype` - Type of maintenance to perform
- `force` - Force maintenance operation

**Status:** Not implemented (throws NotImplementedException)

### UpdateFacilitySettingsCommand.cs
Unimplemented command stub for updating facility configuration settings.

**Inheritance:** AbstractCommand

**Command Key:** `updatefacilitysettings`

**Parameters:**
- `facilityid` - ID of the facility
- `settingname` - Name of the setting to update
- `settingvalue` - New value for the setting

**Status:** Not implemented (throws NotImplementedException)

### ViewAuditLogCommand.cs
Unimplemented command stub for viewing system audit logs with filtering.

**Inheritance:** AbstractCommand

**Command Key:** `viewauditlog`

**Parameters:**
- `startdate` - Start date for log filtering
- `enddate` - End date for log filtering
- `userid` - Optional user filter

**Status:** Not implemented (throws NotImplementedException)
## Authentication
### ChangePasswordCommand.cs
Unimplemented command stub for changing user passwords.

**Inheritance:** AbstractCommand

**Command Key:** `changepassword`

**Parameters:**
- `oldPassword` - Current password
- `newPassword` - New password
- `confirmPassword` - Confirmation of new password

**Status:** Not implemented (throws NotImplementedException)

### LoginCommand.cs
Authenticates users and creates secure sessions by validating credentials against the authentication service.

**Inheritance:** AbstractCommand

**Command Key:** `login`

**Required Permission:** None (public command)

**Can Undo:** No

**Parameters:**
- `username` (string, required) - User's login username
- `password` (string, required) - User's password

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Description | string | get | Returns "Authenticates a user and creates a session" |
| CanUndo | bool | get | Returns false (login cannot be undone, use logout instead) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `LoginCommand(IAuthenticationService authService)` | - | Constructor accepting authentication service via dependency injection |
| `GetRequiredPermission()` | Permission? | Returns null (anyone can attempt login) |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates username and password are provided and not empty |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Warns if already logged in with existing session |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Authenticates user, creates new session, retrieves last login time, and returns SessionContext in result data |

**Execution Flow:**
1. Validates username and password parameters
2. Calls authentication service to verify credentials
3. Checks profile validity
4. Creates new SessionContext for authenticated user
5. Retrieves last login time for welcome message
6. Returns CommandResult with SessionContext as data

### LogoutCommand.cs
Ends the current user session securely, logging the session duration and performing cleanup operations.

**Inheritance:** AbstractCommand

**Command Key:** `logout`

**Required Permission:** None (anyone logged in can logout)

**Can Undo:** No

**Parameters:** None required

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Description | string | get | Returns "Ends the current user session" |
| CanUndo | bool | get | Returns false (logout cannot be undone for security) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `LogoutCommand()` | - | Default constructor with no dependencies |
| `GetRequiredPermission()` | Permission? | Returns null (anyone logged in can logout) |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Returns success (no parameters needed) |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Validates active session exists and warns if already expired |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Captures session info, logs logout event, formats duration message, and returns success |

**Execution Flow:**
1. Validates active session exists
2. Captures username and session duration
3. Logs logout event to audit trail
4. Formats user-friendly duration message
5. Returns CommandResult with null data (signals session should be cleared)
## Clinical
### AddAssessmentCommand.cs
Adds a clinical assessment/impression entry to an existing clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `addassessment`

**Required Permission:** Permission.CreateClinicalDocument

**Can Undo:** Yes

**Parameters:**
- `document_id` (Guid, required) - ID of the clinical document
- `clinical_impression` (string, required) - Clinical assessment/impression text
- `condition` (PatientCondition, optional) - Patient condition (defaults to Stable)
- `prognosis` (Prognosis, optional) - Prognosis assessment (defaults to Good)
- `requires_immediate_action` (bool, optional) - Whether immediate action needed
- `severity` (EntrySeverity, optional) - Severity level (defaults to Routine)
- `confidence` (ConfidenceLevel, optional) - Confidence in assessment (defaults to Moderate)
- `differential_diagnoses` (List\<string\>, optional) - List of differential diagnoses
- `risk_factors` (List\<string\>, optional) - Identified risk factors

**Methods:**

| Signature                                                            | Returns                 | Description                                                                                     |
| -------------------------------------------------------------------- | ----------------------- | ----------------------------------------------------------------------------------------------- |
| `AddAssessmentCommand()`                                             | -                       | Default constructor                                                                             |
| `ValidateParameters(CommandParameters parameters)`                   | CommandValidationResult | Validates document exists, not completed, impression not empty, condition/prognosis consistency |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult           | Creates AssessmentEntry, adds to document, returns entry in result data                         |
| `UndoCore(object previousState, SessionContext? session)`            | CommandResult           | Marks assessment as inactive                                                                    |

### AddDiagnosisCommand.cs
Adds a diagnosis entry to an existing clinical document with ICD-10 coding support.

**Inheritance:** AbstractCommand

**Command Key:** `adddiagnosis`

**Required Permission:** Permission.CreateClinicalDocument

**Can Undo:** Yes

**Parameters:**
- `document_id` (Guid, required) - ID of the clinical document
- `diagnosis_description` (string, required) - Diagnosis description
- `icd10_code` (string, optional) - ICD-10 diagnosis code
- `diagnosis_type` (DiagnosisType, optional) - Type of diagnosis (defaults to Working)
- `is_primary` (bool, optional) - Whether this is the primary diagnosis
- `severity` (EntrySeverity, optional) - Severity level
- `onset_date` (DateTime, optional) - Date of diagnosis onset

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddDiagnosisCommand()` | - | Default constructor |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates document exists, not completed, description not empty, ICD-10 format if provided |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Creates DiagnosisEntry, adds to document, returns entry in result data |
| `UndoCore(object previousState, SessionContext? session)` | CommandResult | Marks diagnosis as inactive |

### AddObservationCommand.cs
Adds a clinical observation entry (subjective or objective) to a clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `addobservation`

**Required Permission:** Permission.CreateClinicalDocument

**Can Undo:** Yes

**Parameters:**
- `document_id` (Guid, required) - ID of the clinical document
- `observation` (string, required) - Observation text
- `vital_signs` (Dictionary<string, string>, optional) - Vital signs measurements
- `numeric_value` (double, optional) - Numeric measurement value
- `unit` (string, optional) - Unit of measurement
- `observation_type` (ObservationType, optional) - Type of observation (defaults to PhysicalExam)
- `body_system` (string, optional) - Body system examined
- `is_abnormal` (bool, optional) - Whether finding is abnormal
- `severity` (EntrySeverity, optional) - Severity level
- `reference_range` (string, optional) - Reference range for values
- `loinc_code` (string, optional) - LOINC code for lab observations

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddObservationCommand()` | - | Default constructor |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates document exists, not completed, observation not empty, vital signs format, numeric value has unit |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Creates ObservationEntry, adds vital signs if provided, adds to document, returns entry in result data |
| `UndoCore(object previousState, SessionContext? session)` | CommandResult | Marks observation as inactive |

### AddPlanCommand.cs
Adds a treatment plan entry to a clinical document with priority and target date tracking.

**Inheritance:** AbstractCommand

**Command Key:** `addplan`

**Required Permission:** Permission.CreateClinicalDocument

**Can Undo:** Yes

**Parameters:**
- `document_id` (Guid, required) - ID of the clinical document
- `plan_description` (string, required) - Plan description
- `plan_type` (PlanType, optional) - Type of plan (defaults to Treatment)
- `priority` (PlanPriority, optional) - Priority level (defaults to Routine)
- `target_date` (DateTime, optional) - Target completion date
- `follow_up_instructions` (string, optional) - Follow-up instructions
- `severity` (EntrySeverity, optional) - Severity level
- `related_diagnoses` (List\<Guid\>, optional) - Related diagnosis IDs

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddPlanCommand()` | - | Default constructor |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates document exists, not completed, description not empty, target date not in past, related diagnoses exist |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Creates PlanEntry, links to diagnoses, adds to document, returns entry in result data |
| `UndoCore(object previousState, SessionContext? session)` | CommandResult | Marks plan as inactive |

### AddPrescriptionCommand.cs
Adds a medication prescription to a clinical document, requiring linkage to an existing diagnosis.

**Inheritance:** AbstractCommand

**Command Key:** `addprescription`

**Required Permission:** Permission.CreateClinicalDocument

**Can Undo:** Yes

**Parameters:**
- `document_id` (Guid, required) - ID of the clinical document
- `diagnosis_id` (Guid, required) - ID of the diagnosis this prescription treats
- `medication_name` (string, required) - Name of medication
- `dosage` (string, required) - Dosage amount and form
- `frequency` (string, required) - Frequency of administration
- `route` (string, optional) - Route of administration (defaults to "Oral")
- `duration` (string, optional) - Duration of treatment
- `refills` (int, optional) - Number of refills allowed (defaults to 0)
- `generic_allowed` (bool, optional) - Whether generic substitution allowed (defaults to true)
- `dea_schedule` (int, optional) - DEA schedule for controlled substances (1-5)
- `instructions` (string, optional) - Additional instructions
- `ndc_code` (string, optional) - National Drug Code

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddPrescriptionCommand()` | - | Default constructor |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates document exists, not completed, diagnosis exists in document, medication name not empty, DEA schedule 1-5 if provided, refills not negative |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Creates PrescriptionEntry linked to diagnosis, sets expiration for controlled substances, adds to document, returns entry with generated Sig in result data |
| `UndoCore(object previousState, SessionContext? session)` | CommandResult | Marks prescription as inactive |

### CreateClinicalDocumentCommand.cs
Creates a new clinical document for a patient encounter.

**Inheritance:** AbstractCommand

**Command Key:** `createclinicaldocument`

**Required Permission:** Permission.CreateClinicalDocument

**Status:** Implementation details not provided in source code read.

### DeleteClinicalDocumentCommand.cs
Deletes a clinical document from the service.

**Inheritance:** AbstractCommand

**Command Key:** `deleteclinicaldocument`

**Required Permission:** Permission.DeleteClinicalDocument

**Status:** Implementation details not provided in source code read.

### ListClinicalDocumentsCommand.cs
Lists clinical documents with optional filtering by patient, physician, or date range.

**Inheritance:** AbstractCommand

**Command Key:** `listclinicaldocuments`

**Required Permission:** Permission dependent on user role

**Status:** Implementation details not provided in source code read.

### UpdateAssessmentCommand.cs
Updates an existing assessment entry in a clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `updateassessment`

**Required Permission:** Permission.UpdateClinicalDocument

**Status:** Implementation details not provided in source code read.

### UpdateClinicalDocumentCommand.cs
Updates properties of an existing clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `updateclinicaldocument`

**Required Permission:** Permission.UpdateClinicalDocument

**Status:** Implementation details not provided in source code read.

### UpdateDiagnosisCommand.cs
Updates an existing diagnosis entry in a clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `updatediagnosis`

**Required Permission:** Permission.UpdateClinicalDocument

**Status:** Implementation details not provided in source code read.

### UpdateObservationCommand.cs
Updates an existing observation entry in a clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `updateobservation`

**Required Permission:** Permission.UpdateClinicalDocument

**Status:** Implementation details not provided in source code read.

### UpdatePlanCommand.cs
Updates an existing plan entry in a clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `updateplan`

**Required Permission:** Permission.UpdateClinicalDocument

**Status:** Implementation details not provided in source code read.

### UpdatePrescriptionCommand.cs
Updates an existing prescription entry in a clinical document.

**Inheritance:** AbstractCommand

**Command Key:** `updateprescription`

**Required Permission:** Permission.UpdateClinicalDocument

**Status:** Implementation details not provided in source code read.

### ViewClinicalDocumentCommand.cs
Retrieves and displays a complete clinical document with all entries.

**Inheritance:** AbstractCommand

**Command Key:** `viewclinicaldocument`

**Required Permission:** Permission dependent on user role

**Status:** Implementation details not provided in source code read.
### CommandFactory.cs
Factory class responsible for creating command instances based on command names or types, managing command registration, and discovering available commands in the system. This class uses dependency injection to provide commands with required services (authentication, scheduling), maintains a service of all available commands using their CommandKey properties, and supports command aliasing for user-friendly alternatives. It includes methods for role-based command filtering, command existence checking, and generating help information for each registered command.

**Inheritance:** None

**Properties:**

| Name                 | Type                                             | Access           | Description |
| -------------------- | ------------------------------------------------ | ---------------- | ----------- |
| \_authService        | `IAuthenticationService`                         | private readonly | Authentication service provider |
| \_schedulerService   | `SchedulerService`                               | private readonly | Scheduler service provider |
| \_profileService     | `ProfileService`                                 | private readonly | Profile service provider |
| \_clinicalDocService | `ClinicalDocumentService`                        | private readonly | Clinical Document service provider |
| \_commandTypes       | `Dictionary&lt;string, Type&gt;`                 | private readonly | Dict of command keys and their types |
| \_commandCreators    | `Dictionary&lt;string, Func&lt;ICommand&gt;&gt;` | private readonly | Dict of command keys and the command's producers |

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `CreateCommand(string)` | `ICommand?` | Creates a command instance by name/key |
| `CreateCommandWithParameters(string, Dictionary)` | `(ICommand?, CommandParameters)` | Creates command with pre-populated parameters |
| `GetAvailableCommands()` | `IEnumerable<string>` | Gets all registered command keys |
| `GetCommandType(string)` | `Type?` | Gets the Type for a command key |
| `GetCommandHelp(string)` | `string` | Gets help text for a command |
| `CommandExists(string)` | `bool` | Checks if a command is registered |
| `GetCommandsForRole(UserRole)` | `IEnumerable<string>` | Gets commands available to a role |

### CommandInvoker.cs
Orchestrates command execution using the Invoker pattern, managing command history, and handling undo/redo operations with thread-safe execution. This class maintains stacks of executed and undone commands to support undo/redo functionality, records detailed execution history including timestamps and performance metrics, and provides batch command execution with transaction-like rollback capabilities. It tracks command success rates and ensures only commands supporting undo can be reversed while maintaining execution audit trails for compliance purposes.
### CommandParameters.cs
Container class for passing parameters to commands in a standardized, type-safe manner using a dictionary-based approach with case-insensitive key matching. This class provides fluent interface methods for setting and getting parameters, supports automatic type conversion including enum and collection handling, validates required parameters with detailed error reporting, and enables parameter cloning and merging for complex command composition. It includes convenience methods for checking parameter existence and accessing typed values safely.
### CommandResult.cs
Represents the outcome of a command execution, encapsulating success status, messages, data, errors, and execution metadata. This class provides factory methods for creating common result types (success, failure, validation failed, unauthorized), stores execution timing and audit information, manages both blocking errors and non-blocking warnings, and supports typed data retrieval from command results. It generates user-friendly display messages that adapt based on the result type and available information.
### CommandValidationResult.cs
Represents the outcome of command validation, distinguishing between blocking errors and non-blocking warnings to support flexible validation scenarios. This class provides factory methods for creating success and failure results, supports merging multiple validation results for composite validation logic, and maintains separate collections for errors and warnings. It generates formatted display messages that communicate validation status clearly to users and calling code.
### ICommand.cs
Defines the contract that all commands in the CliniCore system must implement, establishing the Command Pattern interface for encapsulating requests. This interface specifies properties for command identification (CommandId, CommandKey, CommandName), metadata (Description, CanUndo), required permissions for authorization, and methods for validation, execution, and undo operations. It ensures all commands follow a consistent structure for session-based authorization, parameter validation, and execution with optional undo support.
## Profile
### AssignPatientToPhysicianCommand.cs
Assigns a patient to a physician's care, establishing the physician-patient relationship.

**Inheritance:** AbstractCommand

**Command Key:** `assignpatienttophysician`

**Required Permission:** Permission.CreatePatientProfile

**Can Undo:** Yes

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| patient_id | Guid | Yes | ID of the patient to assign |
| physician_id | Guid | Yes | ID of the physician |
| set_primary | bool | No | Whether to set as primary physician |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Assigns patient to physician and updates relationship |
| `Undo(object previousState, SessionContext? session)` | CommandResult | Removes the physician-patient relationship |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates patient and physician IDs exist and are correct roles |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Physicians can only assign patients to themselves |

### CreateAdministratorCommand.cs
Creates a new administrator profile with authentication credentials.

**Inheritance:** AbstractCommand

**Command Key:** `createadministrator`

**Required Permission:** Permission.CreatePhysicianProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| username | string | Yes | Unique username (min 3 characters) |
| password | string | Yes | Password (min 6 characters) |
| name | string | Yes | Administrator's full name |
| address | string | No | Physical address |
| birthdate | DateTime | No | Date of birth |
| email | string | No | Email address |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Creates administrator profile and registers with authentication service |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates username uniqueness, password strength, and email format |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Only administrators can create administrator profiles |

### CreatePatientCommand.cs
Creates a new patient profile with authentication credentials and demographic information.

**Inheritance:** AbstractCommand

**Command Key:** `createpatient`

**Required Permission:** Permission.CreatePatientProfile

**Can Undo:** Yes

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| username | string | Yes | Unique username for authentication |
| password | string | Yes | Password (min 6 characters) |
| name | string | Yes | Patient's full name |
| address | string | Yes | Physical address |
| birthdate | DateTime | Yes | Date of birth |
| gender | Gender | Yes | Patient's gender |
| race | string | Yes | Patient's race |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Creates patient profile and registers with authentication service |
| `Undo(object previousState, SessionContext? session)` | CommandResult | Removes the created patient profile |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates username uniqueness and password strength |

### CreatePhysicianCommand.cs
Creates a new physician profile with credentials, license information, and specializations.

**Inheritance:** AbstractCommand

**Command Key:** `createphysician`

**Required Permission:** Permission.CreatePhysicianProfile

**Can Undo:** Yes

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| username | string | Yes | Unique username for authentication |
| password | string | Yes | Password (min 6 characters) |
| name | string | Yes | Physician's full name |
| address | string | Yes | Physical address |
| birthdate | DateTime | Yes | Date of birth |
| license_number | string | Yes | Medical license number |
| graduation_date | DateTime | Yes | Medical school graduation date |
| specializations | List&lt;MedicalSpecialization&gt; | Yes | Medical specializations (max 5) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Creates physician profile and registers with authentication service |
| `Undo(object previousState, SessionContext? session)` | CommandResult | Removes the created physician profile |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates username uniqueness, password strength, and specialization count |

### DeleteProfileCommand.cs
Permanently deletes a user profile from the system with dependency checking.

**Inheritance:** AbstractCommand

**Command Key:** `deleteprofile`

**Required Permission:** Permission.DeletePatientProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profileId | Guid | Yes | ID of the profile to delete |
| force | bool | No | Skip dependency checks if true |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Deletes profile after checking/cleaning dependencies |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists and checks for dependencies (documents, appointments, etc.) |
| `CheckDependencies(Guid profileId, IUserProfile profile)` | List&lt;string&gt; | Returns list of dependencies preventing deletion |
| `CleanupDependencies(Guid profileId, IUserProfile profile)` | void | Removes all dependencies when force=true |

### ListPatientsCommand.cs
Lists all patients with optional filtering by physician, search term, and validity status.

**Inheritance:** AbstractCommand

**Command Key:** `listpatients`

**Required Permission:** Permission.ViewAllPatients

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| physician_id | Guid | No | Filter patients by assigned physician |
| search | string | No | Search term for name or username |
| include_inactive | bool | No | Include invalid profiles (default: false) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted list of patients with primary physician info |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates physician ID if provided |
| `FormatPatientInfo(PatientProfile patient)` | string | Formats patient information with primary physician name |

### ListPhysiciansCommand.cs
Lists all physicians with optional filtering by specialization and search term.

**Inheritance:** AbstractCommand

**Command Key:** `listphysicians`

**Required Permission:** None (public access)

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| specialization | MedicalSpecialization | No | Filter by medical specialization |
| search | string | No | Search term for name or license number |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted list of physicians with years of experience |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | No validation needed for listing |
| `FormatPhysicianInfo(PhysicianProfile physician)` | string | Formats physician information with calculated years of experience |

### ListProfileCommand.cs
Lists all profiles in the system, grouped by role with role-specific details.

**Inheritance:** AbstractCommand

**Command Key:** `listprofiles`

**Required Permission:** Permission.ViewAllProfiles

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| include_invalid | bool | No | Include invalid profiles (default: false) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted list of all profiles grouped by role |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | No validation needed for listing |

### UpdateAdministratorProfileCommand.cs
Updates an existing administrator profile's information.

**Inheritance:** AbstractCommand

**Command Key:** `updateadministratorprofile`

**Required Permission:** Permission.UpdateAdministratorProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profileId | Guid | Yes | ID of the administrator profile to update |
| name | string | No | Updated full name |
| address | string | No | Updated physical address |
| birthdate | DateTime | No | Updated date of birth |
| email | string | No | Updated email address |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Updates administrator profile with provided fields |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists, is administrator role, and at least one field provided |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Only administrators can update administrator profiles |

### UpdatePatientProfileCommand.cs
Updates an existing patient profile's information and demographic data.

**Inheritance:** AbstractCommand

**Command Key:** `updatepatientprofile`

**Required Permission:** Permission.UpdatePatientProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profileId | Guid | Yes | ID of the patient profile to update |
| name | string | No | Updated full name |
| address | string | No | Updated physical address |
| birthdate | DateTime | No | Updated date of birth |
| patient_gender | string | No | Updated gender |
| patient_race | string | No | Updated race |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Updates patient profile with provided fields |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists, is patient role, and at least one field provided |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Patients can only update own profile; physicians can update their patients |

### UpdatePhysicianProfileCommand.cs
Updates an existing physician profile including credentials and specializations.

**Inheritance:** AbstractCommand

**Command Key:** `updatephysicianprofile`

**Required Permission:** Permission.UpdatePhysicianProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profileId | Guid | Yes | ID of the physician profile to update |
| name | string | No | Updated full name |
| address | string | No | Updated physical address |
| birthdate | DateTime | No | Updated date of birth |
| physician_license | string | No | Updated medical license number |
| physician_graduation | DateTime | No | Updated graduation date |
| physician_specializations | List&lt;MedicalSpecialization&gt; | No | Updated specializations |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Updates physician profile with provided fields |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists, is physician role, and at least one field provided |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Physicians can only update own profile; administrators can update any |

### UpdateProfileCommand.cs
Router command that delegates to appropriate profile-specific update command based on profile type.

**Inheritance:** AbstractCommand

**Command Key:** `updateprofile`

**Required Permission:** Permission.UpdatePatientProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profileId | Guid | Yes | ID of the profile to update |
| name | string | No | Updated full name |
| address | string | No | Updated physical address |
| birthdate | DateTime | No | Updated date of birth |
| patient_gender | string | No | Updated gender (Patient only) |
| patient_race | string | No | Updated race (Patient only) |
| physician_license | string | No | Updated license (Physician only) |
| physician_graduation | DateTime | No | Updated graduation date (Physician only) |
| physician_specializations | List&lt;MedicalSpecialization&gt; | No | Updated specializations (Physician only) |
| email | string | No | Updated email (Administrator only) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Routes to appropriate update command based on profile role |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists |
| `GetEditableFieldsForProfileType(UserRole role)` | List&lt;string&gt; | Static helper to get editable fields for a profile type |

### ViewAdministratorProfileCommand.cs
Retrieves and displays detailed information for a specific administrator profile.

**Inheritance:** AbstractCommand

**Command Key:** `viewadminprofile`

**Required Permission:** Permission.ViewAdministratorProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profile_id | Guid | Yes | ID of the administrator profile to view |
| show_details | bool | No | Show validation errors if profile is invalid |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted administrator profile information |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists and is administrator role |

### ViewPatientProfileCommand.cs
Retrieves and displays detailed information for a specific patient profile.

**Inheritance:** AbstractCommand

**Command Key:** `viewpatientprofile`

**Required Permission:** Permission.ViewPatientProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profile_id | Guid | Yes | ID of the patient profile to view |
| show_details | bool | No | Show appointment count, document count, and validation errors |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted patient profile with demographics and primary physician |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists and is patient role |

### ViewPhysicianProfileCommand.cs
Retrieves and displays detailed information for a specific physician profile.

**Inheritance:** AbstractCommand

**Command Key:** `viewphysicianprofile`

**Required Permission:** Permission.ViewPhysicianProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profile_id | Guid | Yes | ID of the physician profile to view |
| show_details | bool | No | Show patient count, appointment count, and validation errors |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted physician profile with credentials and specializations |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists and is physician role |

### ViewProfileCommand.cs
Generic profile viewing command that formats and displays any profile type (Patient, Physician, Administrator).

**Inheritance:** AbstractCommand

**Command Key:** `viewprofile`

**Required Permission:** Permission.ViewPatientProfile

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| profile_id | Guid | Yes | ID of the profile to view |
| show_details | bool | No | Show additional details (counts, validation errors) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Execute(CommandParameters params, SessionContext? session)` | CommandResult | Returns formatted profile based on role type |
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates profile exists |
| `FormatPatientProfile(PatientProfile patient, bool showDetails)` | string | Formats patient profile display |
| `FormatPhysicianProfile(PhysicianProfile physician, bool showDetails)` | string | Formats physician profile display |
| `FormatAdministratorProfile(AdministratorProfile admin, bool showDetails)` | string | Formats administrator profile display |
| `FormatGenericProfile(IUserProfile profile, bool showDetails)` | string | Formats generic profile display |
## Query
### FindPhysiciansByAvailabilityCommand.cs
Find available physicians for a specific time slot.

**Inheritance:** AbstractCommand

**Command Key:** `findphysiciansbyavailability`

**Required Permission:** Permission.ViewAllAppointments

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| startTime | DateTime | Conditional | Start time for availability search (required with endTime) |
| endTime | DateTime | Conditional | End time for availability search (required with startTime) |
| date | DateTime | Conditional | Date for availability search (required with duration) |
| duration | int | Conditional | Duration in minutes (required with date) |
| specialization | string | No | Filter by medical specialization |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates either (startTime/endTime) or (date/duration) provided |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Ensures user is logged in |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns list of available physicians with next available slots |
| `TryParseSpecialization(string input, out MedicalSpecialization specialization)` | bool | Parses specialization from string input |
| `FormatAvailabilityResults(List<PhysicianAvailabilityInfo> physicians, DateTime start, DateTime end, TimeSpan duration, MedicalSpecialization? spec, SessionContext? session)` | string | Formats physician availability results |

### FindPhysiciansBySpecializationCommand.cs
Find physicians by their medical specialization.

**Inheritance:** AbstractCommand

**Command Key:** `findphysiciansbyspecialization`

**Required Permission:** Permission.ViewAllPatients

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| specialization | string | Yes | Medical specialization to search for |
| includeAvailability | bool | No | Include standard availability information |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates specialization parameter is valid |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Ensures user is logged in |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns list of physicians with matching specialization |
| `TryParseSpecialization(string input, out MedicalSpecialization specialization)` | bool | Parses specialization from string with partial matching |
| `FormatSearchResults(List<PhysicianProfile> physicians, MedicalSpecialization spec, bool includeAvailability, SessionContext? session)` | string | Formats physician search results |

### GetScheduleCommand.cs
Get schedule for a physician for a specific date or date range.

**Inheritance:** AbstractCommand

**Command Key:** `getschedule`

**Required Permission:** Permission.ViewAllAppointments

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| physicianId | Guid | Yes | ID of the physician whose schedule to retrieve |
| date | DateTime | Conditional | Single date to view (required if no date range) |
| startDate | DateTime | Conditional | Start of date range (required with endDate) |
| endDate | DateTime | Conditional | End of date range (required with startDate, max 90 days) |
| viewType | string | No | View format: summary, detailed, compact, or statistics (default: summary) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates physician exists and date parameters are correct |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Role-based access control (physicians can only view own schedule) |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns formatted schedule based on view type |
| `FormatSummarySchedule(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime start, DateTime end, SessionContext? session)` | string | Summary view with appointment list |
| `FormatDetailedSchedule(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime start, DateTime end, SessionContext? session)` | string | Detailed view with full appointment information |
| `FormatCompactSchedule(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime start, DateTime end)` | string | Compact one-line per appointment view |
| `FormatScheduleStatistics(PhysicianProfile physician, List<AppointmentTimeInterval> appointments, DateTime start, DateTime end)` | string | Statistical summary of schedule |

### ListAllUsersCommand.cs
Lists all users in the system (administrators, physicians, and patients).

**Inheritance:** AbstractCommand

**Command Key:** `listallusers`

**Required Permission:** Permission.ViewAllPatients

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| (none) | | | No parameters required |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | No validation needed (no parameters) |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Only administrators can list all users |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns formatted list of all users grouped by role |
| `FormatUserList(IEnumerable<AdministratorProfile> admins, IEnumerable<PhysicianProfile> physicians, IEnumerable<PatientProfile> patients)` | string | Formats user list with role sections and summary |

### SearchClinicalNotesCommand.cs
Search clinical documents by diagnosis, medication, or general text.

**Inheritance:** AbstractCommand

**Command Key:** `searchclinicalnotes`

**Required Permission:** Permission.ViewOwnClinicalDocuments

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| searchTerm | string | Yes | Text to search for in clinical documents |
| searchType | string | No | Type of search: general, diagnosis, medication, prescription (default: general) |
| patientId | Guid | No | Filter results by patient ID |
| physicianId | Guid | No | Filter results by physician ID |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates search term is not empty and search type is valid |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Patients can only search their own documents |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns matching clinical documents with role-based filtering |
| `SearchByGeneralText(string searchTerm)` | IEnumerable<ClinicalDocument> | Searches across all document fields |
| `FormatSearchResults(List<ClinicalDocument> results, string searchTerm, string searchType)` | string | Formats search results with relevant snippets |

### SearchPatientsCommand.cs
Search patients by name.

**Inheritance:** AbstractCommand

**Command Key:** `searchpatients`

**Required Permission:** Permission.ViewAllPatients

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| searchTerm | string | Yes | Name to search for (minimum 2 characters) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates search term is at least 2 characters |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Patients cannot search for other patients |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns matching patients (physicians see only their patients) |
| `FormatSearchResults(List<PatientProfile> patients, string searchTerm, SessionContext? session)` | string | Formats patient search results with demographics and physician info |

## Reports
### GenerateAppointmentReportCommand.cs
Unimplemented command stub for generating appointment statistics and reports.

**Inheritance:** AbstractCommand

**Command Key:** `generateappointmentreport`

**Required Permission:** Permission.ViewSystemReports

**Status:** Not implemented (intentionally omitted for assignment scope)

### GenerateFacilityReportCommand.cs
Unimplemented command stub for generating facility utilization and performance reports.

**Inheritance:** AbstractCommand

**Command Key:** `generatefacilityreport`

**Required Permission:** Permission.ViewSystemReports

**Status:** Not implemented (intentionally omitted for assignment scope)

### GeneratePatientReportCommand.cs
Unimplemented command stub for generating patient demographics and care reports.

**Inheritance:** AbstractCommand

**Command Key:** `generatepatientreport`

**Required Permission:** Permission.ViewSystemReports

**Status:** Not implemented (intentionally omitted for assignment scope)

### GeneratePhysicianReportCommand.cs
Unimplemented command stub for generating physician productivity and patient load reports.

**Inheritance:** AbstractCommand

**Command Key:** `generatephysicianreport`

**Required Permission:** Permission.ViewSystemReports

**Status:** Not implemented (intentionally omitted for assignment scope)

## Scheduling
### CancelAppointmentCommand.cs
Cancels an existing appointment and updates its status.

**Inheritance:** AbstractCommand

**Command Key:** `cancelappointment`

**Required Permission:** Permission.ViewOwnAppointments

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| appointment_id | Guid | Yes | The unique identifier of the appointment to cancel |
| reason | string | No | The reason for cancellation (defaults to "Cancelled by user") |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates that appointment_id is provided; warns if no cancellation reason is given |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Finds the appointment by ID and cancels it through SchedulingService, creating an audit trail |

### CheckConflictsCommand.cs
Checks for scheduling conflicts for a proposed appointment time without booking it.

**Inheritance:** AbstractCommand

**Command Key:** `checkconflicts`

**Required Permission:** Permission.ViewAllAppointments

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| physician_id | Guid | Yes | The unique identifier of the physician |
| start_time | DateTime | Yes | The proposed start time for the appointment |
| duration_minutes | int | Yes | The duration of the appointment in minutes (15-180) |
| exclude_appointment_id | Guid | No | Appointment ID to exclude from conflict checking (for rescheduling scenarios) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates physician ID, start time (not in past), duration (15-180 minutes), and business hours (Mon-Fri, 8 AM-5 PM) |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Checks for scheduling conflicts and returns detailed conflict information with alternative time suggestions if conflicts are found |

### DeleteAppointmentCommand.cs
Permanently deletes an appointment from the schedule.

**Inheritance:** AbstractCommand

**Command Key:** `deleteappointment`

**Required Permission:** Permission.ScheduleAnyAppointment

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| appointment_id | Guid | Yes | The unique identifier of the appointment to delete |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates that appointment_id is provided and the appointment exists; warns if deleting a completed appointment |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Permanently deletes the appointment from the physician's schedule |

### GetAvailableTimeSlotsCommand.cs
Gets available appointment time slots for a physician on a specific date.

**Inheritance:** AbstractCommand

**Command Key:** `getavailabletimeslots`

**Required Permission:** Permission.ScheduleAnyAppointment

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| physician_id | Guid | Yes | The unique identifier of the physician |
| date | DateTime | Yes | The date to search for available slots (must be a weekday, not in the past) |
| duration_minutes | int | Yes | The desired appointment duration in minutes (15-180) |
| max_slots | int | No | Maximum number of slots to return (1-20, defaults to 10) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates physician exists, date is valid weekday in future, duration is 15-180 minutes, and max_slots is 1-20 if provided |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Uses booking strategy to find available time slots within business hours (8 AM-5 PM) for the specified date and duration |

### ListAppointmentsCommand.cs
Lists appointments with various filters.

**Inheritance:** AbstractCommand

**Command Key:** `listappointments`

**Required Permission:** Permission.ViewOwnAppointments

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| date | DateTime | No | Filter appointments by date (defaults to today) |
| physician_id | Guid | No | Filter appointments by physician ID |
| patient_id | Guid | No | Filter appointments by patient ID |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Always returns success (all parameters are optional) |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Retrieves and formats appointments based on role-based filtering (patients see their own, physicians see their schedule, administrators see all) |

### ScheduleAppointmentCommand.cs
Schedules a new appointment between a patient and physician.

**Inheritance:** AbstractCommand

**Command Key:** `scheduleappointment`

**Required Permission:** Permission.ScheduleAnyAppointment

**Can Undo:** Yes

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| patient_id | Guid | Yes | The unique identifier of the patient |
| physician_id | Guid | Yes | The unique identifier of the physician |
| start_time | DateTime | Yes | The start time of the appointment (must be in future, within business hours) |
| duration_minutes | int | Yes | The duration of the appointment in minutes (15-180) |
| reason | string | No | The reason for the visit (defaults to "General Consultation") |
| notes | string | No | Additional notes for the appointment |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates patient and physician exist, start time is in future, duration is 15-180 minutes, and appointment is within business hours (Mon-Fri, 8 AM-5 PM) |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Ensures patients can only schedule their own appointments |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Creates and schedules the appointment with conflict detection, establishes patient-physician relationship if needed |
| `CaptureStateForUndo(CommandParameters parameters, SessionContext? session)` | object? | Captures the created appointment for undo operation |
| `UndoCore(object previousState, SessionContext? session)` | CommandResult | Cancels the appointment if undo is requested |

### SetPhysicianAvailabilityCommand.cs
Set the availability schedule for a physician.

**Inheritance:** AbstractCommand

**Command Key:** `setphysicianavailability`

**Required Permission:** Permission.EditOwnAvailability

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| physician_id | Guid | Yes | The unique identifier of the physician |
| day_of_week | string | Yes | The day of the week for availability |
| start_time | TimeSpan | Yes | The start time of availability |
| end_time | TimeSpan | Yes | The end time of availability |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates that all required parameters are provided and physician_id is a valid GUID |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Returns failure - not yet implemented (SchedulingService does not support physician availability) |

### UpdateAppointmentCommand.cs
Updates appointment details (time, duration, reason, and notes).

**Inheritance:** AbstractCommand

**Command Key:** `updateappointment`

**Required Permission:** Permission.ScheduleAnyAppointment

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| appointment_id | Guid | Yes | The unique identifier of the appointment to update |
| reason | string | No | Updated reason for the visit |
| notes | string | No | Updated notes for the appointment |
| duration_minutes | int | No | Updated duration in minutes (15-180) |
| new_start_time | DateTime | No | Updated start time (must be in future, within business hours) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates appointment exists, duration is 15-180 minutes if provided, new start time is valid if provided, and checks for conflicts with the proposed changes |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Delegates to SchedulingService to update appointment with conflict validation, returns alternatives if conflicts detected |

### ViewAppointmentCommand.cs
Views detailed information about a specific appointment.

**Inheritance:** AbstractCommand

**Command Key:** `viewappointment`

**Required Permission:** Permission.ViewOwnAppointments

**Can Undo:** No

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| appointment_id | Guid | Yes | The unique identifier of the appointment to view |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ValidateParameters(CommandParameters parameters)` | CommandValidationResult | Validates that appointment_id is provided and in valid GUID format |
| `ValidateSpecific(CommandParameters parameters, SessionContext? session)` | CommandValidationResult | Enforces role-based access control (patients can only view their own appointments, physicians can only view their own appointments) |
| `ExecuteCore(CommandParameters parameters, SessionContext? session)` | CommandResult | Retrieves and formats detailed appointment information including patient info, physician info, appointment details, conflicts, and business hours compliance |
## Domain
### AbstractUserProfile.cs
Base class for all user profiles that provides common functionality and enforces profile structure through the Template Method pattern.

**Inheritance:** IUserProfile

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Id | Guid | get | Unique identifier for the profile, auto-generated on creation |
| Username | string | get/set | User's login username |
| CreatedAt | DateTime | get | Timestamp of profile creation |
| Entries | List<ProfileEntry> | get | Collection of validated profile data entries |
| Role | UserRole | get | Abstract property defining the user's role (Patient, Physician, Administrator) |
| IsValid | bool | get | Returns true if username is not empty and all entries are valid |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetValidationErrors()` | List<string> | Collects and returns all validation error messages from username and entries |
| `GetEntry(string key)` | ProfileEntry? | Retrieves a profile entry by its key, or null if not found |
| `GetValue<T>(string key)` | T? | Gets the typed value from a profile entry by key, or default if not found |
| `SetValue<T>(string key, T value)` | bool | Sets a typed value for a profile entry by key, returns true if successful |
| `GetProfileTemplate()` | IProfileTemplate | Abstract method that subclasses override to provide role-specific profile templates |
### AdministratorProfile.cs
Represents an administrator user profile with system-wide management capabilities.

**Inheritance:** AbstractUserProfile

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Role | UserRole | get | Returns UserRole.Administrator |
| Name | string | get | Administrator's full name from profile entries |
| Department | string | get/set | Department assignment, defaults to "Administration" |
| GrantedPermissions | List<Permission> | get | List of granular permissions granted to this administrator |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetProfileTemplate()` | IProfileTemplate | Returns a new AdministratorProfileTemplate instance |
## Authentication
### AuthenticationException.cs
Custom exception class for authentication failures with detailed failure reason tracking and contextual information.

**Inheritance:** Exception

**Enums:**

| Name | Values | Description |
|------|--------|-------------|
| FailureReason | InvalidCredentials, AccountLocked, AccountNotFound, SessionExpired, PasswordExpired, InvalidPassword, Unknown | Categorizes the specific cause of authentication failure |

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Reason | FailureReason | get | The specific reason for the authentication failure |
| Username | string? | get | The username associated with the failed authentication attempt, if available |
| OccurredAt | DateTime | get | The timestamp when the authentication exception occurred |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AuthenticationException(FailureReason reason, string? username = null, string? message = null)` | - | Creates a new authentication exception with the specified failure reason, optional username, and custom message |
| `AuthenticationException(FailureReason reason, string? username, string message, Exception innerException)` | - | Creates a new authentication exception with an inner exception for exception chaining |
### BasicAuthenticationService.cs
In-memory authentication service implementation that manages user credentials, password hashing, and account security using SHA256 hashing with salt.

**Inheritance:** IAuthenticationService

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Authenticate(string username, string password)` | IUserProfile? | Authenticates a user with username and password, returning their profile on success or null if authentication fails |
| `Register(IUserProfile profile, string password)` | bool | Registers a new user in the system with the specified profile and password, returning true on success |
| `ChangePassword(string username, string currentPassword, string newPassword)` | bool | Changes a user's password after verifying the current password, returning true on success |
| `ValidatePasswordStrength(string password)` | bool | Validates if a password meets security requirements (minimum 6 characters) |
| `UserExists(string username)` | bool | Checks if a username already exists in the system |
| `LockAccount(string username)` | bool | Locks a user account to prevent login (admin function) |
| `UnlockAccount(string username)` | bool | Unlocks a previously locked user account (admin function) |
| `GetLastLoginTime(string username)` | DateTime? | Gets the last login time for a user, or null if never logged in |
| `GetAllUsernames()` | IEnumerable&lt;string&gt; | Gets all registered usernames (for debugging/testing only) |
| `ResetToDefaults()` | void | Clears all users except the default admin account (for testing) |
### IAuthenticationService.cs
Defines the contract for authentication operations in the CliniCore system including user authentication, registration, and password management.

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Authenticate(string username, string password)` | IUserProfile? | Authenticates a user with username and password, returning an authenticated user profile if successful or null otherwise |
| `Register(IUserProfile profile, string password)` | bool | Registers a new user in the system with the specified profile and password, returning true if registration is successful |
| `ChangePassword(string username, string currentPassword, string newPassword)` | bool | Changes a user's password after verifying the current password, returning true if successful |
| `ValidatePasswordStrength(string password)` | bool | Validates if a password meets security requirements, returning true if the password meets requirements |
| `UserExists(string username)` | bool | Checks if a username already exists in the system, returning true if the username exists |
| `LockAccount(string username)` | bool | Locks a user account (admin function), preventing login until unlocked |
| `UnlockAccount(string username)` | bool | Unlocks a previously locked user account (admin function), restoring login access |
| `GetLastLoginTime(string username)` | DateTime? | Gets the last login time for a user, returning the DateTime or null if never logged in |
### RoleBasedAuthorizationService.cs
Service for handling role-based authorization checks using a permission-based access control system.

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `Authorize(SessionContext? session, Permission permission)` | bool | Checks if a session has a specific permission, returning false if session is null or expired |
| `AuthorizeAll(SessionContext? session, params Permission[] permissions)` | bool | Checks if a session has all of the specified permissions, returning true only if all are granted |
| `AuthorizeAny(SessionContext? session, params Permission[] permissions)` | bool | Checks if a session has any of the specified permissions, returning true if at least one is granted |
| `HasPermission(UserRole role, Permission permission)` | bool | Checks if a specific role has a particular permission regardless of session state |
| `GetRolePermissions(UserRole role)` | IEnumerable&lt;Permission&gt; | Gets all permissions for a specific role |
| `CanAccessUserData(SessionContext? session, Guid targetUserId)` | bool | Checks if a user can access another user's data (users can access their own, physicians can access patients, administrators can access all) |
| `CanModifyUserData(SessionContext? session, Guid targetUserId)` | bool | Checks if a user can modify another user's data (users can modify their own with EditOwnProfile permission, administrators can modify all) |
| `ValidateSession(SessionContext? session)` | bool | Validates that a session is valid and not expired |
| `CreateAuthorizationException(SessionContext? session, Permission requiredPermission, string? additionalMessage = null)` | UnauthorizedAccessException | Static factory method that creates an authorization exception with contextual information about the session and required permission |
### SessionContext.cs
Maintains context about the current authenticated session including user profile, session timing, and permission checking.

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| AuthenticatedUser | IUserProfile | get | The currently authenticated user profile |
| SessionId | Guid | get | Unique identifier for this session |
| LoginTime | DateTime | get | When the user logged in |
| LastActivityTime | DateTime | get, private set | Last time any activity occurred in this session |
| UserRole | UserRole | get | Convenience property to get the user's role from the authenticated user |
| Username | string | get | Convenience property to get the username from the authenticated user |
| UserId | Guid | get | Convenience property to get the user's ID from the authenticated user |
| SessionDuration | TimeSpan | get | How long the session has been active (calculated from current time - login time) |
| IdleTime | TimeSpan | get | How long since last activity (calculated from current time - last activity time) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `IsExpired(TimeSpan? maxIdleTime = null)` | bool | Whether the session has been idle too long (default 30 minutes if maxIdleTime is not specified) |
| `UpdateActivity()` | void | Updates the last activity time to now to prevent session timeout |
| `HasPermission(Permission permission)` | bool | Checks if the authenticated user has a specific permission based on their role |
| `CreateSession(IUserProfile authenticatedUser)` | SessionContext | Static factory method to create a new session for an authenticated user |
| `ToString()` | string | Gets a display string for the current session in the format "Session {SessionId} - User: {Username} ({UserRole}) - Active: {SessionDuration}" |
## Enumerations
### AppointmentStatus.cs
Enumeration defining the lifecycle states of medical appointments.

**Values:**

| Name | Value | Description |
|------|-------|-------------|
| Scheduled | 0 | Appointment is scheduled and confirmed |
| Completed | 1 | Appointment was completed successfully |
| Cancelled | 2 | Appointment was cancelled |
| NoShow | 3 | Patient did not show up for appointment |
| InProgress | 4 | Appointment is in progress |
| Tentative | 5 | Appointment is tentatively scheduled, awaiting confirmation |
| Rescheduled | 6 | Appointment was rescheduled |
### EntryKeyResolver.cs
Static helper class for resolving profile entry type keys and display names across different entry type enumerations.

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetKey(Enum entryType)` | string | Resolves the key string for any entry type enum (CommonEntryType, PatientEntryType, PhysicianEntryType, AdministratorEntryType) |
| `GetDisplayName(Enum entryType)` | string | Resolves the display name string for any entry type enum (CommonEntryType, PatientEntryType, PhysicianEntryType, AdministratorEntryType) |
## EntryTypes
### AdministratorEntryType.cs
### CommonEntryType.cs
### PatientEntryType.cs
### PhysicianEntryType.cs
## Extensions
### AdministratorEntryTypeExtensions.cs
### CommonEntryTypeExtensions.cs
### GenderExtensions.cs
### MedicalSpecializationExtensions.cs
### PatientEntryTypeExtensions.cs
### PhysicianEntryTypeExtensions.cs
### Gender.cs
Enumeration providing inclusive gender identity options for patient and user profiles.

**Values:**

| Name | Value | Description |
|------|-------|-------------|
| Man | 0 | Identifies as male |
| Woman | 1 | Identifies as female |
| NonBinary | 2 | Identifies as non-binary |
| GenderQueer | 3 | Identifies as genderqueer |
| GenderFluid | 4 | Gender identity varies over time |
| AGender | 5 | Does not identify with any gender |
| Other | 6 | Gender identity not listed |
| PreferNotToSay | 7 | Prefers not to disclose gender identity |
### MedicalSpecialization.cs
Enumeration defining medical specialties available in the CliniCore system.

**Values:**

| Name | Value | Description |
|------|-------|-------------|
| Emergency | 0 | Emergency Department |
| FamilyMedicine | 1 | Family Practice |
| InternalMedicine | 2 | Internal Medicine |
| Pediatrics | 3 | Children's Health |
| ObstetricsGynecology | 4 | Women's Health |
| Surgery | 5 | General Surgery |
| Orthopedics | 6 | Orthopedics & Sports Medicine |
| Cardiology | 7 | Heart & Vascular |
| Neurology | 8 | Brain & Spine |
| Oncology | 9 | Cancer Center |
| Radiology | 10 | Imaging |
| Anesthesiology | 11 | Anesthesia |
| Psychiatry | 12 | Behavioral Health |
| Dermatology | 13 | Dermatology |
| Ophthalmology | 14 | Eye Care |
### Permission.cs
Enumeration defining granular system permissions for role-based access control.

**Values:**

| Name | Value | Description |
|------|-------|-------------|
| ViewOwnProfile | 0 | Patient: View own profile |
| EditOwnProfile | 1 | Patient: Edit own profile |
| ViewOwnAppointments | 2 | Patient: View own appointments |
| ScheduleOwnAppointment | 3 | Patient: Schedule own appointment |
| ViewOwnClinicalDocuments | 4 | Patient: View own clinical documents |
| ViewAllPatients | 5 | Physician: View all patients |
| CreatePatientProfile | 6 | Physician: Create patient profile |
| ViewPatientProfile | 7 | Physician: View patient profile |
| UpdatePatientProfile | 8 | Physician: Update patient profile |
| DeletePatientProfile | 9 | Physician: Delete patient profile |
| ViewPhysicianProfile | 10 | Physician: View physician profile |
| CreateClinicalDocument | 11 | Physician: Create clinical document |
| UpdateClinicalDocument | 12 | Physician: Update clinical document |
| DeleteClinicalDocument | 13 | Physician: Delete clinical document |
| ViewAllAppointments | 14 | Physician: View all appointments |
| ScheduleAnyAppointment | 15 | Physician: Schedule any appointment |
| EditOwnAvailability | 16 | Physician: Edit own availability |
| CreatePhysicianProfile | 17 | Admin: Create physician profile |
| UpdatePhysicianProfile | 18 | Admin: Update physician profile |
| DeletePhysicianProfile | 19 | Admin: Delete physician profile |
| ViewAdministratorProfile | 20 | Admin: View administrator profile |
| UpdateAdministratorProfile | 21 | Admin: Update administrator profile |
| ViewAllProfiles | 22 | Admin: View all profiles |
| ViewSystemReports | 23 | Admin: View system reports |
| EditFacilitySettings | 24 | Admin: Edit facility settings |
### UserRole.cs
Enumeration defining the three primary user roles in the CliniCore system.

**Values:**

| Name | Value | Description |
|------|-------|-------------|
| Patient | 0 | Patient user with access to own medical records and appointments |
| Physician | 1 | Medical professional with access to patient management and clinical documentation |
| Administrator | 2 | System administrator with full access to user management and system settings |
### IIdentifiable.cs

### IUserProfile.cs
Interface defining the contract for all user profiles in the system.

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Id | Guid | get | Unique identifier for the profile |
| Username | string | get/set | User's login username |
| CreatedAt | DateTime | get | Timestamp of profile creation |
| Role | UserRole | get | User's role in the system |
| Entries | List<ProfileEntry> | get | Collection of profile data entries |
| IsValid | bool | get | Whether the profile is valid |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetValidationErrors()` | List<string> | Gets all validation error messages |
| `GetEntry(string key)` | ProfileEntry? | Retrieves a profile entry by its key |
| `GetValue<T>(string key)` | T? | Gets the typed value from a profile entry |
| `SetValue<T>(string key, T value)` | bool | Sets a typed value for a profile entry |
### PatientProfile.cs
Concrete implementation of a patient user profile with patient-specific properties and medical relationships.

**Inheritance:** AbstractUserProfile

**Properties:**

| Name                | Type       | Access  | Description                                             |
| ------------------- | ---------- | ------- | ------------------------------------------------------- |
| Role                | UserRole   | get     | Returns UserRole.Patient                                |
| Name                | string     | get     | Patient's full name from profile entries                |
| BirthDate           | DateTime   | get     | Patient's date of birth                                 |
| Gender              | Gender     | get     | Patient's gender identity                               |
| Race                | string     | get     | Patient's race from profile entries                     |
| Address             | string     | get     | Patient's address from profile entries                  |
| AppointmentIds      | List<Guid> | get     | Collection of appointment IDs for this patient          |
| ClinicalDocumentIds | List<Guid> | get     | Collection of clinical document IDs for this patient    |
| PrimaryPhysicianId  | Guid?      | get/set | ID of the patient's primary care physician, if assigned |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetProfileTemplate()` | IProfileTemplate | Returns a new PatientProfileTemplate instance |
| `ToString()` | string | Returns formatted patient information including name, age, demographics, physician, and counts of appointments and documents |
### PhysicianProfile.cs
Concrete implementation of a physician user profile with medical professional credentials and practice management features.

**Inheritance:** AbstractUserProfile

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Role | UserRole | get | Returns UserRole.Physician |
| Name | string | get | Physician's full name from profile entries |
| LicenseNumber | string | get | Medical license number from profile entries |
| GraduationDate | DateTime | get | Medical school graduation date from profile entries |
| Specializations | List<MedicalSpecialization> | get | List of medical specializations (1-5 required) |
| PatientIds | List<Guid> | get | Collection of patient IDs under this physician's care |
| AppointmentIds | List<Guid> | get | Collection of appointment IDs for this physician |
| StandardAvailability | Dictionary<DayOfWeek, List<UnavailableTimeInterval>> | get | Weekly availability patterns for scheduling |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetProfileTemplate()` | IProfileTemplate | Returns a new PhysicianProfileTemplate instance |
| `ToString()` | string | Returns formatted physician information including credentials, specializations, and practice statistics |
### ProfileBuilder.cs
Placeholder class for implementing the Builder pattern for complex profile construction.

**Note:** Currently marked as internal and awaiting implementation. This class is intended for future development to provide streamlined profile creation workflows with validation and step-by-step data entry.
### ProfileEntry.cs
Generic container class for storing and validating individual profile data fields with type safety and validation support.

**Base Class: ProfileEntry (abstract)**

**Properties:**

| Name         | Type   | Access             | Description                                                     |
| ------------ | ------ | ------------------ | --------------------------------------------------------------- |
| Key          | string | get, protected set | Unique key identifier for this entry                            |
| DisplayName  | string | get/set            | Human-readable display name                                     |
| IsRequired   | bool   | get/set            | Whether this entry is required                                  |
| ValueType    | Type   | get                | Abstract property returning the type of value stored            |
| IsValid      | bool   | get                | Abstract property indicating if the current value is valid      |
| ErrorMessage | string | get                | Abstract property returning validation error message if invalid |

**Methods:**

| Signature                                      | Returns | Description                                       |
| ---------------------------------------------- | ------- | ------------------------------------------------- |
| `ProfileEntry(string key, string displayName)` | -       | Protected constructor for the abstract base class |

**Generic Class: ProfileEntry\<T\>**

**Inheritance:** ProfileEntry

**Properties:**

| Name         | Type   | Access  | Description                                                            |
| ------------ | ------ | ------- | ---------------------------------------------------------------------- |
| Value        | T      | get/set | The typed value for this entry, validated on assignment                |
| ValueType    | Type   | get     | Returns typeof(T)                                                      |
| IsValid      | bool   | get     | Returns true if the validator passes for the current value             |
| ErrorMessage | string | get     | Returns the validation error message if invalid, empty string if valid |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ProfileEntry(string key, string displayName, bool isRequired = false, Func<T, bool>? validator = null, string? errorMessage = null)` | - | Creates a new typed profile entry with optional validation |
### ProfileEntryFactory.cs
Factory class providing standardized creation methods for all profile entry types with appropriate validators and configuration.

**Methods:**

| Signature                                                                                             | Returns                                       | Description                                                               |
| ----------------------------------------------------------------------------------------------------- | --------------------------------------------- | ------------------------------------------------------------------------- |
| `Create<T>(string key, string displayName, IValidator<T>? validator = null, bool isRequired = false)` | ProfileEntry\<T\>                             | Generic factory method to create a profile entry with optional validation |
| `CreateRequired<T>(string key, string displayName, IValidator<T>? validator = null)`                  | ProfileEntry\<T\>                             | Creates a required profile entry with validation                          |
| `CreateName()`                                                                                        | ProfileEntry\<string\>                        | Creates the Name entry with 2-100 character validation                    |
| `CreateAddress()`                                                                                     | ProfileEntry\<string\>                        | Creates the Address entry with max 200 character validation               |
| `CreateBirthDate()`                                                                                   | ProfileEntry\<DateTime\>                      | Creates the BirthDate entry with date range validation                    |
| `CreatePatientRace()`                                                                                 | ProfileEntry\<string\>                        | Creates the Race entry with max 50 character validation                   |
| `CreatePatientGender()`                                                                               | ProfileEntry\<Gender\>                        | Creates the Gender entry with enum validation                             |
| `CreatePhysicianLicenseNumber()`                                                                      | ProfileEntry\<string\>                        | Creates the LicenseNumber entry with format validation                    |
| `CreatePhysicianGraduationDate()`                                                                     | ProfileEntry\<DateTime\>                      | Creates the GraduationDate entry with date validation                     |
| `CreatePhysicianSpecializationList()`                                                                 | ProfileEntry\<List\<MedicalSpecialization\>\> | Creates the Specializations entry requiring 1-5 valid specializations     |
| `CreateEmail()`                                                                                       | ProfileEntry\<string\>                        | Creates the Email entry with email format validation                      |
### Profileservice.cs
Singleton service managing all user profiles in the system with thread-safe operations and multiple indexing strategies.

**Pattern:** Singleton

**Properties:**

| Name     | Type            | Access | Description                                        |
| -------- | --------------- | ------ | -------------------------------------------------- |
| Instance | Profileservice | get    | Gets the singleton instance of the Profileservice |
| Count    | int             | get    | Gets the total count of profiles in the service   |

**Methods:**

| Signature                                                                             | Returns                           | Description                                                                    |
| ------------------------------------------------------------------------------------- | --------------------------------- | ------------------------------------------------------------------------------ |
| `AddProfile(IUserProfile profile)`                                                    | bool                              | Adds a profile to the service, returns false if ID or username already exists |
| `RemoveProfile(Guid profileId)`                                                       | bool                              | Removes a profile from the service by ID                                      |
| `GetProfileById(Guid profileId)`                                                      | IUserProfile?                     | Gets a profile by ID, or null if not found                                     |
| `GetProfileByUsername(string username)`                                               | IUserProfile?                     | Gets a profile by username (case-insensitive), or null if not found            |
| `GetProfilesByType<T>()`                                                              | IEnumerable<T>                    | Gets all profiles of a specific type (e.g., PatientProfile)                    |
| `GetProfilesByRole(UserRole role)`                                                    | IEnumerable<IUserProfile>         | Gets all profiles with a specific role                                         |
| `GetAllPatients()`                                                                    | IEnumerable<PatientProfile>       | Gets all patient profiles                                                      |
| `GetAllPhysicians()`                                                                  | IEnumerable<PhysicianProfile>     | Gets all physician profiles                                                    |
| `GetAllAdministrators()`                                                              | IEnumerable<AdministratorProfile> | Gets all administrator profiles                                                |
| `SearchByName(string searchTerm)`                                                     | IEnumerable<IUserProfile>         | Searches for profiles by name (case-insensitive)                               |
| `UsernameExists(string username)`                                                     | bool                              | Checks if a username exists in the service                                    |
| `GetAllProfiles()`                                                                    | IEnumerable<IUserProfile>         | Gets all profiles in the service                                              |
| `GetStatistics()`                                                                     | ProfileserviceStatistics         | Gets statistics about the service (total, patients, physicians, admins)       |
| `Clear()`                                                                             | void                              | Clears all profiles from the service (for testing purposes)                   |
| `AssignPatientToPhysician(Guid patientId, Guid physicianId, bool setPrimary = false)` | bool                              | Establishes a physician-patient relationship, optionally setting as primary    |
| `GetPhysicianPatients(Guid physicianId)`                                              | IEnumerable<PatientProfile>       | Gets all patients for a specific physician                                     |
| `GetPatientPhysicians(Guid patientId)`                                                | IEnumerable<PhysicianProfile>     | Gets all physicians for a specific patient                                     |

**Helper Class: ProfileserviceStatistics**

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| TotalProfiles | int | get/set | Total number of profiles |
| PatientCount | int | get/set | Number of patient profiles |
| PhysicianCount | int | get/set | Number of physician profiles |
| AdministratorCount | int | get/set | Number of administrator profiles |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ToString()` | string | Returns formatted statistics string |
## ProfileTemplates
### AbstractProfileTemplate.cs
Base template class implementing the Template Method pattern for defining required profile entries across different user types. This abstract class provides the algorithmic skeleton for building profile entry lists, separating common human entries (name, address, birthdate) from role-specific entries. Subclasses override AddSpecificEntries to customize the profile structure for each user type while maintaining consistent core data requirements.

**Inheritance:** IProfileTemplate

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetRequiredEntries()` | `List<ProfileEntry>` | Public method that retrieves all required profile entries by calling AddCommonEntries and AddSpecificEntries in sequence |
| `AddCommonEntries(List<ProfileEntry> entries)` | `void` | Protected virtual method that adds common human data entries (name, address, birthdate) using ProfileEntryFactory |
| `AddSpecificEntries(List<ProfileEntry> entries)` | `void` | Protected abstract method that subclasses must override to add role-specific profile entries |
### AdministratorProfileTemplate.cs
Profile template for administrator users defining required profile entries specific to system administrators. This class extends AbstractProfileTemplate by adding administrator-specific fields such as email address for system communications. It ensures administrators have appropriate contact information while inheriting common person attributes.

**Inheritance:** AbstractProfileTemplate, IProfileTemplate

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddSpecificEntries(List<ProfileEntry> entries)` | `void` | Protected override that adds administrator-specific profile entries (email address) using ProfileEntryFactory.CreateEmail() |
### IProfileTemplate.cs
Interface defining the contract for profile template implementations that specify required profile entries. This simple interface requires a GetRequiredEntries method that returns the list of ProfileEntry objects needed for a specific profile type, enabling polymorphic profile template usage.

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `GetRequiredEntries()` | `List<ProfileEntry>` | Returns the complete list of ProfileEntry objects required for this profile type |
### PatientProfileTemplate.cs
Profile template for patient users defining required profile entries specific to medical patient records. This class extends AbstractProfileTemplate by adding patient-specific demographic fields including gender and race for comprehensive medical record-keeping. It ensures patient profiles capture all necessary demographic information for quality healthcare delivery.

**Inheritance:** AbstractProfileTemplate, IProfileTemplate

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddSpecificEntries(List<ProfileEntry> entries)` | `void` | Protected override that adds patient-specific profile entries (gender, race) using ProfileEntryFactory.CreatePatientGender() and CreatePatientRace() |
### PhysicianProfileTemplate.cs
Profile template for physician users defining required profile entries specific to medical professionals. This class extends AbstractProfileTemplate by adding physician-specific credential fields including license number, graduation date, and medical specializations. It ensures physician profiles capture all necessary professional qualifications and certifications required for medical practice.

**Inheritance:** AbstractProfileTemplate, IProfileTemplate

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddSpecificEntries(List<ProfileEntry> entries)` | `void` | Protected override that adds physician-specific profile entries (license number, graduation date, specializations list) using ProfileEntryFactory methods |
## Validation
### AbstractValidator.cs
Base class providing common infrastructure for all validators in the system.

**Inheritance:** IValidator\<T\>

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| ErrorMessage | string | public get, protected set | The error message returned when validation fails |

**Methods:**

| Signature                                | Returns | Description                                                                            |
| ---------------------------------------- | ------- | -------------------------------------------------------------------------------------- |
| `AbstractValidator(string errorMessage)` | -       | Protected constructor requiring an error message, throws ArgumentNullException if null |
| `IsValid(T value)`                       | bool    | Abstract method that subclasses must implement to define validation logic              |
### CompositeValidator.cs
Validator implementing the Composite pattern to combine multiple validators into a single validation chain.

**Inheritance:** IValidator\<T\>

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| ErrorMessage | string | public get | Returns the error message from the first failed validator, or empty string if all pass |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `CompositeValidator(params IValidator<T>[] validators)` | - | Constructor accepting array of validators, throws ArgumentNullException if null |
| `IsValid(T value)` | bool | Executes validators sequentially, short-circuits on first failure |
| `And(IValidator<T> validator)` | CompositeValidator<T> | Adds a validator to the chain and returns this for fluent chaining |
### IValidator.cs
Generic interface defining the contract for all validators in the system.

**Inheritance:** None (base interface)

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| ErrorMessage | string | get | Provides detailed error message when validation fails |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `IsValid(T value)` | bool | Returns true if the value passes validation, false otherwise |
### ValidatorFactory.cs
Static factory class providing convenient methods for creating common validators with pre-configured rules and error messages.

**Inheritance:** None (static class)

**Constants:**

| Name | Type | Value | Description |
|------|------|-------|-------------|
| MAX_PATIENT_AGE | int | 150 | Maximum allowed patient age in years |
| MAX_GRAD_YEARS_ELAPSED | int | 75 | Maximum years since physician graduation |
| MIN_NAME_CHARS | int | 2 | Minimum characters for names |
| MAX_NAME_CHARS | int | 100 | Maximum characters for names |
| MIN_LICENSE_CHARS | int | 6 | Minimum characters for license numbers |
| MAX_LICENSE_CHARS | int | 20 | Maximum characters for license numbers |
| EMAIL_ERROR_STR | string | "Must be a valid email address" | Email validation error message |
| LICENSE_ERROR_STR | string | "License number must be 6-20 alphanumeric characters" | License validation error message |
| PATIENT_NAME_NULL_ERROR_STR | string | "Patient name is required" | Patient name required error |
| PHYSICIAN_NAME_NULL_ERROR_STR | string | "Physician name is required" | Physician name required error |
| BIRTHDATE_NULL_ERROR_STR | string | "Birth date is required" | Birth date required error |
| BIRTHDATE_SIZE_ERROR_STR | string | (computed) | Birth date range error message |
| GRADDATE_SIZE_ERROR_STR | string | (computed) | Graduation date range error message |
| PATIENT_NAME_SIZE_ERROR_STR | string | (computed) | Patient name length error message |
| PHYSICIAN_NAME_SIZE_ERROR_STR | string | (computed) | Physician name length error message |

**Methods:**

| Signature                                                                                                               | Returns                 | Description                                                                   |
| ----------------------------------------------------------------------------------------------------------------------- | ----------------------- | ----------------------------------------------------------------------------- |
| `Required<T>(string? errorMessage = null)`                                                                              | IValidator\<T\>         | Creates a required field validator                                            |
| `Composite<T>(params IValidator<T>[] validators)`                                                                       | IValidator\<T\>         | Creates a composite validator from multiple validators                        |
| `StringLength(int? minLength = null, int? maxLength = null, string? errorMessage = null)`                               | IValidator\<string\>    | Creates a string length validator with optional min/max bounds                |
| `Regex(string pattern, string errorMessage, RegexOptions options = RegexOptions.None)`                                  | IValidator\<string\>    | Creates a regex pattern validator                                             |
| `Email()`                                                                                                               | IValidator\<string\>    | Creates an email format validator                                             |
| `LicenseNumber()`                                                                                                       | IValidator\<string\>    | Creates a physician license number validator (6-20 alphanumeric)              |
| `DateRange(DateTime? minDate = null, DateTime? maxDate = null, string? errorMessage = null)`                            | IValidator\<DateTime\>  | Creates a date range validator                                                |
| `BirthDate()`                                                                                                           | IValidator\<DateTime\>  | Creates a birth date validator (within last 150 years)                        |
| `GraduationDate()`                                                                                                      | IValidator\<DateTime\>  | Creates a graduation date validator (within last 75 years)                    |
| `UsernameUniqueness(Profileservice profileservice, string? errorMessage = null)`                                      | IValidator\<string\>    | Creates a username uniqueness validator                                       |
| `PatientName()`                                                                                                         | IValidator<string>      | Creates a composite validator for patient names (required, 2-100 chars)       |
| `PhysicianName()`                                                                                                       | IValidator\<string\>    | Creates a composite validator for physician names (required, 2-100 chars)     |
| `PatientBirthDate()`                                                                                                    | IValidator\<DateTime\>  | Creates a composite validator for patient birth dates (required, valid range) |
| `OneOf<T>(IEnumerable<T> validValues, string? errorMessage = null, bool allowNull = true)`                              | IValidator\<T\>         | Creates a validator for specific set of allowed values                        |
| `OneOf<T>(string? errorMessage = null, bool allowNull = true, params T[] validValues)`                                  | IValidator<T>           | Creates a validator for specific allowed values (params overload)             |
| `ValidEnum<T>(string? errorMessage = null, bool allowNull = true) where T : struct, Enum`                               | IValidator\<T\>         | Creates a validator for C# enum types                                         |
| `ValidEnum<T>(string? errorMessage = null) where T : struct, Enum`                                                      | IValidator\<T?\>        | Creates a validator for nullable C# enum types                                |
| `List<T>(int? minCount = null, int? maxCount = null, IValidator<T>? itemValidator = null, string? errorMessage = null)` | IValidator\<List\<T\>\> | Creates a list validator with optional count bounds and item validation       |
## Validators
### DateRangeValidator.cs
### EnumValidator.cs
### ListValidator.cs
### RegexValidator.cs
### RequiredValidator.cs
### StringLengthValidator.cs
### UsernameUniquenessValidator.cs
## DTO
### PatientDTO.cs
A data transfer object placeholder for patient data serialization and transfer between system layers. Currently marked as internal and awaiting implementation for API and cross-layer communication needs.
## Facilities
### Facility.cs
Placeholder class for representing individual medical facilities within the CliniCore system. Currently marked as internal and awaiting implementation for multi-facility support including facility-specific settings, location data, and operational hours.
### HealthSystem.cs
Placeholder class for representing health system organizations that may contain multiple facilities. Currently marked as internal and awaiting implementation for enterprise-level health system management and multi-facility coordination.
## Repositories
Namespace containing interfaces for data persistence, retrieval, manipulation, and deletion, also including concrete classes that handle the disparate data continuity strategies (DB, in memory, file-based). Facilitates flexible refactoring efforts thanks to these classes, as they collectively enable the core library to support parameterization of storage mediums.
## InMemory
Namespace containing six concrete classes, each of which is dedicated to implementing its corresponding interface in the parent namespace Repositories. This namespace's classes delegates all data handling solely to the RAM, hence any data accrued during the lifetime of the application will be lost upon termination. Use of this persistence strategy incurs dependency on CoreServiceBootstrapper for creation of development credentials.
### InMemoryAdministratorRepository.cs
### InMemoryAppointmentRepository.cs
### InMemoryClinicalDocumentRepository.cs
### InMemoryPhysicianRepository.cs
### InMemoryRepositoryBase.cs
### IAdministratorRepository.cs
### IAppointmentRepository.cs
### IClinicalDocumentRepository.cs
### IPatientRepository.cs
### IPhysicianRepository.cs
An interface for interacting with a repository containing physician entities, it realizes IRepository via type PhysicianProfile, who implements IIdentifiable.

**Realizes:** `IRepository<PhysicianProfile>`

**Properties:** None

**Methods:**

| Signature                                          | Returns                           | Description
| -------------------------------------------------- | --------------------------------- | ------------
| `GetByUsername(string username)`                   | `PhysicianProfile?`               | Gets a physician by their username |
| `FindBySpecialization(MedicalSpecialization spec)` | `IEnumerablePhysicianProfile>`    | Finds physicians by medical specialization |
| `GetAvailableOn(DateTime date)`                    | `IEnumerable<PhysicianProfile>`   | Gets physicians available on a specific date |

### IRepository.cs
Generic profile entity container interface that, when implemented, requires a class to provide implementations for all invariant CRUD operations. Defined as `IRepository<T> where T : class, IIdentifiable`, it asserts that it may only be used by types that implement the IIdentifiable interface, which simply indicates that the type possesses a `Guid` property.

**Realizes:** None

**Properties: None

**Methods:**                                              

| Signature                             | Returns          | Description                             |
| --------------------------------------| ---------------  | --------------------------------------- |
| `GetById(Guid id)`                 | `T?`             | Fetches instance of entity type by GUID |
| `GetAll()`             | `IEnumerable&lt;T&gt;` | Gathers all entities in an enumerable   |
| `Add(T entity)`                  | `void`           | Adds an entity to the repository        |
| `Update(T entity)`               | `void`           | Updates an entity in the repository     |
| `Delete(Guid id)`                | `void`           | Deletes an entity in the repository     |
| `Search(string query)` | `IEnumerable&lt;T&gt;` | Gathers entities satisfying query       |

## Scheduling
### AbstractTimeInterval.cs
Base implementation for all time interval types that provides common functionality for temporal operations.

**Inheritance:** ITimeInterval (abstract)

**Properties:**

| Name        | Type      | Access                      | Description                                          |
| ----------- | --------- | --------------------------- | ---------------------------------------------------- |
| Id          | Guid      | get, protected set          | Unique identifier for this time interval             |
| Start       | DateTime  | get, protected internal set | Start time of the interval                           |
| End         | DateTime  | get, protected internal set | End time of the interval                             |
| Duration    | TimeSpan  | get                         | Duration of the interval (calculated as End - Start) |
| Description | string    | virtual get, protected set  | Description or title of this time interval           |
| DayOfWeek   | DayOfWeek | get                         | Gets the day of week for this interval               |

**Methods:**

| Signature                                                                                     | Returns            | Description                                                                              |
| --------------------------------------------------------------------------------------------- | ------------------ | ---------------------------------------------------------------------------------------- |
| `AbstractTimeInterval(DateTime start, DateTime end, string description = "")`                 | -                  | Protected constructor initializing start, end, and description with validation           |
| `Overlaps(ITimeInterval other)`                                                               | bool               | Checks if this interval overlaps with another                                            |
| `Contains(DateTime moment)`                                                                   | bool               | Checks if this interval contains a specific point in time                                |
| `Contains(ITimeInterval other)`                                                               | bool               | Checks if this interval completely contains another interval                             |
| `IsAdjacentTo(ITimeInterval other)`                                                           | bool               | Checks if this interval is adjacent to another (touching but not overlapping)            |
| `IsValid()`                                                                                   | bool               | Validates that the interval is valid                                                     |
| `GetValidationErrors()`                                                                       | List&lt;string&gt; | Gets validation errors if the interval is not valid                                      |
| `GetSpecificValidationErrors()`                                                               | List&lt;string&gt; | Protected virtual method for derived classes to add specific validation rules            |
| `IsWithinBusinessHours()`                                                                     | bool               | Checks if this interval occurs within business hours (M-F 8am-5pm)                       |
| `MergeWith(ITimeInterval other)`                                                              | ITimeInterval?     | Attempts to merge with another interval if they overlap or are adjacent                  |
| `CreateMergedInterval(DateTime start, DateTime end, string description, ITimeInterval other)` | ITimeInterval?     | Protected abstract method for derived classes to create a properly typed merged interval |
| `IsBusinessHoursSlot(DateTime start, DateTime end)`                                           | bool               | Static helper to check if a given time slot is within business hours                     |
| `ToString()`                                                                                  | string             | Gets a display string for this interval                                                  |
| `Equals(object? obj)`                                                                         | bool               | Equality comparison based on Id                                                          |
| `GetHashCode()`                                                                               | int                | Hash code for dictionary storage based on Id                                             |

### AppointmentTimeInterval.cs
Represents a scheduled medical appointment time interval with comprehensive appointment lifecycle management.

**Inheritance:** AbstractTimeInterval

**Nested Types:**

| Name              | Description                                                                                                                                                                                   |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| StandardDurations | Static class containing standard appointment duration constants: QuickCheckup (15 min), StandardVisit (30 min), ExtendedConsultation (45 min), ComprehensiveExam (60 min), Procedure (90 min) |

**Properties:**

| Name               | Type              | Access                      | Description                                                 |
| ------------------ | ----------------- | --------------------------- | ----------------------------------------------------------- |
| PatientId          | Guid              | get/set                     | The patient this appointment is for                         |
| PhysicianId        | Guid              | get/set                     | The physician conducting this appointment                   |
| Status             | AppointmentStatus | get/set                     | Current status of the appointment                           |
| CreatedAt          | DateTime          | get/set                     | When this appointment was created                           |
| ModifiedAt         | DateTime?         | get/set                     | When this appointment was last modified                     |
| AppointmentType    | string            | get/set                     | Type of appointment based on duration                       |
| ReasonForVisit     | string?           | get/set                     | Reason for visit / chief complaint                          |
| Notes              | string?           | get/set                     | Any notes about this appointment                            |
| ClinicalDocumentId | Guid?             | get/set                     | ID of the clinical document created during this appointment |
| RescheduledFromId  | Guid?             | get/set                     | If this appointment was rescheduled from another            |
| CancellationReason | string?           | get/set                     | If this appointment was cancelled, the reason               |
| Description        | string            | override get, protected set | Override to add appointment-specific description            |

**Methods:**

| Signature                                                                                                                                                                  | Returns                 | Description                                                                    |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------- | ------------------------------------------------------------------------------ |
| `AppointmentTimeInterval(DateTime start, DateTime end, Guid patientId, Guid physicianId, string description = "", AppointmentStatus status = AppointmentStatus.Scheduled)` | -                       | Constructor creating a new appointment with specified parameters               |
| `CanReschedule()`                                                                                                                                                          | bool                    | Checks if this appointment can be rescheduled (Scheduled status and in future) |
| `CanCancel()`                                                                                                                                                              | bool                    | Checks if this appointment can be cancelled (Scheduled status and in future)   |
| `MarkCompleted()`                                                                                                                                                          | void                    | Marks the appointment as completed                                             |
| `Cancel(string reason = "")`                                                                                                                                               | void                    | Marks the appointment as cancelled with optional reason                        |
| `MarkNoShow()`                                                                                                                                                             | void                    | Marks the appointment as no-show                                               |
| `UpdateDuration(int durationMinutes)`                                                                                                                                      | void                    | Updates the duration of this appointment by changing the end time              |
| `Reschedule(DateTime newStart, DateTime newEnd)`                                                                                                                           | AppointmentTimeInterval | Creates a rescheduled copy of this appointment                                 |
| `ConflictsWith(AppointmentTimeInterval other)`                                                                                                                             | bool                    | Checks if this appointment conflicts with another for the same physician       |
| `GetSpecificValidationErrors()`                                                                                                                                            | List&lt;string&gt;      | Protected override to add appointment-specific validation                      |
| `CreateMergedInterval(DateTime start, DateTime end, string description, ITimeInterval other)`                                                                              | ITimeInterval?          | Protected override to create a merged appointment interval                     |
| `ToString()`                                                                                                                                                               | string                  | Returns formatted string with appointment details                              |

### ITimeInterval.cs
Interface defining the contract for objects representing distinct intervals in time.

**Properties:**

| Name        | Type     | Access | Description                                |
| ----------- | -------- | ------ | ------------------------------------------ |
| Start       | DateTime | get    | Start time of the interval                 |
| End         | DateTime | get    | End time of the interval                   |
| Duration    | TimeSpan | get    | Duration of the interval                   |
| Id          | Guid     | get    | Unique identifier for this time interval   |
| Description | string   | get    | Description or title of this time interval |

**Methods:**

| Signature                           | Returns            | Description                                                                       |
| ----------------------------------- | ------------------ | --------------------------------------------------------------------------------- |
| `Overlaps(ITimeInterval other)`     | bool               | Checks if this interval overlaps with another                                     |
| `Contains(DateTime moment)`         | bool               | Checks if this interval contains a specific point in time                         |
| `Contains(ITimeInterval other)`     | bool               | Checks if this interval completely contains another interval                      |
| `IsAdjacentTo(ITimeInterval other)` | bool               | Checks if this interval is adjacent to another (touching but not overlapping)     |
| `IsValid()`                         | bool               | Validates that the interval is valid (end after start, reasonable duration, etc.) |
| `GetValidationErrors()`             | List&lt;string&gt; | Gets validation errors if the interval is not valid                               |
| `IsWithinBusinessHours()`           | bool               | Checks if this interval occurs within business hours (M-F 8am-5pm)                |
| `MergeWith(ITimeInterval other)`    | ITimeInterval?     | Attempts to merge with another interval if they overlap or are adjacent           |

### PhysicianSchedule.cs
Manages the schedule for an individual physician including appointments, unavailable blocks, and standard weekly availability patterns.

**Properties:**

| Name                 | Type                                                        | Access  | Description                                                             |
| -------------------- | ----------------------------------------------------------- | ------- | ----------------------------------------------------------------------- |
| PhysicianId          | Guid                                                        | get     | The physician this schedule belongs to                                  |
| Appointments         | IReadOnlyList&lt;AppointmentTimeInterval&gt;                | get     | All appointments for this physician                                     |
| UnavailableBlocks    | IReadOnlyList&lt;UnavailableTimeInterval&gt;                | get     | All unavailable blocks for this physician                               |
| StandardAvailability | Dictionary&lt;DayOfWeek, (TimeSpan Start, TimeSpan End)&gt; | get/set | Standard weekly availability pattern (e.g., which days physician works) |

**Methods:**

| Signature                                                              | Returns                                    | Description                                                                                                 |
| ---------------------------------------------------------------------- | ------------------------------------------ | ----------------------------------------------------------------------------------------------------------- |
| `PhysicianSchedule(Guid physicianId)`                                  | -                                          | Constructor creating a new physician schedule with standard M-F 8am-5pm availability                        |
| `TryAddAppointment(AppointmentTimeInterval appointment)`               | bool                                       | Adds an appointment to the schedule if no conflicts exist                                                   |
| `RemoveAppointment(Guid appointmentId)`                                | bool                                       | Removes an appointment from the schedule                                                                    |
| `AddUnavailableBlock(UnavailableTimeInterval block)`                   | void                                       | Adds an unavailable block to the schedule                                                                   |
| `HasConflict(AppointmentTimeInterval proposedAppointment)`             | bool                                       | Checks if there's a conflict for a proposed appointment                                                     |
| `IsTimeSlotAvailable(DateTime start, DateTime end)`                    | bool                                       | Checks if a specific time slot is available                                                                 |
| `GetAppointmentsForDate(DateTime date)`                                | IEnumerable&lt;AppointmentTimeInterval&gt; | Gets all appointments for a specific date                                                                   |
| `GetAppointmentsInRange(DateTime startDate, DateTime endDate)`         | IEnumerable&lt;AppointmentTimeInterval&gt; | Gets all appointments in a date range                                                                       |
| `FindNextAvailableSlot(TimeSpan duration, DateTime? afterTime = null)` | DateTime?                                  | Finds the next available time slot of a given duration                                                      |
| `GetAvailabilitySummary(DateTime date)`                                | ScheduleAvailabilitySummary                | Gets availability summary for a date including total appointments, booked hours, and utilization percentage |
| `ClearOldAppointments(DateTime beforeDate)`                            | int                                        | Clears old appointments from the schedule                                                                   |

**Helper Class: ScheduleAvailabilitySummary**

**Properties:**

| Name                  | Type     | Access  | Description                  |
| --------------------- | -------- | ------- | ---------------------------- |
| Date                  | DateTime | get/set | Date for this summary        |
| TotalAppointments     | int      | get/set | Total number of appointments |
| TotalBookedHours      | double   | get/set | Total hours booked           |
| TotalAvailableHours   | double   | get/set | Total hours available        |
| UtilizationPercentage | double   | get/set | Utilization percentage       |

### UnavailableTimeInterval.cs
Represents time intervals when no appointments can be scheduled due to various reasons.

**Inheritance:** AbstractTimeInterval

**Enums:**

| Name                 | Values                                                                                           | Description                               |
| -------------------- | ------------------------------------------------------------------------------------------------ | ----------------------------------------- |
| UnavailabilityReason | NonBusinessHours, Lunch, Meeting, Vacation, SickLeave, Holiday, Administrative, Emergency, Other | Categorizes the reason for unavailability |

**Properties:**

| Name              | Type                 | Access                      | Description                                                         |
| ----------------- | -------------------- | --------------------------- | ------------------------------------------------------------------- |
| Reason            | UnavailabilityReason | get/set                     | The reason for unavailability                                       |
| PhysicianId       | Guid?                | get/set                     | Optional: specific physician this applies to (null = facility-wide) |
| IsRecurring       | bool                 | get/set                     | Whether this is a recurring unavailability (e.g., lunch every day)  |
| RecurrencePattern | string?              | get/set                     | If recurring, the pattern (daily, weekly, etc.)                     |
| Description       | string               | override get, protected set | Override to provide meaningful description                          |

**Methods:**

| Signature                                                                                                                               | Returns                             | Description                                                                   |
| --------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ----------------------------------------------------------------------------- |
| `UnavailableTimeInterval(DateTime start, DateTime end, UnavailabilityReason reason, string description = "", Guid? physicianId = null)` | -                                   | Constructor creating a new unavailable time interval                          |
| `BlocksTimeSlot(DateTime proposedStart, DateTime proposedEnd)`                                                                          | bool                                | Checks if this unavailability blocks a specific time slot                     |
| `CreateLunchBreak(DateTime date, Guid? physicianId = null)`                                                                             | UnavailableTimeInterval             | Static factory method to create standard lunch break intervals (12pm-1pm)     |
| `CreateNonBusinessHours(DateTime date)`                                                                                                 | List&lt;UnavailableTimeInterval&gt; | Static factory method to create non-business hours blocks for a specific date |
| `CreateWeekendBlocks(DateTime weekStart)`                                                                                               | List&lt;UnavailableTimeInterval&gt; | Static factory method to create weekend blocks                                |
| `GetSpecificValidationErrors()`                                                                                                         | List&lt;string&gt;                  | Protected override to validate unavailable intervals                          |
| `CreateMergedInterval(DateTime start, DateTime end, string description, ITimeInterval other)`                                           | ITimeInterval?                      | Protected override to create a merged unavailable interval                    |
| `ToString()`                                                                                                                            | string                              | Returns formatted string with unavailability details                          |

## BookingStrategies
### FirstAvailableBookingStrategy.cs
Implements a booking strategy that finds the earliest available appointment slots using the Strategy pattern.

**Inheritance:** IBookingStrategy

**Properties:**

| Name         | Type   | Access | Description                                                                                  |
| ------------ | ------ | ------ | -------------------------------------------------------------------------------------------- |
| StrategyName | string | get    | Returns "First Available"                                                                    |
| Description  | string | get    | Returns "Finds the earliest available appointment slot that meets the duration requirements" |

**Methods:**

| Signature                                                                                                                                                                                   | Returns                     | Description                               |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | ----------------------------------------- |
| `FindNextAvailableSlot(PhysicianSchedule physicianSchedule, TimeSpan requestedDuration, DateTime earliestTime, List<UnavailableTimeInterval>? facilityUnavailable = null)`                  | AppointmentSlot?            | Finds the next available appointment slot |
| `FindAvailableSlots(PhysicianSchedule physicianSchedule, TimeSpan requestedDuration, DateTime earliestTime, int maxResults = 5, List<UnavailableTimeInterval>? facilityUnavailable = null)` | List&lt;AppointmentSlot&gt; | Finds multiple available slots            |

### IBookingStrategy.cs
Interface for appointment booking strategies using the Strategy pattern.

**Properties:**

| Name         | Type   | Access | Description                                   |
| ------------ | ------ | ------ | --------------------------------------------- |
| StrategyName | string | get    | Gets the name of this booking strategy        |
| Description  | string | get    | Gets a description of how this strategy works |

**Methods:**

| Signature                                                                                                                                                                                   | Returns                     | Description                                                     |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | --------------------------------------------------------------- |
| `FindNextAvailableSlot(PhysicianSchedule physicianSchedule, TimeSpan requestedDuration, DateTime earliestTime, List<UnavailableTimeInterval>? facilityUnavailable = null)`                  | AppointmentSlot?            | Finds the next available appointment slot based on the strategy |
| `FindAvailableSlots(PhysicianSchedule physicianSchedule, TimeSpan requestedDuration, DateTime earliestTime, int maxResults = 5, List<UnavailableTimeInterval>? facilityUnavailable = null)` | List&lt;AppointmentSlot&gt; | Finds multiple available slots based on the strategy            |

## Management
### ScheduleConflictDetector.cs
Resolves scheduling conflicts using the Chain of Responsibility pattern combined with booking strategies to find alternative time slots.

**Methods:**

| Signature                                                                                                                                                                                           | Returns                 | Description                                                                                       |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------- | ------------------------------------------------------------------------------------------------- |
| `ScheduleConflictDetector()`                                                                                                                                                                        | -                       | Default constructor using FirstAvailableBookingStrategy                                           |
| `ScheduleConflictDetector(IBookingStrategy bookingStrategy)`                                                                                                                                        | -                       | Constructor accepting a custom booking strategy                                                   |
| `CheckForConflicts(AppointmentTimeInterval proposedAppointment, PhysicianSchedule physicianSchedule, List<UnavailableTimeInterval>? facilityUnavailable = null, Guid? excludeAppointmentId = null)` | ConflictCheckResult     | Detects all conflicts for a proposed appointment                                                  |
| `FindAlternative(ConflictCheckResult conflictResult, PhysicianSchedule physicianSchedule, List<UnavailableTimeInterval>? facilityUnavailable = null)`                                               | ConflictDetectionResult | Attempts to resolve conflicts by suggesting alternative times using the injected IBookingStrategy |
| `AddStrategy(IConflictDetectionStrategy strategy)`                                                                                                                                                  | void                    | Adds a custom conflict resolution strategy                                                        |

**Helper Classes:**

**ConflictCheckResult:**

| Property               | Type                           | Description                     |
| ---------------------- | ------------------------------ | ------------------------------- |
| ProposedAppointment    | AppointmentTimeInterval        | The proposed appointment        |
| HasConflicts           | bool                           | Whether conflicts were detected |
| Conflicts              | List&lt;ScheduleConflict&gt;   | List of detected conflicts      |
| AlternativeSuggestions | List&lt;TimeSlotSuggestion&gt; | List of alternative time slots  |

**ScheduleConflict:**

| Property            | Type           | Description                                                                                                         |
| ------------------- | -------------- | ------------------------------------------------------------------------------------------------------------------- |
| Type                | ConflictType   | Type of conflict (DoubleBooking, UnavailableTime, OutsideBusinessHours, TooShort, TooLong, Overlap, Holiday, Other) |
| Description         | string         | Description of the conflict                                                                                         |
| ConflictingInterval | ITimeInterval? | The conflicting interval if applicable                                                                              |
| CanOverride         | bool           | Whether this conflict can be overridden                                                                             |

**ConflictDetectionResult:**

| Property            | Type                           | Description                       |
| ------------------- | ------------------------------ | --------------------------------- |
| OriginalAppointment | AppointmentTimeInterval        | The original proposed appointment |
| Conflicts           | List&lt;ScheduleConflict&gt;   | List of detected conflicts        |
| Resolved            | bool                           | Whether conflicts were resolved   |
| AlternativeSlots    | List&lt;TimeSlotSuggestion&gt; | List of alternative time slots    |
| RecommendedSlot     | TimeSlotSuggestion?            | Recommended alternative slot      |

**TimeSlotSuggestion:**

| Property | Type     | Description                  |
| -------- | -------- | ---------------------------- |
| Start    | DateTime | Start time of suggested slot |
| End      | DateTime | End time of suggested slot   |
| Reason   | string   | Reason for this suggestion   |
## Service
### ClinicalDocumentService.cs
A singleton service for managing all clinical documents in the system with thread-safe operations.

**Pattern:** Singleton

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Instance | ClinicalDocumentService | get | Gets the singleton instance of the service |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AddDocument(ClinicalDocument document)` | bool | Adds document to service with duplicate and appointment checks, returns false if already exists |
| `RemoveDocument(Guid documentId)` | bool | Removes document from all indices, returns false if not found |
| `GetDocumentById(Guid documentId)` | ClinicalDocument? | Retrieves document by ID, returns null if not found |
| `GetDocumentByAppointment(Guid appointmentId)` | ClinicalDocument? | Retrieves document associated with an appointment, returns null if not found |
| `GetPatientDocuments(Guid patientId)` | IEnumerable&lt;ClinicalDocument&gt; | Gets all documents for a patient ordered by creation date descending |
| `GetPhysicianDocuments(Guid physicianId)` | IEnumerable&lt;ClinicalDocument&gt; | Gets all documents created by a physician ordered by creation date descending |
| `GetDocumentsInDateRange(DateTime startDate, DateTime endDate, Guid? patientId = null, Guid? physicianId = null)` | IEnumerable&lt;ClinicalDocument&gt; | Gets documents within date range with optional patient/physician filters |
| `SearchByDiagnosis(string diagnosisText)` | IEnumerable&lt;ClinicalDocument&gt; | Searches documents containing specified diagnosis text or ICD-10 code |
| `SearchByMedication(string medicationName)` | IEnumerable&lt;ClinicalDocument&gt; | Searches documents containing specified medication name |
| `GetIncompleteDocuments(Guid? physicianId = null)` | IEnumerable&lt;ClinicalDocument&gt; | Gets all incomplete documents with optional physician filter |
| `GetMostRecentPatientDocument(Guid patientId)` | ClinicalDocument? | Gets the most recent document for a patient |
| `DocumentExists(Guid documentId)` | bool | Checks if a document exists in the service |
| `AppointmentHasDocument(Guid appointmentId)` | bool | Checks if an appointment already has an associated document |
| `GetStatistics()` | ClinicalDocumentStatistics | Gets statistics about documents in the service |
| `GetAllDocuments()` | IEnumerable&lt;ClinicalDocument&gt; | Gets all documents ordered by creation date descending |
| `Clear()` | void | Clears all documents from the service (for testing purposes) |

**Helper Class: ClinicalDocumentStatistics**

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| TotalDocuments | int | get/set | Total number of documents |
| CompletedDocuments | int | get/set | Number of completed documents |
| IncompleteDocuments | int | get/set | Number of incomplete documents |
| UniquePatients | int | get/set | Number of unique patients with documents |
| UniquePhysicians | int | get/set | Number of unique physicians with documents |
| TotalDiagnoses | int | get/set | Total number of diagnoses across all documents |
| TotalPrescriptions | int | get/set | Total number of prescriptions across all documents |
| DocumentsToday | int | get/set | Number of documents created today |
| DocumentsThisWeek | int | get/set | Number of documents created in the past 7 days |
| CompletionRate | double | get | Completion rate percentage (calculated property) |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ToString()` | string | Returns formatted statistics string with document counts and completion rate |

### ProfileService.cs
Singleton registry managing all user profiles in the system with thread-safe operations and multiple indexing strategies.

**Pattern:** Singleton

**Properties:**

| Name     | Type            | Access | Description                                        |
| -------- | --------------- | ------ | -------------------------------------------------- |
| Instance | ProfileRegistry | get    | Gets the singleton instance of the ProfileRegistry |
| Count    | int             | get    | Gets the total count of profiles in the registry   |

**Methods:**

| Signature                                                                             | Returns                           | Description                                                                    |
| ------------------------------------------------------------------------------------- | --------------------------------- | ------------------------------------------------------------------------------ |
| `AddProfile(IUserProfile profile)`                                                    | bool                              | Adds a profile to the registry, returns false if ID or username already exists |
| `RemoveProfile(Guid profileId)`                                                       | bool                              | Removes a profile from the registry by ID                                      |
| `GetProfileById(Guid profileId)`                                                      | IUserProfile?                     | Gets a profile by ID, or null if not found                                     |
| `GetProfileByUsername(string username)`                                               | IUserProfile?                     | Gets a profile by username (case-insensitive), or null if not found            |
| `GetProfilesByType<T>()`                                                              | IEnumerable<T>                    | Gets all profiles of a specific type (e.g., PatientProfile)                    |
| `GetProfilesByRole(UserRole role)`                                                    | IEnumerable<IUserProfile>         | Gets all profiles with a specific role                                         |
| `GetAllPatients()`                                                                    | IEnumerable<PatientProfile>       | Gets all patient profiles                                                      |
| `GetAllPhysicians()`                                                                  | IEnumerable<PhysicianProfile>     | Gets all physician profiles                                                    |
| `GetAllAdministrators()`                                                              | IEnumerable<AdministratorProfile> | Gets all administrator profiles                                                |
| `SearchByName(string searchTerm)`                                                     | IEnumerable<IUserProfile>         | Searches for profiles by name (case-insensitive)                               |
| `UsernameExists(string username)`                                                     | bool                              | Checks if a username exists in the registry                                    |
| `GetAllProfiles()`                                                                    | IEnumerable<IUserProfile>         | Gets all profiles in the registry                                              |
| `GetStatistics()`                                                                     | ProfileRegistryStatistics         | Gets statistics about the registry (total, patients, physicians, admins)       |
| `Clear()`                                                                             | void                              | Clears all profiles from the registry (for testing purposes)                   |
| `AssignPatientToPhysician(Guid patientId, Guid physicianId, bool setPrimary = false)` | bool                              | Establishes a physician-patient relationship, optionally setting as primary    |
| `GetPhysicianPatients(Guid physicianId)`                                              | IEnumerable<PatientProfile>       | Gets all patients for a specific physician                                     |
| `GetPatientPhysicians(Guid patientId)`                                                | IEnumerable<PhysicianProfile>     | Gets all physicians for a specific patient                                     |

**Helper Class: ProfileRegistryStatistics**

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| TotalProfiles | int | get/set | Total number of profiles |
| PatientCount | int | get/set | Number of patient profiles |
| PhysicianCount | int | get/set | Number of physician profiles |
| AdministratorCount | int | get/set | Number of administrator profiles |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `ToString()` | string | Returns formatted statistics string |

### SchedulerService.cs
A high-level singleton facade for managing all scheduling operations across the system.

**Pattern:** Singleton

**Properties:**

| Name     | Type            | Access | Description                                        |
| -------- | --------------- | ------ | -------------------------------------------------- |
| Instance | ScheduleManager | get    | Gets the singleton instance of the ScheduleManager |

**Methods:**

| Signature                                                                                                                                        | Returns                                    | Description                                                                     |
| ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------ | ------------------------------------------------------------------------------- |
| `GetPhysicianSchedule(Guid physicianId)`                                                                                                         | PhysicianSchedule                          | Gets or creates a physician's schedule                                          |
| `ScheduleAppointment(AppointmentTimeInterval appointment)`                                                                                       | ScheduleResult                             | Schedules an appointment after checking for conflicts                           |
| `CancelAppointment(Guid physicianId, Guid appointmentId, string reason = "")`                                                                    | bool                                       | Cancels an appointment                                                          |
| `DeleteAppointment(Guid physicianId, Guid appointmentId)`                                                                                        | bool                                       | Deletes an appointment from the schedule                                        |
| `UpdateAppointment(Guid appointmentId, string? reason = null, string? notes = null, int? durationMinutes = null, DateTime? newStartTime = null)` | ScheduleResult                             | Updates an appointment's details with conflict checking using whitelist pattern |
| `FindNextAvailableSlot(Guid physicianId, TimeSpan duration, DateTime? afterTime = null, IBookingStrategy? strategy = null)`                      | AppointmentSlot?                           | Finds the next available appointment slot using the specified strategy          |
| `GetDailySchedule(Guid physicianId, DateTime date)`                                                                                              | IEnumerable&lt;AppointmentTimeInterval&gt; | Gets all appointments for a physician on a specific date                        |
| `GetScheduleInRange(Guid physicianId, DateTime startDate, DateTime endDate)`                                                                     | IEnumerable&lt;AppointmentTimeInterval&gt; | Gets all appointments for a physician in a date range                           |
| `GetPatientAppointments(Guid patientId)`                                                                                                         | IEnumerable&lt;AppointmentTimeInterval&gt; | Gets all appointments for a patient                                             |
| `GetAllAppointments()`                                                                                                                           | IEnumerable&lt;AppointmentTimeInterval&gt; | Gets all appointments across all physician schedules                            |
| `FindAppointmentById(Guid appointmentId)`                                                                                                        | AppointmentTimeInterval?                   | Finds a specific appointment by ID across all physician schedules               |
| `CheckForConflicts(AppointmentTimeInterval proposedAppointment, Guid? excludeAppointmentId = null, bool includeSuggestions = false)`             | ConflictCheckResult                        | Checks for scheduling conflicts without making any changes                      |
| `AddFacilityUnavailableBlock(UnavailableTimeInterval block)`                                                                                     | void                                       | Adds a facility-wide unavailable block (holidays, emergencies, etc.)            |
| `AddPhysicianUnavailableBlock(Guid physicianId, UnavailableTimeInterval block)`                                                                  | void                                       | Adds a physician-specific unavailable block                                     |
| `SetPhysicianAvailability(Guid physicianId, Dictionary<DayOfWeek, (TimeSpan Start, TimeSpan End)> availability)`                                 | void                                       | Sets standard weekly availability for a physician                               |
| `GetPhysicianStatistics(Guid physicianId, DateTime startDate, DateTime endDate)`                                                                 | ScheduleStatistics                         | Gets utilization statistics for a physician                                     |
| `CleanupOldAppointments(DateTime beforeDate)`                                                                                                    | int                                        | Performs cleanup of old appointments                                            |

**Helper Classes:**

**ScheduleResult:**

| Property               | Type                           | Description                       |
| ---------------------- | ------------------------------ | --------------------------------- |
| Success                | bool                           | Whether the operation succeeded   |
| AppointmentId          | Guid                           | ID of the appointment             |
| Message                | string                         | Result message                    |
| Conflicts              | List&lt;ScheduleConflict&gt;   | List of conflicts if any          |
| AlternativeSuggestions | List&lt;TimeSlotSuggestion&gt; | Alternative time slot suggestions |

**AppointmentSlot:**

| Property    | Type     | Description                                                  |
| ----------- | -------- | ------------------------------------------------------------ |
| Start       | DateTime | Start time of the slot                                       |
| End         | DateTime | End time of the slot                                         |
| PhysicianId | Guid     | Physician ID                                                 |
| IsOptimal   | bool     | Whether this is an optimal time (morning hours 9 AM - 12 PM) |

**ScheduleStatistics:**

| Property                   | Type     | Description                                        |
| -------------------------- | -------- | -------------------------------------------------- |
| PhysicianId                | Guid     | Physician ID                                       |
| StartDate                  | DateTime | Start date of statistics period                    |
| EndDate                    | DateTime | End date of statistics period                      |
| TotalAppointments          | int      | Total number of appointments                       |
| CompletedAppointments      | int      | Number of completed appointments                   |
| CancelledAppointments      | int      | Number of cancelled appointments                   |
| NoShowAppointments         | int      | Number of no-show appointments                     |
| TotalScheduledHours        | double   | Total scheduled hours                              |
| AverageAppointmentDuration | TimeSpan | Average appointment duration                       |
| CompletionRate             | double   | Completion rate percentage (calculated property)   |
| CancellationRate           | double   | Cancellation rate percentage (calculated property) |
| NoShowRate                 | double   | No-show rate percentage (calculated property)      |

## GUI.CliniCore
### App.xaml
Defines the MAUI application's global resource dictionaries, merging color schemes and style definitions from Resources/Styles. This XAML file provides the foundation for consistent visual styling across all pages and components in the CliniCore GUI application, establishing a centralized theme management approach.

### App.xaml.cs
Implements the MAUI Application lifecycle with dependency injection support, accepting an AppShell instance through constructor injection. On initialization, it sets AppShell as the MainPage and immediately navigates to LoginPage using the Shell navigation system, ensuring users are authenticated before accessing core application features.

### AppShell.xaml
Defines the root Shell navigation container with FlyoutBehavior disabled, initially displaying only the LoginPage as the default ShellContent. The Shell architecture supports MAUI's URI-based navigation pattern and provides the foundation for the application's navigation hierarchy, with routes registered in the code-behind for all major application pages.

### AppShell.xaml.cs
Registers all application navigation routes with the Shell routing system, mapping page types to route names for dependency injection-supported navigation. Routes include patient management, physician management, user management, clinical documents, appointments, and stub pages. Implements OnNavigating event override for debugging navigation flow, ensuring proper route resolution throughout the application lifecycle.
## Commands
### CommandParameterConverter.cs
### MauiCommandAdapter.cs
### RelayCommand.cs
## Converters
### PhysicianAssignmentConverter.cs
Implements an IValueConverter that transforms a physician's GUID to a human-readable assignment status string for display in XAML. This converter accesses the Profileservice singleton to resolve physician IDs to names, displaying "Dr. [Name]" for valid assignments, "Assigned" for unresolved GUIDs, or "Unassigned" for null values, providing clear visual feedback in patient management interfaces.

### MainPage.xaml
Default MAUI application template page displaying a sample UI with .NET Bot image and counter button. This page is not actively used in the CliniCore application flow, as the app immediately navigates to LoginPage on startup, but remains as the original template artifact demonstrating basic MAUI page structure and event handling.

### MainPage.xaml.cs
Code-behind for the default MAUI template page implementing a simple click counter with semantic screen reader support. This file demonstrates basic MAUI event handling and state management patterns but is not part of the active CliniCore user interface flow.

### MauiProgram.cs
Configures the MAUI application's dependency injection container and registers all services, ViewModels, Pages, and fonts. This class bootstraps Core.CliniCore services, registers navigation and session management services, configures Material Icons font, and initializes development data in debug mode. It serves as the central composition root ensuring proper service lifetime management and dependency resolution throughout the application.
## Properties
## Services
### HomeViewModelFactory.cs
### IHomeViewModelFactory.cs
### INavigationService.cs
### NavigationService.cs
### SessionManager.cs
## ViewModels
### BaseViewModel.cs
Provides the foundational base class for all ViewModels in the GUI application, implementing INotifyPropertyChanged for data binding support and offering common functionality across all view models.

**Inheritance:** INotifyPropertyChanged

**Properties:**

| Name                  | Type                           | Access  | Description                                                                                        |
| --------------------- | ------------------------------ | ------- | -------------------------------------------------------------------------------------------------- |
| ValidationErrors      | ObservableCollection\<string\> | get/set | Collection of validation error messages bound to UI with automatic property change notifications   |
| ValidationWarnings    | ObservableCollection\<string\> | get/set | Collection of validation warning messages bound to UI with automatic property change notifications |
| HasValidationErrors   | bool                           | get     | Convenience property returning true if ValidationErrors contains any items                         |
| HasValidationWarnings | bool                           | get     | Convenience property returning true if ValidationWarnings contains any items                       |
| IsBusy                | bool                           | get/set | Indicates whether the ViewModel is executing an async operation, used for loading indicators       |
| Title                 | string                         | get/set | Page or view title displayed in the UI                                                             |

**Events:**

| Name            | Type                        | Description                                                          |
| --------------- | --------------------------- | -------------------------------------------------------------------- |
| PropertyChanged | PropertyChangedEventHandler | Standard INotifyPropertyChanged event for data binding notifications |

**Methods:**

| Signature                                              | Returns   | Description                                                                       |
| ------------------------------------------------------ | --------- | --------------------------------------------------------------------------------- |
| HasPermission(SessionManager, Permission)              | bool      | Static helper to check if current user has a specific permission                  |
| HasAnyPermission(SessionManager, params Permission[])  | bool      | Static helper to check if current user has any of the specified permissions       |
| HasAllPermissions(SessionManager, params Permission[]) | bool      | Static helper to check if current user has all of the specified permissions       |
| GetCurrentRole(SessionManager)                         | UserRole? | Static helper to retrieve the current user's role from SessionManager             |
| OnPropertyChanged([CallerMemberName] string?)          | void      | Protected method to raise PropertyChanged event for specified property            |
| SetProperty\<T\>(ref T, T, [CallerMemberName] string?) | bool      | Protected method to set property value and raise PropertyChanged if value changed |
| ClearValidation()                                      | void      | Clears all validation errors and warnings collections                             |

### LoginViewModel.cs
Manages the login page functionality using the MauiCommandAdapter pattern to integrate Core.CliniCore authentication commands with the MAUI UI layer.

**Inheritance:** BaseViewModel, INotifyPropertyChanged

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Username | string | get/set | User's login username, triggers CanExecute validation on LoginCommand when changed |
| Password | string | get/set | User's login password, triggers CanExecute validation on LoginCommand when changed |

**Commands:**

| Name          | Type     | Description                                                                       |
| ------------- | -------- | --------------------------------------------------------------------------------- |
| LoginCommand  | ICommand | Executes login authentication using MauiCommandAdapter wrapping Core LoginCommand |
| CancelCommand | ICommand | Clears the login form (username, password, validation errors)                     |

**Methods:**

| Signature                        | Returns           | Description                                                                                    |
| -------------------------------- | ----------------- | ---------------------------------------------------------------------------------------------- |
| BuildLoginParameters()           | CommandParameters | Private - Builds CommandParameters from Username and Password for LoginCommand execution       |
| HandleLoginResult(CommandResult) | void              | Private - Handles login result, stores session in SessionManager, navigates to home on success |
| ClearLoginForm()                 | void              | Private - Clears username, password, and all validation messages                               |

### AppointmentListViewModel.cs
Provides comprehensive appointment list viewing with filtering capabilities by patient, physician, and date selection.

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name                | Type                                                    | Access  | Description                                         |
| ------------------- | ------------------------------------------------------- | ------- | --------------------------------------------------- |
| PatientIdString     | string                                                  | set     | Query parameter for patient ID filter               |
| PhysicianIdString   | string                                                  | set     | Query parameter for physician ID filter             |
| Appointments        | ObservableCollection&lt;AppointmentListDisplayModel&gt; | get/set | Collection of appointments for display              |
| SelectedAppointment | AppointmentListDisplayModel?                            | get/set | Currently selected appointment, triggers navigation |
| IsRefreshing        | bool                                                    | get/set | Pull-to-refresh state indicator                     |
| SelectedDate        | DateTime                                                | get/set | Date filter for appointments, triggers reload       |

**Commands:**

| Name                     | Type     | Description                                                   |
| ------------------------ | -------- | ------------------------------------------------------------- |
| LoadAppointmentsCommand  | ICommand | Loads appointments using ListAppointmentsCommand with filters |
| ViewAppointmentCommand   | ICommand | Navigates to AppointmentDetailPage with appointment ID        |
| CreateAppointmentCommand | ICommand | Navigates to CreateAppointmentPage with optional filters      |
| RefreshCommand           | ICommand | Pull-to-refresh reload                                        |
| BackCommand              | ICommand | Returns to home page                                          |

### AppointmentDetailViewModel.cs
Displays detailed appointment information with actions for rescheduling, cancellation, and deletion based on appointment status and permissions. This ViewModel loads appointment data directly from SchedulingService rather than using commands for read operations, dynamically enables or disables action buttons based on appointment status (e.g., cannot reschedule completed appointments), displays confirmation dialogs before destructive operations using MAUI's DisplayAlert, and automatically reloads appointment data after status changes to reflect updated information.

### AppointmentFormViewModelBase.cs
Serves as an abstract base class for appointment creation and editing forms, providing shared properties, picker data, and navigation commands while derived classes implement specific save command logic. This base class populates patient and physician pickers from Profileservice, manages form properties including selected date/time/duration with proper validation, implements lazy initialization of the SaveCommand through the CreateSaveCommand factory method to ensure derived classes are fully constructed, and provides the HandleSaveResult method that navigates back to the list page on successful saves.

### AppointmentEditViewModel.cs
Handles editing of existing appointments by extending AppointmentFormViewModelBase and using UpdateAppointmentCommand for persistence. This ViewModel accepts an appointmentId query parameter to load existing appointment data, populates the form fields with current appointment values from SchedulingService, overrides CreateSaveCommand to use UpdateAppointmentCommand with appropriate parameters, and validates that the appointment exists before allowing edit operations.

### CreateAppointmentViewModel.cs
Manages new appointment creation by extending AppointmentFormViewModelBase and using ScheduleAppointmentCommand for persistence. This ViewModel accepts optional patientId and physicianId query parameters to pre-select participants, overrides CreateSaveCommand to use ScheduleAppointmentCommand with required patient, physician, start time, duration, and reason parameters, includes optional notes in the command parameters if provided, and ensures all required fields are populated before enabling the save button.

### PatientListViewModel.cs
Provides patient list management with advanced filtering by assignment status, physician assignment, and search text, plus role-based patient assignment capabilities.

**Inheritance:** BaseViewModel, INotifyPropertyChanged

**Properties:**

| Name                    | Type                                         | Access  | Description                                             |
| ----------------------- | -------------------------------------------- | ------- | ------------------------------------------------------- |
| Patients                | ObservableCollection&lt;PatientProfile&gt;   | get/set | Collection of patients displayed in the UI list         |
| AvailablePhysicians     | ObservableCollection&lt;PhysicianProfile&gt; | get/set | Collection of physicians for assignment filtering       |
| SearchText              | string                                       | get/set | Search filter text, triggers automatic search on change |
| SelectedPatient         | PatientProfile?                              | get/set | Currently selected patient, triggers navigation         |
| ShowMyPatientsOnly      | bool                                         | get/set | Physician toggle: their assigned patients vs all        |
| SelectedPhysicianFilter | PhysicianProfile?                            | get/set | Admin-only physician filter for patient list            |
| AssignmentFilterIndex   | int                                          | get/set | 0=All, 1=Assigned Only, 2=Unassigned Only               |
| IsRefreshing            | bool                                         | get/set | Pull-to-refresh state indicator                         |
| CanCreatePatient        | bool                                         | get     | RBAC: user can create patient profiles                  |
| CanAssignPatients       | bool                                         | get     | RBAC: user can assign patients to physicians            |
| IsPhysician             | bool                                         | get     | True if current user is Physician role                  |
| IsAdministrator         | bool                                         | get     | True if current user is Administrator role              |

**Commands:**

| Name                 | Type     | Description                                           |
| -------------------- | -------- | ----------------------------------------------------- |
| LoadPatientsCommand  | ICommand | Loads patients using ListPatientsCommand with filters |
| SearchCommand        | ICommand | Triggers patient list reload with search filter       |
| ViewPatientCommand   | ICommand | Navigates to PatientDetailPage with patient ID        |
| CreatePatientCommand | ICommand | Navigates to PatientEditPage (create mode)            |
| AssignPatientCommand | ICommand | Assigns patient to physician                          |
| RefreshCommand       | ICommand | Pull-to-refresh reload                                |
| BackCommand          | ICommand | Returns to home page                                  |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `AssignPatient(Guid patientId)` | void | Assigns patient to physician (self or selected) |
| `GetAssignmentStatus(PatientProfile patient)` | string | Returns formatted assignment status |
| `CanAssignPatient(PatientProfile patient)` | bool | Determines if assign button visible for patient |

### PatientDetailViewModel.cs
Displays comprehensive patient profile information with edit, delete, and physician assignment actions.

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name                | Type                                         | Access          | Description                                 |
| ------------------- | -------------------------------------------- | --------------- | ------------------------------------------- |
| PatientId           | Guid                                         | get/private set | Patient identifier, triggers load on change |
| PatientIdString     | string                                       | set             | Query parameter for PatientId navigation    |
| Patient             | PatientProfile?                              | get/set         | Loaded patient profile domain object        |
| PatientName         | string                                       | get/set         | Patient's full name for display             |
| Username            | string                                       | get/set         | Patient's login username                    |
| Address             | string                                       | get/set         | Patient's address                           |
| BirthDate           | string                                       | get/set         | Formatted birth date (yyyy-MM-dd)           |
| Gender              | string                                       | get/set         | Patient's gender as string                  |
| Race                | string                                       | get/set         | Patient's race                              |
| PrimaryPhysician    | string                                       | get/set         | Primary physician name or "None assigned"   |
| AvailablePhysicians | ObservableCollection&lt;PhysicianProfile&gt; | get/set         | Physicians available for assignment         |
| SelectedPhysician   | PhysicianProfile?                            | get/set         | Selected physician for assignment           |
| AppointmentCount    | int                                          | get/set         | Count of patient's appointments             |
| DocumentCount       | int                                          | get/set         | Count of patient's clinical documents       |

**Commands:**

| Name                         | Type     | Description                                               |
| ---------------------------- | -------- | --------------------------------------------------------- |
| LoadPatientCommand           | ICommand | Loads patient using ViewPatientProfileCommand             |
| AssignPhysicianCommand       | ICommand | Assigns selected physician                                |
| EditPatientCommand           | ICommand | Navigates to PatientEditPage                              |
| DeletePatientCommand         | ICommand | Deletes patient after confirmation                        |
| ViewClinicalDocumentsCommand | ICommand | Navigates to ClinicalDocumentListPage filtered by patient |
| BackCommand                  | ICommand | Navigates back to PatientListPage                         |

### PatientEditViewModel.cs
Handles both patient creation and profile editing in a unified interface with mode-specific validation.

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| PatientId | Guid? | get/private set | Patient ID (null in create mode) |
| PatientIdString | string | set | Query parameter for patient ID navigation |
| IsEditMode | bool | get/set | True when editing existing patient |
| Username | string | get/set | Login username (create mode only) |
| Password | string | get/set | Login password (create mode only) |
| Name | string | get/set | Patient's full name |
| Address | string | get/set | Patient's address |
| BirthDate | DateTime | get/set | Patient's birth date |
| SelectedGender | Gender | get/set | Selected gender enum value |
| Race | string | get/set | Patient's race |
| GenderOptions | List&lt;Gender&gt; | get | All available Gender enum values |

**Commands:**

| Name | Type | Description |
|------|------|-------------|
| LoadPatientCommand | ICommand | Loads patient for edit |
| SaveCommand | ICommand | Creates or updates patient based on mode |
| CancelCommand | ICommand | Cancels and navigates back |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `CanSave()` | bool | Validates required fields based on mode |
| `ExecuteCreate()` | void | Creates patient using CreatePatientCommand |
| `ExecuteUpdate()` | void | Updates patient using UpdatePatientProfileCommand |

### PatientHomeViewModel.cs
Provides the patient portal home page with navigation to view their own appointments, clinical documents, and care team physicians. This ViewModel retrieves the patient's profile from ProfileService to display their full name in the welcome message, implements navigation commands with patient-specific filtering (e.g., appointments filtered by patient ID), allows patients to view the full physician directory rather than just their assigned physician, and provides logout functionality with session clearing.

### PhysicianListViewModel.cs
Manages physician list display with search filtering and role-based access control.

**Inheritance:** BaseViewModel, INotifyPropertyChanged

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| Physicians | ObservableCollection&lt;PhysicianProfile&gt; | get/set | Collection of physicians displayed |
| SearchText | string | get/set | Search filter text, triggers automatic search |
| SelectedPhysician | PhysicianProfile? | get/set | Currently selected physician |
| IsRefreshing | bool | get/set | Pull-to-refresh state indicator |
| CanCreatePhysician | bool | get | RBAC: only administrators can create physicians |

**Commands:**

| Name | Type | Description |
|------|------|-------------|
| LoadPhysiciansCommand | ICommand | Loads physicians with search filter |
| SearchCommand | ICommand | Triggers physician list reload |
| ViewPhysicianCommand | ICommand | Navigates to PhysicianDetailPage |
| CreatePhysicianCommand | ICommand | Navigates to PhysicianEditPage (create mode) |
| RefreshCommand | ICommand | Pull-to-refresh reload |
| BackCommand | ICommand | Returns to home page |

### PhysicianDetailViewModel.cs
Displays physician profile details including specializations, license information, and statistics.

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| PhysicianId | Guid | get/private set | Physician identifier |
| PhysicianIdString | string | set | Query parameter for navigation |
| Physician | PhysicianProfile? | get/set | Loaded physician profile |
| PhysicianName | string | get/set | Physician's full name |
| Username | string | get/set | Physician's login username |
| LicenseNumber | string | get/set | Medical license number |
| GraduationDate | string | get/set | Formatted graduation date |
| Specializations | string | get/set | Comma-separated specializations |
| PatientCount | int | get/set | Count of assigned patients |
| AppointmentCount | int | get/set | Count of appointments |
| CanEditPhysician | bool | get | RBAC: user can update profile |
| CanDeletePhysician | bool | get | RBAC: user can delete profile |
| CanViewPatients | bool | get | RBAC: user can view patients |

**Commands:**

| Name | Type | Description |
|------|------|-------------|
| LoadPhysicianCommand | ICommand | Loads physician using ViewPhysicianProfileCommand |
| EditPhysicianCommand | ICommand | Navigates to PhysicianEditPage |
| DeletePhysicianCommand | ICommand | Deletes physician after confirmation |
| ViewPatientsCommand | ICommand | Navigates to PatientListPage |
| BackCommand | ICommand | Navigates back |

### PhysicianEditViewModel.cs
Provides physician creation and editing with multi-select specialization management (1-5 required).

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| PhysicianId | Guid? | get/private set | Physician ID (null in create mode) |
| PhysicianIdString | string | set | Query parameter for navigation |
| IsEditMode | bool | get/set | True when editing existing physician |
| Username | string | get/set | Login username (create mode only) |
| Password | string | get/set | Login password (create mode only) |
| Name | string | get/set | Physician's full name |
| Address | string | get/set | Physician's address |
| BirthDate | DateTime | get/set | Physician's birth date |
| LicenseNumber | string | get/set | Medical license number |
| GraduationDate | DateTime | get/set | Medical school graduation date |
| SpecializationItems | ObservableCollection&lt;SpecializationItem&gt; | get | All specializations with IsSelected binding |
| SelectedSpecializationCount | int | get | Count of selected specializations |
| SpecializationCountMessage | string | get | Validation message for count |
| SpecializationCountColor | string | get | Color based on validation state |

**Commands:**

| Name | Type | Description |
|------|------|-------------|
| LoadPhysicianCommand | ICommand | Loads physician for edit |
| SaveCommand | ICommand | Creates or updates physician |
| CancelCommand | ICommand | Cancels and navigates back |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `CanSave()` | bool | Validates required fields and specialization count (1-5) |
| `ExecuteCreate()` | void | Creates physician using CreatePhysicianCommand |
| `ExecuteUpdate()` | void | Updates physician using UpdatePhysicianProfileCommand |

### PhysicianHomeViewModel.cs
Serves as the physician dashboard providing navigation to patient management, scheduling, clinical documentation, and availability management features. This ViewModel retrieves the physician profile to display "Dr. [Name]" in the welcome message, implements navigation commands with physician-specific filtering for appointments and clinical documents, navigates to the patient list where physicians can view their assigned patients with the ShowMyPatientsOnly toggle, and includes a stub navigation for the availability management feature (not yet implemented).

### ClinicalDocumentListViewModel.cs
Manages clinical document list display with filtering by patient, physician, and completion status.

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| PatientIdString | string | set | Query parameter for patient ID filter |
| PhysicianIdString | string | set | Query parameter for physician ID filter |
| Documents | ObservableCollection&lt;ClinicalDocumentDisplayModel&gt; | get/set | Collection of documents for display |
| SelectedDocument | ClinicalDocumentDisplayModel? | get/set | Currently selected document |
| ShowIncompleteOnly | bool | get/set | Toggle to filter for draft documents only |
| IsRefreshing | bool | get/set | Pull-to-refresh state indicator |
| CanCreateDocument | bool | get | RBAC: physicians and admins can create |

**Commands:**

| Name | Type | Description |
|------|------|-------------|
| LoadDocumentsCommand | ICommand | Loads documents with filters |
| ViewDocumentCommand | ICommand | Navigates to ClinicalDocumentDetailPage |
| CreateDocumentCommand | ICommand | Navigates to ClinicalDocumentEditPage (create mode) |
| RefreshCommand | ICommand | Pull-to-refresh reload |
| BackCommand | ICommand | Returns to home page |

### ClinicalDocumentDetailViewModel.cs
Displays SOAP-formatted clinical documents with categorized entry collections and document management actions (edit, complete, delete). This ViewModel populates separate ObservableCollections for subjective observations, objective observations, assessments, diagnoses, plans, and prescriptions, categorizes observations into subjective vs objective based on ObservationType, implements CanEdit, CanComplete, and CanDelete properties based on document completion status, and calls the Complete() method on the document domain object for finalization with validation error display.

### ClinicalDocumentEditViewModel.cs
Provides comprehensive clinical document creation and editing with inline SOAP entry management.

**Inheritance:** BaseViewModel, INotifyPropertyChanged, QueryProperty

**Properties:**

| Name | Type | Access | Description |
|------|------|--------|-------------|
| DocumentIdString | string | set | Query parameter for document ID (edit mode) |
| PatientIdString | string | set | Query parameter for patient ID (create mode) |
| IsCreateMode | bool | get | True when creating new document |
| IsEditMode | bool | get | True when editing existing document |
| ChiefComplaint | string | get/set | Chief complaint text |
| Observations | ObservableCollection&lt;ObservationDisplayModel&gt; | get | Observation entries |
| Assessments | ObservableCollection&lt;AssessmentDisplayModel&gt; | get | Assessment entries |
| Diagnoses | ObservableCollection&lt;DiagnosisDisplayModel&gt; | get | Diagnosis entries |
| Plans | ObservableCollection&lt;PlanDisplayModel&gt; | get | Plan entries |
| Prescriptions | ObservableCollection&lt;PrescriptionDisplayModel&gt; | get | Prescription entries |
| NewObservation | string | get/set | Text for new subjective observation |
| NewDiagnosisDescription | string | get/set | Description for new diagnosis |
| NewDiagnosisICD10Code | string | get/set | ICD-10 code for new diagnosis |
| NewPrescriptionMedication | string | get/set | Medication name for prescription |
| NewPrescriptionDiagnosisId | Guid? | get/set | Linked diagnosis ID for prescription |
| AvailablePatients | ObservableCollection&lt;PatientProfile&gt; | get/set | Patients for picker (create mode) |
| SelectedPatient | PatientProfile? | get/set | Selected patient (create mode) |

**Commands:**

| Name | Type | Description |
|------|------|-------------|
| LoadDocumentCommand | ICommand | Loads document for edit |
| SaveCommand | ICommand | Creates or updates document |
| FinalizeCommand | ICommand | Marks document as completed |
| AddObservationCommand | ICommand | Adds subjective observation |
| AddObjectiveObservationCommand | ICommand | Adds objective observation |
| AddAssessmentCommand | ICommand | Adds assessment |
| AddDiagnosisCommand | ICommand | Adds diagnosis |
| AddPlanCommand | ICommand | Adds plan |
| AddPrescriptionCommand | ICommand | Adds prescription linked to diagnosis |
| BackCommand | ICommand | Navigates back |

**Methods:**

| Signature | Returns | Description |
|-----------|---------|-------------|
| `CreateDocumentAsync()` | Task | Creates document using CreateClinicalDocumentCommand |
| `UpdateDocumentAsync()` | Task | Updates document using UpdateClinicalDocumentCommand |
| `ExecuteFinalizeAsync()` | Task | Validates and marks document as completed |
| `CanSave()` | bool | Validates required fields based on mode |
| `CanFinalize()` | bool | Checks document is not completed and in edit mode |

### AdministratorEditViewModel.cs
Handles administrator profile creation and editing with mode-specific field visibility and validation requirements. This ViewModel shows password field only in create mode via IsPasswordVisible computed property, uses AdministratorEntryType enum extensions to retrieve profile entry keys for address, birthdate, and email fields, validates that username, password, and name are required in create mode while only name is required in edit mode, and navigates to the UserListPage after successful save operations regardless of mode.

### AdministratorHomeViewModel.cs
Serves as the administrator dashboard providing navigation to all system management features including user management, patient/physician management, scheduling, clinical documents, reports, and system administration. This ViewModel initializes all navigation commands to appropriate pages based on administrator RBAC permissions, displays a personalized welcome message using SessionManager.CurrentUsername, navigates to stub pages for unimplemented features (reports, system admin) with placeholder content, and implements logout functionality that clears the session and navigates back to the login page.

### UserListViewModel.cs
Provides unified user management displaying all system users (administrators, physicians, patients) with role-based badges, search filtering, and role-specific detail navigation. This ViewModel uses UserDisplayModel wrapper to provide role-based formatting (e.g., "Dr." prefix for physicians), display role badges with color coding (red for admins, blue for physicians, green for patients), and format additional info based on user type (license for physicians, DOB for patients, department for admins). It implements client-side filtering on both name and username fields and navigates to appropriate detail pages based on user role (PhysicianDetailPage, PatientDetailPage, or AdministratorEditPage).

### StubViewModel.cs
Displays user-friendly placeholder pages for features that are intentionally unimplemented (reports and system administration). This ViewModel accepts a "type" query parameter to configure the stub content dynamically, provides feature-specific icons, descriptions, and reasons for each stub type using MaterialIcons, explains that reports and multi-facility management are intentionally omitted to keep the assignment scope focused, and includes a back button that navigates to the home page.

### InvertedBoolConverter.cs
A bidirectional IValueConverter that inverts boolean values for XAML binding, commonly used for conditional visibility where an element should be shown when a condition is NOT true. This converter returns the opposite boolean value in both Convert and ConvertBack methods and handles null values by returning false as a safe default.

### IsNotNullConverter.cs
A unidirectional IValueConverter that returns true if a value is not null, false otherwise, typically used for enabling buttons or showing UI elements based on object existence. This converter provides a simple null check in the Convert method and throws NotImplementedException for ConvertBack since reverse conversion is not meaningful for null checking.

### IsStringNotNullOrEmptyConverter.cs
A unidirectional IValueConverter that returns true if a string value is neither null nor empty, commonly used for validation-based visibility or enabling save buttons. This converter uses string.IsNullOrEmpty() in the Convert method to check string validity and throws NotImplementedException for ConvertBack as reverse string validation is not supported.

### SpecializationsConverter.cs
A unidirectional IValueConverter that formats a collection of MedicalSpecialization enums as a comma-separated string for display in physician profiles and lists. This converter checks if the value is IEnumerable\<MedicalSpecialization\>, joins the specializations with commas if any exist, returns "None" for empty collections, and throws NotImplementedException for ConvertBack since string-to-enum conversion is handled elsewhere.
## Views
### LoginPage.xaml
Authentication page featuring a centered card layout with username/password entry fields, Material Design icons, and development credential display in debug mode. Binds to LoginViewModel using two-way bindings for credentials, displays validation errors/warnings in color-coded borders, and provides "Sign In" and "Clear Form" buttons with custom styling using rounded rectangles and shadows for a polished, professional appearance.

### HomePage.xaml
Role-based dashboard page using RoleBasedContentTemplateSelector to dynamically display different interfaces for Administrator, Physician, and Patient users. Each template presents role-specific navigation cards with Material Icons, colored borders matching user roles, and command bindings to navigate to appropriate management pages. The page uses BackButtonBehavior to trigger logout on back navigation and demonstrates advanced XAML templating with DataTemplate selection based on ViewModel type.

### PatientListPage.xaml
Comprehensive patient management interface with search functionality, role-based filtering (physician assignment and "my patients" toggle), and CollectionView displaying patient cards with demographics, physician assignments using PhysicianAssignmentConverter, and inline "Assign" buttons. Supports pull-to-refresh, implements empty state UI with Material Icons, constrains maximum width for wide screens, and includes RBAC-controlled "Create New Patient" action button.

### PatientDetailPage.xaml
Patient profile detail view organized into sections (Demographics, Care Team, Clinical Summary) with Border-wrapped content cards. Displays patient information in grid layouts, provides physician assignment picker with IsNotNullConverter-enabled submit button, shows appointment and document counts with Material Icons, and offers action buttons for viewing clinical documents, editing, and deleting patients based on role permissions.

### PatientEditPage.xaml
Patient create/edit form with conditional field visibility using InvertedBoolConverter to show username/password only in create mode. Features DatePicker for birth date, Picker for gender selection, Entry fields for name/address/race, and displays required field indicators. Binds to PatientEditViewModel with two-way data binding and presents validation errors in styled borders at the bottom of the scrollable form.

### PhysicianListPage.xaml
Physician roster management page with search bar, CollectionView displaying physician cards with names (prefixed "Dr."), usernames, license numbers, specializations using SpecializationsConverter, and patient counts. Implements pull-to-refresh, RBAC-controlled "Create New Physician" button visible only to administrators, empty state handling, and responsive layout with maximum width constraints for optimal viewing on various screen sizes.

### PhysicianDetailPage.xaml
Physician profile page showing professional details (license number, graduation date, specializations) and practice summary (patient count, appointment count) with Material Icons. Includes Grid-based layout for aligned label-value pairs, conditional "View Patients" button visibility based on permissions, and action buttons for editing/deleting physicians with RBAC support ensuring only authorized users can modify physician records.

### PhysicianEditPage.xaml
Physician create/edit form featuring username/password fields in create mode, professional information inputs (name, address, license, graduation date), and multi-select specializations using CheckBox-enabled CollectionView with SpecializationItem binding. Provides real-time feedback on specialization selection count (1-5 required) with color-coded messages and warning icons, validates required fields, and displays validation errors in color-coded borders.

### AppointmentListPage.xaml
Appointment scheduling dashboard with DatePicker for filtering by date, CollectionView displaying appointment cards showing time ranges, status badges with color-coding, patient/physician names, reason for visit, and duration. Implements pull-to-refresh functionality, "Schedule New Appointment" action button, empty state handling for dates without appointments, and responsive design with maximum width constraints for optimal user experience.

### AppointmentDetailPage.xaml
Detailed appointment view presenting appointment information in a grid layout with time, duration, patient, physician, status badge using StatusColor binding, and reason fields. Includes expandable notes section, role-based action buttons (reschedule, cancel, delete) with conditional visibility, and validation message display using green-themed borders for success notifications distinguishing them from error messages.

### CreateAppointmentPage.xaml
New appointment creation form with Picker controls for patient and physician selection using display bindings, DatePicker with minimum date constraint to prevent past appointments, TimePicker for appointment time, duration selection in 15-minute increments, reason Editor field, and optional notes. Includes inline validation hints about business hours (Monday-Friday, 8 AM-5 PM) and presents validation errors in bordered containers.

### AppointmentEditPage.xaml
Appointment rescheduling form with pre-populated Picker fields for patient and physician, DatePicker and TimePicker for new appointment time, duration selection, editable reason and notes fields. Uses identical layout to CreateAppointmentPage for consistency, binds to AppointmentEditViewModel, and displays "Save Appointment" instead of "Schedule Appointment" with back button navigation support through BackButtonBehavior.

### ClinicalDocumentListPage.xaml
Clinical documentation browser with "Show incomplete documents only" checkbox filter, CollectionView displaying document cards with creation dates, completion status badges, patient/physician names, chief complaints, and document summaries. Features pull-to-refresh, RBAC-controlled "Create New Document" button for physicians and admins, empty state UI with Material Icons and helpful messages, and responsive layout supporting both portrait and landscape orientations.

### ClinicalDocumentDetailPage.xaml
Read-only SOAP note viewer displaying Chief Complaint section followed by Subjective Observations, Objective Observations, Assessments, Diagnoses, Treatment Plans, and Prescriptions in separate Border-wrapped sections. Each section uses CollectionView with custom DataTemplates for display models, employs IsStringNotNullOrEmptyConverter for conditional field visibility, and provides action buttons for editing, completing, and deleting documents based on user permissions.

### ClinicalDocumentEditPage.xaml
Comprehensive clinical document editor supporting both create and edit modes with conditional UI sections based on IsCreateMode/IsEditMode. In create mode, displays patient/physician/appointment selection pickers; in edit mode, shows document context information. Features inline forms for adding SOAP note components (observations, assessments, diagnoses, plans, prescriptions) with separate sections for subjective and objective observations, diagnosis-linked prescription creation, and dual save buttons ("Save as Draft" and "Finalize Document") in edit mode.

### UserListPage.xaml
System user management interface displaying all users across roles with search functionality, CollectionView showing user cards with names, usernames, role badges color-coded by role (Administrator=red, Physician=blue, Patient=green), and additional role-specific information. Includes "Create New Administrator" button, pull-to-refresh support, and uses IsStringNotNullOrEmptyConverter for conditional display of supplemental user information based on role type.

### AdministratorEditPage.xaml
Administrator account creation/edit form with username and password fields visible only in create mode (using IsPasswordVisible binding), required name field, optional address, birth date DatePicker, and email Entry with email keyboard type. Presents validation errors in red-bordered containers, uses consistent styling with other edit forms, and binds to AdministratorEditViewModel for managing administrative user accounts.

### StubPage.xaml
Placeholder page for unimplemented features displaying Material Icons, feature name, status information in an orange-themed card explaining why the feature is unavailable, and implementation notes about future development plans. Uses ViewModel binding for Icon, FeatureName, Description, and Reason properties, providing a consistent user experience for navigating to planned but not yet implemented functionality like system administration and advanced reporting features.

### RoleBasedContentTemplateSelector.cs
DataTemplateSelector implementation that dynamically chooses HomePage templates based on user role by examining ViewModel types. Returns AdministratorTemplate for AdministratorHomeViewModel, PhysicianTemplate for PhysicianHomeViewModel, and PatientTemplate for PatientHomeViewModel, with PatientTemplate as the default for security purposes. This class enables the single HomePage.xaml to present completely different UIs tailored to each user role's specific workflows and permissions.

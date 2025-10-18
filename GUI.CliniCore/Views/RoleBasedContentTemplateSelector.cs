using GUI.CliniCore.ViewModels;

namespace GUI.CliniCore.Views
{
    /// <summary>
    /// DataTemplateSelector that chooses the appropriate home page template based on user role
    /// Similar to ConsoleMenuBuilder's role-based menu construction in the CLI
    /// </summary>
    public class RoleBasedContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? AdministratorTemplate { get; set; }
        public DataTemplate? PhysicianTemplate { get; set; }
        public DataTemplate? PatientTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            // Determine which template to use based on the ViewModel type
            return item switch
            {
                AdministratorHomeViewModel => AdministratorTemplate,
                PhysicianHomeViewModel => PhysicianTemplate,
                PatientHomeViewModel => PatientTemplate,
                _ => PatientTemplate // Default to most restrictive
            };
        }
    }
}

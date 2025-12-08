namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class PlanPriorityExtensions
    {
        public static PlanPriority[] All => Enum.GetValues<PlanPriority>();
        public static int TypeCount => All.Length;

        public static PlanPriority? GetByIndex(int oneBasedIndex)
        {
            var all = All;
            if (oneBasedIndex < 1 || oneBasedIndex > all.Length)
                return null;
            return all[oneBasedIndex - 1];
        }

        public static PlanPriority? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(p => p
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static string GetDisplayName(this PlanPriority priority)
        {
            return priority.ToString();
        }

        public static bool RequiresImmediateAction(this PlanPriority priority)
        {
            return priority >= PlanPriority.Urgent;
        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return All.Select((p, index) => (index + 1, p.GetDisplayName())).ToList();
        }
    }
}

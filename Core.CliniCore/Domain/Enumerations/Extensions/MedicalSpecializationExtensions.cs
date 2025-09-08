using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Enumerations.Extensions
{
    public static class MedicalSpecializationExtensions
    {
        public static MedicalSpecialization[] All =>
            Enum.GetValues<MedicalSpecialization>();

        public static int TypeCount => All.Length;

        public static MedicalSpecialization? GetByIndex(int oneBasedIndex)
        {
            MedicalSpecialization[] all = All;
            if (oneBasedIndex < 0 || oneBasedIndex > All.Length)
                return null;

            return all[oneBasedIndex - 1];
        }

        public static MedicalSpecialization? FindByDisplayName(string display)
        {
            return All.FirstOrDefault(s => s
                .GetDisplayName()
                .Equals(display, StringComparison.OrdinalIgnoreCase)
            );
        }
        
        public static string GetDisplayName(this MedicalSpecialization spec)
        {
            return spec switch
            {
                MedicalSpecialization.Emergency => "Emergency Room",
                MedicalSpecialization.FamilyMedicine => "Family Medicine",
                MedicalSpecialization.InternalMedicine => "Internal Medicine",
                MedicalSpecialization.Pediatrics => "Pediatrics",
                MedicalSpecialization.ObstetricsGynecology => "Obstetrics & Gynecology",
                MedicalSpecialization.Surgery => "Surgery",
                MedicalSpecialization.Orthopedics => "Orthopedics",
                MedicalSpecialization.Cardiology => "Cardiology",
                MedicalSpecialization.Neurology => "Neurology",
                MedicalSpecialization.Oncology => "Oncology",
                MedicalSpecialization.Radiology => "Radiology",
                MedicalSpecialization.Anesthesiology => "Anesthesiology",
                MedicalSpecialization.Psychiatry => "Psychiatry",
                MedicalSpecialization.Dermatology => "Dermatology",
                MedicalSpecialization.Ophthalmology => "Opthalmology",
                _ => spec.ToString() // fall back on fully qualified assembly name
            };
        }

        public static string GetLaymanName(this MedicalSpecialization spec)
        {
            return spec switch
            {
                MedicalSpecialization.Emergency => "Emergency Room",
                MedicalSpecialization.FamilyMedicine => "Family Practice",
                MedicalSpecialization.InternalMedicine => "Internal Medicine",
                MedicalSpecialization.Pediatrics => "Children's Health",
                MedicalSpecialization.ObstetricsGynecology => "Women's Health",
                MedicalSpecialization.Surgery => "General Surgery",
                MedicalSpecialization.Orthopedics => "Orthopedics & Sports Medicine",
                MedicalSpecialization.Cardiology => "Heart & Vascular",
                MedicalSpecialization.Neurology => "Brain & Spine",
                MedicalSpecialization.Oncology => "Cancer Center",
                MedicalSpecialization.Radiology => "Imaging",
                MedicalSpecialization.Anesthesiology => "Anesthesia",
                MedicalSpecialization.Psychiatry => "Behavioral Health",
                MedicalSpecialization.Dermatology => "Dermatology",
                MedicalSpecialization.Ophthalmology => "Eye Care",
                _ => spec.ToString() // fall back on fully qualified assembly name
            };

        }

        public static List<(int Index, string Display)> GetNumberedList()
        {
            return Enum.GetValues<MedicalSpecialization>()
                .Select((sp, index) => (index + 1, sp.GetDisplayName()))
                .ToList();
        }
        public static List<string> GetAlphabetizedList()
        {
            return Enum.GetValues<MedicalSpecialization>()
                .Select((sp) => sp.GetDisplayName())
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

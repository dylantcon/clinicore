using Core.CliniCore.Domain.Enumerations;

namespace Core.CliniCore.Bootstrap
{
    /// <summary>
    /// Generates randomized but realistic-looking data for development/demo purposes.
    /// Uses consistent seed for reproducible results when needed.
    /// </summary>
    public class RandomDataGenerator
    {
        private readonly Random _random;
        private readonly HashSet<int> _usedRoomNumbers = new();
        private readonly HashSet<string> _usedUsernames = new();

        #region Name Pools

        private static readonly string[] MaleFirstNames =
        {
            "James", "Michael", "Robert", "David", "William",
            "Richard", "Joseph", "Thomas", "Christopher", "Daniel",
            "Matthew", "Anthony", "Andrew", "Joshua", "Ethan",
            "Alexander", "Benjamin", "Nicholas", "Samuel", "Ryan"
        };

        private static readonly string[] FemaleFirstNames =
        {
            "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth",
            "Barbara", "Susan", "Jessica", "Sarah", "Karen",
            "Emily", "Ashley", "Kimberly", "Michelle", "Amanda",
            "Olivia", "Sophia", "Isabella", "Emma", "Mia"
        };

        private static readonly string[] LastNames =
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones",
            "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
            "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin",
            "Lee", "Perez", "Thompson", "White", "Harris",
            "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
            "Walker", "Young", "Allen", "King", "Wright"
        };

        #endregion

        #region Address Components

        private static readonly string[] StreetNames =
        {
            "Main", "Oak", "Maple", "Cedar", "Pine",
            "Elm", "Washington", "Park", "Lake", "Hill",
            "River", "Forest", "Spring", "Meadow", "Valley"
        };

        private static readonly string[] StreetSuffixes =
        {
            "Street", "Avenue", "Boulevard", "Drive", "Lane",
            "Road", "Way", "Court", "Place", "Circle"
        };

        private static readonly string[] Cities =
        {
            "Springfield", "Riverside", "Fairview", "Madison", "Georgetown",
            "Clinton", "Franklin", "Greenville", "Bristol", "Oakland",
            "Salem", "Manchester", "Burlington", "Lexington", "Kingston"
        };

        private static readonly string[] States =
        {
            "CA", "TX", "FL", "NY", "PA", "IL", "OH", "GA", "NC", "MI",
            "NJ", "VA", "WA", "AZ", "MA", "TN", "IN", "MO", "MD", "WI"
        };

        #endregion

        #region Medical Data

        private static readonly string[] Races =
        {
            "White", "Black or African American", "Asian",
            "Hispanic or Latino", "Native American", "Pacific Islander", "Other"
        };

        private static readonly string[] VisitReasons =
        {
            "Annual checkup", "Follow-up visit", "Consultation",
            "Routine physical", "New patient visit", "Wellness exam",
            "Preventive care", "Health screening", "Lab review",
            "Medication review", "Chronic care management"
        };

        #endregion

        public RandomDataGenerator(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Reserves usernames that should not be generated (e.g., hardcoded ones).
        /// </summary>
        public void ReserveUsernames(IEnumerable<string> usernames)
        {
            foreach (var username in usernames)
            {
                _usedUsernames.Add(username.ToLowerInvariant());
            }
        }

        /// <summary>
        /// Reserves room numbers that should not be generated.
        /// </summary>
        public void ReserveRoomNumbers(IEnumerable<int> roomNumbers)
        {
            foreach (var room in roomNumbers)
            {
                _usedRoomNumbers.Add(room);
            }
        }

        #region Name Generation

        public Gender RandomGender()
        {
            return _random.Next(2) == 0 ? Gender.Man : Gender.Woman;
        }

        public string RandomFirstName(Gender gender)
        {
            var names = gender == Gender.Man ? MaleFirstNames : FemaleFirstNames;
            return names[_random.Next(names.Length)];
        }

        public string RandomLastName()
        {
            return LastNames[_random.Next(LastNames.Length)];
        }

        public (string FullName, string FirstName, string LastName, Gender Gender) GeneratePatientName()
        {
            var gender = RandomGender();
            var firstName = RandomFirstName(gender);
            var lastName = RandomLastName();
            return ($"{firstName} {lastName}", firstName, lastName, gender);
        }

        public (string FullName, string LastName) GeneratePhysicianName()
        {
            var lastName = RandomLastName();
            return (lastName, lastName); // Physicians are referred to by last name
        }

        #endregion

        #region Username Generation

        public string GenerateUsername(string firstName, string lastName)
        {
            // Try variations until we find a unique one
            var baseUsername = $"{firstName[0]}{lastName}".ToLowerInvariant();
            var username = baseUsername;
            var attempt = 1;

            while (_usedUsernames.Contains(username))
            {
                username = $"{baseUsername}{attempt}";
                attempt++;
            }

            _usedUsernames.Add(username);
            return username;
        }

        #endregion

        #region Address Generation

        public string GenerateAddress()
        {
            var streetNumber = _random.Next(100, 9999);
            var streetName = StreetNames[_random.Next(StreetNames.Length)];
            var streetSuffix = StreetSuffixes[_random.Next(StreetSuffixes.Length)];
            var city = Cities[_random.Next(Cities.Length)];
            var state = States[_random.Next(States.Length)];
            var zip = _random.Next(10000, 99999);

            return $"{streetNumber} {streetName} {streetSuffix}, {city}, {state} {zip}";
        }

        #endregion

        #region Date Generation

        public DateTime GenerateBirthDate(int minAge = 18, int maxAge = 85)
        {
            var today = DateTime.Today;
            var minDate = today.AddYears(-maxAge);
            var maxDate = today.AddYears(-minAge);
            var range = (maxDate - minDate).Days;
            return minDate.AddDays(_random.Next(range));
        }

        public DateTime GenerateGraduationDate(DateTime birthDate)
        {
            // Medical school graduation typically at age 26-30
            var gradAge = _random.Next(26, 31);
            var gradYear = birthDate.Year + gradAge;
            var gradMonth = _random.Next(1, 2) == 1 ? 5 : 6; // May or June
            return new DateTime(Math.Min(gradYear, DateTime.Today.Year - 1), gradMonth, 15);
        }

        #endregion

        #region Medical Data Generation

        public string RandomRace()
        {
            return Races[_random.Next(Races.Length)];
        }

        public List<MedicalSpecialization> GenerateSpecializations(int min = 1, int max = 5)
        {
            var allSpecs = Enum.GetValues<MedicalSpecialization>().ToArray();

            var count = _random.Next(min, max + 1);
            var selected = new HashSet<MedicalSpecialization>();

            while (selected.Count < count && selected.Count < allSpecs.Length)
            {
                selected.Add(allSpecs[_random.Next(allSpecs.Length)]);
            }

            return selected.ToList();
        }

        public string GenerateLicenseNumber()
        {
            var stateCode = States[_random.Next(States.Length)];
            var number = _random.Next(10000, 99999);
            return $"MD{stateCode}{number}";
        }

        public string RandomVisitReason()
        {
            return VisitReasons[_random.Next(VisitReasons.Length)];
        }

        #endregion

        #region Room Number Generation

        /// <summary>
        /// Generates a unique room number that hasn't been used.
        /// </summary>
        public int GenerateUniqueRoomNumber(int min = 100, int max = 999)
        {
            if (_usedRoomNumbers.Count >= (max - min + 1))
            {
                throw new InvalidOperationException("All room numbers in range have been used");
            }

            int roomNumber;
            do
            {
                roomNumber = _random.Next(min, max + 1);
            } while (_usedRoomNumbers.Contains(roomNumber));

            _usedRoomNumbers.Add(roomNumber);
            return roomNumber;
        }

        #endregion

        #region Password Generation

        public string GeneratePassword()
        {
            // Simple password for dev: word + numbers
            var words = new[] { "pass", "test", "demo", "dev", "clinic" };
            return $"{words[_random.Next(words.Length)]}{_random.Next(100, 999)}";
        }

        #endregion
    }
}

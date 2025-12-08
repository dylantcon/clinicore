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

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomDataGenerator"/> class, optionally using a specified seed
        /// for deterministic random data generation.
        /// </summary>
        /// <param name="seed">An optional seed value to initialize the random number generator. If specified, the generated data will be
        /// repeatable for the same seed; otherwise, a time-dependent default seed is used for non-deterministic
        /// results.</param>
        public RandomDataGenerator(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Reserves usernames that should not be generated (e.g., hardcoded ones).
        /// </summary>
        /// <param name="usernames">A collection of usernames to reserve.</param>
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
        /// <param name="roomNumbers">A collection of room numbers to reserve.</param>
        public void ReserveRoomNumbers(IEnumerable<int> roomNumbers)
        {
            foreach (var room in roomNumbers)
            {
                _usedRoomNumbers.Add(room);
            }
        }

        #region Name Generation

        /// <summary>
        /// Returns a randomly selected gender value.
        /// </summary>
        /// <returns>A <see cref="Gender"/> value representing either <see cref="Gender.Man"/> or <see cref="Gender.Woman"/>
        /// chosen at random.</returns>
        public Gender RandomGender()
        {
            return _random.Next(2) == 0 ? Gender.Man : Gender.Woman;
        }

        /// <summary>
        /// Returns a randomly selected first name appropriate for the specified gender.
        /// </summary>
        /// <param name="gender">The gender for which to generate a first name. Determines whether a male or female name is returned.</param>
        /// <returns>A randomly chosen first name corresponding to the specified gender.</returns>
        public string RandomFirstName(Gender gender)
        {
            var names = gender == Gender.Man ? MaleFirstNames : FemaleFirstNames;
            return names[_random.Next(names.Length)];
        }

        /// <summary>
        /// Returns a randomly selected last name from the available list.
        /// </summary>
        /// <returns>A string containing a last name chosen at random from the collection. The returned value is never null or
        /// empty.</returns>
        public string RandomLastName()
        {
            return LastNames[_random.Next(LastNames.Length)];
        }

        /// <summary>
        /// Generates a random patient name and associated gender.
        /// </summary>
        /// <remarks>The generated names and gender are selected randomly. This method is useful for
        /// creating test data or anonymized patient records.</remarks>
        /// <returns>A tuple containing the patient's full name, first name, last name, and gender.  The <c>FullName</c> is a
        /// concatenation of the first and last names. </returns>
        public (string FullName, string FirstName, string LastName, Gender Gender) GeneratePatientName()
        {
            var gender = RandomGender();
            var firstName = RandomFirstName(gender);
            var lastName = RandomLastName();
            return ($"{firstName} {lastName}", firstName, lastName, gender);
        }

        /// <summary>
        /// Generates a physician's name using a randomly selected last name.
        /// </summary>
        /// <remarks>Physicians are typically referred to by their last name only. The returned tuple
        /// provides both the full name and last name for convenience.</remarks>
        /// <returns>A tuple containing the physician's full name and last name. Both values will be the same, representing the
        /// physician's last name.</returns>
        public (string FullName, string LastName) GeneratePhysicianName()
        {
            var lastName = RandomLastName();
            return (lastName, lastName); // Physicians are referred to by last name
        }

        #endregion

        #region Username Generation

        /// <summary>
        /// Generates a unique username based on the specified first and last names.
        /// </summary>
        /// <remarks>The generated username is guaranteed to be unique among previously generated
        /// usernames within the current instance. Usernames are case-insensitive and always returned in
        /// lowercase.</remarks>
        /// <param name="firstName">The user's first name. Must not be null or empty.</param>
        /// <param name="lastName">The user's last name. Must not be null or empty.</param>
        /// <returns>A unique username in lowercase, formed by combining the first character of <paramref name="firstName"/> with
        /// <paramref name="lastName"/>. If the generated username is already in use, a numeric suffix is appended to
        /// ensure uniqueness.</returns>
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

        /// <summary>
        /// Generates a random address string composed of a street number, street name, street suffix, city, state, and
        /// ZIP code.
        /// </summary>
        /// <remarks>The generated address is intended for use in testing, sample data, or scenarios where
        /// a realistic but fictitious address is required. The format of the returned address is: "StreetNumber
        /// StreetName StreetSuffix, City, State ZIP".</remarks>
        /// <returns>A string containing a randomly generated address in the format "1234 Main St, Springfield, NY 12345".</returns>
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

        /// <summary>
        /// Generates a random birth date such that the resulting age falls within the specified minimum and maximum age
        /// range.
        /// </summary>
        /// <remarks>The generated birth date is calculated based on the current date. The age range is
        /// inclusive, meaning the returned date will result in an age between <paramref name="minAge"/> and <paramref
        /// name="maxAge"/>, inclusive, as of today.</remarks>
        /// <param name="minAge">The minimum age, in years, that the generated birth date should correspond to. Must be greater than zero and
        /// less than or equal to <paramref name="maxAge"/>.</param>
        /// <param name="maxAge">The maximum age, in years, that the generated birth date should correspond to. Must be greater than or equal
        /// to <paramref name="minAge"/>.</param>
        /// <returns>A <see cref="DateTime"/> representing a birth date for an individual whose age is between <paramref
        /// name="minAge"/> and <paramref name="maxAge"/>, inclusive.</returns>
        public DateTime GenerateBirthDate(int minAge = 18, int maxAge = 85)
        {
            var today = DateTime.Today;
            var minDate = today.AddYears(-maxAge);
            var maxDate = today.AddYears(-minAge);
            var range = (maxDate - minDate).Days;
            return minDate.AddDays(_random.Next(range));
        }

        /// <summary>
        /// Generates a plausible medical school graduation date based on the specified birth date.
        /// </summary>
        /// <remarks>The returned date assumes a typical medical school graduation age range and randomly
        /// selects either May or June as the graduation month. If the calculated graduation year is in the future, it
        /// is capped to the previous year.</remarks>
        /// <param name="birthDate">The date of birth used to estimate the graduation year. Must be a valid <see cref="DateTime"/> value.</param>
        /// <returns>A <see cref="DateTime"/> representing the estimated graduation date, typically in May or June, when the
        /// individual would be between 26 and 30 years old. The graduation year will not be in the current year or
        /// later.</returns>
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

        /// <summary>
        /// Returns a randomly selected race from the available set of races.
        /// </summary>
        /// <returns>A string representing a race chosen at random from the predefined list of races.</returns>
        public string RandomRace()
        {
            return Races[_random.Next(Races.Length)];
        }

        /// <summary>
        /// Generates a random list of distinct medical specializations.
        /// </summary>
        /// <remarks>The returned list will not contain duplicate specializations. If the range specified
        /// by <paramref name="min"/> and <paramref name="max"/> exceeds the number of available specializations, the
        /// list will contain at most all available specializations.</remarks>
        /// <param name="min">The minimum number of specializations to generate. Must be greater than zero and less than or equal to
        /// <paramref name="max"/>.</param>
        /// <param name="max">The maximum number of specializations to generate. Must be greater than or equal to <paramref name="min"/>
        /// and less than or equal to the total number of available specializations.</param>
        /// <returns>A list of <see cref="MedicalSpecialization"/> values containing randomly selected, distinct specializations.
        /// The list will contain between <paramref name="min"/> and <paramref name="max"/> items, depending on the
        /// available specializations.</returns>
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

        /// <summary>
        /// Generates a random medical license number in the format "MD{stateCode}{number}".
        /// </summary>
        /// <remarks>The generated license number is not guaranteed to be unique or valid for any official
        /// use. The format follows "MD" prefix, a state code, and a five-digit number.</remarks>
        /// <returns>A string representing a medical license number, where <c>stateCode</c> is a randomly selected state
        /// abbreviation and <c>number</c> is a five-digit number.</returns>
        public string GenerateLicenseNumber()
        {
            var stateCode = States[_random.Next(States.Length)];
            var number = _random.Next(10000, 99999);
            return $"MD{stateCode}{number}";
        }

        /// <summary>
        /// Returns a randomly selected visit reason from the available list.
        /// </summary>
        /// <returns>A string containing one of the predefined visit reasons, chosen at random.</returns>
        public string RandomVisitReason()
        {
            return VisitReasons[_random.Next(VisitReasons.Length)];
        }

        #endregion

        #region Room Number Generation

        /// <summary>
        /// Generates a unique room number that hasn't been used.
        /// </summary>
        /// <param name="min">The minimum room number (inclusive).</param>
        /// <param name="max">The maximum room number (inclusive).</param>
        /// <returns>A unique integer representing a room number.</returns>
        /// <exception cref="InvalidOperationException">Thrown when all available room numbers in the specified range have been used.</exception>
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

        /// <summary>
        /// Generates a simple password consisting of a predefined word followed by a random three-digit number.
        /// </summary>
        /// <remarks>This method is intended for development or testing scenarios and does not produce
        /// secure passwords suitable for production use. The generated password combines one of several fixed words
        /// with a random number between 100 and 998.</remarks>
        /// <returns>A string containing the generated password in the format "word###", where "word" is one of the predefined
        /// options and "###" is a random three-digit number.</returns>
        public string GeneratePassword()
        {
            // Simple password for dev: word + numbers
            var words = new[] { "pass", "test", "demo", "dev", "clinic" };
            return $"{words[_random.Next(words.Length)]}{_random.Next(100, 999)}";
        }

        #endregion
    }
}

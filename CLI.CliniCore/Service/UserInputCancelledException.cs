using System;

namespace CLI.CliniCore.Service
{
    /// <summary>
    /// Exception thrown when user cancels input by pressing Escape
    /// </summary>
    public class UserInputCancelledException : Exception
    {
        public UserInputCancelledException() 
            : base("User cancelled input operation")
        {
        }

        public UserInputCancelledException(string message) 
            : base(message)
        {
        }

        public UserInputCancelledException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
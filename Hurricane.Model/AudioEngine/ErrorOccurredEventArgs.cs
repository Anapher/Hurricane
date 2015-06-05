using System;

namespace Hurricane.Model.AudioEngine
{
    /// <summary>
    /// The event args for the <see cref="IAudioEngine.ErrorOccurred"/> event
    /// </summary>
    public class ErrorOccurredEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="ErrorOccurredEventArgs"/>
        /// </summary>
        /// <param name="errorMessage">The message of the error</param>
        public ErrorOccurredEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// The message of the error
        /// </summary>
        public string ErrorMessage { get; }
    }
}
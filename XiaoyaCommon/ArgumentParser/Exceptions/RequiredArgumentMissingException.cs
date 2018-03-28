using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.ArgumentParser.Exceptions
{
    /// <summary>
    /// Thrown when an argument with required attribute is not found
    /// </summary>
    [Serializable]
    public class RequiredArgumentMissingException : Exception
    {
        /// <summary>
        /// The name of the missing argument.
        /// </summary>
        public string ArgumentName { get; private set; }

        internal RequiredArgumentMissingException(string message, string argumentName) : base(message)
        {
            this.ArgumentName = argumentName;
        }
    }
}

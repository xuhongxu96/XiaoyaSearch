using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.ArgumentParser.Exceptions
{
    /// <summary>
    /// Indicates that an argument has in invalid value.
    /// </summary>
    [Serializable]
    public class InvalidArgumentValueException : Exception
    {
        /// <summary>
        /// The name of the argument with invalid value.
        /// </summary>
        public string ArgumentName { get; private set; }

        /// <summary>
        /// The value provided for the argument.
        /// </summary>
        public string ArgumentValue { get; private set; }

        internal InvalidArgumentValueException(string argumentName, string message)
            : base(message)
        {
            this.ArgumentName = argumentName;
        }

        internal InvalidArgumentValueException(string argumentName, string argumentValue, string message)
            : this(argumentName, message)
        {
            this.ArgumentValue = argumentValue;
        }

        internal InvalidArgumentValueException(string argumentName, string argumentValue, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ArgumentName = argumentName;
            this.ArgumentValue = argumentValue;
        }
    }
}

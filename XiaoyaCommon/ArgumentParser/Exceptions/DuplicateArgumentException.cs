using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.ArgumentParser.Exceptions
{
    /// <summary>
    /// Exception is thrown when a a duplicate argument identifier is found.
    /// An argument identifier can be its name or alias.
    /// </summary>
    [Serializable]
    public class DuplicateArgumentException : Exception
    {
        /// <summary>
        /// The argument name or alias which is  duplicated.
        /// </summary>
        public string ArgumentIdentifier { get; private set; }

        internal DuplicateArgumentException(string argumentIndentifier, string message) : base(message)
        {
            this.ArgumentIdentifier = argumentIndentifier;
        }
    }
}

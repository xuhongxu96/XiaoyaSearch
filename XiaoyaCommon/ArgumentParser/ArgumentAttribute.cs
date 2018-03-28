using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaCommon.ArgumentParser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// The argument name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The argument alias, usually used as an short alternative to name.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Optional regex used to validate the argument value.
        /// By default regex validation is not applied over default value.
        /// </summary>
        public string RegexValidation { get; set; }

        /// <summary>
        /// The default value for argument. If the argument is not given, this default value will be used.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Defines if the argument is required. If a argument is required and not value for it is
        /// provided and exception is thown.
        /// With this option, "DefaultValue" should not be used.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Bind the current property to a argument.
        /// </summary>
        /// <param name="name">The argument name</param>
        public ArgumentAttribute(string name)
        {
            this.Name = name;
        }
    }
}

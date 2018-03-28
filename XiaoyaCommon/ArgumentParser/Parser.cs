using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using XiaoyaCommon.ArgumentParser.Exceptions;

namespace XiaoyaCommon.ArgumentParser
{
    public static class Parser
    {
        /// <summary>
        /// Get an instance of T populated with values provided by arguments parameter.
        /// </summary>
        /// <typeparam name="T">A class type containing properties with Argument attribute.</typeparam>
        /// <param name="arguments">The arguments and it's values, just like they are receveid by console application args</param>
        /// <returns>And instance of T with populated properties values from arguments</returns>
        public static T ParseArguments<T>(params string[] arguments)
            where T : class
        {
            var propertyInfoArgumentAttributeList = new List<PropertyInfoArgumentAttribute>();

            // Dictionary which will be used to find a propertyInfoArgumentAttribute by its name or alias
            var propertyArgumentsDictionary = new Dictionary<string, PropertyInfoArgumentAttribute>();

            // Retrieve the properties that are bindable to arguments
            foreach (var propArgument in GetObjectBindableProperties<T>())
            {
                string argumentName = propArgument.ArgumentAttribute.Name;

                if (propertyArgumentsDictionary.ContainsKey(argumentName))
                {
                    throw new DuplicateArgumentException(argumentName,
                        string.Format("The argument name must be unique between other arguments names and aliases. " +
                                      "Argument name: {0}.", argumentName));
                }

                // Add the property argument into the dictionary using the argument name as the key.
                propertyArgumentsDictionary.Add(argumentName, propArgument);

                string argumentAlias = propArgument.ArgumentAttribute.Alias;
                if (!string.IsNullOrWhiteSpace(argumentAlias))
                {
                    // If already exists and argument with the given name/alias, an DuplicateArgumentException is thrown.
                    if (propertyArgumentsDictionary.ContainsKey(argumentAlias))
                    {
                        throw new DuplicateArgumentException(argumentAlias,
                            string.Format("The argument alias must be unique between other arguments names and aliases. " +
                                          "Argument alias: {0}.", argumentAlias));
                    }

                    // Add another entry into dictionary, now mapping the property argument
                    // with the argument alias.
                    propertyArgumentsDictionary.Add(argumentAlias, propArgument);
                }

                // Add a reference to list
                propertyInfoArgumentAttributeList.Add(propArgument);
            }

            return SetupArgumentObject<T>(arguments, propertyInfoArgumentAttributeList, propertyArgumentsDictionary);
        }

        private static T SetupArgumentObject<T>(string[] arguments, List<PropertyInfoArgumentAttribute> propertyInfoArgumentAttributeList,
            Dictionary<string, PropertyInfoArgumentAttribute> propertyArgumentsDictionary)
            where T : class
        {
            T argumentObject = Activator.CreateInstance<T>();

            // Store the idenfier of the arguments provided, name or alias.
            var providedArguments = new HashSet<string>();

            #region Iterate trought arguments, setting their values into the object
            for (int i = 0; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                providedArguments.Add(argument);

                if (propertyArgumentsDictionary.TryGetValue(argument, out PropertyInfoArgumentAttribute propInfoArgumentAttribute))
                {
                    var propertyType = propInfoArgumentAttribute.PropertyInfo.PropertyType;

                    // If the property binded to the current argument is a boolean, the existence
                    // of the argument sets the property value to "true".
                    if (propertyType == typeof(bool))
                        propInfoArgumentAttribute.PropertyInfo.SetValue(argumentObject, true, null);
                    else
                    {
                        if (arguments.Length <= i + 1)
                        {
                            throw new InvalidArgumentValueException(argument,
                                string.Format("The argument value was not found. " +
                                              "The argument provided expects a value: {0}", argument));
                        }

                        // Get the value for the current argument.
                        string argumentValueStr = arguments[++i];

                        if (propInfoArgumentAttribute.ArgumentAttribute.Required && string.IsNullOrEmpty(argumentValueStr))
                            throw new InvalidArgumentValueException(argument, argumentValueStr,
                                string.Format("The argument must have a value: {0}", argumentValueStr));

                        // If there is an regex validation provided, validade de argument value against it.
                        if (propInfoArgumentAttribute.ArgumentAttribute.RegexValidation != null)
                        {
                            bool isValid = Regex.IsMatch(argumentValueStr, propInfoArgumentAttribute.ArgumentAttribute.RegexValidation);
                            if (!isValid)
                            {
                                throw new InvalidArgumentValueException(argument, argumentValueStr,
                                    string.Format("The argument value did no matched the regex validation. " +
                                                  "Argument name: {1}{0}Argument value: {2}",
                                                  Environment.NewLine, argument, argumentValueStr));
                            }
                        }

                        object argumentValue = null;
                        try
                        {
                            // If the argument is anything else, try to convert the next argument, which
                            // is the value of the current parameter key, to the property info type.
                            argumentValue = Convert.ChangeType(argumentValueStr, propertyType);
                        }
                        catch (Exception changeTypeException)
                        {
                            throw new InvalidArgumentValueException(argument, argumentValueStr,
                                string.Format("The argument value cannot be converted to the property type. " +
                                              "Argument name: {1}{0}Argument value: {2}",
                                              Environment.NewLine, argument, argumentValueStr),
                                changeTypeException);
                        }

                        propInfoArgumentAttribute.PropertyInfo.SetValue(argumentObject, argumentValue, null);
                    }
                }
                else
                    throw new InvalidArgumentException(argument,
                        string.Format("Argument was not found on valid arguments list: {0}", argument));
            }
            #endregion

            #region Iterate through argument object properties checking it's requirements and default value

            foreach (var propInfoArgumentAttribute in propertyInfoArgumentAttributeList)
            {
                var argumentAttribute = propInfoArgumentAttribute.ArgumentAttribute;

                // Check if the argument name and alias provided
                if (!providedArguments.Contains(argumentAttribute.Name) &&
                   !providedArguments.Contains(argumentAttribute.Alias))
                {
                    // If the argument wasn't provided, and it is required, throw an error.
                    if (argumentAttribute.Required)
                    {
                        throw new RequiredArgumentMissingException(
                                string.Format("The required argument was not provided: {0}",
                                    argumentAttribute.Name),
                                argumentAttribute.Name);
                    }

                    // If there is and default value for this argument, set it to the object property
                    if (argumentAttribute.DefaultValue != null)
                    {
                        // try to convert default argument object, to the property type.
                        var argumentValue = Convert.ChangeType(argumentAttribute.DefaultValue,
                            propInfoArgumentAttribute.PropertyInfo.PropertyType);

                        // Set the default value to the property
                        propInfoArgumentAttribute.PropertyInfo.SetValue(argumentObject, argumentValue, null);
                    }
                }
            }

            #endregion

            return argumentObject;
        }

        /// <summary>
        /// Get the PropertyInfoArgumentAttribute collection for each property on type T
        /// which have an ArgumentAttribute.
        /// </summary>
        private static IEnumerable<PropertyInfoArgumentAttribute> GetObjectBindableProperties<T>()
        {
            var argumentAttributeType = typeof(ArgumentAttribute);
            foreach (var prop in typeof(T).GetProperties())
            {
                var argumentAttributes = prop.GetCustomAttributes(argumentAttributeType, true);

                // If the property has no argument configured, skip it
                if (argumentAttributes == null || argumentAttributes.Length == 0)
                    continue;

                var argumentAttribute = (ArgumentAttribute)argumentAttributes[0];

                yield return new PropertyInfoArgumentAttribute(prop, argumentAttribute);
            }
        }

        private class PropertyInfoArgumentAttribute
        {
            public PropertyInfo PropertyInfo { get; private set; }
            public ArgumentAttribute ArgumentAttribute { get; private set; }

            public PropertyInfoArgumentAttribute(PropertyInfo propertyInfo, ArgumentAttribute argumentAttr)
            {
                this.PropertyInfo = propertyInfo;
                this.ArgumentAttribute = argumentAttr;
            }
        }
    }
}

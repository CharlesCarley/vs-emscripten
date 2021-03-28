/*
-------------------------------------------------------------------------------
    Copyright (c) Charles Carley.

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
-------------------------------------------------------------------------------
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EmscriptenTask
{
    public interface IConvertAttribute
    {
        bool ConvertTo(PropertyInfo prop, object obj);
    }

    /// <summary>
    /// A boolean switch attribute.
    /// </summary>
    public class BoolSwitch : Attribute, IConvertAttribute
    {
        public BoolSwitch(string valueIfTrue = null, string valueIfFalse = null)
        {
            ValueIfTrue  = valueIfTrue;
            ValueIfFalse = valueIfFalse;
        }

        /// <summary>
        /// The switch value if the attribute's property is false.
        /// This can be null, in which case the switch will out be
        /// written to the command line.
        /// </summary>
        public string ValueIfFalse { get; }
        /// <summary>
        /// The switch value if the attribute's property is true.
        /// This can be null, in which case the switch will out be
        /// written to the command line.
        /// </summary>
        public string ValueIfTrue { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var result = prop.PropertyType == typeof(bool);
            if (result)
                ConvertedValue = (bool)prop.GetValue(obj);
            return result;
        }

        public bool ConvertedValue { get; set; }
    }

    /// <summary>
    /// A multi value string based command line switch.
    /// </summary>
    public class SeparatedStringSwitch : Attribute, IConvertAttribute
    {
        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="switchValue">The switch value to put in place of the separator.</param>
        /// <param name="requiresValidation">If this is true the value will be skipped if its not a valid file or directory.</param>
        /// <param name="separator">The character that defines a split</param>
        public SeparatedStringSwitch(string switchValue, bool requiresValidation = false, bool quoteIfHasWs = false, char separator = ';')
        {
            SwitchValue        = switchValue;
            Separator          = separator;
            RequiresValidation = requiresValidation;
            Quote              = quoteIfHasWs;
        }

        /// <summary>
        /// The switch value to put in place of the separator.
        /// </summary>
        public string SwitchValue { get; }

        /// <summary>
        /// The character that defines a split.
        /// </summary>
        public char Separator { get; }

        /// <summary>
        /// Quote the value if it has white space.
        /// </summary>
        public bool Quote { get; }

        /// <summary>
        /// If this is true the value will be skipped if its not a valid file or directory.
        /// </summary>
        public bool RequiresValidation { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var result = prop.PropertyType == typeof(string);
            if (result)
                ConvertedValue = (string)prop.GetValue(obj);
            return result;
        }

        public string ConvertedValue { get; private set; }
    }

    /// <summary>
    /// A string based command line switch.
    /// </summary>
    public class StringSwitch : Attribute, IConvertAttribute
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="switchValue">
        /// The switch value for the attribute's property.
        /// If this value is null the property value for this attribute will be directly
        /// written to the command line.
        /// </param>
        public StringSwitch(string switchValue = null, bool quoteIfHasWs = false)
        {
            SwitchValue = switchValue;
            Quote       = quoteIfHasWs;
        }

        /// <summary>
        /// The switch value for the attribute's property.
        /// </summary>
        public string SwitchValue { get; }
        public bool   Quote { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var result = prop.PropertyType == typeof(string);
            if (result)
            {
                var value = (string)prop.GetValue(obj);
                if (!string.IsNullOrEmpty(value))
                {
                    if (Quote && value.Contains(" "))
                        ConvertedValue = $"\"{value}\"";
                    else
                        ConvertedValue = value;
                }
            }
            return result;
        }

        public string ConvertedValue { get; private set; }
    }

    /// <summary>
    /// An integer based command line switch.
    /// </summary>
    public class IntSwitch : Attribute, IConvertAttribute
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="switchValue">The switch value for the attribute's property.</param>
        /// <param name="validValues">An optional list of valid values to test against the supplied value.</param>
        public IntSwitch(string switchValue, int[] validValues = null)
        {
            SwitchValue = switchValue;
            ValidValues = validValues;
        }
        /// <summary>
        /// The switch value for the attribute's property.
        /// </summary>
        public string SwitchValue { get; }
        /// <summary>
        /// An optional list of valid values to test against the supplied value.
        /// </summary>
        public int[] ValidValues { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var result = false;
            if (prop.PropertyType == typeof(int))
            {
                ConvertedValue = ((int)prop.GetValue(obj)).ToString();
                result         = true;
            }

            if (result)
                return true;

            result = prop.PropertyType == typeof(string);
            if (result)
                ConvertedValue = (string)prop.GetValue(obj);
            return result;
        }

        public string ConvertedValue { get; private set; }
    }

    /// <summary>
    ///  An enum based command line switch.
    /// </summary>
    public class EnumSwitch : Attribute
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="values">A comma separated list of possible options.</param>
        /// <param name="switches">A comma separated list of corresponding switch values.</param>
        /// <param name="defaultValue">A default switch value</param>
        public EnumSwitch(string values, string switches, string defaultValue = null)
        {
            Values   = values.Split(',');
            Switches = switches.Split(',');
            Default  = defaultValue;
        }

        /// <summary>
        /// A default switch value if no value or an invalid value is supplied.
        /// </summary>
        public string Default { get; }
        /// <summary>
        /// A list of possible options.
        /// </summary>
        public string[] Values { get; }

        /// <summary>
        /// A one to one list of corresponding switch values.
        /// </summary>
        public string[] Switches { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var result = prop.PropertyType == typeof(string);
            if (result)
                ConvertedValue = (string)prop.GetValue(obj);
            return result;
        }

        public string ConvertedValue { get; private set; }
    }

    public static class EmSwitchWriter
    {
        public static void Write(TextWriter builder, Type type, object inst)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var attributes = prop.GetCustomAttributes();
                foreach (var attr in attributes)
                {
                    switch (attr)
                    {
                    case BoolSwitch obj when obj.ConvertTo(prop, inst):
                        WriteSwitch(builder, obj);
                        break;
                    case SeparatedStringSwitch obj when obj.ConvertTo(prop, inst):
                        WriteSwitch(builder, obj);
                        break;
                    case StringSwitch obj when obj.ConvertTo(prop, inst):
                        WriteSwitch(builder, obj);
                        break;
                    case EnumSwitch obj when obj.ConvertTo(prop, inst):
                        WriteSwitch(builder, obj);
                        break;
                    case IntSwitch obj when obj.ConvertTo(prop, inst):
                        WriteSwitch(builder, obj);
                        break;
                    }
                }
            }
        }

        private static void WriteSwitch(TextWriter builder, BoolSwitch obj)
        {
            switch (obj.ConvertedValue)
            {
            case true when obj.ValueIfTrue != null:
                builder.Write($" {obj.ValueIfTrue}");
                break;
            case false when obj.ValueIfFalse != null:
                builder.Write($" {obj.ValueIfFalse}");
                break;
            }
        }

        private static void WriteSwitch(TextWriter            builder,
                                        SeparatedStringSwitch obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue) ||
                string.IsNullOrEmpty(obj.SwitchValue))
                return;

            var result = EmUtils.SeparatePaths(obj.ConvertedValue,
                                               obj.Separator,
                                               obj.SwitchValue,
                                               obj.RequiresValidation,
                                               obj.Quote);
            if (!string.IsNullOrEmpty(result))
                builder.Write($" {result}");
        }

        private static void WriteSwitch(TextWriter   builder,
                                        StringSwitch obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue))
                return;

            builder.Write(string.IsNullOrEmpty(obj.SwitchValue)
                              ? $" {obj.ConvertedValue}"
                              : $" {obj.SwitchValue} {obj.ConvertedValue}");
        }

        private static void WriteSwitch(TextWriter builder,
                                        EnumSwitch obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue))
            {
                if (obj.Default != null)
                    builder.Write($" {obj.Default}");
                return;
            }

            var i = obj.Values.TakeWhile(sv => !sv.Equals(obj.ConvertedValue)).Count();
            if (i < obj.Switches.Length)
                builder.Write($" {obj.Switches[i]}");
            else if (obj.Default != null)
            {
                builder.Write($" {obj.Default}");
            }
        }

        private static void WriteSwitch(TextWriter builder,
                                        IntSwitch  obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue) ||
                string.IsNullOrEmpty(obj.SwitchValue))
                return;

            if (!int.TryParse(obj.ConvertedValue, out var outResult))
                return;

            if (obj.ValidValues != null)
            {
                if (obj.ValidValues.Contains(outResult))
                    builder.Write($" {obj.SwitchValue}{outResult}");
            }
            else
                builder.Write($" {obj.SwitchValue}{outResult}");
        }
    }
}

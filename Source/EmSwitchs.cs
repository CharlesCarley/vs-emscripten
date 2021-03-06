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
using Microsoft.Build.Framework;
using static EmscriptenTask.EmUtils;

namespace EmscriptenTask
{
    public class BaseSwitch : Attribute
    {
        public const int PadSwitch          = 0x01;
        public const int GlueSwitch         = 0x02;
        public const int RequiresValidation = 0x04;
        public const int NoQuote            = 0x08;
        public const int QuoteIfWhiteSpace  = 0x10;
        public const int AlwaysQuote        = 0x20;
        public const int PreValidated       = 0x40;
        public const int SkipIfZero         = 0x80;
        public const int DefaultFlags       = PadSwitch | NoQuote;

        public BaseSwitch(int flags = DefaultFlags)
        {
            Flags = flags;
        }

        public bool NeedsValidation()
        {
            return (Flags & RequiresValidation) != 0;
        }

        public bool CanQuote()
        {
            return (Flags & NoQuote) == 0;
        }

        public bool CanQuoteIfWhiteSpace()
        {
            return CanQuote() && (Flags & QuoteIfWhiteSpace) != 0;
        }

        public bool ShouldAlwaysQuote()
        {
            return CanQuote() && (Flags & AlwaysQuote) != 0;
        }

        public bool ShouldGlueSwitch()
        {
            return (Flags & GlueSwitch) != 0;
        }

        public bool PreConverted()
        {
            return (Flags & PreValidated) != 0;
        }

        public bool ShouldSkipIfZero()
        {
            return (Flags & SkipIfZero) != 0;
        }

        protected int Flags { get; set; }
    }

    /// <summary>
    /// A boolean switch attribute.
    /// </summary>
    public class BoolSwitch : BaseSwitch
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
            var value = prop.GetValue(obj);
            if (value is null)
                return false;
            var result = value is bool;
            if (result)
                ConvertedValue = (bool)value;
            return result;
        }

        public bool ConvertedValue { get; set; }
    }

    /// <summary>
    /// A multi value string based command line switch.
    /// </summary>
    public class SeparatedStringSwitch : BaseSwitch
    {
        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="switchValue">The switch value to put in place of the separator.</param>
        /// <param name="separator">The character that defines a split</param>
        public SeparatedStringSwitch(string switchValue,
                                     int    flags     = DefaultFlags,
                                     char   separator = ';') :
            base(flags)
        {
            SwitchValue = switchValue;
            Separator   = separator;
        }

        /// <summary>
        /// The switch value to put in place of the separator.
        /// </summary>
        public string SwitchValue { get; }

        /// <summary>
        /// The character that defines a split.
        /// </summary>
        public char Separator { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var value = prop.GetValue(obj);
            if (value is null)
                return false;

            var result = value is string;
            if (result)
                ConvertedValue = (string)value;
            else if (value is ITaskItem[] items)
            {
                Flags |= PreValidated;
                result         = true;
                ConvertedValue = GetSeparatedSource(Separator, items, CanQuoteIfWhiteSpace());
            }
            return result;
        }

        public string ConvertedValue { get; private set; }
    }

    /// <summary>
    /// A string based command line switch.
    /// </summary>
    public class StringSwitch : BaseSwitch
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="switchValue">
        /// The switch value for the attribute's property.
        /// If this value is null the property value for this attribute will be directly
        /// written to the command line.
        /// </param>
        public StringSwitch(string switchValue = null,
                            int    extraFlags  = DefaultFlags) :
            base(extraFlags)
        {
            SwitchValue = switchValue;
        }

        /// <summary>
        /// The switch value for the attribute's property.
        /// </summary>
        public string SwitchValue { get; }

        public bool ConvertTo(PropertyInfo prop, object obj)
        {
            var value = prop.GetValue(obj);
            if (value is null)
                return false;

            string sValue;
            switch (value)
            {
            case string str:
                sValue = str;
                break;
            case ITaskItem item:
                sValue = item.GetMetadata(FullPath);
                break;
            default:
                return false;
            }

            sValue = sValue?.Trim();
            if (string.IsNullOrEmpty(sValue))
                return false;

            if (NeedsValidation())
            {
                if (!IsFileOrDirectory(sValue))
                    return false;
            }

            if (ShouldAlwaysQuote() || CanQuoteIfWhiteSpace() && sValue.Contains(" "))
                ConvertedValue = $"\"{sValue}\"";
            else
                ConvertedValue = sValue;
            return true;
        }

        public string ConvertedValue { get; private set; }
    }

    /// <summary>
    /// An integer based command line switch.
    /// </summary>
    public class IntSwitch : BaseSwitch
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="switchValue">The switch value for the attribute's property.</param>
        /// <param name="validValues">An optional list of valid values to test against the supplied value.</param>
        public IntSwitch(string switchValue, int[] validValues = null, int extraFlags = GlueSwitch) :
            base(extraFlags)
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
            var value = prop.GetValue(obj);
            switch (value)
            {
            case null:
                return false;
            case int i:
                ConvertedValue = i.ToString();
                return true;
            }
            var result = value is string;
            if (!result)
                return false;
            ConvertedValue = (string)value;
            ConvertedValue = ConvertedValue?.Trim();
            return true;
        }

        public string ConvertedValue { get; private set; }
    }

    /// <summary>
    ///  An enum based command line switch.
    /// </summary>
    public class EnumSwitch : BaseSwitch
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="values">A comma separated list of possible options.</param>
        /// <param name="switches">A comma separated list of corresponding switch values.</param>
        /// <param name="defaultValue">A default switch value</param>
        public EnumSwitch(string values, string switches, string defaultValue = null, int extraFlags = DefaultFlags) :
            base(extraFlags)
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
            var value = prop.GetValue(obj);
            if (value is null)
                return false;
            var result = value is string;
            if (result)
                ConvertedValue = (string)value;
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
                builder.Write(' ');
                builder.Write(obj.ValueIfTrue);
                break;
            case false when obj.ValueIfFalse != null:
                builder.Write(' ');
                builder.Write(obj.ValueIfFalse);
                break;
            }
        }

        private static void WriteSwitch(TextWriter            builder,
                                        SeparatedStringSwitch obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue) ||
                string.IsNullOrEmpty(obj.SwitchValue))
                return;

            if (obj.PreConverted())
            {
                builder.Write(obj.ConvertedValue);
                return;
            }

            var result = SeparatePaths(obj.ConvertedValue,
                                       obj.Separator,
                                       obj.SwitchValue,
                                       obj.NeedsValidation(),
                                       obj.CanQuoteIfWhiteSpace(),
                                       obj.ShouldGlueSwitch());
            if (string.IsNullOrEmpty(result))
                return;

            builder.Write(' ');
            builder.Write(result);
        }

        private static void WriteSwitch(TextWriter   builder,
                                        StringSwitch obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue))
                return;

            builder.Write(' ');
            builder.Write(string.IsNullOrEmpty(obj.SwitchValue)
                              ? obj.ConvertedValue
                          : obj.ShouldGlueSwitch() ? $"{obj.SwitchValue}{obj.ConvertedValue}"
                                                   : $"{obj.SwitchValue} {obj.ConvertedValue}");
        }

        private static void WriteSwitch(TextWriter builder,
                                        EnumSwitch obj)
        {
            if (string.IsNullOrEmpty(obj.ConvertedValue))
            {
                if (string.IsNullOrEmpty(obj.Default))
                    return;

                builder.Write(' ');
                builder.Write($"{obj.Default}");
                return;
            }

            var found = false;
            int i;
            for (i = 0; i < obj.Values.Length; ++i)
            {
                if (!obj.ConvertedValue.Equals(obj.Values[i]))
                    continue;

                found = true;
                break;
            }

            if (found)
            {
                if (string.IsNullOrEmpty(obj.Switches[i]))
                    return;
                if (string.IsNullOrWhiteSpace(obj.Switches[i]))
                    return;

                builder.Write(' ');
                builder.Write($"{obj.Switches[i]}");
            }
            else if (!string.IsNullOrEmpty(obj.Default))
            {
                builder.Write(' ');
                builder.Write($"{obj.Default}");
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

            if (obj.ShouldSkipIfZero() && outResult == 0)
                return;

            if (obj.ValidValues != null)
            {
                if (!obj.ValidValues.Contains(outResult))
                    return;

                builder.Write(' ');
                builder.Write(obj.ShouldGlueSwitch()
                                  ? $"{obj.SwitchValue}{outResult}"
                                  : $"{obj.SwitchValue} {outResult}");
            }
            else
            {
                builder.Write(' ');
                builder.Write(obj.ShouldGlueSwitch()
                                  ? $"{obj.SwitchValue}{outResult}"
                                  : $"{obj.SwitchValue} {outResult}");
            }
        }
    }
}

// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

//-----------------------------------------------------------------------
// <copyright file="StandardOutWriter.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Helios.Util
{
    /// <summary>
    ///     This class contains methods for thread safe writing to the standard output stream.
    /// </summary>
    internal static class StandardOutWriter
    {
        private static readonly object _lock = new object();

        /// <summary>
        ///     Writes the specified <see cref="string" /> value to the standard output stream. Optionally
        ///     you may specify which colors should be used.
        /// </summary>
        /// <param name="message">The <see cref="string" /> value to write</param>
        /// <param name="foregroundColor">Optional: The foreground color</param>
        /// <param name="backgroundColor">Optional: The background color</param>
        public static void Write(string message, ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null)
        {
            WriteToConsole(message, foregroundColor, backgroundColor, false);
        }

        /// <summary>
        ///     Writes the specified <see cref="string" /> value, followed by the current line terminator,
        ///     to the standard output stream. Optionally you may specify which colors should be used.
        /// </summary>
        /// <param name="message">The <see cref="string" /> value to write</param>
        /// <param name="foregroundColor">Optional: The foreground color</param>
        /// <param name="backgroundColor">Optional: The background color</param>
        public static void WriteLine(string message, ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null)
        {
            WriteToConsole(message, foregroundColor, backgroundColor, true);
        }

        private static void WriteToConsole(string message, ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null, bool newLine = false)
        {
            lock (_lock)
            {
                ConsoleColor? fg = null;
                if (foregroundColor.HasValue)
                {
                    fg = Console.ForegroundColor;
                    Console.ForegroundColor = foregroundColor.Value;
                }
                ConsoleColor? bg = null;
                if (backgroundColor.HasValue)
                {
                    bg = Console.BackgroundColor;
                    Console.BackgroundColor = backgroundColor.Value;
                }

                if (newLine)
                    Console.WriteLine(message);
                else
                    Console.Write(message);

                if (fg.HasValue)
                {
                    Console.ForegroundColor = fg.Value;
                }
                if (bg.HasValue)
                {
                    Console.BackgroundColor = bg.Value;
                }
            }
        }
    }
}
// File: Stdio.cs
// Author: TwistingMercury
// Email: twistingmercury@outlook.com
// Copyright: Copyrighte © Jeremy K. Johnosn
// Date Created: 11/18/2018

#pragma warning disable IDE1006

using System;
namespace tsqlsh
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    /// <summary>
    /// A wrapper around System.Console for easier unit testing.
    /// </summary>
    public class Stdio : IStdio
    {
        private const ConsoleColor default_fg = ConsoleColor.White;
        private const ConsoleColor default_bg = ConsoleColor.Black;

        public void EmptyLine()
        {
            Console.WriteLine();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
        public void Write(string value)
        {
            this.write(Console.Write, value);
        }

        public void Write(string value, ConsoleColor fg)
        {
            this.write(Console.Write, value, fg);
        }

        public void Write(string value, ConsoleColor fg, ConsoleColor bg)
        {
            this.write(Console.Write, value, fg, bg);
        }

        public void WriteLine(string value)
        {
            this.write(Console.WriteLine, value);
        }

        public void WriteLine(string value, ConsoleColor fg)
        {
            this.write(Console.WriteLine, value, fg);
        }

        public void WriteLine(string value, ConsoleColor fg, ConsoleColor bg)
        {
            this.write(Console.WriteLine, value, fg, bg);
        }

        private void write(Action<string> writeAction, 
                           string value, 
                           ConsoleColor fg = default_fg, 
                           ConsoleColor bg = default_bg)
        {
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            writeAction(value);
            Console.ForegroundColor = default_fg;
            Console.BackgroundColor = default_bg;
        }
    }
}

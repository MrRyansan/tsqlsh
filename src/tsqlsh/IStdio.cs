// File: IConsole.cs
// Author: TwistingMercury
// Email: twistingmercury@outlook.com
// Copyright: Copyrighte © Jeremy K. Johnosn
// Date Created: 11/18/2018
using System;
namespace tsqlsh
{
    /// <summary>
    /// Provides an interface for writing to System.Console for better
    /// testablility.
    /// </summary>
    public interface IStdio
    {
        string ReadLine();
        void Write(string value);
        void WriteLine(string value);
        void Write(string value, ConsoleColor fg);
        void WriteLine(string value, ConsoleColor fg);
        void Write(string value, ConsoleColor fg, ConsoleColor bg);
        void WriteLine(string value, ConsoleColor fg, ConsoleColor bg);
        void EmptyLine();
    }
}

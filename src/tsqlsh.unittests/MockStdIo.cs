// File: MockStdIo.cs
// Author: TwistingMercury
// Email: twistingmercury@outlook.com
// Copyright: Copyrighte © Jeremy K. Johnosn
// Date Created: 11/18/2018

#pragma warning disable IDE1006

using System;
using System.IO;

namespace tsqlsh.unittests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class MockStdIo : IStdio
    {
        private readonly StringWriter writer = new StringWriter();
        private StringReader reader;

        internal string GetOutput()
        {
            return this.writer.ToString();
        }

        internal void SetInput(params string[] line)
        {
            var w = new StringWriter { NewLine = "\n" };
            foreach (var l in line)
            {
                w.WriteLine(l);
            }
            reader = new StringReader(w.ToString());
        }

        public void EmptyLine()
        {
            this.writer.Write("\n");
        }

        public string ReadLine()
        {
            return reader.ReadLine();
        }

        public void Write(string value)
        {
            writer.Write(value);
        }

        public void Write(string value, ConsoleColor fg)
        {
            writer.Write(value);
        }

        public void Write(string value, ConsoleColor fg, ConsoleColor bg)
        {
            writer.Write(value);
        }

        public void WriteLine(string value)
        {
            writer.WriteLine(value);
        }

        public void WriteLine(string value, ConsoleColor fg)
        {
            writer.WriteLine(value);
        }

        public void WriteLine(string value, ConsoleColor fg, ConsoleColor bg)
        {
            writer.WriteLine(value);
        }
    }
}

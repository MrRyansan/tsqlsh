using System;

namespace tsqlsh
{
    internal interface ISqlEngine: IDisposable
    {
        event EventHandler<string> DbChanged;
        void Connect(string cstr);
        ExecutionResults ExecuteCommand(string sql);
    }
}
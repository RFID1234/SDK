using System;

namespace SoochakBharat.SDK.Events
{
    public enum ReaderConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    public class ReaderStatusEventArgs : EventArgs
    {
        public string ReaderId { get; }
        public ReaderConnectionState State { get; }
        public string? Message { get; }

        public ReaderStatusEventArgs(string readerId, ReaderConnectionState state, string? message = null)
        {
            ReaderId = readerId;
            State = state;
            Message = message;
        }
    }
}

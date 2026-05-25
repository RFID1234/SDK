using System;
using SoochakBharat.SDK.Models;

namespace SoochakBharat.SDK.Events
{
    public class TagReadEventArgs : EventArgs
    {
        public string ReaderId { get; }
        public TagData Tag { get; }

        public TagReadEventArgs(TagData tag, string readerId)
        {
            Tag = tag;
            ReaderId = readerId;
        }

        public static TagReadEventArgs FromLlrp(LlrpTagReportItem item, string readerId)
            => new TagReadEventArgs(TagData.FromLlrp(item), readerId);

        public static TagReadEventArgs FromSim3500(dynamic simTag, string readerId)
            => new TagReadEventArgs(TagData.FromSim3500(simTag), readerId);
    }
}

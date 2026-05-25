using System;
using System.Collections.Generic;
using SoochakBharat.SDK.Models;

namespace SoochakBharat.SDK.Protocols.Llrp
{
    public class LlrpParser
    {
        public int GetMessageLength(byte[] header, int headerLen)
        {
            if (headerLen < 6) return 0;
            int len = (header[2] << 24) | (header[3] << 16) | (header[4] << 8) | header[5];
            return len;
        }

        public bool IsTagReport(byte[] message)
        {
            if (message.Length < 2) return false;
            return true;
        }

        public IEnumerable<LlrpTagReportItem> ParseTags(byte[] message)
        {
            return Array.Empty<LlrpTagReportItem>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class Request
    {
        public string Method { get; }
        public string Uri { get; }
        public string ProtocolName { get; }
        public string ProtocolVersion { get; }
        public Dictionary<string, string> Headers { get; }
        public string InnerText { get; }

        public Request(byte[] receiveBuffer, int byteReceived)
        {
            Headers = new Dictionary<string, string>();

            string requestString = Encoding.UTF8.GetString(receiveBuffer, 0, byteReceived);
            int end = requestString.IndexOf("\r\n");
            string[] startLine = requestString.Substring(0, end).Split(' ');

            Method = startLine[0].Trim();
            Uri = startLine[1].Trim();
            string[] startLine_protocol = startLine[2].Split('/');
            ProtocolName = startLine_protocol[0].Trim();
            ProtocolVersion = startLine_protocol[1].Trim();

            int endIndex;
            for (int i = requestString.IndexOf("\r\n", 0) + 2; i < requestString.Length; i = endIndex + 2)
            {
                endIndex = requestString.IndexOf("\r\n", i);
                string field = requestString.Substring(i, endIndex - i);

                int separatorIndex = field.IndexOf(':');

                if (separatorIndex > 0)
                {
                    string key = field.Substring(0, separatorIndex).Trim();
                    string value = field.Substring(separatorIndex + 1, field.Length - separatorIndex - 1).Trim();
                    if (key != null && !string.IsNullOrEmpty(key))
                        Headers.Add(key.ToLower(), value);
                }
            }
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Method)) ? InnerText : $"{Method} {Uri} {ProtocolName}/{ProtocolVersion}";
        }
    }

}

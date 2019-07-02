using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class Responce
    {
        public Request Request { get; }
        public HttpStatusCode Status { get; set; }
        public Dictionary<string, string> Headers { get; }

        public Responce(Request request)
        {
            Request = request;
            Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"{Request.ProtocolName}/{Request.ProtocolVersion} {(int)Status} {Status}";
        }

        public string GetHeadersString()
        {
            string result = string.Empty;

            foreach (var header in Headers)
                result += header.Key + ": " + header.Value + "\r\n";

            return result;
        }
    }

    class MIMEType
    {
        public string Type = "*";
        public string SubType = "*";

        public MIMEType(string type, string subType)
        {
            Type = type;
            SubType = subType;
        }

        public MIMEType(string extensionFile)
        {
            MIMEType mime = GetMIMEType(extensionFile);

            Type = mime.Type;
            SubType = mime.SubType;
        }

        public override string ToString()
        {
            return Type + "/" + SubType;
        }

        private static MIMEType GetMIMEType(string extension)
        {
            MIMEType mimeType;

            switch (extension)
            {
                // MIME Types: Image Files
                case "ico":     // Icon
                    mimeType = new MIMEType("image", "x-icon");
                    break;
                case "bmp":     // Bitmap
                    mimeType = new MIMEType("image", "bmp");
                    break;
                case "png":     // Portable Network Graphics
                    mimeType = new MIMEType("image", "png");
                    break;

                // MIME Types: Text Files
                case "html":    // HTML file
                case "htm":     // HTML file
                case "stm":     // Exchange streaming media file	
                    mimeType = new MIMEType("text", "html");
                    break;
                case "css":     // Cascading Style Sheet
                    mimeType = new MIMEType("text", "css");
                    break;
                case "bas":     // BASIC source code file
                case "c":       // C/C++ source code file
                case "h":       // C/C++/Objective C header file
                case "txt":     // Text file
                    mimeType = new MIMEType("text", "plain");
                    break;
                default:
                    {
                        if (extension.Length > 0)
                            mimeType = new MIMEType("application", extension);
                        else
                            mimeType = new MIMEType("application", "unknown");
                        break;
                    }
            }

            return mimeType;
        }
    }
}

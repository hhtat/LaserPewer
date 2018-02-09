using System.Collections.Generic;
using System.IO;

namespace LaserPewer.Grbl
{
    public class GrblProgram
    {
        private readonly List<string> _lines;
        public IReadOnlyList<string> Lines { get { return _lines; } }

        public bool EndOfProgram { get { return CurrentLine >= Lines.Count; } }

        private int _currentLine;
        public int CurrentLine
        {
            get { return _currentLine; }
            private set
            {
                _currentLine = value;
                ProgressUpdated = true;
            }
        }

        private bool _errorsDetected;
        public bool ErrorsDetected
        {
            get { return _errorsDetected; }
            private set
            {
                _errorsDetected = value;
                ProgressUpdated = true;
            }
        }

        public bool ProgressUpdated { get; private set; }

        private int nextLine;
        private readonly Queue<GrblRequest> pendingRequests;

        public GrblProgram(string code)
        {
            _lines = new List<string>();
            StringReader reader = new StringReader(code);
            for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                _lines.Add(line);
            }

            pendingRequests = new Queue<GrblRequest>();
        }

        public void ClearProgressUpdated()
        {
            ProgressUpdated = false;
        }

        public void Poll(GrblConnection connection)
        {
            sendLines(connection);
            checkRequests();
        }

        private void sendLines(GrblConnection connection)
        {
            while (nextLine < Lines.Count)
            {
                GrblRequest request = GrblRequest.CreateGCodeRequest(Lines[nextLine], nextLine);
                if (!connection.Send(request)) break;
                if (request.ResponseStatus == GrblResponseStatus.Failure) break;

                pendingRequests.Enqueue(request);
                nextLine++;
            }
        }

        private void checkRequests()
        {
            while (pendingRequests.Count > 0)
            {
                if (pendingRequests.Peek().ResponseStatus == GrblResponseStatus.Pending) break;

                GrblRequest request = pendingRequests.Dequeue();
                CurrentLine = request.LineNumber + 1;
                if (request.ResponseStatus != GrblResponseStatus.Ok &&
                    request.ResponseStatus != GrblResponseStatus.Silent)
                {
                    ErrorsDetected = true;
                }
            }
        }
    }
}

using System;

namespace LaserPewer.Grbl
{
    public class GrblRequest
    {
        public readonly GrblRequestType Type;
        public readonly bool FireAndForget;
        public readonly string Message;
        public readonly int LineNumber;


        public GrblResponseStatus ResponseStatus { get; private set; }
        public int ResponseErrorCode { get; private set; }

        protected GrblRequest(GrblRequestType type, bool fireAndForget, string message, int lineNumber = -1)
        {
            Type = type;
            FireAndForget = fireAndForget;
            Message = message;
            LineNumber = lineNumber;

            ResponseStatus = GrblResponseStatus.Unsent;
            ResponseErrorCode = 0;
        }

        public void Sent()
        {
            if (ResponseStatus != GrblResponseStatus.Unsent)
            {
                throw new InvalidOperationException();
            }

            ResponseStatus = GrblResponseStatus.Pending;
        }

        public void Complete(GrblResponseStatus status, int errorCode = 0)
        {
            if (ResponseStatus != GrblResponseStatus.Pending)
            {
                throw new InvalidOperationException();
            }

            ResponseStatus = status;
            ResponseErrorCode = errorCode;
        }

        public static GrblRequest CreateKillAlarmRequest()
        {
            return new GrblRequest(GrblRequestType.KillAlarm, false, "$X\r");
        }

        public static GrblRequest CreateHomingRequest()
        {
            return new GrblRequest(GrblRequestType.Homing, false, "$H\r");
        }

        public static GrblRequest CreateJoggingRequest(string line)
        {
            return new GrblRequest(GrblRequestType.Jogging, false, "$J=" + line + "\r");
        }

        public static GrblRequest CreateSoftResetRequest()
        {
            return new GrblRequest(GrblRequestType.SoftReset, true, "\u0018");
        }

        public static GrblRequest CreateStatusQueryRequest()
        {
            return new GrblRequest(GrblRequestType.StatusQuery, false, "?");
        }

        public static GrblRequest CreateCycleResumeRequest()
        {
            return new GrblRequest(GrblRequestType.CycleResume, true, "~");
        }

        public static GrblRequest CreateFeedHoldRequest()
        {
            return new GrblRequest(GrblRequestType.FeedHold, true, "!");
        }

        public static GrblRequest CreateGCodeRequest(string line, int lineNumber)
        {
            return new GrblRequest(GrblRequestType.FeedHold, false, line, lineNumber);
        }
    }

    public enum GrblRequestType
    {
        KillAlarm,
        Homing,
        Jogging,
        SoftReset,
        StatusQuery,
        CycleResume,
        FeedHold,
        GCode,
    }

    public enum GrblResponseStatus
    {
        Unsent,
        Pending,
        Silent,
        Ok,
        Error,
        Failure,
    }
}

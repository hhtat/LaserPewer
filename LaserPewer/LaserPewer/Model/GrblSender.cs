using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace LaserPewer.Model
{
    public class GrblSender
    {
        public event EventHandler StateChanged;

        public delegate void ProgressEventHandler(object sender, int lineAt);
        public event ProgressEventHandler Progress;

        private SenderState _state;
        public SenderState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                Application.Current.Dispatcher.Invoke(() => StateChanged?.Invoke(this, null));
            }
        }

        public SenderError Error { get; private set; }

        public int LineCount { get { return lines != null ? lines.Count : 0; } }

        private readonly GrblMachine machine;

        private readonly AutoResetEvent machineResetEvent;
        private readonly AutoResetEvent machineReadyToSendEvent;

        private List<string> lines;
        private BackgroundWorker backgroundWorker;

        public GrblSender(GrblMachine machine)
        {
            this.machine = machine;

            machineResetEvent = new AutoResetEvent(false);
            machineReadyToSendEvent = new AutoResetEvent(false);

            machine.MachineReset += Machine_MachineReset;
            machine.MachineReadyToSend += Machine_MachineReadyToSend;
        }

        private void Machine_MachineReset(object sender, EventArgs e)
        {
            machineResetEvent.Set();
        }

        private void Machine_MachineReadyToSend(object sender, EventArgs e)
        {
            machineReadyToSendEvent.Set();
        }

        public void Start(string program)
        {
            if (State != SenderState.Idle || !machine.Connected || program == null) return;

            State = SenderState.Initializing;
            Error = SenderError.None;

            lines = new List<string>();

            using (StringReader reader = new StringReader(program))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    lines.Add(line);
                }
            }

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            if (backgroundWorker != null) backgroundWorker.CancelAsync();
        }

        private void senderWork()
        {
            machineResetEvent.Reset();

            machine.Reset();
            if (machineResetEvent.WaitOne(5000))
            {
                try
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        while (true)
                        {
                            if (machineResetEvent.WaitOne(0)) { Error = SenderError.MachineReset; break; }
                            if (!machine.Connected) { Error = SenderError.ConnectionBroken; break; }
                            if (backgroundWorker.CancellationPending) { Error = SenderError.Aborted; break; }
                            // TODO check for "error:"

                            GrblMachine.SendResult result = machine.SendGCode(lines[i]);
                            if (result == GrblMachine.SendResult.Sent) { Progress?.Invoke(this, i + 1); break; }
                            if (result == GrblMachine.SendResult.Failed) { Error = SenderError.ConnectionBroken; break; }
                            if (result == GrblMachine.SendResult.Retry) Thread.Sleep(1);
                            else throw new NotSupportedException();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Error = SenderError.Unknown;
                }

                if (machine.Connected)
                {
                    for (int i = 0; i < 5 * 10; i++)
                    {
                        machine.Reset();
                        if (machineResetEvent.WaitOne(0)) break;
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                Error = SenderError.TimedOut;
            }

            State = SenderState.Idle;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            senderWork();
        }

        public enum SenderState
        {
            Idle,
            Initializing,
            Sending,
        }

        public enum SenderError
        {
            None,
            Unknown,
            Aborted,
            ConnectionBroken,
            MachineReset,
            TimedOut,
        }
    }
}

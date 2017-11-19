// <copyright file="Program.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System.Threading;
    using log4net;

    /// <summary>
    /// Main class for the assembly, contains entry point.
    /// </summary>
    public class Program
    {
/*/ Fields /*/
        private static readonly ILog Log;
        private static readonly JuvoClient Juvo;
        private static readonly ManualResetEvent ResetEvent;

/*/ Constructors /*/
        static Program()
        {
            Log = LogManager.GetLogger(typeof(Program));
            ResetEvent = new ManualResetEvent(false);
            Juvo = new JuvoClient(ResetEvent);
        }

/*/ Methods /*/
        private static void Main(string[] args)
        {
            Log.Info("Attempting to launch Juvo...");
            Juvo.Run().Wait();

            WaitHandle.WaitAll(new[] { ResetEvent });
        }
    }
}
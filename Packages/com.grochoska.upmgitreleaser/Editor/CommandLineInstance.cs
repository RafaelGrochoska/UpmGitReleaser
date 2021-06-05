using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Editor
{
    public class CommandLineInstance : IDisposable
    {
        private readonly Process _process = null;

        public CommandLineInstance(string program)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(Application.dataPath, "..")
                }
            };
        }

        public string RunCommand(string args)
        {
            _process.StartInfo.Arguments = args;
            _process.Start();

            var output = _process.StandardOutput.ReadToEnd().Trim();
            _process.WaitForExit();

            return output;
        }


        public void Dispose()
        {
            _process.Dispose();
        }
    }
}
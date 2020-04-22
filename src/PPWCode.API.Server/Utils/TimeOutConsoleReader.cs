// Copyright 2020 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace PPWCode.Server.Core.Utils
{
    public static class TimeOutConsoleReader
    {
        private static readonly AutoResetEvent _getInput;
        private static readonly AutoResetEvent _gotInput;
        private static string _input;

        static TimeOutConsoleReader()
        {
            _getInput = new AutoResetEvent(false);
            _gotInput = new AutoResetEvent(false);
            Thread inputThread = new Thread(DoReadLine) { IsBackground = true };
            inputThread.Start();
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns", Justification = "Should wait for user input")]
        private static void DoReadLine()
        {
            while (true)
            {
                _getInput.WaitOne();
                _input = Console.ReadLine();
                _gotInput.Set();
            }
        }

        public static string ReadLine(int timeOutMillisecs)
        {
            _getInput.Set();
            bool success = _gotInput.WaitOne(timeOutMillisecs);
            if (success)
            {
                return _input;
            }

            throw new TimeoutException("User did not provide input within the time limit.");
        }
    }
}

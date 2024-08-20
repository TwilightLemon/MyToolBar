using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MyToolBar
{
    internal class EntryPoint
    {
        static Mutex? _appMutex;

        static bool IsApplicationAlreadyStarted()
        {
            _appMutex = new Mutex(false, Assembly.GetExecutingAssembly().GetName().Name, out bool firstInstant);

            return !firstInstant;
        }

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            if (IsApplicationAlreadyStarted())
            {
                return;
            }
            var app = new App();
            app.Run();
        }
    }
}

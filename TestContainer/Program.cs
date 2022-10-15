using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Consola;
using Consola.Test;

namespace MidiGUI.Test.Container
{
    static class Program
    {
        private static TestResults isTestrun = TestResults.NONE;
        private static string testcase = string.Empty;
        private static Runner<Form1,MidiGUIControls> testrunner = null;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main( string[] args )
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<string> Args = new List<string>( args );
            if ( Args.Contains("--testrun") ) {
                Application.ApplicationExit += Application_ApplicationExit;
                
                isTestrun = TestResults.TextOutput;
                if( Args.Contains("--verbose") || Args.Contains("-v") )
                    isTestrun |= TestResults.Verbose;
                if( Args.Contains("--xmllogs") || Args.Contains("-x") )
                    isTestrun |= TestResults.XmlOutput;
                if( Args.Contains("--testcase") ) {
                    int casearg = Args.IndexOf( "--testcase" );
                    if( casearg < Args.Count-1 ) {
                        testcase = Args[casearg+1];
                    }
                }
                StdStream.Init( 
                    CreationFlags.TryConsole 
                   |CreationFlags.CreateLog
                   |CreationFlags.NoInputLog
                );
                Form1 window = new Form1();
                window.Paint += Window_Shown;
                Application.Run( window );
            } else {
                Application.Run( new Form1() );
            }

            int returnvalue = 0;
            if( isTestrun != TestResults.NONE ) {
                Suite<Form1> test = testrunner.GetResult();
                returnvalue = test.wasErrors() ? -1
                            : test.getFailures();
            } return returnvalue;
        }

        private static void Window_Shown( object sender, PaintEventArgs e )
        {            
            Form1 window = sender as Form1;
            window.Paint -= Window_Shown;
            testrunner = new Runner<Form1,MidiGUIControls>( new MidiGUIControls(window,isTestrun,testcase) );
            testrunner.Start();
        }

        private static void Application_ApplicationExit( object sender, EventArgs e )
        {
            Consola.Test.Test test = testrunner.GetResult();
            Consola.StdStream.Out.WriteLine("...done ... test {0}", test.Results);
            Consola.StdStream.Out.closeLog();
            Consola.StdStream.Aux.Xml.closeLog();
        }
    }
}

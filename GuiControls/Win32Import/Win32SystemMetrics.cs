#if DEBUG
#define LOG_ALL
#else
#define RELEASE
#endif

using System;
using System.Runtime.InteropServices;

#if USE_WITH_WF
using Point = System.Drawing.Point;
#elif USE_WITH_WPF
using Point = System.Windows.Point;
#endif

namespace Win32Imports
{
    public class SystemMetrics
    {
        public enum PARAMETER_QUERY : int
        {
            SPI_GETMOUSE = 0x03,
            SPI_SETMOUSE = 0x04,
            SPI_GETDOUBLECLICKTIME = 0x1F,
            SPI_SETDOUBLECLICKTIME = 0x20
        };

        public enum METRICS_REQUEST : int
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
            SM_CYCAPTION = 4,
            SM_CXCURSOR = 13,
            SM_CYCURSOR = 14,
            SM_CXFULLSCREEN = 16,
            SM_CYFULLSCREEN = 17,

            SM_MOUSEPRESENT = 19,
            SM_SWAPBUTTON = 23,

            SM_CXMIN = 28,
            SM_CYMIN = 29,
            SM_CXSIZE = 30,
            SM_CYSIZE = 31,
            SM_CXFRAME = 32,
            SM_CYFRAME = 33,
            SM_CXMINTRACK = 34,
            SM_CYMINTRACK = 35,
            SM_CXDOUBLECLK = 36,
            SM_CYDOUBLECLK = 37,
            SM_PENWINDOWS = 41,
            SM_DBCSENABLED = 42,

            SM_CMOUSEBUTTONS = 43,
            SM_MOUSEWHEELPRESENT = 75,

            SM_XVIRTUALSCREEN = 76,
            SM_YVIRTUALSCREEN = 77,
            SM_CXVIRTUALSCREEN = 78,
            SM_CYVIRTUALSCREEN = 79,
            SM_CMONITORS = 80,

            SM_MOUSEHORIZONTALWHEELPRESENT = 91,
            SM_DIGITIZER = 94,
            SM_MAXIMUMTOUCHES = 95,
        };

        [Flags()]
        public enum DIGITIZER_FLAGS : byte
        {
            INTEGRATED_TOUCHINPUT = 1,
            EXTERNAL_TOUCHINPUT = 1 << 1,
            INTEGRATED_PEN = 1 << 2,
            EXTERNAL_PEN = 1 << 3,
            MULTIPOINTER = 1 << 6,
            STACK_READY = 1 << 7
        };

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        private static extern RETURN_CODE GetSystemMetrics(METRICS_REQUEST which);     //return int

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern bool SystemParametersInfo( PARAMETER_QUERY which, UInt32 uiParam,
                                                         ref uint pResult, UInt32 fWinIni );

        [DllImport("user32.dll", EntryPoint = "GetDoubleClickTime")]
        private static extern RETURN_CODE GetDoubleClickTime();

        [DllImport("user32.dll", EntryPoint = "GetLastError")]
        private static extern RETURN_CODE GetLastError();


        public static RETURN_CODE Get(METRICS_REQUEST whichOne)
        {
#if LOG_ALL
            return GetSystemMetrics(whichOne).log(whichOne.ToString());
#else
            return GetSystemMetrics( whichOne ).logError( whichOne.ToString() );
#endif
        }

        public static DIGITIZER_FLAGS DIGITIZR {
            get { return (DIGITIZER_FLAGS)(Get(METRICS_REQUEST.SM_DIGITIZER).u32 % 256); }
        }
        public static Point DOUBLECLKSIZE {
            get { return new Point((int)Get(METRICS_REQUEST.SM_CXDOUBLECLK).u32,
                                   (int)Get(METRICS_REQUEST.SM_CYDOUBLECLK).u32);
            }
        }
        /*
        public static int DOUBLECLICKTIME {
            get { uint dblClickTime = 0;
                if (!SystemParametersInfo(PARAMETER_QUERY.SPI_GETDOUBLECLICKTIME, 0, ref dblClickTime, 0))
                    GetLastError().log("DOUBLECLICKTIME");
                return (int)dblClickTime;
            }
        }
        */
        public static bool MOUSEWHEELPRESENT {
            get { return Get(METRICS_REQUEST.SM_MOUSEWHEELPRESENT).u32 > 0;
            }
        }
        public static bool MOUSEHORIZONTALWHEELPRESENT {
            get { return Get(METRICS_REQUEST.SM_MOUSEHORIZONTALWHEELPRESENT).u32 > 0;
            }
        }
        public static int MOUSEBUTTONS {
            get { return (int)Get(METRICS_REQUEST.SM_CMOUSEBUTTONS).u32;
            }
        }
        public static int MAXIMUMTOUCHES {
            get { return (int)Get(METRICS_REQUEST.SM_MAXIMUMTOUCHES).u32;
            }
        }
        public static int PENWINDOWS {
            get { return (int)Get(METRICS_REQUEST.SM_PENWINDOWS).u32;
            }
        }
        public static bool MULTIPOINTER {
            get { return DIGITIZR.HasFlag(DIGITIZER_FLAGS.MULTIPOINTER);
            }
        }
        public static Point SCREEN {
            get { return new Point((int)Get(METRICS_REQUEST.SM_CXSCREEN).u32,
                                   (int)Get(METRICS_REQUEST.SM_CYSCREEN).u32);
            }
        }
        public static Point FULLSCREEN {
            get { return new Point((int)Get(METRICS_REQUEST.SM_CXFULLSCREEN).u32,
                                   (int)Get(METRICS_REQUEST.SM_CYFULLSCREEN).u32);
            }
        }
        public static Point SCREENSCALE {
            get { Point t = SCREEN;
                t.X = (int)(Get(METRICS_REQUEST.SM_CXVIRTUALSCREEN).u32 / t.X);
                t.Y = (int)(Get(METRICS_REQUEST.SM_CYVIRTUALSCREEN).u32 / t.Y);
              return t;
            }
        }
        public static Point SCALEDSCREEN {
            get { return new Point((int)Get(METRICS_REQUEST.SM_CXVIRTUALSCREEN).u32,
                                   (int)Get(METRICS_REQUEST.SM_CYVIRTUALSCREEN).u32);
            }
        }
        public static Point SCALEDFULLSCREEN {
            get { Point fullscreen = FULLSCREEN;
                  Point scale = SCREENSCALE;
                  fullscreen.X *= scale.X;
                  fullscreen.Y *= scale.Y;
               return fullscreen;
            }
        }



    }
}


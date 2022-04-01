using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Stepflow.Gui;
using System.Windows.Input;
#if   USE_WITH_WF
using Point = System.Drawing.Point;
using Size  = System.Drawing.Size;
using Rect  = System.Drawing.Rectangle;
#elif USE_WITH_WPF
using Point = System.Windows.Point;
using Size  = System.Windows.Size;
using Rect  = System.Drawing.RectF;
#endif


namespace Win32Imports
{
    namespace Touch
    {
#region Constants:
        internal struct DEVICE {
            const uint PRODUCT_STRING_MAX = 520;
        }

        public enum INPUT_TYPE : uint
        {
            PT_POINTER = 0x00000001,   // Generic pointer
            PT_TOUCH = 0x00000002,   // Touch
            PT_PEN = 0x00000003,   // Pen
            PT_MOUSE = 0x00000004,   // Mouse
            PT_TOUCHPAD = 0x00000005,   // Touchpad
        }

        public enum DEVICE_TYPE : uint
        {
            INTEGRATED_PEN = 0x00000001,
            EXTERNAL_PEN = 0x00000002,
            TOUCH = 0x00000003,
            TOUCH_PAD = 0x00000004,
            MAX = 0xFFFFFFFF
        }

        [Flags]
        public enum FLAG : uint
        {
            NONE = 0x00000000,// Default
            NEW = 0x00000001,// New pointer
            INRANGE = 0x00000002,// Pointer has not departed
            INCONTACT = 0x00000004,// Pointer is in contact
            FIRSTBUTTON = 0x00000010,// Primary action
            SECONDBUTTON = 0x00000020,// Secondary action
            THIRDBUTTON = 0x00000040,// Third button
            FOURTHBUTTON = 0x00000080,// Fourth button
            FIFTHBUTTON = 0x00000100,// Fifth button
            PRIMARY = 0x00002000,// Pointer is primary
            CONFIDENCE = 0x00004000,// Pointer is considered unlikely to be accidental
            CANCELED = 0x00008000,// Pointer is departing in an abnormal manner
            DOWN = 0x00010000,// Pointer transitioned to down state (made contact)
            UPDATE = 0x00020000,// Pointer update
            UP = 0x00040000,// Pointer transitioned from down state (broke contact)
            WHEEL = 0x00080000,// Vertical wheel
            HWHEEL = 0x00100000,// Horizontal wheel
            CAPTURECHANGED = 0x00200000,// Lost capture
            HASTRANSFORM = 0x00400000 // Input has a transform associated with it
        }

        public enum BUTTON_CHANGE_TYPE : uint
        {
            NONE,
            FIRSTBUTTON_DOWN,
            FIRSTBUTTON_UP,
            SECONDBUTTON_DOWN,
            SECONDBUTTON_UP,
            THIRDBUTTON_DOWN,
            THIRDBUTTON_UP,
            FOURTHBUTTON_DOWN,
            FOURTHBUTTON_UP,
            FIFTHBUTTON_DOWN,
            FIFTHBUTTON_UP,
        }

        public enum DEVICE_CURSOR_TYPE : uint
        {
            POINTER_DEVICE_CURSOR_TYPE_UNKNOWN = 0x00000000,
            POINTER_DEVICE_CURSOR_TYPE_TIP = 0x00000001,
            POINTER_DEVICE_CURSOR_TYPE_ERASER = 0x00000002,
            POINTER_DEVICE_CURSOR_TYPE_MAX = 0xFFFFFFFF
        }

        public enum TOUCH_FLAGS : uint
        { TOUCH_FLAG_NONE = 0x00000000 }

        public enum TOUCH_MASK : uint
        {
            TOUCH_MASK_NONE = 0x00000000, // Default - none of the optional fields are valid
            TOUCH_MASK_CONTACTAREA = 0x00000001, // The rcContact field is valid
            TOUCH_MASK_ORIENTATION = 0x00000002, // The orientation field is valid
            TOUCH_MASK_PRESSURE = 0x00000004 // The pressure field is valid
        }

#if WIN64
        public enum TOUCH_REGISTRATION_FLAGS : ulong {
#else
        public enum TOUCH_REGISTRATION_FLAGS : uint {
#endif
            NONE = 0,
            FINETOUCH = (0x00000001),
            WANTPALM =  (0x00000002)
        }

        [Flags()]
        public enum PEAK_MESSAGE_FLAGS
        {
            PM_NOREMOVE = 0x0000,
            PM_REMOVE = 0x0001,
            PM_NOYIELD = 0x0002
        }

        public enum WM_POINTER_MESSAGE_TYPE : ushort
        {
            WM_POINTERDEVICECHANGE       = 0x0238,
            WM_POINTERDEVICEINRANGE      = 0x0239,
            WM_POINTERDEVICEOUTOFRANGE   = 0x023A,
            WM_TOUCH                     = 0x0240,
            WM_NCPOINTERUPDATE           = 0x0241,
            WM_NCPOINTERDOWN             = 0x0242,
            WM_NCPOINTERUP               = 0x0243,
            WM_POINTERUPDATE             = 0x0245,
            WM_POINTERDOWN               = 0x0246,
            WM_POINTERUP                 = 0x0247,
            WM_POINTERENTER              = 0x0249,
            WM_POINTERLEAVE              = 0x024A,
            WM_POINTERACTIVATE           = 0x024B,
            WM_POINTERCAPTURECHANGED     = 0x024C,
            WM_TOUCHHITTESTING           = 0x024D,
            WM_POINTERWHEEL              = 0x024E,
            WM_POINTERHWHEEL             = 0x024F,
            DM_POINTERHITTEST            = 0x0250,
            WM_POINTERROUTEDTO           = 0x0251,
            WM_POINTERROUTEDAWAY         = 0x0252,
            WM_POINTERROUTEDRELEASED     = 0x0253,

            MASK_LIFTMESSAGEES = 0x0007|0x000A
        }


        #endregion

        #region Structures:

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct RECT
        {
            public static readonly RECT ZERO = new RECT(0, 0, 0, 0);
            public static readonly RECT EMPTY = new RECT(-1, -1, -1, -1);
            [FieldOffset(0)]
            public int left;
            [FieldOffset(4)]
            public int top;
            [FieldOffset(8)]
            public int right;
            [FieldOffset(12)]
            public int bottom;

            public Point64 corner
            {
                get { return new Point64(left, top); }
                set
                {
                    right += (value.x - left); left = value.x;
                    bottom += (value.y - top); top = value.y;
                }
            }
            public Point32 scales
            {
                get { return new Point32((right - left) / 2, (bottom - top) / 2); }
                set
                {
                    Point64 c = center; corner = c - value;
                    right = c.x + value.x; bottom = c.y + value.y;
                }
            }
            public Point32 length
            {
                get { return new Point32(right - left, bottom - top); }
                set { right = left + value.x; bottom = top + value.y; }
            }
            public Point64 center
            {
                get { return corner + scales; }
                set
                {
                    Point64 s = scales; corner = value - s;
                    right = value.x + s.x; bottom = value.y + s.y;
                }
            }

            public RECT(int x, int y, int w, int h)
            {
                left = x; top = y; right = x + w; bottom = y + h;
            }
            public RECT(Rect rect)
            {
                left = rect.Left; right = rect.Right; top = rect.Top; bottom = rect.Bottom;
            }
            public override string ToString()
            {
                return corner.ToString() + string.Format(
                    " w:{0} h:{1}", right - left, bottom - top
                );
            }
            public static implicit operator Rect(RECT cast)
            {
                return new Rect(cast.corner, cast.length);
            }
            public bool Contains(Point location)
            {
                return ((Rect)this).Contains(location);
            }
        }


#if WIN64
        [StructLayout(LayoutKind.Sequential, Size = 92)]
#else
        [StructLayout(LayoutKind.Sequential, Size = 84)]
#endif
        public struct INFO
        {
            public INPUT_TYPE pointerType;
            public uint pointerId;
            public uint frameId;
            public FLAG pointerFlags;
            public IntPtr sourceDevice;			  //handle
            public IntPtr hwndTarget;				  //hwnd
            public Point64 ptPixelLocation;		  //POINT
            public Point64 ptHimetricLocation;		  //POINT
            public Point64 ptPixelLocationRaw;		  //POINT
            public Point64 ptHimetricLocationRaw;	  //POINT
            public uint dwTime;
            public uint historyCount;
            public int InputData;
            public uint dwKeyStates;
            public ulong PerformanceCount;
            public BUTTON_CHANGE_TYPE ButtonChangeType;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct TOUCHINFO
        {
            public INFO pointerInfo;
            public TOUCH_FLAGS touchFlags;
            public TOUCH_MASK touchMask;
            public RECT rcContact;
            public RECT rcContactRaw;
            public UInt32 orientation;
            public UInt32 pressure;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct TOUCHINPUT
        {
            public Int64 x;
            public Int64 y;
            public UIntPtr hSource;
            public UInt32 dwID;
            public UInt32 dwFlags;
            public UInt32 dwMask;
            public UInt32 dwTime;
            public UIntPtr dwExtraInfo; // ULONG_PTR
            public UInt32 cxContact;
            public UInt32 cyContact;
        };


#if WIN64
        [StructLayout(LayoutKind.Sequential, Size = 1080)]
#else
        [StructLayout(LayoutKind.Sequential,Size = 34+1040)]
#endif
        public unsafe struct DEVICE_INFO
        {
            public System.Windows.Forms.Orientation displayOrientation;
#if WIN64
            public UIntPtr device; // device HANDLE
            public DEVICE_TYPE pointerDeviceType;
            public UIntPtr monitor; // display-device HANDLE
#else         
            public UIntPtr device; // device HANDLE
            public DEVICE_TYPE pointerDeviceType;
            public UIntPtr monitor; // display-device HANDLE
#endif
            //            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char startingCursorId_1;
            public char startingCursorId_2;
            public char startingCursorId_3;
            public char startingCursorId_4;
            public UInt16 maxActiveContacts;  //x86-26/x64-34/ alignment? 32/40
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1040)] //+1040 byte
            public byte[] productString;
        };

/*
#if WIN64
        [StructLayout(LayoutKind.Explicit, Size = 1070)]
#else
        [StructLayout(LayoutKind.Explicit, Size = 1062)]
#endif
        public unsafe struct DEVICE_INFO
        {
            [FieldOffset(0)]
            public System.Windows.Forms.Orientation displayOrientation;
#if WIN64
            [FieldOffset(4)]
            public UIntPtr device; // device HANDLE
            [FieldOffset(12)]
            public DEVICE_TYPE pointerDeviceType;
            [FieldOffset(16)]
            public UIntPtr monitor; // display-device HANDLE
            [FieldOffset(24)]
            public char startingCursorId_1;
            [FieldOffset(25)]
            public char startingCursorId_2;
            [FieldOffset(26)]
            public char startingCursorId_3;
            [FieldOffset(27)]
            public char startingCursorId_4;
            [FieldOffset(28)]
            public UInt16 maxActiveContacts;  //x86-26/x64-34/ alignment? 32/40
            [FieldOffset(30)]
            public fixed byte productString[1040];
#else         
            [FieldOffset(4)]
            public UIntPtr device; // device HANDLE
            [FieldOffset(8)]
            public DEVICE_TYPE pointerDeviceType;
            [FieldOffset(12)]
            public UIntPtr monitor; // display-device HANDLE
            [FieldOffset(16)]
            public char startingCursorId_1;
            [FieldOffset(17)]
            public char startingCursorId_2;
            [FieldOffset(18)]
            public char startingCursorId_3;
            [FieldOffset(19)]
            public char startingCursorId_4;
            [FieldOffset(20)]
            public UInt16 maxActiveContacts;  //x86-26/x64-34/ alignment? 32/40
            [FieldOffset(22)]
            public fixed byte productString[1040];
#endif
        };
*/
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVICE_PROPERTY
        {
            public Int32 logicalMin;
            public Int32 logicalMax;
            public Int32 physicalMin;
            public Int32 physicalMax;
            public UInt32 unit;
            public UInt32 unitExponent;
            public UInt16 usagePageId;
            public UInt16 usageId;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVICE_CURSOR_INFO
        {
            public UInt32 cursorId;
            public DEVICE_CURSOR_TYPE cursor;
        };

#if WIN64
        [StructLayout(LayoutKind.Explicit,Size = 56)]
        public struct POINTER_MESSAGE
        {
            [FieldOffset(0)]
            public IntPtr hwnd;
            [FieldOffset(8)]
            public WM_POINTER_MESSAGE_TYPE message;
            [FieldOffset(16)]
            public UInt32 wParam;
            [FieldOffset(24)]
            public UInt32 lParam;
            [FieldOffset(32)]
            public UInt32 time;
            [FieldOffset(40)]
            public Point64 pt;
            [FieldOffset(48)]
            public UInt32 lPrivate;
#else
        [StructLayout(LayoutKind.Explicit,Size =32)]
        public struct POINTER_MESSAGE
        {
            [FieldOffset(0)]
            public IntPtr hwnd;
            [FieldOffset(4)]
            public WM_POINTER_MESSAGE_TYPE message;
            [FieldOffset(8)]
            public UInt32 wParam;
            [FieldOffset(12)]
            public UInt32 lParam;
            [FieldOffset(16)]
            public UInt32 time;
            [FieldOffset(20)]
            public Point64 pt;
            [FieldOffset(28)]
            public UInt32 lPrivate;
#endif
            public static implicit operator POINTER_MESSAGE( System.Windows.Forms.Message cast )
            {
                unsafe { return *(POINTER_MESSAGE*)&cast; }
            }
            public static implicit operator System.Windows.Forms.Message( POINTER_MESSAGE cast )
            {
                unsafe { return *(System.Windows.Forms.Message*)&cast; }
            }
        };

        #endregion

#region Helpers:
        [StructLayout(LayoutKind.Explicit,Size=16)]   
        unsafe struct PtrActTyp {
            public static readonly PtrActTyp FourCC = new PtrActTyp(new uint[]{7237454,6579265,7628115,7169362});
            public static readonly PtrActTyp Indexe = new PtrActTyp(new uint[]{0,1,2,3}); 
            [FieldOffset(0)] public fixed uint idx[4];
            [FieldOffset(0)] public uint Non;// = 7237454;
            [FieldOffset(4)] public uint Add;// = 6579265;
            [FieldOffset(8)] public uint Set;// = 7628115;
           [FieldOffset(12)] public uint Rem;// = 7169362;
            internal PtrActTyp(uint[] init)
            {
                Non = init[0];
                Add = init[1];
                Set = init[2];
                Rem = init[3];
            }
        }

            [StructLayout(LayoutKind.Explicit,Size=8)]
            internal struct PointerAction {   
                public const ushort Non = 0;
                public const ushort Add = 1;
                public const ushort Set = 2;
                public const ushort Rem = 3;
                
                [FieldOffset(0)]
                private uint wpm;
                [FieldOffset(0)]
                public ushort pid;
                [FieldOffset(2)]
                public ushort typ;
                [FieldOffset(4)]
                private uint lpm;
                [FieldOffset(4)]
                public Point32 pos;

                public PointerAction( ref POINTER_MESSAGE msg ) : this() {
                    wpm = msg.wParam; lpm = msg.lParam;
                    typ =( msg.message == WM_POINTER_MESSAGE_TYPE.WM_POINTERLEAVE
                        || msg.message == WM_POINTER_MESSAGE_TYPE.WM_POINTERUP
                        || (int)msg.message >= 0x250 ) 
                         ? Rem : Non;
                }
                public override string ToString() {
                unsafe {
                    PtrActTyp ptrtyp = PtrActTyp.FourCC;
                    return new string( (sbyte*)&ptrtyp.idx[typ] );
                }}

                public static implicit operator ushort( PointerAction cast ) {
                    return cast.typ;
                }
            }
            
            public class MessageFilter : System.Windows.Forms.IMessageFilter
            {
                public delegate void MessageReceiver( ref System.Windows.Forms.Message msg );
                private MessageReceiver receiver = null;
                public bool PreFilterMessage( ref System.Windows.Forms.Message msg ) {
                    if( msg.Msg >= 0x238 && msg.Msg <= 0x24a ) {
                        receiver?.Invoke( ref msg );
                        return true;
                    } return false;
                }
                public MessageFilter( MessageReceiver recipient ) {
                    receiver = recipient;
                }
            }
            
            public struct PointerHandle {
                public int     number;
                public IntPtr  window;
                public UIntPtr handle;

                public PointerHandle( IntPtr wnd ) {
                    handle = UIntPtr.Zero;
                    number = -1;
                    window = wnd;
                }
                public PointerHandle( int dev, IntPtr wnd ) {
                    handle = UIntPtr.Zero;
                    number = dev;
                    window = wnd;
                }
                public static bool operator ==( PointerHandle This, PointerHandle That ) {
                    return This.number == That.number && This.window == That.window;
                }
                public static bool operator !=( PointerHandle This, PointerHandle That ) {
                    return This.number != That.number || This.window != That.window;
                }
                public static implicit operator bool( PointerHandle cast ) {
                    return cast.handle != UIntPtr.Zero && cast.window != IntPtr.Zero && cast.number != -1;
                }
                public static implicit operator PointerHandle( IntPtr cast ) {
                    return new PointerHandle(cast);
                }
                public override bool Equals( object obj )
                {
                    if( obj is PointerHandle )
                        return this == (PointerHandle)obj;
                    else return false;
                }
                public override int GetHashCode()
                {
                    unsafe { return (int)((long)window.ToPointer() / (number+2)); } 
                }
            }

            public class InitializationEventArgs : EventArgs
            {
                public readonly PointerHandle Device;
                public InitializationEventArgs( ref PointerHandle value )
                {
                    Device = value;
                }
                public static implicit operator PointerHandle( InitializationEventArgs cast )
                {
                    return cast.Device;
                }
            }
        #endregion

        #region ImportWrapper:
        public class Wrapper
        {
            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            GetMessage(ref POINTER_MESSAGE getter, IntPtr hWnd, WM_POINTER_MESSAGE_TYPE wMsgFilterMin, WM_POINTER_MESSAGE_TYPE wMsgFilterMax);

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            PeekMessage(ref POINTER_MESSAGE getter, IntPtr hWnd, WM_POINTER_MESSAGE_TYPE wMsgFilterMin, WM_POINTER_MESSAGE_TYPE wMsgFilterMax, PEAK_MESSAGE_FLAGS removeMsg );

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            DispatchMessage(ref POINTER_MESSAGE returner);

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            GetPointerDevices(out UInt32 deviceCount, [In,Out] DEVICE_INFO[] pointerDevices); // POINTER_DEVICE_INFO*

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            GetPointerDevice(UIntPtr devHandle, ref DEVICE_INFO devInfo);

            [DllImport("user32.dll")]
            protected unsafe static extern RETURN_CODE
            GetPointerDeviceProperties( UIntPtr device, ref UInt32 propertyCount, [In,Out] DEVICE_PROPERTY[] pointerProperties);

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            RegisterPointerDeviceNotifications( IntPtr hWnd, bool notifyRange);

            [DllImport("user32.dll")]
            protected unsafe static extern RETURN_CODE
            GetPointerDeviceCursors(UIntPtr device, ref UInt32 cursorCount, [In,Out] DEVICE_CURSOR_INFO[] deviceCursors);

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            GetPointerDeviceRects(UIntPtr device, out RECT pointerDeviceRect, out RECT displayRect);

            [DllImport("user32.dll")]
            protected static extern RETURN_CODE
            GetRawPointerDeviceData(UInt32 pointerId, UInt32 historyCount, UInt32 propertiesCount, [In] DEVICE_PROPERTY[] pProperties, [Out] Int64[] pValues);

            [DllImport("User32.dll")]
            protected static extern RETURN_CODE
            GetPointerType(uint pointerId, out INPUT_TYPE pointerType);

            [DllImport("User32.dll")]
            protected static extern RETURN_CODE
            GetPointerCursorId(uint pointerId, out uint cursorId);

            [DllImport("User32.dll")]
            protected static extern RETURN_CODE
            GetPointerInfo(uint pointerId, out INFO pointerInfo);

            [DllImport("User32.dll")]
            protected static extern RETURN_CODE
            RegisterTouchWindow( IntPtr hwnd, TOUCH_REGISTRATION_FLAGS ulFlags );

            [DllImport("User32.dll")]
            protected static extern RETURN_CODE
            UnregisterTouchWindow( IntPtr hwnd );

            [DllImport("User32.dll")]
            protected static extern RETURN_CODE
            IsTouchWindow( IntPtr hwnd, out TOUCH_REGISTRATION_FLAGS pulFlags );


            internal static Dictionary<ushort,PointerAction> fingers = null;
            private static List<Wrapper>       instances = null;
            private static UInt32              deviceCount = 0;
            private DEVICE_INFO                deviceInfo;
            private string                     deviceName = "";
            private Wrapper                    handleOwner = null;
            public PointerHandle               device = IntPtr.Zero;
            private System.Windows.Forms.Form  window = null;
  
            public unsafe string DeviceName {
                get { if( handleOwner == null ) return "Still initializing statics... no handleOwner was initialized yet";
                    fixed ( byte* productString = handleOwner.deviceInfo.productString ) {
                        if ( handleOwner.deviceName.Length == 0 ) {
                            if ( productString[0] != '\0' ) {
                                System.Text.StringBuilder devnam = new System.Text.StringBuilder();
                                byte a = 0; byte b = 0; int i = 0;
                                while ( !( ((a = productString[i]) == 0) 
                                        && ((b = productString[i+1]) == 0) ) ) {
                                    if (a != 0) devnam.Append((char)a);
                                    if (b != 0) devnam.Append((char)b);
                                    if ((i += 2) >= 1040)
                                        break;
                                } handleOwner.deviceName = devnam.ToString();
                            }
                        } return handleOwner.deviceName;
                    }
                }
            }
            public DEVICE_TYPE DeviceType {
                get { if( handleOwner == null ) return DEVICE_TYPE.MAX;
                    return handleOwner.deviceInfo.pointerDeviceType;
                }
            }
            public int MaximumTouches {
                get { return handleOwner.deviceInfo.maxActiveContacts; }
            }
            
            public int NumberOfDevices {
                get { return (int)deviceCount; }
            }

            static Wrapper() {
                DEVICE_INFO[] devList = new DEVICE_INFO[16];
                GetPointerDevices( out deviceCount, devList ).logInfo(
                    string.Format( "GetPointerDevices(): {0}", deviceCount )
                );
            }
            public Wrapper( System.Windows.Forms.Form window, int deviceNum ) {
                RETURN_CODE result = true;
                fingers = new Dictionary<ushort,PointerAction>(5);
                if ( instances == null ) {
                     instances = new List<Wrapper>(1);
                     result.logInfo( "Registered for receiving messages from device: "
                               + CreateReceiverLoop( deviceNum, window.Handle ) );
                } if( handleOwner == null )
                    foreach( Wrapper pointer in instances ) {
                        if( pointer.handleOwner.device.window == window.Handle ) {
                            this.handleOwner = pointer;
                            this.device = pointer.device;
                        }
                    }
                this.window = window;
                instances.Add( this );
                result.logInfo( string.Format(
                    "Device Name: {0}\nDevice Type: {1}\n", DeviceName, DeviceType )
                              );
            }
            
            internal delegate void FingerTouchAction( PointerAction action );
            public delegate void InitializationEvent( object sender, InitializationEventArgs e );
            internal event FingerTouchAction InputReceived;
            public event InitializationEvent InitializationDone;
       
            protected System.Windows.Forms.Form ReportingWindow {
                    get { return window; }
            }

            private void DoFingerDown( ref PointerAction action )
            {
                if ( action.typ != PointerAction.Rem ) {
                     action.typ  = PointerAction.Add;
                    fingers.Add( action.pid, action );
                    InputReceived?.Invoke( action );
                }
            }
            private void DoFingerLift( ref PointerAction action )
            {
                action.typ = PointerAction.Rem;
                fingers.Remove( action.pid );
                InputReceived?.Invoke( action );
            }
            private void DoFingerMove( ref PointerAction action )
            {
                action.typ = PointerAction.Set;
                fingers[action.pid] = action;
                InputReceived?.Invoke( action );
            }

            private void inputReceiver( ref System.Windows.Forms.Message input )
            {
                //handleOwner.device.window = input.HWnd;
                RETURN_CODE result = true;
                unsafe { fixed (void* ptMsg = &input) {
                    POINTER_MESSAGE* winMsg = (POINTER_MESSAGE*)ptMsg; 
                if ( winMsg->message != 0 ) {
                #if VERBOSE_DEBUG_LOGGING
                    result.log("incomming touch message!");
                #endif
                    PointerAction act = new PointerAction( ref *winMsg );
                    if( fingers.ContainsKey( act.pid ) ) {
                        switch ( winMsg->message ) {
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERDEVICECHANGE: break;
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERUPDATE:
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERDOWN:
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERACTIVATE:
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERENTER:
                                DoFingerMove( ref act ); break;
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERUP:
                        case WM_POINTER_MESSAGE_TYPE.WM_POINTERLEAVE:
                                DoFingerLift( ref act ); break;
                    } } else DoFingerDown( ref act );
                #if VERBOSE_DEBUG_LOGGING
                    result.log(
                         string.Format( "processed message: {0} {1} {2}\ntouches down: {3}",
                         act.ToString(), act.pid.ToString(), ((Point64)act.pos).ToString(),
                        fingers.Count ) );
                #endif
                } } }
            }

            public string CreateReceiverLoop( int deviceNum, IntPtr hWnd )
            {
                PointerHandle newHandle = new PointerHandle( deviceNum, hWnd );
                if( newHandle == this.device ) {
                    if( newHandle == handleOwner.device ) {
                        return DeviceName;
                    }
                } bool allreadyOpen = false;
                foreach( Wrapper pointer in instances ) {
                    if( pointer.device == newHandle ) {
                        this.handleOwner = pointer;
                        this.device = pointer.device;
                        allreadyOpen = true;
                    }
                } if( allreadyOpen )
                    return DeviceName;
                if( newHandle.number < NumberOfDevices ) {
                    bool canStart = false;
                    DEVICE_INFO[] devList = new DEVICE_INFO[16];
                    if ( canStart = GetPointerDevices( out deviceCount, devList ) ) {
                        this.device = newHandle;
                        this.device.handle = (deviceInfo = devList[this.device.number]).device;
                        deviceName = "";
                    } if(canStart) {
                         canStart = RegisterMessageFilter( this.device.window );
                    } if(canStart) {
                        OnInitializationDone();
                        return DeviceName;
                    }                        
                }
                deviceName = "Invalid Device"; 
                device = IntPtr.Zero;
                return deviceName;
            }

            private RETURN_CODE RegisterMessageFilter( IntPtr hWnd )
            {
                RETURN_CODE result; 
                if ( result = RegisterPointerDeviceNotifications( hWnd, false ) ) {
                    if ( result = RegisterTouchWindow( hWnd, TOUCH_REGISTRATION_FLAGS.WANTPALM ) ) {
                        System.Windows.Forms.Application.AddMessageFilter( new MessageFilter( inputReceiver ) );
                        device.window = hWnd;
                        handleOwner = this;
                    }
                } return result;
            }

            protected virtual void OnInitializationDone()
            {
                if( InitializationDone != null ) InitializationDone( this, new InitializationEventArgs( ref this.device ) );
#if DEBUG
                Consola.StdStream.Out.WriteLine("Successfully registered for receiving Touch Pointer event messages!");
            }

            public void logInfo()
            {
                RETURN_CODE result;
                uint propCount = 0;
                unsafe {
                    if( result = GetPointerDeviceProperties( handleOwner.device.handle, ref propCount, null ) ) {
                        DEVICE_PROPERTY[] props = new DEVICE_PROPERTY[propCount];
                        if( result = GetPointerDeviceProperties( handleOwner.device.handle, ref propCount, props ) ) {
                            for(int i=0;i<propCount;++i ) {
                                    Consola.StdStream.Out.Stream.Put("Device ").Put(i).Put(" got properties!").End();
                            }
                        }
                    }
                }
#endif
            }

            };
#endregion

    }
}

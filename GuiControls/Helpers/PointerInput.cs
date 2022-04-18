using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
//using Windows.Devices.Input;
//using Windows.Devices.HumanInterfaceDevice;
//using Windows.Foundation;
using Win32Imports;
using Win32Imports.Touch;
using PointerAction = Win32Imports.Touch.PointerAction;
using System.Runtime.InteropServices;
using Stepflow.Gui.Helpers;
#if DEBUG
using Consola;
#endif
#if   USE_WITH_WF
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rect  = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using System.ComponentModel;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rect  = System.Windows.Int32Rect;
using RectF = System.Windows.Rect;
using MouseEventArgs = Windows.Devices.Input.MouseEventArgs;
#endif


namespace Stepflow.Gui
{
    public enum AutoRegistration {
        Disabled=0, Enabled=1 
    }

    [Flags]
    public enum IsTouching : ushort {
        // When a fingertip just arrived and at all was'nt recognized before.
        // - within this state, it's yet unclear if the tip already is moving
        //   maybe or, if it's pointing to a fixed position and not moving at all)     
        TouchDown = 0x0000,
        // This could apply for all touches not passable further on to control elements
        // which could handle these (like when the touched area actually does not implement the ITouchable interface)  
        TheScreen = 0x0001, 
        TrackKept = 0x0002,
        AnElement = 0x0004,

        // The area (or the 'group of finger tips') where an actual ongoing primar hand operation is taking place 
        Here = 0x0008,


        // abstract identifiers for 'fingertips' 

        // drop in helper for identifying a fourth finger on a second hand (where cannot be used that ThumbSub bit- due to
        // that bit would signal a FULL hand as well as also 4 fingers down. - So any 'There' location touches then can use
        // the FourthSub bit for signaling '4 fingers down There' with still existing possibility later extending the gesture
        // to a full hand if maybe later a 10'th finger follows
        FourthSub = 0x0010, 
        FirstSub  = 0x0020,
        SecondSub = 0x0040,
        ThirdSub  = 0x0080,
        ThumbSub  = 0x0100,
        
        Prime     = 0x0200,

        //unter erster hand
        SubPrime   = FirstSub|SecondSub|ThirdSub|ThumbSub,
        // bit mask which can be used fore checking if an additional new touch down is determined being additional finger of the same one hand, for modifying that hands ongoing operation 
        SubHere = SubPrime|Here, 
        
        //unter anderer hand
        There     = 0x1000,  // , ...when touches appear too far away from an already recognized group of fingers of a first hand, 
        ExaPrime  = FirstSub|There, // an additional new touch down is determined then belonging to another, independantly started operation, performed by another (the second?) hand ... where ExaPrime signals that it's about a FIRST touch down recognized 'There'  
        FirstExa  = SecondSub,
        SecondExa = ThirdSub, 
        ThirdExa  = FourthSub,
        ThumbExa  = ThumbSub,
        SubThere  = FirstExa|SecondExa|ThirdExa|ThumbExa,
        
        // flags shoud signal EMPTY as soon a fingertip lifts off from the screen. which one it was actually may be signaled via REMOVING that corresponding FingerBit
        NoMore    = 0xE000,
        LiftOff   = 0xffff
    }

    public class PointerInput 
        : Win32Imports.Touch.Wrapper
        , ITouchDispatchTrigger
    {
        public delegate void Registration (ITouchInputDispatcher ToucheInterPatsche);
        public delegate void InternalInit (PointerInput inst, Registration init);
        public delegate void OnInitialize (PointerInput inst);
        public delegate void TouchMessage (FingerTip eventarg);
         
        public event TouchMessage FingerTouchMove;
        public event TouchMessage FingerTouchLift;
        public event TouchMessage FingerTouchDown;

        private static AutoRegistration         registration = AutoRegistration.Enabled;
        public static event InternalInit        InitializationFinished;
        public static event OnInitialize        Initialized;
        public static readonly uint             ThreasholdForDownCount;
        
        public static bool isInitialized()
        {
            if( instance == null )
                return false;
            return true;
        } 

        private static PointerInput instance = null;
        public static  PointerInput Dispatcher {
            get{ return instance; }
            private set { instance = value;
                if( InitializationFinished != null ) {
                    InitializationFinished( instance, instance.RegisterAsTouchDispatcher );
                } Initialized?.Invoke( instance );
            }
        }

#if DEBUG
        private static Consola.StdStreams                std;
#endif

        private System.Diagnostics.Stopwatch           timer;  
        private Control                      translateToArea;
        private Dictionary<ushort,FingerTip>         touches;
        private HashSet<ITouchableElement>          elements;


        private void RegisterAsTouchDispatcher( ITouchInputDispatcher dispatcher )
        {
            if( dispatcher.instance<ITouchDispatchTrigger>() != this ) {
                dispatcher.input.FingerTouchDown += dispatcher.Down;
                dispatcher.input.FingerTouchMove += dispatcher.Move;
                dispatcher.input.FingerTouchLift += dispatcher.Lift;
            }
        }

        public void RegisterTouchableElement( ITouchableElement element )
        {
            elements.Add( element );
        }

        public void UnRegisterTouchableElement( ITouchableElement element )
        {
            elements.Remove( element );
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Auto-Registration related

        public static AutoRegistration  AutoRegistration {
            get { return registration; }
            set { registration = value;
                if( value == AutoRegistration.Enabled ) {
                    RegisterWithMainWindow();
                }
            }
        }

        private static void RegisterWithMainWindow()
        {
            if( PointerInput.instance == null ) {
                Application.Idle += AutoInitializingTouchInput;
            }
        }

        private static void AutoInitializingTouchInput(object sender, EventArgs e)
        {
            if ( Application.OpenForms.Count > 0 ) {
                if (PointerInput.instance == null) {
                    PointerInput t = new PointerInput( Application.OpenForms[0], Application.OpenForms[0], 0 );
                } Application.Idle -= AutoInitializingTouchInput;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////



        bool ITouchable.IsTouched {
            get{ return touches.Count > 0; }
        }

        public ISite Site {
            get { return ((IComponent)translateToArea).Site; }
            set { ((IComponent)translateToArea).Site = value; }
        }
        Rect ITouchable.Bounds {
            get { return translateToArea.Bounds; }
            set { translateToArea.Bounds = value; }
        }

        Control ITouchable.Element {
            get {return translateToArea; }
        }

        public ITouchableElement element()
        {
            return screen().instance().Element as ITouchableElement;
        }

        public ITouchDispatchTrigger screen()
        {
            return this;
        }

        public ITouchTrigger<ITouchDispatchTrigger> trigger()
        {
            return this;
        }

        ITouchable ITouchTrigger.instance<OnSide>()
        {
            return this;
        }

        ITouchDispatchTrigger ITouchTrigger<ITouchDispatchTrigger>.instance()
        {
            return screen();
        }

        static PointerInput()
        {
#if DEBUG
                std = new Consola.StdStreams( CreationFlags.NewConsole );
                std.Out.WriteLine("begin: PointerInput() Debug output:...");
                RETURN_CODE.LogAnyResult = false;
                RETURN_CODE.SetLogOutWriter( std.Out.WriteLine );
#endif
            ThreasholdForDownCount = 300;// (uint)SystemMetrics.DOUBLECLICKTIME;
        }

        public PointerInput( Control panel, Form wind, int deviceNum ) 
            : base( wind, deviceNum )
        {
            timer = new System.Diagnostics.Stopwatch();
            elements = new HashSet<ITouchableElement>();
            translateToArea = panel;
            InputReceived += PointerInput_IncommingMessage;
            touches = new Dictionary<ushort, FingerTip>( NumberOfDevices );
        }

        protected override void OnInitializationDone()
        {
            Application.Idle += IdleWhenConstructed;
            base.OnInitializationDone();
        }

        private void IdleWhenConstructed( object sender, EventArgs e )
        {
            Application.Idle -= IdleWhenConstructed;
            PointerInput.Dispatcher = this;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // dispatching of FingerTips to registered control elements

        private void DispatchFingerTip( FingerTip touch )
        {
            IEnumerator<ITouchableElement> elms = elements.GetEnumerator();
            while( elms.MoveNext() ) {
                IRectangle area = elms.Current.ScreenRectangle();
                if( area.Contains( touch.Origin ) ) {
                    if( touch.Interact( elms.Current ) ) {
                        touch.SetOffset( area.Corner );
                        break;
                    }
                }
            }
        }

        private void RemoveFingerTip( FingerTip touch )
        {
            FingerTouchLift?.Invoke( touch );
        }


        private void touches_AddNew( PointerAction newMessage )
        {
            FingerTip newTip = new FingerTip(
                newMessage, DispatchFingerTip, RemoveFingerTip, translateToArea );
            touches.Add( newMessage.pid, newTip );
            DispatchFingerTip( newTip );
        }

        private void PointerInput_IncommingMessage( PointerAction action )
        {
            switch( action.typ ) {
                case PointerAction.Add: {
                        touches_AddNew( action );
                        break;
                    }
                case PointerAction.Set: {
                        touches[action.pid].Update( action.pos );
                        break;
                    }
                case PointerAction.Rem: {
                        touches[action.pid].Remove( action.pos );
                        touches.Remove( action.pid );
                        break;
                    }
            }
        }

        void ITouchTrigger.Down( FingerTip touch )
        {
            if( FingerTouchDown == null )
                DispatchFingerTip( touch );
            else
                FingerTouchDown( touch );
        }
        void ITouchTrigger.Move( FingerTip touch )
        {
            if( FingerTouchMove == null )
                DispatchFingerTip( touch );
            else
                FingerTouchMove( touch );
        }
        void ITouchTrigger.Lift( FingerTip touch )
        {
            FingerTouchLift?.Invoke( touch );
        }


    }

    //////////////////////////////////////////////////////////////////////////////

    /*    DER WEG IST DAS ZIEL   */ 

    //////////////////////////////////////////////////////////////////////////////
    
   
}

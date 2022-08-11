using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

//using Windows.Devices.Input;
//using Windows.Devices.HumanInterfaceDevice;
//using Windows.Foundation;

using Stepflow;
using Stepflow.Gui.Helpers;

using Win32Imports;
using Stepflow.Gui.Geometry;
using PointerAction = Win32Imports.Touch.PointerAction;
using System.Runtime.InteropServices;
#if DEBUG
using Consola;
#endif
#if   USE_WITH_WF
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rect  = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using System.Drawing;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rect  = System.Windows.Int32Rect;
using RectF = System.Windows.Rect;
using MouseEventArgs = Windows.Devices.Input.MouseEventArgs;
#endif

namespace Stepflow.Gui
{

    public enum TouchEvent
    {
        Down, Move, Lift, Left, Right, Slide, Sized, Screwd
    }

    public interface ITouchable
    {
        ITouchDispatchTrigger screen();
        ITouchableElement element();
        Rect Bounds { get; set; }
        Control Element { get; }
        bool IsTouched { get; }
    }

    public interface ITouchTrigger
        : ITouchable
    {
        ITouchable instance<Purpose>() where Purpose : ITouchTrigger<Purpose>;
        void Down( FingerTip tip );
        void Move( FingerTip tip );
        void Lift( FingerTip tip );
    }

    public interface ITouchTrigger<Innerer>
        : ITouchTrigger
    {
        Innerer trigger();
    }


    public interface ITouchableElement
        : ITouchable
    {
        Point64 ScreenLocation();
        IRectangle ScreenRectangle();
        ITouchEventTrigger touch { get; }
        void TouchInputReady( PointerInput touchdevice );
    }

    public interface IBasicTouchable
        : ITouchableElement
    {
        void OnTouchDown( FingerTip tip );
        void OnTouchMove( FingerTip tip );
        void OnTouchLift( FingerTip tip );

        event FingerTip.TouchDelegate TouchDown;
        event FingerTip.TouchDelegate TouchLift;
        event FingerTip.TouchDelegate TouchMove;
    }

    public interface IBasicTouchTrigger
    {
        FingerTip               finger();
        IBasicTouchTrigger      touch();
        FingerTip.TouchDelegate TouchDown { get; set; }
        FingerTip.TouchDelegate TouchLift { get; set; }
        FingerTip.TouchDelegate TouchMove { get; set; }
    }

    public interface ITouchSelectable
        : IBasicTouchable
    {
        void OnTouchTapped( FingerTip tip );
        void OnTouchDupple( FingerTip tip );
        void OnTouchTrippl( FingerTip tip );

        event FingerTip.TouchDelegate TouchTapped;
        event FingerTip.TouchDelegate TouchDupple;
        event FingerTip.TouchDelegate TouchTrippl;
    }

    public interface ISelectTouchTrigger
        : IBasicTouchTrigger
    {
        ISelectTouchTrigger     tipseld();
        FingerTip.TouchDelegate TouchTapped { get; set; }
        FingerTip.TouchDelegate TouchDupple { get; set; }
        FingerTip.TouchDelegate TouchTrippl { get; set; }
    }

    public interface ITouchGestutred
        : ITouchSelectable
    {
        void OnTouchDraged(MultiFinger tip);
        void OnTouchResize(MultiFinger tip);
        void OnTouchRotate(MultiFinger tip);

        event MultiFinger.TouchDelegate TouchDraged;
        event MultiFinger.TouchDelegate TouchResize;
        event MultiFinger.TouchDelegate TouchRotate;
    }

    public interface IGestureTouchTrigger
        : ISelectTouchTrigger
    {
        MultiFinger               hands();
        IGestureTouchTrigger      gesture();
        MultiFinger.TouchDelegate TouchDraged { get; set; }
        MultiFinger.TouchDelegate TouchResize { get; set; }
        MultiFinger.TouchDelegate TouchRotate { get; set; }
    }

    public interface ITouchEventTrigger
        : ITouchTrigger<ITouchEventTrigger>
    {
        bool hasFinger( ushort fingerTipId );       
    }
    
    public interface ITouchEventTrigger<EventProvider,EventEnsourcer>
        : ITouchEventTrigger
    where EventProvider
        : ITouchableElement
    {
        EventProvider                     outerer();
        EventEnsourcer                    sourced();
        ITouchTrigger<EventEnsourcer>     innerer();
    }


    public interface ITouchHandler<ElementType,EventsType,InnerType>
        : ITouchEventTrigger<EventsType,InnerType>
    where ElementType
        : Control, EventsType
    where EventsType
        : ITouchableElement
    {
        InnerType events();
    }

    public static class Extensions
    {
        public static ITouchableElement ToTouchableElement(this Control cast)
        {
            return cast as ITouchableElement;
        }
    }

    public class BasicTouchHandler<ElementType>
        : ITouchHandler<ElementType,IBasicTouchable,IBasicTouchTrigger>
        , IBasicTouchTrigger
    where ElementType
        : Control
        , IBasicTouchable
    {
        private ElementType           control;
        private ITouchInputDispatcher inputs;

        protected FingerTip           fingers;
        IBasicTouchTrigger IBasicTouchTrigger.touch() { return this; }
        public FingerTip              finger() {
            return fingers;
        }

        FingerTip.TouchDelegate IBasicTouchTrigger.TouchDown { get; set; }
        FingerTip.TouchDelegate IBasicTouchTrigger.TouchLift { get; set; }
        FingerTip.TouchDelegate IBasicTouchTrigger.TouchMove { get; set; }

        public IBasicTouchTrigger events() { return this; }
        

        public BasicTouchHandler( ElementType init ) { control = init; }

        public Rect     Bounds { get { return control.Bounds; } set { control.Bounds = value; } }
        public Control Element { get { return control; } }
        ITouchableElement ITouchable.element() { return control; }

        void ITouchTrigger.Down( FingerTip tip ) { control.OnTouchDown(tip); events().TouchDown?.Invoke(control, tip); }
        void ITouchTrigger.Move( FingerTip tip ) { control.OnTouchMove(tip); events().TouchMove?.Invoke(control, tip); }
        void ITouchTrigger.Lift( FingerTip tip ) { control.OnTouchLift(tip); events().TouchLift?.Invoke(control, tip); }

        public virtual IBasicTouchable           outerer() { return control; }
        public IBasicTouchTrigger                sourced() { return this as IBasicTouchTrigger; }
        public ITouchTrigger<IBasicTouchTrigger> innerer() { return this as ITouchTrigger<IBasicTouchTrigger>; }

        ITouchable ITouchTrigger.instance<Purpose>() {
            return typeof(Purpose) == typeof(ITouchEventTrigger<IBasicTouchable,IBasicTouchTrigger>) 
                 ? (ITouchable)control : (ITouchable)inputs;
        }

        public ITouchEventTrigger trigger() {
            return this;
        }

        public virtual ITouchDispatchTrigger screen() {
            return inputs.dispatch().trigger();
        }

        public bool hasFinger( ushort fingerTipId ) {
            return fingers != null
                 ? fingers.HasFinger(fingerTipId)
                 : false;
        }

        public bool IsTouched {
            get { return hasFinger(0); }
        }

    }

    public class TouchGesturesHandler<ElementType>
        : ITouchHandler<ElementType,ITouchGestutred,IGestureTouchTrigger>
        , IGestureTouchTrigger
    where ElementType
        : Control
        , ITouchGestutred
    {
        private ElementType           control;
        private ITouchInputDispatcher inputs;

        protected FingerTip           fingers;
        public    FingerTip           finger() { return fingers; }
        IBasicTouchTrigger IBasicTouchTrigger.touch() { return this; }
        ISelectTouchTrigger ISelectTouchTrigger.tipseld() { return this; }

        protected MultiFinger         hand;
        public MultiFinger            hands() { return hand; }
        IGestureTouchTrigger IGestureTouchTrigger.gesture() { return this; }

        FingerTip.TouchDelegate IBasicTouchTrigger.TouchDown { get; set; }
        FingerTip.TouchDelegate IBasicTouchTrigger.TouchLift { get; set; }
        FingerTip.TouchDelegate IBasicTouchTrigger.TouchMove { get; set; }

        FingerTip.TouchDelegate ISelectTouchTrigger.TouchTapped { get; set; }
        FingerTip.TouchDelegate ISelectTouchTrigger.TouchDupple { get; set; }
        FingerTip.TouchDelegate ISelectTouchTrigger.TouchTrippl { get; set; }

        MultiFinger.TouchDelegate IGestureTouchTrigger.TouchDraged { get; set; }
        MultiFinger.TouchDelegate IGestureTouchTrigger.TouchResize { get; set; }
        MultiFinger.TouchDelegate IGestureTouchTrigger.TouchRotate { get; set; }

        public TouchGesturesHandler(ElementType init) { control = init; }

        public Rect                   Bounds { get { return control.Bounds; } set { control.Bounds = value; } }
        public Control               Element { get { return control; } }
        ITouchableElement ITouchable.element() { return control; }
        public IGestureTouchTrigger   events() { return this; }
        
         
        void ITouchTrigger.Down( FingerTip tip ) {
            // invoke generation of a Touch down event in the implementing control element
            control.OnTouchDown( tip );
            events().TouchDown?.Invoke( control, tip );
            // check if registered touch maybe consists from more then just a single finger tip 
            if ( tip.HasFlags( IsTouching.SubPrime )
            &&   tip.Prime.TimeDown.TotalMilliseconds <= PointerInput.ThreasholdForDownCount ) {
                // and if so, in addition, invoke generating a higher level gesture event also 
                control.OnTouchTapped( tip );
                events().TouchTapped?.Invoke( control, tip );
            }
        }

        void ITouchTrigger.Move( FingerTip tip ) {
            // invoke generation of a touch move event in the implementing control element
            control.OnTouchMove( tip );
            events().TouchMove?.Invoke(control,tip);

            // check if the actually registerd finger tip maybe slides off from an actual hands ongoing gesture. 
            // ...far away enough to make clear it cannot belong to a single, one-handed interaction gesture, and which
            // rather should be handled as start for a drag/drop operation the user seemingly just begun performing it
            if ( tip.HasFlags( IsTouching.Here|IsTouching.There ) ) {
                // and if so, invoke the implementing control element to let generate a drag or a slide event
                control.OnTouchDraged( hand );
                events().TouchDraged?.Invoke( control, hand );
            }
            // check if more then one fingers belong to maybe one same hand which could perform a one-handed gesture actually  
            if ( tip.HasFlags( IsTouching.SubPrime ) ) {
                if ( hand.GetResizing() != 0 ) {
                    // if so, if maybe distance between fingers in- or de-creases rapidly, make it invoking a resize event
                    control.OnTouchResize( hand );
                    events().TouchResize?.Invoke( control, hand );
                }
                if ( hand.GetRotation() != 0 ) {
                    // and/or if diagonals between fingers rapidly change their rotation angle, make it invoking a screew event
                    control.OnTouchRotate( hand );
                    events().TouchRotate?.Invoke( control, hand );
                }
            }
        }
 
        void ITouchTrigger.Lift( FingerTip tip ) {
            // invoke removing the finger tip from the implementing control element and make it generating a touch lift event
            control.OnTouchLift( tip );
            events().TouchLift?.Invoke( control, tip );
            // check if the just lifted finger tip maybe was part of any kind of higher level gesture operation like dragging or sizeing 
            if ( tip.HasFlags( IsTouching.Here|IsTouching.There ) ) {
                // and make the control element to generate some event for signaling a maby ongoing 'drag' just turned into a 'drop' - plop!
                hand.Discard();
                control.OnTouchDraged( hand );
                events().TouchDraged?.Invoke( control, hand );
            }
        }

        ITouchable ITouchTrigger.instance<Purpose>() {
            return typeof(Purpose) == typeof(ITouchEventTrigger<IBasicTouchable,IBasicTouchTrigger>)
                 ? (ITouchable)control : (ITouchable)inputs;
        }

        public virtual ITouchDispatchTrigger screen() {
            return inputs.dispatch().trigger();
        }

        public bool hasFinger( ushort fingerTipId ) {
            return fingers != null
                 ? fingers.HasFinger( fingerTipId )
                 : false;
        }

        public bool IsTouched {
            get { return hasFinger( 0 ); }
        }


        ITouchGestutred ITouchEventTrigger<ITouchGestutred,IGestureTouchTrigger>.outerer() { return control; }

        IGestureTouchTrigger ITouchEventTrigger<ITouchGestutred,IGestureTouchTrigger>.sourced() { return this as IGestureTouchTrigger; }

        ITouchTrigger<IGestureTouchTrigger> ITouchEventTrigger<ITouchGestutred,IGestureTouchTrigger>.innerer() { return this as ITouchTrigger<IGestureTouchTrigger>; }

        ITouchEventTrigger ITouchTrigger<ITouchEventTrigger>.trigger() { return this; }

    } 

    public interface IBasicTouchableElement<T>
        : IBasicTouchable
    where T
        : Control
        , IBasicTouchable
    {
        BasicTouchHandler<T> handler();
    }

    public interface ITouchGesturedElement<T>
        : ITouchGestutred 
    where T 
        : Control
        , ITouchGestutred
    {
        TouchGesturesHandler<T> handler();
    }

    public interface ITouchDispatchTrigger
        : ITouchTrigger<ITouchDispatchTrigger>
    {
        ITouchTrigger<ITouchDispatchTrigger> dispatch();
    }

    public interface ITouchInputDispatcher
        : ITouchDispatchTrigger
    {
        event EventHandler Load;
        PointerInput input { get; }
    }

    public interface ITouchInputWindow
        : ITouchInputDispatcher
        , ITouchableElement
    {}

}

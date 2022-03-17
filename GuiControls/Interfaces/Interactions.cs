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
using Win32Imports.Touch;
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
    public interface ITouchable
    {
        ITouchDispatchTrigger screen();
        ITouchableElement element();
        Rect    Bounds { get; set; }
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

    public interface ITouchTrigger<Purpose>
        : ITouchTrigger
    {
        Purpose instance();
    }



    //public abstract class TouchEventTrigger : ITouchEventTrigger<ITouchableElement>
    //{
    //    public abstract Rect Bounds { get; set; }
    //    public abstract Control Element { get; }
    //    public abstract bool IsTouched { get; }



    //    public virtual void Down( FingerTip tip )
    //    {
    //        if( !element.IsTouched ) {
    //            if( tip.Reporter != element ) {
    //                tip.Interact( element );
    //                tip.SetOffset( element.ScreenLocation() );
    //            }  fingertip = tip;
    //        } else {
    //            TimeSpan time = tip.duration - fingertip.duration;
    //            if( time.Milliseconds < 300 ) {
    //                fingertip += tip;
    //                Interaction = new MultiFinger( fingertip );
    //            }
    //        } TouchDown?.Invoke( element, tip );
    //    }

    //    public abstract ITouchableElement instance();
    //    public abstract ITouchable instance<OnSide>() where OnSide : ITouchEventTrigger<OnSide>;

    //    public virtual void Lift( FingerTip tip )
    //    {
    //        if( tip.AnyFlags( IsTouching.SubPrime ) )
    //            if( tip.TimeDown < TimeSpan.FromMilliseconds( PointerInput.ThreasholdForDownCount ) ) {
    //                IsTouching stored = tip & IsTouching.SubPrime;
    //                tip.info |= IsTouching.LiftOff;
    //                tip.info &= stored;
    //                element.invokeTouchEvent( tip );
    //            } else if ( Interaction != null ) {
    //                if( fingertip.Count <= 2 )
    //                    Interaction = null;
    //            }
    //        TouchLift?.Invoke( element, tip );
    //    }

    //    public virtual void Move( FingerTip tip )
    //    {
    //        if( Interaction != null ) {
    //            Interaction.Check();
    //        }
    //        TouchMove?.Invoke( element, tip );
    //    }

    //    public abstract ITouchableScreen screen();
    //}



    public interface ITouchableElement
        : ITouchable
    {
        Point64            ScreenLocation();
        IRectangle         ScreenRectangle();
        ITouchEventTrigger touch();
    }

    public interface IBasicTouchable
        : ITouchableElement
    {
        void OnTouchDown(FingerTip tip);
        void OnTouchMove(FingerTip tip);
        void OnTouchLift(FingerTip tip);

        event FingerTip.TouchDelegate TouchDown;
        event FingerTip.TouchDelegate TouchLift;
        event FingerTip.TouchDelegate TouchMove;
    }

    public interface ITouchGestutred
        : IBasicTouchable
    {
        void OnTouchTapped(MultiFinger tip);
        void OnTouchDraged(MultiFinger tip);
        void OnTouchResize(MultiFinger tip);
        void OnTouchRotate(MultiFinger tip);

        event MultiFinger.TouchDelegate TouchTapped;
        event MultiFinger.TouchDelegate TouchDraged;
        event MultiFinger.TouchDelegate TouchResize;
        event MultiFinger.TouchDelegate TouchRotate;
    }

    public interface ITouchEventTrigger
        : ITouchTrigger<ITouchEventTrigger>
    {
        FingerTip       finger();
        MultiFinger     hand();
        bool            hasFinger( ushort fingerTipId );       
    }
    
    public interface ITouchEventTrigger<EventProvider>
        : ITouchEventTrigger
        where EventProvider : ITouchableElement
    {
        EventProvider events();
        ITouchTrigger<ITouchEventTrigger> trigger();  
    }
    
    //public enum TouchEvent
    //{
    //    Down,Move,Lift,Left,Right,Slide,Sized,Screwd
    //}



    /*
    public interface ITouchableElementEvents<HandlerType> : ITouchableElement where HandlerType : ITouchComponent 
    { 
        void  invoke( FingerTip tip );
        event FingerTip.TouchDelegate TouchDown;
        event FingerTip.TouchDelegate TouchLift;
        event FingerTip.TouchDelegate TouchMove;

        // occurrs on recognized single finger tap (where down time was les or equal to
        // the time amount which also is used for recognizing double click mouse events)   
        event FingerTip.TouchDelegate LeftTapped;

        // occurrs on recognized first and second finger tap within same region, taken
        // place within duration of the configured double click time amount) 
        event FingerTip.TouchDelegate RightTapped;

        // occurs each time a finger (which is down for at least the 'double click time'
        // amount on milliseconds) is moving on the screen and emitting TouchMove events
        event FingerTip.TouchDelegate TouchSlide;

        // occurs on two finger 'resize' gestures and should deliver distance between 
        // fingers as well as movement speed off finger against each other as it's EventArgs
        // parameter (should be public)
        event MultiFinger.TouchDelegate TouchSized;

        // occurs on two finger 'screwing' gestures and should deliver rotation angle of
        // the diagonale between the two fingers, as well as rotation axis (center between
        // fingers) and speed of the retation as EventArgs parameter (should be public)
        event MultiFinger.TouchDelegate TouchScrew;

   //     void invoke( TouchEvent named, FingerTip finger );
        
    }
    */

    public interface ITouchHandler<ElementType,EventsType>
        : ITouchEventTrigger<EventsType>
        where ElementType : Control, EventsType
        where EventsType : ITouchableElement
    {
        ITouchHandler<ElementType,EventsType> motivator();
    }

    public static class Extensions
    {
        public static ITouchableElement ToTouchableElement(this Control cast)
        {
            return cast as ITouchableElement;
        }
    }

    abstract public class BasicTouchHandler<ElementType>
        : ITouchHandler<ElementType, IBasicTouchable>
        where ElementType : Control, IBasicTouchable
    {
        private ElementType           control;
        private ITouchInputDispatcher inputs;

        protected FingerTip           finger;
        protected MultiFinger         hand;

        protected FingerTip.TouchDelegate TouchDown;
        protected FingerTip.TouchDelegate TouchLift;
        protected FingerTip.TouchDelegate TouchMove;

        public BasicTouchHandler(ElementType init) { control = init; }

        public Rect                   Bounds { get { return control.Bounds; } set { control.Bounds = value; } }
        public Control               Element { get { return control; } }
        ITouchableElement ITouchable.element() { return control; }

        void ITouchTrigger.Down(FingerTip tip) { control.OnTouchDown( tip ); TouchDown?.Invoke(control,tip); }
        void ITouchTrigger.Move(FingerTip tip) { control.OnTouchMove( tip ); TouchMove?.Invoke(control,tip); }
        void ITouchTrigger.Lift(FingerTip tip) { control.OnTouchLift( tip ); TouchLift?.Invoke(control,tip); }

        public virtual IBasicTouchable  events() { return control; }
        FingerTip    ITouchEventTrigger.finger() { return finger; }
        MultiFinger    ITouchEventTrigger.hand() { return hand; }

        ITouchHandler<ElementType,IBasicTouchable> ITouchHandler<ElementType,IBasicTouchable>.motivator()
        {
            return this;
        }

        public virtual ITouchEventTrigger instance() { return trigger().instance<ITouchEventTrigger>() as ITouchEventTrigger; }
        ITouchable ITouchTrigger.instance<Purpose>() { if( typeof(Purpose) == typeof(ITouchEventTrigger<IBasicTouchable>) ) return control; else return inputs; }

        public ITouchTrigger<ITouchEventTrigger> trigger() { return this; }

        public virtual ITouchDispatchTrigger screen()
        {
            return inputs.trigger().instance();
        }

        public bool hasFinger( ushort fingerTipId ) { return finger != null ? finger.HasFinger(fingerTipId) : false; }
        public bool IsTouched { get { return hasFinger(0); } }

    }

    public class TouchGesturesHandler<ElementType>
        : ITouchHandler<ElementType, ITouchGestutred>
        where ElementType : Control, ITouchGestutred
    {
        private ElementType           control;
        private ITouchInputDispatcher inputs;

        protected FingerTip           finger;
        protected MultiFinger         hand;

        protected FingerTip.TouchDelegate TouchDown;
        protected FingerTip.TouchDelegate TouchLift;
        protected FingerTip.TouchDelegate TouchMove;

        protected MultiFinger.TouchDelegate TouchTapped;
        protected MultiFinger.TouchDelegate TouchDraged;
        protected MultiFinger.TouchDelegate TouchResize;
        protected MultiFinger.TouchDelegate TouchRotate;

        public TouchGesturesHandler(ElementType init) { control = init; }

        public Rect                   Bounds { get { return control.Bounds; } set { control.Bounds = value; } }
        public Control               Element { get { return control; } }
        ITouchableElement ITouchable.element() { return control; }

        void ITouchTrigger.Down( FingerTip tip ) {
            // invoke generation of a Touch down event in the implementing control element
            control.OnTouchDown( tip );
            TouchDown?.Invoke( control, tip );
            // check if registered touch maybe consists from more then just a single finger tip 
            if ( tip.HasFlags( IsTouching.SubPrime )
            &&   tip.Prime.TimeDown.TotalMilliseconds <= PointerInput.ThreasholdForDownCount ) {
                // and if so, in addition, invoke generating a higher level gesture event also 
                control.OnTouchTapped( hand );
                TouchTapped?.Invoke( control, hand );
            }
        }

        void ITouchTrigger.Move( FingerTip tip ) {
            // invoke generation of a touch move event in the implementing control element
            control.OnTouchMove( tip );
            TouchMove?.Invoke(control,tip);

            // check if the actually registerd finger tip maybe slides off from an actual hands ongoing gesture 
            // far away enough to make clear it cannot belong to a single hands gestured interaction and so rather
            // should be handled as a starting drag/drop operation a user seemingly just had begun performing it
            if ( tip.HasFlags( IsTouching.Here|IsTouching.There ) ) {
                // and if so, invoke the implementing control element to let generate a drag or a slide event
                control.OnTouchDraged( hand );
                TouchDraged?.Invoke( control, hand );
            }
            // check if more then one fingers belong to maybe one same hand which could perform a gesture actually  
            if ( tip.HasFlags( IsTouching.SubPrime ) ) {
                if ( hand.GetResizing() != 0 ) {
                    // if so, if maybe distance between fingers in- or de-creases rapidly, make it invoking a resize event
                    control.OnTouchResize( hand );
                    TouchResize?.Invoke(control, hand );
                }
                if ( hand.GetRotation() != 0 ) {
                    // and/or if diagonals between fingers rapidly change their rotation angle, make it invoking a screew event
                    control.OnTouchRotate( hand );
                    TouchRotate?.Invoke(control, hand );
                }
            }
        }
 
        void ITouchTrigger.Lift( FingerTip tip ) {
            // invoke removing the finger tip from the implementing control element and make it generating a touch lift event
            control.OnTouchLift( tip );
            TouchLift?.Invoke( control, tip );
            // check if the just lifted finger tip maybe was part of any kind of higher level gestured operation like dragging or sizeing 
            if ( tip.HasFlags( IsTouching.Here|IsTouching.There ) ) {
                // and make the control element to generate some event for signaling a maby ongoing 'drag' so now turns into a 'drop' now  
                hand.Discard();
                control.OnTouchDraged( hand );
                TouchDraged?.Invoke( control, hand );
            }
        }

        public virtual ITouchGestutred  events() { return control; }
        FingerTip    ITouchEventTrigger.finger() { return finger; }
        MultiFinger ITouchEventTrigger.hand() { return hand; }
        
        public virtual ITouchEventTrigger instance() { return trigger().instance<ITouchEventTrigger>() as ITouchEventTrigger; }
        ITouchable ITouchTrigger.instance<Purpose>() { if( typeof(Purpose) == typeof(ITouchEventTrigger<IBasicTouchable>) ) return control; else return inputs; }

        public ITouchTrigger<ITouchEventTrigger> trigger() { return this; }

        public virtual ITouchDispatchTrigger screen()
        {
            return inputs.trigger().instance();
        }

        public bool hasFinger( ushort fingerTipId ) { return finger != null ? finger.HasFinger(fingerTipId) : false; }
        public bool IsTouched { get { return hasFinger(0); } }

        ITouchHandler<ElementType,ITouchGestutred> ITouchHandler<ElementType,ITouchGestutred>.motivator()
        {
            return this;
        }
    } 

    public interface IBasicTouchableElement<T> : IBasicTouchable where T : Control, IBasicTouchable
    {
        BasicTouchHandler<T> handler();
    }

    public interface ITouchGesturedElement<T> : ITouchGestutred where T : Control, ITouchGestutred
    {
        TouchGesturesHandler<T> handler();
    }




    public interface ITouchDispatchTrigger
        : ITouchTrigger<ITouchDispatchTrigger>
    {
        ITouchTrigger<ITouchDispatchTrigger> trigger();
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



    /*
        abstract public class BasicTouchHandler<ElementType>
            : ITouchHandler<ElementType,IBasicTouchable>
        where ElementType 
            : Control, IBasicTouchable
        {
            protected MultiFinger                  Interactor;
            protected FingerTip                    fingertip;
            public Control                         Element { get { return this.element; } }
            protected ElementType                  element;
            ITouchableElement           ITouchable.element { get { return this.element; } }
            public EventsType                      events() { return this; }

            public TouchHandler( ElementType parent ) {
                element = parent;
                Interactor = null;
            }

            ITouchableElementEvents<ITouchComponent> ITouchComponent.events()
            {
                return this as ITouchableElementEvents<ITouchComponent>;
            }

            MultiFinger ITouchComponent.interaction() {
                if(Interactor==null) Interactor = new MultiFinger(fingertip);
                return Interactor;
            }

            public ITouchableScreen screen() {
                return PointerInput.Dispatcher.screen();
            }

            Rect ITouchable.Bounds {
                get { return element.Bounds; }
                set { element.Bounds = value; }
            }

            Control ITouchable.Element {
                get { return element; }
            } 

            bool ITouchable.IsTouched { get { return fingertip; } }

            public FingerTip finger()
            {
                return fingertip;
            }

            public bool hasFinger( ushort checkId )
            {
                bool has = fingertip != null;
                if ( has ) has = fingertip.HasFinger( checkId );
                return has;
            }

            public ITouchEventTrigger trigger()
            {
                return this;
            }

            void ITouchableElementEvents<TouchHandler<ElementType>>.invoke( FingerTip tip )
            {
                if( tip.HasFlags( IsTouching.NoMore ) ) {
                    if( tip == ~IsTouching.Prime ) {
                        LeftTapped?.Invoke( this, tip );
             } else if( !tip.HasFlags( IsTouching.SubPrime ) ) {
                        RightTapped?.Invoke( this, tip );
                    }
                } else if( tip.HasFlags( IsTouching.SubPrime ) ){
                    if( Interactor ) {
                        float val = Interactor.GetRotation();
                        if( val != 0 )
                            TouchScrew?.Invoke( this, Interactor );
                        val = Interactor.GetResizing();
                        if( val != 0 )
                            TouchSized?.Invoke( this, Interactor );
                    }
                }
            }

            void ITouchableElementEvents<TouchHandler<ElementType>>.invoke( TouchEvent e, FingerTip tip )
            {
                switch(e) {
                    case TouchEvent.Left: LeftTapped?.Invoke(element, tip); break;
                    case TouchEvent.Right: RightTapped?.Invoke(element, tip); break;
                    case TouchEvent.Sized: TouchSized?.Invoke(element, Interactor); break;
                    case TouchEvent.Screwd: TouchScrew?.Invoke(element, Interactor); break;
                    case TouchEvent.Slide: TouchSlide?.Invoke(element, tip); break;
                }
            }


            virtual public void OnTouchDown(FingerTip tip)
            {
                TouchDown?.Invoke( Element, tip );
                Element.Invalidate();
            }

            virtual public void OnTouchMove(FingerTip tip)
            {
                TouchMove?.Invoke( Element, tip );
                Element.Invalidate();
            }

            virtual public void OnTouchLift(FingerTip tip)
            {
                TouchLift?.Invoke( Element, tip );
                Element.Invalidate();
            }

            void ITouchEventTrigger.Down( FingerTip tip )
            {
                if( !hasFinger( tip.Id ) ) {
                    if ( fingertip != null ) {
                        TimeSpan time = tip.duration - fingertip.duration;
                        if( time.Milliseconds < 300 ) {
                            fingertip += tip;
                            touch().interaction().FingerReset( fingertip );
                        } 
                        //tip.Move = trigger().Move;
                        //tip.Lift = trigger().Lift;
                    } else {
                  //      tip.Interact( element );
                  //      tip.SetOffset( element.ScreenLocation() );
                        fingertip = tip;
                    }
                } OnTouchDown( fingertip );
            }


            void ITouchEventTrigger.Lift( FingerTip tip )
            {
                if( tip.AnyFlags( IsTouching.SubPrime ) ) {
                    if( tip.TimeDown < TimeSpan.FromMilliseconds( PointerInput.ThreasholdForDownCount ) ) {
                        IsTouching stored = tip & IsTouching.SubPrime;
                        tip.info |= IsTouching.LiftOff;
                        tip.info &= stored;
                        touch().events().invoke( tip );
                    } else if( Interactor )
                        if( fingertip.Count <= 2 )
                            Interactor.Discard();
                } OnTouchLift( tip );
                fingertip = null;
            }

            void ITouchEventTrigger.Move( FingerTip tip )
            {
                if( Interactor ) {
                    Interactor.CheckFingers();
                } OnTouchMove( tip );
            }

            public ITouchableElement instance()
            {
                return element;
            }

            ITouchable ITouchEventTrigger.instance<OnSide>()
            {
                if ( element is OnSide )
                     return element;
                else return screen();
            }
            Point64 ITouchableElement.ScreenLocation()
            {
                return element.ScreenLocation();
            }
            RECT   ITouchableElement.ScreenRectangle()
            {
                return element.ScreenRectangle();
            }
            public ITouchComponent touch()
            {
                return this;
            }
        }
        */
}

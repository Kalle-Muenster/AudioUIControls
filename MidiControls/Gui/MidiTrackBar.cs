using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using Stepflow.Gui.Geometry;
using Stepflow.Midi;
using Win32Imports.Midi;

namespace Stepflow.Midi.Gui
{
    public class MidiTrackBar
        : TrackBar
        , IInterValuable<Controlled.Int32>
        , IMidiControlElement<MidiInOut>
        , IBasicTouchableElement<MidiTrackBar>

    {
        private MidiInOut                                   midiio;
        private Controlled.Int32                            value;
        private ValenceField<Controlled.Int32,ValenceField> fields;
        private AutomationlayerAddressat                    midiad;

        static MidiTrackBar()
        {
            Valence.RegisterIntervaluableType<Controlled.Int32>();
            if( PointerInput.Dispatcher == null )
                PointerInput.AutoRegistration = AutoRegistration.Enabled;
        }

        public MidiTrackBar() : base()
        {
            
            value = new Controlled.Int32();
            value.SetUp( 0, 127, 1, 64, ControlMode.Element );

            midiad = new AutomationlayerAddressat(1, (byte)Win32Imports.Midi.Message.TYPE.NOTE_ON, 127, 0 );
            midiio = new MidiInOut();
            fields = new ValenceField<Controlled.Int32, ValenceField>( this, new ControllerBase[] {value});
            System.ComponentModel.IContainer connector = this.Container == null ? new System.ComponentModel.Container() : this.Container;
            ContextMenuStrip = new ContextMenuStrip( connector );
            ( this as IInterValuable ).getMenuHook().Add( new ValenceBondMenu<Controlled.Int32>(this, connector) );
            midiio.InitializeComponent( this, connector , Invalidate );
            (this as IBasicTouchableElement<MidiTrackBar>).handler = new BasicTouchHandler<MidiTrackBar>( this );
            midiio.output().ConfigureAsMessagingAutomat( midiad, 0 );
            midiio.input().RegisterAsMesssageListener( midiad );

            if( PointerInput.Dispatcher == null ) {
                PointerInput.Initialized += touch.element().TouchInputReady;
            } else PointerInput.Dispatcher.RegisterTouchableElement( this );

            midiio.input().AutomationEvent += midi().automat().OnIncommingMidiControl;
            ValueChanged += MidiTrackBar_ValueChanged;
            Paint += midiio.input().ProcessMessageQueue;
            Disposed += MidiComboBox_Disposed;
        }



        private void MidiTrackBar_ValueChanged( object sender, EventArgs e )
        {
            int current = value;
            if( current != Value ) {
                value.VAL = Value;
                valence().SetDirty( ValenceFieldState.Flags.VAL );
            }

        }

        private void MidiComboBox_Disposed( object sender, EventArgs e )
        {
            Valence.UnRegisterIntervaluableElement( this );
            PointerInput.Dispatcher?.UnRegisterTouchableElement( this );
        }


        public MidiInOut midi() {
            return midiio;
        }

        AutomationlayerAddressat[] IAutomat<MidiInOut>.channels {
            get { return new AutomationlayerAddressat[] { midiad }; }
        }

        Control ITouchable.Element {
            get { return this; }
        }

        MidiInputMenu<MidiInOut> IMidiControlElement<MidiInOut>.inputMenu {
            get; set;
        }

        bool ITouchable.IsTouched {
            get { return touch.IsTouched; }
        }

        public Value MidiValue {
            get { return new Value( (short)Value ); }
            set { this.value.VAL = (int)(value.ProportionalFloat * (Maximum - Minimum));
                valence().SetDirty( ValenceFieldState.Flags.VAL );
                Value = this.value;
            }
        }

        MidiOutputMenu<MidiInOut> IMidiControlElement<MidiInOut>.outputMenu {
            get; set;
        }

        public ITouchEventTrigger touch {
            get { return (this as IBasicTouchableElement<MidiTrackBar>).handler; }
        }

        private IBasicTouchTrigger touchEvents()
        {
            return ( this as IBasicTouchableElement<MidiTrackBar> ).handler.events();
        }


        public event FingerTip.TouchDelegate TouchDown {
            add { touchEvents().TouchDown += value; }
            remove { touchEvents().TouchDown -= value; }
        }

        public event FingerTip.TouchDelegate TouchLift {
            add { touchEvents().TouchLift += value; }
            remove { touchEvents().TouchLift -= value; }
        }

        public event FingerTip.TouchDelegate TouchMove {
            add { touchEvents().TouchMove += value; }
            remove { touchEvents().TouchMove -= value; }
        }

        void IInterValuable.callOnInvalidated( InvalidateEventArgs e )
        {
            OnInvalidated( e );
        }

        ITouchableElement ITouchable.element()
        {
            return this;
        }

        Action IInterValuable.getInvalidationTrigger()
        {
            return Invalidate;
        }

        ToolStripItemCollection IInterValuable.getMenuHook()
        {
            return ContextMenuStrip.Items;
        }

        BasicTouchHandler<MidiTrackBar> IBasicTouchableElement<MidiTrackBar>.handler
        {
            get; set;
        }

        void IMidiControlElement<MidiInOut>.OnIncommingMidiControl( object sender, Win32Imports.Midi.Message value )
        {
            MidiValue = value;
        }

        void IBasicTouchable.OnTouchDown(FingerTip tip)
        {}

        void IBasicTouchable.OnTouchLift(FingerTip tip)
        {}

        void IBasicTouchable.OnTouchMove(FingerTip tip)
        {}

        ITouchDispatchTrigger ITouchable.screen()
        {
            return touch.screen();
        }

        Point64 ITouchableElement.ScreenLocation()
        {
            return PointToScreen( Point.Empty );
        }

        IRectangle ITouchableElement.ScreenRectangle()
        {
            return AbsoluteEdges.FromRectangle( RectangleToScreen( new Rectangle(0, 0, Width, Height)) );
        }

        void ITouchableElement.TouchInputReady( PointerInput touchdevice )
        {
            touchdevice.RegisterTouchableElement( this );
            PointerInput.Initialized -= touch.element().TouchInputReady;
        }

        public IControllerValenceField<Controlled.Int32> valence()
        {
            return fields;
        }

        public IControllerValenceField<Controlled.Int32> valence( Enum field )
        {
            return fields.field( field );
        }

        IControllerValenceField IInterValuable.valence<cT>()
        {
            if( typeof(cT) == typeof(Controlled.Int32) ) return fields;
            else throw new TypeAccessException( "wrong type" );
        }

        IControllerValenceField IInterValuable.valence<cT>( Enum field )
        {
            return fields.field( field );
        }
    }
}

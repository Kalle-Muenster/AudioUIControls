using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using Stepflow.Gui.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32Imports.Midi;

namespace Stepflow.Midi.Gui
{
    public class MidiComboBox
        : ComboBox
        , IInterValuable<Controlled.Byte>
        , IMidiControlElement<MidiInOut>
        , IBasicTouchableElement<MidiComboBox>

    {
        private Controlled.Byte                 index;
        private MidiInOut                       midiio;
        private AutomationlayerAddressat        midiad;

        static MidiComboBox()
        {
            Valence.RegisterIntervaluableType<Controlled.Byte>();
            if( !PointerInput.isInitialized() ) {
                PointerInput.AutoRegistration = AutoRegistration.Enabled;
            }
        }

        public MidiComboBox() : base()
        {
            index = new Controlled.Byte();
            index.SetUp( 0, 127, 1, 64, ControlMode.Element );

            midiad = new AutomationlayerAddressat(1, (byte)Win32Imports.Midi.Message.TYPE.NOTE_ON, 127, 0 );
            midiio = new MidiInOut();

            ( this as IBasicTouchableElement<MidiComboBox> ).handler = new BasicTouchHandler<MidiComboBox>( this );
            ctr_valence = new ValenceField<Controlled.Byte, ValenceField>( this, new ControllerBase[] { index });
            System.ComponentModel.IContainer connector = this.Container == null ? new System.ComponentModel.Container() : this.Container;
            ContextMenuStrip = new ContextMenuStrip( connector );
            ( this as IInterValuable ).getMenuHook().Add( new ValenceBondMenu<Controlled.Byte>(this, connector));
            midiio.InitializeComponent( this, connector , Invalidate );
            midiio.output().ConfigureAsMessagingAutomat( midiad, 0 );
            midiio.input().RegisterAsMesssageListener( midiad );
            midiio.input().AutomationEvent += midi().automat().OnIncommingMidiControl;

            if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                if( PointerInput.Dispatcher == null ) {
                    PointerInput.Initialized += touch.element().TouchInputReady;
                } else
                    PointerInput.Dispatcher.RegisterTouchableElement(this);
            }

            SelectedIndexChanged += MidiSelectBox_SelectedIndexChanged;
            Paint += midiio.input().ProcessMessageQueue;
            Disposed += MidiComboBox_Disposed;
        }

        public event FingerTip.TouchDelegate TouchDown {
            add { ( this as IBasicTouchableElement<MidiComboBox> ).handler.events().TouchDown += value; }
            remove { ( this as IBasicTouchableElement<MidiComboBox> ).handler.events().TouchDown -= value; }
        }

        public event FingerTip.TouchDelegate TouchLift {
            add { ( this as IBasicTouchableElement<MidiComboBox> ).handler.events().TouchLift += value; }
            remove { ( this as IBasicTouchableElement<MidiComboBox> ).handler.events().TouchLift -= value; }
        }

        public event FingerTip.TouchDelegate TouchMove {
            add { ( this as IBasicTouchableElement<MidiComboBox> ).handler.events().TouchMove += value; }
            remove { ( this as IBasicTouchableElement<MidiComboBox> ).handler.events().TouchMove -= value; }
        }

        private void MidiComboBox_Disposed( object sender, EventArgs e )
        {
            Valence.UnRegisterIntervaluableElement( this );
            PointerInput.Dispatcher?.UnRegisterTouchableElement( this );
        }

        private void MidiSelectBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            index.VAL = (byte)SelectedIndex;
            valence().SetDirty( ValenceFieldState.Flags.VAL );
            midiio.output().OnValueChange( this, MidiValue );
            Invalidate();
        }

        public Byte First { 
            get { return index.MIN; }
            set { index.MIN = value;
                valence().SetDirty( ValenceFieldState.Flags.MIN );
            }
        }

        public Byte Last {
            get { return index.MAX; }
            set { index.MAX = value;
                valence().SetDirty( ValenceFieldState.Flags.MAX );
            }
        }

        public Byte Index { 
            get { return (byte)SelectedIndex; }
            set { SelectedIndex = value; }
        }

#region MidiAutomation
        public Value MidiValue {
            get { return new Value( SelectedIndex ); }
            set { SelectedIndex = value; }
        }

        
        public MidiInOut midi() {
            return midiio;
        }

        MidiInputMenu<MidiInOut> IMidiControlElement<MidiInOut>.inputMenu { 
            get; set;
        }
        MidiOutputMenu<MidiInOut> IMidiControlElement<MidiInOut>.outputMenu {
            get; set;
        }

        AutomationlayerAddressat[] IAutomat<MidiInOut>.channels {
            get { return new AutomationlayerAddressat[] { midiad }; }
        }

        void IMidiControlElement<MidiInOut>.OnIncommingMidiControl( object sender, Win32Imports.Midi.Message value )
        {
            MidiValue = new Value( (short)value.Value );
        }

        public IncommingAutomation<Win32Imports.Midi.Message> midiDelegate()
        {
            return midi().automat().OnIncommingMidiControl;
        }
#endregion

#region IInterValuable
        private ValenceField<Controlled.Byte,ValenceField>  ctr_valence;
        ToolStripItemCollection IInterValuable.getMenuHook() { return ContextMenuStrip.Items; }
        public IControllerValenceField<Controlled.Byte> valence() { return ctr_valence.field(); }
        public IControllerValenceField<Controlled.Byte> valence( Enum which ) { return ctr_valence.field(which); }
        IControllerValenceField IInterValuable.valence<cT>( Enum which ) { return ctr_valence.field(which); }
        IControllerValenceField IInterValuable.valence<cT>() { return ctr_valence.field(); }
        Action IInterValuable.getInvalidationTrigger() { return valueUpdate; }
        void IInterValuable.callOnInvalidated( InvalidateEventArgs e ) { OnInvalidated( e ); }
        private void valueUpdate() { /* TriggerEvents(); */ Invalidate(); }
        #endregion

#region IBasicTouchable
        public ITouchEventTrigger touch { get { return (this as IBasicTouchableElement<MidiComboBox>).handler; } }
        Control ITouchable.Element { get { return this; } }
        public bool IsTouched { get { return touch.IsTouched; } }
        BasicTouchHandler<MidiComboBox> IBasicTouchableElement<MidiComboBox>.handler {
            get; set;
        }

        void IBasicTouchable.OnTouchDown( FingerTip tip ){}

        void IBasicTouchable.OnTouchMove( FingerTip tip ){}

        void IBasicTouchable.OnTouchLift( FingerTip tip ){}

        Point64 ITouchableElement.ScreenLocation() {
            return PointToScreen( Point.Empty );
        }

        IRectangle ITouchableElement.ScreenRectangle() {
            Rectangle size = Rectangle.Empty;
            size.Size = Size;
            return AbsoluteEdges.FromRectangle( RectangleToScreen( size ) );
        }

        void ITouchableElement.TouchInputReady( PointerInput touchdevice )
        {
            touchdevice.RegisterTouchableElement( this );
            PointerInput.Initialized -= touch.element().TouchInputReady;
        }

        ITouchDispatchTrigger ITouchable.screen() {
            return touch.screen();
        }

        ITouchableElement ITouchable.element() {
            return this;
        }
#endregion


    }
}

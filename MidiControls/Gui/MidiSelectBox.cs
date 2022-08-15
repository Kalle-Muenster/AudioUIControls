using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using System;
using System.Collections.Generic;
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

    {
        private Controlled.Byte index; 

        static MidiComboBox()
        {
            Valence.RegisterIntervaluableType<Controlled.Byte>();
        }

        public MidiComboBox() : base()
        {
            index = new Controlled.Byte();
            index.SetUp( 0, 127, 1, 64, ControlMode.Element );

            midiad = new AutomationlayerAddressat(1, (byte)Win32Imports.Midi.Message.TYPE.NOTE_ON, 127, 0 );
            midiio = new MidiInOut();

            ctr_valence = new ValenceField<Controlled.Byte, ValenceField>( this, new ControllerBase[] { index });
            System.ComponentModel.IContainer connector = this.Container == null ? new System.ComponentModel.Container() : this.Container;
            ContextMenuStrip = new ContextMenuStrip( connector );
            mnu_valence = new ValenceBondMenu<Controlled.Byte>( this, connector );
            ( this as IInterValuable ).getMenuHook().Add( new ValenceBondMenu<Controlled.Byte>(this, connector));
            midiio.InitializeComponent( this, connector , Invalidate );
            midiio.automate().ConfigureAsMessagingAutomat( midiad, 0 );
            midiio.automation().RegisterAsMesssageListener( midiad );
            midiio.automation().AutomationEvent += midi().OnIncommingMidiControl;
            SelectedIndexChanged += MidiSelectBox_SelectedIndexChanged;
            Paint += midiio.automation().ProcessMessageQueue;
            Disposed += MidiComboBox_Disposed;
        }

        private void MidiComboBox_Disposed( object sender, EventArgs e )
        {
            Valence.UnRegisterIntervaluableElement( this );
        }

        private void MidiSelectBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            index.VAL = (byte)SelectedIndex;
            valence().SetDirty( ValenceFieldState.Flags.VAL );
            midiio.automate().OnValueChange( this, midi().MidiValue );
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
        private MidiInOut                midiio;
        private AutomationlayerAddressat midiad;

        Value IMidiControlElement<MidiInOut>.MidiValue {
            get {
                return new Value( SelectedIndex );
            }
            set {
                SelectedIndex = value;
            }
        }

        
        MidiInOut IMidiControlElement<MidiInOut>.binding {
            get { return midiio; }
        }

        MidiInputMenu<MidiInOut> IMidiControlElement<MidiInOut>.inputMenu { 
            get; set;
        }
        MidiOutputMenu<MidiInOut> IMidiControlElement<MidiInOut>.outputMenu {
            get; set;
        }

        AutomationlayerAddressat[] IAutomat.channels {
            get { return new AutomationlayerAddressat[] { midiad }; }
        }


        public IMidiControlElement<MidiInOut> midi()
        {
            return this;
        }

        void IMidiControlElement<MidiInOut>.OnIncommingMidiControl( object sender, Win32Imports.Midi.Message value )
        {
            midi().MidiValue = new Value( (short)value.Value );
        }

        public IncommingAutomation<Win32Imports.Midi.Message> midiDelegate()
        {
            return midi().OnIncommingMidiControl;
        }
#endregion

#region IInterValuable
        private ValenceBondMenu<Controlled.Byte>            mnu_valence;
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


    }
}

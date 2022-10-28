using System;
using System.ComponentModel;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using Stepflow.TaskAssist;

#if   USE_WITH_WF
using System.Windows.Forms;
#elif USE_WITH_WPF
using System.Windows.Controls;
#endif



namespace Stepflow
{
    namespace Midi
    {

        public partial class MidiMeter 
            : GuiMeter
            , IMidiControlElement<MidiInput>
        {
            protected MidiInput  midiIn;
            private int          messageReadCount = 0;
            private Action       messageLoopTrigger;

            public IncommingAutomation<Win32Imports.Midi.Message> midiDelegate() {
                return (this as IMidiControlElement<MidiInput>).OnIncommingMidiControl;
            }
            public void OnIncommingMidiControl( object sender, Win32Imports.Midi.Message value ) {
                MidiValue = new Win32Imports.Midi.Value( (short)value.Value );
            }
            public Win32Imports.Midi.Value MidiValue {
                get { return new Win32Imports.Midi.Value( (int)(Proportion*127) ); }
                set { Proportion = value.getProportionalFloat(); }
            }
            AutomationlayerAddressat[] IAutomat<MidiInput>.channels {
                get { return new AutomationlayerAddressat[] { midi().GetAutomationBindingDescriptor(0) }; }
            }
            public MidiInput midi() {
                return midiIn;
            }
            MidiInputMenu<MidiInput> IMidiControlElement<MidiInput>.inputMenu { get; set; }
            MidiOutputMenu<MidiInput> IMidiControlElement<MidiInput>.outputMenu { get; set; }

            protected MidiMeter( bool executeConstructor )
                : base( executeConstructor )
            {}

            public MidiMeter()
                : base(false)
            {
                midiIn = new MidiInput();
                InitValue();
                (valence() as ValenceField<Controlled.Float32,MeterValence>).SetControllerArray(
                    new Controlled.Float32[] { value, dampf }
                );
                InitMeter();
                IContainer connector = InitConnector();
                base.RightToLeft = RightToLeft.Yes;
                directionalOrientation = (int)DirectionalOrientation.Up;
                Inverted = true;
                BorderStyle = BorderStyle.None;
                (this as IInterValuable).getMenuHook().Add(
                    new ValenceBondMenu<Controlled.Float32>( this, connector )
                );
                InitMidi( connector );
            }

            private void readMessageQueue()
            {
                bool read = midi().input().messageAvailable();
                if( !read ) { if ( --messageReadCount <= 0 ) task().assist.ReleaseAssist( messageLoopTrigger ); }
                else { midi().input().ProcessMessageQueue( this, new EventArgs() );
                       messageReadCount = 10; }
                if ( read ) Invalidate();
            }

            // called by some outer thread driving the massage pump on incomming midi message to let start
            // applications own thread polling from the queue which the pump actually is filling up
            private void emptyMessageQueue()
            {
                if( messageReadCount == 0 ) {
                    messageReadCount = 10;
                    task().assist.GetAssistence( messageLoopTrigger );
                } else messageReadCount = 10;
            }

            protected void InitMidi( IContainer connector )
            {
                messageLoopTrigger = readMessageQueue;
                midi().InitializeComponent( this, connector, emptyMessageQueue );
                midi().input().AutomationEvent += midiDelegate();
                messageReadCount = 0;
            }
        }
    }
}

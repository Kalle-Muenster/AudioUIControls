using System;
using Stepflow.Gui;
using Stepflow.Gui.Automation;


namespace Stepflow {
namespace Midi
{
    public partial class MidiButton
        : LedButton
        , IMidiControlElement<MidiInOut>
    {
        private MidiInOut midiio;
        public delegate void MidiValueChangeDelegate(object sender, Win32Imports.Midi.Value value );
        private MidiValueChangeDelegate MidiChanged;

        AutomationlayerAddressat[] IAutomat<MidiInOut>.channels {
            get { return new AutomationlayerAddressat[] { midiio.GetAutomationBindingDescriptor(0) }; }
        }

        public MidiInOut midi() {
            return midiio;
        }
        
        MidiInputMenu<MidiInOut> IMidiControlElement<MidiInOut>.inputMenu { get; set; }
        MidiOutputMenu<MidiInOut> IMidiControlElement<MidiInOut>.outputMenu { get; set; }

        void IMidiControlElement<MidiInOut>.OnIncommingMidiControl( object sender, Win32Imports.Midi.Message newState )
        {
              State = States[(int)(newState.ProportionalFloat*NumberOfStates)];
        }

        public Win32Imports.Midi.Value MidiValue {
            get {  Win32Imports.Midi.Value val = new Win32Imports.Midi.Value(state.VAL);
                val.resolution = (ushort)NumberOfStates;
                return val; }
            set { State = States[(int)(value.resolution * value.ProportionalFloat)]; }
        }

        public MidiButton() : base()
        {
            midiio = new MidiInOut();
            midiio.InitializeComponent( this, getConnector(), Invalidate );

            Paint += midiio.input().ProcessMessageQueue;
            midiio.AutomationEvent += midiio.automat().OnIncommingMidiControl;
            Changed += MidiButton_Changed;
            MidiChanged = midi().output().OnValueChange;
            if (Text == "Gui") {
                Text = "Midi";
            }
        }

        private void MidiButton_Changed( object sender, ValueChangeArgs<Enum> data ) {
            for( int i=0; i<MaxNumState; ++i )
                if(States[i]==data.Value) {
                    MidiChanged?.Invoke(sender, new Win32Imports.Midi.Value(i));
                }
        }

        public static implicit operator bool( MidiButton cast ) {
            return (cast.state.VAL % 2) > 0;
        }

    }
}}

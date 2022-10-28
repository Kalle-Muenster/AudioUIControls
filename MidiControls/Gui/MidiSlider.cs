using Stepflow.Gui;
using Stepflow.Gui.Automation;

using MidiMessage = Win32Imports.Midi.Message;
using MidiValue = Win32Imports.Midi.Value;
using Std = Consola.StdStream;


namespace Stepflow
{
    namespace Midi
    {
        public partial class MidiSlider
        : GuiSlider
        , IMidiControlElement<MidiInOut>
    {

        static MidiSlider() {
#if DEBUG
            Std.Init( Consola.CreationFlags.TryConsole );
#endif
        }

        protected override void OnValueChanged(float val) {
#if DEBUG   
            MidiValue midivalue = MidiValue;
            midi().output().OnValueChange( this, midivalue );
            if( midiIO.MidiOut_Type != MidiMessage.TYPE.ANY )
                Std.Out.WriteLine(
                    "MidiSlider {0} sent message '{1}.Ch.{2}.CC.{3}.VL.{4}' to {5}",
                     this.Name, midi().MidiOut_Type.ToString(), midi().MidiOut_Channel.ToString(), 
                     midiIO.MidiOut_Controller.ToString(), midivalue.value.ToString(), midiIO.MidiOutPortName( midiIO.MidiOutPortID )
                );
#else
            midi().output().OnValueChange( this, MidiValue );
#endif
            base.OnValueChanged( val );
        }

        private MidiInOut  midiIO;

        /// <summary>
        /// Property 'MidiValue' (byte, 0 to 127)
        /// Get (or set) a Slider's value in proportion to the length of it's actual <parmeter>ValueRange</parameter>
        /// described by a MidiValue byte between 0 and 127 (on unsigned Sliders) or -64 to 63 (on signed Sliders)
        /// </summary>
        public MidiValue MidiValue {
            get { return new MidiValue((int)(Proportion*127)); }
            set { Proportion = value.ProportionalFloat; }
        }
        public MidiInOut midi() {
            return midiIO == null ? midiIO = new MidiInOut() : midiIO;
        }
        void IMidiControlElement<MidiInOut>.OnIncommingMidiControl( object sender, MidiMessage midicontrol ) {
             MidiValue = new MidiValue( (short)midicontrol.Value );
        }
        AutomationlayerAddressat[] IAutomat<MidiInOut>.channels {
            get { return new AutomationlayerAddressat[] { midi().GetAutomationBindingDescriptor(0) }; }
        }
        MidiInputMenu<MidiInOut> IMidiControlElement<MidiInOut>.inputMenu { get; set; }
        MidiOutputMenu<MidiInOut> IMidiControlElement<MidiInOut>.outputMenu { get; set; }

        public MidiSlider()
            : base()
        {
            midi().InitializeComponent( this, getConnector(), Invalidate );
            midi().AutomationEvent += midi().automat().OnIncommingMidiControl;
            Paint += midiIO.input().ProcessMessageQueue;           
        }
    }
}}

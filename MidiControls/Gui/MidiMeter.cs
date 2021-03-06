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



namespace Stepflow {
namespace Midi
{

    public partial class MidiMeter 
        : GuiMeter
        , IMidiControlElement<MidiInput>
    {
        protected MidiInput  midiIn;

        public IncommingAutomation<Win32Imports.Midi.Message> midiDelegate() {
            return (this as IMidiControlElement<MidiInput>).OnIncommingMidiControl;
        }
        public void OnIncommingMidiControl(object sender, Win32Imports.Midi.Message value) {
            MidiValue = new Win32Imports.Midi.Value((short)value.Value);
        }
        public Win32Imports.Midi.Value MidiValue {
            get { return new Win32Imports.Midi.Value((int)(Proportion*127)); }
            set { Proportion = value.getProportionalFloat(); }
        }
        AutomationlayerAddressat[] IAutomat.channels {
            get { return new AutomationlayerAddressat[] { midi().binding.GetAutomationBindingDescriptor(0) }; }
        }
        MidiInput IMidiControlElement<MidiInput>.binding {
            get { return midiIn; }
        }
        public IMidiControlElement<MidiInput> midi() {
            return this;
        }
        MidiInputMenu<MidiInput> IMidiControlElement<MidiInput>.inputMenu { get; set; }
        MidiOutputMenu<MidiInput> IMidiControlElement<MidiInput>.outputMenu { get; set; }

        protected MidiMeter( bool executeConstructor )
            : base(executeConstructor)
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
            (this as IInterValuable).getMenuHook().Add( new ValenceBondMenu<Controlled.Float32>( this, connector ) );

            InitMidi( connector );
        }

        protected void InitMidi( IContainer connector )
        {
            midi().binding.InitializeComponent( this, connector, Invalidate );
       //     (this as IInterValuable).getMenuHook().Add( midi().binding.midi_mnu_binding_mnu);
            midi().binding.automation().AutomationEvent += midiDelegate();
            Paint += midi().binding.automation().ProcessMessageQueue;
        //    midi().binding.midi_mnu.AutoClose = false;
        }

    }
}}

using System;
using Win32Imports;
using Win32Imports.Midi;
using MidiValue   = Win32Imports.Midi.Value;
using MidiMessage = Win32Imports.Midi.Message;
using Stepflow.Midi;

namespace Stepflow {
    namespace Gui {
        namespace Automation {

            public delegate void MidiPortChangeDelegate(object sender, int value);
            public delegate void MidiMessagesConfigured(object sender, int value);
            /// <summary>IMidiControlElement[midiinterfaceclass] (interface)
            /// Interface class to make control elements support the 'Midi' automation protocol</summary>
            /// <typeparam name="midiinterfaceclass">Class which provides accessing midi device interfaces</typeparam>
            public interface IMidiControlElement<midiinterfaceclass>
                : IAutomat<midiinterfaceclass>
            where midiinterfaceclass
                : ImportWraper
                , IAutomationProtocol
            {
                /// <summary> The current control element's value represented as Midi value </summary>
                MidiValue MidiValue { get; set; }
                void OnIncommingMidiControl( object sender, MidiMessage value );

                /// <summary> midi()
                /// Get midi functionallity of impementing control element class
                /// </summary><returns> an interface object providing the api </returns>
                midiinterfaceclass midi();
                
                /// <summary> midi().inputMenu
                /// defines a property which (when implemented) provides accessing the context
                /// menu on control elements implementing a IMidiControlElement interface type.
                /// should be initialized within a control elements Constructor or InitComponent
                /// call via creating a MidiInputMenu instance and assigning it to the property.
                ///  example: (creates on a control element a menu used for configuring midi for MidiInput only implementation)
                ///    this.midi().inputMenu = new MidiInputMenu\<MidiInput\>(this,this.components);
                ///  example: (creates a menu used for configuring all the input related parts of a MidiThru (In and Out) implementation )
                ///    this.midi().inputMenu = new MidiInputMenu\<MidiInOut\>(this,this.components);
                /// </summary>
                MidiInputMenu<midiinterfaceclass> inputMenu { get; set; }
                MidiOutputMenu<midiinterfaceclass> outputMenu { get; set; }
            };

        }
    }
}

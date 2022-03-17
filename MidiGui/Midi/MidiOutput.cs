using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Win32Imports;
using Win32Imports.Midi;
using Stepflow.Midi.ControlHelpers;
using Message = Win32Imports.Midi.Message;

namespace Stepflow.Gui.Automation
{
    public class MidiOutput 
        : Out
        , IAutomationController<Value>
    {
        public event ValueChangeDelegate<int>   OutPortChanged;
        private int                             destination;
        private IMidiControlElement<MidiOutput> element;

        internal void triggerPortChange( AutomationDirection direction, object sender, int newPort ) {
            switch( direction ) {
                case AutomationDirection.Output: OutPortChanged?.Invoke( sender, newPort ); break;
            }
        }

        public void InitializeComponent( IAutomat automatElement, IContainer automatConnector )
        {
            element = automatElement as IMidiControlElement<MidiOutput>;
            IContainer components = automatConnector;
            element.inputMenu = null;
            element.outputMenu = new Midi.MidiOutputMenu<MidiOutput>(element, components);
            OutPortChanged += element.outputMenu.OnPortChanged;
        }

        public int MidiOutDeviceID {
            get { return base.MidiOutPortID; }
            set { if ( base.MidiOutPortID != value ) {
                    OutPortChanged( this, base.MidiOutPortID = value );
                }
            }
        }
 
        public AutomationDirection direction {
            get { return AutomationDirection.Output; }
        }
 
        public bool MessageForwardingEnabled {
            get { return base.MidiOutPortThru; }
        }

        public MidiOutput() : base () {
            destination = 0;
        }


        public void OnValueChange( object sender, float value )
        {
            switch ( MidiOut_Type ) {
                case Win32Imports.Midi.Message.TYPE.CTRL_CHANGE:
                    MidiOut_Value = (int)(value * 127);
                    break;
                case Win32Imports.Midi.Message.TYPE.PITCH:
                    SendPitchChange( value );
                    break;
            } 
        }

        public void OnValueChange( object sender, Value value )
        {
             MidiOut_Value = value;
        }

        public IAutomationController<Value> automate()
        {
            return this;
        }

        public AutomationlayerAddressat GetDestinationAddressat( int channel )
        {
            return element.channels[channel];
        }

        public void ConfigureAsMessagingAutomat( AutomationlayerAddressat bindingDescriptor, int channelAutomatic )
        { 
            MidiOut_Type = (Message.TYPE)bindingDescriptor.tyByte;
            MidiOut_Channel = bindingDescriptor.loByte;
            MidiOut_Controller = bindingDescriptor.hiByte;

            if( MidiOutPortID != bindingDescriptor.dryByte )
                triggerPortChange( AutomationDirection.Output, element, bindingDescriptor.dryByte );

            if ( channelAutomatic < 0 ) {
                element.channels[channelAutomatic] = bindingDescriptor;
            } else {
                element.channels[destination] = bindingDescriptor;
            }
        }
    }
}

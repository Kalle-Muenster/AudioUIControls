using System;
using System.Collections.Generic;
using System.ComponentModel;
#if USE_WITH_WF
using System.Windows.Forms;
using System.Drawing;
#elif USE_WITH_WPF
using System.Windows.Controls;
using System.Windows.Media;
#endif
using Win32Imports;
using Win32Imports.Midi;
using Message = Win32Imports.Midi.Message;
using Stepflow.Midi.ControlHelpers;

namespace Stepflow.Gui.Automation
{
    public class MidiInOut 
        : Thru
        , IAutomationController<Value>
        , IAutomationControlable<Message>
    {
        
        public event IncommingAutomation<Message> AutomationEvent;
        public event ValueChangeDelegate<int>     InpPortChanged;
        public event ValueChangeDelegate<int>     OutPortChanged;

        private MidiController    listenToCC;
        private Queue<Message>    incoming;
        private volatile bool     learning;
        private volatile string   learnedInput;
        private IMidiControlElement<MidiInOut>  element;

#if USE_WITH_WF
        private Action invalidator;

        internal void setLearnung() {
            learning = true;
        }

        bool IAutomationControlable<Message>.messageAvailable()
        {
            return incoming.Count > 0;
        }

        internal void triggerPortChange( AutomationDirection direction, object sender, int newPort ) {
            switch( direction ) {
                case AutomationDirection.Input: InpPortChanged?.Invoke(sender, new ValueChangeArgs<int>( newPort ) ); break;
                case AutomationDirection.Output: OutPortChanged?.Invoke(sender, new ValueChangeArgs<int>( newPort )); break;
            }
        }

        public void InitializeComponent( IAutomat automatElement, IContainer automatConnector, Action automatInvalidation )
        {
            automation().invalidation = automatInvalidation;
            automate().InitializeComponent( automatElement, automatConnector );
        }

        void IAutomationController<Value>.InitializeComponent( IAutomat automatElement, IContainer automatConnector )
        {
            element = automatElement as IMidiControlElement<MidiInOut>;
            IContainer components = automatConnector;

            element.inputMenu = new Midi.MidiInputMenu<MidiInOut>(element, components);
            InpPortChanged += element.inputMenu.OnPortChanged;

            element.outputMenu = new Midi.MidiOutputMenu<MidiInOut>(element, components);
            OutPortChanged += element.outputMenu.OnPortChanged;
        }

#endif

        public new int MidiInPortID {
            get { return base.MidiInPortID; }
            set { if ( base.MidiInPortID != value) {
                    InpPortChanged(this, base.MidiInPortID = value);
                }
            }
        }
 
        public new int MidiOutPortID {
            get { return base.MidiOutPortID; }
            set { if (base.MidiOutPortID != value) {
                    OutPortChanged(this, new ValueChangeArgs<int>( base.MidiOutPortID = value ) );
                }
            }
        }

        public bool MessageForwardingEnabled {
            get { return base.MidiInPortThru && base.MidiOutPortThru; }
            set { base.MidiOutPortThru = base.MidiInPortThru = value; }
        }

        public MidiInOut() {
            incoming = new Queue<Message>(0);
            IncomingMidiMessage += automation().incommingMessagQueue;
        }


        ////////////////////////////////////////////// Output related:


        // This will create and send a midi message each 
        // time an elements 'Value' property may change
        // (always produces CC or PITCH messages, regarding
        // actual setting of the elements MidiOutType proprty)
        public void OnValueChange( object sender, float value ) 
        {
            switch ( MidiOut_Type ) {
                case Message.TYPE.POLY_PRESSURE:
                case Message.TYPE.NOTE_OFF:
                case Message.TYPE.NOTE_ON:
                    MidiOut_Value = (int)(value*127);
                    break;
                case Message.TYPE.CTRL_CHANGE:
                    MidiOut_Value = (int)(value*127);
                    break;
                case Message.TYPE.PITCH:
                    SendPitchChange( value );
                    break;
                case Message.TYPE.MONO_PRESSURE:
                    Message sent = LastMessage;
                    sent.ProportionalFloat = value;
                    SendMidiOut( sent.data.raw );
                    break;
            }
        }

        // This will send a midi message which an element
        // creates each time it's 'Value' property change
        // (gives possibillity to developers implementing
        // creation of distinct message types on their own).
        public void OnValueChange( object sender, Value value )
        {
            if( MidiOut_Type < Message.TYPE.CTRL_CHANGE )
                SendMidiOut( value.asNotationMessage(
                    MidiOut_Channel, MidiOut_Note,
                    MidiOut_Type == Message.TYPE.POLY_PRESSURE
                    ).data.raw );
            else MidiOut_Value = value;
        }

        public AutomationDirection direction {
            get { return AutomationDirection.Thruput; }
        }

        public IAutomationController<Value> automate() {
            return this;
        }

        ////////////////////////////////////////////// Input related:

        public IAutomationControlable<Message> automation() {
            return this;
        }

        Action IAutomationControlable<Message>.invalidation {
            get { return invalidator; }
            set { invalidator = value; }
        }

        void IAutomationControlable<Message>.RegisterAsMesssageListener( AutomationlayerAddressat automationlayer )
        {
            if( (listenToCC.channel == automationlayer.loShort)
             && (listenToCC.control == automationlayer.hiShort) ) return;

            Message.Filter filter = new Message.Filter(
                Message.TYPE.ANY );

            listenToCC = new MidiController( automationlayer.loShort,
                                 automationlayer.hiShort > 127 ? 0
                               : automationlayer.hiShort,
                                 automationlayer.hiShort > 127
                               ? automationlayer.hiShort : 127 );

            if ( automationlayer.tyByte != 0 ) {
                Message.TYPE type = (Message.TYPE)automationlayer.tyByte;
                switch( type ) {
                    case Message.TYPE.NOTE_ON:
                    case Message.TYPE.NOTE_OFF:
                    case Message.TYPE.POLY_PRESSURE:
                    if( automationlayer.dryByte != 0 )
                        filter = new Message.Filter( type,
                            automationlayer.loByte,
                            automationlayer.hiByte,
                            automationlayer.dryByte );
                    else filter = new Message.Filter( type,
                            automationlayer.loShort );
                    break;
                    case Message.TYPE.PROG_CHANGE:
                        filter = new Message.Filter(
                            (listenToCC.channel-1),
                             listenToCC.control );
                    break;
                    case Message.TYPE.MONO_PRESSURE:
                    case Message.TYPE.PITCH:
                        filter = new Message.Filter( type,
                             (listenToCC.channel-1) );
                    break;
                }
            } else if( listenToCC.resolut < 128 ) {
                filter = new Message.Filter( (listenToCC.channel - 1), listenToCC.control );
            } else {
                filter = new Message.Filter( Message.TYPE.PITCH, (listenToCC.channel - 1) );
            }
            
            base.UseMessageFilter( filter );
        }

        void IAutomationControlable<Message>.SignOutFromAutomationLoop()
        {
            base.RemoveAnyFilters();
        }

        // this will be called by the system's midi message dispatchig thread 
        // so any input sent, need to be stored to a queue for being later
        // rooted to the elements 'MidiInputEvent' event by some other thread 
        // owned by the application. (by maybe the GUI thread, or by any other
        // thread which the application has instanciated and started itself)
        void IAutomationControlable<Message>.incommingMessagQueue( Message message )
        {
            if(!learning ) {
                incoming.Enqueue( message );
                automation().invalidation();
            } else {
                Message.TYPE type = message.Type;
                if ( type < Message.TYPE.SYSEX && type != Message.TYPE.PROG_CHANGE ) {
                    learning = false;
                    learnedInput = string.Format( "{0}~{1}", ( message.Channel+1 ).ToString(),
                                    message.Number.ToString() );
                    automation().invalidation();
                }
            }
        }

        // this need to be called by the application for processing the
        // midi-message queue and to create MidiInputEvents (it will 
        // trigger an element's 'MidiInputEvent' for each message which
        // is contained in the queue actually.)
        void IAutomationControlable<Message>.ProcessMessageQueue( object sender, EventArgs e )
        {
            if( AutomationEvent != null ) {
                while( incoming.Count > 0 )
                    AutomationEvent( sender, incoming.Dequeue() );
            } else {
                incoming.Clear();
            }

            if( learnedInput != null ) {
                string[] chanNum = learnedInput.Split('~');
                learnedInput = null;
                if( element.inputMenu.midiIn_mnu.Visible )
                    element.inputMenu.midiIn_mnu.Close();

                AutomationlayerAddressat learnedbinding = new AutomationlayerAddressat(
                    short.Parse( chanNum[0] ), short.Parse( chanNum[1] )
                );

                automation().RegisterAsMesssageListener( learnedbinding );
            }
        }

        public string getName()
        {
            return (element as Control).Name;
        }

        // get an input binding descriptor which describes the message filter which actually is intalled
        // with the message loop. called receiver function so the control element will receive just mesages
        // of distinct types or just mesages delivering values in a distinct defined limited range
        public AutomationlayerAddressat GetAutomationBindingDescriptor( int channelAutomatic )
        {
            return new AutomationlayerAddressat( (byte)MessageFilter.Chan, (byte)MessageFilter.loTy,
                                                 (byte)MessageFilter.from, (byte)MidiInPortID );
        }

        public void ConfigureAsMessagingAutomat( AutomationlayerAddressat bindingDescriptor, int channelAutomatic )
        {
            MidiOut_Type = (Message.TYPE)bindingDescriptor.tyByte;
            MidiOut_Channel = bindingDescriptor.loByte;
            MidiOut_Controller = bindingDescriptor.hiByte;

            if( MidiOutPortID != bindingDescriptor.dryByte )
                triggerPortChange( AutomationDirection.Output, element, bindingDescriptor.dryByte );
             element.channels[channelAutomatic] = bindingDescriptor;
        }
    }
}

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
    public partial class MidiInput 
        : In
        , IAutomationControlable<Message>
    {
        public event IncommingAutomation<Message> AutomationEvent;
        public event ValueChangeDelegate<int>     InpPortChanged;
        
        private MidiController     controll;
        private Queue<Message>     incoming;
        private volatile bool      learning;
        private string             learnedInput;
        private Action             invalidator;
        private IMidiControlElement<MidiInput> element;


        internal void triggerPortChange(AutomationDirection direction,object sender, int newPort)
        {
            if( direction == AutomationDirection.Input )
                InpPortChanged?.Invoke(sender, newPort);
        }

        internal void setLearnung()
        {
            learning = true;
        }

        bool IAutomationControlable<Message>.messageAvailable()
        {
            return incoming.Count > 0;
        }

        public void InitializeComponent( IAutomat parent, IContainer parentConnector, Action parentInvalidator )
        {
            element = parent as IMidiControlElement<MidiInput>;
            IContainer components = parentConnector;
            automation().invalidation = parentInvalidator;
            element.inputMenu = new Midi.MidiInputMenu<MidiInput>(element, components);
            InpPortChanged += element.inputMenu.OnPortChanged;
        }

        public MidiInput()
            : base()
        {  
            incoming = new Queue<Message>(0);
            controll = new MidiController(-1,-1,-1);
            IncomingMidiMessage += automation().incommingMessagQueue;
            return;   
        }

        public int MidiInDeviceID {
            get { return base.MidiInPortID; }
            set { if ( base.MidiInPortID != value ) {
                    InpPortChanged( this, base.MidiInPortID = value );
                }
            }
        }

        public AutomationDirection direction {
            get { return AutomationDirection.Input; }
        }

        private void Invalidate() {
            invalidator();
        }

        Action IAutomationControlable<Message>.invalidation {
            get { return invalidator; }
            set { invalidator = value; }
        }

        public IAutomationControlable<Message> automation() {
            return this;
        }

        public void RegisterAsMesssageListener( AutomationlayerAddressat bindingDescriptor )
        {
            if( ( controll.channel == bindingDescriptor.loShort )
             && ( controll.control == bindingDescriptor.hiShort ) ) return;

            Message.Filter filter = new Message.Filter(
                Message.TYPE.ANY );

            controll = new MidiController(bindingDescriptor.loShort,
                                 bindingDescriptor.hiShort > 127 ? 0
                               : bindingDescriptor.hiShort,
                                 bindingDescriptor.hiShort > 127
                               ? bindingDescriptor.hiShort : 127);

            if( bindingDescriptor.tyByte != 0 ) {
                Message.TYPE type = (Message.TYPE)bindingDescriptor.tyByte;
                switch( type ) {
                    case Message.TYPE.NOTE_ON:
                    case Message.TYPE.NOTE_OFF:
                    case Message.TYPE.POLY_PRESSURE:
                    if( bindingDescriptor.dryByte != 0 )
                        filter = new Message.Filter( type,
                            bindingDescriptor.loByte,
                            bindingDescriptor.hiByte,
                            bindingDescriptor.dryByte);
                    else filter = new Message.Filter( type,
                            bindingDescriptor.loShort);
                    break;
                    case Message.TYPE.PROG_CHANGE:
                    filter = new Message.Filter(
                        ( controll.channel - 1 ),
                          controll.control );
                    break;
                    case Message.TYPE.MONO_PRESSURE:
                    case Message.TYPE.PITCH:
                    filter = new Message.Filter( type,
                         ( controll.channel - 1 ) );
                    break;
                }
            } else if( controll.resolut < 128 ) {
                filter = new Message.Filter( (controll.channel-1), controll.control );
            } else {
                filter = new Message.Filter( Message.TYPE.PITCH, (controll.channel-1) );
            }
            base.UseMessageFilter( filter );
        }

        public void SignOutFromAutomationLoop()
        {
            base.RemoveAnyFilters();
        }

        void IAutomationControlable<Message>.incommingMessagQueue( Message midiData )
        {
            if( !learning ) {
                incoming.Enqueue( midiData );
                automation().invalidation();
            } else { Message.TYPE type = midiData.Type;
                if ( type < Message.TYPE.SYSEX && type != Message.TYPE.PROG_CHANGE ) {
                    learning = false;
                    learnedInput = string.Format( "{0}~{1}", (midiData.Channel+1).ToString(), midiData.Number );
                    automation().invalidation();
                }
            }  
        }

        void IAutomationControlable<Message>.ProcessMessageQueue( object sender, EventArgs e )
        {
            if ( AutomationEvent != null ) {
                while ( incoming.Count > 0 )
                    AutomationEvent( sender, incoming.Dequeue() );
            } else {
                incoming.Clear();
            }
            if ( learnedInput != null ) {
                string[] chanNum = learnedInput.Split('~');
                learnedInput = null;
                if( element.inputMenu.midiIn_mnu.Visible )
                    element.inputMenu.midiIn_mnu.Close();

                AutomationlayerAddressat learnedbinding = new AutomationlayerAddressat(
                    short.Parse(chanNum[0]), short.Parse(chanNum[1])
                );

                RegisterAsMesssageListener( learnedbinding );
            }
        }

        public string getName()
        {
            return (element as Control).Name;
        }

        public AutomationlayerAddressat GetAutomationBindingDescriptor( int descriptiionChannal )
        {
            return new AutomationlayerAddressat( (byte)MessageFilter.Chan, (byte)MessageFilter.loTy,
                                                 (byte)MessageFilter.from, (byte)MidiInPortID );
        }
    }
}

#define USE_MIDI_IN
#define USE_MIDI_OUT

#if SEPARATED_MIDI_IO
#undef USE_MIDI_IN
#undef USE_MIDI_OUT
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace Win32Imports
{


    #region MIDI Interface:

#if USE_MIDI_IN || USE_MIDI_OUT || SEPARATED_MIDI_IO
    namespace Midi
    {
        #region Structures used by MidiIn AND MidiOut

        public abstract class MidiBaseClass {
            static MidiBaseClass() {
                Win32Imports.RETURN_CODE.AddErrorBase<Midi.MIDI_ERRORS>(ERROR_BASES.MIDIERR);
            }
        }

        public enum MIDI_ERRORS : uint
        {
            NO_MIDIERROR = MMSYSERR.NOERROR,
            UNPREPARED = (ERROR_BASES.MIDIERR + 0),  /* header not prepared */
            STILLPLAYING = (ERROR_BASES.MIDIERR + 1),  /* still something playing */
            NOMAP = (ERROR_BASES.MIDIERR + 2),  /* no configured instruments */
            NOTREADY = (ERROR_BASES.MIDIERR + 3),  /* hardware is still busy */
            NODEVICE = (ERROR_BASES.MIDIERR + 4),  /* port no longer connected */
            INVALIDSETUP = (ERROR_BASES.MIDIERR + 5),  /* invalid MIF */
            BADOPENMODE = (ERROR_BASES.MIDIERR + 6),  /* operation unsupported w/ open mode */
            DONT_CONTINUE = (ERROR_BASES.MIDIERR + 7),  /* thru device 'eating' a message */
            LASTERROR = MIDI_ERRORS.DONT_CONTINUE  /* last error in range */
        }

        [StructLayout(LayoutKind.Explicit, Size = 4)]
        public struct Word
        {
            [FieldOffset(0)]
            public uint raw;
            [FieldOffset(0)]
            public byte typ;
            [FieldOffset(1)]
            public byte low;
            [FieldOffset(2)]
            public byte hig;
            [FieldOffset(3)]
            public byte nix;
        }

        public enum Note : byte
        {
            C_, Cis_, D_, Dis_, E_, F_, Fis_, G_, Gis_, A_, b_, H_,
            C0, Cis0, D0, Dis0, E0, F0, Fis0, G0, Gis0, A0, b0, H0,
            C1, Cis1, D1, Dis1, E1, F1, Fis1, G1, Gis1, A1, b1, H1,
            C2, Cis2, D2, Dis2, E2, F2, Fis2, G2, Gis2, A2, b2, H2,
            C3, Cis3, D3, Dis3, E3, F3, Fis3, G3, Gis3, A3, b3, H3,
            C4, Cis4, D4, Dis4, E4, F4, Fis4, G4, Gis4, A4, b4, H4,
            C5, Cis5, D5, Dis5, E5, F5, Fis5, G5, Gis5, A5, b5, H5,
            C6, Cis6, D6, Dis6, E6, F6, Fis6, G6, Gis6, A6, b6, H6,
            C7, Cis7, D7, Dis7, E7, F7, Fis7, G7, Gis7, A7, b7, H7,
            C8, Cis8, D8, Dis8, E8, F8, Fis8, G8, Gis8, A8, b8, H8,
            C9, Cis9, D9, Dis9, E9, F9, Fis9, G9
        }

        public struct Message
        {
            [Flags]
            public enum TYPE : byte
            {
                ANY = 0,
                NOTE_OFF = 0x80,
                NOTE_ON = 0x90,
                POLY_PRESSURE = 0xA0,
                CTRL_CHANGE = 0xB0,
                PROG_CHANGE = 0xC0,
                MONO_PRESSURE = 0xD0,
                PITCH = 0xE0,
                SYSEX = 0xF0, // SYSEX with channel 0 = begin sysexdata 
                              // SYSEX with channel 7 = endof sysexdata
            };

            public enum TIME : byte // time code byte is 'SYSEX' as 4bit type part | 4bit channel part discribing the timing related message
            {
                CLOCK = TYPE.SYSEX|8,  // midiclock/metronome - should count 24 times per quarter note
                START = TYPE.SYSEX|10, // 'start' (from beginning) command or 'begin' mark of a following timecode message sequence which may be interleved within a songs note and controll change messages
                PLAY= TYPE.SYSEX|11, // 'resume' playback (when stopped before) - like 'start' but not from the begining
                STOP = TYPE.SYSEX|12, // 'stop' command or a song's timecode 'end' mark
            }

            [StructLayout(LayoutKind.Explicit, Size = 4)]
            public struct Filter
            {
                [FieldOffset(0)]
                public uint data;
                [FieldOffset(0)]
                public TYPE loTy;
                [FieldOffset(1)]
                public TYPE hiTy;
                [FieldOffset(2)]
                public ushort range;
                [FieldOffset(2)]
                public byte from;
                [FieldOffset(3)]
                public byte till;

                public Filter(TYPE filter)
                {
                    data = range = from = till = 0;
                    if (filter == TYPE.ANY)
                    {
                        loTy = TYPE.NOTE_OFF;
                        hiTy = TYPE.SYSEX;
                    }
                    else
                    if (filter <= TYPE.POLY_PRESSURE)
                    {
                        loTy = TYPE.NOTE_OFF;
                        hiTy = TYPE.POLY_PRESSURE;
                    }
                    else
                    if (filter == TYPE.CTRL_CHANGE)
                    {
                        loTy = TYPE.CTRL_CHANGE;
                        hiTy = TYPE.PITCH;
                    }
                    else
                    if (filter == TYPE.PITCH)
                    {
                        loTy = hiTy = TYPE.PITCH;
                    }
                    else
                    if (filter == TYPE.PROG_CHANGE)
                    {
                        loTy = hiTy = TYPE.PROG_CHANGE;
                    }
                    else
                    if (filter == TYPE.SYSEX)
                    {
                        loTy = hiTy = TYPE.SYSEX;
                    }
                    else
                    {
                        loTy = hiTy = TYPE.ANY;
                    }
                    range = 32768;
                    from = 0;
                    till = 128;
                }
                public Filter(TYPE a, TYPE b)
                {
                    data = from = till = 0x0;
                    range = 32768;
                    loTy = a;
                    hiTy = b;
                }
                public Filter(int chan, int numb)
                    : this(TYPE.CTRL_CHANGE)
                {
                    loTy = (TYPE)((byte)loTy + chan);
                    from = till = (byte)numb;
                }
                public Filter(TYPE type, int chan)
                    : this(type)
                {
                    loTy = (TYPE)((byte)loTy + (byte)chan);
                    range = 32768;
                }
                public Filter(TYPE type, int chan,
                       int rngLo, int rngHi)
                    : this(type)
                {
                    loTy = (TYPE)((byte)loTy + (byte)chan);
                    from = (byte)rngLo;
                    till = (byte)rngHi;
                }
                public byte Chan {
                    get { return (byte)((byte)loTy % 16); }
                }

                public Message check( Message msg )
                {
                    bool match = true;
                    TYPE type = (TYPE)(data & 0x000000f0);
                    if ( type > 0 ) {
                        if( match = msg.Type.HasFlag( type ) ) {
                            type = msg.Type;
                            match = type <= hiTy;
                        }
                    } if( match ) {
                        byte Ch = Chan;
                        if( match = Ch < 15 ? msg.Channel == Ch : true ) {
                            byte num = (byte)msg.Number;
                            match = (num >= from) ? 
                                   (from == till) ? num == till : num < till 
                                                  : false;
                        }
                    } if( !match )
                        msg.data.raw = 0;
                    return msg;
                }
            };

            public Word data;

            public TYPE Type {
                get { return (TYPE)(data.typ - (data.typ % 16)); }
                set {
                    data.typ = (byte)((byte)value|(data.nix&0x0f));
                    data.nix = (byte)(data.typ % 16);
                }
            }
            public int Channel {
                get { return data.nix; }
                set { data.typ = (byte)(Type + (data.nix = (byte)value)); }
            }
            public int Number {
                get { return data.low; }
                set { data.low = (byte)value; }
            }
            public int Value {
                get { return data.hig; }
                set { data.hig = (byte)value; }
            }
            public short HiRes {
                get { return (short)((data.hig << 8) | (data.low)); }
                set { data.low = (byte)(value%128);
                      data.hig = (byte)(value/128);
                    if( Type < TYPE.MONO_PRESSURE)
                        Type = TYPE.PITCH;
                }
            }
            public float ProportionalFloat {
                get { return ((Value)this).ProportionalFloat; }
                set { Midi.Value val = (Value)this;
                    if( val.resolution > 127 )
                        HiRes = (short)(value * val.resolution);
                    else
                        data.hig = (byte)(value * val.resolution);
                }
            }
            public Message( uint rawMessageData )
            {
                data.typ = data.low = data.hig = 0;
                data.raw = rawMessageData;
                data.nix = (byte)(data.typ % 16);
            }
            public Message( byte status, byte notenumber, byte velocityvalue )
            {
                data.raw = 0;
                data.typ = status;
                data.low = notenumber;
                data.hig = velocityvalue;
                data.nix = (byte)(status % 16);
            }
            public Message( TYPE typus ) : this()
            {
                data.raw = 0;// = new Word();
                Type = typus;
            }
            public Message(TYPE typus, int channel, int number, int value)
            {
                channel = channel == 0 ? 0xf : channel - 1;
                data.raw = 0;
                data.typ = (byte)((int)typus + channel);
                data.nix = (byte)channel;
                data.low = (byte)number;
                data.hig = (byte)value;
            }

            public static implicit operator Message( uint cast )
            {
                return new Message( cast );
            }
            public static implicit operator bool( Message cast )
            {
                return cast.data.raw > 0;
            }
            public override string ToString()
            {
                return string.Format( "{0}{1}:{2} channel {3}", 
                                      Type.ToString(), 
                                      Type < TYPE.CTRL_CHANGE 
                                           ? "."+((Note)Number).ToString() 
                                    : Type < TYPE.MONO_PRESSURE
                                           ? ".CC" + Number.ToString()
                                           : "",
                                      Type < TYPE.MONO_PRESSURE 
                                           ? Value.ToString()+ " on"
                                           : HiRes.ToString()+" for",
                                      Channel.ToString() );
            }

        };

        [StructLayout(LayoutKind.Explicit, Size = 4)]
        public struct Value
        {
            public const ushort MAX_RESOLUTION = 32767;

            [FieldOffset(0)]
            private UInt32 data;
            [FieldOffset(0)]
            public UInt16 resolution;
            [FieldOffset(2)]
            public UInt16 value;
            [FieldOffset(2)]
            public byte HiByte;
            [FieldOffset(3)]
            public byte LoByte;

            public Value(Value copy)
            {
                value = resolution = LoByte = HiByte = 0;
                data = copy.data;
            }
            public Value(int val)
            {
                data = value = LoByte = HiByte = 0;
                resolution = 127;
                value = (ushort)val;
            }
            public Value(Message.TYPE fromType, int lo, int hi)
            {
                data = value = 0;
                resolution = (ushort)(fromType <= Message.TYPE.CTRL_CHANGE ? 127 : MAX_RESOLUTION);
                LoByte = resolution > 127 ? (byte)lo : (byte)0;
                HiByte = (byte)hi;
            }
            public Value(Message msgdata)
                : this(msgdata.Type, msgdata.Number, msgdata.Value)
            { }

            public Value(int loByte, int hiByte)
            {
                value = 0;
                data = 0;
                resolution = MAX_RESOLUTION;
                LoByte = (byte)loByte;
                HiByte = (byte)hiByte;
            }
            public Value(int loByte, int hiByte, int resolution)
            {
                data = value = 0;
                this.resolution = (ushort)resolution;
                LoByte = (byte)loByte;
                HiByte = (byte)hiByte;
            }

            public Value(short hiResVal) : this(hiResVal % 256, hiResVal >> 8) { }

            public float getProportionalFloat()
            {
                return (float)value / resolution;
            }

            public float ProportionalFloat {
                get { return (float)value / resolution; }
                set { this.value = (ushort)(value * resolution); }
            }

            public static implicit operator Value(Message msg)
            {
                return new Value( msg.data.raw );
            }
            public static implicit operator int(Value cast)
            {
                return cast.value;
            }
            public static implicit operator Value(int cast)
            {
                return new Value(cast);
            }
            public static implicit operator float(Value cast)
            {
                return cast.getProportionalFloat();
            }
            public Message asControlMessage( int channel )
            {
                return new Message( Message.TYPE.PITCH, channel, HiByte, LoByte );
            }
            public Message asControlMessage( int channel, int number )
            {
                if( resolution > 127 ) return asControlMessage( channel );
                else return new Message( Message.TYPE.CTRL_CHANGE, channel, number, LoByte );
            }
            public Message asNotationMessage( int channel, Note note, bool on )
            {
                return new Message((byte)((byte)(on?Message.TYPE.NOTE_ON:Message.TYPE.NOTE_OFF)+(channel-1)), (byte)note, LoByte);
            }
            
        };

        public abstract class ImportWraper : MidiBaseClass
        {
            protected static List<ImportWraper> inputinstances;
            protected static List<ImportWraper> outputinstances; 
            static ImportWraper()
            {
                RETURN_CODE.AddErrorBase<MIDI_ERRORS>(ERROR_BASES.MIDIERR);
                RETURN_CODE.LogAnyResult = false;
                RETURN_CODE.ActivLogging = false;
                inputinstances = new List<ImportWraper>();
                outputinstances = new List<ImportWraper>();
            }

            protected RETURN_CODE result;
            internal bool         midiThru;
            internal UInt16       inPortID;
            internal UInt16       outPortID;
            internal ImportWraper outputOwner;
            internal ImportWraper inputOwner;
            internal IntPtr       hmidiin;
            internal IntPtr       hmidiout;


            public RETURN_CODE getLastError()
            {
                return result;
            }

        };

        #endregion

#if USE_MIDI_OUT || SEPARATED_MIDI_IO
        public class OutputBase
        {
            #region Structure Definitions

            public enum MIDI_OUT_MESSAGE : uint
            {
                OPEN = 0x3C7,
                CLOSE = 0x3C8,
                DONE = 0x3C9
            }

            [StructLayout(LayoutKind.Sequential, Size = 53, CharSet = CharSet.Ansi)]
            public struct MIDIOUTCAPS
            {
                public UInt16 wMid;
                public UInt16 wPid;
                public UInt32 vDriverVersion;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string szPname;
                public UInt16 wTechnology;
                public UInt16 wVoices;
                public UInt16 wNotes;
                public UInt16 wChannelMask;
                public UInt32 dwSupport;
            }

            #endregion



            //#if USE_MIDI_OUT
            public abstract class Wrapper : ImportWraper
            {
                #region Dll imports:
                [DllImport("winmm.dll")]
                private static extern RETURN_CODE midiOutShortMsg(IntPtr devicehandle, UInt32 msg);
                [DllImport("winmm.dll")]
                private static extern      UInt16 midiOutGetNumDevs();
                [DllImport("winmm.dll")]
                private static extern RETURN_CODE midiOutOpen(out IntPtr devicehandle, UInt16 midiid, UInt32 nullnix, UInt32 garnix, UInt32 nixnix);
                [DllImport("winmm.dll")]
                private static extern RETURN_CODE midiOutClose(out IntPtr devicehandle);
                [DllImport("winmm.dll")]
                private static extern RETURN_CODE midiOutGetID(IntPtr hmo, out UInt16 puDeviceID);
                [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
                private static extern RETURN_CODE midiOutGetDevCaps(UInt16 uMidiOutID, out MIDIOUTCAPS outputCapacitiez, UInt32 sizeofDOTNET_MIDIOUTCAPS);
                [DllImport("winmm.dll")]
                private static extern RETURN_CODE midiOutReset(UInt16 uMidiOutID);
                #endregion

                #region public api:
                private MIDIOUTCAPS  midiOutCAPS;

                private Message      out_message;
                public Message LastMessage {
                    get { return out_message; }
                }

                public Wrapper()
                {
                    this.midiOutCAPS.szPname = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
                    this.midiOutCAPS.vDriverVersion =
                    this.midiOutCAPS.dwSupport =
                    this.midiOutCAPS.wChannelMask =
                    this.midiOutCAPS.wTechnology =
                    this.midiOutCAPS.wVoices =
                    this.midiOutCAPS.wNotes =
                    this.midiOutCAPS.wMid =
                    this.midiOutCAPS.wPid =
                    this.outPortID = 0;
                    this.out_message = new Message(Message.TYPE.CTRL_CHANGE, 1, 0, 0);
                    if (outputinstances.Count == 0) {
                        outPortID = ushort.MaxValue;
                        outputOwner = this;
                    } else unsafe {
                        outputOwner = outputinstances[0];
                        outPortID = outputOwner.outPortID;
                        hmidiout = outputOwner.hmidiout;
                    } outputinstances.Add( this );
                }

                public int NumberOfMidiOutPorts {
                    get { return (int)midiOutGetNumDevs(); }
                }
                public Message.TYPE MidiOut_Type {
                    get { return out_message.Type; }
                    set { out_message.Type = value; }
                }
                public int MidiOut_Channel {
                    get { return out_message.Channel + 1; }
                    set { out_message.Channel = (value - 1); }
                }
                public int MidiOut_Controller {
                    get { return out_message.Number; }
                    set { out_message.Number = value; }
                }
                public Note MidiOut_Note {
                    get { return (Note)out_message.Number; }
                    set { out_message.Number = (byte)value;  }
                }
                public int MidiOut_Value {
                    get { return out_message.Value; }
                    set { out_message.Value = value;
                        SendMidiOut( out_message.data.raw );
                    }
                }

                public bool MidiOutPortThru {
                    get { return outputOwner.midiThru; }
                    set { outputOwner.midiThru = value; }
                }

                public virtual int MidiOutPortID {
                    get { return (int)(outPortID = outputOwner.outPortID); }
                    set {
                        ushort setNewPort = (ushort)value;
                        if (setNewPort == outPortID)
                        {
                            if (outputOwner.outPortID == setNewPort)
                            {
                                return;
                            }
                        }
                        bool allreadyOpen = false;
                        for (int i = 0; i < outputinstances.Count; ++i)
                        {
                            if (outputinstances[i].outPortID == setNewPort) unsafe
                                {
                                    outputOwner = outputinstances[i].outputOwner;
                                    hmidiout = new IntPtr(outputOwner.hmidiout.ToPointer());
                                    allreadyOpen = true;
                                    break;
                                }
                        }
                        if (allreadyOpen)
                        {
                            return;
                        }
                        if (midiOutGetNumDevs() > setNewPort)
                        {
                            result = midiOutReset(setNewPort);
                            result = midiOutReset(setNewPort);
                            result = midiOutClose(out hmidiout);
                            if (result = midiOutOpen(out hmidiout, setNewPort, 0, 0, 0))
                                outPortID = setNewPort;
                            else
                            {
                                result.log(string.Format("ERROR: can't open midi output port '{0}'", outPortID));
                                result = midiOutOpen(out hmidiout, outPortID, 0, 0, 0);
                            }
                            result.log(string.Format("MidiOutput instance now owns own output port {0}-{1} ",
                                                    outPortID.ToString(), MidiOutPortName(outPortID))
                                                        );
                            outputOwner = this;
                        }
                    }
                }

                public string MidiOutPortName(int devnum)
                {
                    if (midiOutGetNumDevs() > 0)
                    {
                        if (result = midiOutGetDevCaps((UInt16)devnum, out midiOutCAPS, (uint)Marshal.SizeOf(midiOutCAPS)))
                            return midiOutCAPS.szPname;
                        else return result.ToString();
                    }
                    return "INVALID DEVICE";
                }


                public bool SendMidiOut( uint word )
                {
                    if ( outPortID != ushort.MaxValue ) {
                         return midiOutShortMsg(hmidiout, word).log("Message sent!");
                    } return false;
                }
                protected bool SendMidiOut( byte a, byte b, byte c )
                {
                    out_message = new Message( a, b, c );
                    return SendMidiOut( out_message.data.raw );
                }

                public void SendOutMessages( Message[] messages )
                {
                    int outcount = 0;
                    while( outcount < messages.Length )
                        SendMidiOut( (out_message = messages[outcount++]).data.raw );
                }
                public void SendController( int channel, int number, int value )
                {
                    MidiOut_Type = Message.TYPE.CTRL_CHANGE;
                    MidiOut_Channel = channel;
                    MidiOut_Controller = number;
                    MidiOut_Value = value;
                }
                public void SendController( int number, int value )
                {
                    if( MidiOut_Type != Message.TYPE.CTRL_CHANGE )
                        MidiOut_Type = Message.TYPE.CTRL_CHANGE;
                    MidiOut_Controller = number;
                    MidiOut_Value = value;
                }
                public void SendControlChange( sbyte value )
                {
                    MidiOut_Value = value;
                }
                public void SendControlChange( Value value )
                {
                    if( value.resolution > 127 )
                        out_message.Number = value.HiByte;
                    out_message.Value = value.LoByte;
                    SendMidiOut( out_message.data.raw );
                }


                public void SendNote( Note note, int value )
                {
                    if (value > 0)
                        SendNoteOn( MidiOut_Channel, note, value );
                    else
                        SendNoteOff( MidiOut_Channel, note );
                }

                public void SendNoteOn( int channel, Note note, int velocity )
                {
                    out_message.Type = Message.TYPE.NOTE_ON;
                    MidiOut_Channel = channel;
                    MidiOut_Note = note;
                    MidiOut_Value = velocity;
                }
                public void SendNoteOff( int channel, Note note )
                {
                    MidiOut_Type = Message.TYPE.NOTE_OFF;
                    MidiOut_Channel = channel;
                    MidiOut_Note = note;
                    MidiOut_Value = 0;
                }


                // send hi-res controller (value: double -1 to 1)
                public void SendPitchBend( int channel, double value )
                {
                    int twoBytes = (int)((value * 8192) + 8191);
                    out_message = new Message( (byte)((int)Message.TYPE.PITCH + (channel - 1)),
                                               (byte)(twoBytes % 128),
                                               (byte)(twoBytes / 128) );
                    SendMidiOut( out_message.data.raw );
                }
                public void SendPitchChange( float value )
                {
                    int data = (int)((value*8192)+8191);
                    out_message = new Message( (byte)((int)Message.TYPE.PITCH + out_message.Channel),
                                               (byte)(data % 128),
                                               (byte)(data / 128) );
                    SendMidiOut( out_message.data.raw );
                }
                #endregion
            }
            //#endif
        };
#endif

#if USE_MIDI_IN || SEPARATED_MIDI_IO
        public class InputBase
        {
            #region Structure Definitions

            [Flags]
            public enum OPEN_FLAGS : uint
            {
                NO_CALLBACK = 0x00000000,
                MIDI_IO_STATUS = 0x00000020,
                CALLBACK_WINDOW = CALLBACK.WINDOW,
                CALLBACK_THREAD = CALLBACK.THREAD,
                CALLBACK_FUNCTION = CALLBACK.FUNCTION,
                CALLBACK_EVENT = CALLBACK.EVENT
            }

            public enum MIDI_IN_MESSAGE : uint
            {
                OPEN = 0x3C1,
                CLOSE = 0x3C2,
                DATA = 0x3C3,
                LONGDATA = 0x3C4,
                ERROR = 0x3C5,
                LONGERROR = 0x3C6,
            }

            [StructLayout(LayoutKind.Sequential, Size = 45, CharSet = CharSet.Ansi)]
            public struct MIDIINCAPS
            {
                public UInt16 wMid;
                public UInt16 wPid;
                public UInt32 vDriverVersion;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string szPname;
                public UInt32 dwSupport;
            }

            /* MIDI data block header */
            [StructLayout(LayoutKind.Sequential, Size = 64, CharSet = CharSet.Ansi)]
            public struct MIDI_HEADER
            {
                public UIntPtr lpData;                  /* pointer to locked data block */
                public UInt32 dwBufferLength;           /* length of data in data block */
                public UInt32 dwBytesRecorded;          /* used for input only */
                public UIntPtr dwUser;                  /* for client's use */
                public UInt32 dwFlags;                  /* assorted flags (see defines) */
                public IntPtr lpNext;                   /* reserved for driver */
                public UIntPtr reserved;                    /* reserved for driver */
                public UInt32 dwOffset;                 /* Callback offset into buffer */
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public UIntPtr[] dwReserved;                /* Reserved for MMSYSTEM */
            }
            #endregion

            #region Dll Imports
            [DllImport("winmm.dll")]
            protected static extern
                UInt16 midiInGetNumDevs();

            [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
            protected static extern
                RETURN_CODE midiInGetDevCaps(UInt32 uDeviceID,
                                              out MIDIINCAPS lpMidiInCaps,
                                              UInt32 cbMidiInCaps);
            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInGetErrorText(MIDI_ERRORS mmrError,
                                                out string pszText,
                                                UInt32 cchText);
            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInOpen(out System.IntPtr lphMidiIn,
                                        ushort uDeviceID, System.IntPtr dwCallback,
                                        ref UIntPtr dwCallbackInstance, OPEN_FLAGS dwFlags);
            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInClose(System.IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInPrepareHeader(System.IntPtr hmi,
                                                 ref MIDI_HEADER pmidiheader,
                                                 UInt32 sizeofmidiheader);
            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInUnprepareHeader(System.IntPtr hmi,
                                                   ref MIDI_HEADER pmh,
                                                   UInt32 sizeofheader);
            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInAddBuffer(System.IntPtr hmi,
                                             ref MIDI_HEADER pmidiheader,
                                             UInt32 sizeofmidiheader);
            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInStart(System.IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInStop(System.IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInReset(System.IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            protected static extern
                RETURN_CODE midiInGetID(System.IntPtr hmi,
                                         out UInt16 puDeviceID);
            #endregion


#if SEPARATED_MIDI_IO
            public abstract class Wrapper : ImportWraper
            {
            #region private static:
                private static RETURN_CODE Result;
                private delegate void MidiDelegate( IntPtr hmidiin, MIDI_IN_MESSAGE wMsg,
													ref UIntPtr dwData, UIntPtr dwParam1, 
													UIntPtr dwParam2 );
                private MidiDelegate MidiInHandler;
                private IntPtr callbackFuncPt;
                internal void InvokeMidiEvent(UInt32 message)
                {
                    if( IncomingMidiMessage != null ) {
                        Message msg = prefilter.check( message );
                        if( msg )
                            IncomingMidiMessage( msg );
                    }
                }
                private void MidiInCalback( IntPtr hmidiin, MIDI_IN_MESSAGE wMsg,
                                            ref UIntPtr dwData, UIntPtr dwParam1,
                                            UIntPtr dwParam2 )
                {
#if WIN64
                    Int64 portHandle = hmidiin.ToInt64();
#else
                    Int32 portHandle = hmidiin.ToInt32();
#endif
                    switch (wMsg)
                    {
                            case MIDI_IN_MESSAGE.OPEN:
                                Result.log("MidiInput: port-handle " + portHandle + ": OPEN");
                                break;
                            case MIDI_IN_MESSAGE.ERROR:
                                Result = dwParam1.ToUInt32();
                                break;
                            case MIDI_IN_MESSAGE.DATA: unsafe {
                                    foreach ( ImportWraper WRAPPER in inputinstances )
                                        if( WRAPPER.hmidiin.ToPointer() == hmidiin.ToPointer() ) {
                                            if(WRAPPER is InputBase.Wrapper) {
                                              (WRAPPER as InputBase.Wrapper).InvokeMidiEvent( dwParam1.ToUInt32() );
                                            } else {
                                              (WRAPPER as InOutBase.Wrapper).InvokeMidiEvent( dwParam1.ToUInt32() );
                                            }
                                        }
                                } break;
                            case MIDI_IN_MESSAGE.CLOSE:
                                Result.log("MidiInput: port-handle " + portHandle + ": CLOSED");
                                break;
                            case MIDI_IN_MESSAGE.LONGDATA:
                                Result.log("MidiInput: port-handle " + portHandle + ": SYSEX");
                                break;
                            case MIDI_IN_MESSAGE.LONGERROR:
                                Result.log("MidiInput: port-handle " + portHandle + ": SYSEX-ERROR");
                                break;
                     }
                }

            #endregion

            #region public api:
                public delegate void IncommingMidiData( Message midiData );
                public event IncommingMidiData IncomingMidiMessage;
                internal protected MIDI_HEADER midiHeader;
                private Message.Filter prefilter;
                private MIDIINCAPS midiinCAPS;
				private ulong midiindata;
				private uint sizeOfHeader;
                
                public Wrapper() : base()
                {
                    prefilter = new Message.Filter(Message.TYPE.ANY);
                    this.midiinCAPS.szPname = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
                    this.midiinCAPS.dwSupport = this.midiinCAPS.vDriverVersion =
                    this.midiinCAPS.wMid = this.midiinCAPS.wPid = ushort.MaxValue;
                    if ( inputinstances.Count == 0 ) {
                        inputOwner = this;
                        inPortID = ushort.MaxValue;
                    } else {
                        inputOwner = inputinstances[0];
                        inPortID = inputOwner.inPortID;
                    }
                    this.midiHeader = new MIDI_HEADER();
                    unsafe {
                        MidiInHandler = MidiInCalback;
                        callbackFuncPt = Marshal.GetFunctionPointerForDelegate( MidiInHandler );
                        midiHeader.lpData = new UIntPtr( Marshal.GetIUnknownForObject(this).ToPointer() );
                        midiHeader.dwFlags = 0;
                        midiHeader.dwBufferLength = 8;
                        sizeOfHeader = (uint)Marshal.SizeOf( midiHeader );
                    } inputinstances.Add( this );
                    return;
                }
                
                public Message.Filter MessageFilter {
                    get { return prefilter; }
                    set { prefilter = value; }
                }
                public void UseMessageFilter( Message.Filter filter )
                {
                    prefilter = filter;
                }
                public void RemoveAnyFilters() 
                {
                    prefilter = new Message.Filter( Message.TYPE.ANY );
                }

                public int NumberOfMidiInPorts
                { get { return (int)midiInGetNumDevs(); } }

                public string MidiInPortName(int ID)
                {
                    if (midiInGetNumDevs() > 0) {
                        MIDIINCAPS caps;
                        if(result = midiInGetDevCaps( (ushort)ID, out caps, (uint)Marshal.SizeOf<MIDIINCAPS>()) ) 
							return caps.szPname;
						else
							return result.ToString();
                    } return "INVALID DEVICE";
                }

                public bool MidiInPortThru {
                    get { return inputOwner.midiThru; }
                    set { inputOwner.midiThru = value; }
                }

                public virtual int MidiInPortID
                {
                    get { this.inPortID = (ushort)inputOwner.inPortID;
                          return inPortID; }
                    set { ushort setInPort = (ushort)value;
                        if ( setInPort == inPortID ) {
                            if ( inputOwner.inPortID == setInPort ) {
                                return;
                            }
                        }
                        bool allreadyOpen = false;
                        for ( int i = 0; i < inputinstances.Count; ++i ) {
                            if ( inputinstances[i].inPortID == setInPort ) unsafe {
                                inputOwner = inputinstances[i].inputOwner;
                                hmidiin = new IntPtr(inputOwner.hmidiin.ToPointer());
                                allreadyOpen = true;
                                break;
                            }
                        }
                        if ( allreadyOpen ) {
                            return;
                        }
                        if ( midiInGetNumDevs() > setInPort ) {
                            result = midiInStop(this.hmidiin);
                            result = midiInReset(this.hmidiin);
                            result = midiInReset(this.hmidiin); // reset twice to trigger midi panic.  
                            result = midiInClose(this.hmidiin);

                            result = midiInUnprepareHeader(this.hmidiin, ref this.midiHeader, this.sizeOfHeader);
                            result = midiInPrepareHeader(this.hmidiin, ref this.midiHeader, this.sizeOfHeader);

                            if ( midiInOpen( out this.hmidiin, setInPort, this.callbackFuncPt, ref this.midiHeader.lpData, OPEN_FLAGS.CALLBACK_FUNCTION ).log() )
                                inPortID = setInPort;
                            else
                                midiInOpen( out this.hmidiin, this.inPortID, this.callbackFuncPt, ref this.midiHeader.lpData, OPEN_FLAGS.CALLBACK_FUNCTION ).log();
                            result = midiInStart( this.hmidiin ).log( string.Format( "MidiInputInstance: now owns own device {0}-{1}",
                                                                      this.inPortID.ToString(), this.MidiInPortName( this.inPortID ) ) );
                            inputOwner = this;
                        }
                    }
                }
                #endregion
            };
#endif
        };
#endif

#if SEPARATED_MIDI_IO || (USE_MIDI_IN && USE_MIDI_OUT)
        public class InOutBase : InputBase
        {
            public new abstract class Wrapper : OutputBase.Wrapper
            {
                #region private static:
                private static RETURN_CODE Result;
                private delegate void MidiDelegate(IntPtr hMidiIn, MIDI_IN_MESSAGE wMsg,
                                                    ref UIntPtr dwData, UIntPtr dwParam1,
                                                    UIntPtr dwParam2);
                private MidiDelegate MidiInHandler;
                private IntPtr callbackFuncPt;
                internal void InvokeMidiEvent( UInt32 message )
                {
                    if( IncomingMidiMessage != null ) {
                        Message msg = prefilter.check( message );
                        if( msg )
                            IncomingMidiMessage( msg );
                    }
                }
                private void MidiInCalback( IntPtr hMidiIn, MIDI_IN_MESSAGE wMsg,
                                            ref UIntPtr dwData, UIntPtr dwParam1,
                                            UIntPtr dwParam2 )
                {
#if WIN64
                    Int64 portHandle = hMidiIn.ToInt64();
#else
                    Int32 portHandle = hMidiIn.ToInt32();
#endif
                    switch (wMsg)
                    {
                        case MIDI_IN_MESSAGE.OPEN:
                            Result.log("MidiInput: port-handle " + portHandle + ": OPEN");
                            break;
                        case MIDI_IN_MESSAGE.ERROR:
                            Result = dwParam1.ToUInt32();
                            break;
                        case MIDI_IN_MESSAGE.DATA: unsafe {
                            foreach( ImportWraper WRAPPER in inputinstances )
                                if( WRAPPER.hmidiin.ToPointer() == hmidiin.ToPointer() ) {
                                    if( WRAPPER is InOutBase.Wrapper ) {
                                        ((InOutBase.Wrapper)WRAPPER).InvokeMidiEvent( dwParam1.ToUInt32() );
                                    } else {
                                        ((InputBase.Wrapper)WRAPPER).InvokeMidiEvent( dwParam1.ToUInt32() );
                                    }
                                }
                          } break;
                        case MIDI_IN_MESSAGE.CLOSE:
                            Result.log("MidiInput: port-handle " + portHandle + ": CLOSED");
                            break;
                        case MIDI_IN_MESSAGE.LONGDATA:
                            Result.log("MidiInput: port-handle " + portHandle + ": SYSEX");
                            break;
                        case MIDI_IN_MESSAGE.LONGERROR:
                            Result.log("MidiInput: port-handle " + portHandle + ": SYSEX-ERROR");
                            break;
                    }
                }
                #endregion

                #region public api:
                public delegate void IncommingMidiData(Message midiData);
                public event IncommingMidiData IncomingMidiMessage;
                internal protected MIDI_HEADER midiHeader;
                private Message.Filter prefilter;
                private MIDIINCAPS midiinCAPS;
                private ulong midiindata;
                private uint sizeOfHeader;


                public Wrapper() : base()
                {
                    prefilter = new Message.Filter(Message.TYPE.ANY);
                    this.midiinCAPS.szPname = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
                    this.midiinCAPS.dwSupport = this.midiinCAPS.vDriverVersion =
                    this.midiinCAPS.wMid = this.midiinCAPS.wPid = ushort.MaxValue;
                    if (inputinstances.Count == 0)
                    {
                        inputOwner = this;
                        inPortID = ushort.MaxValue;
                    }
                    else
                    {
                        inputOwner = inputinstances[0];
                        inPortID = inputOwner.inPortID;
                    }
                    this.midiHeader = new MIDI_HEADER();
                    unsafe
                    {
                        MidiInHandler = MidiInCalback;
                        callbackFuncPt = Marshal.GetFunctionPointerForDelegate(MidiInHandler);
                        midiHeader.lpData = new UIntPtr(Marshal.GetIUnknownForObject(this).ToPointer());
                        midiHeader.dwFlags = 0;
                        midiHeader.dwBufferLength = 8;
                        sizeOfHeader = (uint)Marshal.SizeOf(midiHeader);
                    }
                    inputinstances.Add(this);
                    return;
                }

                public Message.Filter MessageFilter {
                    get { return prefilter; }
                    set { prefilter = value; }
                }
                public void UseMessageFilter(Message.Filter filter)
                {
                    prefilter = filter;
                }
                public void RemoveAnyFilters()
                {
                    prefilter = new Message.Filter(Message.TYPE.ANY);
                }

                public int NumberOfMidiInPorts { get { return (int)midiInGetNumDevs(); } }

                public string MidiInPortName(int ID)
                {
                    if (midiInGetNumDevs() > 0)
                    {
                        if (result = midiInGetDevCaps( (ushort)ID, out midiinCAPS, (uint)Marshal.SizeOf<MIDIINCAPS>() ) )
                            return midiinCAPS.szPname;
                        else
                            return result.ToString();
                    }
                    return "INVALID DEVICE";
                }

                public bool MidiInPortThru {
                    get { return inputOwner.midiThru; }
                    set { inputOwner.midiThru = value; }
                }

                public virtual int MidiInPortID {
                    get {
                        this.inPortID = (ushort)inputOwner.inPortID;
                        return inPortID;
                    }
                    set {
                        ushort setInPort = (ushort)value;
                        if (setInPort == inPortID)
                        {
                            if (inputOwner.inPortID == setInPort)
                            {
                                return;
                            }
                        }
                        bool allreadyOpen = false;
                        for (int i = 0; i < inputinstances.Count; ++i)
                        {
                            if (inputinstances[i].inPortID == setInPort) unsafe
                                {
                                    inputOwner = inputinstances[i].inputOwner;
                                    hmidiin = new IntPtr(inputOwner.hmidiin.ToPointer());
                                    allreadyOpen = true; break;
                                }
                        }
                        if (allreadyOpen)
                        {
                            return;
                        }
                        if (midiInGetNumDevs() > setInPort)
                        {
                            result = midiInStop(this.hmidiin);
                            result = midiInReset(this.hmidiin);
                            result = midiInReset(this.hmidiin);
                            result = midiInClose(this.hmidiin);

                            result = midiInUnprepareHeader(this.hmidiin, ref this.midiHeader, this.sizeOfHeader);
                            result = midiInPrepareHeader(this.hmidiin, ref this.midiHeader, this.sizeOfHeader);

                            if ( midiInOpen(out this.hmidiin, setInPort, this.callbackFuncPt, ref this.midiHeader.lpData, OPEN_FLAGS.CALLBACK_FUNCTION).log() )
                                inPortID = setInPort;
                            else
                                midiInOpen(out this.hmidiin, this.inPortID, this.callbackFuncPt, ref this.midiHeader.lpData, OPEN_FLAGS.CALLBACK_FUNCTION).log();
                            result = midiInStart( this.hmidiin ).log( string.Format("MidiIOInstance: now owns own device {0}-{1}",
                                                                      this.inPortID.ToString(), this.MidiInPortName(this.inPortID)) );
                            inputOwner = this;
                        }
                    }
                }
                #endregion
            };
        };
#endif

        #region Midi.Wrapper (use this for deriving midi in/out implementations)
#if SEPARATED_MIDI_IO
        public abstract class In      : InputBase.Wrapper {};
        public abstract class Out     : OutputBase.Wrapper {};
        public abstract class Thru    : InOutBase.Wrapper {};
#elif USE_MIDI_OUT && USE_MIDI_IN
        public abstract class Wrapper : InOutBase.Wrapper { };
#elif USE_MIDI_OUT
        public abstract class Wrapper : OutputBase.Wrapper {};
#elif USE_MIDI_IN
        public abstract class Wrapper : InputBase.Wrapper {};
#endif
        #endregion
    }
#endif

    #endregion
}


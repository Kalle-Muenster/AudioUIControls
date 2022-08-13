using System;
using Stepflow;
using System.Collections.Generic;
#if   USE_WITH_WF
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rect = System.Drawing.Rectangle;
using RectF = System.Drawing.RectangleF;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rect = System.Windows.Int32Rect;
using RectF = System.Windows.Rect;
#endif
using Win32Imports;
using Win32Imports.Midi;
using Message = Win32Imports.Midi.Message;
using System.Runtime.InteropServices;

namespace Stepflow.Midi.ControlHelpers
{
        public static class Frequency
        {
            private static float[] tunes;
            private static Message detune;
            private static bool detuneByCC;
            private static bool detuneByPP;
            private static Value detuneation;

            static Frequency()
            {
                tunes = new float[128];
                setBaseTunes( new double[] {
                    261.6255653006,277.1826309769,293.6647679174,
                    331.1269837221,329.6275569129,349.2282314330,
                    369.9944227116,391.9954359817,415.3046975799,
                    440.0000000000,466.1637615181,493.8833012561}
                );
                setDetunationType( Message.TYPE.POLY_PRESSURE );
            }

            /// <summary> Frequency.setBaseTunes( basetunes[12] )
            /// Tune the application via setting a new base tune table.
            /// which contains base frequencies of all notes of the
            /// (4th)'octave. (12 halftones, ranging from C4 to H4) 
            /// </summary><param name="basetable"></param>
            public static void setBaseTunes( double[] basetable )
            {
                if( basetable.Length != 12 )
                    throw new Exception(
                "tuning table must be given 12 elementar tunes"
                                          );
                double[] tunings = basetable.Clone() as double[];
                for( int t = 0; t < 12; ++t )
                    tunes[60 + t] = (float)tunings[t];
                for(int o = 4; o >= 0; --o ) {
                    int O = 12 * o;
                    for(int t = 0; t < 12; ++t ) {
                        int n = O + t;
                        tunes[n] = (float)(tunings[t] /= 2.0);
                    }
                } tunings = basetable.Clone() as double[];
                for(int o = 6; o < 11; ++o ) {
                    int O = 12 * o;
                    for(int t = 0; t < 12; ++t ) {
                        int n = O + t;
                        if (n == 128) break;
                        tunes[n] = (float)(tunings[t] *= 2.0);
                    }
                }
            }

            /// <summary> Frequency.setDetunationType( Message TYPE, cc number )
            /// Define how the application should apply detunation to played midi notes
            /// when tones to be played won't match any existing midi note exactly </summary>
            /// <param name="type"> MidiMesage type to be generated for applying detunation.
            /// When type 'ControlChange' is given, a second parameter then becomes mandatory.
            /// </param><param name="ccnum">Defines the controller number to be used when for
            /// message type 'ControlChange' was given as first parameter. It discribes which
            /// controller number should to be send to the performing syntheszizer when this
            /// is configured to apply detuneation to the used instrument via that controller
            /// </param>
            public static void setDetunationType( Message.TYPE type, byte parameter )
            {   
                switch( type ) {
                case Message.TYPE.PITCH: 
                     detuneation = new Value((short)8192);
                     detune = detuneation.asControlMessage( parameter );
                     detuneByCC = true;
                     detuneByPP = false; break;
                case Message.TYPE.CTRL_CHANGE:
                     detune = new Message(type,0,parameter,0);
                     detuneation = new Value((short)127);
                     detuneByCC = true;
                     detuneByPP = false; break;
                case Message.TYPE.POLY_PRESSURE: 
                     detune = new Message(type,parameter,0,0);
                     detuneation = new Value((short)127);
                     detuneByCC = false;
                     detuneByPP = true; break;
                case Message.TYPE.MONO_PRESSURE:
                     detuneation = new Value((short)8192);
                     detune = detuneation.asControlMessage( parameter );
                     detune.Type = type;
                     detuneByPP = detuneByCC = false; break;
                default: throw new Exception(
                    "type is not capable controlling detuneation"
                                                );
                }
            }
            /// <summary> Frequency.setDetunationType( Message TYPE )
            /// Define how the application should apply detunation to played midi notes
            /// when tones to be played won't match any existing midi note exactly </summary>
            /// <param name="type"> MidiMesage type to be generated for applying detunation.
            /// possible values are: PITCH, POLY_PRESSURE, MONO_PRESSURE. For using CTRL_CHANGE
            /// messages the other overload with second 'CCnum' parameter is to be called
            /// instead, otherwise a 'MissingParameter' Exception will be thrown </param>
            public static void setDetunationType( Message.TYPE type )
            {
                if( type == Message.TYPE.CTRL_CHANGE) {
                     throw new Exception(
                     "missing cc number parameter for detunation"
                                           );
                }
                setDetunationType( type, 0 );
            }

            /// <summary> Frequency.fromMidiNote(Note)
            /// returns frequency (in Hz) which given midi note parameter represents </summary>
            /// <param name="note"> Midi note where to retreive represented frequency from </param>
            /// <returns> frequency value in Hz </returns>
            public static float fromMidiNote( Note note )
            {
                return tunes[(int)note];
            }
            /// <summary> Frequency.fromMidiNote(Note,Fine)
            /// returns frequency (in Hz) of given midi note when some additional amout on pitch would be applied.
            /// </summary><param name="note"> Midi note where to retreive represented frequency from </param>
            /// <param name="fine"> Pitch amount applied to that channel where given note is playing actually.
            /// </param><returns> frequency value in Hz </returns>
            public static float fromMidiNote( Note note, float detuned )
            {
                float q = tunes[(int)note];
                return q + ( (tunes[(int)++note]-q) * detuned );
            }

            /// <summary> Frequency.toMidiData(frequencyHz)
            /// Create a midi messages (or maybe several messages) which plays a note that matches requested Hz frequency
            /// When given frequency does not exactly match any midi note's frequency value, an additional control change
            /// message will be generated which will apply detunation (via pitch or aftertouch) to that channel where the
            /// generated note will be played to make the played tone matching that requested frequency value exactly.
            /// </summary> <param name="frequencyHz"></param><returns> An array of midi messages (where length can be
            /// either one or two messages) </returns>
            public static Message[] toMidiData( float frequencyHz )
            {
                return toMidiData( frequencyHz, 1.0f );
            }
            /// <summary> Frequency.toMidiData(frequencyHz,volumeDb)
            /// Create a midi messages (or maybe several messages) which plays a note at given volume, which tune matches the 
            /// requested Hz frequency parameter. When given frequency does not exactly match any midi note's frequency value,
            /// an additional control change message will be generated which will apply detunation (via pitch or aftertouch)
            /// to that channel where the generated note is going to be played for making it matching the requested frequency
            /// value exactly. </summary> <param name="frequencyHz"></param><returns> An array of midi messages (where length
            /// can be either one or two messages) </returns>
            public static Message[] toMidiData( float Hz, float db )
            {
                return toMidiData( Hz, db, 0 );
            }
            public static Message[] toMidiData( float Hz, float db, int ch)
            {
                int note = -1;
                float freq = 0;
                while ( Hz > (freq=tunes[++note]) );
                detune.ProportionalFloat = (freq - Hz) / (freq - tunes[note - 1]); 
                if ( detune.Value == 0 ) {
                    return new Message[1] {
                           new Message( Message.TYPE.NOTE_ON, ch, note, (byte)(db*127) )
                    };
                } else unchecked {
                        if( !detuneByCC ) {
                            if( detuneByPP )
                                detune.Number = note;
                        } if( ch > 0 )
                            detune.Channel = ch-1;
                    return new Message[2] {
                           new Message( Message.TYPE.NOTE_ON, ch, note, (byte)(db*127) ),
                                                 detune
                    };
                }
            }

        };
    }
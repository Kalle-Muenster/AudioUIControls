using Win32Imports.Midi;

namespace Stepflow.Midi.Helpers
{
    public static class Binding
    {
        public static readonly object[] NoteValues = new object[128];
        public static readonly object[] Controller = new object[128];
        public static readonly object[] ChannelNum = new object[17]{
            "All","1","2","3","4","5","6","7","8",
            "9","10","11","12","13","14","15","16"
        };
        public static readonly double[] MidiTuning = new double[12]{
            261.6255653006, 277.1826309769, 293.6647679174,
            331.1269837221, 329.6275569129, 349.2282314330,
            369.9944227116, 391.9954359817, 415.3046975799,
            440.0000000000, 466.1637615181, 493.8833012561
        };

        public enum Type : uint
        {
            NoteValues = 0,
            NoteVolume = Message.TYPE.NOTE_ON,
            NotePresure = Message.TYPE.POLY_PRESSURE,
            PitchWheel = Message.TYPE.PITCH,
            Modulation = Message.TYPE.MONO_PRESSURE,
            Controller = Message.TYPE.CTRL_CHANGE
        }

        static Binding()
        {
            for( Note note = Note.C_; note < Note.G9; ++note )
                NoteValues[(int)note] = note;
            NoteValues[127] = Note.G9;

            for( sbyte ccnu = 0; ccnu < 127; ++ccnu )
                Controller[ccnu] = ccnu;
            Controller[127] = (sbyte)127;
        }
    }
}

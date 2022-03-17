using Stepflow;

namespace Stepflow.Gui.Helpers
{
    /*
    public class PinAccessor 
    {
        private ControllerBase  c;
        public PinAccessor(ControllerBase controller)
        {
            c = controller;
        }
        public void Set(ControllerBase controlledvalue)
        {
            c = controlledvalue;
        }
        public CT Get<CT>() where CT : ControllerBase
        {
            return (CT)c;
        }
        public byte this[byte idx] {
            get { unsafe { return *(byte*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(byte*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public sbyte this[sbyte idx] {
            get { unsafe { return *(sbyte*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(sbyte*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public ushort this[ushort idx] {
            get { unsafe { return *(ushort*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(ushort*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public short this[short idx] {
            get { unsafe { return *(short*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(short*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public uint this[uint idx] {
            get { unsafe { return *(uint*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(uint*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public int this[int idx] {
            get { unsafe { return *(int*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(int*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public ulong this[ulong idx] {
            get { unsafe { return *(ulong*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(ulong*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public long this[long idx] {
            get { unsafe { return *(long*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(long*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public float this[float idx] {
            get { unsafe { return *(float*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(float*)c.GetPin((int)idx).ToPointer() = value; } }
        }
        public double this[double idx] {
            get { unsafe { return *(double*)c.GetPin((int)idx).ToPointer(); } }
            set { unsafe { *(double*)c.GetPin((int)idx).ToPointer() = value; } }
        }
    }
    */

    //public class StreamFXContainer
    //{
    //    short[] frame;
    //    public PinAccessor L;
    //    public PinAccessor R;

    //    public StreamFXContainer()
    //    {
    //        frame = new short[2];
    //        Controlled.Int16 l = new Controlled.Int16(ControlMode.Filter3Band4Pole);
    //        l.SetUp(880, 6000, 0, 0, ControlMode.Filter3Band4Pole);
    //        Controlled.Int16 r = new Controlled.Int16(ControlMode.Filter3Band4Pole);
    //        r.SetUp(880, 6000, 0, 0, ControlMode.Filter3Band4Pole);

    //        L = new PinAccessor(l);
    //        R = new PinAccessor(r);
    //    }

    //    public short[] IO {
    //        get { unsafe {
    //                frame[0] = L.Get<Controlled.Int16>().VAL;
    //                frame[1] = R.Get<Controlled.Int16>().VAL;
    //            } return frame;
    //        }
    //        set { L.Get<Controlled.Int16>().VAL = value[0];
    //              R.Get<Controlled.Int16>().VAL = value[1]; 
    //        }
    //    }

    //    public double this[double idx] {
    //        get { return L[idx]; }
    //        set { L[idx] = value;  R[idx] = value; }
    //    }
    //    public short this[short idx] {
    //        get { return L[idx]; }
    //        set { L[idx] = value; R[idx] = value; }
    //    }
    //}
}

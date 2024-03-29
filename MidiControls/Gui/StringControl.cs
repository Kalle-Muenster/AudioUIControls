﻿#define TOUCHINPUT_EXPERIMENTAL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32Imports.Midi;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Helpers;
using Stepflow.Gui.Geometry;
using Stepflow.Gui.Automation;
using Stepflow.Controller;
using Stepflow.TaskAssist;
using Stepflow.Midi;
using Stepflow.Midi.Helpers;

using Point64 = Stepflow.Gui.Geometry.Point64;
using System.Windows.Input;
using Stepflow.Midi.ControlHelpers;
using Orientation = Stepflow.Gui.Orientation;
using Style = Stepflow.Gui.Style;
using MidiMessage = Win32Imports.Midi.Message;
using MidiValue = Win32Imports.Midi.Value;
using FingerTip = Stepflow.Gui.FingerTip;
using Win32Imports.Touch;


namespace Stepflow
{
    namespace Midi
    {
        public partial class StringControl
            : UserControl
            , IInterValuable<Controlled.Float32>
            , IMidiControlElement<MidiOutput>
        {
            public class SlideBar
                : MidiMeter
                , ITouchGesturedElement<SlideBar>
            {
                public  enum Valence { Level, Tone }
                public const Valence Volume = Valence.Level;
                public const Valence Tuning = Valence.Tone;

                public event ValueChangeDelegate<float> ToneChanged;

                private String             saite;
                private Controlled.Float32 tone;

                public IResonator resonator {
                    get { return saite; }
                    set { saite = value as String; }
                }

                override public IControllerValenceField<Controlled.Float32> valence()
                {
                    return getJoints<Valence>().field();
                }

                override public IControllerValenceField<Controlled.Float32> valence( Enum which )
                {
                    return getJoints<Valence>().field( which ) as IControllerValenceField<Controlled.Float32>;
                }
                public float Tone {
                    get { return tone; }
                    set { float current = tone.VAL;
                        if( current != value ) {
                            tone.VAL = value;
                            valence( Tuning ).SetDirty(
                             ValenceFieldState.Flags.VAL );
                            ToneChanged?.Invoke( this, tone.VAL );
                            Invalidate();
                        }
                    }
                }
                public float ToneRange {
                    get { return tone.MAX - tone.MIN; }
                }
                public float ToneProportion {
                    get { return ClipFactor; }
                    set { Tone = (value * ToneRange) + tone.MIN; }
                }
                public override float ClipFactor {
                    get {
                        float t = tone;
                        clipo = (t - tone.MIN) / ToneRange;
                        return clipo;
                    }
                }

                public SlideBar( StringControl instrument, String stringDing,
                                 float minFrq, float maxFrq )
                    : base( false )
                {
                    saite = stringDing;
                    midiIn = new MidiInput();
                    tone = new Controlled.Float32();
                    
                    (this as ITouchGesturedElement<SlideBar>).handler = new TouchGesturesHandler<SlideBar>(this);
                    Parent = instrument.steg.Panel2;
                    tone.SetUp(minFrq, maxFrq, 0, minFrq, ControlMode.Element);
                    unsafe
                    {
                        *(bool*)tone.GetPin(ElementValue.CYCLED).ToPointer() = false;
                        *(bool*)tone.GetPin(ElementValue.CLAMPED).ToPointer() = false;
                        *(bool*)tone.GetPin(ElementValue.UNSIGNED).ToPointer() = true;
                    }
                    tone.SetCheckAtSet();
                    tone.Active = true;
               //     tone.Invert = Spin.DOWN;

                    InitValue();
                    (valence() as ValenceField<Controlled.Float32, Valence>).SetControllerArray(
                        new Controlled.Float32[] { value, tone });
                    InitMeter();
                    dampfTackter = new TaskAssist<SteadyAction, Action, Action>(this, stringDing.taskAssist, 60);
                    IContainer connector = InitConnector();
                    base.RightToLeft = RightToLeft.Yes;
                    directionalOrientation = (int)DirectionalOrientation.Up;
                    BorderStyle = BorderStyle.None;
                    InitMidi( connector );
                    Inverted = false;
                    Unsigned = true;

                    if( PointerInput.AutoRegistration == AutoRegistration.Enabled ) {
                        if( PointerInput.Dispatcher == null ) {
                            PointerInput.Initialized += (this as ITouchableElement).TouchInputReady;
                        } else PointerInput.Dispatcher.RegisterTouchableElement(this);
                    }

                    Disposed += SlideBar_Disposed;
                }

                private void SlideBar_Disposed( object sender, EventArgs e )
                {
                    PointerInput.Dispatcher?.UnRegisterTouchableElement( this );
                }

                //private void SlideBar_TouchLift( object sender, FingerTip touch )
                //{
                //    // TODO check if any vibrato (quer-richting touch moved) is applied - and gegebenenfalls remove that vibrato,
                //    // (or beter transform such 'rest-vibrato' amount to a value forwarded for being applied as zupf energy (slap)
                //}

                //private void SlideBar_TouchMove( object sender, FingerTip touch )
                //{
                //    // Not used actually
                //}

                //private void SlideBar_TouchDown( object sender, FingerTip touch )
                //{

                //}

                TouchGesturesHandler<SlideBar> ITouchGesturedElement<SlideBar>.handler { get; set; }

                void ITouchableElement.TouchInputReady( PointerInput inst )
                {
                    PointerInput.Initialized -= (this as ITouchableElement).TouchInputReady;
                    inst.RegisterTouchableElement( this );
                }

                private IGestureTouchTrigger touchEvents() { return ( this as ITouchGesturedElement<SlideBar> ).handler.events(); }

                public event FingerTip.TouchDelegate   TouchDown { add { touchEvents().TouchDown += value;} remove { touchEvents().TouchDown -= value; } }
                public event FingerTip.TouchDelegate   TouchMove { add { touchEvents().TouchMove += value;} remove { touchEvents().TouchMove -= value; } }
                public event FingerTip.TouchDelegate   TouchLift { add { touchEvents().TouchLift += value;} remove { touchEvents().TouchLift -= value; } }
                 
                public event FingerTip.TouchDelegate   TouchTapped { add { touchEvents().TouchTapped += value;} remove { touchEvents().TouchTapped -= value; } }
                public event FingerTip.TouchDelegate   TouchDupple { add { touchEvents().TouchDupple += value;} remove { touchEvents().TouchDupple -= value; } }
                public event FingerTip.TouchDelegate   TouchTrippl { add { touchEvents().TouchTrippl += value;} remove { touchEvents().TouchTrippl -= value; } }

                public event MultiFinger.TouchDelegate TouchDraged { add { touchEvents().TouchDraged += value;} remove { touchEvents().TouchDraged -= value; } }
                public event MultiFinger.TouchDelegate TouchResize { add { touchEvents().TouchResize += value;} remove { touchEvents().TouchResize -= value; } }
                public event MultiFinger.TouchDelegate TouchRotate { add { touchEvents().TouchRotate += value;} remove { touchEvents().TouchRotate -= value; } }

                virtual public void OnTouchDown( FingerTip tip )
                {}

                virtual public void OnTouchMove( FingerTip tip )
                {}

                virtual public void OnTouchLift( FingerTip tip )
                {
                }

                public void OnTouchTapped(FingerTip tip)
                {
                }

                public void OnTouchDupple(FingerTip tip)
                {
                }

                public void OnTouchTrippl(FingerTip tip)
                {
                }

                public void OnTouchDraged(MultiFinger tip)
                {
                }

                public void OnTouchResize(MultiFinger tip)
                {
                }

                public void OnTouchRotate(MultiFinger tip)
                {
                }

                public IRectangle ScreenRectangle()
                {
                    return CornerAndSize.FromRectangle( RectangleToScreen( new Rectangle(0,0,Width,Height) ) );
                }

                public Point64 ScreenLocation()
                {
                    return PointToScreen( Location );
                }

                public ITouchEventTrigger touch {
                    get { return (this as ITouchGesturedElement<SlideBar>).handler; }
                }

                public ITouchDispatchTrigger screen()
                {
                    return touch.screen();
                }

                ITouchableElement ITouchable.element()
                {
                    return this;
                }

                void passFinger( FingerTip tip )
                {
                    Invalidate();
                }

                public bool TouchHitTest( Win32Imports.Touch.RECT fingerprint )
                {
                    return Bounds.IntersectsWith( fingerprint );
                }

                Control ITouchable.Element {
                    get { return this; }
                }

                bool ITouchable.IsTouched {
                    get { return touch.IsTouched; }
                }

                public ITouchableElement element {
                    get { return this; }
                }
            };

            public struct StringSet
            {
                private static List<string> KnownSetNames = new List<string>();
                private readonly int index;
                public string SetName { get { return KnownSetNames[index]; } }
                public readonly StringVariant Instrumtation;
                public readonly int Saiten;
                public readonly int Buende;
                public readonly  Note[] Names;
                public readonly float[] Tunes;

                public StringSet(string setname, StringVariant instrumentation, int saiten, int buende,
                                  ICollection<Note> notes,
                                  ICollection<float> tunes)
                {
                    if( saiten != notes.Count || saiten != tunes.Count )
                        throw new Exception("construction parameters: array lengths mismatch");
                    setname = string.Format("{0}({1},{2})", setname, saiten, buende);
                    if( KnownSetNames.Contains(setname) )
                        throw new Exception("StringSet '" + setname + "' already is defined!");
                    else index = KnownSetNames.Count;
                    KnownSetNames.Add(setname);
                    Instrumtation = instrumentation;
                    Saiten = saiten;
                    Buende = buende;
                    Names = new Note[saiten];
                    Tunes = new float[saiten];
                    IEnumerator<Note> note  = notes.GetEnumerator();
                    IEnumerator<float> tune = tunes.GetEnumerator();
                    for( int n = 0; n < saiten; ++n ) {
                        note.MoveNext();
                        Names[n] = note.Current;
                        tune.MoveNext();
                        Tunes[n] = tune.Current;
                    }
                }
            }

            public struct StringSets
            {
                public static readonly StringSet Guitar = new StringSet("Guitar",StringVariant.Zupf,6,12,new Note[]{Note.E2,Note.A2,Note.D3,Note.G3,Note.H3,Note.E4},new float[] {82.41f,110.0f,146.83f,196.0f,246.94f,329.63f});
                public static readonly StringSet Bass = new StringSet("Bass",StringVariant.Zupf,4,12,new Note[]{Note.E1,Note.A1,Note.D2,Note.G2},new float[] {41.205f,55.0f,73.415f,99.0f});
                public static readonly StringSet Cello = new StringSet("Cello",StringVariant.Strike, 4,0,new Note[] { Note.A3,Note.D3,Note.G2,Note.C2},new float[] {Frequency.fromMidiNote(Note.A3),Frequency.fromMidiNote(Note.D3),Frequency.fromMidiNote(Note.G2),Frequency.fromMidiNote(Note.C2) } );
                public static readonly StringSet Violine = new StringSet("Violine",StringVariant.Strike, 4, 0, new Note[] { Note.G3, Note.D3, Note.A4, Note.E5 }, new float[] { Frequency.fromMidiNote(Note.G3), Frequency.fromMidiNote(Note.D3), Frequency.fromMidiNote(Note.A4), Frequency.fromMidiNote(Note.E5) });
                public static readonly StringSet BanjoModal = new StringSet("BanjoModal",StringVariant.Zupf,5,12, new Note[] { Note.G4, Note.D3, Note.G3, Note.C4, Note.D4 }, new float[] { Frequency.fromMidiNote(Note.G4), Frequency.fromMidiNote(Note.D3), Frequency.fromMidiNote(Note.G3), Frequency.fromMidiNote(Note.C4), Frequency.fromMidiNote(Note.D4) } );
                public static readonly StringSet BanjoDualC = new StringSet("BanjoDualC",StringVariant.Zupf,5,12, new Note[] { Note.G4, Note.C3, Note.G3, Note.C4, Note.D4 }, new float[] { Frequency.fromMidiNote(Note.G4), Frequency.fromMidiNote(Note.C3), Frequency.fromMidiNote(Note.G3), Frequency.fromMidiNote(Note.C4), Frequency.fromMidiNote(Note.D4) } );
                public static readonly StringSet BanjoGrass = new StringSet("BanjoGrass",StringVariant.Zupf,5,12, new Note[] { Note.G4, Note.D3, Note.G3, Note.H3, Note.D4 }, new float[] { Frequency.fromMidiNote(Note.G4), Frequency.fromMidiNote(Note.C3), Frequency.fromMidiNote(Note.G3), Frequency.fromMidiNote(Note.H3), Frequency.fromMidiNote(Note.D4) } );
                public static readonly StringSet Ukulele = new StringSet("Ukulele",StringVariant.Zupf,4,12,new Note[]{Note.G5,Note.C5,Note.E5,Note.A5},new float[] {Frequency.fromMidiNote(Note.G5), Frequency.fromMidiNote(Note.C5), Frequency.fromMidiNote(Note.E5), Frequency.fromMidiNote(Note.A5)});
            }

            public class String
                : IResonator
                , IMidiControlElement<MidiOutput>
            {
                public event ValueChangeDelegate<float> FrequencyChanged {
                    add { elementar.ToneChanged += value; }
                    remove { elementar.ToneChanged -= value; }
                }
                public event ValueChangeDelegate<float> StringDetunation;
                public event ValueChangeDelegate<float> AmplitudeChanged {
                    add { elementar.LevelChanged += value; }
                    remove { elementar.LevelChanged -= value; }
                }

                private MidiMessage[]     midimsges = new MidiMessage[2];
                private Note              notevalue;
                public SlideBar elementar { get; set; }
                public Controlled.Float32 oscilator;
                public Controlled.Float32 frequency;
                public Controlled.Float32 detunated;
                public Controlled.Float32 amplitude;

                public StringControl      Instrument;
                public int                outchannel;

                private float       marker;
                public readonly int index;
                private string      name;

                IInterValuable IStepflowControlElementComponent.getElement()
                {
                    return Instrument;
                }

                private volatile bool sloaping = false;
                public bool Sloaping {
                    get { return sloaping; }
                    set {
                        if( value != sloaping ) {
                            if( value ) {
                                sloaping = true;
                                AmplitudeChanged += String_AmplitudeChanged;
                                FrequencyChanged += String_FrequencyChanged;
                                elementar.task().StartAssist();
                            } else {
                                sloaping = false;
                                elementar.task().StoptAssist();
                                AmplitudeChanged -= String_AmplitudeChanged;
                                FrequencyChanged -= String_FrequencyChanged;
                            }
                        }
                    }
                }

                private bool sliding = false;
                public bool Slide {
                    get { return sliding; }
                    set {
                        if( value != sliding ) {
                            if( value ) {
                                elementar.TouchMove += OnStringSlide;
                                elementar.TouchLift += OnStringRelease;
                            } else {
                                elementar.TouchMove -= OnStringSlide;
                                elementar.TouchLift -= OnStringRelease;
                            }
                            sliding = value;
                        }
                    }
                }

                private void OnStringSlide( object sender, FingerTip e )
                {
                    float newdetune = 0;
                    Point64 pos = e.Position;
                    switch( Instrument.Orientation ) {
                        case Orientation.Horizontal:
                            TapPosition = pos.x;
#pragma warning disable CS1690 
                            newdetune = detunated.MOV + (float)( pos.y - (elementar.Height/2)) * 0.1f;
#pragma warning restore CS1690
                            break;
                        case Orientation.Vertical:
                            TapPosition = pos.y;
#pragma warning disable CS1690
                            newdetune = detunated.MOV + (float)( pos.x - (elementar.Width/2)) * 0.1f;
#pragma warning restore CS1690
                            break;
                    }
                    if( detunated.VAL != newdetune ) {
                        StringDetunation?.Invoke( this, newdetune );
                    }
                    elementar.Invalidate();
                }
                public string Name {
                    get { return name; }
                    internal protected set { name = value; }
                }
                public float ZupfPoint {
                    get { return marker; }
                }
                public bool TuneInvert {
                    get { return frequency.Invert; }
                    set { frequency.Invert = value ? Spin.UP : Spin.DOWN; }
                }
                private IntPtr tuneCycled;
                public bool TuneCycled {
                    get { unsafe { return *(bool*)tuneCycled.ToPointer(); } }
                    set { unsafe { *(bool*)tuneCycled.ToPointer() = value; } }
                }
                private IntPtr tuneUnsign;
                public bool TuneUnsign {
                    get { unsafe { return *(bool*)tuneUnsign.ToPointer(); } }
                    set { unsafe { *(bool*)tuneUnsign.ToPointer() = value; } }
                }
                public float TuneRange {
                    get { return (frequency.MAX - frequency.MIN); }
                }
                public float Proportion {
                    get { return (frequency - frequency.MIN) / TuneRange; }
                    set { frequency.VAL =  (value * TuneRange) + frequency.MIN; }
                }
                public int TapPosition {
                    get {
                        return (int)(((float)elementar.PixelRange * elementar.ToneProportion) + elementar.InnerBorder);
                    }
                    set {
                        elementar.ToneProportion = GuiMeter.rasterize( Instrument.NumberOfBounds,
                                      (float)(value - elementar.InnerBorder) / elementar.PixelRange
                        );
                    }
                }

                public MidiValue MidiValue {
                    get {
                        return ((IMidiControlElement<MidiOutput>)Instrument).MidiValue;
                    }

                    set {
                        ((IMidiControlElement<MidiOutput>)Instrument).MidiValue = value;
                    }
                }

                public AutomationlayerAddressat[] channels {
                    get {
                        return ((IMidiControlElement<MidiOutput>)Instrument).channels;
                    }
                }

                public void OnIncommingMidiControl(object sender, MidiMessage value)
                {
                    ((IMidiControlElement<MidiOutput>)Instrument).OnIncommingMidiControl(sender, value);
                }

                public MidiOutput midi()
                {
                    return Instrument.midi();
                }

                MidiInputMenu<MidiOutput> IMidiControlElement<MidiOutput>.inputMenu {
                    get; set;
                }

                MidiOutputMenu<MidiOutput> IMidiControlElement<MidiOutput>.outputMenu {
                    get; set;
                }
                private static float vibrator(ref float val, ref float min, ref float max, ref float mov)
                {
                    return val = min + (max * mov * min);
                }

                public void createEventMarker(float zupfpoint)
                {
                    marker = zupfpoint;
                    if( Instrument.Variant == StringVariant.Zupf ) {
                        Instrument.plectrum.AddEventMarker((marker = (marker * Instrument.plectrum.ValueRange) + Instrument.plectrum.Minimum), name, OnStringInvoke);
                    }
                }

                public String( StringControl instrument, int stringdex, float zupfpoint, float Min, float Max, string name )
                {
                    marker = zupfpoint;
                    index = stringdex;
                    Instrument = instrument;
                    outchannel = stringdex + 1;
                    sliding = false;

                    oscilator = new Controlled.Float32();
                    oscilator.SetUp(-100, 100, Min, 0, ControlMode.Pulse);
                    oscilator.SetCheckAtGet();

                    Orientation orientation = instrument.Orientation;

                    elementar = new SlideBar(instrument, this, Min, Max);
                    elementar.Range = 100;
                    elementar.Proportion = 0.5f;
                    elementar.Orientation = orientation;
                    amplitude = elementar.valence(SlideBar.Volume).controller();
                    frequency = elementar.valence(SlideBar.Tuning).controller();
                    tuneCycled = frequency.GetPin(ElementValue.CYCLED);
                    tuneUnsign = frequency.GetPin(ElementValue.UNSIGNED);
                    TuneUnsign = true;
                    TuneCycled = false;
                    TuneInvert = false;
                    unsafe
                    {
                        detunated = new Controlled.Float32(ref *(float*)frequency.GetPin(Pin.MOV).ToPointer());
                    }
                    detunated.AttachedDelegate = vibrator;
                    detunated.MIN = frequency.MOV;
                    detunated.MAX = 0.1f;
                    detunated.MOV = 0f;
                    detunated.SetCheckAtSet();
                    this.name = name;

                    float prop = (1.0f / instrument.NumberOfStrings) * stringdex;
                    if( orientation == Orientation.Horizontal ) {
                        elementar.Width = instrument.steg.Panel2.Width;
                        elementar.Height = (int)(instrument.steg.Panel2.Height * (1.0f / instrument.NumberOfStrings)) - 5;
                        elementar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                        elementar.Location = new Point(0, (int)(prop * instrument.steg.Panel2.Height));
                        createEventMarker((float)(elementar.Location.Y + (float)elementar.Height / 2) / instrument.steg.Panel2.Height);
                    } else {
                        elementar.Height = instrument.steg.Panel2.Height;
                        elementar.Width = (int)(instrument.steg.Panel2.Width * (1.0f / instrument.NumberOfStrings)) - 5;
                        elementar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                        elementar.Location = new Point((int)(prop * instrument.steg.Panel2.Width), 0);
                        createEventMarker((float)(elementar.Location.X + ((float)elementar.Width / 2)) / instrument.steg.Panel2.Width);
                    }

                    elementar.Style = Style.Flat;
                    elementar.ForeColor = Color.CornflowerBlue;

                    elementar.TouchDown += OnStringTap;
                    elementar.Paint += OnStringPaint;

                    oscilator.Active = true;
                    detunated.Active = true;

                    instrument.steg.Panel2.Controls.Add( elementar );

                    FrequencyChanged += String_FrequencyChanged;
                    StringDetunation += String_StringDetunation;
                }

                private void String_StringDetunation( object sender, ValueChangeArgs<float> value )
                {
                    detunated.VAL = value.Value;
                }

                private void String_AmplitudeChanged( object sender, ValueChangeArgs<float> value )
                {
                    midi().SendNoteOn( outchannel, notevalue, (int)(127.0f * value.Value) );
                }

                private void String_FrequencyChanged( object sender, ValueChangeArgs<float> value )
                {
                    midimsges = Frequency.toMidiData( value.Value, amplitude, outchannel );
                    notevalue = (Note)midimsges[0].Number;
                    midi().SendOutMessages( midimsges );
                }


                private void OnStringPaint( object sender, PaintEventArgs e )
                {
                    if( sloaping )
                        Sloaping = elementar.Level > 0;
                }

                private void OnStringTap( object sender, FingerTip e )
                {
                    Slide = true;
                    TapPosition = Instrument.Orientation == Orientation.Horizontal
                                ? e.X : e.Y;
                }

                public void OnStringInvoke( object sender, MidiSlider.MarkerPassedEventArgs zupfPoint )
                {
                    float set = Math.Abs( zupfPoint.Speed * 5 );
                    oscilator.MIN = -set;
                    oscilator.MAX = set;
                    elementar.Level = set;
                    if( !Sloaping ) {
                        Sloaping = true;
                    }
                    midimsges = Frequency.toMidiData( frequency, amplitude, outchannel );
                    notevalue = (Note)midimsges[0].Number;
                    midi().SendOutMessages( midimsges );
                }

                private void OnStringRelease(object sender, FingerTip e)
                {
                    Slide = false;
                    elementar.Tone = frequency.MIN;
                }

                private float OnStringSlope()
                {
                    detunated.VAL = frequency.MOV;
                    float var = detunated.MOV;
                    if( var != 0 ) {
                        if( (var *= 0.8f) <= 0.01 )
                            var = 0;
                        detunated.MOV = var;
                    }
                    var = (elementar.Proportion * 0.93f);
                    if( var <= 0.0001 ) {
                        return 0;
                    }
                    return var;
                }

                internal void taskAssist()
                {
                    elementar.Proportion = OnStringSlope();
                }
            };


            public MidiSlider                plectrum;
            public Dictionary<string,String> Strings;

            private MidiOutput             midiOut;
            private int                    stringCount;
            private int                    staegeCount;
            private List<String>           strings;
            private DirectionalOrientation direction;

            public int NumberOfStrings {
                get { return stringCount; }
                set { stringCount = value; }
            }

            public int NumberOfBounds {
                get { return staegeCount; }
                set { staegeCount = value; }
            }


            private void attachStrings(int value)
            {
                if( value != strings.Count ) {
                    stringCount = value;
                    float distant = (plectrum.ValueRange / value);
                    plectrum.Proportion = 0;
                    foreach( String saite in strings )
                        Controls.Remove(saite.elementar);
                    strings.Clear();
                    Strings.Clear();
                    plectrum.ClearEventMarkers();
                    for( int i = 0; i < value; ++i ) {
                        float pos = plectrum.Minimum + (i * distant);
                        String saite = new String( this, i, pos, 100, 2000, string.Format("Saite{0}", (char)('A' + i)) );
                        strings.Add( saite );
                        Strings.Add( saite.Name, saite );
                    } Invalidate();
                }
            }

            public void AttachStrings( StringSet set )
            {
                float distance = ( plectrum.ValueRange / set.Saiten );
                plectrum.Proportion = 0;
                foreach( String saite in strings )
                    Controls.Remove( saite.elementar );
                strings.Clear(); Strings.Clear();
                plectrum.ClearEventMarkers();
                NumberOfStrings = set.Saiten;
                NumberOfBounds = set.Buende;
                Variant = set.Instrumtation;
                for( int i = 0; i < set.Saiten; ++i ) {
                    float pos = plectrum.Minimum + (i * distance);
                    String saite = new String( this, i, pos, set.Tunes[i],
                                               set.Tunes[i] * 2.0f,
                                               set.Names[i].ToString() );
                    strings.Add( saite );
                    Strings.Add( set.Names[i].ToString(), saite );
                } Invalidate();
            }

            private int orientation;
            public Orientation Orientation {
                get { return (Orientation)orientation; }
                set {
                    Orientation current = (Orientation)orientation;
                    if( value != current ) {
                        int sw = steg.Panel1.Width;
                        int sh = steg.Panel1.Height;
                        if( current == Orientation.Horizontal ) {
                            int t = Width;
                            Width = Height;
                            Height = t;
                            steg.Orientation = System.Windows.Forms.Orientation.Horizontal;
                            steg.SplitterDistance = sw;
                        } else {
                            int t = Height;
                            Height = Width;
                            Width = t;
                            steg.Orientation = System.Windows.Forms.Orientation.Vertical;
                            steg.SplitterDistance = sh;
                        }
                        orientation = (int)value;
                        layoutStrings();
                    }
                }
            }

            private void layoutStrings()
            {
                switch( Orientation ) {
                    case Orientation.Horizontal: {
                            int stringSize = steg.Panel2.Height/stringCount;
                            plectrum.ClearEventMarkers();
                            int pt = plectrum.Orientation == Orientation.Horizontal ? plectrum.Height : plectrum.Width;
                            plectrum.Orientation = Orientation.Vertical;
                            plectrum.Height = steg.Panel1.Height;
                            plectrum.Width = pt;
                            plectrum.Location = new Point((steg.Panel1.Width / 2) - (plectrum.Width / 2), steg.Panel1.Top);
                            for( int i = 0; i < NumberOfStrings; ++i ) {
                                String str = strings[i];
                                str.elementar.Anchor = AnchorStyles.None;
                                str.elementar.Orientation = Orientation.Horizontal;
                                str.elementar.Width = steg.Panel2.Width;
                                str.elementar.Height = stringSize;
                                str.elementar.Location = new Point(0, i * stringSize);
                                str.elementar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                                str.createEventMarker( (float)(str.elementar.Location.Y + (float)str.elementar.Height / 2) / steg.Panel2.Height );
                                str.elementar.Inverted = false;
                            }
                        } break;
                    case Orientation.Vertical: {
                            int stringSize = steg.Panel2.Width / stringCount;
                            plectrum.ClearEventMarkers();
                            int pt = plectrum.Orientation == Orientation.Vertical ? plectrum.Width : plectrum.Height;
                            plectrum.Orientation = Orientation.Horizontal;
                            plectrum.Width = steg.Panel1.Width;
                            plectrum.Height = pt;
                            plectrum.Location = new Point(steg.Panel1.Left, (steg.Panel1.Height / 2) - (plectrum.Height / 2));
                            for( int i = 0; i < NumberOfStrings; ++i ) {
                                String str = strings[i];
                                str.elementar.Anchor = AnchorStyles.None;
                                str.elementar.Orientation = Orientation.Vertical;
                                str.elementar.Width = stringSize;
                                str.elementar.Height = steg.Panel2.Height;
                                str.elementar.Location = new Point(i * stringSize, 0);
                                str.elementar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                                str.createEventMarker( (float)(str.elementar.Location.X + (float)str.elementar.Width / 2) / steg.Panel2.Width );
                                str.elementar.Inverted = false;
                            }
                        } break;
                } Invalidate( true );
            }

            private void TapResize()
            {
                foreach( String saite in strings ) {
                    switch( Orientation ) {
                        case Orientation.Horizontal:
                            saite.elementar.Height = steg.Panel2.Height / NumberOfStrings;
                            break;
                        case Orientation.Vertical:
                            saite.elementar.Width = steg.Panel2.Width / NumberOfStrings;
                            break;
                    }
                }
            }

            private ValenceField<Controlled.Float32,ValenceField> joints;
            private Controlled.Float32 value = new Controlled.Float32();
            Action IInterValuable.getInvalidationTrigger() { return plecUpdate; }
            public IControllerValenceField<Controlled.Float32> valence() { return joints.field(); }
            public IControllerValenceField<Controlled.Float32> valence(Enum which) { return joints.field(which); }
            IControllerValenceField IInterValuable.valence<cT>() { return joints.field(); }
            IControllerValenceField IInterValuable.valence<cT>(Enum which) { return joints.field(which); }
            void IInterValuable.callOnInvalidated(InvalidateEventArgs e) { OnInvalidated(e); }
            private void plecUpdate() { }


            static StringControl()
            {
                Valence.RegisterIntervaluableType<Controlled.Float32>();
                TaskAssist<SteadyAction,Action,Action>.Init( 60 );
            }


            public StringControl()
            {
                Frequency.setDetunationType( MidiMessage.TYPE.POLY_PRESSURE, 1 );

                midiOut = new MidiOutput();
                joints = new ValenceField<Controlled.Float32, ValenceField>(this);

                strings = new List<String>();
                Strings = new Dictionary<string, String>();

                direction = DirectionalOrientation.Right;
                orientation = (int)direction & ~0x4;

                plectrum = new MidiSlider();
                plectrum.Minimum = -8192;
                plectrum.Maximum = 8191;
                plectrum.Orientation = Orientation.Vertical;
                plectrum.Inverted = false;
                plectrum.Cycled = false;

                plectrum.Proportion = 0.5f;
                InitializeComponent();

                StringMotivator = new Controlled.Float32();
                StringMotivator.SetUp(0, 2, 0, 0, ControlMode.Clamp);
                StringMotivator.SetCheckAtSet();
                StringMotivator.Active = true;

                steg.Panel1.Controls.Add(plectrum);
                plectrum.Width = steg.Panel1.Width / 5;
                plectrum.Height = steg.Panel1.Height;
                plectrum.Location = new System.Drawing.Point((steg.Panel1.Width / 2) - plectrum.Width / 2, 0);
                plectrum.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

                AttachStrings( StringSets.Guitar );
                if( Variant == StringVariant.Zupf ) {
                    plectrum.MarkerPassed += OnStringClampf;
                    plectrum.ValueChanged += OnPlecSlowMove;
                }
                plectrum.MovementFast += OnPlecFastMove;
                plectrum.LedColor = LED.Cyan;
                plectrum.Style = Style.Dark;
                plectrum.AttachSideChain( IntPtr.Zero );

                midi().InitializeComponent( this, components );
                Disposed += Dispose_function;
            }

            private void Driver_Round(object sender, LapEventArgs e)
            {
                currentAmplitude = CurrentAmplitude();
                plectrum.Invalidate();
            }

            private void Dispose_function( object sender, EventArgs e )
            {
                Valence.UnRegisterIntervaluableElement( this );
            }

            public enum StringVariant {
                Zupf, Strike
            }
            private StringVariant variant = StringVariant.Zupf;
            public StringVariant Variant {
                get { return variant; }
                set {
                    if( value != variant ) {
                        if( value == StringVariant.Zupf ) {
                            plectrum.ThresholdForFastMovement = 2400f;
                            plectrum.AttachSideChain( IntPtr.Zero );
                            (plectrum.task().assist.driver as SteadyAction).Round -= Driver_Round;
                        } else {
                            plectrum.ThresholdForFastMovement = 0.4f;
                            plectrum.AttachSideChain(ref currentAmplitude);
                            (plectrum.task().assist.driver as SteadyAction).Round += Driver_Round;
                        }
                        variant = value;
                        layoutStrings();
                    }
                }
            }

            private void OnPlecSlowMove(object sender, ValueChangeArgs<float> value)
            {
                dbglbl.Text = "Move: " + value.Value.ToString();
            }

            private Controlled.Float32 StringMotivator;
            private void OnPlecFastMove(object sender, ValueChangeArgs<float> speed)
            {
                dbglbl2.Text = "Fast: " + speed.Value.ToString();
                float isfx = Math.Abs(speed.Value/40.0f);
                foreach( String saite in strings ) {
                    StringMotivator.VAL = isfx;
                    StringMotivator.VAL -= (3.0f * Math.Abs(((((float)saite.ZupfPoint * plectrum.ValueRange) - 8192.0f) / 8191.0f) - ((float)plectrum.Value / 8192.0f)));
                    saite.elementar.Level += (StringMotivator * 3.0f);
                    saite.Sloaping = true;
                }
                //currentAmplitude = CurrentAmplitude();
            }

            private void OnStringClampf(object sender, MidiSlider.MarkerPassedEventArgs zupfpoint)
            {
                String saite = Strings[zupfpoint.Named];
                plectrum.AttachSideChain( saite.amplitude.GetTarget() );
                // plectrum.LedSource = GuiSlider.LEDSource.TheSideChainedValue;
            }

            private float currentAmplitude = 0;
            public float CurrentAmplitude()
            {
                float vol = 0;
                foreach( String saite in strings ) vol += saite.amplitude;
                return vol;
            }

            private float Range {
                get;
                set;
            }

            ToolStripItemCollection IInterValuable.getMenuHook() { return InstrumentContextMenu.Items; }

            MidiValue IMidiControlElement<MidiOutput>.MidiValue {
                get {
                    throw new NotImplementedException();
                }

                set {
                    throw new NotImplementedException();
                }
            }

            public MidiOutput midi() {
                return midiOut;
            }


            void IMidiControlElement<MidiOutput>.OnIncommingMidiControl(object sender, MidiMessage msg)
            {
                (plectrum as IMidiControlElement<MidiInOut>).MidiValue = new Value((short)msg.Value);
            }

            MidiInputMenu<MidiOutput> IMidiControlElement<MidiOutput>.inputMenu { get; set; }
            MidiOutputMenu<MidiOutput> IMidiControlElement<MidiOutput>.outputMenu { get; set; }
            AutomationlayerAddressat[] IAutomat<MidiOutput>.channels {
                get { return new AutomationlayerAddressat[] {
                    new AutomationlayerAddressat(
                        (byte)midi().MidiOut_Channel,
                        (byte)midi().MidiOut_Type,
                        (byte)midi().MidiOut_Controller,
                        (byte)midi().MidiOutPortID) 
                    };
                }
            }

            public virtual void OnTouchDown(FingerTip tip) { }
            public virtual void OnTouchMove(FingerTip tip) { }
            public virtual void OnTouchLift(FingerTip tip) { }
        }
    }
}

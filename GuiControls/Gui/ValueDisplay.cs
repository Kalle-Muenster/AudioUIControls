//#define IMPLEMENT_MIDICONTOL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Stepflow;
using Stepflow.Gui;
using Stepflow.Gui.Automation;
using Orientation  = Stepflow.Gui.Orientation;
using Style        = Stepflow.Gui.Style;
using Compatibles  = System.Collections.Generic.List<Stepflow.Gui.Valence.FieldDescriptor>;
using Resources    = GuiControls.Properties.Resources;

#if IMPLEMENT_MIDICONTOL
using MidiControls;
using Win32Imports.Midi;
#endif

namespace Stepflow.Gui
{
    public partial class ValueDisplay
        : UserControl
        , IInterValuable<Controlled.Float32>
    {
        private static readonly StyleSet[]  stylers;
        private static readonly System.Drawing.Rectangle[] borders;
        private const string    format = "###0.0##";
        private static Compatibles      compatibles;

#if IMPLEMENT_MIDICONTOL
        private MidiIO midiIO;
        static MidiValueDisplay() {
#else
        static ValueDisplay() {
#endif
            Valence.RegisterIntervaluableType<Controlled.Float32>();
            stylers = new StyleSet[] {
                new StyleSet( Resources.umittelung,
                              Color.Gray,Color.Black,Color.LightGray ),
                new StyleSet( Resources.hellebarden,
                              Color.LightGray,Color.Black,Color.DarkBlue ),
                new StyleSet( Resources.umrandunkel,
                              StyleSet.Dark,Color.Lime,Color.CornflowerBlue )
            };
            borders = new System.Drawing.Rectangle[] {
                new System.Drawing.Rectangle(0,0,10,64), new System.Drawing.Rectangle(10,0,492,10),
                new System.Drawing.Rectangle(502,0,10,64), new System.Drawing.Rectangle(10,54,492,10)
            };
#if DEBUG
            Consola.StdStream.Init( Consola.CreationFlags.TryConsole );
            Win32Imports.RETURN_CODE.SetLogOutWriter(Consola.StdStream.Out.WriteLine);
#endif
        }

        private bool                             input;
        private UnitScale                        scale;
        private UnitsType                        units;
        private float                            modif;
        private int                              style;
        private string                           custo;
        private int                              frame;
        private Controlled.Float32               value=null;
        private Controlled.Float32               cover=null;
        private IInterValuable                   other=null;
        private bool                             dirty=false;
        
        #region Impl: IInterValuable
        private Action                                invalidateExternal;
        private ValenceField<Controlled.Float32,ValenceField> valenceField;
        ToolStripItemCollection IInterValuable.getMenuHook() { return mnu_value.Items; }
        Action  IInterValuable.getInvalidationTrigger() { return needUpdate; }
        void    IInterValuable.callOnInvalidated( InvalidateEventArgs e )
                                                { OnWrapInvalidated(e); }

        public  IControllerValenceField<Controlled.Float32> valence()
                                        { return valenceField.field(); }
        public  IControllerValenceField<Controlled.Float32> valence( Enum which )
                                        { return valenceField.field( which ); }

        IControllerValenceField IInterValuable.valence<cT>()
                                               { return valenceField.field(); }
        IControllerValenceField IInterValuable.valence<cT>(Enum field)
                                               { return valenceField.field(field); }

        private void needUpdate() { Value = cover; }

        #endregion

#if     IMPLEMENT_MIDICONTOL
    #region Impl: IMidiControlElemen
        bool midiHires=false;
        public bool HiResController {
            get { return midiHires; }
            set { midiHires = value; }
        }

        public Value MidiValue {
            get { if( midiHires ) return new Value((short)((Proportion - 0.5f) * (short.MaxValue * 2)));
                  else return new Value((int)(Proportion * 127)); }
            set { Proportion = value.ProportionalFloat; }
        }

        MidiIO IMidiControlElement<MidiIO>.binding {
            get { return midiIO; }
        }

        void IMidiControlElement<MidiIO>.OnIncommingMidiControl(object sender, Win32Imports.Midi.Message midicontrol)
        {
            MidiValue = new Value( (short)midicontrol.Value );
        }

        public IMidiControlElement<MidiIO> midi()
        {
            return this;
        }
    #endregion
#endif

        public Style Style {
            get { return (Style)style; }
            set { if((int)value != style) {
                    style = (int)value;
                    txt_value.BackColor =
                    txt_units.BackColor =
                         this.BackColor = stylers[style].color;
                    txt_value.ForeColor =
                         this.ForeColor = stylers[style].value;
                    txt_units.ForeColor = stylers[style].units;
                    Invalidate();
                }
            }
        }

        private void init()
        {
            units = UnitsType.Hz;
            scale = UnitScale.Base;

            InitializeComponent();
            txt_units.Text = " " + units.ToString();
            txt_value.TextChanged += txt_value_TextChanged;       
            
#if IMPLEMENT_MIDICONTOL
            midiIO = new MidiIO();
            midi().binding.InitializeComponent( this, components, Invalidate );
            mnu_value.Items.Add( midiIO.midiIn_mnu_binding_mnu );
            mnu_value.Items.Add( midiIO.midiOut_mnu_binding_mnu );
            midi().binding.AutomationEvent += midi().OnIncommingMidiControl;
            Paint += midiIO.automation().ProcessMessageQueue;
#endif
            Paint += GuiValue_Paint;
            Style = Style.Dark;
        }

        public new void Dispose()
        {
            Valence.UnRegisterIntervaluableElement( this );
            base.Dispose();
        }

#if IMPLEMENT_MIDICONTOL
        public MidiValueDisplay() {
#else
        public ValueDisplay() {
#endif
            input = false;
            modif = 1;
            style = 0;
            frame = 5;
            custo = "Sec";
            value = new Controlled.Float32();
            value.SetUp( -1, 1, 0, 0, ControlMode.Element );
            value.SetCheckAtSet();
            value.Active = true;
            cover = value;
            valenceField = new ValenceField<Controlled.Float32,ValenceField>(
                this, new ControllerBase[1] { cover }
            );
            init();
            input = true;
        }

#if IMPLEMENT_MIDICONTOL
        public MidiValueDisplay( IInterValuable<Controlled.Float32> wrappedElement ) {
#else
        public ValueDisplay( IInterValuable<ControllerBase> wrappedElement ) {
#endif
            input = false;
            modif = 1;
            style = 0;
            frame = 5;
            custo = "Sec";
            input = true;
            valenceField = new ValenceField<Controlled.Float32,ValenceField>( this );
            init();
            Wrap( wrappedElement );
            input = true;
        }

        public bool Wrap<cT>( IInterValuable<cT> element ) where cT : ControllerBase
        {
            if ( element == other ) return true;
            if ( element == this ) {
               if( value == null ) {
                   value = new Controlled.Float32();
                   value.SetUp( -1, 1, 0, 0, ControlMode.Element );
                   value.SetCheckAtSet();
                   value.Active = true;
                } cover = value;
            } else {
                bool foundValenceFieldOfMatchingType = false;
                Valence.FieldIndices indices = element.valence().Field.Indices;
                for(int i=0; i<indices.Count;++i ) {
                    if( element.valence( indices[i] ).controller<ControllerBase>() is Controlled.Float32 ) {
                        cover = element.valence( indices[i] ).controller<Controlled.Float32>();
                        foundValenceFieldOfMatchingType = true; break;
                    }
                } if( !foundValenceFieldOfMatchingType )
                    return false;
            } valenceField.SetControllerArray( new ControllerBase[] { cover } );

            if( other != null ) {
               (other as Control).Invalidated -= valence().getInvalidationHandler();
            }
            if ( element != this ) {
                (element as Control).Invalidated += valence().getInvalidationHandler();
                invalidateExternal = element.getInvalidationTrigger();
                other = (element as IInterValuable<ControllerBase>);
            } else {
                invalidateExternal = null;
                other = null;
            }
            if( input ) Invalidate();
            else input = true;
            return input;
        }

        public bool Wrap( IControllerValenceField<Controlled.Float32> otherField )
        {
            if( !(otherField.controller() is Controlled.Float32) ) return false;
            IInterValuable otherElement = (otherField as IStepflowControlElementComponent).getElement();
            if ( otherElement == other && otherField == cover ) return true;
            if ( otherElement == this ) {
               if( value == null ) {
                   value = new Controlled.Float32();
                   value.SetUp( -1, 1, 0, 0, ControlMode.Element );
                   value.SetCheckAtSet();
                   value.Active = true;
                } cover = value;
            } else {
                cover = otherField.controller();
            } valenceField.SetControllerArray( new ControllerBase[] { cover } );

            if ( other != null )
                (other as Control).Invalidated -= valence().getInvalidationHandler();
            if ( otherElement != this ) {
                (otherElement as Control).Invalidated += valence().getInvalidationHandler();
                invalidateExternal = otherElement.getInvalidationTrigger();
                other = otherElement;
            } else {
                invalidateExternal = null;
                other = null;
            }
            if( input ) Invalidate();
            else input = true;
            return input;
        }

        private string CreateText()
        {
            switch(units) {
                case UnitsType.sec: {
                    if(Scale>=UnitScale.Base) {
                        float val = cover;
                        int msec = (int)(val*1000);
                        msec %= 1000;
                        int sec = (int)val;
                        int min = sec/60; 
                        sec %= 60;
                        int hrs = min/60;
                        min %= 60;
                        hrs %= 24;
                        return string.Format( "{0}:{1}:{2}.{3}",
                                              hrs.ToString("#0"),
                                              min.ToString("#0"),
                                              sec.ToString("#0"),
                                              msec.ToString("##0") );
                    } else {
                       return (cover / modif).ToString( format );
                    }
                } default:
                    return (cover / modif).ToString( format );
            }
        }

        private void txt_value_TextChanged( object sender, EventArgs e )
        {
            if ( input ) Text = txt_value.Text;
            else input = true;
        }
        private void txt_value_KeyPress( object sender, KeyPressEventArgs e )
        {
            if( e.KeyChar == '\n' ) {
                e.Handled = true;
                if ( input ) Text = txt_value.Text;
                else input = true;
            }
        }
        public override string Text {
            get { return txt_value.Text + txt_units.Text; }
            set { float val = 0;
                if( units ==  UnitsType.sec && scale >= UnitScale.Base) {
                    DateTime time;
                    if( DateTime.TryParse(value, out time) ) {
                        long t = time.Hour*3600000;
                        t += time.Minute * 60000;
                        t += time.Second * 1000;
                        t += time.Millisecond;
                        Value = (float)((double)t / 1000.0);
                    }
                } else if( float.TryParse( value, out val ) ) {
                    val *= modif;
                    if( input ) {
                       if( val >= this.cover.MIN && val <= this.cover.MAX )
                           Value = val;
                    } else Value = val;
                }
            }
        }


        public float Range {
            get { return cover.MAX - cover.MIN; }
        }

        public float Minimum {
            get { return cover.MIN; }
            set { cover.MIN = value;
                valence().SetDirty( ValenceFieldState.Flags.MIN );
            }
        }
        public float Maximum {
            get { return cover.MAX; }
            set { cover.MAX = value;
                valence().SetDirty( ValenceFieldState.Flags.MAX );
            }
        }
        public float Movement {
            get { return cover.MOV; }
            set { cover.MOV = value;
                valence().SetDirty( ValenceFieldState.Flags.MOV );
            }
        }
        public float Value {
            get { return cover; }
            set { cover.VAL = value;
                  valence().SetDirty( ValenceFieldState.Flags.VAL );
                if( invalidateExternal != null ) {
                    invalidateExternal();
                } else { input = false;                  
                    txt_value.Text = CreateText();
                } 
            }
        }
        public float Proportion {
            get { return ((cover - cover.MIN) / Range); }
            set { Value = ((Range * value) + cover.MIN); }
        }
        public new UnitScale Scale {
            get { return scale; }
            set { if( value != scale ) {
                    if( (int)value > (int)UnitScale.G )
                        value = UnitScale.G;
                    else if( (int)value < (int)UnitScale.p )
                        value = UnitScale.p;
                    modif = (float)Math.Pow( 1000.0, (double)value );
                    ScaleChange( value );
                }
            }
        }

        public UnitsType Units {
            get { return units; }
            set { if( value != units ) {
                    if( value > UnitsType.Per || value < UnitsType.CUSTOM )
                        value = UnitsType.CUSTOM;
                    if( value == UnitsType.Per )
                        custo = "%";
                    UnitsChange( scale, value );
                }
            }
        }

        private void UnitsChange( UnitScale toScale, UnitsType toType )
        {
            string newScale = toScale != UnitScale.Base
                            ? " " + toScale.ToString()
                            : " ";
            string newUnits = toType > UnitsType.CUSTOM 
                           && toType < UnitsType.Per
                            ? toType.ToString()
                            : custo;
            txt_units.Text = newScale + newUnits;
            scale = toScale;
            units = toType;
        }

        private void ScaleChange( UnitScale toScale )
        {
            UnitsChange( toScale, units );
            input = false;
            txt_value.Text = CreateText();
            input = true;
        }

        private void BoundsChange()
        {
            frame = (int)(0.5f+(float)Bounds.Height*0.15625f);
            int height = Bounds.Height - (2 * frame);
            int unitsW = (int)(Bounds.Height * 1.5f);
            int unitsL = Bounds.Width-(unitsW+frame);
            int valueW = unitsL-frame;
            txt_value.SetBounds( frame, frame, valueW, height );
            txt_value.Font = new Font( txt_value.Font.FontFamily, height-frame,
                                       FontStyle.Regular, GraphicsUnit.Pixel );
            txt_units.SetBounds( unitsL, frame, unitsW, height );
            txt_units.Font = new Font( txt_value.Font, FontStyle.Bold );
        }

        protected override void OnResize( EventArgs e )
        {
            base.OnResize(e);
            BoundsChange();
        }

        private void GuiValue_Paint( object sender, PaintEventArgs e )
        {
            if( dirty ) {
                dirty = false;
                input = false;
                txt_value.Text = CreateText();
                input = true;
            }
            System.Drawing.Rectangle area = Bounds;
            area.Location = Point.Empty;
            area.Width = frame;
            e.Graphics.DrawImage( stylers[style].image, area,
                                  borders[0], GraphicsUnit.Pixel );
            area.X = Bounds.Width - frame;
            e.Graphics.DrawImage( stylers[style].image, area, 
                                  borders[2], GraphicsUnit.Pixel );
            area = Bounds;
            area.Y = area.Height - frame;
            area.Height = area.X = frame;
            area.Width -= (frame + frame);
            e.Graphics.DrawImage( stylers[style].image, area,
                                  borders[3], GraphicsUnit.Pixel );
            area.Y = 0;
            e.Graphics.DrawImage( stylers[style].image, area,
                                  borders[1], GraphicsUnit.Pixel );
        }

        private void OnWrapInvalidated( InvalidateEventArgs e )
        {
            dirty = true;
            Invalidate();            
        }
        
        private bool canChangeUnits = true;
        public  bool CanChangeUnits {
            get { return canChangeUnits; }
            set { if(canChangeUnits!=value) {
                    if(!value) {
                        mnu_units.Container.Remove( mnu_units_select );
                    } else {
                        mnu_units.Container.Add( mnu_units_select );
                    } canChangeUnits = value;
                }
            }
        }

        private bool canScaleUnits = true;
        public  bool CanScaleUnits {
            get { return canScaleUnits; }
            set { if( canScaleUnits != value ) {
                    if(!value) {
                        mnu_units.Container.Remove( mnu_units_up );
                        mnu_units.Container.Remove( mnu_units_down );
                    } else {
                        mnu_units.Container.Add( mnu_units_up );
                        mnu_units.Container.Add( mnu_units_down );
                    } canScaleUnits = value;
                }
            }
        }

        public bool HasMidiMenu {
            get { return txt_value.ContextMenuStrip == mnu_value; }
            set { if( value ) {
                    if( txt_value.ContextMenuStrip != mnu_value )
                        txt_value.ContextMenuStrip = mnu_value;
                } else {
                    txt_value.ContextMenuStrip = null;
                }
            }
        }

        private void ToolStripMenuItem_Click( object sender, EventArgs e )
        {
            switch( (sender as ToolStripMenuItem).Text ) {
                case "Scale Up": Scale = (UnitScale)((int)scale + 1);
                    break;
                case "Scale Down": Scale = (UnitScale)((int)scale - 1);
                    break;
            }
        }

        private void mnu_units_select_TextChanged( object sender, EventArgs e )
        {
            Units = (UnitsType)(sender as ToolStripComboBox).SelectedIndex;
        }

        
        private void mnu_value_refer_Sellector_SelectedIndexChanged( object sender, EventArgs e )
        {
            Valence.FieldDescriptor target = compatibles[mnu_value_refer_Sellector.SelectedIndex];
            Wrap( target.Element.valence<Controlled.Float32>( target.Index )
              as IControllerValenceField<Controlled.Float32> );
        }

        private void mnu_value_reference_CheckStateChanged( object sender, EventArgs e )
        {
            if( (sender as ToolStripMenuItem).Checked ) {
                int indexOfThisSelf = 0;
                if (mnu_value_refer_Sellector.Items.Count > 0)
                    mnu_value_refer_Sellector.Items.Clear();
                compatibles = Valence.CompatibleFields<Controlled.Float32>();
                mnu_value_refer_Sellector.Enabled = false;
                foreach (Valence.FieldDescriptor field in compatibles) {
                    if( field.Element != this ) {
                        mnu_value_refer_Sellector.Items.Add( field.ToString() );
                    } else {
                        indexOfThisSelf = mnu_value_refer_Sellector.Items.Count;
                        mnu_value_refer_Sellector.Items.Add(
                            "Self" + string.Format( "->{0} field", field.Index )
                                                              );
                    }
                } mnu_value_refer_Sellector.SelectedIndex = indexOfThisSelf;
            } else {
                mnu_value_refer_Sellector.Enabled = false;
                mnu_value_refer_Sellector.SelectedIndex = 0;
            } mnu_value_refer_Sellector.Enabled = true;
        }
    }

}


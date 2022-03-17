using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if USE_WITH_WF
using System.Windows.Forms;
using System.Drawing.Imaging;
using Point  = System.Drawing.Point;
using Rect   = System.Drawing.Rectangle;
using RectF  = System.Drawing.RectangleF;
using Color  = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;
using Brush  = System.Drawing.SolidBrush;
using AbstractBrush = System.Drawing.Brush;
#elif USE_WITH_WPF
using System.Windows.Controls;
using Point  = System.Windows.Point;
using Rect   = System.Windows.Int32Rect;
using RectF  = System.Windows.Rect;
#endif

namespace Stepflow.Gui.Helpers
{
    [StructLayout(LayoutKind.Explicit,Size=4,Pack=1)]
    public struct LedGlower
    {
        private const int NOALPHA = 0x00ffffff;
        private LED ledState( int next ) {
            return ledCol[next] != (color & NOALPHA)
                 ? ledState( next+1 ) : (LED)next;
        }
        public enum GrundFarben
        { 
            Green = 0x00FF00,
            Red = 0xFF3030,
            Gelb = 0xFFFF10,
            Blue = 0x2030FF,
            Orange = 0xFF6400,
            Mint = 0x00B080,
            Pink = 0xFF80FF,
            Cyan = 0x00FFFF,
            Dark = 0x101010
        } 
        public static readonly int[] ledCol = {
            0x00FF00,0xFF3030,0xFFFF10,
            0x2030FF,0xFF6400,0x00B080,
            0xFF80FF,0x00FFFF,0x101010
        };

        [FieldOffset(0)]
        private int color;
        [FieldOffset(3)]
        private byte alpha;

        public float Glim {
            get { return (float)alpha/255; }
            set { alpha = (byte)(value*255); }
        }
        public Color Glow {
            get { return Color.FromArgb(color); }
            set { byte remember = alpha;
                color = value.ToArgb();
                alpha = remember;
            }
        }
        public LED Led {
            get { return ledState(0); }
            set { byte remember = alpha;
                color = ledCol[(int)value];
                alpha = remember;
            }
        }
        public bool Off {
            get { return (( color & NOALPHA ) == ledCol[8] ) 
                         || alpha == 0; }
            set { alpha = (byte)(value ? 0 : 1); }
        }
    }

    #if USE_WITH_WF
    public struct LedGlimmer {
        public enum Mode {
            AutoRGBAdjust,
            AutoBrushSelect
        }
        private int                 spritesheet;
        private float               hueRotation;
        private List<Bitmap>        spriteimage;
        private Dictionary<int,int> imageMapper; 
        private List<IRectangle[]>  spriteframe;
        private ImageAttributes     attributzke;
        private ColorMatrix         alfamatrize;
        private LedGlower           lowerglower;
        private Brush               glowerBrush;
        private Brush               darkenBrush;
        private Mode                autoRGBmode;
        private float               targetValue;

        public LedGlimmer( Bitmap img, float a ) {
            targetValue = 1;
            autoRGBmode = Mode.AutoBrushSelect;
            spritesheet = 0;
            spriteimage = new List<Bitmap>(1);
            lowerglower = new LedGlower();
            lowerglower.Led = LED.off;
            spriteframe = new List<IRectangle[]>(1);
            imageMapper = new Dictionary<int,int>(1);
            spriteframe.Add( new IRectangle[8] );
            imageMapper.Add( 0,0 );
            spriteimage.Add( img );
            attributzke = new ImageAttributes();
            glowerBrush = new Brush( lowerglower.Glow );
            alfamatrize = new ColorMatrix( new float[5][] {
                new float[]{ 1,0,0,0,0 },
                new float[]{ 0,1,0,0,0 },
                new float[]{ 0,0,1,0,0 },
                new float[]{ 0,0,0,a,0 },
                new float[]{ 0,0,0,0,1 }
            } );
            darkenBrush = new Brush( Color.Black );
            Pre = 1;
            hueRotation = 0;
            SetSheet( spritesheet );
            int step = spriteimage[imageMapper[spritesheet]].Height/8;
            for ( int i=0; i<8; ++i )
                spriteframe[spritesheet][i] =
                    Rectangle<CenterAndScale>.Create(
                        StorageLayout.CornerAndSizes, 0, i*step, step, step
                                                        );
        }

        public LedGlimmer( Bitmap img, float alf, IRectangle[] src )
            : this( img, alf )
        {
            spriteframe[spritesheet] = src;
        }

        public static implicit operator Bitmap( LedGlimmer cast ) {
            return cast.spriteimage[cast.imageMapper[cast.spritesheet]];
        }
        public static implicit operator ImageAttributes( LedGlimmer cast ) {
            return cast.attributzke;
        }
        public static implicit operator AbstractBrush( LedGlimmer cast ) {
            return cast.glowerBrush;
        }
        public static implicit operator Color( LedGlimmer cast ) {
            return cast.lowerglower.Glow;
        }
        public static implicit operator Rect( LedGlimmer source ) {
            return source.spriteframe[0][(int)source.Led].ToRectangle();
        }

        public float Pre;

        public LED Led {
            get { return Off ? LED.off : lowerglower.Led; }
            set { lowerglower.Led = value;
                if( autoRGBmode == Mode.AutoRGBAdjust ) {
                    Rgb = lowerglower.Glow;
                } else {
                    glowerBrush.Color = lowerglower.Glow;
                }
            }
        }
        public float Lum {
            get { return alfamatrize.Matrix33 / Pre; }
            set { lowerglower.Glim = value;
                targetValue = (value * Pre);            
                glowerBrush.Color = lowerglower.Glow;
            }
        }
        public Color Rgb {
            get { return Color.FromArgb( 255,
                    (int)(alfamatrize.Matrix00*255),
                    (int)(alfamatrize.Matrix11*255),
                    (int)(alfamatrize.Matrix22*255) ); }
            set { alfamatrize.Matrix00 = (float)value.R / 255;
                  alfamatrize.Matrix11 = (float)value.G / 255;
                  alfamatrize.Matrix22 = (float)value.B / 255;
                attributzke.SetColorMatrix( alfamatrize );
            }
        }
        private ColorMatrix MatrixProduct( ColorMatrix m1, ColorMatrix m2 )
        {
            ColorMatrix outp = new ColorMatrix();
            for(int i = 0; i < 5; ++i ) {
                for(int j=0; j<5;++j ) {
                    outp[i,j] = 0;
                    for(int c = 0; c < 5; ++c ) {
                        outp[i,j] += (m1[i,c] * m2[c,j]);
                    }
                }
            } return outp;
        }

        public float Hue {
            get { return hueRotation; }
            set { if ( value != hueRotation ) {
                    float cos = (float)(value*System.Math.PI/180);
                    float sin = (float)Math.Sin(cos);
                          cos = (float)Math.Cos(cos);
                    ColorMatrix m1 = new ColorMatrix();
                    ColorMatrix m2 = new ColorMatrix();
                    m1.Matrix33 = alfamatrize.Matrix33;
                    m1.Matrix00 =  cos; m1.Matrix01 = sin;
                    m1.Matrix10 = -sin; m1.Matrix11 = cos;
                    m2.Matrix11 =  cos; m2.Matrix12 = sin;
                    m2.Matrix21 = -sin; m2.Matrix22 = cos;
                    m1 = MatrixProduct( m1, m2 );
                    m2 = new ColorMatrix();
                    m2.Matrix00 = cos; m2.Matrix02 = -sin;
                    m2.Matrix20 = sin; m2.Matrix22 = cos;
                    alfamatrize = MatrixProduct( m1, m2 );
                    attributzke.SetColorMatrix(
                                   alfamatrize );
                    hueRotation = value;
                }
            }
        }
        public void SetStyle( Style style ) {
            switch( style ) {
                case Style.Flat: darkenBrush.Color = Color.FromArgb( 255, Color.FromArgb(0x7a7a7a) ); break;
                case Style.Lite: darkenBrush.Color = Color.FromArgb( 255, 56, 56, 56 ); break;
                case Style.Dark: darkenBrush.Color = Color.FromArgb( 255, 
                    Color.FromArgb( LedGlower.ledCol[(int)LED.off]) ); break;
            } Pre = 1.0f - (float)style/3;
        }
        public bool Off {
            get { return lowerglower.Off; }
            set { lowerglower.Off = value; }
        }
        public void DrawBrush( System.Drawing.Graphics ctx, IRectangle dst ) {
            ctx.FillRectangle( darkenBrush, dst.X, dst.Y, dst.W, dst.H );
            if( !Off ) ctx.FillRectangle( this, dst.X, dst.Y, dst.W, dst.H );
        }
        public void DrawBrush( System.Drawing.Graphics ctx, Rect dst ) {
            ctx.FillRectangle( darkenBrush, dst );
            if( !Off ) ctx.FillRectangle( this, dst );
        }
        public void DrawSprite( System.Drawing.Graphics ctx, IRectangle dst )
        {
            DrawSprite( ctx, dst.ToRectangle() );
        }
        public void DrawSprite( System.Drawing.Graphics ctx, Rect dst ) {
            if( !Off ) {
                IRectangle src = spriteframe[spritesheet][(int)Led];
                if( alfamatrize.Matrix33 != targetValue ) {
                    float d = targetValue - alfamatrize.Matrix33;
                    int s = d < 0 ? -1 : 1;
                    d = Math.Abs(d);
                    d = (d > 0.075f ? 0.075f : d) * s;
                    alfamatrize.Matrix33 += d;
                    attributzke.SetColorMatrix( alfamatrize );
                } ctx.DrawImage( this, dst, src.X, src.Y, src.W, src.H, 
                                 System.Drawing.GraphicsUnit.Pixel,
                                 attributzke );
            }
        }

        public void SetModus( Mode automode ) {
            autoRGBmode = automode;
        }

        public void SetImage( Bitmap img ) {
            spriteimage[imageMapper[spritesheet]] = img;
        }
        public void AddImage( Bitmap img ) {
            int newIdx = spriteimage.Count;
            spriteimage.Add( img );
            imageMapper[spritesheet] = newIdx;
        }
        public void SetSource( LED led, int x, int y, int w, int h ) {
            spriteframe[spritesheet][(int)led] = Rectangle<CenterAndScale>.Create(
                                         StorageLayout.CornerAndSizes, x, y, w, h );
        }
        public void SetSource( LED forLed, IRectangle source ) {
            spriteframe[spritesheet][(int)forLed] = source;
        }
        public void SetSheet( int number ) {
            while( number >= spriteframe.Count ) {
                spriteframe.Add( new IRectangle[8] );
            } if( !imageMapper.ContainsKey( number ) ) {
                imageMapper.Add( number, spritesheet );
            } spritesheet = number;
        }
        public void AddSheet( IRectangle[] sheet ) {
            int set = spriteframe.Count;
            spriteframe.Add( sheet );
            SetSheet( set );
        }

        public void AddSheet( Rect[] sheet ) {
            int set = spriteframe.Count;
            spriteframe.Add( new IRectangle[sheet.Length] );
            for(int i=0;i<sheet.Length;++i ) {
                spriteframe[set][i] = CenterAndScale.FromRectangle( sheet[i] );
            } SetSheet( set );
        }
    }
    #endif
}

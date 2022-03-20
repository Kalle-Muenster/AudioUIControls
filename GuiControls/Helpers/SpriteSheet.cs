using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Layout;

using Stepflow;
using Stepflow.Helpers;
using Stepflow.Gui.Helpers;

using Win32Imports.Touch;

namespace Stepflow.Gui
{
    public class SpriteSheet
    {
        static SpriteSheet()
        {
#if DEBUG
            Consola.StdStream.Init( Consola.CreationFlags.TryConsole );
            MessageLogger.SetErrorWriter( Consola.StdStream.Err.WriteLine );
            MessageLogger.SetLogWriter( Consola.StdStream.Out.WriteLine );
#else
            MessageLogger.SetLoggingLevel( MessageLogger.Level.Garnich );
#endif
        }
        public interface ISprite
        {
            Image       image { get; }
            IRectangle  frame { get; }
            float       alpha { get; set; }
            Color       color { get; set; }
            ImageAttributes attributes { get; }
            void Draw( Graphics g, IRectangle dst );
            void Draw( Graphics g, Rectangle dst );
        }

        public class IndexBasedSprite : ISprite
        {
            protected int         img;
            protected int         idA;
            protected int         idB;
            protected int         idC;
            protected SpriteSheet set;

            public virtual Image      image { get { return set.bitmaps[img]; } }
            public virtual IRectangle frame { get { return set.sources[idA][idB][idC]; } }
            float ISprite.alpha { get; set; }
            Color ISprite.color { get; set; }
            ImageAttributes ISprite.attributes { get { return set.attributes; } }
            protected ISprite sprite() { return this; }
            
            public IndexBasedSprite( SpriteSheet parentSheet, int spriteGroup, int styledGroup, int sourceFrame, int sourceImage )
            {
                set = parentSheet;
                img = sourceImage;
                idA = spriteGroup;
                idB = styledGroup;
                idC = sourceFrame;
            }

            virtual public void Draw( Graphics g, Rectangle dstRec )
            {
                if ( sprite().attributes == null ) {
                    g.DrawImage( sprite().image, dstRec, frame.ToRectangle(), GraphicsUnit.Pixel );
                } else {
                    IRectangle srcRec = frame;
                    if( set.dirtOnTheMatrix ) {
                        set.attributes.SetColorMatrix( set.color, ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
                        set.dirtOnTheMatrix = false;
                    }
                    g.DrawImage( sprite().image, dstRec, srcRec.X, srcRec.Y, srcRec.W, srcRec.H, GraphicsUnit.Pixel, set.attributes );
                }
            }

            virtual public void Draw( Graphics g, IRectangle dstRec )
            {
                Draw( g, dstRec.ToRectangle() );
            }
        }

        public class RectangleBasedSprite : ISprite
        {
            protected int           img;
            protected SpriteSheet   set;
            protected SystemDefault rec;

            public virtual Image      image { get { return set.bitmaps[img]; } }
            public virtual IRectangle frame { get { return rec; } }
            float ISprite.alpha { get; set; }
            Color ISprite.color { get; set; }
            ImageAttributes ISprite.attributes { get { return set.attributes; } }
            protected ISprite sprite() { return this; }
            
            public RectangleBasedSprite( SpriteSheet parentSheet, IRectangle sourceFrame, int sourceImage )
            {
                set = parentSheet;
                img = sourceImage;
                rec = (SystemDefault)sourceFrame.cast<SystemDefault>();
            }

            virtual public void Draw( Graphics g, Rectangle dstRec )
            {
                g.DrawImage( image, dstRec, rec.ToRectangle(), GraphicsUnit.Pixel );
            }

            virtual public void Draw( Graphics g, IRectangle dstRec )
            {
                g.DrawImage( sprite().image, dstRec.ToRectangle(), rec.ToRectangle(), GraphicsUnit.Pixel );
            }
        }

        public class ColorSprite : RectangleBasedSprite, ISprite
        {
            protected ImageAttributes attributes;
            protected ColorMatrix     colormatrx;
            protected Color           colorvalue;
            protected bool            colordirty;

            ImageAttributes ISprite.attributes { get { return this.attributes; } }

            public Color color {
                get { return colorvalue; }
                set { if( colorvalue != value ) { colordirty = true;
                        colormatrx.Matrix00 = (float)value.R / 255;
                        colormatrx.Matrix11 = (float)value.G / 255;
                        colormatrx.Matrix22 = (float)value.B / 255;
                        colormatrx.Matrix33 = (float)value.A / 255;
                        colorvalue = value;
                    }
                }
            }
            public float alpha {
                get { return colormatrx.Matrix33; }
                set { if ( colormatrx.Matrix33 != value ) {
                        colorvalue = Color.FromArgb( (int)(value * 255), colorvalue );
                        colormatrx.Matrix33 = value;
                        colordirty = true;
                    } 
                }
            }
            public ColorSprite( SpriteSheet sourceSheet, int sourceImage, IRectangle sourceFrame, Color drawColor )
                : base( sourceSheet, sourceFrame, sourceImage )
            {
                colordirty = true;
                colormatrx = new ColorMatrix(
                    new float[5][] {
                        new float[]{ (float)drawColor.R/255,0,0,0,0 },
                        new float[]{ 0,(float)drawColor.G/255,0,0,0 },
                        new float[]{ 0,0,(float)drawColor.B/255,0,0 },
                        new float[]{ 0,0,0,(float)drawColor.A/255,0 },
                        new float[]{ 0,0,0,0,1 }
                    }
                );
            }
            private void updateMatrices()
            {
                attributes.SetColorMatrix( colormatrx, ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
                colordirty = false;
            }
            public override void Draw( Graphics g, IRectangle dstRec )
            {
                Draw( g, dstRec.ToRectangle() );
            }
            public override void Draw( Graphics g, Rectangle dst )
            {
                if( colordirty ) updateMatrices();
                g.DrawImage( image, dst, rec.X, rec.Y, rec.W, rec.H, GraphicsUnit.Pixel, attributes );
            }
        }

        public class Loader
        {
            private System.Xml.XPath.XPathNavigator xml;
            private System.Xml.XPath.XPathNavigator elm;
            private StringBuilder                   pth;
            private Enum                            enm;
            private string                          val;
            private Assembly                        asm;
            
            public enum ElmType { sprite = 2, array = 3,
                           SpriteSheet = -1, element = 0,
                                 group = 1, unknown = -2 }

            public Loader()
            {
                asm = Assembly.GetEntryAssembly();
                enm = null;
                elm = null;
                xml = null;
                pth = new StringBuilder("//SpriteSheet/");
                val = "";
            }

            public Loader( string xmlstring ) : this()
            {
                xml = new System.Xml.XPath.XPathDocument(
                      new System.IO.StringReader( xmlstring ) 
                ).CreateNavigator();
            }

            public Loader( System.IO.FileInfo xmlfile ) : this()
            {
                xml = new System.Xml.XPath.XPathDocument(
                      xmlfile.OpenText() 
                ).CreateNavigator();
            }

            public int elmCount()
            {
                return elmCount( pth.ToString() );
            }
            public int elmCount( string element )
            {
                if( element == null ) { if ( elm == null )
                    element = "//SpriteSheet";
                }
                pth.Clear();
                pth.Append( element );
                int count = 0;
                if( Int32.TryParse( xml.SelectSingleNode( pth.ToString()+"/@count" ).ToString(), out count) ) {
                        return count;
                } return 0;
            }

            public ElmType elmType()
            {
                ElmType typ = ElmType.unknown;
                if( elm != null ) {
                    Enum.TryParse<ElmType>( elm.Name, out typ );
                } return typ;
            }
            
            public string getName()
            {
                return getName(null);
            }

            public string getName( string element )
            {
                if ( element == null ) { if ( pth == null )
                    pth = new StringBuilder("//SpriteSheet");
                } else {
                    pth.Clear();
                    pth.Append( element ); 
                    val =  xml.SelectSingleNode( pth.ToString()+"/@name" ).ToString();
                } if( val == null ) val = "";
                return val;
            }

            public bool hasEnum()
            {
                return hasEnum( null );
            }
            public bool hasEnum( string forelm )
            {
                if ( forelm != null )
                    val = xml.SelectSingleNode( forelm + "/@enum" ).ToString();
                else {
                    val = xml.SelectSingleNode( pth.ToString() + "/@enum" ).ToString();
                    Type typ = asm.GetType( val, false );
                    if ( typ != null ) {
                        enm = Enum.Parse( typ, val ) as Enum;
                        return enm != null;
                    }
                } return false;
            }

            public Array enmValues()
            {
                if( enm != null )
                    return Enum.GetValues( enm.GetType() );
                return Array.Empty<Enum>();
            }

            public StringBuilder makeXPath( string name, ElmType type )
            {
                string[] names = name.Split('.');
                pth.Clear();
                pth.AppendFormat( "//SpriteSheet/element[@name='{0}']", names[0] );
                for ( int i = 1; i < names.Length-1; ++i ) {
                    if ( names[i].Length > 0 ) {
                        pth.AppendFormat( "/group[@name='{0}']",names[i] );
                    }
                }
                if( type > 0 ) {
                    pth.AppendFormat( "/{0}[@name='{1}']", type, names[names.Length-1] );
                } return pth;
            }

            public string addXPart( StringBuilder xpath, string name, ElmType type )
            {
                string[] names = name.Split('.');
                for ( int i = 0; i < names.Length-1; ++i ) {
                    if ( names[i].Length > 0 ) {
                        xpath.AppendFormat( "/group[@name='{0}']",names[i] );
                    }
                }
                if( type >= 0 ) {
                    xpath.AppendFormat( "/{0}[@name='{1}']", type, names[names.Length-1] );
                } return xpath.ToString();
            }

            public StringBuilder makeXPath( Enum element, string name, ElmType type )
            {
                string[] names = name.Split('.');
                pth.Clear();
                pth.AppendFormat( "//SpriteSheet/element[@name='{0}']", element.ToString() );
                for ( int i=0; i < names.Length-1; ++i ) {
                    if (names[i].Length > 0) {
                        pth.AppendFormat( "/group[@name='{0}']",names[i] );
                    }
                }
                if( type > 0 ) {
                      pth.AppendFormat( "/{0}[@name='{1}']", type, names[names.Length-1] );
                } return pth;
            }

            public Bitmap getImage( string xpath )
            {
                return GuiControls.Properties.Resources.ResourceManager.GetObject(
                       xml.SelectSingleNode( xpath + "/@image" ).ToString()
                ) as Bitmap;
            }

            public IRectangle getSprite( Enum element, string name )
            {
                return getValues(
                    makeXPath( element, name, ElmType.sprite ).ToString()
                                                 );
            }

            private CornerAndSize getValues( string xpres )
            {
                int X = xml.SelectSingleNode( xpres+"/X/text()" ).ValueAsInt;
                int Y = xml.SelectSingleNode( xpres+"/Y/text()" ).ValueAsInt;
                int W = xml.SelectSingleNode( xpres+"/W/text()" ).ValueAsInt;
                int H = xml.SelectSingleNode( xpres+"/H/text()" ).ValueAsInt;
                return new CornerAndSize( X, Y, W, H );
            }

            public IRectangle[] getArray( Enum element, string name )
            {
                string xpath = makeXPath( element, name, ElmType.array ).ToString();
                Size img_size = getImage( xpath ).Size;
                int box = int.Parse( xml.SelectSingleNode(xpath+"/@box").ToString() );
                int count = int.Parse( xml.SelectSingleNode(xpath+"/@count").ToString() );
                char dir = xml.SelectSingleNode(xpath+"/@dir").ToString()[0];
                CornerAndSize rect = getValues( xpath );
                Point32 origin = rect.Corner;
                IRectangle[] load = new IRectangle[count];
                Point32 pt = Point32.ZERO;
                pt.X = dir == 'X' ? box : 0;
                pt.Y = dir == 'Y' ? box : 0;
                for( int i = 0; i < count; ++i ) {
                    load[i] = Rectangle<CenterAndScale>.Create(
                                   StorageLayout.CornerAndSizes,
                                   rect.X,rect.Y, rect.W,rect.H );
                    rect.Corner += pt;
                    if( rect.X >= img_size.Width ) {
                        rect.X = origin.x;
                        rect.Corner.Y += box;
                    }
                    if( rect.Y >= img_size.Height ) {
                        rect.Y = origin.y;
                        rect.Corner.X += box;
                    }
                } return load;
            }
        }

        private ImageAttributes            attributes;
        private ColorMatrix                color;
        private bool                       dirtOnTheMatrix;
        private Bitmap[]                   bitmaps;
        private IRectangle[][][]           sources;
        private ISprite[][][]              sprites;
        public  string                     Name;
        private int                        aaa;
        private int                        bbb;
        private int                        ccc;

        internal SpriteSheet( Bitmap[] images, IRectangle[][][] rects )
        {
            bitmaps = images;
            sources = rects;
            Initialize();
        }

        public SpriteSheet( int imgCount, int AAAcount, int BBBcount, int CCCcount )
        {
            bitmaps = new Bitmap[imgCount];
            sources = new IRectangle[AAAcount][][];
            for(int i=0;i<AAAcount;++i ) {
                sources[i] = new IRectangle[BBBcount][];
                for(int c = 0; c < BBBcount; ++c ) {
                    sources[i][c] = new IRectangle[CCCcount];
                }
            }
            aaa = AAAcount;
            bbb = BBBcount;
            ccc = CCCcount;
        }

        public void Initialize()
        {
            sprites = new ISprite[sources.Length][][];
            for(int a=0;a<sources.Length;++a ) {
                sprites[a] = new ISprite[sources[a].Length][];
                for(int b = 0; b < sources[a].Length; ++b ) {
                    sprites[a][b] = new ISprite[sources[a][b].Length];
                    for(int c = 0; c < sources[a][b].Length; ++c ) {
                        sprites[a][b][c] = new IndexBasedSprite(this, a, b, c, a);
                    }
                }
            }
            aaa = bbb = ccc = 0;
            attributes = null;
            color = null;
        }

        ISprite Current {
            get { return sprites[aaa][bbb][ccc]; }
        }

        private void enableColor(bool enable)
        {
            if (enable) {
            if( attributes==null ) {
                attributes = new ImageAttributes();
            }
            if( color == null ) {
                color = new ColorMatrix(
                    new float[5][] {
                        new float[]{ 1,0,0,0,0 },
                        new float[]{ 0,1,0,0,0 },
                        new float[]{ 0,0,1,0,0 },
                        new float[]{ 0,0,0,1,0 },
                        new float[]{ 0,0,0,0,1 }
                    }
                );
            }
            } else {
                attributes.ClearColorMatrix(ColorAdjustType.Bitmap);
                attributes.ClearGamma(ColorAdjustType.Bitmap);
                attributes = null;
                color = null;
            }
        } 

        public void SetColor( Color drawColor )
        {
            enableColor( true );
            color.Matrix00 = (float)drawColor.R / 255;
            color.Matrix11 = (float)drawColor.G / 255;
            color.Matrix22 = (float)drawColor.B / 255;
            color.Matrix33 = (float)drawColor.A / 255;
            dirtOnTheMatrix = true;
        }
        public void SetAlpha( float alpha )
        {
            enableColor( true );
            color.Matrix33 = alpha;
            dirtOnTheMatrix = true;
        }

        public void SetGamma( float gamma )
        {
            if( attributes == null ) attributes = new ImageAttributes();
            attributes.SetGamma( gamma );
        }

        public Color Color {
            get { return attributes == null? Color.Transparent : Color.FromArgb((int)(color.Matrix00*255),(int)(color.Matrix11*255),(int)(color.Matrix22*255),(int)(color.Matrix33*255)); }
            set { SetColor(value); }
        }

        public void SetSprite( int num )
        {
            ccc = num;
        }
        public void SetSprite( Enum frame )
        {
            ccc = frame.ToInt32();
        }
        public ISprite GetSprite( int frame )
        {
            return sprites[aaa][bbb][ccc=frame];
        }
        public ISprite GetSprite( Enum idx )
        {
            return sprites[aaa][bbb][ccc=idx.ToInt32()];
        }
        public void SetGroup( Enum group )
        {
            aaa = group.ToInt32();
        }
        public E GetGroup<E>() where E : struct, IConvertible
        {
            return (E)Enum.GetValues(typeof(E)).GetValue(aaa);
        }
        public void SetLoop( Enum loop )
        {
            bbb = loop.ToInt32();
        }
        public E GetLoop<E>() where E : struct, IConvertible
        {
            return (E)Enum.GetValues(typeof(E)).GetValue(bbb);
        }
        public void SetSprite( Enum group, Enum loop )
        {
            aaa = group.ToInt32();
            bbb = loop.ToInt32();
        }
        public ISprite GetSprite( int orient, int style, int frame )
        {
            return sprites[aaa=orient][bbb=style][frame];
        } 
    }
}

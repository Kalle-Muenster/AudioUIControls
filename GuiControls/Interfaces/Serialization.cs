using System;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using Win32Imports.Midi;
#if USE_WITH_WF
using System.Windows.Forms;
using System.Drawing;
#elif USE_WITH_WPF
using System.Windows.Controls;
using System.Windows.Media;
#endif
using Stepflow;
//using Stepflow.Midi.ControlHelpers;
//using Message = Win32Imports.Midi.Message;

namespace MidiControls
{

    /*
    public interface BindingStorageIO<RType,WType>
    {
        RType reader { get; set; }
        WType writer { get; set; }

        RType createReader(string forChunk);
        WType createWriter(string forChunk);

        string StorageChunk { get; set; }
    }

    abstract public class AutomationBindingStorage<FrameworkComponentType,AP,T,StorageType,R,W> : System.ComponentModel.Component where AP : IAutomationProtocol<T> where T : struct where StorageType : BindingStorageIO<R,W>
    {

        //  public delegate void ControllerChangeDelegate( Value data );

        

        protected StorageType          store; 
        protected System.Windows.Forms.Form entity;
        protected abstract string getEntityName();

       // private string StorageDataIdentifier = null;
        
        abstract protected bool StoreBindings( StorageType writer,string chunkname );
        abstract protected bool LoadeBindings( StorageType reader,string chunkname );
        //{
        //    get
        //    {
        //        if (bindingsXMLfileName == null)
        //        {
        //            bindingsXMLfileName 
        //        }
        //        return bindingsXMLfileName;
        //    }
        //    set
        //    {
        //        bindingsXMLfileName = value;
        //    }
        //}

        abstract public string  StorageChunkIdentifier { get; set; }
        abstract public void    BindElement( FrameworkComponentType element, byte loAddrByte, byte hiAddrByte );

        abstract public FrameworkComponentType FindBoundElement( int AddrLoByte, int AddrHiByte );
        abstract public AutomationlayerAddressat FindBoundSignal( FrameworkComponentType element );

        abstract public void UnBindElement( FrameworkComponentType element );
        abstract public void UnBindSignal( int loAddrByte, int hiAddrByte );

        public bool SaveBindings()
        {
            return StoreBindings( store, StorageChunkIdentifier );
        }
        public bool LoadeBindings()
        {
            return LoadeBindings( store, StorageChunkIdentifier );
        }

    };



    public class XmlAutomationBindingStore : BindingStorageIO<XPathDocument, XmlWriter>
    {
        private FileInfo file;
        public XmlAutomationBindingStore(string filename)
        {
            file = new FileInfo(filename);
        }

        public string StorageChunk {
            get { return file.FullName; }
            set { if( value != file.Name 
                   && value != file.FullName ) {
                    writer.Close();
                    reader = null;
                    file = new FileInfo(value);
                } 
            }
        }

        XPathDocument BindingStorageIO<XPathDocument,XmlWriter>.reader
        {
            get { if( reader == null ) {
                    if( writer != null )
                        writer.Close();
                    reader = ((BindingStorageIO<XPathDocument, XmlWriter>)this)
                             .createReader( file.FullName );
                } return reader;
            } set { if( reader != null )
                    reader = null;
                reader = value;
            }
        }

        XmlWriter BindingStorageIO<XPathDocument, XmlWriter>.writer
        {
            get { if( writer == null ) {
                    if( reader != null )
                        reader = null;
                    writer = ((BindingStorageIO<XmlReader, XmlWriter>)this)
                             .createWriter( file.FullName );
                } return writer;
            } set { if( writer != null )
                    writer.Close();
                writer = value;
            }
        }


        XPathDocument reader;
        XmlWriter writer;

        XPathDocument BindingStorageIO<XPathDocument, XmlWriter>.createReader(string chunkID)
        {
            if( file.FullName != chunkID ) {
                file = new FileInfo(chunkID);
            } if( file.Exists ) {
                return new XPathDocument( file.OpenRead() );
            } else return null;
        }

        XmlWriter BindingStorageIO<XPathDocument, XmlWriter>.createWriter(string chunkID)
        {
            return XmlWriter.Create(chunkID);
        }
    }

    public class AutomationBindingsXmller<AutomationType,MessageType> : AutomationBindingStorage<IAutomationControlable<MessageType>,AutomationType,MessageType,BindingStorageIO<XPathDocument,XmlWriter>,XPathDocument,XmlWriter> where AutomationType : IAutomationProtocol<MessageType> where MessageType : struct
    {
        private Dictionary<IAutomationControlable<MessageType>,AutomationlayerAddressat> elements;

        public AutomationBindingsXmller( System.Windows.Forms.Form parent )
        {
            entity = parent;
            elements = new Dictionary<IAutomationControlable<MessageType>, AutomationlayerAddressat>();
            store = new XmlAutomationBindingStore( StorageChunkIdentifier );
        }

        public override string StorageChunkIdentifier {
            get { return "Content/Sqripts/" + entity.Text + "_"
                       + this.GetType().Name + "Bindings.xml";  }
            set { store.StorageChunk = value; }
        }

        public override void BindElement(IAutomationControlable<MessageType> element, byte loAddrByte, byte hiAddrByte)
        {
            AutomationlayerAddressat adr =  new AutomationlayerAddressat(loAddrByte,hiAddrByte);
            element.RegisterAsMesssageListener( adr );
            if( !elements.ContainsKey(element) )
                elements.Add(element, adr);
        }

        public override IAutomationControlable<MessageType> FindBoundElement(int AddrLoByte, int AddrHiByte)
        {
            foreach(KeyValuePair<IAutomationControlable<MessageType>,AutomationlayerAddressat> control in elements) {
                if( control.Value.hiByte == AddrHiByte && control.Value.loByte == AddrLoByte )
                    return control.Key;  
            } return null;
        }

        public override AutomationlayerAddressat FindBoundSignal(IAutomationControlable<MessageType> element)
        {
            return elements[element];
        }

        public override void UnBindElement(IAutomationControlable<MessageType> element)
        {
            if( elements.ContainsKey(element) )
                elements.Remove(element);
            element.SignOutFromAutomationLoop();
        }

        public override void UnBindSignal(int loAddrByte, int hiAddrByte)
        {
            UnBindElement( FindBoundElement( loAddrByte, hiAddrByte ) );
        }

        protected override string getEntityName()
        {
            return (entity as Control).FindForm().Text;
        }

        protected override bool LoadeBindings(BindingStorageIO<XPathDocument, XmlWriter> reader, string chunkname)
        {         
            XPathNavigator document = reader.reader.CreateNavigator();
            XPathNodeIterator results = document.Select("//element[@=name]/text()");
            if( results.Count == 0 ) return false; 
            string[] names = new string[results.Count];
            while(results.MoveNext())
                names[results.CurrentPosition] = results.Current.Value.ToString();
            foreach(string name in names) {
                int port = document.SelectSingleNode(string.Format("//element[name='{0}']/input/port/text()",name)).ValueAsInt;
                Message.TYPE type = (Message.TYPE)Enum.Parse(typeof(Message.TYPE),document.SelectSingleNode(string.Format("//element[name='{0}']/input/type/text()",name)).Value);
                AutomationlayerAddressat addr = new AutomationlayerAddressat(document.SelectSingleNode(string.Format("//element[name='{0}']/input/addr/text()",name)).ValueAsLong);
                foreach( IAutomationControlable<MessageType> elm in elements.Keys )
                    if( elm.getName() == name ) {
                        elm.RegisterAsMesssageListener( addr );
                        elements[elm] = addr;
                    }
            } return true;
        }

        protected override bool StoreBindings(BindingStorageIO<XPathDocument, XmlWriter> writer, string chunkname)
        {
            writer.writer.WriteStartDocument();
            writer.writer.WriteStartElement("bindings");
             writer.writer.WriteAttributeString( "automation",
              string.Format("{0}", typeof(Message).FullName ) );
             writer.writer.WriteEndAttribute();
            foreach( IAutomationControlable<MessageType> element in elements.Keys ) {
                writer.writer.WriteStartElement("element");
                 writer.writer.WriteAttributeString("name", element.getName());
                 writer.writer.WriteEndAttribute();
                  writer.writer.WriteStartElement("input");
                    writer.writer.WriteStartElement("type");
                      writer.writer.WriteValue(elements[element].tyByte);
                    writer.writer.WriteEndElement();
                    writer.writer.WriteStartElement("addr");
                      writer.writer.WriteValue((uint)elements[element]);
                    writer.writer.WriteEndElement();
                  writer.writer.WriteEndElement();
                writer.writer.WriteEndElement();
            } writer.writer.WriteEndElement();
            writer.writer.WriteEndDocument();
            writer.writer.Close();
            return true;
        }
*/

    /*
public bool SaveBindings( string fileName )
{
System.Xml.XmlWriter xml = System.Xml.XmlWriter.Create(fileName);
xml.WriteStartDocument();
xml.WriteStartElement(this.GetType().Name);
StoreBinding(xml).WriteFullEndElement();
xml.WriteEndDocument();
xml.Close();
return true;
}




public bool LoadBindings(string fileName)
{
System.IO.FileInfo file = new System.IO.FileInfo(fileName);
if (!file.Exists)
   return false;

System.Xml.XmlReader document = System.Xml.XmlReader.Create(file.OpenRead());
while (document.Read())
{
   if (document.IsStartElement(this.GetType().Name))
   {
       System.Xml.XmlReader subtree = document.ReadSubtree();
       subtree.Read();
       while (LoadeBinding(subtree).Read()) ;
   }
}
document.Close();
return true;
}
*/
}


using System.IO;
using System.Text;
using System.Xml;

namespace AMS.Profile
{
    public abstract class XmlBased : Profile
    {
        private Encoding m_encoding = Encoding.UTF8;
        internal XmlBuffer m_buffer;

        protected XmlBased()
        {
        }

        protected XmlBased(string fileName)
          : base(fileName)
        {
        }

        protected XmlBased(XmlBased profile)
          : base(profile)
          => m_encoding = profile.Encoding;

        protected XmlDocument GetXmlDocument()
        {
            if (m_buffer != null)
                return m_buffer.XmlDocument;
            VerifyName();
            if (!File.Exists(Name))
                return null;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Name);
            return xmlDocument;
        }

        protected void Save(XmlDocument doc)
        {
            if (m_buffer != null)
                m_buffer.m_needsFlushing = true;
            else
                doc.Save(Name);
        }

        public XmlBuffer Buffer(bool lockFile)
        {
            if (m_buffer == null)
                m_buffer = new XmlBuffer(this, lockFile);
            return m_buffer;
        }

        public XmlBuffer Buffer() => Buffer(true);

        public bool Buffering => m_buffer != null;

        public Encoding Encoding
        {
            get => m_encoding;
            set
            {
                VerifyNotReadOnly();
                if (m_encoding == value || !RaiseChangeEvent(true, ProfileChangeType.Other, null, nameof(Encoding), value))
                    return;
                m_encoding = value;
                RaiseChangeEvent(false, ProfileChangeType.Other, null, nameof(Encoding), value);
            }
        }
    }
}

namespace AsystentZOOM.VM.Interfaces
{
    public interface IXmlDeserializationCallback
    {
        void OnDeserialized(object sender);
        bool IsDataReady { get; set; }
    }
}

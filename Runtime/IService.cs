namespace Core
{
    public interface IService
    {
        public bool IsServiceInitialized { get; }
        
        public void InitializeService();

        public void DisposeService();
    }
}
namespace UI.Core
{
    public interface IViewData
    {
    }
    
    public interface IViewData<T> : IViewData
    {
        public T GetData();
    }
}
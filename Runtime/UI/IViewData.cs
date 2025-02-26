namespace UI
{
    public interface IViewData
    {
    }
    
    public interface IViewData<T> : IViewData
    {
        public T GetData();
    }
}
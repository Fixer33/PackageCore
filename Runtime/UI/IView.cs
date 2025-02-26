using System;
using UnityEngine;

namespace UI
{
    public class ViewVisibilityArgs : EventArgs
    {
        public bool IsVisible { get; private set; }

        public ViewVisibilityArgs(bool isVisible)
        {
            IsVisible = isVisible;
        }
    }
    
    public interface IView
    {
        public event EventHandler<ViewVisibilityArgs> VisibilityChanged;
        public bool IsInHierarchy { get; }
        public GameObject GameObject { get; }
        protected bool IsObjectAlive { get; }

        public void Show(Action onComplete = null, Action onHide = null, IViewData data = null);
        public void Hide(Action onComplete = null);
        public void ShowInstant(IViewData data = null);
        public void HideInstant();
        public bool IsVisible();

        public static bool IsAlive(IView view)
        {
            return view is { IsObjectAlive: true };
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class IScreenViewExtensions
    {
        public static bool IsAlive(this IView screenView)
        {
            return IView.IsAlive(screenView);
        }
    }
}
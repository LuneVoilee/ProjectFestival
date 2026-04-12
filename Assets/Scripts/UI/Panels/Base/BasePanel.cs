#region

using System;
using Core.Data;
using UnityEngine;

#endregion

namespace UI
{
    public abstract class BasePanel : MonoBehaviour
    {
        [NonSerialized] public RectTransform Canvas;
        [NonSerialized] public bool IsShowing;

        public virtual void OnShow()
        {
            IsShowing = true;
        }

        public virtual void OnHide()
        {
            IsShowing = false;
        }

        protected virtual void OnDestroy()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RemovePanel(this);
            }
        }
    }

    public abstract class BasePanel<TData> : BasePanel where TData : BasePanelData
    {
        protected TData Data { get; private set; }

        public void Bind(TData data)
        {
            Unbind();

            Data = data;
            if (Data != null)
            {
                OnBind();
            }
        }

        public void Unbind()
        {
            if (Data != null)
            {
                OnUnbind();
                Data = null;
            }
        }

        protected abstract void OnBind();

        protected abstract void OnUnbind();


        protected override void OnDestroy()
        {
            Unbind();
            base.OnDestroy();
        }
    }
}
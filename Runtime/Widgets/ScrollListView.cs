using UniMob.UI.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UniMob.UI.Widgets
{
    internal class ScrollListView : View<IScrollListState>
    {
        private RectTransform _contentRoot;
        private ViewMapperBase _mapper;
        private RectMask2D _rectMask;

        protected override void Awake()
        {
            base.Awake();

            _contentRoot = (RectTransform) transform.GetChild(0);
            _rectMask = GetComponent<RectMask2D>();
        }

        protected override void Activate()
        {
            base.Activate();

            if (_mapper == null)
                _mapper = new PooledViewMapper(_contentRoot);
        }

        protected override void Render()
        {
            var useMask = State.UseMask;
            if (_rectMask.enabled != useMask)
            {
                _rectMask.enabled = useMask;
            }

            var children = State.Children;
            var mainAxis = State.MainAxisAlignment;
            var crossAxis = State.CrossAxisAlignment;
            var listSize = State.InnerSize.GetSizeUnbounded();

            if (float.IsInfinity(listSize.y))
            {
                foreach (var child in State.Children)
                {
                    if (float.IsInfinity(child.Size.MaxHeight))
                    {
                        Debug.LogError("Cannot render vertically stretched widgets inside ScrollList.\n" +
                                       $"Try to wrap '{child.GetType().Name}' into another widget with fixed height");
                    }
                }

                return;
            }

            var alignX = crossAxis == CrossAxisAlignment.Start ? Alignment.TopLeft.X
                : crossAxis == CrossAxisAlignment.End ? Alignment.TopRight.X
                : Alignment.Center.X;

            var childAlignment = new Alignment(alignX, Alignment.TopCenter.Y);
            var corner = childAlignment.WithTop();
            var cornerPosition = new Vector2(0, 0);

            // content root
            {
                var contentPivotX = crossAxis == CrossAxisAlignment.Start ? 0.0f
                    : crossAxis == CrossAxisAlignment.End ? 1.0f
                    : 0.5f;

                var contentPivotY = mainAxis == MainAxisAlignment.Start ? 1.0f
                    : mainAxis == MainAxisAlignment.End ? 0.0f
                    : 0.5f;

                _contentRoot.pivot = new Vector2(contentPivotX, contentPivotY);

                LayoutData contentLayout;
                contentLayout.Size = new Vector2(float.PositiveInfinity, listSize.y);
                contentLayout.Alignment = childAlignment;
                contentLayout.Corner = childAlignment;
                contentLayout.CornerPosition = Vector2.zero;
                ViewLayoutUtility.SetLayout(_contentRoot, contentLayout);
            }

            using (var render = _mapper.CreateRender())
            {
                foreach (var child in children)
                {
                    var childSize = child.Size.GetSizeUnbounded();

                    var childView = render.RenderItem(child);

                    LayoutData layout;
                    layout.Size = childSize;
                    layout.Alignment = childAlignment;
                    layout.Corner = corner;
                    layout.CornerPosition = cornerPosition;
                    ViewLayoutUtility.SetLayout(childView.rectTransform, layout);

                    cornerPosition += new Vector2(0, childSize.y);
                }
            }
        }
    }

    internal interface IScrollListState : IViewState
    {
        WidgetSize InnerSize { get; }
        IState[] Children { get; }
        CrossAxisAlignment CrossAxisAlignment { get; }
        MainAxisAlignment MainAxisAlignment { get; }
        bool UseMask { get; }
    }
}
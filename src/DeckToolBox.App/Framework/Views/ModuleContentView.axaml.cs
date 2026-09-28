using System;
using Avalonia;
using Avalonia.Controls;

namespace DeckToolBox.App.Framework.Views;

public partial class ModuleContentView : UserControl
{
    public ModuleContentView()
    {
        InitializeComponent();
    }

    // 切换模块时回到顶部，而不是停留在上一个模块的滚动位置
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Scroller.Offset = Vector.Zero;
    }
}

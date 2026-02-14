using System.Windows;
using System.Windows.Controls;
using Masar.Domain.Enums;

namespace Masar.UI.Controls;

/// <summary>
/// شارة حالة المشروع
/// Project Status Badge Control
/// </summary>
public partial class StatusBadge : UserControl
{
    public StatusBadge()
    {
        InitializeComponent();
    }

    /// <summary>
    /// خاصية الربط: حالة المشروع
    /// </summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(ProjectStatus),
            typeof(StatusBadge),
            new PropertyMetadata(ProjectStatus.Proposed));

    public ProjectStatus Status
    {
        get => (ProjectStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Masar.Application.Reporting.Components;

/// <summary>
/// مكون الترويسة والتذييل
/// </summary>
public class HeaderFooterComponent
{
    private readonly string _reportTitle;
    private readonly bool _isArabic;

    public HeaderFooterComponent(string reportTitle, bool isArabic)
    {
        _reportTitle = reportTitle;
        _isArabic = isArabic;
    }

    /// <summary>
    /// بناء الترويسة
    /// </summary>
    public void ComposeHeader(IContainer container)
    {
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10)
            .Row(row =>
            {
                // الجانب الأيمن - اسم الجامعة
                row.RelativeItem().Text(_isArabic ? "جامعة إقليم سبأ" : "Saba Region University")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                // الجانب الأيسر - التاريخ
                row.RelativeItem().AlignRight().Text(DateTime.Now.ToString(_isArabic ? "yyyy/MM/dd" : "dd/MM/yyyy"))
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
    }

    /// <summary>
    /// بناء التذييل
    /// </summary>
    public void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10)
            .Row(row =>
            {
                // الجانب الأيمن - عنوان التقرير
                row.RelativeItem().Text(_reportTitle)
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);

                // الجانب الأيسر - رقم الصفحة
                row.RelativeItem().AlignRight()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                        text.Span(_isArabic ? "صفحة " : "Page ");
                        text.CurrentPageNumber();
                        text.Span(_isArabic ? " من " : " of ");
                        text.TotalPages();
                    });
            });
    }
}

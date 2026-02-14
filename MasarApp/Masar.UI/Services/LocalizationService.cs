using Masar.Domain.Enums;
using Masar.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Masar.UI.Services;

public class LocalizationService : ILocalizationService
{
    private const string ResourcePrefix = "Resources/Strings.";
    private const string ResourceSuffix = ".xaml";

    public event EventHandler? LanguageChanged;

    public string CurrentLanguage { get; private set; } = "en";

    public bool IsArabic => CurrentLanguage == "ar";

    public string GetString(string key)
    {
        if (System.Windows.Application.Current.Resources.Contains(key))
        {
            return System.Windows.Application.Current.Resources[key]?.ToString() ?? key;
        }

        return key;
    }

    public void SetLanguage(string languageCode)
    {
        if (string.Equals(CurrentLanguage, languageCode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CurrentLanguage = languageCode;
        UpdateResourceDictionary(languageCode);
        UpdateFlowDirection(languageCode);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ToggleLanguage()
    {
        SetLanguage(IsArabic ? "en" : "ar");
    }

    public string GetStatusLabel(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Proposed => IsArabic ? "مقترح" : "Proposed",
            ProjectStatus.Approved => IsArabic ? "معتمد" : "Approved",
            ProjectStatus.InProgress => IsArabic ? "قيد التنفيذ" : "In Progress",
            ProjectStatus.Completed => IsArabic ? "مكتمل" : "Completed",
            ProjectStatus.Rejected => IsArabic ? "مرفوض" : "Rejected",
            _ => status.ToString()
        };
    }

    public List<StatusOption> GetStatusOptions()
    {
        return Enum.GetValues<ProjectStatus>()
            .Select(s => new StatusOption(s, GetStatusLabel(s)))
            .ToList();
    }

    private void UpdateResourceDictionary(string languageCode)
    {
        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        var existing = dictionaries.FirstOrDefault(dict =>
            dict.Source != null
            && dict.Source.OriginalString.Contains(ResourcePrefix, StringComparison.OrdinalIgnoreCase));

        var source = new Uri($"{ResourcePrefix}{languageCode}{ResourceSuffix}", UriKind.Relative);

        if (existing == null)
        {
            dictionaries.Add(new ResourceDictionary { Source = source });
        }
        else
        {
            existing.Source = source;
        }
    }

    private static void UpdateFlowDirection(string languageCode)
    {
        var flowDirection = languageCode == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        System.Windows.Application.Current.Resources["AppFlowDirection"] = flowDirection;
    }
}

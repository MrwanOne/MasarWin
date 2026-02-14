using System;

namespace Masar.UI.Services;

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;
    bool IsArabic { get; }
    string CurrentLanguage { get; }
    string GetString(string key);
    string GetStatusLabel(Masar.Domain.Enums.ProjectStatus status);
    System.Collections.Generic.List<Masar.UI.Models.StatusOption> GetStatusOptions();
    void SetLanguage(string languageCode);
    void ToggleLanguage();
}

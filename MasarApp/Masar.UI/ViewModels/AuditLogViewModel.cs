using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class AuditLogViewModel : ViewModelBase
{
    private readonly IAuditLogRepository _auditLogRepository;

    public ObservableCollection<AuditLogItemViewModel> AuditLogs { get; } = new();
    public ObservableCollection<string> EntityTypes { get; } = new() { "الكل" };

    private string _selectedEntityType = "الكل";
    public string SelectedEntityType
    {
        get => _selectedEntityType;
        set
        {
            if (SetProperty(ref _selectedEntityType, value))
                ApplyFilters();
        }
    }

    private string _searchUsername = string.Empty;
    public string SearchUsername
    {
        get => _searchUsername;
        set
        {
            if (SetProperty(ref _searchUsername, value))
                ApplyFilters();
        }
    }

    private DateTime? _dateFrom;
    public DateTime? DateFrom
    {
        get => _dateFrom;
        set
        {
            if (SetProperty(ref _dateFrom, value))
                ApplyFilters();
        }
    }

    private DateTime? _dateTo;
    public DateTime? DateTo
    {
        get => _dateTo;
        set
        {
            if (SetProperty(ref _dateTo, value))
                ApplyFilters();
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private List<AuditLog> _allLogs = new();

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }

    public AuditLogViewModel(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
        RefreshCommand = new RelayCommand(_ => NotifyTask.Create(LoadAsync()));
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            _allLogs = await _auditLogRepository.GetAllAsync();
            
            // Populate entity types for filter
            var types = _allLogs.Select(x => x.EntityName).Distinct().OrderBy(x => x).ToList();
            EntityTypes.Clear();
            EntityTypes.Add("الكل");
            foreach (var type in types)
            {
                EntityTypes.Add(type);
            }

            ApplyFilters();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allLogs.AsEnumerable();

        if (!string.IsNullOrEmpty(_selectedEntityType) && _selectedEntityType != "الكل")
        {
            filtered = filtered.Where(x => x.EntityName == _selectedEntityType);
        }

        if (!string.IsNullOrEmpty(_searchUsername))
        {
            filtered = filtered.Where(x => x.Username.Contains(_searchUsername, StringComparison.OrdinalIgnoreCase));
        }

        if (_dateFrom.HasValue)
        {
            filtered = filtered.Where(x => x.ChangedAt >= _dateFrom.Value);
        }

        if (_dateTo.HasValue)
        {
            filtered = filtered.Where(x => x.ChangedAt <= _dateTo.Value.AddDays(1));
        }

        AuditLogs.Clear();
        foreach (var log in filtered.Take(500))
        {
            AuditLogs.Add(new AuditLogItemViewModel(log));
        }
    }

    private void ClearFilters()
    {
        SelectedEntityType = "الكل";
        SearchUsername = string.Empty;
        DateFrom = null;
        DateTo = null;
    }
}

public class AuditLogItemViewModel
{
    public int AuditLogId { get; }
    public string EntityName { get; }
    public string EntityId { get; }
    public string Action { get; }
    public string ActionDisplay { get; }
    public string Username { get; }
    public DateTime ChangedAt { get; }
    public string OldValues { get; }
    public string NewValues { get; }

    public AuditLogItemViewModel(AuditLog log)
    {
        AuditLogId = log.AuditLogId;
        EntityName = log.EntityName;
        EntityId = log.EntityId;
        Action = log.Action;
        ActionDisplay = GetActionDisplay(log.Action);
        Username = log.Username;
        ChangedAt = log.ChangedAt;
        OldValues = FormatValues(log.OldValues);
        NewValues = FormatValues(log.NewValues);
    }

    private static readonly HashSet<string> HiddenFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedAt", "CreatedByUserId", "UpdatedAt", "UpdatedByUserId",
        "IsDeleted", "PasswordHash", "PasswordSalt"
    };

    private static readonly Dictionary<string, string> FieldLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Name"] = "الاسم",
        ["NameAr"] = "الاسم (عربي)",
        ["NameEn"] = "الاسم (إنجليزي)",
        ["FullName"] = "الاسم الكامل",
        ["Email"] = "البريد",
        ["Phone"] = "الهاتف",
        ["Title"] = "العنوان",
        ["Description"] = "الوصف",
        ["Status"] = "الحالة",
        ["Code"] = "الرمز",
        ["Gender"] = "الجنس",
        ["Username"] = "اسم المستخدم",
        ["Role"] = "الدور",
        ["IsActive"] = "نشط",
        ["StudentNumber"] = "الرقم الجامعي",
        ["GPA"] = "المعدل",
        ["Level"] = "المستوى",
        ["Qualification"] = "المؤهل",
        ["Specialization"] = "التخصص",
        ["Rank"] = "الرتبة",
        ["DepartmentId"] = "القسم",
        ["CollegeId"] = "الكلية",
        ["TeamId"] = "الفريق",
        ["SupervisorId"] = "المشرف",
        ["CommitteeId"] = "اللجنة",
        ["MaxSupervisionCount"] = "الحد الأقصى للإشراف",
        ["EnrollmentYear"] = "سنة القبول",
        ["CompletionRate"] = "نسبة الإنجاز",
        ["Place"] = "المكان",
        ["Beneficiary"] = "الجهة المستفيدة",
    };

    private static string FormatValues(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var lines = new List<string>();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (HiddenFields.Contains(prop.Name))
                    continue;

                var label = FieldLabels.TryGetValue(prop.Name, out var ar) ? ar : prop.Name;
                var value = prop.Value.ValueKind switch
                {
                    JsonValueKind.Null => "-",
                    JsonValueKind.True => "نعم",
                    JsonValueKind.False => "لا",
                    JsonValueKind.String => string.IsNullOrEmpty(prop.Value.GetString()) ? "-" : prop.Value.GetString()!,
                    _ => prop.Value.ToString()
                };

                lines.Add($"{label}: {value}");
            }

            return string.Join("\n", lines);
        }
        catch
        {
            return json ?? string.Empty;
        }
    }

    private static string GetActionDisplay(string action) => action switch
    {
        "Added" => "إضافة",
        "Modified" => "تعديل",
        "Deleted" => "حذف",
        _ => action
    };
}

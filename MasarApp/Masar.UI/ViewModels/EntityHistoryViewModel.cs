using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Masar.UI.ViewModels;

public class EntityHistoryViewModel : ViewModelBase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly string _entityName;
    private readonly string _entityId;

    public ObservableCollection<AuditLogItemViewModel> History { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _isEmpty;
    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public EntityHistoryViewModel(IAuditLogRepository auditLogRepository, string entityName, string entityId)
    {
        _auditLogRepository = auditLogRepository;
        _entityName = entityName;
        _entityId = entityId;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var logs = await _auditLogRepository.GetByEntityAsync(_entityName, _entityId);
            History.Clear();
            foreach (var log in logs)
            {
                History.Add(new AuditLogItemViewModel(log));
            }
            IsEmpty = History.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

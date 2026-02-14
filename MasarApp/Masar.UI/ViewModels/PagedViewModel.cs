using Masar.UI.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Masar.UI.ViewModels;

public abstract class PagedViewModel<T> : ViewModelBase
{
    public ObservableCollection<T> PagedItems { get; } = new();
    protected List<T> AllItems { get; private set; } = new();
    protected List<T> FilteredItems { get; private set; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                ApplyFilter();
            }
        }
    }

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                RefreshPage();
            }
        }
    }

    public int TotalPages => FilteredItems.Count == 0 ? 1 : (int)Math.Ceiling(FilteredItems.Count / (double)PageSize);

    public RelayCommand NextPageCommand { get; }
    public RelayCommand PreviousPageCommand { get; }
    public RelayCommand SearchCommand { get; }

    protected PagedViewModel()
    {
        NextPageCommand = new RelayCommand(_ => NextPage(), _ => CurrentPage < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CurrentPage > 1);
        SearchCommand = new RelayCommand(_ => ApplyFilter());
    }

    protected void SetItems(IEnumerable<T> items)
    {
        AllItems = items.ToList();
        ApplyFilter();
    }

    protected void ApplyFilter()
    {
        var query = AllItems.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var keyword = SearchText.Trim();
            query = query.Where(item => FilterItem(item, keyword));
        }

        FilteredItems = query.ToList();
        CurrentPage = 1;
        RefreshPage();
    }

    protected void RefreshPage()
    {
        PagedItems.Clear();

        if (FilteredItems.Count == 0)
        {
            OnPropertyChanged(nameof(TotalPages));
            NextPageCommand.RaiseCanExecuteChanged();
            PreviousPageCommand.RaiseCanExecuteChanged();
            return;
        }

        var skip = (CurrentPage - 1) * PageSize;
        foreach (var item in FilteredItems.Skip(skip).Take(PageSize))
        {
            PagedItems.Add(item);
        }

        OnPropertyChanged(nameof(TotalPages));
        NextPageCommand.RaiseCanExecuteChanged();
        PreviousPageCommand.RaiseCanExecuteChanged();
    }

    protected abstract bool FilterItem(T item, string searchText);

    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }
}

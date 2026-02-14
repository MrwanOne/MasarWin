using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Masar.UI.Controls;
using Masar.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Masar.UI.ViewModels;

public class DocumentsViewModel : DialogViewModel
{
    private readonly IDocumentService _documentService;
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly IToastService _toastService;

    private readonly int _parentId;
    private readonly string _parentType; // Project, Discussion, Student

    public ObservableCollection<DocumentDto> Documents { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private DocumentDto? _selectedDocument;
    public DocumentDto? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (SetProperty(ref _selectedDocument, value))
            {
                DownloadCommand.RaiseCanExecuteChanged();
                PreviewCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand UploadCommand { get; }
    public AsyncRelayCommand DownloadCommand { get; }
    public AsyncRelayCommand PreviewCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }

    public DocumentsViewModel(
        IDocumentService documentService,
        IDialogService dialogService,
        ILocalizationService localizationService,
        IToastService toastService,
        int parentId,
        string parentType)
    {
        _documentService = documentService;
        _dialogService = dialogService;
        _localizationService = localizationService;
        _toastService = toastService;
        _parentId = parentId;
        _parentType = parentType;

        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        UploadCommand = new AsyncRelayCommand(UploadAsync);
        DownloadCommand = new AsyncRelayCommand(DownloadAsync, () => SelectedDocument != null);
        PreviewCommand = new AsyncRelayCommand(PreviewAsync, () => SelectedDocument != null);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedDocument != null);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            Documents.Clear();
            var docs = await _documentService.GetByProjectAsync(_parentId); // Currently only project supported in service, will extend if needed
            foreach (var doc in docs.OrderByDescending(d => d.CreatedAt))
            {
                Documents.Add(doc);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UploadAsync()
    {
        var path = _dialogService.OpenFile("Supported Files|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.png;*.jpg;*.jpeg;*.ppt;*.pptx|All Files|*.*");
        if (string.IsNullOrEmpty(path)) return;

        await UploadFileInternalAsync(path);
    }

    public async Task UploadFileInternalAsync(string filePath)
    {
        IsLoading = true;
        try
        {
            using var stream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);
            var result = await _documentService.UploadAsync(_parentId, stream, fileName);

            if (result.IsSuccess)
            {
                _toastService.ShowSuccess(_localizationService.GetString("Success.UploadDocument"));
                await LoadAsync();
            }
            else
            {
                _toastService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DownloadAsync()
    {
        if (SelectedDocument == null) return;

        var path = _dialogService.SaveFile("Files|*" + System.IO.Path.GetExtension(SelectedDocument.FileName));
        if (string.IsNullOrEmpty(path)) return;

        IsLoading = true;
        try
        {
            var result = await _documentService.DownloadAsync(SelectedDocument.DocumentId);
            if (result.IsSuccess)
            {
                using (var fileStream = File.Create(path))
                {
                    await result.Value!.CopyToAsync(fileStream);
                }
                _toastService.ShowSuccess(_localizationService.GetString("Success.DownloadDocument"));
            }
            else
            {
                _toastService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedDocument == null) return;

        if (_dialogService.Confirm(_localizationService.GetString("Question.DeleteDocument")))
        {
            var result = await _documentService.DeleteAsync(SelectedDocument.DocumentId);
            if (result.IsSuccess)
            {
                _toastService.ShowSuccess(_localizationService.GetString("Success.DeleteDocument"));
                await LoadAsync();
            }
            else
            {
                _toastService.ShowError(result.Message);
            }
        }
    }

    private async Task PreviewAsync()
    {
        if (SelectedDocument == null) return;

        IsLoading = true;
        try
        {
            var result = await _documentService.DownloadAsync(SelectedDocument.DocumentId);
            if (result.IsSuccess)
            {
                var tempPath = Path.Combine(Path.GetTempPath(), SelectedDocument.FileName);
                using (var fileStream = File.Create(tempPath))
                {
                    await result.Value!.CopyToAsync(fileStream);
                }

                var ext = Path.GetExtension(SelectedDocument.FileName).ToLower();
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (imageExtensions.Contains(ext))
                {
                    // Show internal image preview
                    var previewWindow = new Window
                    {
                        Title = SelectedDocument.FileName,
                        Width = 800,
                        Height = 600,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = new System.Windows.Controls.Image
                        {
                            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(tempPath)),
                            Stretch = System.Windows.Media.Stretch.Uniform,
                            Margin = new Thickness(10)
                        },
                        Background = System.Windows.Media.Brushes.Black
                    };
                    previewWindow.ShowDialog();
                }
                else
                {
                    // Open with system default viewer
                    var info = new System.Diagnostics.ProcessStartInfo(tempPath)
                    {
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(info);
                }
            }
            else
            {
                _toastService.ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

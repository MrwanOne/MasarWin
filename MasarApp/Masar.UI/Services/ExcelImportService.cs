using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace Masar.UI.Services;

public class ExcelImportResult
{
    public bool IsSuccess { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; } = new();
}

public class StudentExcelRow
{
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public int EnrollmentYear { get; set; }
    public string Gender { get; set; } = string.Empty;
}

public interface IExcelImportService
{
    string? OpenExcelFileDialog();
    List<StudentExcelRow> ReadStudentsFromExcel(string filePath);
}

public class ExcelImportService : IExcelImportService
{
    static ExcelImportService()
    {
        // EPPlus 7.x uses LicenseContext
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public string? OpenExcelFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*",
            Title = "Select Students Excel File"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public List<StudentExcelRow> ReadStudentsFromExcel(string filePath)
    {
        var students = new List<StudentExcelRow>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];
        
        if (worksheet == null)
            throw new InvalidOperationException("No worksheet found in Excel file.");

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        
        // Start from row 2 (skip header row)
        for (int row = 2; row <= rowCount; row++)
        {
            var studentNumber = worksheet.Cells[row, 1].Text?.Trim() ?? "";
            var fullName = worksheet.Cells[row, 2].Text?.Trim() ?? "";
            
            // Skip empty rows
            if (string.IsNullOrWhiteSpace(studentNumber) && string.IsNullOrWhiteSpace(fullName))
                continue;

            var student = new StudentExcelRow
            {
                StudentNumber = studentNumber,
                FullName = fullName,
                Email = worksheet.Cells[row, 3].Text?.Trim() ?? "",
                Phone = worksheet.Cells[row, 4].Text?.Trim() ?? "",
                DepartmentCode = worksheet.Cells[row, 5].Text?.Trim() ?? "",
                Gender = worksheet.Cells[row, 6].Text?.Trim() ?? ""
            };

            // Parse enrollment year
            if (int.TryParse(worksheet.Cells[row, 7].Text?.Trim(), out int year))
            {
                student.EnrollmentYear = year;
            }
            else
            {
                student.EnrollmentYear = DateTime.Now.Year;
            }

            students.Add(student);
        }

        return students;
    }
}

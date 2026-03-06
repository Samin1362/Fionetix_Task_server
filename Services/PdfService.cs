using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FionetixAPI.Models;

namespace FionetixAPI.Services;

public class PdfService
{
    public byte[] GenerateTablePdf(List<Employee> employees, string? searchTerm)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Employee Registry").FontSize(18).Bold();
                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                        col.Item().Text($"Search filter: \"{searchTerm}\"").FontSize(9).Italic();
                    col.Item().PaddingBottom(10);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);   // #
                        columns.RelativeColumn(3);     // Name
                        columns.RelativeColumn(2.5f);  // NID
                        columns.RelativeColumn(2);     // Department
                        columns.RelativeColumn(2.5f);  // Phone
                        columns.RelativeColumn(1.5f);  // Salary
                    });

                    // Header row
                    table.Header(header =>
                    {
                        void HeaderCell(string text) =>
                            header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                                .Text(text).FontColor(Colors.White).Bold().FontSize(9);

                        HeaderCell("#");
                        HeaderCell("Name");
                        HeaderCell("NID");
                        HeaderCell("Department");
                        HeaderCell("Phone");
                        HeaderCell("Basic Salary");
                    });

                    // Data rows
                    for (int i = 0; i < employees.Count; i++)
                    {
                        var emp = employees[i];
                        var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        void DataCell(string text) =>
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(text).FontSize(9);

                        DataCell((i + 1).ToString());
                        DataCell(emp.Name);
                        DataCell(emp.NID);
                        DataCell(emp.Department);
                        DataCell(emp.Phone);
                        DataCell($"{emp.BasicSalary:N2}");
                    }
                });

                page.Footer().AlignCenter()
                    .Text($"Total: {employees.Count} employee(s)").FontSize(9);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateCvPdf(Employee employee)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Employee Profile").FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
                    col.Item().PaddingBottom(15);
                });

                page.Content().Column(col =>
                {
                    // Personal Information
                    col.Item().Text("PERSONAL INFORMATION").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(8);

                    void InfoRow(string label, string value)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(130).Text(label).Bold();
                            row.RelativeItem().Text(value);
                        });
                        col.Item().PaddingBottom(4);
                    }

                    InfoRow("Name:", employee.Name);
                    InfoRow("NID:", employee.NID);
                    InfoRow("Phone:", employee.Phone);
                    InfoRow("Department:", employee.Department);
                    InfoRow("Basic Salary:", $"BDT {employee.BasicSalary:N2}");

                    col.Item().PaddingBottom(20);

                    // Spouse Information
                    col.Item().Text("SPOUSE INFORMATION").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(8);

                    if (employee.Spouse != null)
                    {
                        InfoRow("Name:", employee.Spouse.Name);
                        if (!string.IsNullOrEmpty(employee.Spouse.NID))
                            InfoRow("NID:", employee.Spouse.NID);
                    }
                    else
                    {
                        col.Item().Text("No spouse on record.").Italic().FontColor(Colors.Grey.Medium);
                    }

                    col.Item().PaddingBottom(20);

                    // Children
                    col.Item().Text("CHILDREN").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(8);

                    if (employee.Children.Count > 0)
                    {
                        col.Item().Table(childTable =>
                        {
                            childTable.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);   // Name
                                columns.RelativeColumn(2);   // DoB
                                columns.RelativeColumn(1.5f); // Age
                            });

                            childTable.Header(header =>
                            {
                                void HeaderCell(string text) =>
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                                        .Text(text).Bold().FontSize(10);

                                HeaderCell("Name");
                                HeaderCell("Date of Birth");
                                HeaderCell("Age");
                            });

                            foreach (var child in employee.Children)
                            {
                                var age = DateTime.Today.Year - child.DateOfBirth.Year;
                                if (DateOnly.FromDateTime(DateTime.Today) < child.DateOfBirth.AddYears(age))
                                    age--;

                                void DataCell(string text) =>
                                    childTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5).Text(text).FontSize(10);

                                DataCell(child.Name);
                                DataCell(child.DateOfBirth.ToString("yyyy-MM-dd"));
                                DataCell($"{age} years");
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("No children on record.").Italic().FontColor(Colors.Grey.Medium);
                    }
                });

                page.Footer().AlignCenter()
                    .Text($"Generated: {DateTime.Now:yyyy-MM-dd}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }
}

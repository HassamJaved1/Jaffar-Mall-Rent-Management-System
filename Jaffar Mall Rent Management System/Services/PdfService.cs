using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using Jaffar_Mall_Rent_Management_System.Models;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class PdfService
    {
        public byte[] GenerateLeaseContractPdf(string tenantName, string propertyName, decimal rentAmount, int months, DateTime startDate, DateTime endDate, int rentDueDays, decimal securityDeposit, int securityDueDays, int incrementMonths = 0, decimal incrementPercentage = 0, string status = "Active")
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    string documentTitle = "LEASE AGREEMENT";
                    if (status.Contains("Terminated", StringComparison.OrdinalIgnoreCase)) documentTitle = "LEASE TERMINATION NOTICE";
                    else if (status.Contains("Cancelled", StringComparison.OrdinalIgnoreCase) || status.Contains("Canceled", StringComparison.OrdinalIgnoreCase)) documentTitle = "LEASE CANCELLATION NOTICE";
                    else if (status.Contains("Pending", StringComparison.OrdinalIgnoreCase)) documentTitle = "LEASE PROPOSAL / DRAFT";

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("JAFFAR").FontSize(24).ExtraBold().FontColor("#1e3a8a");
                                text.Span(" ").FontSize(24).ExtraBold();
                                text.Span("MALL").FontSize(24).ExtraBold().FontColor("#eab308");
                            });
                            column.Item().Text("Jhelum, Punjab, Pakistan").FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                        row.RelativeItem().AlignRight().Column(column =>
                        {
                            column.Item().Text(documentTitle).FontSize(16).SemiBold().FontColor(status == "Active" ? Colors.Grey.Darken2 : Colors.Red.Medium);
                            column.Item().Text($"Date: {DateTime.Now:MMMM dd, yyyy}").FontSize(10);
                        });
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(15);

                        column.Item().Text($"Dear {tenantName},").SemiBold();

                        if (status == "Active")
                        {
                            column.Item().Text("We are pleased to officially confirm the lease assignment for the property located at " + propertyName + ". This document serves as a formal acknowledgment of the terms agreed upon for your tenancy at Jaffar Mall.");
                        }
                        else
                        {
                            column.Item().Text($"This document serves as a formal notification regarding the {status} status of your lease for property {propertyName}. Below are the reference terms of the agreement as of this update.");
                        }

                        column.Item().Text(text =>
                        {
                            text.Span("According to our records, your lease was scheduled to commence on ");
                            text.Span(startDate.ToString("MMMM dd, yyyy")).SemiBold();
                            text.Span(" and remain in effect for a duration of ");
                            text.Span(months.ToString()).SemiBold();
                            text.Span(" months, concluding on ");
                            text.Span(endDate.ToString("MMMM dd, yyyy")).SemiBold();
                            text.Span(". The monthly rental commitment for this unit was established at ");
                            text.Span($"PKR {rentAmount:N2}").SemiBold();
                            text.Span(", with payments due every ");
                            text.Span(rentDueDays.ToString()).SemiBold();
                            text.Span(" days.");
                        });

                        if (incrementMonths > 0 && incrementPercentage > 0 && status == "Active")
                        {
                            column.Item().Background(Colors.Amber.Lighten5).Padding(10).Column(inner => {
                                inner.Item().Text("Note on Rent Increment:").SemiBold().FontColor(Colors.Amber.Darken3);
                                inner.Item().Text($"Please be advised that as per the agreement, a rent increase of {incrementPercentage}% will be applied automatically every {incrementMonths} months during the tenure of this lease.");
                            });
                        }

                        column.Item().Text(text =>
                        {
                            text.Span("The security deposit for this premises was ");
                            text.Span($"PKR {securityDeposit:N2}").SemiBold();
                            text.Span(". This deposit remains subject to the management's review of the premises' condition and the terms of settlement defined in the mall's administrative policies.");
                        });

                        column.Item().PaddingTop(10).Text("We are committed to providing a professional and supportive managed environment. Should you have any questions or require further assistance, please do not hesitate to contact our management office directly.");

                        column.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().Column(signature =>
                            {
                                signature.Item().PaddingBottom(5).Text("__________________________");
                                signature.Item().Text("Manager Signature").FontSize(9).SemiBold().FontColor("#1e3a8a");
                                signature.Item().Text("Jaffar Mall Management").FontSize(9).FontColor("#1e3a8a");
                            });

                            row.RelativeItem().Column(signature =>
                            {
                                signature.Item().PaddingBottom(5).Text("__________________________");
                                signature.Item().Text("Tenant Signature").FontSize(9).SemiBold();
                                signature.Item().Text(tenantName).FontSize(9);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Column(column => {
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        column.Item().PaddingTop(5).Text(x =>
                        {
                            x.Span("Main GT Road, Jhelum, Punjab, Pakistan | Contact: +92 310 3709000").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GeneratePaymentVoucherPdf(RentPayment payment, string tenantName, string propertyName)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(0.5f, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("JAFFAR").FontSize(18).ExtraBold().FontColor("#1e3a8a");
                                text.Span(" ").FontSize(18).ExtraBold();
                                text.Span("MALL").FontSize(18).ExtraBold().FontColor("#eab308");
                            });
                            column.Item().Text("Rent & Security Receipt").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().AlignRight().Column(column =>
                        {
                            column.Item().Text($"Voucher #{payment.Id:D5}").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                            column.Item().Text($"Date: {payment.PaymentDate:MMM dd, yyyy}").FontSize(9);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        column.Item().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Tenant:").SemiBold();
                            table.Cell().Text(tenantName);

                            table.Cell().Text("Property:").SemiBold();
                            table.Cell().Text(propertyName);

                            table.Cell().Text("Payment Type:").SemiBold();
                            table.Cell().Text(payment.PaymentType);

                            table.Cell().Text("Payment Method:").SemiBold();
                            table.Cell().Text(payment.PaymentMethod);
                        });

                        column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL PAID AMOUNT").FontSize(12).SemiBold();
                            row.RelativeItem().AlignRight().Text($"PKR {payment.Amount:N2}").FontSize(14).ExtraBold().FontColor("#1e3a8a");
                        });

                        if (!string.IsNullOrEmpty(payment.Remarks))
                        {
                            column.Item().PaddingTop(5).Text($"Remarks: {payment.Remarks}").FontSize(8).Italic();
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("This is an electronically generated receipt.").FontSize(7).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text("Management Signature: ________________").FontSize(8).SemiBold();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateMaintenanceVoucherPdf(Maintenance maintenance, string propertyName)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(0.5f, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("JAFFAR").FontSize(18).ExtraBold().FontColor("#1e3a8a");
                                text.Span(" ").FontSize(18).ExtraBold();
                                text.Span("MALL").FontSize(18).ExtraBold().FontColor("#eab308");
                            });
                            column.Item().Text("Maintenance & Repair Voucher").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().AlignRight().Column(column =>
                        {
                            column.Item().Text($"Voucher #{maintenance.Id:D5}").FontSize(12).SemiBold().FontColor(Colors.Red.Medium);
                            column.Item().Text($"Date: {maintenance.CreatedAt:MMM dd, yyyy}").FontSize(9);
                        });
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        column.Item().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Property:").SemiBold();
                            table.Cell().Text(propertyName);

                            table.Cell().Text("Issue:").SemiBold();
                            table.Cell().Text(maintenance.Title);

                            table.Cell().Text("Repairer:").SemiBold();
                            table.Cell().Text(maintenance.RepairerName ?? "N/A");

                            table.Cell().Text("Status:").SemiBold();
                            table.Cell().Text(((MaintenanceStatus)maintenance.Status).ToString());
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c => {
                                c.Item().PaddingTop(8).Table(t => {
                                    t.ColumnsDefinition(cols => {
                                        cols.RelativeColumn();
                                        cols.ConstantColumn(120);
                                    });

                                    // Header row
                                    t.Cell().Background("#e5e7eb").Padding(6).Text("Description").FontSize(9).SemiBold();
                                    t.Cell().Background("#e5e7eb").Padding(6).AlignRight().Text("Amount (PKR)").FontSize(9).SemiBold();

                                    // Issue Cost row
                                    t.Cell().BorderBottom(1).BorderColor("#d1d5db").Padding(6).Text("Issue / Repair Cost Estimate").FontSize(9);
                                    t.Cell().BorderBottom(1).BorderColor("#d1d5db").Padding(6).AlignRight().Text($"{maintenance.RepairCost:N2}").FontSize(9);

                                    // Repairer amount row
                                    t.Cell().BorderBottom(1).BorderColor("#d1d5db").Padding(6).Text("Amount Paid to Repairer").FontSize(9);
                                    t.Cell().BorderBottom(1).BorderColor("#d1d5db").Padding(6).AlignRight().Text($"{maintenance.AmountPaid:N2}").FontSize(9).Bold();

                                    // Grand total row
                                    decimal grandTotal = maintenance.RepairCost + maintenance.AmountPaid;
                                    t.Cell().Background("#1e3a8a").Padding(8).Text("GRAND TOTAL").FontSize(11).ExtraBold().FontColor(Colors.White);
                                    t.Cell().Background("#1e3a8a").Padding(8).AlignRight().Text($"PKR {grandTotal:N2}").FontSize(12).ExtraBold().FontColor("#eab308");
                                });
                            });
                        });
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Office Copy | Jaffar Mall Management").FontSize(7).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text("Authorized Signature: ________________").FontSize(8).SemiBold();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}

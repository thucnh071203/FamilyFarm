using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;


namespace FamilyFarm.BusinessLogic.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateBillPdf(BillPaymentMapper data)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    // Header: "Paid" badge
                    page.Header().Row(row =>
                    {
                        row.ConstantColumn(200)
                            .AlignRight()
                            .Background(Colors.Green.Medium)
                            .PaddingVertical(1)
                            .Text("Paid")
                            .FontSize(16)
                            .FontColor(Colors.White)
                            .Bold()
                            .AlignCenter();
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Invoice title
                        col.Item().PaddingBottom(10).Text($"Invoice: {data.PaymentId}")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        // Divider (no padding here)
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Top info
                        col.Item().PaddingVertical(10).Row(row =>
                        {
                            row.RelativeColumn().Column(left =>
                            {
                                left.Item().Text("Payer information").Bold();
                                left.Item().Text(data.PayerName ?? "");

                                left.Item().PaddingTop(5).Text("Country").Bold();
                                left.Item().Text("Viet Nam");

                                left.Item().PaddingTop(5).Text("Invoice creation date").Bold();
                                left.Item().Text(data.PayAt?.ToString("d/M/yyyy") ?? "");
                            });

                            row.RelativeColumn().AlignRight().Column(right =>
                            {
                                right.Item().Text("System Provider").Bold().AlignRight();
                                right.Item().Text("Family Farm").AlignRight();

                                right.Item().PaddingTop(5).Text("Payment method").Bold().AlignRight();
                                right.Item().Text("Transfer application").AlignRight();
                            });
                        });

                        // Payment info box
                        col.Item()
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .Background(Colors.Grey.Lighten3)
                            .Padding(15)
                            .Column(box =>
                            {
                                box.Item().Text("Payment information")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.Blue.Medium)
                                    .AlignCenter();

                                box.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1); // ❌ KHÔNG dùng PaddingVertical ở đây

                                // Description
                                box.Item().Row(descRow =>
                                {
                                    descRow.RelativeColumn().Column(d =>
                                    {
                                        d.Item().Text("Detailed description").Bold();
                                        d.Item().Text($"Service name: {data.ServiceName}");
                                        d.Item().Text($"Category: {data.CategoryServiceName}");
                                        d.Item().Text($"Service owner: {data.ExpertName}");
                                        d.Item().Text($"Service booker: {data.FarmerName}");
                                        d.Item().Text($"Date booked: {data.BookingServiceAt?.ToString("d/M/yyyy")}");
                                    });

                                    descRow.ConstantColumn(150).AlignRight().Column(t =>
                                    {
                                        t.Item().Text("Total amount").Bold().AlignRight();
                                        t.Item().Text($"{data.Price?.ToString("#,##0")} đ").AlignRight();
                                    });
                                });

                                // Total block
                                box.Item().PaddingTop(10).Row(totalRow =>
                                {
                                    totalRow.RelativeColumn(); // spacer

                                    totalRow.ConstantColumn(180).Column(t =>
                                    {
                                        t.Item().Row(r =>
                                        {
                                            r.RelativeColumn().Text("Total service fee").Bold();
                                            r.ConstantColumn(80).Text($"{data.Price?.ToString("#,##0")} đ").AlignRight();
                                        });

                                        t.Item().Row(r =>
                                        {
                                            r.RelativeColumn().Text("Total payment").Bold();
                                            r.ConstantColumn(80).Text($"{data.Price?.ToString("#,##0")} đ").AlignRight();
                                        });
                                    });
                                });
                            });
                    });

                    // Footer
                    page.Footer().AlignCenter()
                        .Text("Family Farm - Thank you!")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Darken2);
                });
            });

            return doc.GeneratePdf();
        }
    }
}

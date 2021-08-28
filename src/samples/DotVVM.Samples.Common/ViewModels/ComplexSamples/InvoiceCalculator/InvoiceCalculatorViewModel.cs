using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.InvoiceCalculator
{
    public class InvoiceCalculatorViewModel : DotvvmViewModelBase
    {

        public string Number { get; set; }

        public string DueDate { get; set; }

        public List<InvoiceLine> InvoiceLines { get; set; }

        public InvoiceCalculatorViewModel()
        {
            InvoiceLines = new List<InvoiceLine>();
        }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                InvoiceLines.Add(new InvoiceLine()
                {
                    Number = "001",
                    Quantity = 15,
                    TaxCoeff = 1,
                    Title = "Beer",
                    UnitPrice = 6.4m
                });
            }
            return base.Init();
        }

        public decimal Total
        {
            get { return InvoiceLines.Select(l => l.Total).DefaultIfEmpty(0).Sum(); }
        }

        public TaxRate[] TaxRates
        {
            get
            {
                return new[]
                {
                    new TaxRate() { Id = 0, Title = "no tax", Coeff = 1 },
                    new TaxRate() { Id = 1, Title = "reasonable tax (5%)", Coeff = 1.05m },
                    new TaxRate() { Id = 2, Title = "still quite reasonable tax (10%)", Coeff = 1.1m },
                    new TaxRate() { Id = 3, Title = "European tax (20%)", Coeff = 1.2m },
                    new TaxRate() { Id = 3, Title = "European tax in 10 years (40% or even more)", Coeff = 1.4m }
                };
            }
        }


        public void AddLine()
        {
            InvoiceLines.Add(new InvoiceLine()
            {
                Number = "#",
                Title = ""
            });
        }

        public void RemoveLine(InvoiceLine line)
        {
            InvoiceLines.Remove(line);
        }

        public void Recalculate()
        {
            
        }

    }

    public class TaxRate
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public decimal Coeff { get; set; }
    }

    public class InvoiceLine
    {

        public string Number { get; set; }

        public string Title { get; set; }

        public decimal TaxCoeff { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Quantity { get; set; }

        public decimal Total
        {
            get { return UnitPrice * Quantity * TaxCoeff; }
        }

    }
}
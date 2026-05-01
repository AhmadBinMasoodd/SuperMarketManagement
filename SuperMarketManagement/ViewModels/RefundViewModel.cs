using System;
using System.Collections.ObjectModel;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public class RefundViewModel : ViewModelBase, IDisposable
    {
        private int _saleId;
        private DateTime _saleDateTime;
        private string _cashierName = string.Empty;
        private decimal _totalAmount;
        private ObservableCollection<RefundLineItemViewModel> _items = new();

        public int SaleId
        {
            get => _saleId;
            set => SetProperty(ref _saleId, value);
        }

        public DateTime SaleDateTime
        {
            get => _saleDateTime;
            set => SetProperty(ref _saleDateTime, value);
        }

        public string CashierName
        {
            get => _cashierName;
            set => SetProperty(ref _cashierName, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public ObservableCollection<RefundLineItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public void Dispose()
        {
        }
    }

    public class RefundLineItemViewModel : ViewModelBase
    {
        private int _srNo;
        private string _productName = string.Empty;
        private decimal _quantity;
        private decimal _unitPrice;
        private decimal _lineTotal;

        public int SrNo
        {
            get => _srNo;
            set => SetProperty(ref _srNo, value);
        }

        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        public decimal Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public decimal LineTotal
        {
            get => _lineTotal;
            set => SetProperty(ref _lineTotal, value);
        }
    }
}

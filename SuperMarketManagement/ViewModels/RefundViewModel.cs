using System;
using System.Collections.ObjectModel;
using SuperMarketManagement.ViewModels.Base;

namespace SuperMarketManagement.ViewModels
{
    public class EditableReceiptViewModel : ViewModelBase, IDisposable
    {
        private int _saleId;
        private DateTime _saleDateTime;
        private string _cashierName = string.Empty;
        private decimal _originalTotalAmount;
        private decimal _currentTotalAmount;
        private ObservableCollection<EditableReceiptLineItemViewModel> _items = new();

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

        public decimal OriginalTotalAmount
        {
            get => _originalTotalAmount;
            set => SetProperty(ref _originalTotalAmount, value);
        }

        public decimal CurrentTotalAmount
        {
            get => _currentTotalAmount;
            set => SetProperty(ref _currentTotalAmount, value);
        }

        public ObservableCollection<EditableReceiptLineItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public void Dispose()
        {
        }
    }

    public class EditableReceiptLineItemViewModel : ViewModelBase
    {
        private int _srNo;
        private int _productId;
        private string _productName = string.Empty;
        private decimal _originalQuantity;
        private decimal _currentQuantity;
        private decimal _unitPrice;

        public int SrNo
        {
            get => _srNo;
            set => SetProperty(ref _srNo, value);
        }

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        public decimal OriginalQuantity
        {
            get => _originalQuantity;
            set => SetProperty(ref _originalQuantity, value);
        }

        public decimal CurrentQuantity
        {
            get => _currentQuantity;
            set
            {
                if (SetProperty(ref _currentQuantity, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                    OnPropertyChanged(nameof(HasChanged));
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                {
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public decimal LineTotal => CurrentQuantity * UnitPrice;
        public bool HasChanged => CurrentQuantity != OriginalQuantity;
    }
}

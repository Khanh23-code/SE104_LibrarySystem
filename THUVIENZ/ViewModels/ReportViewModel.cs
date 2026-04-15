using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel quản lý dữ liệu cho màn hình Báo cáo và Thống kê.
    /// </summary>
    public class ReportViewModel : ObservableObject
    {
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _toDate = DateTime.Now;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BookStatDTO> _bookStats = new ObservableCollection<BookStatDTO>();
        public ObservableCollection<BookStatDTO> BookStats
        {
            get => _bookStats;
            set
            {
                _bookStats = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ReaderStatDTO> _readerStats = new ObservableCollection<ReaderStatDTO>();
        public ObservableCollection<ReaderStatDTO> ReaderStats
        {
            get => _readerStats;
            set
            {
                _readerStats = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadBookStatsCommand { get; }
        public ICommand LoadReaderStatsCommand { get; }

        private readonly ReportService _reportService;

        public ReportViewModel()
        {
            _reportService = new ReportService();
            
            LoadBookStatsCommand = new RelayCommand(_ => ExecuteLoadBookStats());
            LoadReaderStatsCommand = new RelayCommand(_ => ExecuteLoadReaderStats());

            // Tự động nạp dữ liệu lần đầu
            ExecuteLoadBookStats();
            ExecuteLoadReaderStats();
        }

        /// <summary>
        /// Lấy dữ liệu thống kê mượn sách theo bộ lọc thời gian.
        /// </summary>
        private void ExecuteLoadBookStats()
        {
            var stats = _reportService.GetTopBorrowedBooks(FromDate, ToDate);
            BookStats = new ObservableCollection<BookStatDTO>(stats);
        }

        /// <summary>
        /// Lấy dữ liệu thống kê nợ và quá hạn của độc giả.
        /// </summary>
        private void ExecuteLoadReaderStats()
        {
            var stats = _reportService.GetReaderOverdueReports();
            ReaderStats = new ObservableCollection<ReaderStatDTO>(stats);
        }
    }
}

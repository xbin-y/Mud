using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Mud.Core;
using Mud.Core.Models;
using Mud.Windows.ViewModels;

namespace Mud.Windows.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SearchStockListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox && SearchStockListBox.SelectedItem is StockInfo stockInfo)
        {
            var stockManager = Container.ServiceProvider.GetRequiredService<IStockManager>();
            if (DataContext is MainWindowViewModel vm && vm.FocusOnStocks.All(t => t.FullSymbol != stockInfo.FullSymbol))
            {
                await stockManager.AddFocusOnStockAsync(stockInfo);
                vm.SearchText = string.Empty;
                vm.FocusOnStocks.Add(stockInfo);
            }
        }
    }

    private async void Remove_Focus_Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is StockInfo stockInfo)
        {
            var stockManager = Container.ServiceProvider.GetRequiredService<IStockManager>();
            await stockManager.RemoveFocusOnStockAsync(stockInfo);
            (DataContext as MainWindowViewModel)?.FocusOnStocks.Remove(stockInfo);
        }
    }
}

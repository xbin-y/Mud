using Trady.Core.Infrastructure;

namespace Trady.Analysis.Backtest.FeeCalculators
{
    /// <summary>
    /// Interface to calculate buy/sell logic.
    /// </summary>
    /// <remarks>
    /// You can implement your own calculator for any particular broker's rules.
    /// For example: some brokers charge differently for larger orders...  You can implement that logic in in your own IAssetCalculator
    /// </remarks>
    public interface IFeeCalculator
    {
        /// <summary>
        /// 購買
        /// </summary>
        /// <param name="indexedCandle"></param>
        /// <param name="cash"></param>
        /// <param name="nextCandle"></param>
        /// <param name="buyInCompleteQuantities"></param>
        /// <returns></returns>
        Transaction BuyAsset(IIndexedOhlcv indexedCandle, decimal cash, IIndexedOhlcv nextCandle, bool buyInCompleteQuantities);

        /// <summary>
        /// 賣出
        /// </summary>
        /// <param name="indexedCandle"></param>
        /// <param name="lastTransaction"></param>
        /// <returns></returns>
        Transaction SellAsset(IIndexedOhlcv indexedCandle, Transaction lastTransaction);
    }
}

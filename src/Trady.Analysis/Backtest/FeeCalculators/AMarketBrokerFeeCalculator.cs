using System;
using Trady.Core.Infrastructure;

namespace Trady.Analysis.Backtest.FeeCalculators;

public class AMarketBrokerFeeCalculator : IFeeCalculator
{
    public AMarketBrokerFeeCalculator(decimal transactionFees, int roundingDecimals = 2, decimal stampDuty = 0.0005m, decimal transferFee = 0.0001m)
    {
        StampDuty = stampDuty;
        TransferFee = transferFee;
        RoundingDecimals = roundingDecimals;
        TransactionFees = transactionFees;
    }

    /// <summary>
    /// 四捨五入小數
    /// </summary>
    protected int RoundingDecimals { get; }

    /// <summary>
    /// 印花稅
    /// </summary>
    protected decimal StampDuty { get; }

    /// <summary>
    /// 過戶費
    /// </summary>
    protected decimal TransferFee { get; }

    /// <summary>
    /// 交易佣金
    /// </summary>
    protected decimal TransactionFees { get; private set; }

    public Transaction BuyAsset(IIndexedOhlcv indexedCandle, decimal cash, IIndexedOhlcv nextCandle, bool buyInCompleteQuantities)
    {
        //多少手
        var hand = (int)(cash / nextCandle.Open / 100);
        var costs = DetermineFee(nextCandle,hand);
        costs = RoundingDecimals != -1
            ? Math.Round(costs, RoundingDecimals)
            : costs;
        var cashToBuyAsset = nextCandle.Open * hand * 100 + costs;
        return new Transaction(indexedCandle.BackingList, nextCandle.Index, nextCandle.DateTime, TransactionType.Buy, hand * 100, cashToBuyAsset, costs);
    }

    public Transaction SellAsset(IIndexedOhlcv indexedCandle, Transaction lastTransaction)
    {
        var nextCandle = indexedCandle.Next;
        var quantity = lastTransaction.Quantity;
        var costs = DetermineTransactionFee(quantity, nextCandle.Open);
        costs = RoundingDecimals != -1
            ? Math.Round(costs, RoundingDecimals)
            : costs;
        var cashWhenSellAsset = nextCandle.Open * quantity;
        cashWhenSellAsset -= costs;
        return new Transaction(indexedCandle.BackingList, nextCandle.Index, nextCandle.DateTime, TransactionType.Sell, quantity, cashWhenSellAsset, costs);
    }

    private decimal DetermineFee(IIndexedOhlcv nextCandle, decimal hand)
    {
        //股票花費
        var transFee =hand * 100 * nextCandle.Open;
        //過戶費
        var transferFee = transFee * TransferFee;
        //交易佣金
        var transactionFees =transFee * TransactionFees;
        return transferFee + transactionFees;
    }

    private decimal DetermineTransactionFee(decimal quantity, decimal tradePrice)
    {
        var transFee = quantity * tradePrice;
        //過戶費
        var transferFee = transFee * TransferFee;
        //交易佣金
        var transactionFees = transFee * TransactionFees;
        //印花稅
        var stampDuty =transFee * StampDuty;
        return transferFee + transactionFees + stampDuty;
    }
}

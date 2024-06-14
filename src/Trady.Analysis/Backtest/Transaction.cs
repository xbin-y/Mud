﻿using System;
using System.Collections.Generic;
using Trady.Core.Infrastructure;

namespace Trady.Analysis.Backtest
{
    public class Transaction : IEquatable<Transaction>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="candles"></param>
        /// <param name="index">K綫索引</param>
        /// <param name="dateTime">交易日期</param>
        /// <param name="type">交易類型</param>
        /// <param name="quantity">交易數量(股)</param>
        /// <param name="absCashFlow">操作費用</param>
        /// <param name="cost">必要成本</param>
        public Transaction(IEnumerable<IOhlcv> candles, int index, DateTimeOffset dateTime, TransactionType type,
            decimal quantity, decimal absCashFlow, decimal cost)
        {
            OhlcvList = candles;
            Index = index;
            DateTime = dateTime;
            Type = type;
            Quantity = quantity;
            AbsoluteCashFlow = absCashFlow;
            Cost = cost;
        }

        public IEnumerable<IOhlcv> OhlcvList { get; }

        public DateTimeOffset DateTime { get; }

        public int Index { get; }

        public TransactionType Type { get; }

        public decimal Quantity { get; }

        public decimal AbsoluteCashFlow { get; }

        public decimal Cost { get; set; }

        public bool Equals(Transaction other)
            => other != null
               && OhlcvList.Equals(other.OhlcvList)
               && DateTime == other.DateTime
               && Index == other.Index
               && Type == other.Type
               && Quantity == other.Quantity
               && AbsoluteCashFlow == other.AbsoluteCashFlow
               && Cost == other.Cost;

        public override bool Equals(object obj)
        {
            return Equals(obj as Transaction);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        public override string ToString()
        {
            return
                $"Idx: {Index}; Date: {DateTime:d} Type: {Type}; Quantity: {Quantity:N3}; AbsoluteCashFlow: {AbsoluteCashFlow:N3}; Cost: {Cost:N3}";
        }
    }
}

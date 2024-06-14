﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Core.Infrastructure;

namespace Trady.Analysis.Infrastructure
{
    public abstract class RuleExecutorBase<TInput, TIndexed, TOutput> : IRuleExecutor<TInput, TIndexed, TOutput> where TIndexed : IIndexedObject<TInput> where TInput : IOhlcv
    {
        protected readonly IAnalyzeContext<TInput> _context;

        protected RuleExecutorBase(
            Func<TIndexed, int, TOutput> outputFunc,
            IAnalyzeContext<TInput> context,
            Predicate<TIndexed>[] rules)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OutputFunc = outputFunc ?? throw new ArgumentNullException(nameof(outputFunc));
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            if (!rules.Any())
                throw new ArgumentException("You must have at least one rule to execute", nameof(rules));
        }

        protected Predicate<TIndexed>[] Rules { get; }

        protected Func<TIndexed, int, TOutput> OutputFunc { get; }

        public virtual IReadOnlyList<TOutput> Execute(int? startIndex = default, int? endIndex = default)
        {
            var output = new List<TOutput>();
            for (int i = startIndex ?? 0; i <= (endIndex ?? (_context.BackingList.Count() - 1)); i++)
            {
                var indexedObject = IndexedObjectConstructor(_context.BackingList, i);
                indexedObject.Context = _context;
                for (int j = 0; j < Rules.Count(); j++)
                {
                    if (Rules[j](indexedObject))
                    {
                        var result = OutputFunc(indexedObject, j);
                        if (typeof(TOutput).IsValueType || !result.Equals(default(TOutput)))   // Ignore all null objects
                        {
                            output.Add(result);
                        }

                        break;
                    }
                }
            }
            return output;
        }

        protected abstract Func<IEnumerable<TInput>, int, TIndexed> IndexedObjectConstructor { get; }
    }
}

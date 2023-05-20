using StockIndicatorsAnalyzer.Models;
using System.Collections.Generic;

namespace StockIndicatorsAnalyzer.Utilities
{
    public class IndicatorsCalculator
    {

        public static Indicators CalculateAllIndicators(string symbol,double[] closePrices,DateTime[] timeStamps,int OptInPeriod,
            int startIndex,int endIndex)
        {
            int outSize = closePrices.Length - OptInPeriod + 1;
            double[] outRsi = new double[10000];

            TicTacTec.TA.Library.Core.Rsi(startIndex, endIndex, closePrices, OptInPeriod, out int outBegIndx, out int outNBEElement, outRsi);

            List<IndicatorData> data = new();

            for (int i = 0; i < outNBEElement; i++)
            {
                //printf("Day %d = %f\n", outBeg + i, out[i]);
                data.Add(new IndicatorData
                {
                    Time = timeStamps[outBegIndx + i],
                    Value = outRsi[i]
                });
            }

            return new Indicators
            {
                Symbol = symbol,
                RSI = data.OrderByDescending(x => x.Time).ToList()
            };
        }
    }
}

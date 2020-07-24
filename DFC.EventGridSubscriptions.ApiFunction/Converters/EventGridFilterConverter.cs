using DFC.EventGridSubscriptions.Data.Models;
using Microsoft.Azure.Management.EventGrid.Models;
using System;
using System.Linq;

namespace DFC.EventGridSubscriptions.ApiFunction.Converters
{
    public static class EventGridFilterConverter
    {
        public static AdvancedFilter Convert(this ApiAdvancedFilter advancedFilter)
        {
            _ = advancedFilter ?? throw new ArgumentNullException(nameof(advancedFilter));

            switch (advancedFilter.Type)
            {
                case FilterTypeEnum.StringContains:
                    return new StringContainsAdvancedFilter(advancedFilter.Property, advancedFilter.Values);
                case FilterTypeEnum.StringEndsWith:
                    return new StringEndsWithAdvancedFilter(advancedFilter.Property, advancedFilter.Values);
                case FilterTypeEnum.StringIn:
                    return new StringInAdvancedFilter(advancedFilter.Property, advancedFilter.Values);
                case FilterTypeEnum.StringBeginsWith:
                    return new StringBeginsWithAdvancedFilter(advancedFilter.Property, advancedFilter.Values);
                case FilterTypeEnum.StringNotIn:
                    return new StringNotInAdvancedFilter(advancedFilter.Property, advancedFilter.Values);
                case FilterTypeEnum.NumberNotIn:
                    return new NumberNotInAdvancedFilter(advancedFilter.Property, advancedFilter.Values.Select(x => double.Parse(x, null)).Cast<double?>().ToList());
                case FilterTypeEnum.NumberLessThanOrEquals:
                    return new NumberLessThanOrEqualsAdvancedFilter(advancedFilter.Property, double.Parse(advancedFilter.Values.FirstOrDefault(), null));
                case FilterTypeEnum.NumberLessThan:
                    return new NumberLessThanAdvancedFilter(advancedFilter.Property, double.Parse(advancedFilter.Values.FirstOrDefault(), null));
                case FilterTypeEnum.NumberIn:
                    return new NumberInAdvancedFilter(advancedFilter.Property, advancedFilter.Values.Select(x => double.Parse(x, null)).Cast<double?>().ToList());
                case FilterTypeEnum.NumberGreaterThanOrEquals:
                    return new NumberGreaterThanAdvancedFilter(advancedFilter.Property, double.Parse(advancedFilter.Values.FirstOrDefault(), null));
                case FilterTypeEnum.BoolEquals:
                    return new BoolEqualsAdvancedFilter(advancedFilter.Property, bool.Parse(advancedFilter.Values.FirstOrDefault()));
                default:
                    throw new NotSupportedException(nameof(advancedFilter.Type));
            }
        }
    }
}

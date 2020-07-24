namespace DFC.EventGridSubscriptions.Data.Models
{
    public enum FilterTypeEnum
    {
        StringContains = 0,
        StringBeginsWith = 1,
        StringEndsWith = 2,
        StringIn = 3,
        StringNotIn = 4,
        NumberIn = 5,
        NumberNotIn = 6,
        NumberLessThan = 7,
        NumberLessThanOrEquals = 8,
        NumberGreaterThanOrEquals = 9,
        BoolEquals = 10
    }
}

namespace iOne.ResSequences;

public enum ResSequenceDateRangeType
{
    Week = 0,      // Tuần (sang tuần mới sẽ reset)
    Month = 1,     // Tháng (sang tháng mới sẽ reset)
    Quarter = 2,   // Quý (sang quý mới sẽ reset)
    Half = 3,      // Nửa năm (sang nửa năm tiếp theo sẽ reset)
    Year = 4       // Năm (sang năm mới sẽ reset)
}


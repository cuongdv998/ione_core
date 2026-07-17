namespace iOne.Master.ResSequences;

public class GetNextSequenceOutput
{
    /// <summary>
    /// Mã đã được sinh tự động theo cấu hình
    /// </summary>
    public string GeneratedCode { get; set; } = null!;

    /// <summary>
    /// Số tiếp theo sẽ được sử dụng (sau khi đã tăng)
    /// </summary>
    public long NextNumber { get; set; }
}


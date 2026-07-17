namespace iOne.ResSequences;

public enum ResSequenceType
{
    Normal = 0,    // Thông thường (có thể nhảy cóc mà ko cần quan tâm thứ tự cấp phát số tăng dần)
    NoGap = 1     // Không có khoảng trống (cần lock transaction để cấp phát số theo thứ tự)
}


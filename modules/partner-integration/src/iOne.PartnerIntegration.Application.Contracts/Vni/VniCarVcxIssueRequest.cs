using System.Text.Json.Serialization;

namespace iOne.PartnerIntegration.Vni;

public class VniCarVcxIssueRequest
{
    [JsonPropertyName("dvi_sl")]
    public string DviSl { get; set; } = "";

    [JsonPropertyName("ma_cn")]
    public string MaCn { get; set; } = "";

    [JsonPropertyName("nv")]
    public string Nv { get; set; } = "XO";

    [JsonPropertyName("ma_sp")]
    public string MaSp { get; set; } = "XT";

    [JsonPropertyName("so_hd")]
    public string SoHd { get; set; } = "";

    [JsonPropertyName("so_hd_n")]
    public string SoHdN { get; set; } = "";

    [JsonPropertyName("so_hd_g")]
    public string SoHdG { get; set; } = "";

    [JsonPropertyName("so_id")]
    public int SoId { get; set; }

    [JsonPropertyName("so_id_dt")]
    public int SoIdDt { get; set; }

    [JsonPropertyName("kieu_hd")]
    public string KieuHd { get; set; } = "G";

    [JsonPropertyName("ttrang")]
    public string Ttrang { get; set; } = "T";

    [JsonPropertyName("ten_ttrang")]
    public string TenTtrang { get; set; } = "Trình duyệt";

    [JsonPropertyName("phi_dt")]
    public decimal PhiDt { get; set; }

    [JsonPropertyName("thue")]
    public decimal Thue { get; set; }

    [JsonPropertyName("ttoan")]
    public decimal Ttoan { get; set; }

    [JsonPropertyName("tra_phi")]
    public string TraPhi { get; set; } = "QR";

    [JsonPropertyName("coche")]
    public int Coche { get; set; }

    [JsonPropertyName("ma_tracuu")]
    public string MaTracuu { get; set; } = "";

    [JsonPropertyName("ngay_ht")]
    public string NgayHt { get; set; } = "";

    [JsonPropertyName("nt_phi")]
    public string NtPhi { get; set; } = "VND";

    [JsonPropertyName("tt_kh")]
    public bool TtKh { get; set; } = true;

    [JsonPropertyName("nhom_kh")]
    public string NhomKh { get; set; } = "CN";

    [JsonPropertyName("ma_thue")]
    public string MaThue { get; set; } = "";

    [JsonPropertyName("ten_kh")]
    public string TenKh { get; set; } = "";

    [JsonPropertyName("phone_kh")]
    public string PhoneKh { get; set; } = "";

    [JsonPropertyName("email_kh")]
    public string EmailKh { get; set; } = "";

    [JsonPropertyName("dchi_kh")]
    public string DchiKh { get; set; } = "";

    [JsonPropertyName("tt_th")]
    public bool TtTh { get; set; }

    [JsonPropertyName("so_cmt")]
    public string SoCmt { get; set; } = "";

    [JsonPropertyName("ten")]
    public string Ten { get; set; } = "";

    [JsonPropertyName("dchi")]
    public string Dchi { get; set; } = "";

    [JsonPropertyName("gioi")]
    public string Gioi { get; set; } = "0";

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("bien_xe")]
    public string BienXe { get; set; } = "";

    [JsonPropertyName("so_khung")]
    public string SoKhung { get; set; } = "";

    [JsonPropertyName("so_may")]
    public string SoMay { get; set; } = "";

    [JsonPropertyName("hang_xe")]
    public string HangXe { get; set; } = "";

    [JsonPropertyName("hieu_xe")]
    public string HieuXe { get; set; } = "";

    [JsonPropertyName("tgian_sx")]
    public string TgianSx { get; set; } = "";

    [JsonPropertyName("so_cn")]
    public int SoCn { get; set; }

    [JsonPropertyName("ttai")]
    public int Ttai { get; set; }

    [JsonPropertyName("md_sd")]
    public string MdSd { get; set; } = "K";

    [JsonPropertyName("loai_xe")]
    public string LoaiXe { get; set; } = "01.01";

    [JsonPropertyName("ngay_hl")]
    public string NgayHl { get; set; } = "";

    [JsonPropertyName("thang")]
    public string Thang { get; set; } = "1";

    [JsonPropertyName("ngay_kt")]
    public string NgayKt { get; set; } = "";

    [JsonPropertyName("bn")]
    public bool Bn { get; set; } = true;

    [JsonPropertyName("phi_tnds")]
    public decimal PhiTnds { get; set; }

    [JsonPropertyName("tl")]
    public bool Tl { get; set; }

    [JsonPropertyName("goi_tlx")]
    public decimal GoiTlx { get; set; }

    [JsonPropertyName("tien_tlx")]
    public decimal TienTlx { get; set; }

    [JsonPropertyName("pt_tlx")]
    public string PtTlx { get; set; } = "0";

    [JsonPropertyName("phi_tlx")]
    public decimal PhiTlx { get; set; }

    [JsonPropertyName("tn")]
    public bool Tn { get; set; }

    [JsonPropertyName("goi_tnx")]
    public decimal GoiTnx { get; set; }

    [JsonPropertyName("tien_tnx")]
    public decimal TienTnx { get; set; }

    [JsonPropertyName("pt_tnx")]
    public string PtTnx { get; set; } = "0";

    [JsonPropertyName("phi_tnx")]
    public decimal PhiTnx { get; set; }

    [JsonPropertyName("tv")]
    public bool Tv { get; set; } = true;

    [JsonPropertyName("pt_cb")]
    public string PtCb { get; set; } = "0";

    [JsonPropertyName("pt_gia")]
    public string PtGia { get; set; } = "0";

    [JsonPropertyName("pt_tv_g")]
    public decimal PtTvG { get; set; }

    [JsonPropertyName("phi_tvbs")]
    public string PhiTvbs { get; set; } = "0";

    [JsonPropertyName("gia_xe_tk")]
    public string GiaXeTk { get; set; } = "0";

    [JsonPropertyName("gia_xe")]
    public string GiaXe { get; set; } = "0";

    [JsonPropertyName("tien_bh")]
    public string TienBh { get; set; } = "0";

    [JsonPropertyName("pt_sang")]
    public string PtSang { get; set; } = "0";

    [JsonPropertyName("pt_tv")]
    public string PtTv { get; set; } = "0";

    [JsonPropertyName("phi_tv")]
    public decimal PhiTv { get; set; }

    [JsonPropertyName("tch")]
    public bool Tch { get; set; }

    [JsonPropertyName("phi_dvch")]
    public string PhiDvch { get; set; } = "0";

    [JsonPropertyName("muc_ktru")]
    public decimal MucKtru { get; set; }

    [JsonPropertyName("ndung")]
    public string Ndung { get; set; } = "";

    [JsonPropertyName("dt_dkbs")]
    public object? DtDkbs { get; set; }

    [JsonPropertyName("keystore")]
    public string Keystore { get; set; } = "";

    [JsonPropertyName("fileAtt")]
    public object? FileAtt { get; set; }
}

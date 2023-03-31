namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Müstahsil Makbuzu Ana Nesnesinde Olması Gereken Alanlarla oluşturulan sınıftır
    /// Burada firma, iletişim, banka, Makbuz toplam tutarı vb. bilgiler bulunmaktadır
    /// </summary>
    public class MustahsilMakbuzuDTO : OrtakVerilerDTO
    {
        // /// <summary>
        // /// Müstahsil Makbuzu Id Bilgisi
        // /// </summary>
        // public string MustahsilMakbuzuId;

        /// <summary>
        /// Makbuz Kesen Birimin Alt Şube Bilgisi
        /// </summary>
        public string AltBirimAd;

        /// <summary>
        /// Müstahsil Makbuzu No Bilgisi
        /// </summary>
        public string MustahsilMakbuzuNo;

        /// <summary>
        /// Net Tutar Bilgisi
        /// </summary>
        public decimal? NetTutar;

        /// <summary>
        /// Müstahsil Makbuzu Tutar Bilgisi
        /// </summary>
        public decimal? MustahsilMakbuzuTutari;

        /// <summary>
        /// Gelir Vergisi Bilgisi
        /// </summary>
        public decimal? GelirVergisi;

        /// <summary>
        /// Makbuz Grup Türü Kod Bilgisi
        /// </summary>
        public int? MakbuzGrupTuruKod;
    }
}
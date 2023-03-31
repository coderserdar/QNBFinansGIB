namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Müstahsil Makbuzu Detay Nesnesinde olması gereken alanlarla oluşturulan sınıftır
    /// Tutarlar, ürün adı, miktar, birim fiyat gibi bilgiler bulunmaktadır
    /// </summary>
    public class MustahsilMakbuzuDetayDTO : OrtakDetayVerilerDTO
    {
        // /// <summary>
        // /// Müstahsil Makbuzu Id Bilgisi
        // /// </summary>
        // public string MustahsilMakbuzuId;

        /// <summary>
        /// İşin Mahiyeti Bilgisi
        /// </summary>
        public string IsinMahiyeti;

        /// <summary>
        /// Gelir Vergisi Bilgisi
        /// </summary>
        public decimal? GelirVergisi;

        /// <summary>
        /// Net Tutar Bilgisi
        /// </summary>
        public decimal? NetTutar;

        /// <summary>
        /// Toplam Tutar Bilgisi
        /// </summary>
        public decimal? ToplamTutar;
    }
}
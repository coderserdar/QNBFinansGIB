namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Müstahsil Makbuzu Detay Nesnesinde olması gereken alanlarla oluşturulan sınıftır
    /// Tutarlar, ürün adı, miktar, birim fiyat gibi bilgiler bulunmaktadır
    /// </summary>
    public class MustahsilMakbuzuDetayDTO
    {
        /// <summary>
        /// Müstahsil Makbuzu Id Bilgisi
        /// </summary>
        public string MustahsilMakbuzuId { get; set; }

        /// <summary>
        /// İşin Mahiyeti Bilgisi
        /// </summary>
        public string IsinMahiyeti { get; set; }

        /// <summary>
        /// Birim Fiyat Bilgisi
        /// </summary>
        public decimal? BirimFiyat;

        /// <summary>
        /// Miktar Bilgisi
        /// </summary>
        public decimal? Miktar;

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

        /// <summary>
        /// GİB Kısaltma Bilgisi (Malzemenin Ölçü Birimi, GİB tarafında Kilogram KGM olarak adlandırılmaktadır)
        /// </summary>
        public string GibKisaltma { get; set; }
    }
}
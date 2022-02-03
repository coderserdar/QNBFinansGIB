using System;

namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Giden Fatura Detay typelist'indeki alanlar üzerinden oluşturulan sınıftır
    /// </summary>
    public class GidenFaturaDetayDTO
    {
        /// <summary>
        /// Fatura Detayındaki Ürün Türü Bilgisi
        /// </summary>
        public string FaturaUrunTuru;

        /// <summary>
        /// İskonto Oran Bilgisi
        /// </summary>
        public decimal? IskontoOran;

        /// <summary>
        /// Plaka No Bilgisi
        /// </summary>
        public string PlakaNo;

        /// <summary>
        /// İrsaliye No Bilgisi
        /// </summary>
        public string IrsaliyeNo;

        /// <summary>
        /// Tonaj Bilgisi
        /// </summary>
        public decimal? Tonaj;

        /// <summary>
        /// Sevk İrsaliyesi No Bilgisi
        /// </summary>
        public string SevkIrsaliyesiNo;

        /// <summary>
        /// Sevk İrsaliye Tarihi Tarihi Bilgisi
        /// </summary>
        public DateTime? SevkIrsaliyeTarihi { get; set; }

        /// <summary>
        /// Yükleme Formu No Bilgisi
        /// </summary>
        public int? YuklemeFormuNo;

        /// <summary>
        /// Birim Fiyat Bilgisi
        /// </summary>
        public decimal? BirimFiyat;

        /// <summary>
        /// Miktar Bilgisi
        /// </summary>
        public decimal? Miktar;

        /// <summary>
        /// KDV Hariç Tutar Bilgisi
        /// </summary>
        public decimal? KdvHaricTutar;

        /// <summary>
        /// KDV Tutarı Bilgisi
        /// </summary>
        public decimal? KdvTutari;

        /// <summary>
        /// Malzeme Fatura Açıklaması Bilgisi
        /// </summary>
        public string MalzemeFaturaAciklamasi;

        /// <summary>
        /// GİB Kısaltma Bilgisi
        /// </summary>
        public string GibKisaltma;

        /// <summary>
        /// KDV Oran Bilgisi
        /// </summary>
        public decimal? KdvOran;
    }
}
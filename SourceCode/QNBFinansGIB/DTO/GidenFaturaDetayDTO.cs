using System;

namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Giden Fatura Detay Nesnesinde olması gereken alanlarla oluşturulan sınıftır
    /// Tutarlar, ürün adı, KDV ve iskonto oranları
    /// Malzeme faturası ile ilgili alanlar
    /// Malzemenin taşındığı araç Plaka, sevk irsaliyesi vb. bilgileri içerir.
    /// </summary>
    public class GidenFaturaDetayDTO : OrtakDetayVerilerDTO
    {
        /// <summary>
        /// Giden Fatura Id Bilgisi
        /// </summary>
        public string GidenFaturaId;

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
        /// Sevk İrsaliye Tarihi Bilgisi
        /// </summary>
        public DateTime? SevkIrsaliyeTarihi;

        /// <summary>
        /// Yükleme Formu No Bilgisi
        /// </summary>
        public int? YuklemeFormuNo;

        /// <summary>
        /// KDV Hariç Tutar Bilgisi
        /// </summary>
        public decimal? KdvHaricTutar;

        /// <summary>
        /// KDV Tutarı Bilgisi
        /// </summary>
        public decimal? KdvTutari;
        
        /// <summary>
        /// Konaklama Vergisi Bilgisi
        /// </summary>
        public decimal? KonaklamaVergisi;

        /// <summary>
        /// Malzeme Fatura Açıklaması Bilgisi
        /// </summary>
        public string MalzemeFaturaAciklamasi;

        /// <summary>
        /// KDV Oran Bilgisi
        /// </summary>
        public decimal? KdvOran;
    }
}
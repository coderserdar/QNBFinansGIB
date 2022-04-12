using System;

namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Müstahsil Makbuzu Ana Nesnesinde Olması Gereken Alanlarla oluşturulan sınıftır
    /// Burada firma, iletişim, banka, Makbuz toplam tutarı vb. bilgiler bulunmaktadır
    /// </summary>
    public class MustahsilMakbuzuDTO
    {
        /// <summary>
        /// Müstahsil Makbuzu Id Bilgisi
        /// </summary>
        public string MustahsilMakbuzuId { get; set; }

        /// <summary>
        /// Makbuz Kesen Birimin Alt Şube Bilgisi
        /// </summary>
        public string AltBirimAd { get; set; }

        /// <summary>
        /// Tüzel Kişi Ad Bilgisi
        /// </summary>
        public string TuzelKisiAd { get; set; }

        /// <summary>
        /// Vergi Numarası Bilgisi
        /// </summary>
        public string VergiNo { get; set; }

        /// <summary>
        /// Vergi Dairesi Bilgisi
        /// </summary>
        public string VergiDairesi { get; set; }

        /// <summary>
        /// İl Ad Bilgisi
        /// </summary>
        public string IlAd { get; set; }

        /// <summary>
        /// İlçe Ad Bilgisi
        /// </summary>
        public string IlceAd { get; set; }

        /// <summary>
        /// Adres Bilgisi
        /// </summary>
        public string Adres { get; set; }

        /// <summary>
        /// E-Posta Adresi Bilgisi
        /// </summary>
        public string EPostaAdresi { get; set; }

        /// <summary>
        /// Faks No Bilgisi
        /// </summary>
        public string FaksNo { get; set; }

        /// <summary>
        /// İBAN No Bilgisi
        /// </summary>
        public string IbanNo { get; set; }

        /// <summary>
        /// Banka Ad Bilgisi
        /// </summary>
        public string BankaAd { get; set; }

        /// <summary>
        /// Banka Şube Bilgisi
        /// </summary>
        public string BankaSube { get; set; }

        /// <summary>
        /// Müstahsil Makbuzu No Bilgisi
        /// </summary>
        public string MustahsilMakbuzuNo { get; set; }

        /// <summary>
        /// Müstahsil Makbuzu Tarihi Bilgisi
        /// </summary>
        public DateTime? MustahsilMakbuzuTarihi { get; set; }

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
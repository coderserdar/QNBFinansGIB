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
        public string MustahsilMakbuzuId;

        /// <summary>
        /// Makbuz Kesen Birimin Alt Şube Bilgisi
        /// </summary>
        public string AltBirimAd;

        /// <summary>
        /// Tüzel Kişi Ad Bilgisi
        /// </summary>
        public string TuzelKisiAd;

        /// <summary>
        /// Vergi Numarası Bilgisi
        /// </summary>
        public string VergiNo;

        /// <summary>
        /// Vergi Dairesi Bilgisi
        /// </summary>
        public string VergiDairesi;

        /// <summary>
        /// İl Ad Bilgisi
        /// </summary>
        public string IlAd;

        /// <summary>
        /// İlçe Ad Bilgisi
        /// </summary>
        public string IlceAd;

        /// <summary>
        /// Adres Bilgisi
        /// </summary>
        public string Adres;

        /// <summary>
        /// E-Posta Adresi Bilgisi
        /// </summary>
        public string EPostaAdresi;

        /// <summary>
        /// Faks No Bilgisi
        /// </summary>
        public string FaksNo;

        /// <summary>
        /// İBAN No Bilgisi
        /// </summary>
        public string IbanNo;

        /// <summary>
        /// Banka Ad Bilgisi
        /// </summary>
        public string BankaAd;

        /// <summary>
        /// Banka Şube Bilgisi
        /// </summary>
        public string BankaSube;

        /// <summary>
        /// Müstahsil Makbuzu No Bilgisi
        /// </summary>
        public string MustahsilMakbuzuNo;

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
    }
}
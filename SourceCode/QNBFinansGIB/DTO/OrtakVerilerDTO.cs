using System;

namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Müstahsil Makbuzu ve E-Fatura kısmında
    /// BuyerCustomerParty XML kısmının hazırlanması için
    /// Gerekli olan ortak alanların belirlendiği sınıftır.
    /// </summary>
    public class OrtakVerilerDTO
    {
        /// <summary>
        /// Müstahsil Makbuzu veya Fatura Düzenleme Tarihi Bilgisi
        /// </summary>
        public DateTime? IslemTarihi;
        
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
    }
}
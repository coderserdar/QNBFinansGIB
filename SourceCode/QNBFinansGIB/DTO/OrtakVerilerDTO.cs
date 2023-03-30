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
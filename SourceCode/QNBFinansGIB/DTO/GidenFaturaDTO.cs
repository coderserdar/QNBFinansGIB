using System;

namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Giden Fatura Ana Nesnesinde Olması Gereken Alanlarla oluşturulan sınıftır
    /// Burada firma, iletişim, banka, fatura toplam tutarı vb. bilgiler bulunmaktadır
    /// </summary>
    public class GidenFaturaDTO
    {
        /// <summary>
        /// Giden Fatura Id Bilgisi
        /// </summary>
        public string GidenFaturaId;

        /// <summary>
        /// Fatura Kesen Birimin Alt Şube Bilgisi
        /// </summary>
        public string AltBirimAd;

        /// <summary>
        /// Satış Türü Kod Bilgisi
        /// </summary>
        public int? SatisTuruKod;

        /// <summary>
        /// Fatura Türü Kod Bilgisi
        /// </summary>
        public int? FaturaTuruKod;

        #region Gerçek Kişi Bilgileri

        /// <summary>
        /// Gerçek Kişi Ad Bilgisi
        /// </summary>
        public string GercekKisiAd;

        /// <summary>
        /// Gerçek Kişi Soyad Bilgisi
        /// </summary>
        public string GercekKisiSoyad;

        /// <summary>
        /// Gerçek Kişi TC Kimlik No Bilgisi
        /// </summary>
        public string GercekKisiTcKimlikNo;

        /// <summary>
        /// İkametgah Adresi Bilgisi
        /// </summary>
        public string IkametgahAdresi;

        /// <summary>
        /// Cep Telefon No Bilgisi
        /// </summary>
        public string CepTelefonNo;
        #endregion

        #region Tüzel Kişi Bilgileri
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
        /// Telefon No Bilgisi
        /// </summary>
        public string TelefonNo;

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
        #endregion

        /// <summary>
        /// Düzenleme Tarihi Bilgisi
        /// </summary>
        public DateTime? DuzenlemeTarihi { get; set; }

        /// <summary>
        /// GİB Numarası Bilgisi
        /// </summary>
        public string GibNumarasi;

        /// <summary>
        /// Belge Oid Id alanı
        /// </summary>
        public string BelgeOid;

        /// <summary>
        /// KDV Hariç Tutar Bilgisi
        /// </summary>
        public decimal? KdvHaricTutar;

        /// <summary>
        /// KDV Tutarı Bilgisi
        /// </summary>
        public decimal? KdvTutari;

        /// <summary>
        /// Fatura Tutarı Bilgisi
        /// </summary>
        public decimal? FaturaTutari;
    }
}
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
        public string GidenFaturaId { get; set; }

        /// <summary>
        /// Fatura Kesen Birimin Alt Şube Bilgisi
        /// </summary>
        public string AltBirimAd { get; set; }

        /// <summary>
        /// Satış Türü Kod Bilgisi
        /// </summary>
        public int? SatisTuruKod;

        /// <summary>
        /// Fatura Türü Kod Bilgisi
        /// </summary>
        public int? FaturaTuruKod;

        /// <summary>
        /// Fatura Grup Türü Kod Bilgisi
        /// </summary>
        public int? FaturaGrupTuruKod;

        /// <summary>
        /// Fatura Açıklama Bilgisi
        /// </summary>
        public string Aciklama { get; set; }

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
        /// Telefon No Bilgisi
        /// </summary>
        public string TelefonNo { get; set; }

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
        #endregion

        /// <summary>
        /// Düzenleme Tarihi Bilgisi
        /// </summary>
        public DateTime? DuzenlemeTarihi { get; set; }

        /// <summary>
        /// GİB Numarası Bilgisi
        /// </summary>
        public string GibNumarasi { get; set; }

        /// <summary>
        /// Belge Oid Id alanı
        /// </summary>
        public string BelgeOid { get; set; }

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
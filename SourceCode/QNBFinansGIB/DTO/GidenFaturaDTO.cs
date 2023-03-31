using System;

namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Giden Fatura Ana Nesnesinde Olması Gereken Alanlarla oluşturulan sınıftır
    /// Burada firma, iletişim, banka, fatura toplam tutarı vb. bilgiler bulunmaktadır
    /// </summary>
    public class GidenFaturaDTO : OrtakVerilerDTO
    {
        // /// <summary>
        // /// Giden Fatura Id Bilgisi
        // /// </summary>
        // public string GidenFaturaId;

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

        /// <summary>
        /// Fatura Grup Türü Kod Bilgisi
        /// </summary>
        public int? FaturaGrupTuruKod;

        /// <summary>
        /// Fatura Açıklama Bilgisi
        /// </summary>
        public string Aciklama;

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

        /// <summary>
        /// Telefon No Bilgisi
        /// </summary>
        public string TelefonNo;

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
        /// Konaklama Vergisi Bilgisi
        /// </summary>
        public decimal? KonaklamaVergisi;

        /// <summary>
        /// Fatura Tutarı Bilgisi
        /// </summary>
        public decimal? FaturaTutari;
    }
}
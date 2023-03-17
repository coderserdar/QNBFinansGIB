using System;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// Satış Türü Bilgilerinin Bulunduğu Enum sınıfı
    /// </summary>
    public static class Enums
    {
        [Flags]
        public enum BelgeTur
        {
            EFatura,
            EArsiv
        }
        
        [Flags]
        public enum LetterCase
        {
            Lower,
            Upper
        }
        
        /// <summary>
        /// Faturadaki Satış Türlerinin Kodlarının Bulunduğu enum bilgisi
        /// Burada İhraç Kayıtlı ise XML içerisinde bazı yapısal değişiklikler gerçekleştirildiği için
        /// Bu konu önemlidir.
        /// </summary>
        public enum SatisTur
        {
            Vadeli = 1,
            Hesaben = 2,
            Pesin = 3,
            TeskilataVerilen = 4,
            TeslimattaVadeli = 5,
            CiftciyeVerilen = 6,
            IstirakeSatilan = 7,
            PersoneleSatilan = 8,
            ImalatciIhracatci = 9,
            IhracKayitli = 10,
            IstirakeSatilanRezervli = 11,
            RafFiyatGarantili = 12
        }

        /// <summary>
        /// Faturada Türlerinin Kodlarının Bulunduğu enum bilgisi
        /// Burada İade faturası ise XML içerisinde bazı yapısal değişiklikler gerçekleştirildiği için
        /// Bu konu önemlidir.
        /// </summary>
        public enum FaturaTur
        {
            Muhtelif = 1,
            Kantin = 2,
            Ambar = 3,
            Satis = 4,
            Iade = 5,
            Fark = 6,
            Personel = 7,
            Proforma = 8,
            NoterlikMakbuzu = 9,
            Harcirah = 10
        }

        /// <summary>
        /// İstisna Faturalarındave Müstahsil Makbuzlarında Grup Türüne Göre Kontrol yapılmasını sağlayan enum Bilgisi
        /// Burada Küspe olup olmamasına göre XML içeriğinde değişiklikler yapıldığı için
        /// Bu kısım önemlidir
        /// </summary>
        public enum FaturaMakbuzGrupTur
        {
            Seker = 1,
            Melas = 2,
            Kuspe = 3,
            Diger = 4,
            Malzeme = 5,
            PancarKuyrugu = 6,
            Alkol = 7,
            Personel = 8
        }
    }
}
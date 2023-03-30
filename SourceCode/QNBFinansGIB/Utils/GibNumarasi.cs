using System;
using System.Text;
using static QNBFinansGIB.Utils.Enums;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// GİB Numarası değerlerinin otomatik olarak oluşturulması için hazırlanan sınıftır
    /// </summary>
    public static class GibNumarasi
    {
        /// <summary>
        /// Fatura veya Makbuz nesnesi üzerinden rastgele GİB numarası oluşturulması için
        /// Gerekli metottur
        /// </summary>
        /// <param name="belgeTarihi">Belge Tarihi Bilgisi</param>
        /// <returns>GİB Numarası</returns>
        public static string RastgeleGibNumarasiOlustur(DateTime? belgeTarihi)
        {
            var baslangicMetni = RastgeleMetinOlustur(3, LetterCase.Lower);
            var islemTarihi = belgeTarihi ?? DateTime.Now;
            // maksimum 9 haneli olacağı için böyle bir güncelleme yapıldı
            var faturaSayisi = new Random().Next(Sabitler.MaksimumFaturaNumarasi);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(baslangicMetni);
            stringBuilder.Append(islemTarihi.Year.ToString());
            stringBuilder.Append(faturaSayisi.ToString().PadLeft(9, '0'));
            return stringBuilder.ToString();
        }
    
        /// <summary>
        /// Girilen boyut ve yazı türüne göre
        /// Rastgele metin oluşturulması için hazırlanan bir metottur
        /// </summary>
        /// <param name="boyut">İstenen metnin boyutu</param>
        /// <param name="letterCase">Yazı Tipi (Büyük Harf mi, Küçük Harf Mi)</param>
        /// <returns>Rastgele Metin Bilgisi</returns>
        private static string RastgeleMetinOlustur(int boyut, LetterCase letterCase)  
        {  
            var builder = new StringBuilder(boyut);
            var random = new Random();
            var offset = letterCase == LetterCase.Lower ? 'a' : 'A';  
            const int lettersOffset = 26;
            for (var i = 0; i < boyut; i++)  
            {  
                var @char = (char)random.Next(offset, offset + lettersOffset);  
                builder.Append(@char);  
            }  
            return letterCase == LetterCase.Lower ? builder.ToString().ToLower() : builder.ToString();  
        }  
    }
}
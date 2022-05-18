using System;
using System.Text;
using QNBFinansGIB.DTO;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// GİB Numarası değerlerinin otomatik olarak oluşturulması için hazırlanan sınıftır
    /// </summary>
    public static class GibNumarasi
    {
        /// <summary>
        /// Fatura nesnesi üzerinden rastgele GİB numarası oluşturulması için
        /// Gerekli metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Nesnesi</param>
        /// <returns>GİB Numarası</returns>
        public static string RastgeleGibNumarasiOlusturFaturadan(GidenFaturaDTO gidenFatura)
        {
            var baslangicMetni = RastgeleMetinOlustur(3, false);
            var islemTarihi = gidenFatura.DuzenlemeTarihi ?? DateTime.Now;
            // maksimum 9 haneli olacağı için böyle bir güncelleme yapıldı
            var faturaSayisi = new Random().Next(999999999);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(baslangicMetni);
            stringBuilder.Append(islemTarihi.Year.ToString());
            stringBuilder.Append(faturaSayisi.ToString().PadLeft(9, '0'));
            return stringBuilder.ToString();
        }
        
        /// <summary>
        /// Müstahsil Makbuzu nesnesi üzerinden rastgele GİB numarası oluşturulması için
        /// Gerekli metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Nesnesi</param>
        /// <returns>GİB Numarası</returns>
        public static string RastgeleGibNumarasiOlusturMakbuzdan(MustahsilMakbuzuDTO mustahsilMakbuzu)
        {
            var baslangicMetni = RastgeleMetinOlustur(3, false);
            var islemTarihi = mustahsilMakbuzu.MustahsilMakbuzuTarihi ?? DateTime.Now;
            // maksimum 9 haneli olacağı için böyle bir güncelleme yapıldı
            var faturaSayisi = new Random().Next(999999999);
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
        /// <param name="size">İstenen metnin boyutu</param>
        /// <param name="lowerCase">Yazı Tipi (Büyük Harf mi, Küçük Harf Mi)</param>
        /// <returns>Rastgele Metin Bilgisi</returns>
        private static string RastgeleMetinOlustur(int size, bool lowerCase)  
        {  
            var builder = new StringBuilder(size);
            var random = new Random();
            char offset = lowerCase ? 'a' : 'A';  
            const int lettersOffset = 26; // A...Z or a..z: length=26  
            for (var i = 0; i < size; i++)  
            {  
                var @char = (char)random.Next(offset, offset + lettersOffset);  
                builder.Append(@char);  
            }  
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();  
        }  
    }
}
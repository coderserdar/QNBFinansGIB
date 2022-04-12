using System.Linq;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// E-Fatura Servisinde Önizleme Metodu çağrıldığında dönecek olan değer
    /// Burada Tipe göre
    /// Dosya uzantısı Zip veya PDF olacağı için
    /// Bu konu önemlidir
    /// </summary>
    public class GeriDonus
    {
        /// <summary>
        /// Çıktı alınacak dosyanın byte dizisi hali
        /// </summary>
        public byte[] Dosya { get; set; }
        /// <summary>
        /// Çıktı Tipi Bilgisi
        /// </summary>
        public int Tip { get; set; }
    }

    /// <summary>
    /// Yapılan Ekranlarda Verilecek Mesajlarla İlgili Sabit Sınıf
    /// </summary>
    public static class MesajSabitler
    {
        /// <summary>
        /// İşlemin Başarılı Olduğuna Dair Mesaj Bilgisi
        /// </summary>
        public static string IslemBasarili = "İşlem Başarılı";
        /// <summary>
        /// İşlemin Başarısız Olduğuna Dair Mesaj Bilgisi
        /// </summary>
        public static string IslemBasarisiz = "İşlem Başarısız";
        /// <summary>
        /// İşlem Mesajlarının Kutusunda Gösterilecek Başlık Mesaj Bilgisi
        /// </summary>
        public static string MesajBasligi = "QNB Finans GİB Servis Uygulaması";
    }

    public static class BoslukKaldir
    {
        public static TSelf BosluklariKaldir<TSelf>(this TSelf input)
        {
            if (input == null)
                return input;

            var stringProperties = typeof(TSelf).GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            foreach (var stringProperty in stringProperties)
            {
                string currentValue = (string)stringProperty.GetValue(input, null);
                if (currentValue != null)
                    stringProperty.SetValue(input, currentValue.Trim().TrimEnd().TrimStart(), null);
            }
            return input;
        }
    }
}
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
    }
}
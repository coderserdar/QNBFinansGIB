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
}
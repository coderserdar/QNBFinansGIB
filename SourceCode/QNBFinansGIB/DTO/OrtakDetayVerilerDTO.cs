namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Müstahsil Makbuzu ve E-Fatura Detay kısmında
    /// Ortak Bulunan alanların hazırlanması için
    /// Gerekli olan ortak alanların belirlendiği sınıftır.
    /// </summary>
    public class OrtakDetayVerilerDTO
    {
        /// <summary>
        /// Giden Fatura veya Müstahsil Makbuzu Id Bilgisi
        /// </summary>
        public string AnaNesneId;
        
        /// <summary>
        /// Birim Fiyat Bilgisi
        /// </summary>
        public decimal? BirimFiyat;

        /// <summary>
        /// Miktar Bilgisi
        /// </summary>
        public decimal? Miktar;
        
        /// <summary>
        /// GİB Kısaltma Bilgisi (Malzemenin Ölçü Birimi, GİB tarafında Kilogram KGM olarak adlandırılmaktadır)
        /// </summary>
        public string GibKisaltma;
    }
}
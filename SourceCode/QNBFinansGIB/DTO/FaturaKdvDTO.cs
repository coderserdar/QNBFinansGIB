namespace QNBFinansGIB.DTO
{
    /// <summary>
    /// Fatura Detaylarında Farklı Tür KDV Oranları Kullanılmışsa
    /// Çıktı tarafında (XML)
    /// TaxTotal kısmında bu bilgilerin ayrı ayrı gösterilmesi için hazırlanan nesnedir
    /// </summary>
    public class FaturaKdvDTO
    {
        /// <summary>
        /// KDV Oran Bilgisi
        /// </summary>
        public decimal? KdvOran;

        /// <summary>
        /// KDV Tutarı Bilgisi
        /// </summary>
        public decimal? KdvTutari;

        /// <summary>
        /// KDV Hariç Tutar Bilgisi
        /// </summary>
        public decimal? KdvHaricTutar;
    }
}
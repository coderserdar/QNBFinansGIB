using System.Linq;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// Girdi olarak verilen bir nesnenin
    /// İçerisindeki String türündeki elemanları kontrol ederek
    /// Bu string değerlerdei boşlukları kaldırmaya yarayan bir sınıftır
    /// </summary>
    public static class BoslukKaldir
    {
        /// <summary>
        /// Generic olarak herhangi bir sınıf vb. kontrol edildiğinde
        /// İçerisindeki string değerlerin boşluklarının kaldırılması için
        /// Hazırlanmış metottur.
        /// </summary>
        /// <typeparam name="TSelf">Girdinin ait olduğu tür, sınıf vb.</typeparam>
        /// <param name="input">Girdi Değeri</param>
        /// <returns>Girdinin boşluksuz halleri</returns>
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
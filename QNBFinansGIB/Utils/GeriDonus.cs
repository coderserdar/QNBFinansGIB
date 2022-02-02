namespace QNBFinansGIB.Utils
{
    public class GeriDonus
    {
        public byte[] Dosya { get; set; }
        public int Tip { get; set; }
    }

    public static class MesajSabitler
    {
        public static string IslemBasarili = "İşlem Başarılı";
        public static string IslemBasarisiz = "İşlem Başarısız";
    }
}
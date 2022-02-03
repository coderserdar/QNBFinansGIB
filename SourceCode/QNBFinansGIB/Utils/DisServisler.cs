using QNBFinansGIB.DTO;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QNBFinansGIB.Utils
{
    public static class DisServisler
    {
        #region QNB GİB

        private static readonly string GIBKullaniciAdi = "turkseker.ws";
        private static readonly string GIBSifre = "1234qqqQ";
        private static readonly string GIBVKN = "3250566851";
        private static readonly string GIBERPKodu = "TSF30125";
        private static GIBUserService.userService gibUserService = new GIBUserService.userService();
        private static GIBEFatura.connectorService gibEFaturaService = new GIBEFatura.connectorService();
        private static GIBEArsiv.EarsivWebService gibEArsivService = new GIBEArsiv.EarsivWebService();

        /// <summary>
        /// Vergi Kimlik Numarası Gönderilen Tüzel Kişinin E Fatura Kullanıcısı Olup Olmadığını Kontrol Eden
        /// Bir Metottur
        /// </summary>
        /// <param name="vergiNo">Tüzel Kişi Vergi Kimlik Numarası</param>
        /// <returns>Kullanıcı Olup Olmadığı Bilgisi</returns>
        public static bool EFaturaKullanicisiMi(string vergiNo)
        {
            try
            {
                var kullaniciMi = false;

                gibUserService = new GIBUserService.userService();
                gibEFaturaService = new GIBEFatura.connectorService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEFaturaService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                kullaniciMi = gibEFaturaService.efaturaKullanicisi(vergiNo);

                return kullaniciMi;
            }
            catch (System.Exception ex)
            {
                return false;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Oluşturulan XML dosyasının QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="gidenFaturaId">Giden Fatura Id Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns></returns>
        public static string EFaturaGonder(GidenFaturaDTO gidenFatura, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEFaturaService = new GIBEFatura.connectorService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEFaturaService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var parametreler = new GIBEFatura.gidenBelgeParametreleri();
                parametreler.vergiTcKimlikNo = "3250566851";
                parametreler.belgeTuru = "FATURA_UBL";
                parametreler.belgeNo = gidenFatura.GidenFaturaId;
                parametreler.belgeVersiyon = "1.0";
                parametreler.veri = System.IO.File.ReadAllBytes(dosyaAdi);
                parametreler.belgeHash = GetMD5Hash(parametreler.veri);
                parametreler.mimeType = "application/xml";
                parametreler.erpKodu = GIBERPKodu;

                return gibEFaturaService.belgeGonderExt(parametreler);
            }
            catch (System.Exception ex)
            {
                return MesajSabitler.IslemBasarisiz;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Oluşturulan XML dosyasının QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns></returns>
        public static GeriDonus EFaturaOnIzleme(GidenFaturaDTO gidenFatura, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEFaturaService = new GIBEFatura.connectorService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEFaturaService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                string vergiTcKimlikNo = "3250566851";
                if (!File.Exists(dosyaAdi))
                {
                    return null;
                }
                byte[] veri = System.IO.File.ReadAllBytes(dosyaAdi);
                string belgeFormati = "PDF";  //HTML, PDF, UBL
                string belgeTuru = "FATURA"; //EFATURA ve EIRSALIYE değerleri alabilir.

                byte[] onizleme = new byte[1];

                var geriDonus = new GeriDonus
                {
                    Dosya = onizleme,
                    Tip = 0
                };

                if (!string.IsNullOrEmpty(gidenFatura.BelgeOid))
                {
                    var idListesi = new string[1];
                    idListesi[0] = gidenFatura.BelgeOid;
                    onizleme = gibEFaturaService.gidenBelgeleriIndir(vergiTcKimlikNo, idListesi, belgeTuru, belgeFormati);
                    geriDonus.Dosya = onizleme;
                    geriDonus.Tip = 1;
                }
                else
                {
                    onizleme = gibEFaturaService.ublOnizleme(vergiTcKimlikNo, veri, belgeFormati, belgeTuru);
                    geriDonus.Dosya = onizleme;
                    geriDonus.Tip = 0;
                }

                return geriDonus;
            }
            catch (System.Exception ex)
            {
                return null;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Oluşturulan XML dosyasının QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="gidenFaturaId">Giden Fatura Id Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns></returns>
        public static string EArsivGonder(GidenFaturaDTO gidenFatura, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEArsivService = new GIBEArsiv.EarsivWebService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEArsivService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var belge = new GIBEArsiv.belge();

                string input = "{\"islemId\":\"" + gidenFatura.GidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";

                var fatura = new GIBEArsiv.belge();
                fatura.belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL;
                fatura.belgeFormatiSpecified = true;
                fatura.belgeIcerigi = System.IO.File.ReadAllBytes(dosyaAdi);
                GIBEArsiv.earsivServiceResult serviceResult = new GIBEArsiv.earsivServiceResult();
                belge = gibEArsivService.faturaOlustur(input, fatura, out serviceResult);

                if (serviceResult.resultCode != "AE00000")
                    return MesajSabitler.IslemBasarisiz;
                else
                    return MesajSabitler.IslemBasarili;
            }
            catch (System.Exception ex)
            {
                return MesajSabitler.IslemBasarisiz;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Oluşturulan XML dosyasının QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="gidenFaturaId">Giden Fatura Id Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns></returns>
        public static byte[] EArsivOnIzleme(GidenFaturaDTO gidenFatura, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEArsivService = new GIBEArsiv.EarsivWebService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEArsivService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var belge = new GIBEArsiv.belge();

                string input = "{\"islemId\":\"" + gidenFatura.GidenFaturaId.ToUpper() + "\",\"faturaUuid\":\"" + gidenFatura.GidenFaturaId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"\",\"donenBelgeFormati\":\"3\"}"; // Buradaki 3 PDF

                var fatura = new GIBEArsiv.belge();
                fatura.belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL;
                fatura.belgeFormatiSpecified = true;
                fatura.belgeIcerigi = System.IO.File.ReadAllBytes(dosyaAdi);

                GIBEArsiv.earsivServiceResult serviceResult = new GIBEArsiv.earsivServiceResult();
                var temp = gibEArsivService.faturaSorgula(input, out serviceResult);
                if (temp != null)
                    return temp.belgeIcerigi;
                else
                {
                    temp = gibEArsivService.faturaOnizleme(input, fatura, out serviceResult);
                    if (temp != null)
                        return temp.belgeIcerigi;
                    else
                        return null;
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Dosyanın MD5 hashi alınması için yazılan metottur
        /// </summary>
        /// <param name="gelen">Girdi Metni</param>
        /// <returns>MD5 hashlenmiş hali</returns>
        public static string GetMD5Hash(byte[] gelen)
        {
            //Fatura dosyalarının hash'lerini almak için kullanılan fonksiyondur.
            MD5 md5Hash = new MD5CryptoServiceProvider();
            byte[] data = md5Hash.ComputeHash(gelen);
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        #endregion QNB GİB
    }
}
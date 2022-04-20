using QNBFinansGIB.DTO;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// QNB Finans Firması tarafından sağlanan GİB Web Servis referanslarının tüketildiği
    /// Ve servise talep gönderilip sonuçlarının temin edildiği sınıftır.
    /// </summary>
    public static class DisServisler
    {
        #region QNB GİB

        /// <summary>
        /// Servis Kullanıcı Adı Bilgisi
        /// </summary>
        private static readonly string GIBKullaniciAdi = "turkseker.ws";
        /// <summary>
        /// Servis Şifre Bilgisi
        /// </summary>
        private static readonly string GIBSifre = "1234qqqQ";
        /// <summary>
        /// Servise Gönderilecek Vergi No Bilgisi
        /// </summary>
        private static readonly string GIBVKN = "3250566851";
        /// <summary>
        /// Servis İçin Tanımlanmış ERP Kodu Bilgisi
        /// </summary>
        private static readonly string GIBERPKodu = "TSF30125";
        /// <summary>
        /// GİB Login Servis istemcisi
        /// </summary>
        private static GIBUserService.userService gibUserService = new GIBUserService.userService();
        /// <summary>
        /// GİB E-Fatura Servis istemcisi
        /// </summary>
        private static GIBEFatura.connectorService gibEFaturaService = new GIBEFatura.connectorService();
        /// <summary>
        /// GİB E-Arşiv Servis istemcisi
        /// </summary>
        private static GIBEArsiv.EarsivWebService gibEArsivService = new GIBEArsiv.EarsivWebService();
        /// <summary>
        /// GİB E-Arşiv Servis istemcisi
        /// </summary>
        private static GIBEMustahsil.MustahsilWebService gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

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
        /// İlgili faturanın QNB Finans servislerine gönderilip gönderilmediği
        /// Gönderildi ise durumuna bakarak 
        /// Bu kaydın silinebilir olup olmadığını belirleye bir metottur.
        /// </summary>
        /// <param name="gidenFaturaId">Giden Fatura Id Bilgisi</param>
        /// <returns>Kaydın Silinmeye Uygun Olup Olmadığı Bilgisi</returns>
        public static bool EFaturaSilmeyeUygunMu(string gidenFaturaId)
        {
            try
            {
                var uygunMu = true;

                gibUserService = new GIBUserService.userService();
                gibEFaturaService = new GIBEFatura.connectorService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEFaturaService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var parametreler = new GIBEFatura.gidenBelgeParametreleri();
                parametreler.vergiTcKimlikNo = "3250566851";
                parametreler.belgeTuru = "FATURA_UBL";
                parametreler.belgeNo = gidenFaturaId;
                parametreler.erpKodu = GIBERPKodu;

                var belgeDurumEsas = gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                if (belgeDurumEsas.durum == 1 || belgeDurumEsas.durum == 3)
                    uygunMu = false;

                return uygunMu;
            }
            catch (System.Exception ex)
            {
                return true;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Gönderilen Id bilgisine sahip Faturanın Belge Oid Bilgisinin QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="gidenFaturaId">Giden Fatura Id Bilgisi</param>
        /// <param name="baslangicTarihi">Başlangıç Tarihi bilgisi</param>
        /// <param name="bitisTarihi">Bitiş Tarihi Bilgisi</param>
        /// <returns>İşlem Sonucu (Buradan dönen değer gerekli yerlerde kontrol edilip ona göre kullanıcıya bilgi verilmektedir.)</returns>
        public static string BelgeOidKontrol(string gidenFaturaId, DateTime? baslangicTarihi, DateTime? bitisTarihi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEFaturaService = new GIBEFatura.connectorService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEFaturaService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var parametreler = new GIBEFatura.gidenBelgeleriListeleParametreleri();
                parametreler.vkn = "3250566851";
                parametreler.belgeTuru = "FATURA_UBL";
                parametreler.baslangicGonderimTarihi = baslangicTarihi?.ToString("yyyyMMdd");
                parametreler.bitisGonderimTarihi = bitisTarihi?.ToString("yyyyMMdd");

                var sonucMesaji = "İşlem Başarılı";

                var liste = gibEFaturaService.gidenBelgeleriListele(parametreler);
                foreach (var item in liste)
                {
                    var nesne = item as GIBEFatura.gidenBelgeleriListeleData;
                    if (nesne.yerelBelgeNo == gidenFaturaId)
                        return nesne.belgeOid;
                }

                return sonucMesaji;
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
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı Bilgisi</param>
        /// <param name="belgeOid">Belge Oid Bilgisi</param>
        /// <returns>İşlem Sonucu (Buradan dönen değer gerekli yerlerde kontrol edilip ona göre kullanıcıya bilgi verilmektedir.)</returns>
        public static string EFaturaGonder(GidenFaturaDTO gidenFatura, string dosyaAdi, string belgeOid)
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

                var sonucMesaji = string.Empty;

                try
                {
                    var belgeDurumEsas = gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                    if (belgeDurumEsas.durum != 1 && belgeDurumEsas.durum != 3)
                    {
                        #region Belge Oid Durumuna Göre Evrak Gönderme
                        if (!string.IsNullOrEmpty(belgeOid))
                        {
                            if (belgeOid != "Tekrar Gönder")
                                sonucMesaji = gibEFaturaService.belgeGonderExt(parametreler);
                            else
                            {
                                var yenidenGonderDurum = false;
                                //var ettnDizi = new string[1];
                                //ettnDizi[0] = parametreler.belgeNo;
                                //while (yenidenGonderDurum == false)
                                //{
                                //    yenidenGonderDurum = gibEFaturaService.belgeleriTekrarGonder(parametreler.vergiTcKimlikNo, ettnDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                                //}
                                var belgeOidDizi = new string[1];
                                belgeOidDizi[0] = belgeOid;
                                while (yenidenGonderDurum == false)
                                {
                                    yenidenGonderDurum = gibEFaturaService.belgeleriTekrarGonderBelgeOid(parametreler.vergiTcKimlikNo, belgeOidDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                                }
                                sonucMesaji = belgeOid;
                            }
                        }
                        else
                            sonucMesaji = gibEFaturaService.belgeGonderExt(parametreler);
                        #endregion

                        #region Belge Durumu Kontrolü
                        if (sonucMesaji.Length <= 20)
                        {
                            // todo: buradaki metotta Belge Oid'ye göre kontrol yapılıyor
                            //var belgeDurum = gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                            //var durum = belgeDurum.durum;
                            //while (durum == 1)
                            //{
                            //    belgeDurum = gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                            //    durum = belgeDurum.durum;
                            //}
                            var belgeDurum = gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                            var durum = belgeDurum.durum;
                            while (durum == 1)
                            {
                                belgeDurum = gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                                durum = belgeDurum.durum;
                            }
                            if (durum == 2)
                                sonucMesaji = MesajSabitler.IslemBasarisiz;
                            else if (durum == 3)
                            {
                                var gonderimDurumu = belgeDurum.gonderimDurumu;
                                if (gonderimDurumu == 2)
                                    return sonucMesaji;
                                else if (gonderimDurumu == 4)
                                    return sonucMesaji;
                                else if (gonderimDurumu == 3)
                                {
                                    var gibYanitKodu = belgeDurum.gonderimCevabiKodu;
                                    if (gibYanitKodu > 1300)
                                        return sonucMesaji;
                                    else if (Sabitler.TekrarGonderilebilecekKodListesi.Any(j => j == gibYanitKodu))
                                        return "Tekrar Gönder";
                                    else if (!Sabitler.TekrarGonderilebilecekKodListesi.Any(j => j == gibYanitKodu) && (gibYanitKodu <= 1200 && gibYanitKodu >= 1100))
                                        return MesajSabitler.IslemBasarisiz;
                                    else if (gibYanitKodu == 1210 || gibYanitKodu == 1120)
                                        return MesajSabitler.IslemBasarisiz;
                                    else
                                        return sonucMesaji;
                                }
                            }
                        }
                        #endregion
                    }
                    else if (belgeDurumEsas.durum == 3)
                    {
                        if (!string.IsNullOrEmpty(belgeOid))
                            return belgeOid;
                        else
                            sonucMesaji = MesajSabitler.IslemBasarili;
                    }
                    else
                    {
                        sonucMesaji = MesajSabitler.IslemBasarisiz;
                    }

                    return sonucMesaji;
                }
                catch (Exception)
                {
                    #region Belge Oid Durumuna Göre Evrak Gönderme
                    if (!string.IsNullOrEmpty(belgeOid))
                    {
                        if (belgeOid != "Tekrar Gönder")
                            sonucMesaji = gibEFaturaService.belgeGonderExt(parametreler);
                        else
                        {
                            var yenidenGonderDurum = false;
                            //var ettnDizi = new string[1];
                            //ettnDizi[0] = parametreler.belgeNo;
                            //while (yenidenGonderDurum == false)
                            //{
                            //    yenidenGonderDurum = gibEFaturaService.belgeleriTekrarGonder(parametreler.vergiTcKimlikNo, ettnDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                            //}
                            var belgeOidDizi = new string[1];
                            belgeOidDizi[0] = belgeOid;
                            while (yenidenGonderDurum == false)
                            {
                                yenidenGonderDurum = gibEFaturaService.belgeleriTekrarGonderBelgeOid(parametreler.vergiTcKimlikNo, belgeOidDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                            }
                            sonucMesaji = belgeOid;
                        }
                    }
                    else
                        sonucMesaji = gibEFaturaService.belgeGonderExt(parametreler);
                    #endregion

                    #region Belge Durumu Kontrolü
                    if (sonucMesaji.Length <= 20)
                    {
                        // todo: buradaki metotta Belge Oid'ye göre kontrol yapılıyor
                        //var belgeDurum = gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                        //var durum = belgeDurum.durum;
                        //while (durum == 1)
                        //{
                        //    belgeDurum = gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                        //    durum = belgeDurum.durum;
                        //}
                        var belgeDurum = gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                        var durum = belgeDurum.durum;
                        while (durum == 1)
                        {
                            belgeDurum = gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                            durum = belgeDurum.durum;
                        }
                        if (durum == 2)
                            sonucMesaji = MesajSabitler.IslemBasarisiz;
                        else if (durum == 3)
                        {
                            var gonderimDurumu = belgeDurum.gonderimDurumu;
                            if (gonderimDurumu == 2)
                                return sonucMesaji;
                            else if (gonderimDurumu == 4)
                                return sonucMesaji;
                            else if (gonderimDurumu == 3)
                            {
                                var gibYanitKodu = belgeDurum.gonderimCevabiKodu;
                                if (gibYanitKodu > 1300)
                                    return sonucMesaji;
                                else if (Sabitler.TekrarGonderilebilecekKodListesi.Any(j => j == gibYanitKodu))
                                    return "Tekrar Gönder";
                                else if (!Sabitler.TekrarGonderilebilecekKodListesi.Any(j => j == gibYanitKodu) && (gibYanitKodu <= 1200 && gibYanitKodu >= 1100))
                                    return MesajSabitler.IslemBasarisiz;
                                else if (gibYanitKodu == 1210 || gibYanitKodu == 1120)
                                    return MesajSabitler.IslemBasarisiz;
                                else
                                    return sonucMesaji;
                            }
                        }
                    }
                    #endregion

                    return sonucMesaji;
                }
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
        /// Oluşturulan XML dosyasının QNB Finans Tarafında Önizlemesinin yapılmasını sağlayan metottur
        /// Eğer Belge Oid numarası varsa XML dosyasını göndermez, bu numarayla sistemdeki çıktı alınır.
        /// Yoksa XML dosyası gönderilir, Mali Değeri Yoktur yazan bir filigranla yine XML üzerindeki 
        /// Yapı üzerinden PDF çıktısı alınıcak şekilde veri döner
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns>PDF veya ZIP olarak alınacak dosyanın Byte dizisi hali</returns>
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
        /// İlgili faturanın QNB Finans servislerine gönderilip gönderilmediği
        /// Gönderildi ise durumuna bakarak 
        /// Bu kaydın silinebilir olup olmadığını belirleye bir metottur.
        /// </summary>
        /// <param name="gidenFaturaId">Giden Fatura Id Bilgisi</param>
        /// <returns></returns>
        public static bool EArsivSilmeyeUygunMu(string gidenFaturaId)
        {
            try
            {
                var uygunMu = true;

                gibUserService = new GIBUserService.userService();
                gibEArsivService = new GIBEArsiv.EarsivWebService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEArsivService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                string inputKontrol = "{\"faturaUuid\":\"" + gidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";

                var fatura = new GIBEArsiv.belge();
                fatura.belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL;
                fatura.belgeFormatiSpecified = true;
                GIBEArsiv.earsivServiceResult serviceResult = new GIBEArsiv.earsivServiceResult();

                gibEArsivService.faturaSorgula(inputKontrol, out serviceResult);
                if (serviceResult.resultCode == "AE00000")
                    uygunMu = false;

                return uygunMu;
            }
            catch (System.Exception ex)
            {
                return true;
            }
            finally
            {
                gibUserService.logout();
            }
        }

        /// <summary>
        /// Oluşturulan XML dosyasının QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
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

                string input = "{\"islemId\":\"" + gidenFatura.GidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";
                //string inputKontrol = "{\"vkn\":\"3250566851\",\"donenBelgeFormati\":\"9\",\"faturaUuid\":\"" + gidenFatura.GidenFaturaId + "\"";
                string inputKontrol = "{\"faturaUuid\":\"" + gidenFatura.GidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";

                var belgeTemp = new GIBEArsiv.belge();
                belgeTemp.belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL;
                belgeTemp.belgeFormatiSpecified = true;
                belgeTemp.belgeIcerigi = System.IO.File.ReadAllBytes(dosyaAdi);
                GIBEArsiv.earsivServiceResult serviceResult = new GIBEArsiv.earsivServiceResult();

                gibEArsivService.faturaSorgula(inputKontrol, out serviceResult);
                if (serviceResult.resultCode != "AE00000")
                {
                    var belge = gibEArsivService.faturaOlustur(input, belgeTemp, out serviceResult);
                }
                else
                    return MesajSabitler.IslemBasarili;

                if (serviceResult.resultCode != "AE00000")
                    return MesajSabitler.IslemBasarisiz;
                else
                    return MesajSabitler.IslemBasarili;

                //var belge = gibEArsivService.faturaOlustur(input, belgeTemp, out serviceResult);

                //if (serviceResult.resultCode != "AE00000")
                //    return MesajSabitler.IslemBasarisiz;
                //else
                //    return MesajSabitler.IslemBasarili;
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
        /// Burada da E-Faturada olduğu gibi önce Fatura Id bilgisi ile gönderip
        /// Bir sonuç gelmezse hazırlanan XML dosyası gönderilip çıktı alınmaya çalışılmaktadır
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns>PDF olarak çıktısı alınacak dosyanın byte dizisi hali</returns>
        public static byte[] EArsivOnIzleme(GidenFaturaDTO gidenFatura, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEArsivService = new GIBEArsiv.EarsivWebService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEArsivService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                // Burada VKN ve ERP Kodu önemlidir
                string input = "{\"islemId\":\"" + gidenFatura.GidenFaturaId.ToUpper() + "\",\"faturaUuid\":\"" + gidenFatura.GidenFaturaId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"3\"}"; // Buradaki 3 PDF

                var belgeTemp = new GIBEArsiv.belge();
                belgeTemp.belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL;
                belgeTemp.belgeFormatiSpecified = true;
                belgeTemp.belgeIcerigi = System.IO.File.ReadAllBytes(dosyaAdi);

                GIBEArsiv.earsivServiceResult serviceResult = new GIBEArsiv.earsivServiceResult();
                var temp = gibEArsivService.faturaSorgula(input, out serviceResult);
                if (temp != null)
                    return temp.belgeIcerigi;
                else
                {
                    temp = gibEArsivService.faturaOnizleme(input, belgeTemp, out serviceResult);
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
        /// Oluşturulan XML dosyasının QNB Finans Tarafına gönderilmesini sağlayan metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns></returns>
        public static string EMustahsilGonder(MustahsilMakbuzuDTO mustahsilMakbuzu, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEMustahsilService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, "tr");

                string input = "{\"islemId\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\"}";

                var belgeTemp = new GIBEMustahsil.belge();
                belgeTemp.belgeFormati = GIBEMustahsil.belgeFormatiEnum.UBL;
                belgeTemp.belgeFormatiSpecified = true;
                belgeTemp.belgeIcerigi = System.IO.File.ReadAllBytes(dosyaAdi);
                GIBEMustahsil.earsivServiceResult serviceResult = new GIBEMustahsil.earsivServiceResult();
                var belge = gibEMustahsilService.mustahsilMakbuzOlustur(input, belgeTemp, out serviceResult);

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
        /// Burada da E-Faturada olduğu gibi önce Fatura Id bilgisi ile gönderip
        /// Bir sonuç gelmezse hazırlanan XML dosyası gönderilip çıktı alınmaya çalışılmaktadır
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="dosyaAdi">Dosya Adı bilgisi</param>
        /// <returns>PDF olarak çıktısı alınacak dosyanın byte dizisi hali</returns>
        public static byte[] EMustahsilOnIzleme(MustahsilMakbuzuDTO mustahsilMakbuzu, string dosyaAdi)
        {
            try
            {
                gibUserService = new GIBUserService.userService();
                gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

                gibUserService.CookieContainer = new System.Net.CookieContainer();
                gibEMustahsilService.CookieContainer = gibUserService.CookieContainer;

                gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, "tr");

                // Burada VKN ve ERP Kodu önemlidir
                string input = "{\"islemId\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper() + "\",\"uuid\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"3\"}"; // Buradaki 3 PDF

                var belgeTemp = new GIBEMustahsil.belge();
                belgeTemp.belgeFormati = GIBEMustahsil.belgeFormatiEnum.UBL;
                belgeTemp.belgeFormatiSpecified = true;
                belgeTemp.belgeIcerigi = System.IO.File.ReadAllBytes(dosyaAdi);

                GIBEMustahsil.earsivServiceResult serviceResult = new GIBEMustahsil.earsivServiceResult();
                var temp = gibEMustahsilService.mustahsilMakbuzSorgula(input, out serviceResult);
                if (temp != null)
                    return temp.belgeIcerigi;
                else
                {
                    temp = gibEMustahsilService.mustahsilMakbuzOnizleme(input, belgeTemp, out serviceResult);
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
        /// <param name="gelen">Girdi Bilgisi</param>
        /// <returns>Metnin veya girdinin MD5 hashlenmiş hali</returns>
        public static string GetMD5Hash(byte[] gelen)
        {
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
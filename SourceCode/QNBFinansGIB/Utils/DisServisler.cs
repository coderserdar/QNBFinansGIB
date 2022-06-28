using QNBFinansGIB.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QNBFinansGIB.GIBEFatura;

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
        private static GIBUserService.userService _gibUserService = new GIBUserService.userService();
        /// <summary>
        /// GİB E-Fatura Servis istemcisi
        /// </summary>
        private static GIBEFatura.connectorService _gibEFaturaService = new GIBEFatura.connectorService();
        /// <summary>
        /// GİB E-Arşiv Servis istemcisi
        /// </summary>
        private static GIBEArsiv.EarsivWebService _gibEArsivService = new GIBEArsiv.EarsivWebService();
        /// <summary>
        /// GİB E-Arşiv Servis istemcisi
        /// </summary>
        private static GIBEMustahsil.MustahsilWebService _gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

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
                _gibUserService = new GIBUserService.userService();
                _gibEFaturaService = new GIBEFatura.connectorService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEFaturaService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var kullaniciMi = _gibEFaturaService.efaturaKullanicisi(vergiNo);

                return kullaniciMi;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                _gibUserService.logout();
            }
        }
        
        /// <summary>
        /// Girilen tarihten sonraki E-Fatura Mükellef Listesini getiren metottur
        /// Bir Metottur
        /// </summary>
        /// <param name="islemTarihi">İşlem Tarihi Bilgisi</param>
        /// <returns>E-Fatura Kullanıcı Listesi</returns>
        public static List<string> EFaturaKullaniciListesi(DateTime islemTarihi)
        {
            try
            {
                _gibUserService = new GIBUserService.userService();
                _gibEFaturaService = new GIBEFatura.connectorService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEFaturaService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var vergiKimlikNoListesi = new List<string>();
                
                var islemTarihiString = islemTarihi.Date.ToString("yyyyMMdd") + islemTarihi.ToString("HHmmss");
                var list = _gibEFaturaService.eFaturaKayitliKullaniciListele(islemTarihiString);
                foreach (var item in list)
                {
                    vergiKimlikNoListesi.Add(item.vergiTcKimlikNo + " - " + item.unvan + " - " + item.kayitZamani);
                }

                return vergiKimlikNoListesi;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
            finally
            {
                _gibUserService.logout();
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

                _gibUserService = new GIBUserService.userService();
                _gibEFaturaService = new GIBEFatura.connectorService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEFaturaService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var parametreler = new GIBEFatura.gidenBelgeParametreleri
                {
                    vergiTcKimlikNo = "3250566851",
                    belgeTuru = "FATURA_UBL",
                    belgeNo = gidenFaturaId,
                    erpKodu = GIBERPKodu
                };
                var belgeDurumEsas = _gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                if (belgeDurumEsas.durum == 1 || belgeDurumEsas.durum == 3)
                    uygunMu = false;

                return uygunMu;
            }
            catch (Exception ex)
            {
                return true;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEFaturaService = new GIBEFatura.connectorService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEFaturaService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var parametreler = new GIBEFatura.gidenBelgeleriListeleParametreleri
                {
                    vkn = "3250566851",
                    belgeTuru = "FATURA_UBL",
                    baslangicGonderimTarihi = baslangicTarihi?.ToString("yyyyMMdd"),
                    bitisGonderimTarihi = bitisTarihi?.ToString("yyyyMMdd")
                };
                var sonucMesaji = "İşlem Başarılı";
                var liste = _gibEFaturaService.gidenBelgeleriListele(parametreler);
                foreach (var item in liste)
                {
                    if (item is gidenBelgeleriListeleData nesne && nesne.yerelBelgeNo == gidenFaturaId)
                        return nesne.belgeOid;
                }

                return sonucMesaji;
            }
            catch (Exception ex)
            {
                return MesajSabitler.IslemBasarisiz;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEFaturaService = new GIBEFatura.connectorService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEFaturaService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var parametreler = new GIBEFatura.gidenBelgeParametreleri
                {
                    vergiTcKimlikNo = "3250566851",
                    belgeTuru = "FATURA_UBL",
                    belgeNo = gidenFatura.GidenFaturaId,
                    belgeVersiyon = "1.0",
                    veri = File.ReadAllBytes(dosyaAdi)
                };
                parametreler.belgeHash = GetMD5Hash(parametreler.veri);
                parametreler.mimeType = "application/xml";
                parametreler.erpKodu = GIBERPKodu;

                var sonucMesaji = string.Empty;

                try
                {
                    var belgeDurumEsas = _gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                    if (belgeDurumEsas.durum != 1 && belgeDurumEsas.durum != 3)
                    {
                        #region Belge Oid Durumuna Göre Evrak Gönderme
                        if (!string.IsNullOrEmpty(belgeOid))
                        {
                            if (belgeOid != "Tekrar Gönder")
                                sonucMesaji = _gibEFaturaService.belgeGonderExt(parametreler);
                            else
                            {
                                var yenidenGonderDurum = false;
                                //var ettnDizi = new string[1];
                                //ettnDizi[0] = parametreler.belgeNo;
                                //while (yenidenGonderDurum == false)
                                //{
                                //    yenidenGonderDurum = _gibEFaturaService.belgeleriTekrarGonder(parametreler.vergiTcKimlikNo, ettnDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                                //}
                                var belgeOidDizi = new string[1];
                                belgeOidDizi[0] = belgeOid;
                                while (yenidenGonderDurum == false)
                                {
                                    yenidenGonderDurum = _gibEFaturaService.belgeleriTekrarGonderBelgeOid(parametreler.vergiTcKimlikNo, belgeOidDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                                }
                                sonucMesaji = belgeOid;
                            }
                        }
                        else
                            sonucMesaji = _gibEFaturaService.belgeGonderExt(parametreler);
                        #endregion

                        #region Belge Durumu Kontrolü

                        if (sonucMesaji.Length > 20) return sonucMesaji;
                        // önemli not: buradaki metotta Belge Oid'ye göre kontrol yapılıyor
                        //var belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                        //var durum = belgeDurum.durum;
                        //while (durum == 1)
                        //{
                        //    belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                        //    durum = belgeDurum.durum;
                        //}
                        var belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                        var durum = belgeDurum.durum;
                        while (durum == 1)
                        {
                            belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                            durum = belgeDurum.durum;
                        }
                        switch (durum)
                        {
                            case 2:
                                sonucMesaji = MesajSabitler.IslemBasarisiz;
                                break;
                            case 3:
                            {
                                var gonderimDurumu = belgeDurum.gonderimDurumu;
                                switch (gonderimDurumu)
                                {
                                    case 2:
                                        return sonucMesaji;
                                    case 4:
                                        return sonucMesaji;
                                    case 3:
                                    {
                                        var gibYanitKodu = belgeDurum.gonderimCevabiKodu;
                                        if (gibYanitKodu > 1300)
                                            return sonucMesaji;
                                        else if (Sabitler.TekrarGonderilebilecekKodListesi.Any(j => j == gibYanitKodu))
                                            return "Tekrar Gönder";
                                        else if (Sabitler.TekrarGonderilebilecekKodListesi.All(j => j != gibYanitKodu) && gibYanitKodu <= 1200 && gibYanitKodu >= 1100)
                                            return MesajSabitler.IslemBasarisiz;
                                        else if (gibYanitKodu == 1210 || gibYanitKodu == 1120)
                                            return MesajSabitler.IslemBasarisiz;
                                        else
                                            return sonucMesaji;
                                    }
                                }

                                break;
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
                            sonucMesaji = _gibEFaturaService.belgeGonderExt(parametreler);
                        else
                        {
                            var yenidenGonderDurum = false;
                            //var ettnDizi = new string[1];
                            //ettnDizi[0] = parametreler.belgeNo;
                            //while (yenidenGonderDurum == false)
                            //{
                            //    yenidenGonderDurum = _gibEFaturaService.belgeleriTekrarGonder(parametreler.vergiTcKimlikNo, ettnDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                            //}
                            var belgeOidDizi = new string[1];
                            belgeOidDizi[0] = belgeOid;
                            while (yenidenGonderDurum == false)
                            {
                                yenidenGonderDurum = _gibEFaturaService.belgeleriTekrarGonderBelgeOid(parametreler.vergiTcKimlikNo, belgeOidDizi, parametreler.belgeTuru, parametreler.alanEtiket, parametreler.gonderenEtiket);
                            }
                            sonucMesaji = belgeOid;
                        }
                    }
                    else
                        sonucMesaji = _gibEFaturaService.belgeGonderExt(parametreler);
                    #endregion

                    #region Belge Durumu Kontrolü

                    if (sonucMesaji.Length > 20) return sonucMesaji;
                    // önemli not: buradaki metotta Belge Oid'ye göre kontrol yapılıyor
                    //var belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                    //var durum = belgeDurum.durum;
                    //while (durum == 1)
                    //{
                    //    belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgula(parametreler.vergiTcKimlikNo, sonucMesaji);
                    //    durum = belgeDurum.durum;
                    //}
                    var belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                    var durum = belgeDurum.durum;
                    while (durum == 1)
                    {
                        belgeDurum = _gibEFaturaService.gidenBelgeDurumSorgulaYerelBelgeNo(parametreler.vergiTcKimlikNo, parametreler.belgeNo);
                        durum = belgeDurum.durum;
                    }
                    switch (durum)
                    {
                        case 2:
                            sonucMesaji = MesajSabitler.IslemBasarisiz;
                            break;
                        case 3:
                        {
                            var gonderimDurumu = belgeDurum.gonderimDurumu;
                            switch (gonderimDurumu)
                            {
                                case 2:
                                    return sonucMesaji;
                                case 4:
                                    return sonucMesaji;
                                case 3:
                                {
                                    var gibYanitKodu = belgeDurum.gonderimCevabiKodu;
                                    if (gibYanitKodu > 1300)
                                        return sonucMesaji;
                                    else if (Sabitler.TekrarGonderilebilecekKodListesi.Any(j => j == gibYanitKodu))
                                        return "Tekrar Gönder";
                                    else if (Sabitler.TekrarGonderilebilecekKodListesi.All(j => j != gibYanitKodu) && gibYanitKodu <= 1200 && gibYanitKodu >= 1100)
                                        return MesajSabitler.IslemBasarisiz;
                                    else if (gibYanitKodu == 1210 || gibYanitKodu == 1120)
                                        return MesajSabitler.IslemBasarisiz;
                                    else
                                        return sonucMesaji;
                                }
                            }

                            break;
                        }
                    }
                    #endregion

                    return sonucMesaji;
                }
            }
            catch (Exception ex)
            {
                return MesajSabitler.IslemBasarisiz;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEFaturaService = new GIBEFatura.connectorService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEFaturaService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                const string vergiTcKimlikNo = "3250566851";
                if (!File.Exists(dosyaAdi))
                {
                    return null;
                }
                var veri = File.ReadAllBytes(dosyaAdi);
                const string belgeFormati = "PDF";  //HTML, PDF, UBL
                const string belgeTuru = "FATURA"; //EFATURA ve EIRSALIYE değerleri alabilir.

                var onizleme = new byte[1];

                var geriDonus = new GeriDonus
                {
                    Dosya = onizleme,
                    Tip = 0
                };

                if (!string.IsNullOrEmpty(gidenFatura.BelgeOid))
                {
                    var idListesi = new string[1];
                    idListesi[0] = gidenFatura.BelgeOid;
                    onizleme = _gibEFaturaService.gidenBelgeleriIndir(vergiTcKimlikNo, idListesi, belgeTuru, belgeFormati);
                    geriDonus.Dosya = onizleme;
                    geriDonus.Tip = 1;
                }
                else
                {
                    onizleme = _gibEFaturaService.ublOnizleme(vergiTcKimlikNo, veri, belgeFormati, belgeTuru);
                    geriDonus.Dosya = onizleme;
                    geriDonus.Tip = 0;
                }

                return geriDonus;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _gibUserService.logout();
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

                _gibUserService = new GIBUserService.userService();
                _gibEArsivService = new GIBEArsiv.EarsivWebService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEArsivService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var inputKontrol = "{\"faturaUuid\":\"" + gidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";

                var fatura = new GIBEArsiv.belge
                {
                    belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL,
                    belgeFormatiSpecified = true
                };
                var serviceResult = new GIBEArsiv.earsivServiceResult();
                _gibEArsivService.faturaSorgula(inputKontrol, out serviceResult);
                if (serviceResult.resultCode == "AE00000")
                    uygunMu = false;

                return uygunMu;
            }
            catch (Exception ex)
            {
                return true;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEArsivService = new GIBEArsiv.EarsivWebService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEArsivService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                var input = "{\"islemId\":\"" + gidenFatura.GidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";
                //string inputKontrol = "{\"vkn\":\"3250566851\",\"donenBelgeFormati\":\"9\",\"faturaUuid\":\"" + gidenFatura.GidenFaturaId + "\"";
                var inputKontrol = "{\"faturaUuid\":\"" + gidenFatura.GidenFaturaId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"9\"}";

                var belgeTemp = new GIBEArsiv.belge
                {
                    belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL,
                    belgeFormatiSpecified = true,
                    belgeIcerigi = File.ReadAllBytes(dosyaAdi)
                };
                var serviceResult = new GIBEArsiv.earsivServiceResult();
                _gibEArsivService.faturaSorgula(inputKontrol, out serviceResult);
                if (serviceResult.resultCode != "AE00000")
                {
                    var belge = _gibEArsivService.faturaOlustur(input, belgeTemp, out serviceResult);
                }
                else
                    return MesajSabitler.IslemBasarili;

                return serviceResult.resultCode != "AE00000" ? MesajSabitler.IslemBasarisiz : MesajSabitler.IslemBasarili;

                //var belge = _gibEArsivService.faturaOlustur(input, belgeTemp, out serviceResult);

                //if (serviceResult.resultCode != "AE00000")
                //    return MesajSabitler.IslemBasarisiz;
                //else
                //    return MesajSabitler.IslemBasarili;
            }
            catch (Exception ex)
            {
                return MesajSabitler.IslemBasarisiz;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEArsivService = new GIBEArsiv.EarsivWebService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEArsivService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, GIBVKN);

                // Burada VKN ve ERP Kodu önemlidir
                var input = "{\"islemId\":\"" + gidenFatura.GidenFaturaId.ToUpper() + "\",\"faturaUuid\":\"" + gidenFatura.GidenFaturaId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"3\"}"; // Buradaki 3 PDF

                var belgeTemp = new GIBEArsiv.belge
                {
                    belgeFormati = GIBEArsiv.belgeFormatiEnum.UBL,
                    belgeFormatiSpecified = true,
                    belgeIcerigi = File.ReadAllBytes(dosyaAdi)
                };
                var serviceResult = new GIBEArsiv.earsivServiceResult();
                var temp = _gibEArsivService.faturaSorgula(input, out serviceResult);
                if (temp != null)
                    return temp.belgeIcerigi;
                else
                {
                    temp = _gibEArsivService.faturaOnizleme(input, belgeTemp, out serviceResult);
                    return temp?.belgeIcerigi;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _gibUserService.logout();
            }
        }

        /// <summary>
        /// İlgili faturanın QNB Finans servislerine gönderilip gönderilmediği
        /// Gönderildi ise durumuna bakarak 
        /// Bu kaydın silinebilir olup olmadığını belirleye bir metottur.
        /// </summary>
        /// <param name="mustahsilMakbuzuId">Müstahsil Makbuzu Id Bilgisi</param>
        /// <returns>Kaydın Silinmeye Uygun Olup Olmadığı Bilgisi</returns>
        public static bool EMustahsilSilmeyeUygunMu(string mustahsilMakbuzuId)
        {
            try
            {
                var uygunMu = true;

                _gibUserService = new GIBUserService.userService();
                _gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEMustahsilService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, "tr");

                var inputKontrol = "{\"islemId\":\"" + mustahsilMakbuzuId.ToUpper() + "\",\"uuid\":\"" + mustahsilMakbuzuId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\"}";

                var serviceResult = new GIBEMustahsil.earsivServiceResult();

                _gibEMustahsilService.mustahsilMakbuzSorgula(inputKontrol, out serviceResult);
                if (serviceResult.resultCode == "AE00000")
                    uygunMu = false;

                return uygunMu;
            }
            catch (Exception ex)
            {
                return true;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEMustahsilService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, "tr");

                var input = "{\"islemId\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\"}";
                var inputKontrol = "{\"islemId\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper() + "\",\"uuid\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\"}";

                var belgeTemp = new GIBEMustahsil.belge
                {
                    belgeFormati = GIBEMustahsil.belgeFormatiEnum.UBL,
                    belgeFormatiSpecified = true,
                    belgeIcerigi = File.ReadAllBytes(dosyaAdi)
                };
                var serviceResult = new GIBEMustahsil.earsivServiceResult();
                _gibEMustahsilService.mustahsilMakbuzSorgula(inputKontrol, out serviceResult);
                if (serviceResult.resultCode != "AE00000")
                {
                    var belge = _gibEMustahsilService.mustahsilMakbuzOlustur(input, belgeTemp, out serviceResult);
                }
                else
                    return MesajSabitler.IslemBasarili;

                return serviceResult.resultCode != "AE00000" ? MesajSabitler.IslemBasarisiz : MesajSabitler.IslemBasarili;
            }
            catch (Exception ex)
            {
                return MesajSabitler.IslemBasarisiz;
            }
            finally
            {
                _gibUserService.logout();
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
                _gibUserService = new GIBUserService.userService();
                _gibEMustahsilService = new GIBEMustahsil.MustahsilWebService();

                _gibUserService.CookieContainer = new System.Net.CookieContainer();
                _gibEMustahsilService.CookieContainer = _gibUserService.CookieContainer;

                _gibUserService.wsLogin(GIBKullaniciAdi, GIBSifre, "tr");

                // Burada VKN ve ERP Kodu önemlidir
                var input = "{\"islemId\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper() + "\",\"uuid\":\"" + mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper() + "\",\"vkn\":\"3250566851\",\"sube\":\"DFLT\",\"kasa\":\"DFLT\",\"erpKodu\":\"TSF30125\",\"donenBelgeFormati\":\"3\"}"; // Buradaki 3 PDF

                var belgeTemp = new GIBEMustahsil.belge
                {
                    belgeFormati = GIBEMustahsil.belgeFormatiEnum.UBL,
                    belgeFormatiSpecified = true,
                    belgeIcerigi = File.ReadAllBytes(dosyaAdi)
                };
                var serviceResult = new GIBEMustahsil.earsivServiceResult();
                var temp = _gibEMustahsilService.mustahsilMakbuzSorgula(input, out serviceResult);
                if (temp != null)
                    return temp.belgeIcerigi;
                else
                {
                    temp = _gibEMustahsilService.mustahsilMakbuzOnizleme(input, belgeTemp, out serviceResult);
                    return temp?.belgeIcerigi;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _gibUserService.logout();
            }
        }

        /// <summary>
        /// Dosyanın MD5 hashi alınması için yazılan metottur
        /// </summary>
        /// <param name="gelen">Girdi Bilgisi</param>
        /// <returns>Metnin veya girdinin MD5 hashlenmiş hali</returns>
        private static string GetMD5Hash(byte[] gelen)
        {
            var md5Hash = new MD5CryptoServiceProvider();
            var data = md5Hash.ComputeHash(gelen);
            var sBuilder = new StringBuilder();

            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }

        #endregion QNB GİB
    }
}
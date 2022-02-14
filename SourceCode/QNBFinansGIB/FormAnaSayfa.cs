using QNBFinansGIB.DTO;
using QNBFinansGIB.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QNBFinansGIB
{
    /// <summary>
    /// QNB Finans GİB Servisi için veri hazırlanan ve servisin test edildiği metotlar içeren
    /// Form sınıfıdır
    /// </summary>
    public partial class frmAnaSayfa : Form
    {
        /// <summary>
        /// Giden Fatura Listesi
        /// </summary>
        public List<GidenFaturaDTO> gidenFaturaListesi = new List<GidenFaturaDTO>();
        /// <summary>
        /// Giden Fatura Detay Listesi
        /// </summary>
        public List<GidenFaturaDetayDTO> gidenFaturaDetayListesi = new List<GidenFaturaDetayDTO>();

        public frmAnaSayfa()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Burada Form görüntülenirken
        /// Kendi hazırladığımız Giden Fatura ve Detaylarının oluşturup listeye atıyoruz ki
        /// Buradan sonra butonlara tıklandığında
        /// Bu listeler üzerinden kolayca işlem yapılabilsin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAnaSayfa_Shown(object sender, EventArgs e)
        {
            #region İlk Verilerin Hazırlanması
            #region Giden Faturaları Ekleme
            var gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "1",
                AltBirimAd = "A Birimi",
                TuzelKisiAd = "Deneme Şirketi - 1",
                VergiNo = "53602329864",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay Gima'nın Önü",
                FaksNo = "03121111111",
                BankaAd = "Türkiye İş Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR130006297372542652383131",
                DuzenlemeTarihi = new DateTime(2022, 1, 13),
                IlAd = "Ankara",
                IlceAd = "Çankaya",
                SatisTuruKod = 1,
                GibNumarasi = "MLT2022000010998",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "2",
                AltBirimAd = "B Birimi",
                TuzelKisiAd = "Deneme Şirketi - 2",
                VergiNo = "9250936109",
                VergiDairesi = "ANKARA",
                Adres = "Kızılay YKM",
                FaksNo = "03121111112",
                BankaAd = "Vakıflar Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR530006291586775935345942",
                DuzenlemeTarihi = new DateTime(2022, 1, 1),
                IlAd = "Ankara",
                IlceAd = "Etimesgut",
                SatisTuruKod = 2,
                GibNumarasi = "MLT2022000099999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "3",
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "5240018140",
                VergiDairesi = "ANKARA",
                Adres = "Zafer Çarşısı",
                FaksNo = "03121111113",
                BankaAd = "Vakıf Katılım Bankası",
                BankaSube = "Kızılay",
                IbanNo = "TR830006214925976839299642",
                DuzenlemeTarihi = new DateTime(2022, 1, 11),
                IlAd = "Ankara",
                IlceAd = "Etimesgut",
                SatisTuruKod = 2,
                FaturaTuruKod = 5, // İade Faturası
                GibNumarasi = "KST2022000009999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "4",
                TuzelKisiAd = "Deneme Şirketi - 4",
                VergiNo = "6940116151",
                VergiDairesi = "ANKARA",
                Adres = "Göksu Parkı",
                FaksNo = "03121111114",
                BankaAd = "Kuveyt Tür",
                BankaSube = "Kızılay",
                IbanNo = "TR350006267645192879857714",
                DuzenlemeTarihi = new DateTime(2022, 1, 31),
                IlAd = "Ankara",
                IlceAd = "Çubuk",
                SatisTuruKod = 10, //İhraç Kayıtlı Fatura
                GibNumarasi = "ILG202200009999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "5",
                TuzelKisiAd = "Deneme Şirketi - 3",
                VergiNo = "20101516422",
                VergiDairesi = "ANKARA",
                Adres = "Dost'un Önü",
                FaksNo = "03121111113",
                BankaAd = "Enpara.Com Bankadan Güzeli",
                BankaSube = "Karakusunlar Şube",
                IbanNo = "TR630006295412435711171488",
                DuzenlemeTarihi = new DateTime(2022, 2, 1),
                IlAd = "Ankara",
                IlceAd = "Pursaklar",
                SatisTuruKod = 2,
                GibNumarasi = "SUS2022000999999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "6",
                GercekKisiTcKimlikNo = "52270709114",
                GercekKisiAd = "Adnan",
                GercekKisiSoyad = "Şenses",
                CepTelefonNo = "05555555555",
                EPostaAdresi = "adnansenses@mynet.com",
                IkametgahAdresi = "Dost'un Önü",
                BankaAd = "Halkbank",
                BankaSube = "Etimesgut Şube",
                IbanNo = "TR540006234114125194267726",
                DuzenlemeTarihi = new DateTime(2022, 2, 2),
                IlAd = "Konya",
                IlceAd = "Beyşehir",
                SatisTuruKod = 10,
                GibNumarasi = "YZG2022000999999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);

            gidenFatura = new GidenFaturaDTO
            {
                GidenFaturaId = "7",
                GercekKisiTcKimlikNo = "20077290692",
                GercekKisiAd = "Mehmet Emin",
                GercekKisiSoyad = "Akdeniz",
                CepTelefonNo = "05555555554",
                EPostaAdresi = "memo@yahoo.com",
                IkametgahAdresi = "Fışkiyenin orası",
                BankaAd = "Bank Mellat",
                BankaSube = "Yenimahalle Şube",
                IbanNo = "TR410006276748779926367358",
                DuzenlemeTarihi = new DateTime(2022, 1, 25),
                IlAd = "Ağrı",
                IlceAd = "Patnos",
                SatisTuruKod = 11,
                GibNumarasi = "AGR2022000999999",
                KdvHaricTutar = 0,
                KdvTutari = 0,
                FaturaTutari = 0,
            };
            gidenFaturaListesi.Add(gidenFatura);
            #endregion

            // burada fatura detayları kabarık olsun diye tüm fatura ana nesnelerine eklenecek bir yapı kuruldu
            // ancak arada bir ilişki istenirse
            // DTO sınıfında GidenFaturaDetayDTO sınıfına bir GidenFaturaId alanı eklenerek
            // Ve üstteki regionda faturalara elle id verildikten sonra
            // fatura detayları eklenirken buradaki ana nesne id alanları girilebilir
            #region Giden Fatura Detayları Ekleme

            var gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "7",
                BirimFiyat = (decimal)0.1234,
                Miktar = 1000,
                KdvOran = 0,
                IskontoOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = 0,
                GibKisaltma = "KGM",
                IrsaliyeNo = "1",
                MalzemeFaturaAciklamasi = "Ofis demirbaşı için alındı",
                SevkIrsaliyeTarihi = new DateTime(2022, 1, 1),
                SevkIrsaliyesiNo = "34",
                PlakaNo = "06GUC777",
                FaturaUrunTuru = "Dizüstü Bilgisayar",
                Tonaj = 1,
                YuklemeFormuNo = 1234
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "7",
                BirimFiyat = (decimal)0.1234,
                Miktar = 1000,
                KdvOran = 1,
                IskontoOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = (decimal)12.34,
                GibKisaltma = "MTR",
                PlakaNo = "06GC7447",
                FaturaUrunTuru = "Çubuk Mikrofon",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "2",
                BirimFiyat = (decimal)0.1234,
                Miktar = 1000,
                KdvOran = 1,
                IskontoOran = 0,
                KdvHaricTutar = 1234,
                KdvTutari = (decimal)12.34,
                GibKisaltma = "KWT",
                PlakaNo = "06GC7447",
                FaturaUrunTuru = "Çubuk Mikrofon",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "3",
                BirimFiyat = (decimal)0.124,
                Miktar = 5000,
                KdvOran = 8,
                IskontoOran = 0,
                KdvHaricTutar = 6020,
                KdvTutari = (decimal)481.6,
                GibKisaltma = "MON",
                PlakaNo = "06J1234",
                FaturaUrunTuru = "Mouse",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            gidenFaturaDetay = new GidenFaturaDetayDTO
            {
                GidenFaturaId = "1",
                BirimFiyat = 500,
                Miktar = 120,
                KdvOran = 18,
                IskontoOran = 0,
                KdvHaricTutar = 60000,
                KdvTutari = 12800,
                GibKisaltma = "LTR",
                PlakaNo = "06C99876",
                FaturaUrunTuru = "Ayakkabı",
            };
            gidenFaturaDetayListesi.Add(gidenFaturaDetay);

            #endregion

            #region Giden Fatura Toplamları Hesaplama ve Olmayanlara Ekleme

            foreach (var item in gidenFaturaListesi)
            {
                if (gidenFaturaDetayListesi.Any(j => j.GidenFaturaId == item.GidenFaturaId))
                {
                    var gidenFaturaDetayListesiTemp = gidenFaturaDetayListesi.Where(j => j.GidenFaturaId == item.GidenFaturaId).ToList();
                    item.KdvHaricTutar = gidenFaturaDetayListesiTemp.Sum(j => j.KdvHaricTutar);
                    item.KdvTutari = gidenFaturaDetayListesiTemp.Sum(j => j.KdvTutari);
                    item.FaturaTutari = item.KdvHaricTutar + item.KdvTutari;
                }
                else
                {
                    gidenFaturaDetay = new GidenFaturaDetayDTO
                    {
                        GidenFaturaId = item.GidenFaturaId,
                        BirimFiyat = (decimal)1.5,
                        Miktar = 1000,
                        KdvOran = 1,
                        IskontoOran = 0,
                        KdvHaricTutar = 1500,
                        KdvTutari = 15,
                        GibKisaltma = "MTR",
                        FaturaUrunTuru = "Deneme " + item.GidenFaturaId,
                    };
                    item.KdvHaricTutar = gidenFaturaDetay.KdvHaricTutar;
                    item.KdvTutari = gidenFaturaDetay.KdvTutari;
                    item.FaturaTutari = item.KdvHaricTutar + item.KdvTutari;
                    gidenFaturaDetayListesi.Add(gidenFaturaDetay);
                }
            }

            #endregion
            #endregion
        }

        /// <summary>
        /// Form kapandığı zaman ne yapılacağını belirten bir metottur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAnaSayfa_FormClosed(object sender, FormClosedEventArgs e)
        {
            MessageBox.Show("Programı kullandığınız için teşekkürler");
            Application.Exit();
        }

        /// <summary>
        /// Sistemde E-Fatura ve E-Arşiv Servislerine Gönderilecek Formatta
        /// İdeal XML dosyalarının oluşturulması için XML Oluştur Butonuna tıklandığı zaman
        /// Yapılacak işlemlerin hazırlandığı metottur.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXmlOlustur_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                dialogKlasorSecimi.SelectedPath = Application.StartupPath;
                DialogResult result = dialogKlasorSecimi.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialogKlasorSecimi.SelectedPath))
                {
                    var klasorAdi = dialogKlasorSecimi.SelectedPath;
                    // Burada form yüklendiği zaman hazırlanan giden faturalardan rastgele birinin
                    // ve ona ait detay kayıtlarının seçilmesi sağlanıyor
                    var index = new Random().Next(gidenFaturaListesi.Count);
                    var gidenFatura = gidenFaturaListesi[index];
                    var gidenFaturaDetayListesiTemp = new List<GidenFaturaDetayDTO>();
                    if (gidenFaturaDetayListesi.Any(j => j.GidenFaturaId == gidenFatura.GidenFaturaId))
                        gidenFaturaDetayListesiTemp = gidenFaturaDetayListesi.Where(j => j.GidenFaturaId == gidenFatura.GidenFaturaId).ToList();

                    #region XML Oluşturma
                    var dosyaAdi = "";
                    if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
                    {
                        var kullaniciMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
                        if (kullaniciMi)
                        {
                            dosyaAdi = YardimciSiniflar.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi);
                        }
                        else
                        {
                            dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, true);
                        }
                    }
                    else
                    {
                        dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, false);
                    }

                    MessageBox.Show(dosyaAdi + " adresinde gerekli XML dosyası oluşturulmuştur.");

                    #endregion
                }
            }
        }

        /// <summary>
        /// Sistemde E-Fatura veya E-Arşive yollansın yollanmasın
        /// Hazırlanan XML dosyalarının bu sistemlerde nasıl göründüğüne dair
        /// Önizleme alınmasını sağlayan metottur. Bu önizleme PDF veya ZIP olarak alınacak
        /// Sorun olsa da olmasa da bir mesaj ile kullanıcı bilgilendirilecektir.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGIBOnizleme_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                dialogKlasorSecimi.SelectedPath = Application.StartupPath;
                DialogResult result = dialogKlasorSecimi.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialogKlasorSecimi.SelectedPath))
                {
                    var klasorAdi = dialogKlasorSecimi.SelectedPath;
                    // Burada form yüklendiği zaman hazırlanan giden faturalardan rastgele birinin
                    // ve ona ait detay kayıtlarının seçilmesi sağlanıyor
                    var index = new Random().Next(gidenFaturaListesi.Count);
                    var gidenFatura = gidenFaturaListesi[index];
                    var gidenFaturaDetayListesiTemp = new List<GidenFaturaDetayDTO>();
                    if (gidenFaturaDetayListesi.Any(j => j.GidenFaturaId == gidenFatura.GidenFaturaId))
                        gidenFaturaDetayListesiTemp = gidenFaturaDetayListesi.Where(j => j.GidenFaturaId == gidenFatura.GidenFaturaId).ToList();

                    #region XML Oluşturma ve Servis Önizlemesi
                    var dosyaAdi = "";
                    var geriDonus = new GeriDonus();
                    geriDonus.Tip = 0;
                    var dosya = new byte[1];
                    var kullaniciMi = false;
                    if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
                    {
                        kullaniciMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
                        if (kullaniciMi)
                        {
                            dosyaAdi = YardimciSiniflar.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi);
                            geriDonus = DisServisler.EFaturaOnIzleme(gidenFatura, dosyaAdi);
                            if (geriDonus != null)
                                dosya = geriDonus.Dosya;
                        }
                        else
                        {
                            dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, true);
                            dosya = DisServisler.EArsivOnIzleme(gidenFatura, dosyaAdi);
                        }
                    }
                    else
                    {
                        dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, false);
                        dosya = DisServisler.EArsivOnIzleme(gidenFatura, dosyaAdi);
                    }

                    if (dosya != null && dosya.Length > 1)
                    {
                        var dosyaAdiTemp = dosyaAdi.Replace("xml", "pdf");
                        if (kullaniciMi && !string.IsNullOrEmpty(gidenFatura.BelgeOid))
                            dosyaAdiTemp = dosyaAdi.Replace("xml", "zip");
                        if (geriDonus != null && geriDonus.Tip == 1)
                            dosyaAdiTemp = dosyaAdi.Replace("xml", "zip");

                        File.WriteAllBytes(dosyaAdiTemp, dosya);

                        MessageBox.Show(dosyaAdiTemp + " adresinde gerekli PDF veya ZIP dosyası oluşturulmuştur.");
                    }
                    else
                    {
                        MessageBox.Show("Söz konusu faturanın önizlemesi oluşturulamamıştır");
                    }

                    #endregion
                }
            }
        }

        /// <summary>
        /// Sistem tarafından hazırlanan XML dosyasının
        /// E-Fatura veya E-Arşiv servisine gönderilmesi için hazırlanan metottur.
        /// Burada işlemin başarılı olup olmamasına göre kullanıcı bilgilendirilecektir.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnServiseGonder_Click(object sender, EventArgs e)
        {
            using (var dialogKlasorSecimi = new FolderBrowserDialog())
            {
                dialogKlasorSecimi.SelectedPath = Application.StartupPath;
                DialogResult result = dialogKlasorSecimi.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialogKlasorSecimi.SelectedPath))
                {
                    var klasorAdi = dialogKlasorSecimi.SelectedPath;
                    // Burada form yüklendiği zaman hazırlanan giden faturalardan rastgele birinin
                    // ve ona ait detay kayıtlarının seçilmesi sağlanıyor
                    var index = new Random().Next(gidenFaturaListesi.Count);
                    var gidenFatura = gidenFaturaListesi[index];
                    var gidenFaturaDetayListesiTemp = new List<GidenFaturaDetayDTO>();
                    if (gidenFaturaDetayListesi.Any(j => j.GidenFaturaId == gidenFatura.GidenFaturaId))
                        gidenFaturaDetayListesiTemp = gidenFaturaDetayListesi.Where(j => j.GidenFaturaId == gidenFatura.GidenFaturaId).ToList();

                    #region XML Oluşturma ve Servise Gönderme
                    var dosyaAdi = "";
                    var sonuc = "";
                    var kullaniciMi = false;
                    if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
                    {
                        kullaniciMi = DisServisler.EFaturaKullanicisiMi(gidenFatura.VergiNo);
                        if (kullaniciMi)
                        {
                            dosyaAdi = YardimciSiniflar.EFaturaXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi);
                            sonuc = DisServisler.EFaturaGonder(gidenFatura, dosyaAdi);
                            if (sonuc != MesajSabitler.IslemBasarisiz)
                            {
                                if (sonuc.Length <= 20)
                                    gidenFatura.BelgeOid = sonuc;
                            }
                        }
                        else
                        {
                            dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, true);
                            sonuc = DisServisler.EArsivGonder(gidenFatura, dosyaAdi);
                        }
                    }
                    else
                    {
                        dosyaAdi = YardimciSiniflar.EArsivXMLOlustur(gidenFatura, gidenFaturaDetayListesiTemp, klasorAdi, false);
                        sonuc = DisServisler.EArsivGonder(gidenFatura, dosyaAdi);
                    }

                    if (sonuc == MesajSabitler.IslemBasarisiz)
                    {
                        MessageBox.Show(dosyaAdi + " dosyasının servise gönderilmesinde bir sorun yaşanmıştır.");
                    }
                    else
                    {
                        MessageBox.Show(dosyaAdi + " dosyası başarıyla GİB servislerine gönderilmiştir.");
                    }

                    #endregion
                }
            }
        }
    }
}
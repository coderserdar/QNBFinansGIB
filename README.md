# QNB Finans GİB (Gelir İdaresi Başkanlığı) E-Fatura, E-Arşiv ve E-Müstahsil Web Servis Uygulaması

![GitHub stars](https://img.shields.io/github/stars/coderserdar/QNBFinansGIB?style=social) ![GitHub forks](https://img.shields.io/github/forks/coderserdar/QNBFinansGIB?style=social) ![GitHub watchers](https://img.shields.io/github/watchers/coderserdar/QNBFinansGIB?style=social) ![GitHub repo size](https://img.shields.io/github/repo-size/coderserdar/QNBFinansGIB?style=plastic) ![GitHub language count](https://img.shields.io/github/languages/count/coderserdar/QNBFinansGIB?style=plastic) ![GitHub top language](https://img.shields.io/github/languages/top/coderserdar/QNBFinansGIB?style=plastic) ![GitHub last commit](https://img.shields.io/github/last-commit/coderserdar/QNBFinansGIB?color=red&style=plastic)

Bu bir **Windows Form** uygulamasıdır ve içerisinde genel olarak **QNB Finans** firması üzerinden **GİB (Gelir İdaresi Başkanlığı)**'e ait olan *E-Fatura*, *E-Arşiv* ve *E-Müstahsil* web servislerine göndermek üzere kesilen faturaların ve müstahsil makbuzlarının **XML** dosyalarını oluşturma, servis üzerinde PDF veya ZIP formatlarında önizlemesini yapma, hazırlanan XML dosyalarını GİB tarafına gönderme gibi işlemlerin yapılmasını sağlamaktadır. Örnek olarak yine tarafımca yazılan **Türkiye Şeker Fabrikaları A.Ş.** bünyesindeki yapı kullanılmıştır. Olabildiğince açıklama satırları yazılarak hazırlanmış bir uygulamadır. **C#** programlama dili ile **.NET 4** framework üzerinde **Visual Studio 2017 IDE**si kullanılarak hazırlanmıştır.

Bu uygulamada aşağıdaki işlemler gerçekleştirilebilmektedir.

 - **Gelir İdare Başkanlığı** *E-Fatura* ve *E-Arşiv* web servislerine kesilen fatura ile ilgili bilgilerin doğru bir şekilde gönderilebilmesi için **XML (UBL)** dosyaları oluşturabilme
    + Daha önceden bu servis üzerinden gönderilen faturaların çıktısının ideal olarak alınabilmesi için **Belge Oid** denilen alanlarının temin edilmesi
 - **Gelir İdare Başkanlığı** *E-Müstahsil* web servisine müstahsil makbuzu ile ilgili bilgilerin doğru bir şekilde gönderilebilmesi için **XML (UBL)** dosyaları oluşturabilme
 - Eğer daha önceden gönderildiyse gönderilen faturanın veya müstahsil makbuzunun, gönderilmediyse hazırlanan **XML (UBL)** dosyasının **GİB** portalında nasıl görüneceğine dair **PDF** veya **ZIP** formatında *önizleme* yapabilme 
 - Hazırlanan **XML (UBL)** dosyasının **GİB** *E-Fatura* ve *E-Arşiv* servisine (fatura kesilen tüzel kişinin E-Fatura mükellefi olup olmamasına bağlı olarak) veya *E-Müstahsil* servisine gönderilmesi
 - Fatura veya Makbuzları silmeye çalışırken, daha önce başarılı bir şekilde **GİB** servislerine gönderilip gönderilmediğinin kontrol edilmesi
 - Belli bir tarihten sonra E-Fatura Mükellefi olan kullanıcıların listesi (*ListBox* içerisinde gösteriliyor)

Bu uygulamada bazı dikkat edilmesi gereken önemli noktalar bulunmaktadır. Bunlar aşağıda belirtilmiştir;

 - Öncelikle web servis adresleri Service Reference yerine Web Reference olarak eklenmelidir.
    - *Diğer türlü çalışması mümkün değildir (Ya da en azından ben beceremedim)*
 - **Giden Fatura** ana nesnesinde bulunan *GIB Numarası* alanı XML ve matbu çıktı için önem taşımaktadır.
    - *Formatı faturayı gönderen kurumun kısaltması 3 harf, faturanın yılı 4 karakter ve geri kalan 9 hane de faturanın sıra numarası vb. olaack şekildedir. Mesela ERP2022000000001 gibi. Ancak burada XML oluşturulurken eğer faturanın düzenlenme tarihi 2021 ise ve GIB numarasında 2022 yazıyorsa sistem hata vermektedir. Ayrıca GIB numarası üzerinden tekillik kontrolü de yapıldığı için bu konuda özen gösterilmesi gerekmektedir.*
    - *Aynı GİB veya Müstahsil Makbuzu numarasını birden çok göndermeye kalktığımız zaman hata alabiliriz. Bu durum fatura veya makbuzun id bilgisi için de geçerlidir. Gerçi son güncellemede GİB-Makbuz numarası sorununu rastgele vererek hallettim. Ama bu konu ile ilgili bir exception alırsanız id bilgilerini değiştirmeniz sorunu çözecektir.*
 - Önizleme kısmında **E-Fatura** sistemi için *Belge Oid* adı verilen alan oldukça önemlidir.
    - *Bu da tekil bir değer olmaktadır. XML dosyası E-Fatura servisine gönderildiği zaman, işlem başarılı ise servisten bu değer dönmektedir. Bu değerin bir şekilde fatura içerisinde kaydedilmesi önemlidir. Çünkü önizleme yapılırken bu bilgi gönderilirse Mali Değer İçermez adındaki filigran çıktıda görünmez. Ama eğer bu bilgi yoksa, sistemdeki çıktı alınırken yukarıda bahsedilen filigran çıktıda görünür*
 - **Müstahsil Makbuzu** ana nesnesinde bulunan *Müstahsil Makbuzu No* alanı XML ve matbu çıktı için önem taşımaktadır.
    - *Formatı makbuzu kesen kurumun kısaltması 3 harf, makbuzun yılı 4 karakter ve geri kalan 9 hane de makbuzun sıra numarası vb. olaack şekildedir. Mesela ERP2022000000001 gibi. Ancak burada XML oluşturulurken eğer makbuzun tarihi 2021 ise ve GIB numarasında 2022 yazıyorsa sistem hata vermektedir. Ayrıca GIB numarası üzerinden tekillik kontrolüde yapıldığı için bu konuda özen gösterilmesi gerekmektedir.*
 - Sistemde XML çıktısı için QNB Finans portalı üzerinde kendinize bir şablon oluşturmanız faydalı olacaktır. Bu şablonda faturadaki veya müstahsil makbuzundaki hangi bilgilerin nerede görüneceği, logonun nasıl olacağı vb. bilgileri ayarlayabilmektesiniz. Şu anda projede **Türkşeker** şablonu kullanılmaktadır. Ama bunu kendinize göre ayarlayabilirsiniz.
 - Bir faturanın çıktısı alınırken bazı istisnai durumlar söz konusu olabilmektedir. Mesela gönderilen faturadaki tüzel kişi fatura tarihinde e-fatura mükellefi olmayıp sonrasında mükellef olmuş olabilir. Bu yüzden Önizleme metodlarında en son eklenen kayıtlı kullanıcı listesi çalıştırılarak, eğer bu listede firmanın vergi numarası varsa **E-Fatura** değilse **E-Arşiv** çıktısı alınması mantıklı olacaktır. Ben şimdilik eski usülde bıraktım.
 - Olabildiğince kodlarda kontroller konuldu ancak herhangi bir durumda *exception* vb. oluşursa kodu **debug** ederek hatanın nerede olduğunu kontrol edebilirsiniz.
 - Kendi kullandığınız bazı alanlar XML içerisine tag ile konulmamış olabilir. Mesela Tevkifat kesintileri konulmamış olabilir. Burada en sık kullanılan alanlar üzerinden **XML** oluşturulmaya çalışıldı
    - *Burada deneme yanılma yöntemi uygulayabilirsiniz. Ya da ben fırsat buldukça uygulamayı güncellemeye çalışacağım*
   
# Dokümantasyon ve Örnek Ekran Görüntüleri

Kaynak kod hakkında, web servis metotları ile ilgili olarak hazırlanmış dokümanlara ve örnek XML dosyalarına [Dokümantasyon](https://github.com/coderserdar/QNBFinansGIB/blob/main/Documentation/) kısmından ulaşabilirsiniz. Burada özellikle [Kaynak Kod Dokümantasyonu](https://github.com/coderserdar/QNBFinansGIB/blob/main/Documentation/QNBFinansGIB.pdf) önem taşımaktadır. Bu *PDF* dosyası üzerinden kaynak kodları inceleyebilirsiniz. Diğer PDF dosyalarında da web servis metotlarına ilişkin bilgiler yer almaktadır. Bu *PDF* dosyaları üzerinden kaynak kodları inceleyebilirsiniz. PDF dosyası Hyperlink desteklediği için doküman üzerinden kodlara, fonksiyonlara vb. gidebilirsiniz. Kaynak kod içerisinde olabildiğince detaylı bir şekilde açıklama satırları yazmaya çalıştım.

Programla ilgili örnek ekran görüntüleri aşağıdadır

<table>
   <tr>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_01.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_02.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_03.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_04.png?raw=true"></td>
   </tr>
   <tr>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_05.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_06.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_07.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_08.png?raw=true"></td>
   </tr>
   <tr>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_09.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_10.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_11.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_12.png?raw=true"></td>
   </tr>
   <tr>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_13.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_14.png?raw=true"></td>
      <td><img src="https://github.com/coderserdar/QNBFinansGIB/blob/main/Screenshots/App_Screens_15.png?raw=true"></td>
   </tr>
</table>
using QNBFinansGIB.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static QNBFinansGIB.Utils.Enums;

namespace QNBFinansGIB.Utils
{
    /// <summary>
    /// Giden Fatura ve Müstahsil Makbuzları ile ilgili olarak, Kendisi, Ana Kayda İlişkin Detaylar, Aktarılacak Klasör gibi bilgiler kullanılarak
    /// QNB Finans GİB Web Servislerine gönderilecek XML dosyalarının hazırlanması ile ilgili
    /// Metotların bulunduğu yardımcı sınıftır.
    /// </summary>
    public static class YardimciSiniflar
    {
        /// <summary>
        /// Giden Fatura Bilgisi Üzerinden E-Fatura Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        /// <returns>Kaydedilen Dosyanın Bilgisayardaki Adresi</returns>
        public static string EFaturaXMLOlustur(GidenFaturaDTO gidenFatura,
            List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, string aktarilacakKlasorAdi)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(declaration, root);
            doc.AppendChild(
                doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

            root = doc.CreateElement("Invoice");

            gidenFatura = gidenFatura.BosluklariKaldir();

            var kodSatisTuruKod = 0;
            if (gidenFatura.SatisTuruKod != null)
                kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;

            var kodFaturaTuruKod = 0;
            if (gidenFatura.FaturaTuruKod != null)
                kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;

            var kodFaturaGrupTuruKod = 0;
            if (gidenFatura.FaturaGrupTuruKod != null)
                kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

            #region Standart ve Faturaya Bağlı Bilgiler

            // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır

            #region Standart Bilgiler

            root.RemoveAllAttributes();
            var locationAttribute = "xsi:schemaLocation";
            var location = doc.CreateAttribute("xsi", "schemaLocation",
                "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
            location.Value =
                "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
            //var location = doc.CreateAttribute(schemaLocation);
            //location.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
            var xmlns = doc.CreateAttribute("xmlns");
            xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
            xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
            var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
            xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            var xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
            xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var xmlnsext = doc.CreateAttribute("xmlns:ext");
            xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
            root.Attributes.Append(location);
            root.Attributes.Append(xmlns);
            root.Attributes.Append(xmlnsn4);
            root.Attributes.Append(xmlnsxsi);
            root.Attributes.Append(xmlnscac);
            root.Attributes.Append(xmlnscbc);
            root.Attributes.Append(xmlnsext);
            var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
            var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
            var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
            var extUbl4 = doc.CreateElement("n4", "auto-generated_for_wildcard", xmlnsn4.Value);
            extUbl3.AppendChild(extUbl4);
            extUbl2.AppendChild(extUbl3);
            extUbl.AppendChild(extUbl2);

            root.AppendChild(extUbl);

            var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
            versionId.InnerText = "2.1";
            root.AppendChild(versionId);

            var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
            customizationId.InnerText = "TR1.2";
            root.AppendChild(customizationId);

            var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
            profileId.InnerText = "TEMELFATURA";
            root.AppendChild(profileId);

            var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                id.InnerText = gidenFatura.GibNumarasi;
            else
                id.InnerText = "ABC" + gidenFatura.DuzenlemeTarihi?.Year +
                               DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
            root.AppendChild(id);

            var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
            copyIndicator.InnerText = "false";
            root.AppendChild(copyIndicator);

            var gidenFaturaId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
            gidenFaturaId.InnerText = gidenFatura.GidenFaturaId.Length == 36
                ? gidenFatura.GidenFaturaId.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            root.AppendChild(gidenFaturaId);

            var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            root.AppendChild(issueDate);

            var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
            issueTime.InnerText = gidenFatura.DuzenlemeTarihi?.ToString("HH:mm:ss") ?? string.Empty;
            root.AppendChild(issueTime);

            var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
            invoiceTypeCode.InnerText = "SATIS";
            if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                invoiceTypeCode.InnerText = "ISTISNA";
            if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                invoiceTypeCode.InnerText = "IHRACKAYITLI";
            if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                invoiceTypeCode.InnerText = "IADE";
            root.AppendChild(invoiceTypeCode);

            #region İrsaliye Bilgileri

            #region İrsaliye ve Tonaj Düzenlemesi

            var gidenFaturaDetayListesi2 = new List<GidenFaturaDetayDTO>();
            foreach (var item in gidenFaturaDetayListesi)
            {
                if (!string.IsNullOrEmpty(item.IrsaliyeNo) && !string.IsNullOrEmpty(item.PlakaNo))
                {
                    if (!gidenFaturaDetayListesi2.Any(j =>
                            j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo))
                    {
                        item.Tonaj = item.Miktar;
                        gidenFaturaDetayListesi2.Add(item);
                    }
                    else
                    {
                        var index = gidenFaturaDetayListesi2.FindIndex(j =>
                            j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo);
                        gidenFaturaDetayListesi2[index].Tonaj += item.Miktar;
                    }
                }
                else
                    gidenFaturaDetayListesi2.Add(item);
            }
            //if (gidenFaturaDetayListesi2.Count < 1)
            //    gidenFaturaDetayListesi2.Add(new GidenFaturaDetayDTO());

            #endregion İrsaliye ve Tonaj Düzenlemesi

            if (gidenFaturaDetayListesi2.Count > 0)
            {
                foreach (var item in gidenFaturaDetayListesi2)
                {
                    var irsaliyeNote = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                    irsaliyeNote.InnerText = "";
                    if (!string.IsNullOrEmpty(item.PlakaNo))
                        irsaliyeNote.InnerText += "Plaka No: " + item.PlakaNo;
                    if (!string.IsNullOrEmpty(item.SevkIrsaliyesiNo))
                        irsaliyeNote.InnerText += " İrsaliye No: " + item.SevkIrsaliyesiNo;
                    if (item.SevkIrsaliyeTarihi != null)
                        irsaliyeNote.InnerText += " İrsaliye Tarihi: " + item.SevkIrsaliyeTarihi?.ToShortDateString();
                    if (item.YuklemeFormuNo != null)
                        irsaliyeNote.InnerText += " Yükleme Formu No: " + item.YuklemeFormuNo;
                    if (item.Tonaj != null)
                        irsaliyeNote.InnerText += " Tonaj: " + item.Tonaj;
                    if ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                        kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode() &&
                        !string.IsNullOrEmpty(gidenFatura.Aciklama))
                        irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                    if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                        root.AppendChild(irsaliyeNote);
                }
            }

            #endregion

            var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
            documentCurrencyCode.InnerText = "TRY";
            root.AppendChild(documentCurrencyCode);

            var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
            lineCountNumeric.InnerText = gidenFaturaDetayListesi.Count.ToString();
            root.AppendChild(lineCountNumeric);

            #region AdditionalDocumentReference
            
            var additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = gidenFatura.GidenFaturaId;
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            additionalDocumentReference.AppendChild(issueDate);
            var documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd");
            additionalDocumentReference.AppendChild(issueDate);
            documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            var attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            var embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            XmlAttribute characterSetCode = doc.CreateAttribute("characterSetCode");
            characterSetCode.Value = "UTF-8";
            XmlAttribute encodingCode = doc.CreateAttribute("encodingCode");
            encodingCode.Value = "Base64";
            // XmlAttribute fileName2 = doc.CreateAttribute("fileName");
            // fileName2.Value = "efatura.xslt";
            XmlAttribute mimeCode = doc.CreateAttribute("mimeCode");
            mimeCode.Value = "application/xml";
            embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            // embeddedDocumentBinaryObject.Attributes.Append(fileName2);
            embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            #region Base 64 Metin
            embeddedDocumentBinaryObject.InnerText = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHhzbDpzdHlsZXNoZWV0IHZlcnNpb249IjIuMCIgeG1sbnM6eHNsPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L1hTTC9UcmFuc2Zvcm0iCiAgICAgICAgeG1sbnM6Y2FjPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6c2NoZW1hOnhzZDpDb21tb25BZ2dyZWdhdGVDb21wb25lbnRzLTIiCiAgICAgICAgeG1sbnM6Y2JjPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6c2NoZW1hOnhzZDpDb21tb25CYXNpY0NvbXBvbmVudHMtMiIKICAgICAgICB4bWxuczpjY3RzPSJ1cm46dW46dW5lY2U6dW5jZWZhY3Q6ZG9jdW1lbnRhdGlvbjoyIgogICAgICAgIHhtbG5zOmNsbTU0MjE3PSJ1cm46dW46dW5lY2U6dW5jZWZhY3Q6Y29kZWxpc3Q6c3BlY2lmaWNhdGlvbjo1NDIxNzoyMDAxIgogICAgICAgIHhtbG5zOmNsbTU2Mzk9InVybjp1bjp1bmVjZTp1bmNlZmFjdDpjb2RlbGlzdDpzcGVjaWZpY2F0aW9uOjU2Mzk6MTk4OCIKICAgICAgICB4bWxuczpjbG02NjQxMT0idXJuOnVuOnVuZWNlOnVuY2VmYWN0OmNvZGVsaXN0OnNwZWNpZmljYXRpb246NjY0MTE6MjAwMSIKICAgICAgICB4bWxuczpjbG1JQU5BTUlNRU1lZGlhVHlwZT0idXJuOnVuOnVuZWNlOnVuY2VmYWN0OmNvZGVsaXN0OnNwZWNpZmljYXRpb246SUFOQU1JTUVNZWRpYVR5cGU6MjAwMyIKICAgICAgICB4bWxuczpmbj0iaHR0cDovL3d3dy53My5vcmcvMjAwNS94cGF0aC1mdW5jdGlvbnMiIHhtbG5zOmxpbms9Imh0dHA6Ly93d3cueGJybC5vcmcvMjAwMy9saW5rYmFzZSIKICAgICAgICB4bWxuczpuMT0idXJuOm9hc2lzOm5hbWVzOnNwZWNpZmljYXRpb246dWJsOnNjaGVtYTp4c2Q6SW52b2ljZS0yIgogICAgICAgIHhtbG5zOnFkdD0idXJuOm9hc2lzOm5hbWVzOnNwZWNpZmljYXRpb246dWJsOnNjaGVtYTp4c2Q6UXVhbGlmaWVkRGF0YXR5cGVzLTIiCiAgICAgICAgeG1sbnM6dWR0PSJ1cm46dW46dW5lY2U6dW5jZWZhY3Q6ZGF0YTpzcGVjaWZpY2F0aW9uOlVucXVhbGlmaWVkRGF0YVR5cGVzU2NoZW1hTW9kdWxlOjIiCiAgICAgICAgeG1sbnM6eGJybGRpPSJodHRwOi8veGJybC5vcmcvMjAwNi94YnJsZGkiIHhtbG5zOnhicmxpPSJodHRwOi8vd3d3Lnhicmwub3JnLzIwMDMvaW5zdGFuY2UiCiAgICAgICAgeG1sbnM6eGR0PSJodHRwOi8vd3d3LnczLm9yZy8yMDA1L3hwYXRoLWRhdGF0eXBlcyIgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiCiAgICAgICAgeG1sbnM6eHM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIiB4bWxuczp4c2Q9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hIgogICAgICAgIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiCiAgICAgICAgZXhjbHVkZS1yZXN1bHQtcHJlZml4ZXM9ImNhYyBjYmMgY2N0cyBjbG01NDIxNyBjbG01NjM5IGNsbTY2NDExIGNsbUlBTkFNSU1FTWVkaWFUeXBlIGZuIGxpbmsgbjEgcWR0IHVkdCB4YnJsZGkgeGJybGkgeGR0IHhsaW5rIHhzIHhzZCB4c2kiPgogICAgICAgIDx4c2w6ZGVjaW1hbC1mb3JtYXQgbmFtZT0iZXVyb3BlYW4iIGRlY2ltYWwtc2VwYXJhdG9yPSIsIiBncm91cGluZy1zZXBhcmF0b3I9Ii4iIE5hTj0iIi8+CiAgICAgICAgPHhzbDpvdXRwdXQgdmVyc2lvbj0iNC4wIiBtZXRob2Q9Imh0bWwiIGluZGVudD0ibm8iIGVuY29kaW5nPSJVVEYtOCIKICAgICAgICBkb2N0eXBlLXB1YmxpYz0iLS8vVzNDLy9EVEQgSFRNTCA0LjAxIFRyYW5zaXRpb25hbC8vRU4iCiAgICAgICAgZG9jdHlwZS1zeXN0ZW09Imh0dHA6Ly93d3cudzMub3JnL1RSL2h0bWw0L2xvb3NlLmR0ZCIvPgogICAgICAgIDx4c2w6cGFyYW0gbmFtZT0iU1ZfT3V0cHV0Rm9ybWF0IiBzZWxlY3Q9IidIVE1MJyIvPgoKICAgICAgICA8eHNsOnRlbXBsYXRlIG5hbWU9InJlcE5MIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0icFRleHQiIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJub3QoY29udGFpbnMoc3Vic3RyaW5nKHN1YnN0cmluZy1iZWZvcmUoY29uY2F0KCRwVGV4dCwnJiN4QTsnKSwnJiN4QTsnKSwwLDgpLCAnIyMnKSkgYW5kIHN0cmluZy1sZW5ndGgoc3Vic3RyaW5nLWJlZm9yZShjb25jYXQoJHBUZXh0LCcmI3hBOycpLCcmI3hBOycpKT4zIj4KICAgICAgICAgICAgICAgICAgICAgICAgPGI+KiA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y29weS1vZiBzZWxlY3Q9InN1YnN0cmluZy1iZWZvcmUoY29uY2F0KCRwVGV4dCwnJiN4QTsnKSwnJiN4QTsnKSIvPgogICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjb250YWlucygkcFRleHQsICcmI3hBOycpIj4KCiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZXBOTCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9InBUZXh0IiBzZWxlY3Q9CiAgICAic3Vic3RyaW5nLWFmdGVyKCRwVGV4dCwgJyYjeEE7JykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2FsbC10ZW1wbGF0ZT4KICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgIDwveHNsOnRlbXBsYXRlPgoKICAgICAgICA8eHNsOnRlbXBsYXRlIG5hbWU9InJlcE5MMiI+CiAgICAgICAgICAgICAgICA8eHNsOnBhcmFtIG5hbWU9InBUZXh0IiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY29udGFpbnMoc3Vic3RyaW5nKHN1YnN0cmluZy1iZWZvcmUoY29uY2F0KCRwVGV4dCwnJiN4QTsnKSwnJiN4QTsnKSwwLDgpLCAnIyMnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNvcHktb2Ygc2VsZWN0PSJzdWJzdHJpbmctYWZ0ZXIoc3Vic3RyaW5nLWJlZm9yZShzdWJzdHJpbmctYmVmb3JlKGNvbmNhdCgkcFRleHQsJyYjeEE7JyksJyYjeEE7JyksJzonKSwnIyMnKSIvPjo8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y29weS1vZiBzZWxlY3Q9InN1YnN0cmluZy1hZnRlcihzdWJzdHJpbmctYmVmb3JlKGNvbmNhdCgkcFRleHQsJyYjeEE7JyksJyYjeEE7JyksJzonKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY29udGFpbnMoJHBUZXh0LCAnJiN4QTsnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZXBOTDIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2l0aC1wYXJhbSBuYW1lPSJwVGV4dCIgc2VsZWN0PQogICAgInN1YnN0cmluZy1hZnRlcigkcFRleHQsICcmI3hBOycpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNhbGwtdGVtcGxhdGU+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICA8L3hzbDp0ZW1wbGF0ZT4KCiAgICAgICAgPHhzbDp2YXJpYWJsZSBuYW1lPSJYTUwiIHNlbGVjdD0iLyIvPgogICAgICAgIDx4c2w6dGVtcGxhdGUgbmFtZT0icmVtb3ZlTGVhZGluZ1plcm9zIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0ib3JpZ2luYWxTdHJpbmciLz4KICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0ic3RhcnRzLXdpdGgoJG9yaWdpbmFsU3RyaW5nLCcwJykiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJyZW1vdmVMZWFkaW5nWmVyb3MiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9Im9yaWdpbmFsU3RyaW5nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZy1hZnRlcigkb3JpZ2luYWxTdHJpbmcsJzAnICkiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndpdGgtcGFyYW0+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2FsbC10ZW1wbGF0ZT4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9IiRvcmlnaW5hbFN0cmluZyIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgPC94c2w6dGVtcGxhdGU+CiAgICAgICAgPHhzbDp0ZW1wbGF0ZSBuYW1lPSJUcmFuc3BvcnRNb2RlIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0iVHJhbnNwb3J0TW9kZVR5cGUiIC8+CiAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRUcmFuc3BvcnRNb2RlVHlwZT0xIj5EZW5penlvbHU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFRyYW5zcG9ydE1vZGVUeXBlPTIiPkRlbWlyeW9sdTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkVHJhbnNwb3J0TW9kZVR5cGU9MyI+S2FyYXlvbHU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFRyYW5zcG9ydE1vZGVUeXBlPTQiPkhhdmF5b2x1PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRUcmFuc3BvcnRNb2RlVHlwZT01Ij5Qb3N0YTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkVHJhbnNwb3J0TW9kZVR5cGU9NiI+w4dvayBhcmHDp2zEsTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkVHJhbnNwb3J0TW9kZVR5cGU9NyI+U2FiaXQgdGHFn8SxbWEgdGVzaXNsZXJpPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRUcmFuc3BvcnRNb2RlVHlwZT04Ij7EsMOnIHN1IHRhxZ/EsW1hY8SxbMSxxJ/EsTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIkVHJhbnNwb3J0TW9kZVR5cGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgPC94c2w6Y2hvb3NlPgogICAgICAgIDwveHNsOnRlbXBsYXRlPgogICAgICAgIDx4c2w6dGVtcGxhdGUgbmFtZT0iUGFja2FnaW5nIj4KICAgICAgICAgICAgICAgIDx4c2w6cGFyYW0gbmFtZT0iUGFja2FnaW5nVHlwZSIgLz4KICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzFBJyI+RHJ1bSwgc3RlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzFCJyI+RHJ1bSwgYWx1bWluaXVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPScxRCciPkRydW0sIHBseXdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzFGJyI+Q29udGFpbmVyLCBmbGV4aWJsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nMUcnIj5EcnVtLCBmaWJyZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nMVcnIj5EcnVtLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzJDJyI+QmFycmVsLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzNBJyI+SmVycmljYW4sIHN0ZWVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSczSCciPkplcnJpY2FuLCBwbGFzdGljPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc0MyciPkJhZywgc3VwZXIgYnVsazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNDQnIj5CYWcsIHBvbHliYWc8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzRBJyI+Qm94LCBzdGVlbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNEInIj5Cb3gsIGFsdW1pbml1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNEMnIj5Cb3gsIG5hdHVyYWwgd29vZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNEQnIj5Cb3gsIHBseXdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzRGJyI+Qm94LCByZWNvbnN0aXR1dGVkIHdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzRHJyI+Qm94LCBmaWJyZWJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc0SCciPkJveCwgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nNUgnIj5CYWcsIHdvdmVuIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzVMJyI+QmFnLCB0ZXh0aWxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc1TSciPkJhZywgcGFwZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzZIJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc2UCciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9JzdBJyI+Q2FzZSwgY2FyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc3QiciPkNhc2UsIHdvb2RlbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nOEEnIj5QYWxsZXQsIHdvb2RlbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nOEInIj5DcmF0ZSwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSc4QyciPkJ1bmRsZSwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdBQSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmlnaWQgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUInIj5SZWNlcHRhY2xlLCBmaWJyZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUMnIj5SZWNlcHRhY2xlLCBwYXBlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUQnIj5SZWNlcHRhY2xlLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FFJyI+QWVyb3NvbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUYnIj5QYWxsZXQsIG1vZHVsYXIsIGNvbGxhcnMgODBjbXMgKiA2MGNtczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQUcnIj5QYWxsZXQsIHNocmlua3dyYXBwZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FIJyI+UGFsbGV0LCAxMDBjbXMgKiAxMTBjbXM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FJJyI+Q2xhbXNoZWxsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdBSiciPkNvbmU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0FMJyI+QmFsbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQU0nIj5BbXBvdWxlLCBub24tcHJvdGVjdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdBUCciPkFtcG91bGUsIHByb3RlY3RlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQVQnIj5BdG9taXplcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQVYnIj5DYXBzdWxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCNCciPkJlbHQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JBJyI+QmFycmVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCQiciPkJvYmJpbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQkMnIj5Cb3R0bGVjcmF0ZSAvIGJvdHRsZXJhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JEJyI+Qm9hcmQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JFJyI+QnVuZGxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCRiciPkJhbGxvb24sIG5vbi1wcm90ZWN0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JHJyI+QmFnPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCSCciPkJ1bmNoPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCSSciPkJpbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQkonIj5CdWNrZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JLJyI+QmFza2V0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCTCciPkJhbGUsIGNvbXByZXNzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JNJyI+QmFzaW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JOJyI+QmFsZSwgbm9uLWNvbXByZXNzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JPJyI+Qm90dGxlLCBub24tcHJvdGVjdGVkLCBjeWxpbmRyaWNhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlAnIj5CYWxsb29uLCBwcm90ZWN0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JRJyI+Qm90dGxlLCBwcm90ZWN0ZWQgY3lsaW5kcmljYWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JSJyI+QmFyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCUyciPkJvdHRsZSwgbm9uLXByb3RlY3RlZCwgYnVsYm91czwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlQnIj5Cb2x0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCVSciPkJ1dHQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0JWJyI+Qm90dGxlLCBwcm90ZWN0ZWQgYnVsYm91czwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlcnIj5Cb3gsIGZvciBsaXF1aWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCWCciPkJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQlknIj5Cb2FyZCwgaW4gYnVuZGxlL2J1bmNoL3RydXNzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdCWiciPkJhcnMsIGluIGJ1bmRsZS9idW5jaC90cnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ0EnIj5DYW4sIHJlY3Rhbmd1bGFyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDQiciPkNyYXRlLCBiZWVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDQyciPkNodXJuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDRCciPkNhbiwgd2l0aCBoYW5kbGUgYW5kIHNwb3V0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDRSciPkNyZWVsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDRiciPkNvZmZlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ0cnIj5DYWdlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDSCciPkNoZXN0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDSSciPkNhbmlzdGVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDSiciPkNvZmZpbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ0snIj5DYXNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDTCciPkNvaWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NNJyI+Q2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ04nIj5Db250YWluZXIsIG5vdCBvdGhlcndpc2Ugc3BlY2lmaWVkIGFzIHRyYW5zcG9ydCBlcXVpcG1lbnQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NPJyI+Q2FyYm95LCBub24tcHJvdGVjdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDUCciPkNhcmJveSwgcHJvdGVjdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDUSciPkNhcnRyaWRnZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1InIj5DcmF0ZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1MnIj5DYXNlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDVCciPkNhcnRvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1UnIj5DdXA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NWJyI+Q292ZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0NXJyI+Q2FnZSwgcm9sbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nQ1gnIj5DYW4sIGN5bGluZHJpY2FsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDWSciPkN5bGluZGVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdDWiciPkNhbnZhczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREEnIj5DcmF0ZSwgbXVsdGlwbGUgbGF5ZXIsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RCJyI+Q3JhdGUsIG11bHRpcGxlIGxheWVyLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RDJyI+Q3JhdGUsIG11bHRpcGxlIGxheWVyLCBjYXJkYm9hcmQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RHJyI+Q2FnZSwgQ29tbW9ud2VhbHRoIEhhbmRsaW5nIEVxdWlwbWVudCBQb29sIChDSEVQKTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREgnIj5Cb3gsIENvbW1vbndlYWx0aCBIYW5kbGluZyBFcXVpcG1lbnQgUG9vbCAoQ0hFUCksIEV1cm9ib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RJJyI+RHJ1bSwgaXJvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREonIj5EZW1pam9obiwgbm9uLXByb3RlY3RlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nREsnIj5DcmF0ZSwgYnVsaywgY2FyZGJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdETCciPkNyYXRlLCBidWxrLCBwbGFzdGljPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdETSciPkNyYXRlLCBidWxrLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ROJyI+RGlzcGVuc2VyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdEUCciPkRlbWlqb2huLCBwcm90ZWN0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RSJyI+RHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRFMnIj5UcmF5LCBvbmUgbGF5ZXIgbm8gY292ZXIsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RUJyI+VHJheSwgb25lIGxheWVyIG5vIGNvdmVyLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RVJyI+VHJheSwgb25lIGxheWVyIG5vIGNvdmVyLCBwb2x5c3R5cmVuZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRFYnIj5UcmF5LCBvbmUgbGF5ZXIgbm8gY292ZXIsIGNhcmRib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRFcnIj5UcmF5LCB0d28gbGF5ZXJzIG5vIGNvdmVyLCBwbGFzdGljIHRyYXk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0RYJyI+VHJheSwgdHdvIGxheWVycyBubyBjb3Zlciwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdEWSciPlRyYXksIHR3byBsYXllcnMgbm8gY292ZXIsIGNhcmRib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRUMnIj5CYWcsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VEJyI+Q2FzZSwgd2l0aCBwYWxsZXQgYmFzZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRUUnIj5DYXNlLCB3aXRoIHBhbGxldCBiYXNlLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VGJyI+Q2FzZSwgd2l0aCBwYWxsZXQgYmFzZSwgY2FyZGJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdFRyciPkNhc2UsIHdpdGggcGFsbGV0IGJhc2UsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VIJyI+Q2FzZSwgd2l0aCBwYWxsZXQgYmFzZSwgbWV0YWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0VJJyI+Q2FzZSwgaXNvdGhlcm1pYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRU4nIj5FbnZlbG9wZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRkInIj5GbGV4aWJhZzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRkMnIj5DcmF0ZSwgZnJ1aXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZEJyI+Q3JhdGUsIGZyYW1lZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRkUnIj5GbGV4aXRhbms8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZJJyI+Rmlya2luPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdGTCciPkZsYXNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdGTyciPkZvb3Rsb2NrZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZQJyI+RmlsbXBhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZSJyI+RnJhbWU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0ZUJyI+Rm9vZHRhaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nRlcnIj5DYXJ0LCBmbGF0YmVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdGWCciPkJhZywgZmxleGlibGUgY29udGFpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdHQiciPkJvdHRsZSwgZ2FzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdHSSciPkdpcmRlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR0wnIj5Db250YWluZXIsIGdhbGxvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR1InIj5SZWNlcHRhY2xlLCBnbGFzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR1UnIj5UcmF5LCBjb250YWluaW5nIGhvcml6b250YWxseSBzdGFja2VkIGZsYXQgaXRlbXM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0dZJyI+QmFnLCBndW5ueTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nR1onIj5HaXJkZXJzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0hBJyI+QmFza2V0LCB3aXRoIGhhbmRsZSwgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSEInIj5CYXNrZXQsIHdpdGggaGFuZGxlLCB3b29kZW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0hDJyI+QmFza2V0LCB3aXRoIGhhbmRsZSwgY2FyZGJvYXJkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdIRyciPkhvZ3NoZWFkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdITiciPkhhbmdlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSFInIj5IYW1wZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lBJyI+UGFja2FnZSwgZGlzcGxheSwgd29vZGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJQiciPlBhY2thZ2UsIGRpc3BsYXksIGNhcmRib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSUMnIj5QYWNrYWdlLCBkaXNwbGF5LCBwbGFzdGljPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJRCciPlBhY2thZ2UsIGRpc3BsYXksIG1ldGFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJRSciPlBhY2thZ2UsIHNob3c8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lGJyI+UGFja2FnZSwgZmxvdzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSUcnIj5QYWNrYWdlLCBwYXBlciB3cmFwcGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJSCciPkRydW0sIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lLJyI+UGFja2FnZSwgY2FyZGJvYXJkLCB3aXRoIGJvdHRsZSBncmlwLWhvbGVzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdJTCciPlRyYXksIHJpZ2lkLCBsaWRkZWQgc3RhY2thYmxlIChDRU4gVFMgMTQ0ODI6MjAwMik8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0lOJyI+SW5nb3Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0laJyI+SW5nb3RzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0pCJyI+QmFnLCBqdW1ibzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSkMnIj5KZXJyaWNhbiwgcmVjdGFuZ3VsYXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0pHJyI+SnVnPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdKUiciPkphcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nSlQnIj5KdXRlYmFnPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdKWSciPkplcnJpY2FuLCBjeWxpbmRyaWNhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nS0cnIj5LZWc8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0tJJyI+S2l0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdMRSciPkx1Z2dhZ2U8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0xHJyI+TG9nPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdMVCciPkxvdDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTFUnIj5MdWc8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J0xWJyI+TGlmdHZhbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTFonIj5Mb2dzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01BJyI+Q3JhdGUsIG1ldGFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdNQiciPkJhZywgbXVsdGlwbHk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01DJyI+Q3JhdGUsIG1pbGs8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01FJyI+Q29udGFpbmVyLCBtZXRhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTVInIj5SZWNlcHRhY2xlLCBtZXRhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTVMnIj5TYWNrLCBtdWx0aS13YWxsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdNVCciPk1hdDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTVcnIj5SZWNlcHRhY2xlLCBwbGFzdGljIHdyYXBwZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J01YJyI+TWF0Y2hib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J05BJyI+Tm90IGF2YWlsYWJsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTkUnIj5VbnBhY2tlZCBvciB1bnBhY2thZ2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdORiciPlVucGFja2VkIG9yIHVucGFja2FnZWQsIHNpbmdsZSB1bml0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdORyciPlVucGFja2VkIG9yIHVucGFja2FnZWQsIG11bHRpcGxlIHVuaXRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdOUyciPk5lc3Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J05UJyI+TmV0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdOVSciPk5ldCwgdHViZSwgcGxhc3RpYzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nTlYnIj5OZXQsIHR1YmUsIHRleHRpbGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09BJyI+UGFsbGV0LCBDSEVQIDQwIGNtIHggNjAgY208L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09CJyI+UGFsbGV0LCBDSEVQIDgwIGNtIHggMTIwIGNtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPQyciPlBhbGxldCwgQ0hFUCAxMDAgY20geCAxMjAgY208L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09EJyI+UGFsbGV0LCBBUyA0MDY4LTE5OTM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09FJyI+UGFsbGV0LCBJU08gVDExPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPRiciPlBsYXRmb3JtLCB1bnNwZWNpZmllZCB3ZWlnaHQgb3IgZGltZW5zaW9uPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPSyciPkJsb2NrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdPVCciPk9jdGFiaW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J09VJyI+Q29udGFpbmVyLCBvdXRlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUDInIj5QYW48L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BBJyI+UGFja2V0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQQiciPlBhbGxldCwgYm94IENvbWJpbmVkIG9wZW4tZW5kZWQgYm94IGFuZCBwYWxsZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BDJyI+UGFyY2VsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQRCciPlBhbGxldCwgbW9kdWxhciwgY29sbGFycyA4MGNtcyAqIDEwMGNtczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUEUnIj5QYWxsZXQsIG1vZHVsYXIsIGNvbGxhcnMgODBjbXMgKiAxMjBjbXM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BGJyI+UGVuPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQRyciPlBsYXRlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQSCciPlBpdGNoZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BJJyI+UGlwZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUEonIj5QdW5uZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BLJyI+UGFja2FnZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUEwnIj5QYWlsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQTiciPlBsYW5rPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQTyciPlBvdWNoPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQUCciPlBpZWNlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQUiciPlJlY2VwdGFjbGUsIHBsYXN0aWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BUJyI+UG90PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdQVSciPlRyYXk8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BWJyI+UGlwZXMsIGluIGJ1bmRsZS9idW5jaC90cnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUFgnIj5QYWxsZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BZJyI+UGxhdGVzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1BaJyI+UGxhbmtzLCBpbiBidW5kbGUvYnVuY2gvdHJ1c3M8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FBJyI+RHJ1bSwgc3RlZWwsIG5vbi1yZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUUInIj5EcnVtLCBzdGVlbCwgcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FDJyI+RHJ1bSwgYWx1bWluaXVtLCBub24tcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FEJyI+RHJ1bSwgYWx1bWluaXVtLCByZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUUYnIj5EcnVtLCBwbGFzdGljLCBub24tcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FHJyI+RHJ1bSwgcGxhc3RpYywgcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FIJyI+QmFycmVsLCB3b29kZW4sIGJ1bmcgdHlwZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUUonIj5CYXJyZWwsIHdvb2RlbiwgcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FLJyI+SmVycmljYW4sIHN0ZWVsLCBub24tcmVtb3ZhYmxlIGhlYWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FMJyI+SmVycmljYW4sIHN0ZWVsLCByZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUU0nIj5KZXJyaWNhbiwgcGxhc3RpYywgbm9uLXJlbW92YWJsZSBoZWFkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdRTiciPkplcnJpY2FuLCBwbGFzdGljLCByZW1vdmFibGUgaGVhZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUVAnIj5Cb3gsIHdvb2RlbiwgbmF0dXJhbCB3b29kLCBvcmRpbmFyeTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUVEnIj5Cb3gsIHdvb2RlbiwgbmF0dXJhbCB3b29kLCB3aXRoIHNpZnQgcHJvb2Ygd2FsbHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1FSJyI+Qm94LCBwbGFzdGljLCBleHBhbmRlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUVMnIj5Cb3gsIHBsYXN0aWMsIHNvbGlkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdSRCciPlJvZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUkcnIj5SaW5nPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdSSiciPlJhY2ssIGNsb3RoaW5nIGhhbmdlcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUksnIj5SYWNrPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdSTCciPlJlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1JPJyI+Um9sbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nUlQnIj5SZWRuZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1JaJyI+Um9kcywgaW4gYnVuZGxlL2J1bmNoL3RydXNzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdTQSciPlNhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NCJyI+U2xhYjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0MnIj5DcmF0ZSwgc2hhbGxvdzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0QnIj5TcGluZGxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdTRSciPlNlYS1jaGVzdDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0gnIj5TYWNoZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NJJyI+U2tpZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0snIj5DYXNlLCBza2VsZXRvbjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU0wnIj5TbGlwc2hlZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NNJyI+U2hlZXRtZXRhbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU08nIj5TcG9vbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1AnIj5TaGVldCwgcGxhc3RpYyB3cmFwcGluZzwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1MnIj5DYXNlLCBzdGVlbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1QnIj5TaGVldDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1UnIj5TdWl0Y2FzZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1YnIj5FbnZlbG9wZSwgc3RlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NXJyI+U2hyaW5rd3JhcHBlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nU1gnIj5TZXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1NZJyI+U2xlZXZlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdTWiciPlNoZWV0cywgaW4gYnVuZGxlL2J1bmNoL3RydXNzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUMSciPlRhYmxldDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVEInIj5UdWI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RDJyI+VGVhLWNoZXN0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdURCciPlR1YmUsIGNvbGxhcHNpYmxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdURSciPlR5cmU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RHJyI+VGFuayBjb250YWluZXIsIGdlbmVyaWM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RJJyI+VGllcmNlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUSyciPlRhbmssIHJlY3Rhbmd1bGFyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUTCciPlR1Yiwgd2l0aCBsaWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ROJyI+VGluPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdUTyciPlR1bjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFInIj5UcnVuazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFMnIj5UcnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFQnIj5CYWcsIHRvdGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RVJyI+VHViZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFYnIj5UdWJlLCB3aXRoIG5venpsZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVFcnIj5QYWxsZXQsIHRyaXdhbGw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RZJyI+VGFuaywgY3lsaW5kcmljYWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1RaJyI+VHViZXMsIGluIGJ1bmRsZS9idW5jaC90cnVzczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVUMnIj5VbmNhZ2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdVTiciPlVuaXQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZBJyI+VmF0PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWRyciPkJ1bGssIGdhcyAoYXQgMTAzMSBtYmFyIGFuZCAxNcOCwrBDKTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVkknIj5WaWFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWSyciPlZhbnBhY2s8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1ZMJyI+QnVsaywgbGlxdWlkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWTyciPkJ1bGssIHNvbGlkLCBsYXJnZSBwYXJ0aWNsZXMgKMOC4oCcbm9kdWxlc8OC4oCdKTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVlAnIj5WYWN1dW0tcGFja2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWUSciPkJ1bGssIGxpcXVlZmllZCBnYXMgKGF0IGFibm9ybWFsIHRlbXBlcmF0dXJlL3ByZXNzdXJlKTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nVk4nIj5WZWhpY2xlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWUiciPkJ1bGssIHNvbGlkLCBncmFudWxhciBwYXJ0aWNsZXMgKMOC4oCcZ3JhaW5zw4LigJ0pPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWUyciPkJ1bGssIHNjcmFwIG1ldGFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdWWSciPkJ1bGssIHNvbGlkLCBmaW5lIHBhcnRpY2xlcyAow4LigJxwb3dkZXJzw4LigJ0pPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXQSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV0InIj5XaWNrZXJib3R0bGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dDJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBzdGVlbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV0QnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIGFsdW1pbml1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV0YnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIG1ldGFsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXRyciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgc3RlZWwsIHByZXNzdXJpc2VkID4gMTAga3BhPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXSCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgYWx1bWluaXVtLCBwcmVzc3VyaXNlZCA+IDEwIGtwYTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV0onIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIG1ldGFsLCBwcmVzc3VyZSAxMCBrcGE8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dLJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBzdGVlbCwgbGlxdWlkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXTCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgYWx1bWluaXVtLCBsaXF1aWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dNJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBtZXRhbCwgbGlxdWlkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXTiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgd292ZW4gcGxhc3RpYywgd2l0aG91dCBjb2F0L2xpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXUCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgd292ZW4gcGxhc3RpYywgY29hdGVkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXUSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgd292ZW4gcGxhc3RpYywgd2l0aCBsaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1InIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHdvdmVuIHBsYXN0aWMsIGNvYXRlZCBhbmQgbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dTJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBwbGFzdGljIGZpbG08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dUJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB0ZXh0aWxlIHdpdGggb3V0IGNvYXQvbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dVJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBuYXR1cmFsIHdvb2QsIHdpdGggaW5uZXIgbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dWJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB0ZXh0aWxlLCBjb2F0ZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1dXJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCB0ZXh0aWxlLCB3aXRoIGxpbmVyPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdXWCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgdGV4dGlsZSwgY29hdGVkIGFuZCBsaW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nV1knIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHBseXdvb2QsIHdpdGggaW5uZXIgbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1daJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByZWNvbnN0aXR1dGVkIHdvb2QsIHdpdGggaW5uZXIgbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1hBJyI+QmFnLCB3b3ZlbiBwbGFzdGljLCB3aXRob3V0IGlubmVyIGNvYXQvbGluZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1hCJyI+QmFnLCB3b3ZlbiBwbGFzdGljLCBzaWZ0IHByb29mPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdYQyciPkJhZywgd292ZW4gcGxhc3RpYywgd2F0ZXIgcmVzaXN0YW50PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdYRCciPkJhZywgcGxhc3RpY3MgZmlsbTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEYnIj5CYWcsIHRleHRpbGUsIHdpdGhvdXQgaW5uZXIgY29hdC9saW5lcjwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEcnIj5CYWcsIHRleHRpbGUsIHNpZnQgcHJvb2Y8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1hIJyI+QmFnLCB0ZXh0aWxlLCB3YXRlciByZXNpc3RhbnQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1hKJyI+QmFnLCBwYXBlciwgbXVsdGktd2FsbDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWEsnIj5CYWcsIHBhcGVyLCBtdWx0aS13YWxsLCB3YXRlciByZXNpc3RhbnQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lBJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlIGluIHN0ZWVsIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lCJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlIGluIHN0ZWVsIGNyYXRlIGJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUMnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gYWx1bWluaXVtIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lEJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlIGluIGFsdW1pbml1bSBjcmF0ZTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUYnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gd29vZGVuIGJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUcnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gcGx5d29vZCBkcnVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZSCciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIHBsYXN0aWMgcmVjZXB0YWNsZSBpbiBwbHl3b29kIGJveDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUonIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gZmlicmUgZHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWUsnIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gZmlicmVib2FyZCBib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lMJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgcGxhc3RpYyByZWNlcHRhY2xlIGluIHBsYXN0aWMgZHJ1bTwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWU0nIj5Db21wb3NpdGUgcGFja2FnaW5nLCBwbGFzdGljIHJlY2VwdGFjbGUgaW4gc29saWQgcGxhc3RpYyBib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lOJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBzdGVlbCBkcnVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZUCciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGUgaW4gc3RlZWwgY3JhdGUgYm94PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZUSciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGUgaW4gYWx1bWluaXVtIGRydW08L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lSJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBhbHVtaW5pdW0gY3JhdGU8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lTJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiB3b29kZW4gYm94PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZVCciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGUgaW4gcGx5d29vZCBkcnVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZViciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGUgaW4gd2lja2Vyd29yayBoYW1wZXI8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lXJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBmaWJyZSBkcnVtPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdZWCciPkNvbXBvc2l0ZSBwYWNrYWdpbmcsIGdsYXNzIHJlY2VwdGFjbGUgaW4gZmlicmVib2FyZCBib3g8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1lZJyI+Q29tcG9zaXRlIHBhY2thZ2luZywgZ2xhc3MgcmVjZXB0YWNsZSBpbiBleHBhbmRhYmxlIHBsYXN0aWMgcGFjazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWVonIj5Db21wb3NpdGUgcGFja2FnaW5nLCBnbGFzcyByZWNlcHRhY2xlIGluIHNvbGlkIHBsYXN0aWMgcGFjazwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWkEnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHBhcGVyLCBtdWx0aS13YWxsPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaQiciPkJhZywgbGFyZ2U8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pDJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBwYXBlciwgbXVsdGktd2FsbCwgd2F0ZXIgcmVzaXN0YW50PC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaRCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmlnaWQgcGxhc3RpYywgd2l0aCBzdHJ1Y3R1cmFsIGVxdWlwbWVudCwgc29saWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaRiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmlnaWQgcGxhc3RpYywgZnJlZXN0YW5kaW5nLCBzb2xpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pHJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByaWdpZCBwbGFzdGljLCB3aXRoIHN0cnVjdHVyYWwgZXF1aXBtZW50LCBwcmVzc3VyaXNlZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWkgnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIHJpZ2lkIHBsYXN0aWMsIGZyZWVzdGFuZGluZywgcHJlc3N1cmlzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pKJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCByaWdpZCBwbGFzdGljLCB3aXRoIHN0cnVjdHVyYWwgZXF1aXBtZW50LCBsaXF1aWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaSyciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmlnaWQgcGxhc3RpYywgZnJlZXN0YW5kaW5nLCBsaXF1aWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaTCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgY29tcG9zaXRlLCByaWdpZCBwbGFzdGljLCBzb2xpZHM8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pNJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBjb21wb3NpdGUsIGZsZXhpYmxlIHBsYXN0aWMsIHNvbGlkczwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWk4nIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIGNvbXBvc2l0ZSwgcmlnaWQgcGxhc3RpYywgcHJlc3N1cmlzZWQ8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pQJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBjb21wb3NpdGUsIGZsZXhpYmxlIHBsYXN0aWMsIHByZXNzdXJpc2VkPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaUSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgY29tcG9zaXRlLCByaWdpZCBwbGFzdGljLCBsaXF1aWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaUiciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgY29tcG9zaXRlLCBmbGV4aWJsZSBwbGFzdGljLCBsaXF1aWRzPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaUyciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgY29tcG9zaXRlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaVCciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgZmlicmVib2FyZDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIkUGFja2FnaW5nVHlwZT0nWlUnIj5JbnRlcm1lZGlhdGUgYnVsayBjb250YWluZXIsIGZsZXhpYmxlPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaViciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgbWV0YWwsIG90aGVyIHRoYW4gc3RlZWw8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pXJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBuYXR1cmFsIHdvb2Q8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iJFBhY2thZ2luZ1R5cGU9J1pYJyI+SW50ZXJtZWRpYXRlIGJ1bGsgY29udGFpbmVyLCBwbHl3b29kPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IiRQYWNrYWdpbmdUeXBlPSdaWSciPkludGVybWVkaWF0ZSBidWxrIGNvbnRhaW5lciwgcmVjb25zdGl0dXRlZCB3b29kPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9IiRQYWNrYWdpbmdUeXBlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgIDwveHNsOmNob29zZT4KICAgICAgICA8L3hzbDp0ZW1wbGF0ZT4KICAgICAgICA8eHNsOnRlbXBsYXRlIG1hdGNoPSIvIj4KICAgICAgICA8eHNsOmNvbW1lbnQ+ZUZpbmFucyDFnmFibG9uIFRhc2FyxLFtIEFyYWPEsSDEsGxlIEhhesSxcmxhbm3EscWfdMSxci48L3hzbDpjb21tZW50PgogICAgICAgIDxodG1sPgogICAgICAgIDxoZWFkPgo8bWV0YSBodHRwLWVxdWl2PSJYLVVBLUNvbXBhdGlibGUiIGNvbnRlbnQ9IklFPWVkZ2UiIC8+CiAgICAgICAgPHRpdGxlPmUtRmF0dXJhPC90aXRsZT4KPHN0eWxlIHR5cGU9InRleHQvY3NzIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5waCB7b3ZlcmZsb3c6aGlkZGVuO21heC1oZWlnaHQ6MjUwcHg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnBoNyB7dGV4dC1hbGlnbjpjZW50ZXI7bWFyZ2luLWJvdHRvbToxOHB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5waDggaW1nIHttYXJnaW4tYm90dG9tOiAxOHB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5waDEwIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZmxvYXQ6IGxlZnQ7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdpZHRoOiAyOTVweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWFyZ2luLXRvcDogMTRweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWFyZ2luLXJpZ2h0OiAxMnB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nOiA4cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9keSB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbjogN3B4IDAgMTBweCAwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXh0LWFsaWduOiBjZW50ZXI7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJhY2tncm91bmQtY29sb3I6ICNCQkJCQkI7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvbnQtZmFtaWx5OkFyaWFsLCBIZWx2ZXRpY2EsIHNhbnMtc2VyaWY7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvbnQtc2l6ZTogMTJweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1pbWFnZTogdXJsKGRhdGE6aW1hZ2UvcG5nO2Jhc2U2NCxpVkJPUncwS0dnb0FBQUFOU1VoRVVnQUFBQmNBQUFBWENBWUFBQURnS3RTZ0FBQUJDRWxFUVZSNDJwMVZXdzdESUF6TC9TL1oxMEUycFpLUlo4VUI5b0VLeGFTSmJkSjRudWR6SE1jWXVjWTczVnZCNVBvOHozY2UxM1VOTUIvU0Fkek9YdHozM1dhR3d6bHlYdUh6SGZEQXZjR3JBMXJ5TGoyWWh3dmFhY0ZVZ0YvTzJnYlhJRndWenp0OUJpMU9URGRYUi9DK1ZoRmRnQjNLK0lrS1k4WG5TbE5tcUIrcnFnOCt4SjVuY1diY01nNjJ6QVNpeXN4Vm9UcDBleGtuVnF3MzQ5aFZFYXE2NDd6eXVEUEFDRDdMcWhKdTlhWkc5Y1hPOXhCc2RnOSszQUtGWGE5Z0I2MTB4cGNXN1dUVmdJTTBBU2RtU1l2TG1DK1BvMlZMVUNjeVY4Rjl2QXplWmJ2YnpMUjVoZXZGREtyV2pPTy9FS3I1dWFILy9JQVZ4M29NV2hnODQ3MTY3eXo4QllKTWYyaXZWYkxKQUFBQUFFbEZUa1N1UW1DQyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0YWJsZSB7Zm9udC1zaXplOjEycHg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyIGEge3BvaW50ZXItZXZlbnRzOm5vbmU7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyT3V0ZXIgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbjogMCBhdXRvOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIEBtZWRpYSBzY3JlZW4gewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9keSB7Y29sb3I6ICM2NjY7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmRvY3VtZW50Q29udGFpbmVyIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXgtd2lkdGg6IDk0NXB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1pbi13aWR0aDogODUwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgb3ZlcmZsb3c6aGlkZGVuOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRleHQtYWxpZ246IGxlZnQ7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm94LXNpemluZzogYm9yZGVyLWJveDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBkaXNwbGF5OmlubGluZS1ibG9jazsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtd2Via2l0LWJveC1zaGFkb3c6IDAgMXB4IDRweCByZ2JhKDAsIDAsIDAsIDAuMyksIDAgMCA0MHB4IHJnYmEoMCwgMCwgMCwgMC4xKSBpbnNldDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtbW96LWJveC1zaGFkb3c6IDAgMXB4IDRweCByZ2JhKDAsIDAsIDAsIDAuMyksIDAgMCA0MHB4IHJnYmEoMCwgMCwgMCwgMC4xKSBpbnNldDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3gtc2hhZG93OiAwIDFweCA0cHggcmdiYSgwLCAwLCAwLCAwLjMpLCAwIDAgNDBweCByZ2JhKDAsIDAsIDAsIDAuMSkgaW5zZXQ7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246IHJlbGF0aXZlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6IDIwcHggMjBweCAyMHB4IDI4cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuZG9jdW1lbnRDb250YWluZXI6YmVmb3JlLCAuZG9jdW1lbnRDb250YWluZXI6YWZ0ZXIgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250ZW50OiAiIjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246IGFic29sdXRlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB6LWluZGV4OiAtMTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLXdlYmtpdC1ib3gtc2hhZG93OiAwIDAgMjBweCByZ2JhKDAsMCwwLDAuOCk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC1tb3otYm94LXNoYWRvdzogMCAwIDIwcHggcmdiYSgwLDAsMCwwLjgpOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3gtc2hhZG93OiAwIDAgMjBweCByZ2JhKDAsMCwwLDAuOCk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvcDogNTAlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3R0b206IDA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxlZnQ6IDEwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJpZ2h0OiAxMHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtbW96LWJvcmRlci1yYWRpdXM6IDEwMHB4IC8gMTBweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLXJhZGl1czogMTAwcHggLyAxMHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5kb2N1bWVudENvbnRhaW5lcjphZnRlciB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJpZ2h0OiAxMHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBsZWZ0OiBhdXRvOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAtd2Via2l0LXRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC1tb3otdHJhbnNmb3JtOiBza2V3KDhkZWcpIHJvdGF0ZSgzZGVnKTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLW1zLXRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC1vLXRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRyYW5zZm9ybTogc2tldyg4ZGVnKSByb3RhdGUoM2RlZyk7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9ICAgICAgICAKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjdXN0Qm9sdW17CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbi10b3A6IDI3cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbi1ib3R0b206IDE1cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5lZmF0dXJhTG9nbyB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dC1hbGlnbjpjZW50ZXI7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmVmYXR1cmFMb2dvIGltZ3sKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aWR0aDo3MHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5lZmF0dXJhTG9nbyBoMXsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb250LXNpemU6IDE0cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbGluZS1oZWlnaHQ6IDE0cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWFyZ2luOiA0cHggMCAwIDA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmt1dHUge3ZlcnRpY2FsLWFsaWduOiB0b3A7fQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmt1dHUgdGFibGV7ZmxvYXQ6bm9uZTt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5nb25kZXJpY2kgewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRpc3BsYXk6IGlubGluZS1ibG9jazsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nOiA4cHggOHB4IDhweCAwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJveC1zaXppbmc6IGJvcmRlci1ib3g7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmdvbmRlcmljaSAucGFydHlOYW1lIHtmb250LXdlaWdodDpib2xkO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmFsaWNpIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aWR0aDogMzcwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFkZGluZzogOHB4IDRweCA0cHggMDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3gtc2l6aW5nOiBib3JkZXItYm94OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hbGljaSAuY3VzdG9tZXJUaXRsZSB7Zm9udC13ZWlnaHQ6Ym9sZDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5hbGljaSAua3V0dSB7IGJvcmRlcjoxcHggc29saWQgI0NDQzsgYmFja2dyb3VuZC1jb2xvcjojRjRGNEY0O30KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNFVFROIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXJnaW4tdG9wOiA3cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcGFkZGluZzogOHB4IDRweCA0cHggMDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI2Rlc3BhdGNoVGFibGUgLnBsYWNlSG9sZGVyLmNvbXBhbnlMb2dvIGltZyB7bWFyZ2luLWJvdHRvbToxOHB4O30KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICN0b3BsYW1sYXJDb250YWluZXIge292ZXJmbG93OmhpZGRlbjt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIge2Zsb2F0OnJpZ2h0O3dpZHRoOiAxMDAlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciB0ciB7dGV4dC1hbGlnbjpyaWdodDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIgdGgge2ZvbnQtd2VpZ2h0Om5vcm1hbDt0ZXh0LWFsaWduOnJpZ2h0O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciB0YWJsZSB7d2lkdGg6MjM4cHg7bWFyZ2luLXRvcDogMTRweDtib3JkZXItc3BhY2luZzowO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciB0YWJsZSB0ZCB7Zm9udC13ZWlnaHQ6IGJvbGQ7IHdpZHRoOjI1JTsgd2hpdGUtc3BhY2U6bm93cmFwO21pbi13aWR0aDogNTVweDsgdmVydGljYWwtYWxpZ246IHRvcDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRoLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRkewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjY2NjOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci1yaWdodDogMXB4IHNvbGlkICNjY2M7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLWxlZnQ6IDFweCBzb2xpZCAjY2NjOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6M3B4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIgdGFibGUgdGgge3doaXRlLXNwYWNlOm5vd3JhcDtib3JkZXItcmlnaHQ6IG5vbmU7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRoLmlzLWxvbmctdHJ1ZSwKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyID4gdGQgLmlzLWxvbmctdHJ1ZSB7d2hpdGUtc3BhY2U6cHJlLWxpbmU7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRyOmZpcnN0LWNoaWxkIHRoLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAudG9wbGFtbGFyIHRhYmxlIHRyOmZpcnN0LWNoaWxkIHRkewogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItdG9wOjFweCBzb2xpZCAjY2NjOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1sYXIgdGFibGUgdGQgc3BhbiB7Zm9udC13ZWlnaHQ6bm9ybWFsO2ZvbnQtc2l6ZTogMTJweDtjb2xvcjogIzdDN0M3Qzt9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ci5wYXlhYmxlQW1vdW50IHRoOmZpcnN0LWNoaWxkIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjZmNmY2OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ci5wYXlhYmxlQW1vdW50IHRkIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI2Y2ZjZmNjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnRvcGxhbWxhciA+IGRpdiB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGlzcGxheTogaW5saW5lLWJsb2NrOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC50b3BsYW1UYWJsb3sKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXJnaW4tbGVmdDogMzFweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmbG9hdDpyaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgfQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI25vdGxhciB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1hcmdpbi10b3A6IDE0cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci10b3A6IDFweCBzb2xpZCAjREREOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBvdmVyZmxvdzogaGlkZGVuOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nLXRvcDogOHB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nLWJvdHRvbTogMnB4OwoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNub3RsYXIgdGFibGUge2JvcmRlcjpub25lO2JhY2tncm91bmQ6bm9uZTt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuY2xlYXJmaXg6YmVmb3JlLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmNsZWFyZml4OmFmdGVyIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY29udGVudDoiIjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZGlzcGxheTp0YWJsZTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuYm94e2Rpc3BsYXk6IGlubGluZS1ibG9jazsqZGlzcGxheTogaW5saW5lO3pvb206IDE7d2lkdGg6IDMzJTsgYm94LXNpemluZzpib3JkZXItYm94OyB2ZXJ0aWNhbC1hbGlnbjogdG9wO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNiMXt3aWR0aDogNDAlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNiMnt3aWR0aDogMjUlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNiM3t3aWR0aDogMzUlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNrdW55ZSB7Ym9yZGVyLXNwYWNpbmc6MDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAja3VueWUgdHJ7IGJvcmRlci1ib3R0b206IDFweCBkb3R0ZWQgI0NDQzt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAja3VueWUgdGR7IGJvcmRlcjoxcHggc29saWQgI0NDQztib3JkZXItdG9wOiBub25lO3BhZGRpbmc6M3B4O3dpZHRoOiAxMDAlO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNrdW55ZSB0aHt2ZXJ0aWNhbC1hbGlnbjp0b3A7Zm9udC13ZWlnaHQ6Ym9sZDtwYWRkaW5nOjNweDt3aGl0ZS1zcGFjZTogbm93cmFwO2JvcmRlcjoxcHggc29saWQgI2NjYztib3JkZXItdG9wOiBub25lO2JvcmRlci1yaWdodDogbm9uZTt0ZXh0LWFsaWduOmxlZnQ7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI2t1bnllIHRyOmZpcnN0LWNoaWxkIHRke2JvcmRlci10b3A6IDFweCBzb2xpZCAjY2NjO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNrdW55ZSB0cjpmaXJzdC1jaGlsZCB0aHtib3JkZXItdG9wOiAxcHggc29saWQgI2NjYzt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAja3VueWUgdGQ6bnRoLWNoaWxkKDIpIHt3b3JkLWJyZWFrOiBicmVhay1hbGw7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnNhdGlybGFyIHttYXJnaW4tdG9wOjIwcHg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgd2lkdGg6MTAwJTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9udC1mYW1pbHk6ICJMdWNpZGEgU2FucyBVbmljb2RlIiwgIkx1Y2lkYSBHcmFuZGUiLCBTYW5zLVNlcmlmOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBiYWNrZ3JvdW5kOiAjZmZmOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItY29sbGFwc2U6IGNvbGxhcHNlOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXh0LWFsaWduOiByaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvbnQtd2VpZ2h0OiBub3JtYWw7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6IDJweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dC1hbGlnbjpyaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb2xvcjogYmxhY2s7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmctbGVmdDogM3B4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItYm90dG9tOiAycHggc29saWQgI0FBQUFBQTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI0RGREZERjsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLXJpZ2h0OiAxcHggc29saWQgI0I4QjhCODsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICNDNUM1QzU7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmctcmlnaHQ6IDVweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdmVydGljYWwtYWxpZ246IG1pZGRsZTsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbWluLWhlaWdodDogMzVweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0cjpmaXJzdC1jaGlsZCA+IHRoOmZpcnN0LWNoaWxkIHtib3JkZXItbGVmdDogMXB4IHNvbGlkICNCOEI4Qjg7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyOmZpcnN0LWNoaWxkID4gdGgubWhDb2x1bW4ge21pbi13aWR0aDo3MnB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0aCA+IC50aFRvcFRpdGxlIHt0ZXh0LWFsaWduOmNlbnRlcjt3aWR0aDogODlweDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdGggLnRoU3ViVGl0bGUge3dpZHRoOiA0N3B4OyBkaXNwbGF5OiBpbmxpbmUtYmxvY2s7dGV4dC1hbGlnbjogcmlnaHQ7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRoIC50aFN1YlRpdGxlLkhGIHt3aWR0aDozNnB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRoLmFsaWduTGVmdCB7dGV4dC1hbGlnbjpsZWZ0O3dpZHRoOiAyMiU7fQoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRkIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBwYWRkaW5nLWxlZnQ6M3B4OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2NjYzsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY29sb3I6ICMwMDA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvcmRlci1yaWdodDogMXB4IHNvbGlkICNjY2M7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmctcmlnaHQ6IDNweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGV4dC1hbGlnbjpyaWdodDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaGVpZ2h0OjI1cHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdHIgPiB0ZC5pc2tvbnRvT3Jhbmkge3BhZGRpbmctbGVmdDowOyBwYWRkaW5nLXJpZ2h0OjA7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyID4gdGQuaXNrb250b09yYW5pIHRke3RleHQtYWxpZ246IGNlbnRlcjt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdHI6aG92ZXIgPiB0ZCB7Ym9yZGVyLXJpZ2h0OiAxcHggc29saWQgIzk2OTY5Njtib3JkZXItYm90dG9tOiAxcHggc29saWQgIzk2OTY5Njtib3JkZXItdG9wOiAxcHggc29saWQgIzk2OTY5Njt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSA+IHRib2R5ID4gdHIgPiB0ZC53cmFwIHt3aGl0ZS1zcGFjZTpub3JtYWw7dGV4dC1hbGlnbjpsZWZ0O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0ciA+IHRkOmZpcnN0LWNoaWxkLAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyID4gdGg6Zmlyc3QtY2hpbGQge2JvcmRlci1sZWZ0OiAxcHggc29saWQgI0I4QjhCODt9CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI21hbEhpem1ldFRhYmxvc3UgPiB0Ym9keSA+IHRyOmhvdmVyID4gdGQKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI0QxRDFEMSAhaW1wb3J0YW50OwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjdXJzb3I6ZGVmYXVsdDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0cjpudGgtY2hpbGQoZXZlbikgPiB0ZCB7YmFja2dyb3VuZC1jb2xvcjogI0ZGRn0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICNtYWxIaXptZXRUYWJsb3N1ID4gdGJvZHkgPiB0cjpudGgtY2hpbGQob2RkKSA+IHRkIHtiYWNrZ3JvdW5kLWNvbG9yOiAjRUVFfQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLnN1bVZhbHVlIHt3aGl0ZS1zcGFjZTpub3dyYXA7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmlza29udG9PcmFuaUhlYWRlciwKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlEZWdlcmxlciB7d2lkdGg6MTAwJTtib3JkZXItc3BhY2luZzowO30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlIZWFkZXIgdGQge2JvcmRlci10b3A6IDFweCBzb2xpZCAjOTY5Njk2O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlIZWFkZXIgdGQsCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAuaXNrb250b09yYW5pRGVnZXJsZXIgdGQKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHtib3JkZXItbGVmdDogMXB4IHNvbGlkICM5Njk2OTY7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLmlza29udG9PcmFuaUhlYWRlciB0ZDpmaXJzdC1jaGlsZCwKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5pc2tvbnRvT3JhbmlEZWdlcmxlciB0ZDpmaXJzdC1jaGlsZCB7Ym9yZGVyLWxlZnQ6bm9uZTt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjaWhyYWNhdEJpbGdpbGVyaXsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgYm9yZGVyOjFweCBzb2xpZCAjQ0NDOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBtYXJnaW4tdG9wOjEwcHg7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHBhZGRpbmc6MTBweDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgcG9zaXRpb246cmVsYXRpdmU7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIEBtZWRpYSBwcmludCB7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJvZHkge2NvbG9yOiAjMDAwO3RleHQtYWxpZ246IGxlZnQ7YmFja2dyb3VuZDpub25lO2JhY2tncm91bmQtY29sb3I6I2ZmZmZmZjttYXJnaW46MDt9CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC5kb2N1bWVudENvbnRhaW5lciB7cGFkZGluZzowO21pbi1oZWlnaHQ6IGluaXRpYWw7d2lkdGg6IDg0NXB4ICFpbXBvcnRhbnQ7fQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAjbWFsSGl6bWV0VGFibG9zdSB7d2lkdGg6IDg0NXB4O30KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgCjwvc3R5bGU+Cgo8L2hlYWQ+Cjxib2R5PgogICAgICAgIDxkaXYgY2xhc3M9ImRvY3VtZW50Q29udGFpbmVyT3V0ZXIiPgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0iZG9jdW1lbnRDb250YWluZXIiPgogICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9IiRYTUwiPgogICAgICAgIDxkaXYgaWQ9InVzdEJvbHVtIj4KICAgICAgICAKICAgICAgICA8ZGl2IGlkPSJiMSIgY2xhc3M9ImJveCI+CiAgICAgICAgCiAgICAgICAgPGRpdiBpZD0iQWNjb3VudGluZ1N1cHBsaWVyUGFydHkiIGNsYXNzPSJnb25kZXJpY2kga3V0dSI+CiAgICAgICAgCiAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkiPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9InBhcnR5TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJub3QoY2FjOlBlcnNvbi9jYmM6Rmlyc3ROYW1lID0nJykgb3Igbm90KGNhYzpQZXJzb24vY2JjOkZhbWlseU5hbWUgPScnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBlcnNvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGl0bGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Rmlyc3ROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOk1pZGRsZU5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6RmFtaWx5TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpOYW1lU3VmZml4Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0ibm90KGNhYzpQYXJ0eU5hbWUvY2JjOk5hbWUgPScnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlBhcnR5TmFtZS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L3hzbDpmb3ItZWFjaD4KCjx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFjY291bnRpbmdTdXBwbGllclBhcnR5Ij4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJhZGRyZXMiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UG9zdGFsQWRkcmVzcyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlJlZ2lvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpTdHJlZXROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkJ1aWxkaW5nTmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6QnVpbGRpbmdOdW1iZXIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpCdWlsZGluZ051bWJlciI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpSb29tIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkthcMSxIE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6RGlzdHJpY3QgIT0nJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6RGlzdHJpY3QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpQb3N0YWxab25lICE9ICcnICI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6UG9zdGFsWm9uZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpDaXR5U3ViZGl2aXNpb25OYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiAvIDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkNpdHlOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CjwveHNsOmZvci1lYWNoPgoKPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkiPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9InRlbEZheCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpDb250YWN0Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRlbGVwaG9uZSAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRlbDogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlRlbGVwaG9uZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGVsZWZheCAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBGYWtzOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGVsZWZheCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L3hzbDpmb3ItZWFjaD4KCjx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdTdXBwbGllclBhcnR5L2NhYzpQYXJ0eS9jYmM6V2Vic2l0ZVVSSSI+CiAgICAgICAgPGRpdiBjbGFzcz0iV2Vic2l0ZVVSSSI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+V2ViIFNpdGVzaTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC9kaXY+CjwveHNsOmZvci1lYWNoPgoKPHhzbDpmb3ItZWFjaAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkvY2FjOlBhcnR5L2NhYzpDb250YWN0L2NiYzpFbGVjdHJvbmljTWFpbCI+CiAgICAgICAgPGRpdiBjbGFzcz0iZU1haWwiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PmUtUG9zdGE6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgIDwvZGl2Pgo8L3hzbDpmb3ItZWFjaD4KCjxkaXYgY2xhc3M9InRheE9mZmljZSI+CiAgICAgICAgPHhzbDp0ZXh0PlZlcmdpIERhaXJlc2k6IDwveHNsOnRleHQ+CiAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nU3VwcGxpZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5VGF4U2NoZW1lL2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4KPC9kaXY+Cgo8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQYXJ0eUlkZW50aWZpY2F0aW9uL2NiYzpJRCI+CiAgICAgICAgPHhzbDppZiB0ZXN0PSJAc2NoZW1lSUQgIT0gJ01VU1RFUklOTyciPgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0icGFydHlJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAc2NoZW1lSUQgPSAnVElDQVJFVFNJQ0lMTk8nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UaWNhcmV0IFNpY2lsIE5vPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAc2NoZW1lSUQgPSAnTUVSU0lTTk8nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NRVJTxLBTIE5vPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9IkBzY2hlbWVJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+OiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgPC94c2w6aWY+CjwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkvY2FjOlBhcnR5L2NhYzpBZ2VudFBhcnR5Ij4KPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkiPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWdlbnRQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0ic3ViZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+xZ51YmU6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlBhcnR5TmFtZS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ1N1cHBsaWVyUGFydHkvY2FjOlBhcnR5L2NhYzpBZ2VudFBhcnR5L2NhYzpQYXJ0eUlkZW50aWZpY2F0aW9uIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9ImNiYzpJRC9Ac2NoZW1lSUQgPSAnU1VCRU5PJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IC0gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC94c2w6Zm9yLWVhY2g+CjwveHNsOmlmPiAKCjwvZGl2PgoKPGRpdiBjbGFzcz0iYWxpY2kga3V0dSI+CiAgICAgICAgCiAgICAgICAgPGRpdiBjbGFzcz0iY3VzdG9tZXJUaXRsZSI+CiAgICAgICAgPHhzbDp0ZXh0PlNBWUlOPC94c2w6dGV4dD4KPC9kaXY+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCA9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VW52YW7EsTogPC94c2w6dGV4dD4KICAgICAgICA8L2I+CiAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QnV5ZXJDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6UGFydHlOYW1lL2NiYzpOYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIiAvPgogICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDxiciAvPgogICAgICAgIDxiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsSwbGk6IDwveHNsOnRleHQ+CiAgICAgICAgPC9iPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkJ1eWVyQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBvc3RhbEFkZHJlc3MvY2JjOkNpdHlOYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPGJyIC8+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+xLBsw6dlc2k6IDwveHNsOnRleHQ+CiAgICAgICAgPC9iPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkJ1eWVyQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBvc3RhbEFkZHJlc3MvY2JjOkNpdHlTdWJkaXZpc2lvbk5hbWUiPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8YnIgLz4KICAgICAgICA8Yj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5Tb2thazogPC94c2w6dGV4dD4KICAgICAgICA8L2I+CiAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QnV5ZXJDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6UG9zdGFsQWRkcmVzcy9jYmM6U3RyZWV0TmFtZSI+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDxiciAvPgogICAgICAgIDxiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsOcbGtlc2k6IDwveHNsOnRleHQ+CiAgICAgICAgPC9iPgogICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkJ1eWVyQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBvc3RhbEFkZHJlc3MvY2FjOkNvdW50cnkvY2JjOk5hbWUiPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8YnIgLz4KICAgICAgICA8Yj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5SZXNtaSBVbnZhbsSxOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQYXJ0eUxlZ2FsRW50aXR5L2NiYzpSZWdpc3RyYXRpb25OYW1lIj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPGJyIC8+CiAgICAgICAgPGI+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VmVyZ2kgTnVtYXJhc8SxOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvYj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCdXllckN1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpQYXJ0eUxlZ2FsRW50aXR5L2NiYzpDb21wYW55SUQiPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnSUhSQUNBVCciPgogICAgICAgIDxkaXYgY2xhc3M9InBhcnR5TmFtZSI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UGFydHkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0icGFydHlOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJub3QoY2FjOlBlcnNvbi9jYmM6Rmlyc3ROYW1lID0nJykgb3Igbm90KGNhYzpQZXJzb24vY2JjOkZhbWlseU5hbWUgPScnKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQZXJzb24iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpUaXRsZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpGaXJzdE5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6TWlkZGxlTmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpGYW1pbHlOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOk5hbWVTdWZmaXgiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJub3QoY2FjOlBhcnR5TmFtZS9jYmM6TmFtZSA9JycpIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlBhcnR5TmFtZS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvZGl2Pgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCA9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIj4KICAgICAgICA8ZGl2IGNsYXNzPSJwYXJ0eU5hbWUiPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBhcnR5Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VW52YW7EsTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpQYXJ0eU5hbWUvY2JjOk5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6UG9zdGFsQWRkcmVzcyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U29rYWsgQWTEsTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlN0cmVldE5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5CaW5hIEFkxLE6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpCdWlsZGluZ05hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5CaW5hIE5vOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6QnVpbGRpbmdOdW1iZXIiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7EsGzDp2U6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvYj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpDaXR5U3ViZGl2aXNpb25OYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+xLBsOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6Q2l0eU5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5Qb3N0YSBLb2R1OiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6UG9zdGFsWm9uZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsOcbGtlOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6Q291bnRyeS9jYmM6TmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8Yj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5WZXJnaSBEYWlyZXNpOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpQYXJ0eVRheFNjaGVtZS9jYWM6VGF4U2NoZW1lL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpDb250YWN0Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZWxlZm9uOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6VGVsZXBob25lIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RXBvc3RhOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYmM6RWxlY3Ryb25pY01haWwiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvZGl2Pgo8L3hzbDppZj4KCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ1lPTENVQkVSQUJFUkZBVFVSQScgYW5kIC8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGRpdiBjbGFzcz0iYWRkcmVzIj4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOlBvc3RhbEFkZHJlc3MiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlJlZ2lvbiI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6U3RyZWV0TmFtZSAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlN0cmVldE5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6QnVpbGRpbmdOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpCdWlsZGluZ051bWJlciAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkJ1aWxkaW5nTnVtYmVyIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpSb29tICE9JyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Um9vbSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkthcMSxIE5vOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpEaXN0cmljdCAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkRpc3RyaWN0Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlBvc3RhbFpvbmUgIT0gJycgIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlBvc3RhbFpvbmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6Q2l0eVN1YmRpdmlzaW9uTmFtZSAhPSAnJyAiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Q2l0eVN1YmRpdmlzaW9uTmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pi8gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpDaXR5TmFtZSAhPSAnJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpDaXR5TmFtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvZGl2Pgo8L3hzbDppZj4KCgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NhYzpDb250YWN0L2NiYzpUZWxlcGhvbmUgIT0nJyBvciAvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6Q29udGFjdC9jYmM6VGVsZWZheCAhPScnIj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnWU9MQ1VCRVJBQkVSRkFUVVJBJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ0lIUkFDQVQnIj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5Ij4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpQYXJ0eSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0idGVsRmF4Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6Q29udGFjdCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRlbGVwaG9uZSAhPScnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZWw6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGVsZXBob25lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRlbGVmYXggIT0nJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEZha3M6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6VGVsZWZheCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9kaXY+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC94c2w6Zm9yLWVhY2g+CjwveHNsOmlmPgoKPC94c2w6aWY+IAo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NiYzpXZWJzaXRlVVJJICE9ICcnIj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnWU9MQ1VCRVJBQkVSRkFUVVJBJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ0lIUkFDQVQnIj4KICAgICAgICA8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHkvY2FjOlBhcnR5L2NiYzpXZWJzaXRlVVJJIj4KICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9IldlYnNpdGVVUkkiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+V2ViIFNpdGVzaTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgIDwveHNsOmZvci1lYWNoPgo8L3hzbDppZj4KCjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkFjY291bnRpbmdDdXN0b21lclBhcnR5L2NhYzpQYXJ0eS9jYWM6Q29udGFjdC9jYmM6RWxlY3Ryb25pY01haWwgIT0gJyciPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnSUhSQUNBVCciPgogICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOkNvbnRhY3QvY2JjOkVsZWN0cm9uaWNNYWlsIj4KICAgICAgICAgICAgICAgIDxkaXYgY2xhc3M9ImVNYWlsIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PmUtUG9zdGE6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC94c2w6aWY+Cgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQgIT0gJ1lPTENVQkVSQUJFUkZBVFVSQScgYW5kIC8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGRpdiBjbGFzcz0idGF4T2ZmaWNlIj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5WZXJnaSBEYWlyZXNpOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5VGF4U2NoZW1lL2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4KICAgICAgICA8L2Rpdj4KPC94c2w6aWY+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCAhPSAnSUhSQUNBVCciPgogICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eS9jYWM6UGFydHkvY2FjOlBhcnR5SWRlbnRpZmljYXRpb24iPgogICAgICAgICAgICAgICAgPGRpdiBjbGFzcz0icGFydHlJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJjYmM6SUQvQHNjaGVtZUlEID0gJ1RJQ0FSRVRTSUNJTE5PJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGljYXJldCBTaWNpbCBObzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iY2JjOklEL0BzY2hlbWVJRCA9ICdNRVJTSVNOTyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk1FUlPEsFMgTm88L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOklEL0BzY2hlbWVJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2hvb3NlPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+OiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOklEIi8+CiAgICAgICAgICAgICAgICA8L2Rpdj4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC94c2w6aWY+CgoKPC9kaXY+Cgo8ZGl2IGlkPSJFVFROIj4KICAgICAgICAKICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsgIj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5FVFROOiA8L3hzbDp0ZXh0PgogICAgICAgIDwvc3Bhbj4KICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlVVSUQiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3hzbDpmb3ItZWFjaD4KPC9kaXY+CgoKCjwvZGl2PgoKPGRpdiBpZD0iYjIiIGNsYXNzPSJib3giPgogICAgICAgIAogICAgICAgIDxkaXYgY2xhc3M9ImVmYXR1cmFMb2dvIj4KICAgICAgICAKICAgICAgICA8aW1nIHN0eWxlPSJ3aWR0aDo5MXB4OyIgYWxpZ249Im1pZGRsZSIgYWx0PSJFLUZhdHVyYSBMb2dvIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc3JjPSJkYXRhOmltYWdlL2pwZWc7YmFzZTY0LC85ai80UUFZUlhocFpnQUFTVWtxQUFnQUFBQUFBQUFBQUFBQUFQL3NBQkZFZFdOcmVRQUJBQVFBQUFCa0FBRC80UU1aYUhSMGNEb3ZMMjV6TG1Ga2IySmxMbU52YlM5NFlYQXZNUzR3THdBOFAzaHdZV05yWlhRZ1ltVm5hVzQ5SXUrN3Z5SWdhV1E5SWxjMVRUQk5jRU5sYUdsSWVuSmxVM3BPVkdONmEyTTVaQ0kvUGlBOGVEcDRiWEJ0WlhSaElIaHRiRzV6T25nOUltRmtiMkpsT201ek9tMWxkR0V2SWlCNE9uaHRjSFJyUFNKQlpHOWlaU0JZVFZBZ1EyOXlaU0ExTGpZdFl6RXpNaUEzT1M0eE5Ua3lPRFFzSURJd01UWXZNRFF2TVRrdE1UTTZNVE02TkRBZ0lDQWdJQ0FnSUNJK0lEeHlaR1k2VWtSR0lIaHRiRzV6T25Ka1pqMGlhSFIwY0RvdkwzZDNkeTUzTXk1dmNtY3ZNVGs1T1M4d01pOHlNaTF5WkdZdGMzbHVkR0Y0TFc1ekl5SStJRHh5WkdZNlJHVnpZM0pwY0hScGIyNGdjbVJtT21GaWIzVjBQU0lpSUhodGJHNXpPbmh0Y0UxTlBTSm9kSFJ3T2k4dmJuTXVZV1J2WW1VdVkyOXRMM2hoY0M4eExqQXZiVzB2SWlCNGJXeHVjenB6ZEZKbFpqMGlhSFIwY0RvdkwyNXpMbUZrYjJKbExtTnZiUzk0WVhBdk1TNHdMM05VZVhCbEwxSmxjMjkxY21ObFVtVm1JeUlnZUcxc2JuTTZlRzF3UFNKb2RIUndPaTh2Ym5NdVlXUnZZbVV1WTI5dEwzaGhjQzh4TGpBdklpQjRiWEJOVFRwRWIyTjFiV1Z1ZEVsRVBTSjRiWEF1Wkdsa09qWkROREpCTkVJMlFqVkNSREV4UlRoQ1FqTTBSRUl3UWtaR01FUXhPRFkwSWlCNGJYQk5UVHBKYm5OMFlXNWpaVWxFUFNKNGJYQXVhV2xrT2paRE5ESkJORUkxUWpWQ1JERXhSVGhDUWpNMFJFSXdRa1pHTUVReE9EWTBJaUI0YlhBNlEzSmxZWFJ2Y2xSdmIydzlJa0ZrYjJKbElGQm9iM1J2YzJodmNDQkRVelFnVjJsdVpHOTNjeUkrSUR4NGJYQk5UVHBFWlhKcGRtVmtSbkp2YlNCemRGSmxaanBwYm5OMFlXNWpaVWxFUFNJelJFVkVOa1UxTjBGRFJFVkRORUpCTnpreE5VTTJNME5DTjBSRU56TTBOeUlnYzNSU1pXWTZaRzlqZFcxbGJuUkpSRDBpTTBSRlJEWkZOVGRCUTBSRlF6UkNRVGM1TVRWRE5qTkRRamRFUkRjek5EY2lMejRnUEM5eVpHWTZSR1Z6WTNKcGNIUnBiMjQrSUR3dmNtUm1PbEpFUmo0Z1BDOTRPbmh0Y0cxbGRHRStJRHcvZUhCaFkydGxkQ0JsYm1ROUluSWlQejcvN2dBT1FXUnZZbVVBWk1BQUFBQUIvOXNBaEFBQkFRRUJBUUVCQVFFQkFRRUJBUUVCQVFFQkFRRUJBUUVCQVFFQkFRRUJBUUVCQVFFQkFRRUJBUUVCQWdJQ0FnSUNBZ0lDQWdJREF3TURBd01EQXdNREFRRUJBUUVCQVFJQkFRSUNBZ0VDQWdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3TURBd01EQXdNREF3UC93QUFSQ0FCbUFHa0RBUkVBQWhFQkF4RUIvOFFBdHdBQUFnTUFBUVVCQUFBQUFBQUFBQUFBQ0FrQUJ3b0dBUUlFQlFzREFRQUJCQUlEQVFBQUFBQUFBQUFBQUFBR0FBUUZCd2dKQVFJRENoQUFBQVlCQXdNQ0F3VUhBd1FEQUFBQUFRSURCQVVHQndBUkNDRVNFeFFKTVNJVlFWRXlJeGJ3WVhHQm9SY0trYkhCMFZJekpFSTBKeEVBQWdFQ0JBSUhCQWNHQkFRSEFBQUFBUUlERVFRQUlSSUZNUVpCVVdFaU1oTUhjWUVVQ0pHaHNjRkNJeFh3MFZKaU13bHlVM01rNFlLaUZ0SkRnOU5VSlJmLzJnQU1Bd0VBQWhFREVRQS9BTi9HbGhZbWxoWW1saFlwck1uSUxEWEg2QkpZc3ZaRHJsS1pPRCtDTFp5YjBocHl3UFRiQWxHVnF1dGdYbkxGS0xHTUFFYnNtNjZ4aEg4TzNYVWp0dTFiaHUwL2tXTVpkcVZKcUFCN1NTQVBwejZNTmJpOHQ3WmRVelU5eCs0SEE0bzhrT1JHV2E5TFQrRGVQQ21PNnl5S2RSdGtMbDNLeW1JR1QxZ2tReXptYWo4YlJVSlpNZ3B4YURiODBEektVRVl3RkVERktVTy9VcCtrN1ZZMGl2cmxwTDZ0RERGSHFvYStFeWFxVjdBRDFZYkM0dTdnRm9VQ1JVeVlrZlRwcDl1QjRYdUhKRzE1WGlzTldmM0RNYzQ4djFoV1pOb2lBd2p4TmVTVlpkeUV0VDVISVViVm0rWE1xU0Y3b2J1NXZhQkVPSnBDTUk3YnlxMFVUMWhHZnB6RU9NMllOdEcxbmNZdG5ZMjhkU3p0Y25YUU9JeTVpcnJDQ1JoSHIwYU5aMGF0V1dHekM0YWNRUGNqV2VIY3lyU3RLOEswRmFWclRPbE1BaHljem55SDQ3NVM1QjQ4Y2NzK1RGdW44WTR3cjl1eGlrd0RBTUFYTE4vZXY4VHRMTFRFNDVmQnNqRlZpTnJUSE04UEtLdWpMdVZQcHlMOVVVQ3BNanJHSzlpMlBaZDZzYk85K0dqaWludUNrcExGaERIU1VySjBGeXhna1VMUWQ3UU5STGdDUHVwSjdhU1NMV1daVXF1WGlQZHFPb1UxS2E1NVZOS0RCZVJPUU9WZGR0dEJ4N0VjOXEvS1pDdmRMaXJkQ1Zqa2R4Q1ZTbzh1dXZRM2VTSmVzUU9ac1hwNHFyTXBMUkZTaW5yOWZzTzRYUVp0VjFUb0dNZ3NrbU5HejJ0cldhOS9TeEpheFNGV1pMbzYwR3NScXpRaHRTZ3VWVUVycDFNQlUxRlgycTRqZFVFNUVqQ29CVEk1RWtCaUtFZ0FtbkdnN01jcnhCN2hYSkZ4VWF2ZE1yOFJaL0l0RnRHUHFYbEpua1BpZzdsNys2YVVUSVNFbTZwMDdONGp1VVBVcndVOHl3aUZYWHBJVmFlZHBJR0lmeEdUVVRPZHB1UEt1M1c3dmJ4M2F4N2trMGtSaFpTUUpJMkFkUE5xRjd0ZkVSUW51MXFEam0zM0c3S2g1SXkwQlVOcXFCM1RtRFFEOXVPR0I0TDVSWUY1S1JUcVN3M2ttdlc1ekZIOEZpckpGMUl1NzFGNlhZcWtkY0tSTHBNYlZWbjZSeDdSU2V0RURDUHczRHFJdHVXejdsdFRoTDJNeDFGUWFoZ2ZlQ1JYczQ4TXN4aVR0NzIzdUZEUk5VKy83d01YL3FOdzZ4TkxDeE5MQ3hOTEN4TkxDeDBIL1lQMjY2V0ZoZGVVK1dWdHlMa2xiamp4SGNWQmU3bG1WcW5lTTczaDBpT01jWFQ2VVU5bXBHcDFlSEZ5eWU1bnpKSFY1Z3ZJZnB5TVdJaXhhb0hXa1hUVk11eGpPeTVmZ3NiSDlZMzRQNVFYVWtLaGlaRnFGSmFSQ2ZLQVowOFlGU1F0UVRpSG52SkxpUVcxa1FHSnpiTExwOExEUGdlSHR3QVkzekd1UFdybTU0UHJlVXM4OG5KZVdqSmNuS25MVkxZWkR0bVVNVlFsMlBSTTJYSGpUQnNuRmtOQ1J1R2JPbzJhVDlkWTE2T2V4VVdxZVJUaVpZaUJCV0tmZ2J1OTBXMjd5UXdiTEdyQmJjUHA4aVZvL01oam5rS0pRVHFDMGJlWVVacUlYak9hczBNVUExUUJudVNSVjZlSlFhTXlyVStBbnZDbFFNd0c2VDZzK0ljb2MwT0g5SGNXeWZtc0Y4aDFLcmFXak8waFZMRFhvb1hWZ2pMQmptMUdzK0pKcVpaVERqSGVWcVk2VWVwd2MyWnJNUlpIak55ZEtQbG1KVTI0ckJ1Tmh5enpIS0lFUzcyYnpGcW10V2FpbFpGMFRCU29saWNCZk1qQlI5TEtDOFRuVkpORExlMlM2aVk3bWh6b1FNd1ZOVnI0V0dlazVpb09UREs0b2Joamh5RnpORDU1WUoyQ092N0NFcHNWTGhCeWdRa0JhSGxDcVR1a1ZpV25XclJ1TTI0Q09yVDBXMzA0SkVJaHlWQnNvNWF1Rm1yZFZPTWs1azNLWGJHMmx5aldaWnl1b2FtUVNPSFlLVDNSVmhYVnAxaXJCV0FaZ1hBc29WbUZ3S2lTZzRaQTBGQlhwNGRGYWNNc2hqenNtOEx1TitYcm4vY0srWStKTVhBejZ3U1NrMEU1WVc2eWo2ellsZllRa2x3Ym95Z00yLzhBK2RQek5raUlwcEpwT2swbmdGOVdrUmN2Rmp6TnZXM1d4czdTYlRiRlZYVHBVNUxNSndNeFgrcUFhbk1pcStFa1lVdGxiVE9KSkZxNEpOYW5pVktIL3BOUHI0NHErMSszZGhHWGtibllxaEszakc5c3QxVHlCV1VwK3ZTektSV3I3ekkySjRUREV4YVluOVJ4c3JJRnNEQ2xWeG1SaVk3b1VHaXlaanBwZ0N5eFZIdHZ6aHVrU1JRM0t4VDI4VWtiYVdCQVlSeXRNRWJTVkdrdXphcUNwQjQ1Q25rKzN3c1daQ3lPd0lxT2lxaGFpdGM2QVV3TTB6VGMxNGJ6Zlc4WjhkeVByRmszSUdVcnhrRzZQMzFmeVBDWU14bGcrSHd2SDRPd1V4dXM4b1JsV3JmQ1kxZ0dNYTlTclVXOTlmUFd0cXNkSUk5STd0OHptNGJuYk55MnQ3M2VTc2RqQmJwR2dEUm00bG5hWXp6bEZ6WkdrWXVESzY2WTRTQWRaQ0k3VmttaG5FVnVDWlhjazFCMEtvWFN0ZWcwRk82RFZtcWNoVWpqbEtqY1hjKzhoVDY3M0ZWeXdWbCtqUWtuSllvNWhZMGxIOU56Rkx0YXZZR2RUY3k5MVFqYVBDVStQYVd5VWNsa1dkVGN5OXlZTE1Dcm95S2NlL2FyTTB1OTR0MXlyWm9zVndrOWxNd0U5dlJkSTFLVzBKSVdhUmdvN3J2b2lHcWhRU0l3YytheHg3Zzdkd3BLdGRENTF5TktsYUJSWG9GV3k0NlNDQmRPTCtZMTd3amtOdngyNW95MVFtWFEydHBqdWljcnFBZG1qamk3Mjk3R3NabUlvT1phc3djUDFNQTVra1lXV1pyb01YNm9SVTBEa2gyQy9jWUVkUk45eS9EdU5vdTZiQ3JwcVRXMXVWYnVLQ1UxSkpJUjVvWmtid1ZvYXJ4QUdPOEY3TGF5QzF2YU5uUVBVWm1nTkNxaktsUng0OGNOSUFRTUFDQTdnUFVCRGZZUSt6K0lDSCt1Z3NtaE5lSXhOWTd0YzRXSnBZV09tL1hiU3AwNFdBYTVFWEc5WlZzVC9qbGgrMEtVR09hUnFjdnlMem8yY04ycXVJYUV1Z1o2RUJWSkIyUG9FY20yOWkyVUJGVllESlEwY0tqNVVPNEc1RGxtejI5dHRjVWU5WDYrWk14LzI4SXJXUWh0TE1TdGRPZzVxR1h2SGdPbkVWZE5MZE9iV0U2VUhqYnE2UmthVnIySExDKzVxM3dzdklZendYeFFNMXVIR1diWjNUR3VQWURqTzhNbGxXbFoyZ2tjZjNxSXo3bC9KdHRnVVQ0eWV3emh4S08wek95dkdjNUZlb2R1VFRha3N6aWt5cUcxZUtPYmM5K1VSYjJubHl5TmNnQ0tTQStaRVlJb295UE9MMFFNUlIxa29CNVhsdktXcGFwV0MxT3EzTlZHanhCc20xTXg4Tk02RGdSL0ZxQzRhL2dqai9ENG5oVlg4OFNBc09TckhaSCtSYnJZWW1JY3hkWUpsQ3oxMkpnc2hXakhGVms1S2NMamRyZjNVWWVSbFdrZXVtazlsSDd4MG9IZTVVRFFGdTI3eWJoS0ZpMXBaUm9JNDFKQmZ5bFl0R3NqZ0w1cGpCQ3F6Q3FvcXFNbEdKYTN0MWhXclVNaE9va2NOUkZDVkJKMDE0a0RpU1R4SndSSmpGSVV4ekNCU2xBUk1ZM1FBQU9vaUkvWUFCcUdIYmh5QVRrTXpqT2g3bmZ1UFBUT1pmQmVDN1E4ZzBJWng0N3prS0Rrbk1hK0k4YmdndjhBUnE5S01GVzdwc2Rzb0JpdVZTSCtJQ1FQdDJxRG5ublJyUS9wMjFPVmtVOTV4MGNNZ0dVZ2p0Qi80N1FQbEUrVTIyM2lLTG52MUV0eExhVExXQzJZNUZhbFRJenczSVlOL0k2WkR0OE9lV1o1ZjUyVGVMZ255Z3phQlV6bkQ4dkt0MkFvQUJoRDhKWnJwOE52dTFWTGM0NytoN3QyNFBzWC93QU9ObGRwOHNYbzBZVkw4dTIxU1A4QU5uNlAvWHhVMDd6cjVJS1BtMEpXdVEyZnBpWWZycHMyTFpybEs5ckx1WEs1d1RTU1NicHpZblVPWTVnQU9uMjZVUE5mTkZ5NnhRM1Q2aWFjRTZmYW93eDNiMEcrWHpseXlrM0hkdGl0VXRvbEpidjNiVXAvZ2tZL1FEaDl2dG1ZRzVWejlvcm1WT1JQSWJrUTlGSlJHVWhzZEk1WHVpMElRaXlDZ3BGdGlEdVZXSS9NSUtGSHdCOGhSRDV1N1Z6Y3BiWHZ3Vkx6ZDdsbVBFSVF2dDRvMzNZMVIvTWo2bCtrVXBuNWM5TTlpZ3Q0eFZHbldXNEpORC9sM051Q09CNFA3OGFNTWc0NnRhMkpjdXR1T2k5SHhEbS9JVURKdVluSWJpblJyaEJTOUxORkVtTmp0YWJKcWtyTlNaUEljaWIxMm0vRnFxY0Z6dDNaQ0diTFhIdDkvYi9IMnJieUpibmE0bkFhUFdRZkxCelZTZkNPd0ZhOEF5azZocjRuaVl4eWZDNlVtWUdocDA5Wi9ZK3c4TUpsd2Z4MHhiUzdka2VENVFNbldPOFF5dFVrOFpTT0xzMFJGWnRtY2MwdWNzelVKYjdWazNPZVRjWjNpd3NiZmpMR3VZU1M1YW5mSnF0d2J4bThsVnpLUzZTQlVrbHJNM2plTHU5dElXMmZUY1grc1NlWkVXVzNoRVN0R3NVRU1zU2xKWklkQmxoamxrVWhGcEhxQkt3VUZyR2pzTG1xdzBwUnMzWXNReFptVmpWUTFkTEZRY3puVGljZkhhLzVHNHI1ZmgrRXZJS3hTOTFwdG9aeUw3aDNuK3hMQ3ZLWE9zd2pjRjNtQzhveTZvcGxkWmxvMGNrWlptOUVwQXNFT1FGZHZWSU9BTUk3cGEyMjlXRGN3YmFBczhkUGlZeFdpRm0wcTRMRWF0ZkVoUWFmaXpxUzl0WGt0SmxzWmpWR3JvUFhRVkl5NnUwK3pEUGR0dmhvTno5Mkpib3hPdjM2V09LSHJ4VEdmc3NKWWF4aFBYQkpyOVVzYXdzNjVSSzhVZjhBMkxUa0N5dWs0YW0xdG9uc1l5aXNyT3UwU0QyZ0pnVDdqYkR0dHFUMmpiLzFLOVdCaUJBQVdjbmhwWE05SU9mREk5UFZodGR6L0R3bHg0emtQYlVEdHdvUE03ZksrTHB6RkdJNUNhdU9CNXE2Mm13TThuOGxzbk1vZklIQy9QcWVWNm4zV2VuNVJwRVJMQ1pqYTdGbEI5SDFTdXRaZDlUWkZ2Q2xWV1lUQysvMHgzWW0xRGJyNks1M01KRmRoRVZrdG9TMGQ3QTBUWlNReUZmNmF4QjVYS0pPb09sWGlXZ2tTSWxFMEhsMjRKaXFTQzdVTVRCaHdZQThTeENpcFE4U0dQaExKK0svRnRsZ2hDelhTek9XTS9tTEpLcXIyNXpMVnBWMTQ2ck1YMWhuN3FwakNpV0tKeC9RTE5NNHlybHh0OHM1aWpXQk43TUVTZGdrczVPbWlpUk1LMzdmVzNVeDJzQUtiYkFLSXRYcTVDcW5uU0swa2lySzZJZ2Z5OUtWV29VRWttVnRMUVcrcVJ6V2QrSnl5RlNkSUlWU1ZCSnBxcWMrT0M4ME80ZURDbXZkTTVrbndGamxQR05Ka1NvWk15S3ljSmVvUlVURmV1VmczZTNleXhpR0lZU09IUWdaRnVicHNmY3dEOHVnTG5ubVA5SHNQaHJjMHZKUlFkZ3lxYzFJNmVHTXp2azc5Q1Q2bzg1RG1EZUl3M0xHM1NLV0JQOEFVbHowcjNab3BBQVFEcUFaYTVFSE9tSzNMdVJWbmF5OE94Y25PVVZGRE8xeFAzS0xyS2ozSFVPY2R6SE9jd2lJaUk5UkhXTTEzY00wbEs1OVAxZG1Qb0U1VTJDR3l0ME9taUtvQ2lwTkFBT25VYThPbkFmMktaZGVWR01qRTEza3JJTEp0V3JSdWtaZHk1ZExuOGFhU1NhZTUxRkZEanNBQUh4MTRXMXUwOHFRSjRtTkIrMWNTSE5XL3dCcnNHM1NYbHkybTNqUWxqUW5JQ3ZRckhnT2dIR2xUMnNQYStqNmRIbHpyblZpMS9VeWNjYWNWQ1VLYjBOSmgwU2VyVjNLcitUOVM4Q2Zjb29JZmwvaEFRNjZ5QjVQNVNnMmlBWDE3L1hwVThlN3cvaGNnL1JqUmg4elh6SWJ4Nm5jeEhrN2xadi9BS3N5R05SUkQ1ckUwcCtiYXhPbWY4OU8zQmpXejNvY0g4ZWNwd05MaDhSdlpiRjR5aG9lVnY2TXMzYnZmR2dxTFVaaVBpenMxUFVSNVRtQTRnS3laaFQzRUFFZGdIdEo2bDJkcnVTMlNSYXJRdFF2cVlkbmg4c242OFNlMGZJSnpYdm5JRW5ORjVmK1R2d2gxcmJpR0Z3MWFNQVpoZktneVBIUmwxVnhvenBWdXI5NnJFSGJLdTlSa0lPd1JiS1dqSGFCZ09tdXlmb0VjTmxTbUQvdVRVRC9BRjFhME15VHhMTkhtakNvOWh4cnEzWGJydmFOeG0yeStHbTZna0tNS2cwWWNjMUpCOXhJN2NML0FPZVhFYWlaTWpaYlBCNnZXckxaYWJYSTF4YzZsZkxsTTByRnQvcTFGU3Q1NGR4bEtYZ0tuZGJXV29ZNmhzZzJWN0lSY0MzWXViYkh1bFlpUldjc0RnME1kY3BjeDNkaEl1MUNTU09DUnpvZU5GZVdONU5GZktEUEdtdVF4UktyeUZoQ3dFc1lXUWFzRGU0V1VjeW1lZ0xBQ29KSVZnSzAxVUJOQUdhb0ZOUU9scWpMRk5VbkZlVytZbkZTMFV6azFjLzBweXp0TUxTT1FHUG03QzFVZHdHQnJ4R00vTGppeVk5cEVCRlJGNG9NTlZieEZMeDhxMW5EU2pweTRJOWFxU1RyeUxvcFA3NjgyM1lOL1M0MkNQWHNVVWp3NnlyZ3kwSnFaSkNXUjIwc3JLWXdxNmFmbHFEbjRKRExlMlpTOUlGMHdEVXFPNzdBS0VDb0l6cWE5SndibkNia1ErNUw4ZjZ6ZWJURnAxcktjQklUdU44MlV2WUNMMHpMK1BwVnpXYnZDcklCOHlEVnhJc2ZYc0JIL3dBc2E4YnFodVU0YUZ1WTlxajJqZHBMZUE2clE2V1E5WUtnMDRrNUVrWm1wQUI2Y1BOdXV2aXJaV2JLUVpIM1pkUTQwd1dtb1BEN0N0T1ZPYTZySGNzOFIxbTNoS3ZhUng0cXNkbk93d3RlakZaNmZ0V1dzcTNhT3dGeHhva0xBcG1JYVNzVmp0bG5rRkkwbTVTZzViZ2M1a3lsOHBEblpkcm5rMktVUklQaUwyb1IyT2xVamdEU1RPeDZFQ0s1Yi9EbFU4SWE1blg0NVMzZ2g0Z2NTWkFBb0hhV0lBNDl0TUhLaGxTTXNHVms4UnhyQ01jeWtOVFkyOTMrTHNUdVRoN0pYSWl3cnFwVUNUZ1lWZXRQWUc3UjBqTndVbTBrSERXV1MralBHU1JURlZPc0FFRnpZdkZZZnFEbGhHMGhTTXFBVllxUHpBemFneUVLeUZRVU9zTWVGTTVQelEwdmtpbFF0VFhpSzhLQ2xEbURYUEttTG0xSFk5c2VoczloamFuWFpxelRLNVdzVkF4anlWa0hCOSsxRm95UU91dWNlMHBqZkttUVIrQTY4cHBZNEkybms4Q0FrK3pFaHRXMjNPNzdsQnRkbU5WMWNTcWlqSVZaalFaa2dEMmtnZFp4Z3o1dDhsWmpNMlVNZ1pVazNhaC9ya282amF3M09jNGtZVmRrNFhSaDJxSkRBVVV3RnRzb2NBQUE4aWhoMWlwelp2Y3U3Yms5MmZBVFJSMUFVSEhTQ2ZlSzQra2I1Y2ZTcTA5TytROXY1YXRWMHlyR0htTlQzcEdJZG1JODJRQTUwb3JhY3FnQ3RNS1lucFl5YVRwKzVPSW5NQmo5eHVvaU93L2Z2b0pIZUk3Y1pWVE90bGJnZEFIMmZUaG0vdEVjTmxzNjVJVnpaY29WU1FnSzlJR2phV3pkcEZPMGVTNEFCblVvZE0rNEtGajB6N0pkTmdPTy93Qm1yazlPK1hUTEorcFRyM1FScHorbmczMmpHcGY1NVBYTjdSUCt3ZG5sbzdxVE9kUEFWR2tkKzNOYWl1YVNaVnp4cUw5eHR3NTQ4ZTMxa04xWHlLTTNFcVdFclVvNmJFMldUalp4K2t6ZjdtVCtZcFZFRkJLSS9adnF5K2Q3aDl2NWFsa2c0MEMxN0NSMTE5bU1IUGsrMlMwNXgrWURiTFBjUUhpQmxsVVpqdkpFN0tlNnljRFE1bWxlakdFelBtUUZiaS9pWXlMRlJ5Y1NKczJpQ1pEQ29zNmRMSmxBcFNpQUNKakcyQU5ZeFJOSmVYU0JSVnkzMys3SDBDNzVKYjhzYkJNOXdkS3JDU2VKOEs4Y3RmVWVHUG9WZTJPV3l4dkZuRk5jdFNpNmt0QjBtRVp1Z1hFZklSUWpSSWZDYmNSNm9GTUJCKzRRMWx4eStra08yUXhTWkhSMmZkajVsL1dHOHM5eDUrM0crcy82VXR3eEI3M1JSZnhBSE9uVU1NWEVwVEZFcGdBeFRBSlRGTUFDQmdFTmhBUUhvSUNHcHdaY01WYmhMZEx4WlRlRTNKcFMzSHBlY3B5blNGckhIRWJla0l2RU9NZU4ySzR6UDF2cHhXUzdsTDlSdDh0NXh5Uk9TcVVCRXk4MFZsS0VWWGp3Y3V5dGpvTHV3c3E0M0M0NW8yTDRjUzJpWEtwNWhRbWVXNW1OdWoxQU9rd3dSSXZtT2lha0lERlYxQXF1SVZJa3Niclhwa01aT212Y1ZGMWtjYzlUTVRRRTBQQ3BwbWNYRmo0bjlndmM5eTlqNUVBWlVQbW5pQ0p6elhHUk5rbzl0bWpEQm96SCtUaXNHNVB5d2QyMmxTY0xKT3hBcFJNNFpLS21FeDFoMUZYYmZxZkoxdElvL00yOTNWeVR4RXJpbEIyZHdkSjRuTHBVTmJiZHBFSGdtQVB2VmEvdjZzTkQwRlltc0lEczJSc05QdVRuTkgrLzBSYTMxR3kxeU93cHhuanJSVGYxWVNmeGE1NCtjZEova1RCWDJHZFVTT2s3aXptWUhKOEswUEh1WThxYTdDU2ZvdXhPVk5CUURXM2EyZTZuWWR2bTJwNDF1YlMydUpRcjZOTXF6VHhRdkVSSVFoVmtuYldyRWhrRExUUEF5c2tKdXBmaUZZckt5Q29yVlNxc3diTE1FRkJRamdhSERIdUlGYnd2SUszTEtlT3VRR1NlVGxsazJjRlE1ZkkyVkptSWs1K3YxeUFWazdIQ1VXTlpWK2k0NmlJdVBiT2JTdTZXVU5ISGtYcXFwQmVPVnpJcEFrRjh4VDdpb2lzTHV6Z3NZRkxPSTRsWUt6TlJXa0plU1JpVG9BQTFhRm9kQ3JVMW1MTkllOUxISTByR2dxeEZRQlVnWkJSMDlWVDBrMHdiMmhqRDdDd1Bkdnk2dGkzaUZhNDJQYytubDhqdjJGS1o5cHhJcUxSODRUV2x6SkczQVNtSkhJbkRjUGgzYUN1ZmR5TmhzRWdYeHkwWDNWRmVnOUgyNHl6K1RMa2xPY2ZXdXhrdUZyYVdLdk1mOFhsdUVHVG9lSUpxSzhPQjZNTHVYcG96aVNRakNISHhOa3c3dnREdUVUYi9BQi9udHJGbTZsMXZrY2g5OU1mUmh5eGFDRzJFaDhSQSt3ZHVCOVRyOG5mTGRXS0RDcG1Xa3JOTngwSzJUVEwzR0ZaKzVTUTdnQUEzTUNZS2R3OVBnR3ZYYTdSNzI5anQwOFRFZnR4SFIyNEcvVTdtbURsamxtNzNpWTBqdDRTM0FuT2xBTWtjNW1nOEp4c2p3ZnlBNHZlM0RWS1JoNjVRbHhmMlN2VXF2dnBBYXZDTlpCbzJjU1RCSmM1M1NwM2lDNHZuQ3ZjcVlCSjBLWXZYV1NTOHdiSnl0R20xM1JZT2k5QWMxcUFhOEdwWGp4T05FTW5vWDZzL01WYzNIUDhBc1VjYjJkMU8xTmNsc2xDcmFTQldTRmpwSXBVeHJYaUs4Y1dGbW4zWHVCL0pMRTEweEJlSzVsQlN0VytHY3hid0ZxMDNSY05CVVRONEh6VTVwQVFUZHNsUktvbWI3REYvbHByZjg4Y3A3cFlTV2M3UG9rV25oazl2UUIwanJ3VGNqZktCOHgvcHp6YlpjMTdKQmJKdVZyTHFVK2ZaTjJFVWVaMXpCNGxUVGpUQ0p1SWVET0VXUk9ZRUxTYUpMNVJ5RlppclM4cFYyMXRyTVBIMXVMYnd6ZGQ0czRrRm1rbzVYY09HNkpmeXplSHRFNEIwRDQ2QytVdHY1ZGszY0pac1pKZUl5a1dnRlRYTTBPTW12bWs1cjljN1AwMytMNXBqV3pzbllMS0ExaE1HSjBnS0RHbXNBRThRQld1ZkRHM2ZEZEZSbzFXWng2SlNsRUVDQVBhR3hSNkJzQUJzR3dCMi9kcSs3ZU5VakJIR243djNZMDNibmN0ZFhUTzNpcWYyNERGd2E5OFIrRkljOTZyak5ITStNY2t6ODdkb0c3eHNNd3I5Um5zWjRNd1JrcTdWcWRiVGoyV2F1SzFrSGtISDJTaDR3dDB1emZHSzFPV1BSazNTRFlmVE9EcUZTVExZSEtWMWVqYnA3S0pZbnRHWXM2U3ozRVVicnBBNzhkc3lTU29wRlQzaXFrOTRVcWNSRzRSeEdaSkdMQ1FESXFxTXdQWVhCQ2sreXA2TWZ0eXVnbkdMOHJlMVprQmF3M0MxUzFXNUl1OE96TnF2eDRZOTNtb0xQZUxiUEFQQXM2dGRpSUtFSktMMjZMaGpySnMyYlpxVXlIYVJJcFNsQVBQWnJtSzYyamZJZ3NjWWxTSmxXUFZvWHkyZGlGMUZtSTRVTE1UMGtrNDhyMUNsNWFNeFppQzJacFU1RGpTZ3I3Qmh0RzRmc0E2QThUZW9ZVnB3S2dhL0k1azUvS3prVXlmMm1sYzlMNVk2KytldDBsMzBDamI4SzR6aUVuMFlzWUJPMFVrb01YVFlURjJNTGRRNU45akNBbWZNRHp4N1B0M2xzUkRMYnNyQUh4YVhWc3gwZ0VxUjJqc3hDYllFTTgyck5sSzB5NFZCSDE0YVpvTXhPWTZEdjluN2Y3NldGMGR1TTRQdjczUXlDSEh1amxXTVVxenk1V1paRHUyS2NHcmVKalVqbUwvOHRoZEcyKzdycW0vVm02SWp0YlRvT3RoN3RHWER0SFRqYWovYlEyS0s0M1htSGVtSDVrUzJzWU5UbHI4K29wcUF6cC9DZmFNWkhMbTdNNm41RlVSL0NxY29iN0NJQVhjTnVnaUhUYitRNngvY2xtSkhIRzY3Yll4RlpvdjdjY0VaN1kxRC91VHpnb0JYRFVycGxVenVyUXNWUW9uVElySHA5clVUQnYwN2xEaHR2MDMyL2Rxd2ZUeXorSTNsWldHYWh2clU5bzRZd1A4QW5vNXJmWlBTK1MwUnFQZFR4Z21sYXFrMGJFVTBOMloxQjl1SEo4cS9iQzVsNVl6YmRjbnhlUzZhM2lMbExndkJReFU1VXhvdUNJUk50R01WZTVzWklEb05pQUJnS08yKytqWGZPUXR4M1hjSHZqTnBEVW9OQ21nQUEvekIxZFdNV1BSejUwK1JQVGZraXo1VC9UTmMwV3NzM3hGd0tzOGpPVHArQmxBcVdPUWNnZEZPR0VXWHo5WlVDUnQ5WmxaTmhJdWF2S3lFQTRrMlNRa1FkT1dLNnJSZFJEY3BEZHZsVE1BYmdBOU5VeGZReVdkeTlvV3FVTkswQTZqd3orM0cxL2s3ZGJQbTdsK3k1aWlqOHI0dUxXRjFNMU15Qm1RbGVGZkNQWmcwL1kzaDM5ZzVpV08yRktZeHF6VmxrVTNBbEVkbHBwd1ptb2tVNGR3QVk3Y1RDUDdnMVp2cFpiaDl3a21JOEkvZU92dHhydy91Sjh3Tkh5bFk3TlhLV1Y2ai9Eb0lOZFBTUncxREcrMkRJWWtXekEvNC9DVHUvanNBZjAyMWtNbmhHTkpMbXJrOVp4N2JYYkhYQ1ErY1BHeU5oODVUT1duOTRsazYvbXlHdnNmWm9ocjdjV1J1YVkxSnBONDJ3Zml1MlB5Vy9HTHRacGo4VjY3aWlLWGl6ek1RL2RtY0xTWlV6dVdlN1J2YVhLdTlQUHRTN2NrUytmYU5HVlk3bkZZNmlzczhxQXBObkpScG5EZVc2QUFSMUN2M21nZHd0d2x3Wml4MFNCcWp5R2xwVlVVNXI0Y2xGTlFQVHhHUTU3ektwTUxSc2NjQTZqWEg4bkpwU0h1RThXYkV3Y1REZDIwa25QcmNqa3VNNEFOWk5Nc3l3YXRHSHFCUmF2VkZYVEpvbVZzWlFRU0tBUVcyeXozdjZsY1NCVksyMURTaEdWUU9HUkpwbVZ5SnExTThkNzVBcjI2aXJEVVRubDFIOXZvdzMvUVZpYndybmpzc0dPdmN3NTY0dGNtOU8xeTFST1BYSmFxSUdINVhTS05lZjRpdktqWWR4QVJaV0NyczFIQWJiZ0w1TGZvSWFOdDZDUzhuN1ZkQTFrUnJoWDdQek83L0FOSzlBOXZiRFdmNWU1VHhkaVUrai9qaGp0ZnUxUnRqeWNZVml6UWMrN3JFa2FIc2JhSWxHa2d2Qnk1Q2Q1NHVXU2JLcUhZUDB5RHVaRlVDbktBOVEwREpJajEwRUdtQ2E2MjYrc28wbHU0bmpqbEJLRS9pQXBXbnNxUHBHT1VmdzZmdi9uKzhOZHRRcFhETEdWNy9BQ0RBY3RzcmNkSFp1NEdxOU51clVwaEVPd0YwcFdGVU1YL3Q3akpxZ1A4QUFOVWY2dUVpZXlZL3d5L2JIamNGL2JCS3Z0SE5NUDQvaUxFL1ZkWXkyV0RjWk9URVErS3kvd0RwdWJiVkd2M1dvTWJncmJLMlduVmhtL3NVeENVcHkrdWl5cFFFN0ttTlJTS2JxT3k4dWtpY2Q5aDZDWDQvRHBxM1BTa0sxOUllSkNqOXZyT05WZjhBY1h1SkY1WXNJd2U2WjVLOFA1ZXpHNmUyR2lheGptY3NyNU5CTkt2MWVRa3pMS0ZMc21WbkhuV0F3N2xIdDZrMWZjN0xEYVBLY2xWU1RqVGR5L2FUYnJ6RGFiZkNOVXMxd2lLTWg0aUJTdFI5Skk5b3g4NC9Qa3lFZzBucDlVcEUzZG1uNVNaWEtVZndxU0xweTlPWGZvSWdCMVIxaHp1TWpYRTVsNldKKzJ2Wmo2bk9TYkpOczJHMjI2THV4VzhLSU9uSUtLY1NUMGRKUHR3M1gvSGRvaG5zdmxTNXJJQUpYMWtpSXRzc0pRTVBoWk1uU3l4Q2lPK3hmSWNOOWgxZG5wWFpsTE9TNFljU1B0OXYzWTFFL3dCdzNtQTNQTkZwdGRlN0RISlgya0lmNFIxOVp4dEphazhiWkFnQjBLbVVQNGRQK05YTmpWMGFrMXg1T3VPbkN4WE9TTXQ0encvR1JremsrOFZ1aHhFek10cTlHU2xvbEdzUXdkelR0Qnc1YlJxYnQ0ZEpEMUt6ZG1xY3BSTUc1VXpEOW12R2E0dDdVQjUyMGdtblQ5MkpmYU5qM1RmWm5nMnFFelNvaFlnRlJrT3JVUlU5UUZTZWdIQUU4c0hhT1NlYS90eTRralZVWDdPQnVtVnVUVmlLZ2NGRTA0ZkhPT0h0VnFMMVF4UkVoMnJtelpCSWRJMjRoNW01ZHZzMVlQTDNsMi9LRzhYakxWcEZnUkRYK2NodXZvZGVQdTdBN2NFZVRkYmFFWkdObjFkbVEvZGhuZTQvY1A4QVQvcm9GeE5ZVkJ6NlVQeDc1QThRdWM3ZE02RlVwbHdlOGJ1UXNpa0ErR013cG5oM0dNWXUzekI5d0tsQ1VISnNiRnZIWng2SnQxenFkUlRBcGpybFpWM1hhTnc1WkM2cnU0alZvYzZVWkNXYlBJWjBYaVFCbjdEQ2JrVGEzY0Y5L3dDVXJkLzZxZFo2K0F4eEhqcGp5SjRzOHVKZXNXMjNZWG9xT1duRnpOaXhoRXliZ2NrNStocG1hYzNINnpkVUFqMnNlRXJUSktRTTBqM0M3dDY2ZEE0ZEpvaWtrWWlRMHpZMjY3VHVSczVHQ2xpUWdwbTRGU1NhVkFwL01hbmdNWlZjN2IzY2VvM0lxNy9ZUU5PYlVxMTA0WUt0dXpsVVJWVnREU2F4bitVaktnTldOU3h3NHorblgvZmNmMzZMQnd4amZYcnhuRi95SWFTN2NZdXdSa3BvajNJMWk2VGNIS0xBVWZ5bWsvSE1oYkNZZGhBQU04WWdIVVE2anFuL0FGY3RDKzIyOTVTb2pkZ2YrYlFCMDluVmphSi9iSTVqaXRPZWQ1NWFsT2QzYlJTS08yRVRzVGtwNEE5TEFaOENjWkRySVRlUVdVTHNYMUJPOHZVRGZqRGYvbldQUnBXdkhHOEMxSk1BWHEvZmhqM3NkMnRsVnVjYmlFZWdIZmNLaThZc3hFd0ZBSEVlNVJmaDhUQUFpY3BSKzhldXJUOUxKMGgzUm9meEVmdnhyTS91SGJGTmQ4Z1crNXgrQ0NkaWVING1qSFN3N2VBUERHeDczQzcyRkI0UzVqbVUxZ1JjdnFmOUNZbk1ZQ2lMcWNjTm85SUM3aVhjNGc0SGJZZC91MWRmTmQxOEp5N1BLRG1VQUh2STdEalZiOHN2TDQ1aTliOWoyK1FWakZ5enQ3RWlkdjRsNlIwR3ZWajU5R2YzNElzbWJNRENBSm9LS2lVQkVkdmxFQ2dJOVIzRVIrL1dKODdhcFZJR1lQN3NmU3Z0NmVUdHJ0MDZCOW1OUnY4QWoxNC9DSTQ5c0xBWk0za3MxaW1KZzV6QVBWUHY5TWdJR0g0bEVpWWdHc2ovQUU4dC9LMlNKeitLcEo5L3Q5MzE0MEQvQUR1YjhkejlYTHVBSEtBS24wcUsvaEgzKzNHbm9BMkFBRDdQM2Y4QUdySXlyWEdFV09nanNBanY5Z2lIOGcxeGt3N01jNFJ6eXl5TnlRdHZLT3BZTUxqS25aYXdkWTdsVGw1R3QzREVUeS80eGthZkx5bzF5ekhMbGRDR2JSbFF5TFJDVjV4SkJIdWdYVkVaWTRDWVd6WXEyZzdjYmpjMzNkTFNOUTltekFVN3REVUQ4Ujd3SVBVY1pVY2hiRnlCYWVuYzIvWE03VzNOaXhNd2xBdUMwZEhJWDhvTVlaRmRhQ3JMcEZSVVZyZ2dPSG5abnpsanlwNWVva0l2UUlENlJ4SndBOEtJSGF1NnhpOTZySTVqc3NTcVFmVHJzTEpsRlFqRUZDYmh0QUFUY0RGT0dydzVvaWoyYllkdjViSGQzR0pYZTRHZkZpR2pCNHFhS3hIZFkrRWFnRFFERHV3ZDcyK24zSnpxU1J1NTlZYnQ2dUk5bUdqN0Q5LzlOQVdXSnpMRmJaaXhUVHM1NHJ5RGg3SU1hbkwwckpWU25LYlpHQncyTXBHVHJCWmlzczJVNkdidm1ncWdzM1dLSlRvcnBrT1FRTVVCQjN0MTdQdDE1SGV3RWlhTnE1WlY2Q09CNGdrSEk1SGhqd25nUzRpYUtUd0g5dnR3akhHTmNzVmtSbGVOZVphM0w1QTV1OEJZK05ReFczQzJzcUM0NVQ0Q1J0RUhLNGp5RityWHhQR1NPWk9heXpSc0thYW9xcHZtU3lTMy93QnpySWVvR3dRWHF3ODRiVERxdExpcENhaUNqTGs5U3paMWNNYTA0MUF5S2tubnBQejNlYkJMTHlqZlhZc3RzdVA2a3ZsQ2JUUlcwalFFWmpxcUVxR0duVnFOUUNNTko0ZForbXN5MW1iaWJWWWE5ZTduU0phUmg3dmRzZlF6dUt4Z25hVFAxblRxaVZWOUtPMUh0b2NVbGc2YnMza2tpbVZzNFhJWVJCRlh1UktIYlRlUGRRNlpXMXlyeGFsQng0QUFVeUdWUld2SHR3LzlRdVdJTmh2MW1zb0RiV0V3R2lNdVhZQlZVRm1MTVhHczFhakJhVjAwQkJBNGI3bmVCajhodUdXWWFVeWFnN240K0NOYXF5bUFkeC9yZGJVSktOZ1QrWW85NmhXNXliQjFFQjIrM2JVWnpqdG42dHNNMXNCM3dOUTlvSVBXT2l2VFRGZ2ZLNTZnL3dENXY2MGJSdjBqYWJScGpGSmxYdXlLeTAvcHlITWtDcXJYUEk0K2VGSnlpSlNnMmRpWkI2eVVVYXVFbFFFcHlLSUhNUlFod05zSlRrTVVRRUIrQWhyRVdSR2piUS9qR1BwMnM3KzF1YmVPN2liOG1SUXd5UEFqdEFQMGdZc1BpZGx4RENYTExDbVRDUEFTam95N3hEYVhNVSt4UmlwSndSaThCUVIzMlRLa3QzRCs0dWlQbEsvL0FFN2ZJSm0vaXA3YTlIQTlPTWIvQUpuK1RZZWUvUzdkTnJoRlovSTF4bXB5WlNHSnplTUhLdVJOT3pHeEQzbk14UTdEaHZqbUpMSUpsYTVJdFZmZUpPQ0tENDFZK0paRm5DcWZMK05OVS9qSDRoMUg0RHE3L1VxK1JPWFVpVTV5TU9qb1VxZXJxeHFlK1FMa3k2dnZXdWUrWmF2WTJyQWlvR2NpU0N2akhEVDIvZmpFcG0rMHg4eElyQ3hjRVdSQWhFRXhEdURjZnc5TnczMk1JL2NHKzJzZEYvT25CL0RVZmRqZVBmazJHME5FK1RDTTErajM0M2UremRqOEtieFd4TXpNMjlPdCtqb3AwdVhzRWdtVmZJbmVtT1lQaDNHSXVYZmZxT3NzdVVyWDRiYUlVSEFKOXVZNlQwWSthVDVndDdiZS9VcmM3NE5xUnJnamhUd2dEK0Zla0hvdzVqZnB2OFAyL2Z0b3BOZmZpaWVHQWI1bjhpNnRqS3VSK01vM05DZUhjdlpGY3hzZlJyV1dsclpBanFxL2NUTWF6aVg5NGhrbTY3ZU1xTmltSEtFUWQwNE1nWHZlL2xuS2N2Y1dHM1hjSWJkVnR4TDVWMUo0RzBscVVJcmxTbVlOTS9hT0dMVDlOZVQ3N2VMbVRmNTlzL1V1WGJJSHpvek9MZXBaVzAwYlVISlFqWFJRUWFhU1FHd0RWd1kzUGovanBIajlpaUJyMWE1NTg3WmQycFo0T2gyMjAyYkhPTjJnK3JqOGs4akl5Sm1qcGhVcTdHeEN5ajlSSkVqYjFVeXVrMktxb29VaHRIUHA1eS9id0xKelB1MFlYYkxhaGs3eDd6dGxIUUsxUlJtRFpDbGFBNVYwaW5xbHpsTHpEdWNleVdGd1o5dWlxSVdLQkNvWUw1Z09wRmRxVTA2bk5TRnJ4TlMzREJPR2FkeDZ3L2ozQ3RDYkdiVlBIZGFZMTZNRlVwZlZQbEVBTXZKek1pY3ZSYVZucFZkZDY3VStLcmx3b2NlcHRSKzdiamNidnVNdTVYSi9Oa0lydzRBQlZHUUF5VUFWb0swcWNDRnRicGJRTEJINEZIMm1wNitrNHRyVWZqM3hOTDdNTEFJYzArSWNubnh0Uzh0WVl0S09LT1crQ25EMmJ3WmxVVVRxUnlobnBDSnoyTnNpTkd4ZlUySEdGNFlsTTFmcys0RElIT1Z3bDg1QklvVWN0Nzh1Mk05anVLZWJzZHpRVFIxcFdsZExCbEJjYVNkVkZJcjlHSTIvc1d1Tk04QjAzY1pxcDQ5WFFUVG82Y0MveEN0V0x1UkdjSE01a0F1UU9Qbk1QajVDQlhzZzhUU1dSR3ZVdXFuZHlLMGhaOGlVV0FpVzdacmtTZ1pVZlBrVlR5eDFYcVJ5Sm9GRXFDKzUxV1hNZkpzZTAzTVc4MnJHYmFKS21HWHcxeUdxcWFpd3BtS3NBRHhGS2dZTWR1OVNkMHVPWFg1UWNLb2swaVlFQm1rS3NHUWx0SGRvUUtCV0dWQWNxMUxqSHZMT3ZaZHkxbFNsUkxDUC9zL2p0UnRUSG1WNUNUajIwRFpNbXZrV0NyMmh3cWJ4MDJkdm4wUTFlSEs2OGJkWkVGZzhmbEJRREo2RGJmY1V1N3A0OUkrRlNsSHI0aVJVaWxBUlE0TDk1NUJ1T1hOaTIvY21tWitZYmt1eld3UVZoVkdvcmF3N0t3WmFOd0ZDU3BxUWNJaDVGZXcxaUxKRjhzdVQ4YlpSdURldTMrWmYyaG13cmlOYWthODFDWGVLdTFrb1Y0a2dmelIvbVVNS1k5NXcySGJmcHF2YjMwMjJ1NXUzdkEyVWpWNE1ldzUrYVBzR015ZVZmbng5Uk9XZGh0ZVhMcUV2SmF4Qk5XcTJXb0dZN3Y2ZTFNai9BQk1lczRIRXYrUFhDQzVRVS91amswZ3BIS1lwaXhsZjNLWXBnRURGTjZjQUF4VEJ1R215K21XM3FLcTJZNmFOL3dDN2lkdVA3Z1BPbHpHWTViYXFrWi9tUVovUnR3dzBIT3Z0Y3VPUzJBOEk0M3YyWk1tSURnMnRLUUVVc3laUVIxYklZeUxkdTNsWndqaG1vQVNMWmsxS2lVVWhLVVNmRUJIcm9qM2prMkhlTE9DMnVYeWhCcGtlbW5VNjlYV2NVbDZYZk5QdkhwVHpOdW0vY3YybExqZFRIckhteDVhTmYrWmFUQTE4d25KVXBUcDZGZ3V2OGVxQWNTQ2Fwc25aS1dTU2RwcTlpa2JYd0JVcEZDbk1VNCttM0FERURxTzNUZlVCRjZaYlpDUktHclE5VDU1LzZ1TGx2L244NTMzRzJlQ2EzcHJVaXV1M3lyN051R05TWEZYRndZaXhuQTFkZnVTUWdJYU5pa2xsdTFNUmJSYkZKbW1vcjJsSVFvaVJBQk45bStyU3M0QmF3SkVQQ29wWDZ1czQxN2N5N3BKdlc3eTN4L3FTdXpVN1dOZXBmc0dLbXlOejh4akVabW11TE5mZHU0ZlBEMW9zenBnMnFLVVFxMHZNU3RkWlMxUGNSam4xU0FUckd4dlpBV3pic1VSS2RWaTc3enBrUkF4NHlmZnJYNHh0cmhiL0FIOUtjRGtTdW9aMDBuSTlkTVdCcy9wQnpDL0s4SHFGdVVRLzdRSjFNd2VPcFZaakM0S2lVU3Izd1FTRUxkSUhUZ0ozRnJzMkFtMUF5VHlzckVabS93QndTMVA3ZlgrTUdIS1VXT0hKcm12V29ySnllcFpIZjA2VC9Sa3pVNmpLSnFQVlpWd2tFYkVOaWlkSlV5cFRxaVQ4aThsWHUrTCtxNysvbFd0dFZwSjZBK1dDRHA3aU9OWmFnR1FPZ0dyZEF3MDlVL1VibDdicDdubFQwc3JGeXJkcEVHai9BREdFcklBeElhNmpNMGVtU3RlOG9lbVhkNG54eEU0dVdmRjhqY2M5WitzTEhJWEs3TTZMSlRJTm9ZcGlOYXg5V20remlId3ppd2k2WlhUR2cxZGN4am5WVS84QVpsSHhqdVZoQXZnUlJJdVplWUl0d0VlMmJZbms3RmJFaU5LbGptYWxpekFQUmptQXhKRmZjS1QyNnlhQ3R6Y0hWZHlac2VIdW9EVDZCZzU5Q3VKVEUwc0xFMHNMRTBxZE9GZ01PV1BDVEdYS1ZPdVcxZVRzR0tzOFkzVU8reEh5RXh3NitrWklvRWdBTENWcURzZ2xiV1dwUEZGemV0aHBFcTdGMFV3L0tSVFpRcEhzWE1sM3NvZTJBRXUyVGYxWWpRQnhRaW1yU1dYaitIajAxeEhYdTNSWGJMTDRaMDhMWm1udXFBZmZoUS9JMWpsaW5WMWpqMzNFTVVUN210d2M1T1R0YjU5Y1JzZkkyMm5LeTAvV1hkTWtMZnlCd2FFTEt1YVZZelFEeE1Ba3lObnpKdThTS1pxc2dLYVluYzdueVR5OXpsQ3Jjc1NpSGRDdFJhc0hZclFkNmtzanFyMUM2dUpwWGlPR0R2a1AxWDVqOU90d005NG54TzJ5VUVnMUltdlRYUlFyRzdKcExkQW93eVlFY0NMd1JrYktwSkozTjhUY2g0UXo5dzl4L2lPeVIrTjhkNHN0RUZZTFVzOHFOTXJUTEhWVm5JdDZDVnJyMTlrck9xK1BLSFZkQzNGdW1RaXlDYnBUdkN2cjNZT1p1Vzc5b0xtTWkwakZBbjVmVlFVY0ZxNTk0MEpIRVZybGl5SDVtOUwrZGVYVWErVXdjNXp6RnBycG11V3k4M1VUNVNxc05CRitXb0FCclRJRFBCR09lWU9Sc2MyN0NtTU13OGVuNDNiSmNWV0hsaW5hZEtKb1VhdXlGcHNiS0FUZzRxU3M3ZU9KWXAydHB2UWR5ekZGY3J0QnFRVE4wM1FpVW90VHVseERMRkJORFIzclVodUhUd3BtZWpqeHd4ZzlPOWszZmFiM2U5bTNNbTJ0V1VLclFIdmxtMGtsaklDaTVhZ1NoSlVpb0JxTWRrUjdoTlN1Q0xVYXJqMjJSSm1YSTZrNEZuMGJCR01KSHlFdDZrc1J2WVl4ekIyQXpGT01WU2pBV0s1TXM0TWtrb1F4Mnh1OEFEbTIzdUs1cjVZSkFkUWV3TldoNGRuRDY4Y2J0NlFibHNaaitOblVyTmJ5U0lRb3pNV2pVdVVoTlB6Rjd4QVA4dVBVY3Y4QVBITERHT2VNYzBYQTJIVmIzUzVpdHg5c20zeldsMk9iQ1VjUmw2Z0k2eVVVdHFZdGxLeFRwdWFxRDV5ckd1WlZacTFTV1JNcXFvS1pPdy9YYzduZElieU5MR1BXaEZUbW82c3N3ZnUrbkQzMDk1YjlQdDI1WnU5dzVydnZoZHhqYlNoTWN6MHFHN3dFYnFwNkJRaGowa1V4V3VjV3ViblV0bmRueXp6eml6Q0hFR3gxV1hocTR5bmJuQjFhMG9TS1VyWDUrbVQwZEpWZEN1MlV5UlR0M1VmS01GWmd4M2dBVWlhYXFhcGdGeGFjdDh5OHhYa3RncXM5ak1VQ0tBbVhBbXJCbEk3d3oxc0IwY01lMFBQWHBweVB0V3o3cnk5YWx1ZGJRM0h4RTNtWEZIRGxrUW1PWkhoRkluSUhsS2M2TXhEQ2hwakFGeHl4ZXFwVHFYd2F4V2EyMmF1VWxiSE1yN2lQSVNtek5Ob3FkS1BZSDB1bEZZdWhKbEJXNTVjYVFibHlRV0NDUGhoZkkzS0NxeWZ6ZGxsMkhKdXljb3hLZWE1dk0zS0phaTFBWUZ3YUZRWkkzZFZwWHJ6QXBxRlNCVWZPM3FEdWZQRzZYRTIxUi9EN1ZPeW1sVmNBaEFITldqalk2bTFFaWdGVHdQSEROZU1YRFNqOGVIOWl5Sk1XR3c1azVEWkFRYmt5Wm56SVNwSGR3c1pVREdPbENRREZJZnBGRXBURlEremVJaTAwVU5pbE11WmRVUEtMSGY4QW1lNzN0STdSRkVXMFFaUXdpaDBBZ0E5L1NyTlVpdmVyVGdNQ2xqdDBObldYeFhEZUpzOC9kVWdlN0JqYUdnQU1oaVJ4TkxDeE5MQ3hOTEN4TkxDeE5MQ3grSy9nOU90NnJ3K2w4U25xUFAyZUR3ZGcrWHplVDVQRjQ5KzdmcHQ4ZExDd2g3a3pFK3l0TjVYY01KVzBRdFE1R0xMbUI3T2NMRzJXbmVZV3NsNXpkaHJTaHhYcjlsV05NQTQvOGYxcG9vNDMyN09tMnJRMkIvVWVQYnE3YXBmYmE4SDhnZFg4WkVsUHF3TlhTN0UwL2ZiVGNWNnBEbmwxWlk1L1JlTU9mVjQ5cy80OSs1cHpUZ1lWUW9HWlFuS1RqREtaQVZRU0VUQ2dtYytWTWVZa3lKc1VOd042dDZvY1M3YmlIUWROSmQ0MmVJMDNYWjRKWk9reDNZWFBweWhGUHJ4Mmp0cFNmOXRja0hMakg3S2VMM1lzaHZnWDNNUVJCdVQzQnNCbllGZUNtTWdUaFl3K3FLUHUwNVJjS05FOHhrUkxON2ZNWXUvZDM5TmVhNzd5SGtSc1RhcS8vTW45MkpFMm5NSFRkOS8vQUVvOGNVdDNHWGtZUm1zOXp4N2xuTFdXaVNGT1plSjR6Y1lUVUpkWWdGTUt5WlZLTlI4djNqdE1udUJmU3VremdPM2FPKzJ2Vk41MktWcWJWczBFVW5XOTJHRmVqK3NLWVlHMmtVZjdpNUxML3AwNnY0VGlyOEpRbnM2UUdWR1VkWmJndGQ4L3BQQStsejNPeHBtWmxlM012OEREVUcvS0d0VldCSk5kNENKL29qUWpycjgzVGJUL0FIaC9VaVRiR04wbmw3WHBHU0czUGRxS1UwRXlVNGNPampsanBacnNhdUJHMnFicklrSDI1WWVrejlKNlZ0NkgwL29mQWw2UDBuajlMNmJ4bDhIcC9EK1Y0UEh0Mjl2eTl1MjJxck9xdmU4V0NNVXBsd3g1T3VNYzRtbGhZbWxoWW1saFkvL1oiLz4KICAgICAgICA8aDE+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+ZS1GYXR1cmE8L3hzbDp0ZXh0PgogICAgICAgIDwvaDE+CjwvZGl2PgoKCjwvZGl2PgoKPGRpdiBpZD0iYjMiIGNsYXNzPSJib3giPgogICAgICAgIAogICAgICAgIDxkaXYgaWQ9ImRlc3BhdGNoVGFibGUiPgogICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzPSJwaCBlbXB0eSBwaDciPgogICAgICAgIAogICAgICAgIDx4c2w6dGV4dCBkaXNhYmxlLW91dHB1dC1lc2NhcGluZz0ieWVzIj4KICAgICAgICAgICAgICAgIDwhW0NEQVRBWzxpbWcgc3JjPSJkYXRhOmltYWdlL3BuZztiYXNlNjQsaVZCT1J3MEtHZ29BQUFBTlNVaEVVZ0FBQXNjQUFBTGxDQVlBQUFBb2trc1RBQUFBQ1hCSVdYTUFBQzRqQUFBdUl3RjRwVDkyQUFBZ0FFbEVRVlI0bk96ZFRWSWJXZGFIOFZUSE8xUUU3aFZBclFCcXFKR3BGWmhhZ2VXcEpwWlhZTHlDZ2dsVHd3b0tWbEF3WWxob0JZVlcwRllFYzcxeHFaTjJJa3RJbVhrL3pybjMrVVVRWGQzUlpZdVVsUG5QaytlZU8xZ3VseFVBb0wzaFpIUzg4aThkVlZYMVpzMGZ0UHIvNitLaHFxcHZhLzY5MWYvOTRlbmlmdDMvRHdDd0E4SXhBRFEwQW04ZGROL0lQMWZ5ejRlR2p0ZThxcXBIK2VmSHhqL2Yxdi9iMDhYOTQ0Wi9Gd0NLUkRnR1VCUUp2MjhhNGZmSVlPajFiZDRJejgwZnF0QUFpa000QnBDZDRXUjBVRlhWZ2JRejFQL3NRdkFlNzNZbmQ5SzY4U0EvcnVMOFlQRDNBSUN0Q01jQXpCcE9SblhsdC81eElmZ3Q3MmcwTTZreTE2SDVnVFlOQU5ZUmpnR1kwQWpDeDQwd3ZNKzdwODVDZ3ZJdGdSbUFSWVJqQUNvTko2Tm1FRDRtQ0p1MmFJVGwyNmVMKzl1TWYxY0F4aEdPQWFnZ0MrV09HNEdZL3VDOHpTUXczMHBnWnVFZkFCVUl4d0NTV0FuRDlBbURzQXhBQmNJeGdDZ2FiUkx1NXgxSEhWdlVZZm1hTmd3QU1SR09BUVFoQytoY0VENmhaeGdlM0RUQ01ndjhBQVJET0FiZ2pjd1hQcEVmV2lVUVNsMVZ2bVRlTWdEZkNNY0FlcEYyaWJGVWgwdmVaUTVwdUVrWTExSlJ2dVk5QU5BWDRSaEFhNDFBZkVLN0JCUWhLQVBvalhBTVlDY0VZaGhEVUFiUUNlRVl3RWJTUXp5V0h3SXhyS3FEOGlXVEx3QnNRemdHOElKTW1YRFY0U2s5eE1qUVhJTHlHVk12QUt4RE9BYndiRGdablVpRm1CbkVLSVdiZW5FbXJSZHNPZ0xnR2VFWUtCaHRFOEIzVjdSZEFLZ0l4MENaaHBOUkhZaVpSUXk4TkpkcThpWFZaS0JNaEdPZ0VGU0pnZGF1cERlWmpVYUFnaENPZ2N3Tko2TmpDY1R2ZWErQlRtWVNraTg1ZkVEK0NNZEFwcVIxZ29rVGdEOExhYms0bytVQ3lCZmhHTWlJakdHYjBqb0JCT2RhTGs0WkJ3ZmtoM0FNWkVENmlVOWxQdkVlN3lrUXpaMkVaS1pjQUprZ0hBT0dOVUl4L2NSQVduZlNic0ZXMVlCeGhHUEFJRmxrTjJYRERrQ2R1VlNTV2J3SEdFVTRCZ3lSVUh6S2ZHSkFQVUl5WUJUaEdEQ0FVQXlZUlVnR2pDRWNBNG9SaW9Gc0VKSUJJd2pIZ0VLeTBPNk1ubUlnT3k0a1QxbTRCK2hGT0FZVVlmb0VVQXhHd0FGS0VZNEJCUnFiZDN6bS9RQ0s0a0x5bU0xRUFEMEl4MEJpdzhub1ZJSXhtM2NBNVdMSFBVQUp3akdReUhBeU9wRytZclo1QnVBczVKemdOaFA1eGhFQjBpQWNBNUVOSjZNanVRQXlnUUxBT2t5MkFCSWlIQU9SU0YveEdZdnRBT3pvVGlaYlBIREFnSGdJeDBBRXc4bG9LbE1vNkNzRzBOYVZoR1JhTFlBSUNNZEFRTEtKaDZzV0gzS2NzK0g2UXV0SzNyZkdQMWZ5ejZzQjVxRVphb2FUMGM0bjNhZUwrMEh6djh2bmFWWHpmenVRbi9xZjZXZlB4MEphTGM1S1B4QkFhSVJqSUFCYUtNeGFOQUp1SFhyck9iUVBQaXAzZmNKeGo3K3pEdEN1My8xTkkwUVRvTzJoMVFJSWpIQU1lRGFjak1ZU2pHbWgwTXNGakVmNWVRN0RzVFpqU0JHT3Q1RkZvbStrQ3YxR1F2UVJuMkhWenFXU1RLc0Y0Qm5oR1BCRWRyZTdaQXFGS2pNSnY0OVNBWDVNUFVkV1l6amVSSjZBMUVINVFQNlR6N2NlYkVVTkJFQTRCanhnSXc4VjZpRDhJQzBRS3JmbHRSU09ONUZLYy9PSHdKeldqZXl5UnhVWjhJQndEUFFnSWVHU0JYZlJMYVFTN0lMd3JkWWd2RTRPNFhnZCtTNGNTMkErcHBjNXVvVUVaS3JJUUUrRVk2QWpxUlovNXZoRk1aY3dmQ3RWWWJPTGtYSU54NnVremFnT3lzZmNRRVpERlJub2lYQU10RVMxT0lwRkl3eGZwKzRUOXFtVWNMeEsrcGRkU0Q2aHNod2NWV1NnQjhJeDBBTFY0cUJjei9DMWhPRnN4MVNWR281WFNXVzVEc3J2ZEwyNmJGQkZCam9nSEFNN2tBdjVOZFZpcitycWNCMklpN2lBRTQ3WEcwNUdKMVNWZzZDS0RMUkVPQWEyWU90bnJ4YU5NRnpreFpwd3ZKMjBMbzNwVmZhS0xhaUJIUkdPZ1Eya1IvS1NSNzY5elNVUVg1YStxNWVFdnI5Yi9DdS9jc3krdDErTUNjcTl1ZS9pU2VtZktXQWJ3akd3aG15M2UwMjF1RE1xeEQvUEF1NjZWZk84c1pQZmcvVnBIWDFJVUI3TEQ2MFgzWDE1dXJnL3RmcmlnZEFJeDhDSzRXVGt0bjcreUhIcDVLclVRTHl5d093NDhJMVZ0dE04ZHRWb3ZSaHpFOXZKblZTUmFiTUFWaENPQWNHaXU4N2NsSW16a2hiVjFSUlZNbWVOMXBVU2czTGRka0VMVkRzTENjaG1OdEVCWWlBY0F6OHVycGRVb0hhMmtPTjFWbWdZcXdPeHhtMlQ3eVFrWHlwNExWSEpPb0d4Yk9WTzI4WHVhTE1BR2dqSEtCNXRGSzJVSHJ5bWh2cGQ1NDBibU9JZW5jdTZBZmRldlZmd2NpeWd6UUlRaEdNVVM4TE9MVzBVVzlXTDYwNUxyQkpYUHpaL21ScDlzckNRZ0Z4a1paQnFjaXUwV2FCNEZlRVlwV0lheFU3bU10KzV1RjdpbXJSUG5HWVNxdVl5NTdiWXpTQ2tmV3FxdEIxR2swOVBGL2RucFI4RWxJdHdqT0xJcGg1LzhNNXZkQ2VWeHBKRDFJRzBKT1FZb3U1a3g3UWlud0pVUDk3ZlUxb3VYc1hXMHlnVzRSakZrTWVyWjF3UU43cVNVRno2cGhNbDdJaTRrRGFab3F1RGpUNXlxeTB6b2Mwa0lMTnBDSXBDT0VZUkdOTzJVZkg5eERVSlN0ZUZQWEpuRVJaOXlkc3NKQ0FYK3lRSjVTRWNJM3V5V2NBdGxhRVhGbEpGTDNLU3dhckNQeVB1czNCTWRmQmZtZldaKzBRZk1vcEJPRWJXNUVMM2xYZjVPMEx4Q2o0ajMzMG9jVVRmSm9Ua3RhNmVMdTdIQ2w4WDRCWGhHTmxpZnZFTGhPSTErSXo4NVB6cDRuNnE3RFVsUlVqK3lVeWVOSEFlUWJZSXg4aU85QTllc3BYc00wTHhCc1BKNkpMRm1XdFJIVnlEa1B6Q1hIclZhY1ZCbGdqSHlBb2JlN3h3SlhOdENjVXJDTVpiRVpBM0tHU2F5UzdZTUFUWklod2pHeXk4Kys2SzZST2JFWXgzUmtEZWdCRndMOUNyanV3UWpwRUZkcng3ZGllaG1Fck9CZ1RqMWdqSXIyQjIrbmYwcWlNcmhHT1l4N1FCdGdYZUJUc2pka1psY0F0NWFuVlcrTGJVM0VnaEc0UmptRlo0NEZuSVFydFRCYTlGdGVGa2RGSlYxWitsSDRjZWZ1T0p4SGJ5Qk91eTRFVjdiQ3FETEJDT1lWYmhqOGhaYkxjajJSM3hnZDdRWHR5TjJCRjk3THNaVGthbkJmY2pNK29ONXYySHR4QVdGUnlNWjFMRkczUHgyVm5wdmVnKzdNbHh4QTdrYVk1cnRiZ3A4SGk1U1VHMzBvOE5tRVRsR0tZVVBNTjRJWXZ0Mkw2MUJUYjU4TzRMYlR6dEZOeHF3YmJrTUl0d0RETUtubUY4SXkwVVBOSnVRVUxKWDJaZXNCMi9FbmphazFhTHo5WmVkMDhFWkpoRVd3Vk1LRFFZdXlrVXZ6OWQzSjhRakR0aHdrSVlITmNPcE9MK2l5eGFLOFdldEZnY0ZmUTdJd09FWTZoWGFEQStsd1ZROUhsMklGVTZ0dmtONDFDT0wxcHlON2xQRi9mdWljWW5xYXFXZ0lBTWMyaXJnR29GQm1OWExSNHpOcXM3cGxORTRZTGRBWXRDdTVQUDZXVkJzNUVYY203amhoL3FVVG1HV2dVRzQ3cGFURER1NTVSZ0hOeWViSHFCamdxc0lydlB6Sit5YVJPZ0dwVmpxRlJZTUtaYTdJbFU0LzdKNHBleDRSZjY0ZnNyc0lyTXJvdFFqY294MUNrc0dGOVJMZmFLWHRpNE9ONGVGRmhGL2tvRkdacFJPWVlxQlFWait1ODhvMnFjRE5WamoyVGgybVVoeFFFcXlGQ0p5akhVS0NnWTM4bGlKb0t4WDlPY2ZobERxQUI2NUdZQ1AxM2N1NEQ4Slp0ZmFqTXF5RkNKeWpGVUtDZ1lmMktYdXpDR2s5RTNGdUlsc1hpNnVHZXI0QUJrSTV2Y3R6OW5veENvUStVWXlSVVNqT2V5c3hqQk9BQ3BQaEdNMDlpaitoZUdyRVU0a0YweWM4VWNaS2hET0VaU2hRVGpHMWwwUjJVa25KTmNmekVqT1A2QnVGblNicGRNV2F5WEt3SXlWS0d0QXNrVUVveHBvd2hNUGtmL3kvcVh0T0cvYkFvU2xvVEgyNHlma3RCaUFSV29IQ09sbkZkazAwWVJEMVZMSFhnZkFwUFFlQ0NMZW5OVVY1QVBjbjBQWVFQaEdFa01KeU1Yak45bGV2VHZhS09JaWxDbUErOURCTkptY1p6eE5Bc1hrSy9saVJDUUJPRVkwVWt3ZnAvcGtUOTNGeTRlTDBkMVhORHZxaG52UTBSUEYvZHVBNWJmTTkwMDVGQXF5QVJrSkVFNFJsU3lxajNIWUx5UWdmYk0ybzFJUmwweHBVS0hQUlpVeFNXejB0MTNZSmJocjNjb1kreUE2QWpIaUVhQzhkY01qL2hjRnBHdzAxTjhoREZkcUI1SEp1MWJ4NW1PZTNzclR4cUJxQWpIaUVJcWZEa0c0eG45eFVrUnhuVGhaaVdCeHJpMzh3eC92ZmZEeVlpRnpZaUtjSXpnNUZGcmpvL0hydHcyci9RWEowVVkwNFgzSXlGcDYvcVE0YS8ya1kxbUVCUGhHRUUxWmhubjFoZnE1aGR6c2s1dnYvUURvRXp1MjcrckorMWR2MmE0VU8vcmNESmlJZ3FpSUJ3am1FeURjYjN3anNkOGliSDRTeWZlbC9RYWZjaTVMZFM3NVBPRkdBakhDQ20zVFQ0V0xMeFRoVEZQT3ZHK0tKQnBRTjVqeEJ0aUlCd2pDRmxBa2RNbUgzTzJOVldIQ3BKT3ZDOUt5SG9JRjVDdk12cTFDTWdJam5BTTcyVGh4TWVNaml3VEtYVGk0cWdUNzRzaU1zbGluRmxBZGs4a2FXMURNSVJqZUNYOVlEbU5iSnRKeFppSkZBRE1rb0NjMDViVGJzVGJxWUxYZ1F6OUgyOHFmR2tzd012RlZleUpGRElQK2tCK2pxUUs5MlpMNzdacitYaVVmNzV0L09kRDVxR2VDaVdpay9QY2tiUXJ2R20wa1J4c21aN2liclMveWMrRGZHY2ZZajZSY2x0T0R5ZWp4NHdLR0orSGs5R0Q3QlFJZUROWUxwY2NUZlRXQ01hNUxNQUxIb3psbUIwM2ZrSWN1N2xjaU4xN2MvMTBjZis0dzc5anduQXljci9UMjF4K240eEV2NmtNYVRnWnVkQjdJdC9UbzBEakErOGEzOVBiMERlMW1lMVd1bUE5Q0h3akhNTUwyZUx6ZlNaSE05akZYUUx4aWZ5a1dMQTRsdzFaTHExZlRBakhhdDA5WGR5YjNybFEyc1BHOGoxTk1VdjdScjZuMTZHQzhuQXljaHVHL0JIaXowNWdMdXRDYUgrREY0Umo5SmJaU2JZSzBXY3N3K3ZIeWlaNHpHVlJ5NlhGaXdyaFdDMlRsV09wRUkvbFI5UG1NamZ5SGZYV09wRGhrNzRxaDVzeTZFRTRSaTlTWWZrN3c2UFlPeURMQmNoZGFLY0dkbkp6SzlsUExiVmR5R0tjendwZUNsNzY0bnBiclJ3VE9ZZE5EVHo1OG5Jem0ya3dycG42N0VFdndqRTZrNVBzWTRaYlE5YzZCV1E1TGxQNXNYWnNYTy9qMUVMTEJlRllMUk1CUlJhL25ocDgrckNRa0h6VzhkeVVhekN1L2M0Q1BmVEZLRGYwY1oxeE1LN2tBdEpxMkx3c2RIbVUwR2J4MkxpZzhMZnJJWmZIekVCVzNPZGExa2o4WmJRdFowL09MNDl5dnRsSkljRzRraTJtT1hlaEY4SXhPcEVkOEVybzk5d3BJTHNxbEJzcEpDdkFjN2hoY0krWUg1VFBFYzFtOGtabTFENTFrTS96UXlhTGg5MTU1cXM3NzBocnlFWUZCZU5Lamd1VlkvUkNXd1ZhazhWbGZ4WjI1TmEyV01oRjV6U3pIUUZYdWQ5OXJLM1ZRaDZMLzZYZ3BlQ2wzNTR1N2xYTk81ZndlSmw1T0R5WGRRUHJ6bEdsQk9PbTg2ZUwrNm1lbHdOTHFCeWpGWGxjZFZuZ1VmdXBnaXdYM052TWczRWx2L3ZmQ3F2SVZJNTEwbllUZFNxTGhuTVBoeC9sSFBXOWlseHdNSFkrU2lFSGFJM0tNVnJKWkh6V29rZnJ3MHcyQXpqSmJKdnNYYmtGZXlkYVJyOE5KeU5PWU1vOFhkd1BOTHdpQ1liWEJZNzdXOGhpNE91ZXdYZ211LzVaYmhOYnlQeGpicVRSQ3BWajdFd3FNTll2TkZkeXdwOTEvUGZkaGVaL2hRYmpTdDcvclQyT0VYVjlIeEhHblliaktwL1B4MExuWU8vSitlbC9QWVB4c1lSc3krZy9SaWVFWSt4RStqdXRqODE2M3B4QXFwN0hCS3ZPOXVYeHJZWkhsbXdacTB2eUNwMU1jTGpOZkpKT1NOL1hWenhkM0xzV3VnL0dmNTlEV1VBTzdJeHdqSzNrOGFUMVB1TVh1M1lSa0h0endlUFBOcU9rQWlFYzY1TDAvWkRQWXk0VFkxTDRhZUZ4SmdINW94UjRnSjBRanJHTFN3TTd2TDFtdHU3eElBSFppNitKQTdLcXFRaEk5MzQwZ2pHNjJianBrUVRrTDhhUDYzV2JtZlVvRytFWXI1SUx6anZEUituVlhlNEl5RjRrQzhneVhtNlI0dS9HVHhhcHh2MFJqSHZidWh1bzdIcDRaZWRYK3NsZW9aT1cwQUhoR0J2SjJEYkx2VnJ6WGJaL0ppQjdrYktDVFBWWWh5UUxud2pHdmUyOFRiNjBwbGtPeU84VXRJTEJBTUl4WG5OcHVIZHYwV2JrR0FIWmk3TkVVeXhZamE1RDlQZEJQbThFNCs1MkRzWU5VK1BueVRPMmw4WTJoR09zTlp5TXBzYkhJQjIzZmNSTFFPNXRiNWV0dGdNZ0hPc1F0WUxmMk9BQzNYUUp4am1jSjJtdndGYUVZL3hFcWpGL0dENHlIN3IyUHNxSm53dHVkM3V4ajUrOFp6Y3gvMDc4NUNiQnhqQ01hK3ZudHV0N0p2L2UySEMvLzFzcEFBRnJFWTZ4anVXNzZpK3lzcm9UbWQyYiszYlFvYVdZSzByMU9LMm94MTgrWHlWdWlleFRyL0ZtVW9Dd1BCN3RsUFlLYkVJNHhndXlDNTdWaTg2VnJLanVKSk41emxyRW5pdEtPRTRyMnZIbkJ0YXJYdVBOSkNCYm5ZRk1ld1UySWh6ak8ybW5zTG9MM3RwWnhpMWQ4NWpXcTJoelJXbXRTQ3BhU3dVM3NONzFEb2pHWnlEVFhvRzFDTWRvc2pxMmJkRmxZVW1UalBleHZBQlJvNzNJbnltcXgybkVES3VXSitobzlhN3ZWdkR5eE03cXpTbnRGZmdKNFJqUERFK244QkdNM3hpZjU2elorNGp0RllUaitOekdIMUdPdTN5T0xHOUlwTm1saDZjOFk2TVRMR2l2d0U4SXg2ZzMrK2pjcTV2WTFNT3VYR2RVbzRLS2N1R2h0U0tKbURja0JKaHc5dnBlQStUN2QySjBnc1ZiTmdkQkUrRVlsZUZIbGVkOUpsTlVQL3FzMy90N1NWaGpQK0tGaHdBVlY2eXFzZnY4N0N2NnZYUDBzVzk3d2RQRi9hTUVaSXZPRXN4b2gxS0U0OEpKcjVuRmRvcTdwNHQ3SHdzcGFLZUlJOHFUQ1huRWIzWDJxalhSV2lvTVA5bXlwdmZONWRQRi9hM1JCWHF4MTBoQU1jSnh3UXl2L0Y3NHFFNUlEeU9MOE9LSVdUMm05emdPcXNiNWVldGpqWURoQlhveDEwaEFNY0p4MlU2TnRsUDBXb0RYUURVcXJsakhtM0FjQjFYalBQa2FiZVp1YXVZR2p4Q3RXU0FjbDBwNmJTME8wdi9rWVFGZXZRaVJxbkZjVWFySHRGWkVFYVdsZ3FweEV1OThqRFl6dkVCdlh6YkRRc0VJeCtXeTJGdmxOaHZ3OWJvNSthVkI5VGdQVkkzejVxVjZMSVVNaTV0c1RKbDlYRGJDY1lHTWJuZ3hsOGQwdlVtdnRkVVYxZGJGNmowbUhJY1YvT2FhcW5GUzNyNmpNbEhJV3Y4eGkvTUtSemd1ak9FTkwwNDhibEY3d2x6anBJSlhBK1dSdjhWK1J3dm1QbHFiZGtEVk9KMDl6emV4RnZ1UDM3RTRyMXlFNC9KWVhJVDN4ZlBGbUtweFdsU1BiYVBYdUF6ZXpwT04vbU5yV0p4WEtNSnhRYVNIeXRvaXZEc1pDK1NGVk03WmdqYTlHRlZCTG14aHhEaXVWSTNUZStkelV3d3BjRmliZjh6aXZFSVJqc3RpTFN3c2ZQYStDYXJHT2dTdkhzdkZtTllLdjRLM1ZGQTFWc1hyK1ZJS0hYZkdqc0dVbmZQS1F6Z3VoTkVOTDhheUhhbFA5SkRwRWFNaVEydUZYMVNOeXhMaWZEazJOdDZOeFhrRkloeVh3MXJWK0NiUUhGVXF4M3JzUjFqd1FtdUZYMEdQSjFWamRieC9QNlhnRVd1M1RGL2VNOXF0TElUakFoaTg0SGdiMjlZa0c1OHdwVUtYb0ZWQ2FRR1lHVHd1R3MwQ1BNbFpaWEVtYnM3MlE0UkNLWHhZRysvR2pYWkJDTWVaTXpxNmJleHhiRnZUVVlBL0UvMjhwWHBzUnVpcXNmc2NIT1oxeUxJUTZ2dHByYjBpeHJrS1NoQ084emMxVmkwOWY3cTR2dzMwWnhPT2RRcmRZMHJmc1IraGp5Tzl4am9GT1c5S0FjUmFld1c5eDRVZ0hHZE1xc2FXSGxQT0ExOGdDY2M2QmEzSVNDc0FyUlg5QkcycE1McGd1QlRCenBzRzJ5c09JODFvUjJLRTQ3eWRHYXNhaDJxbnFCR085UXBkTmFTMW9wL1FGVE9xeG5xRlBtOWFhNi9nczFvQXduR21aQkhGZTBPLzNWWEFkb29haS9IMEN0M1BSMnRGUDhHT0gxVmo5WUtlTncyMlY4VGE0Uk1KRVk3elplbnVkaEc2L1lPRkZDWUUrOHhLUzRDMTFmRmEzQVIrb2tNbFRyblE1MCtEN1JWbmJBeVNOOEp4aGd4V2pVTzNVOENHdHpKdUx4U3F4OTFRTlVZTVUwUHRGWHVNSGN3YjRUaFBsaW94ZDRFMisxakZBSGNiUWw1d0NNZmRoRHh1QkF3OGs2YzdscTVkYkN1ZE1jSnhab3hWalJjUmU4MEl4ellFMjRsS25rN1FXdEhPVmFpbk92SSt2d3Y0MnVGUGxMYTBwNHY3TTBPVFphZ2VaNHh3bkI5TGN4alBJdXk0Qlh0Q1ZvK29IcmNUOG5qUmE0eDFMQzEybzNxY0tjSnhSb3hWWXVaUEYvZGNITEZPeU9yeHBiR3hVU2t0UXJVOEdWd1hnVWhreS9kekk4ZWI2bkdtQ01kNXNSUTJHWVdEMTFBOVRvK3FNVkk1TlhRVFMvVTRRNFRqVEJpcnhOeEVtR2tNMjRKVmp3bkhPNk5xakNTa3o5MUtSWmJxY1lZSXgvbXdVb2tKUHRNWTJRanltWlpXQVZvclhqY1BPRVdHcWpHMmtoYW9PeU5IaW10YVpnakhHVEJXaVVtMUNJODV5dmFjQkh4Y3lYYlNyNk5xaktaVUM2ZXQzRWp0c1d0ZVhnakhlYkJ5QXBrbm5LYnhrT2p2UlhjaEgxY1NqbDhYNnZnUUlHeEtFbzZsL2U3S3lCSGppVWhHQ01mR1NXWE5TaVhtbEozdzBGS1F4UzZ5SW43T203SFdYSTZQVi9JKzh2Z1piVmxabkxkUDlUZ2ZoR1A3ckZ4c1p0SkRsZ3J6bEcwS1dUMW1ZZDU2b2I2blUzay9ZVSt5SjIvU2htZGxmai9WNDB3UWpnMHpWb2xKK2pyWmJNUzBVS09TYUsxWXovdHhvV3BzbTRJbmZtZUdxc2RSZGhORVdJUmoyOFpHS2pGM1NrYTNXZG1XRkM4RnFSNUw2d0NmaVpkbWdXNGtxUnJibFh4aWhMSFJibFNQTTBBNHRzM0t5VUpMSHhiVlk3dENmWWFvSHI5RTFSaXJWSnczcFMzUHdqcUJ0d0ZudENNU3dyRlJ3OG5veEQzQ01mRHFyeFMxTkRDeHdxNVFpMTNvTzM0cHhQR3c4b1FMNjJrNmIxcXB5bEk5Tm81d2JCZVBtTnBqVno3YnZIK1c1TWFOMW9wLzNRVnNxWUJkYXNLeG9ZMUIzck9sdEcyRVk0T0drOUdSZTNSajRKVnJxaHBYYkZsdFhxanFzWldWOEtHRmFLa1lHM25DaFEwVW5qZXRWR1c1S1RTTWNHeVRoUy9kUXVsSnpNcDJwRmd2eEdlSzFvcC9oVGdPUEY2MlRkMzVVc0s2aGZNNE00OE5JeHdiWTJqVGoxVGJSRzlEOWRnMjc5VmpXUWwvVStDeGJMcnhQYTZMcW5FV3RONDRXcmpwWWxNUXd3akg5bGo0c2kwVVA2cW1TbWhmaUNjbnBYOHVxQnBqSFpYRkJLckhDSTF3YkkrRmxvb3pyZHRFczIxd0ZnNERETnEvTnJMSlFBZ0wzN3RYVWpYT1FwQnR4RDJ5Y1BQRldEZWpDTWVHU0NDd2NNSFJQanUyOUNwaERyeGVHT1ZtcnRUUFJhanhiYkJOOVhuY1VQV1loWGtHRVk1dHNmQWxVeldoWWdPbUU5ajNObEQxdUVSZWYyOTVYeXhNMDhIckxHeVFZNkY2UEdhc216MkVZeVBrMGN3N0E2OVcvY2xLd2p0VEsrenpYVDB1c2JWaUliKzNUL1FhMnhkcTVyVlhScXJIYmdPY0V3V3ZBeTBRanUydzhPV3lVRFd1c1cyd2ZWU1ArL1BkYTB6Vk9BK1ducTVaT0pmVFptUU00ZGdPQ3kwVlppcEdodmJweCt0OGZ5OUthN254SFN5b0d0czNEL0EwSVJnajUzSVc1aGxET0RiQXlFSzhHME5WNHhvWGN2dmUrYnpvRkRiTnhPczBBa003ZCtKMUZzK0xGbDR6Qy9NTUlSemJZT0dSakxtS0c5WGpiUGkrTUpiU1d1SDc5K1RpYjkvYzkxaS9HT1ExYTE4dlFOK3hJWVJqNVdTVnEvWXYxVXpoL3Z1N29ucHMzM3ZQanl4TDZVZjNka01yeDkvQ3pwMTRuZVh6b2ZZQ2pkc3hqNEJzQk9GWXZ4Tlo3YXFaMlQ1TnFUak1GTHdVOU9QdG9pNnRCcmwvSm1hZTI2QzR5YlJ2WnJGcTNHRGhPa1E0Tm9Kd3JKLzJMNVBKeDNBcmVCeHNIOVhqZHJ6OWZsU05zMkg2UENnYitWd3BlQ212ZWMvTVl4c0l4NG9abVcxc1BrUklTNGoya3lxMjgzbHh6NzN2Mk9mdlI5WFl2blBEclhGTlZJL2hCZUZZTnd0Zm9seEdYMDBMM0FBaU45NTJvcEtXZzF4Yks3eHQ4Q0RIbTZxeGJZdGNibkNrSlVyN3BpRE1QRGFBY0t5YjlpL1JsVHpLTWs5K0QwNWF0dTE1cmg3bk92UFk1OU1lV3BMc084bmxQQzYwUDgxazVyRUJoR09sNU10enFQeGxaaFVlWlBEOXVZS1hndTZtSG52NmNtMnQ4UEo3eVhFbUhOdVdTenZGZDBaR2ROSmFvUnpoV0M4TDQ5dThiU0NneGRQRi9aVHBGYVo1cXg1TE5lMG1zK056NDdGS09EVXdTUWViemVSOGx5UHQxV09lVWlwSE9OWkwrNWNuNTIxMlQrZy9ObzNxOFdaVWpWSEorZTA0NHlPaFBSd2YwbHFoRytGWUlkbUdWWE5MeFNMbjFmeXlXT21ZZ0d6V25zY25MOWNaZlE0V0hzY3VValcyNnprWVo5Wm4vSUtjdzdVLzlhRzFRakhDc1U3YTcraXZjejZ4Vmo5V1BWTVpzOHZMNm52NW5PZHlJK2p6OStDeHNGM2pIRnZpMXFDMUFwMFJqbldpcFVJQnFiSjlLT0YzelpEYnF0WFg5NGh3M0NESGRkL1hpMEpVSDJUaGNmYms5OVQ4MUlmV0NzVUl4OG9ZbUZJeEw2VHE4SXlBYkpxdjZuRU9yUlVMajZHSVRUOXMrcERCYnFadGFmOTlhYTFRaW5Dc2ovWXZTeEZWNHlZQ3NsbFVqMy93RWhLb0dwdTBLRFFZVndhdVY3UldLRVU0MWtmN2w2V0lSM0tyNU1MeUs0djB6UEZWNWJSK1UrZ3JHRkUxdHFWZWZGZGlNTGF3MCtXaHg4azY4SWh3ckloOFNUUzNWTno0Mm5iV0lta25PV0lPc2ltdWV0eDdnYXU4OTlvM0Z0akVTeXNVVldOejNIbnFxS1EydUExb3JVQnJoR05kdEg5Smlxd2FOelhHdkYzcGVWWFl3bGUxMCtybjMxYzQ0Qkd3SFZkU01TNjJtTkZBT0VacmhHTmRDTWNHdVBGZVR4ZjNZK2xEcHMxQ3Y3Yytxc2NHTHJLYjlIN2RjdnplQm51RjhLWHVMeDduUG01elZ3WjJ1c3g1TXhhekNNZTZhUDZTWEhHeWZVbjYrRnlieFoybTE0VzFlbGVQNWZHMHRaYWFtYWZxSWIzRyt0MUpHMFdSL2NWYmFDN3M3QTBuSTZySHloQ09sWkF2aCtZZHA2Z2FyK0dDeDlQRnZidXArVVFWV2JWU3E4ZFVqZlBuemp1ZjNIbUlOb3FOdEYrL3FCNHJRempXUS9PWHcrZU0xQ3c5WGR5N2FRWnVSdlY1NmNkQ01SODlzOWErQno1ZUwxVmp2ZHo1NWtET1A5akFRR3NGbFdObENNZDZhQTdIQk9NZFNDK3kyM0w2Rnhic3FmUys3NDVVVXBtejBrYlRlN3FNSEMrcXh2cTQ4OHN2N254RHU5dk9ORi9IOXRrdFR4ZkNzUUlHZHNVakhMY2dyUlpqUXJKS1BxcWdWbG9ycUJybnB3N0ZZMW9vV3ROK0hhTjZyQWpoV0FkYUtqSkVTRmFwZC9YWTBNMWlyOWNweCttOXY1ZURIZ2pGUFRHMUFtMFFqbldncFNKamhHUjFlbFZERFZ4a0sybXA2UHU0bmFweFdndENzWGVhcjJlRVkwVUl4enBvZnB4eXErQTFaR0VsSkg5aHVrVXlKVlNQZTdWK1VEVk9haUhuaHdOQ3NYZmFSN29kS1hnZHhhc0l4K25KbDRFUmJnV1JrSHdxMHkwSXlXbjBtbHdoczJTMXZtOCtXcUdvR3NmWERNV25MTFR6VDQ2cDVnVzE5QjByUVRoT1QvT2psRHRPME9ISWRBdENjaHJUNFdUMHB1ZmZyUFhHc1crdjhSc3UwbEVSaXVPaXRRSmJFWTdUbzkrNGNJVGtKTnpUbW1uUHYxanI5NlB2TkkycDhxZFp1U0FVcDZINXVzYllSQ1VHeStXeTlHT1ExSEF5K3FiNFF2UUwvVzd4U2VWdUtvLys5MHY3L1NOYVNERHBIRXFHazlHanN2ZG8vblJ4MzdtZldqNTdqNFRqb09aVlZibE5PeTRKeEdrby9ONDIvZlowY2M5YW44U29IQ2Vrdk45NFRqQk9vNjRrUzhqNUlCZFQrSmRqOWJqdjY2RnFISTc3SG45dzMydTNveDNCT0NsYUsvQXF3bkZhdEZUZ1ZXN2hGeUU1cUw1YlNtdmJFS1R6NjJrOHNZQmZ6VkJzWlFPWjNHbXV6QktPRlNBY3A2WDVTOEJqSFVVSXljRzRiVnM3QitTbmkvc0hSZS9IWEY1UFZ5ZFVqYjBpRkN1bGZHTXJ4cmtwUURoT1MrMlhnRjN4ZEdxRTVOK1ZqeVN5cE8vWU1pM0I1NnpudjgvNE5qL3VwRytVVUt5YjFvMThtSGVzQU9FNEVSbXlyM1ZCQUtGTE9YZno4blJ4NzU0OC9NYjcxVnV2NnJHaWNOejVobForZnhaLzlsT0g0bU1XVkpsQWF3VTJJaHluUTBzRmVuTVhZVUt5RjUycnBySndkWmI0OVVyU1Y5VUFBQ0FBU1VSQlZNOTZMcUNsYXR3ZG9kZ216ZThWbGVQRUNNZnBhUDd3YzRJM1ppVWtYNVYrUERwdzFlTStHMStrcmg1M2JxbWdhdHlaKzU3OVNpaTJTZnJ6dGM2VXAzS2NHT0U0SGEzaGVNR0ozaTRKeVM3cy9FSklicTNQcEliVTRialBHZ0VtVkxSekpUUGd4ejBYUUNJOXJkZTZmUTg3ZUtJSHduRTZXbmZDNFdTZkFmZUluWkRjMnR2aFpOU3BZaU16YTFNdDhMbnBPak5YZnQ5RC95OHBTODFRekF6NFBOQmFnYlVJeHdsMHZRQkhRdFU0SXlzaCtaeXRxYmZxMDN1YmFzSkxuNytYWHVQWDFWczhFNHJ6eEtJOHJFVTRUb04rWTBRbElkazlQaitRaXowaGViM08xZU5FNFhqUjllK1YzMVByRTZ6VTZsQjhJTHRWRW9velJOOHhOdmsvamt3U211Y2JaeG1PcFgvcnhYRXZzYmRhSHIrZkRpZWpNK2sxWmJ2Z24wMjczQ1M2WXp1Y2pOeWo5L2NSWG1QdHVzYzJ4RlNOZjdhUXhZMUZiKys4NWdieEllUGo0YjdyN3hTOGpsVzBWU1JFT0U3alFPbnJNanNLckJGK2orWDQxajhiVitFUEo2UG1mM1dqdUw1SnovV2ovR2UyRjRTVmtEeVdRTWpFZ24rOWMzUElPMVlMcjJPSDR5Ny9rbXd5UU5YNGg3bmNMUFM1MlRDaGNhNDhrbk9rKzg4MzIzclBHK2ZMdVp3ajY1OWI0K2ZLQjZYaDJHMEc4cWJrbTdTVUJzdmxzdHpmUHBIaFpLVDFvSDl4anhBVnZJNnQ1QVIvSW1INE9HQ3dtOG5KOHpiM0M2ZU05RG9sSkQrN2tsN3Qxb2FUMGJkSTFYZzNXYWJUaXZiaFpIUVpPY1JyOVJ5S2M5N0pMdUs1Y2k3blNWUG5TcW1TLzZYZ3BhenpHOU9qMGlBY1J5WVZtNytWdnJ6Zk5XOGIzVGpKanhOV3ZXNmtXcGR0VUNZa2YvZExsK3B4eE9CNUxuM2tyY2p1blA5RWVIMmFaUjJLRytmS2s0UlZVVFBuU2dwV1dFVmJSWHdzeG10Sjd1eW5TaDU5dlpPZnI4UEpLTXVnTElIaFVqYkZtQmI4K1AxVWJzVGFpaFdPdXdhN2tpKzJkeEtLczZ2R0tRbkVUYzF6cGV2RnYxUjgzR2RLUnhyU2Q1d0lsZVBJaHBPUnV6QjlWdmpTNWs4WDk2cDZvWTFWTUxPdEtNdk55V21oSWJscjlmZzI4UEc2a3gwUld5bTRhcHhsS0ZZWWlMZFJXYkdYdFJjZkZieVVWVzViZUFKeUFsU080OU02bmtYTjVoOUdIK3YvVkZITzVaR3RCSXJqUWtQeXVHT2w5VFJ3SDJQWDZtOXB1K0hkeU9TSmJFS3h3VURjdEMvbnlGTmxJVm5yNWxkczBKTUlsZVBJaHBQUm85TFFsN3kzU2NMWFpVYTlyb3RHTlZsdEwzZGJVbjA4TFdSQjEwSm0zYlorR2pDY2pLNERoUmUzSTk1Smg5ZnpScVlMbERDNjcwckNWemJ6aWFYTnFmN0o1VDEwbGVSeDZwc1g1V3VCV0pTWEFKWGorTFFHdjJSZlBnbGJad2FySU52c1NZQjhQNXlNc2duS0VqakdkZlVuODVDOEo5WFdMamVPNHdCaGRONnhEN29xWktaMVZxRTQwMERjNUs2SGY4blR0bW1xOTgxdEJySXkybE1UcmFOZnMwYmxPQ0xsSTJQK202SlhkamdaMWNHanBJMG9GbElodjVRZG1reHJWSkp6dllEM0dabDJKRGVlUG82TCs5d2NkL25NWkY0MXJyOVBaem1FWXZuTWpPV250UE9pdTdFNVMvR1hSMWduMEJVVEt4SWdIRWNrdmJSZkZiNjA2SXZ4NUdKOW1XRzF1SzI1VkpUTkIyVjVUM1BkZGU5RDEvNUlUd0c1Y3pDdWRKOTcrc2htTjd0R0lENWhoT0p6bi9nNDludXFlRkZlcHpZcTlQTWZqbDlVV2grUFJLMjJ5SVZBNjY1RXNlM0xDZm52NFdUa0h1MU5wUkpyanJ1WVNZWER2ZjR2RWw1eTBYa2hyUVRhWXhrWDFZWDc5NDU2M2p6bGRIRmR5T2ZMOVlLZldnM0c3bnN1My9kSDZYZjlTREIrNXE0TEQzS2RpRW5yVXdmYUtoS2djaHdSajIyeXJXQ0ZNSlBLK3JYVlI4VlNTYzVsYTJvdlQxZGFUbUx4TnZZcTRxNTlJWm5mNGxsdWZPdU5qSmhFc0Yzbkp6WnRhVzU3ZkxxNEh5aDRHVVVoSEVma0tvTktUNGhSVGtDS0gxdHBaMzZHY2lhNzdubnJ5NWVGVnNjeTVQOUlndXRDbnFnOGIxZnVhK0dtM0tUOHo4ZWZsWWpwM2V5TWoxN1RJRXJ4UnZuM3BOTzhkWFJIT0k1SThSYVZ2NGJ1ZDQyNHBXN3VUTTlRTmg2U1RZNVVVcjRRK0RVejZTZTIvRmtuRVB0eDlYUngzM1ZLeTg0VWoxcGxuRnRrakhLTFJITWZLY0hZbE9mTlJxUUtiMjQwWEdOcjZwSjMzY1Byek81bVY4RG90VlRjT013cVFrRFdHbzZQVW81YkxSSGhPQjZ0NFhnZThnOG5HQWV6T2tQWjFHaTR3bmZkdzNvbVE3RXNISnNTaUlPTEVaQzFyZ3ZxTkVvUzNSR080OUc2UDNxd1BpYUNjVFI3MHN2OWNUZ1ptUm9OdHhLU3gzeGVpblFsbjFjem9aalJhOG1FRHNoYTEzUjBucGFEYmdqSDhXaTk4d3R5UVpMTlBRZzY4ZTAzZ3JLWmlSY1NqRzQxNzdwbnVPZFA2MDJTcWQzc21EU2hoZ3ZJRDRFMkN6Ry9LUlA4WUVGZUpNUEo2RnJwd294UHZrOHkwbmYzcDg4L0U3M05aTU1FRXhNdkdydnVhUW5KczZlTGU2MVBmN1pTdHRESVRDaHVqQ01rRU92enUrLzFGbkxlK1Vmakw4czR0N2dJeDVFb25uSHNkUldzNSsxeUVZYVowWENLZHQwN2Y3cTRueWI4KzN0Uk1FYlJ6RzUyakY0em85ZXVrWnRvblNwRk9JNkxjQnlKNGlIODN1WW55a1hsbGdxTEdRc3JFeThVaEdUVGMwWVRWc1FzaGVJNkVOTU9ac2RNQXJLM3o1YmkvUWdZNXhZUlBjZnhxS3lrZXI3Z254S01UVm1kZUhFdElVWmQzNTFjL0U2bEFocDcxNzByNndQNDNlc2ZUa1pYRVlQZlhFTHhwZVpRM0ZnSXlxUUptdzdsdXVQenFZN0pqWmJnRjVYakNCVDNNWG5aRXJleXZkRUFmbVppNGtXa0RVWGNUY09CMVowSm02VDYvaGc0QktyZnpZNUpFMW55VmxXVlJjR2ZGUjRrNyt1RHNCbVY0emkwempqMjJVNWhjaGNyckdWaTRrVmpRNUdRSWRuckk5dVUzTzhoclFNaGJtSlY3MmJYbURRUjg0a0Q0bkhuZ2FOY3Zxc2JNT3M0b3Y4VTg1dGlIVjloaHd0T3Z0eGp5ei9ja3cvWGkrZUNxTndNcWVFQ21Ud0IrVTAya3ZCaElhdmhzeHJ0Sk5XMTMrWDM4K0ZPcW5aSDJvS3grNXk2a1pMU1EvcVBmSTQ1VCtWcDMyTnJoZGErWHJXNzdPYUl0b29JRkQrbStmSjBjWC9hNXcvUVBQb0dRYW1kZU9GaDF6MFgrS2E1QmVNbWFTMjQ3TEZHUU9WdWRreWFLRjd2aGJPS1d3VHZuaTd1MlF3a0V0b3F5dWFqY2t3UFZKbmV5YytaelBCV00vRml6YTU3dXk2MlV0MGE0Sk1FLzZPV0xTbHFGMjAySmsyd3NLNXNwL0tkNzRPTlFFRGxPQVlGTTBZMzZiV0lnVVY0V0tFNVBCM0xGcXdISzQ4bkgrVmllSnR6cFhnYnFTUWZ5emIzcThmblVZNlB0aXJ4a1R4S0p4Q2pxZmZpUEtXempoZFBGL2YwSFVkQzVUZ09zenRyYmRHckpRUFphWTZHbXplQ2N2S0ZmUFgyMVBrZGNqL2t4a0Q5elFHVEpyQ0RxWWZ2K2x6aDU0c2J3SWlvSEVlZ2RYZThQanZ1MEd1TUZ0Uk92SUIralVrVGJPR01YZlhxUGM3eG1vMTJxQnpIa2VPakVLckcyRlU5OGVLUDRXUjAxd2pLRE52SFdvMkZkV09sMis1RE54Kzl4K3E0OWpCMnlZdURVVzV4YUt4MmRCN2xKQmN1dGxoRkZ5N29mSzJxNm45dUlaOHNDQU9leWFoQTE0N3pQL21jRUl6UnhmdWVJeWRabEZjNEtzZmw2dlBsSjlEQWgrZUpGN0pnVmRYRUM4VERwQWtFTXU0eFRZbW5Xb1VqSEtNTHdqRjhXcmVRVC9YVzFlaUhoWFdJb0U4NDFvcU5RQ0loSEFjbUZ3R05PdDBaeStJWUZzVWdsT2JXMVhQcFQ3NWtJWjk5Y3U0WXl3K0JHS0VkdXM5Y3gzT0gxaHR6d25Fa2hPUHd0QzdHNi9ybFAvSDhPb0JOOW1WbnljL0R5V2pXQ01vODhqU0NTUk5JN0tSajlaaHpUT0VJeDJpTGxncWswSng0b1hicmFyQ0ZNMVRKc2JVQ0VUQ3RBanVUaXg3Vkg2VDJUaVlaUEE0bm8wdFowSVhFM1B2ZzNvL0dwQW1DTVZJNzdEbTFRaHQyeUl1RXluRjRXai9NWFNwdXh3RmVCOUJWY3lIZlF0b3VWT3pJVndwcG01aEtoWTVKRTlEb1dKNDA3Y3pORWg1T1JocC9sMXgzMjFXSHluRjRXai9NWFhxT0NjZlFhazhXOHYwem5JeE9NNnNXcWVPT3J6dk9za3ZtUjRJeEZDTlFvalhDTWRyZ0pBTUwzQ0srVzhXVFlreVQ0L29neHhuUWpxSU9XaU1jb3cxMnE0SVZod1JrLzZTLys1WlJiRENFNnhaYUl4eGpKOUpiQ0ZpeVIwRDJSNDdqbjdSUXdCcXVYMmlMY0l4ZGNYS0JSWFZBcGdlNUJ3a1h0MlovQVpTdXkvVnJWdnBCS3huaHVGeHRWL1JUZllOVmV6TEpBdDJkVVRHR1lWMnVYeHBucUhNZGpvUlJidVVhdHh4Vk15MzlnTUcwZDhQSjZOaU5hT0p0Yk1jZE4yWVd3N2d1RTJ3MFBpM2xCalVTd25GNFd0c1JXR21PMG94cERlaUVYVEZoM1I3WFBMUkJXMFY0OU9vQ09yQ1RYamZ2TGI1b0FPaUtjQXlnRkh2U0lvQWRjYndBbElod0RLQWtQTWxwaHdWQUFJcERPQVpRRXNKeE80ekFBMUFjd2pFQUFBQWdDTWNBU3RKMnZuZnBPRjRBaWtNNEJsQVN3bDQ3SEM4QXhTRWNBeWpGZ2sxQTJwSGp0YkQwbWdHZ0w4SXhnRkpjODA1M3duRURVQlIyeUF2UFBaWjhxL0IxZlduNS8zY2JLQndHZWkxQURHY2M1VTR1MlFnRXhzMDYzT1M1blNIM2VlUExSRGdPVDJYUDN0UEYvV21iLy85d012cFdWZFVmNFY0UkVOVFYwOFg5QTRlNFBkZGFNWnlNYnFxcWVtZnR0UVBpOHVuaXZ0WE5zV3lBUXpndUZHMFZoUnBPUm0ySCt4TXNZSlhybVozeTd2VXlwdmNZaHVWeS9lSTdHQW5odUZ4dGgvdXphaDBXdVl2SjhkUEYvVGZldmU3aytCMXpjWVpSWGE1ZkdqZkFvVWdWQ2VFWU8zbTZ1Q2NjdzVvNkdITkI4VUNPSXdFWjFpdzZYcjlZWTFNd3dqSGF1T05vd1FpM0FPZUlZT3hYSXlEUGN2cTlrRFhPQVdpTmNJdzJPTWxBTzFmVi9QSjBjWC9FMDQ0d1hFQjJ4MWNtM2xCRmhuYk1Oa2RyaEdPMHdVa0dXczBsckIyMG5jU0NidVE0SDhoeG4zTVlvUlJGSGJUR0tMZnd0SDR4anpxRVhjSXhORm5JN05JejJpZlNrSVY2TGlTZnlnU2NxY3hFM3l2eGVFQWxybHRvalhBY250WlY4cTFYNHJvTDRYQXltckZRQVlsZHVWRDhkSEhQem0yS3lBMktHL25tUmtXZVNFaG04eENrZE5kbFVvM01PTmFJSWtBa2hHTzBkY2xtSUVqZ1JxckUxNHhsMDA5dVhLNkhrMUZkU1Q1aEV4RWtrTnNOTk9lK1NBakhhT3VhY0l4SVpuSXpka2tndGtuZXQrZjNjRGdaSFVoSUh2UDBDWkh3ZEFtZEVJN0QwM3BSYjd0RDNqTTNBWURXQ2dRMGJ3UmlwazFrUk41UHQ0WHZtUVRsc2Z5d1JTOUNtUFU0aDJqY0FBUVJFWTREYzMxNHc4bEk0MHZyOCtWM0Y3aXZIbDhMeWphWENzOGxDK3ZLSUtHbHVaQ3ZEc29zNUlNdlp6MytuRTdGb3dnb0dFUkNPRVlYMTNMaTRVS0dyaGFOSG1JZWZSWk1ib2hjYi9LMHNaQ1BpUmZvWTVGcFN3WGhPQkxDY2JrNjN4bkwxSXByVnFLamd4c0p4SmNjUEt5cUYvSlYvMDRNR0xPUUR4MzFYYmhMVzBYaENNZHh6QlgyMWZXdHlwd1NqckdqTytralp0SUVkaVkzVUc0aDM1dkdRcjYzSEVIc29POUdRRnJiS2hBSjRUaU94OXdXbmNqQ3ZDc0NNamFZTlFJeGp3TFJHUk12ME5KVnJ1ZWNwNHQ3TmpTSmhIQmNNRGZvdk9lWGplb3htdWFOSGV2VVhKeWs4bmdzMWFEVjRmN2ZaTEQrZGNtTEFXVlIzSWtjbzlWSHlyZHlqRzVUVi81WEpsN1VDL2xPbUhpQkJoL2J4eDl3UU1zMldDNlhwUitENElhVDBhWFNFUGxiM3p0UnhiOGI0bEM3aGJQc2NqVnQwYlBxd3YxcFNmM1EwdGQ3MmlKYzNzaDdyYXFDeGRiVkVLNXFQTzU3TUlhVGtjcGc5SFJ4UDFEd01vcEE1VGdPclk5NGZOd2RVejB1aitwSkV6THhZTnFoUDlVRnhLK3lxOXM0NTBxeWhNbnJEaFZYZDZQeGJqZ1ozY21OaElxUXZHSHJhb0p5V1JZK3FzYnlwRW1qdTlMZjRKaitVODZ2aWpWNmgyTjV6UG1GZzFzRVZ6WDg0RDQzcmpxakxSaTdLdWh3TW5LZnh6OTdMdHh5dmF5M1VsWE5qdnhlZi9kc1JYREg5eTkzdkxVZEovZTVsT3JoZ1h4ZWJ4UzhMSVRucTUyTHhYaWdyU0lHZWJ6N2w4S1hkdjUwY1QvMThRZEpLS0h2THo4ejZmRlVPMm1pUTJ0QUc3L25OSWRacXFwL0J2aWpWYmVrTkNaZVRGbklsNlg1MDhXOWx6NWh4ZGRyTHkwajJBMXRGV1h6ZVljOFZucENRWHZxSjAxSTJKbktUOGhINTg4VEVuSVlRU2ZITEZSNHJWdFNUdVh2T05OMHpKaDRrVDJmb1hGMTBhNFdUUDJKaUhBY2g5WVB0YmZlS3RkN09KeU16cXVxK3VqcnowUlVKclp3amhpS2EzdFNPYytoWWhOalYwc1hrai9MYm5kbjJrSnl4Y1NMSEgxaHhCbDhvNjBpa2xKV3Z3NG5vd2VxTVdhb25UU3hTcXA5VXdreUtSWlovV0o1ZHFvY3YzOFMvTlVMQ2FLWDJvOGZFeTlNbWoxZDNIdnRFUjVPUnJkS041dnBQVjBLdTZOeUhNOUM0d2xYSGhuN3ZHaWR5RXhVTGk0NnFaNDBzVXBDbllhSktIVzEycXBVcjMxUEtzbWZaZE9nVTYwaG1Za1g1aXprL1FHOFkxcEZQRm9yYzE2SG5jdUZqeE9XUHFvblRheHlvVmhtYVAralpGU2cxajdFWFduNFRycjM4Ui8zdnNwTmoxcE12RERoSk5DTmx0WXR5b3ZkcENnRjJpb2lHVTVHMXkwMkk0anAwOVBGL1pudnYwOG1DSHhWK1B1V1JQMmtpVld5VXZ4VTR3WEs2Z0IrNmRQK240S1hza3JsaGlLYk1QRkNsUThoSnFNby9xNndBVWhrdEZYRTg2QTBIQWNaZU81T1hOTER4d0s5dU5SUG1saEhjeWl1ZWRodVBSV3RjMXRWYmlpeUNSTXYxRGdQT0RKUTYzZGxwdUExRklWd0hJL1d5bDJ3eDhWdWhyTGNpYk9EWGxqenhzSTZVNHZHZXV4bWgzelVHNHJjeVdkWWZTODhFeStTdWZJMW0zOERyZUhZL0NoSmF3akg4V2p0RndxNlZhYnIyeHRPUmhVQjJidEZYY1d5dU0xeDRJMDdZSk1MeVcrSGs1SHFEVVZXeWZkdkt1UHI2cUNjYXFwS3ptSnNncUYxNjJqNmpTTWpITWVqOWM0ditDTkJBckkzcGlaTnJFTW94ZzZhRzRxWUNjblZ6MEdaaVJmK3hOb2RUdXZDV3lySGtiRWdMeUt0czQ2cnF2bzFSdlZ4T0JtNWk4WWZvZitlRE4xSWhkaHFJSDdUbUZGc05oVDdXaERUV05oMUxJOXhtemVvTTZrUzNmb0toWm9YR2UxbzNwaVZiRElrU0ZBZUsxMTNvbDJReFhmckRDZWpSNlhuS0dZY1IwWTRqa2p4RisvM1dNRkxLb2N4ZHVxeTdxWlJKYllhQ0dMdlpoZlMvT25pdnRmNHNRN0hZeUU5dUtkOWZ5L0Y1NTQyNmcxRjFPMjZ0NnZHamRFSlFYa3I5MzVISFR0WmVnRUxQeENPSTFLODg4NFhIeGZnWFVsZjNpV3J2SDh5YS9RUm0zMk1KaXY1eDVtRTRscXZ4N3J5bWIvdUdGQm5FaEk2WHh4bFpuUXViVTNtUTNMMUl5aVBtWGl4VnUvUGZGc3lNZWV2V0g5Zkc0eHhpNDlOUU9MUytsZ2thcCtWblBEYzMza1Y4KzlWeWwwRVBzbjJ4RWR1NXJUaHFsaHo0NDdQbVQwZDZGeTlra2ZxdHowcXR5NDQzVXJBN2lxblI3TDFybnYvczdDaHlDYnVleTdmZC9lKy9pTG5BVVoyL1h0ZE9FNVFLZFU2cVdLdTREVVVoOHB4UklwN2Jucy9NdTVLZ3NObFlXMFdaa2V2cmFOb2krZFFGazhYOTUxV3NVdWcvZHZUNjFyMENRM0R5ZWhieHQ4ejFWdFR0eUhmcDJtQm8rR2l0MUUwRFNlak02VnorZStlTHU2dDc5QnBEcFhqdUxUMkRPM0xJNzdvNUVSNFVFQVYyUVhpYytrZGMxczRUNjFmeU4xalNHVmJQSWZTYVFkSitVNzVyTmp1U1FXNTYzZlYrMDZZaWpTM3B0WmFBZHlKT3kvSStjR2RGMytWODBidTFjTXIyZG8rNWFKanJaOGJGdUlsd0NpM3VEUTMxQitsK2hKS0c4RllncGJxWGRKYVdqUXF4TmtzcHJDd201MUhpeDZoOGpwQXBYWlAvdHd1bGFTenpQckExM0VoK2IyVlhmZTJXUmtOZDl6WWJDU1g5MURUKzZUMWZHYithWWhGdEZWRXB2alJadFJGZWE4eFBndlgvQ3ppVFFvTHhiVk8zd3RwRi9vejJLdnFPTnBKWmdkL0R2T1NWTW9pSksvS1lJYXlxbzFlUExjLytjWVl0d1NvSE1mM29EUmNxSG1rSkNmTVN3bkpZd05oTE50QVhQMjRXWmtXdUtLK1Q5VTRkQXZEV2NmdjdGbGg0Ymk1TmZXbHBRMUZYaVBubWVkempiR2dyUFY5VU51S1F6Qk9nOHB4WklxYi9wTXR5dHRHN3VxbnlrNytXUWZpaXQzc25IUFgrOW4yWDRvNEVxcHI5VGluc1c1dG1kcWF1cTFHVU5iMC9xcHZMK082akZWVWp1TlR2U2hQNHhneE9hR09GUXpReno0UVY0VGlwcTdWM3hqYjNOWi9UNWVxVXM2VFJiWXh1elgxTHVxS3NreEdPazVjVWJhMGtaSFd5akViZnlSQzVUZ3l6WVBHWSs2VTU0TlVTWTdsSjlRai83dkdkcjQ1QitLY2RyUHpvZk9tSHhIWEZmUVpNVmR5OWJocExxTWtUVzhvc2szalhIa1VzRTF0SmpkcjVzNlZpbmZHVTdNV3FEU0U0d1Q0SW9ZaE54N3U1UCttc1pwL2x3dkJYRllFZjVNZzdQNzVvWVR0T2duRkcvM1NaZFJlaElWNHF6cmQwTW9zM1grQ3Z6bzdzdGgxYjFkeXJqeVFuL3FjZWJEajA2STcrYzliT1ZjK1d1NkxwV0NGZFdpclNHT21kSEdUNlVIamNvSm04Y0lPTXQzaTJaZXJIak9vWTdWVTFFNjY3TjduZnIvaFpIUkY5Zmk3ZXRlOXFmU2ZaaDJTV2VUMWd1YTUyTFJWSk1JbUlHbG8vY0NYTktLclNKbHY4ZXhMbjAwL1l2ZkN2Kyt4S1VpV2k5SjZ5bUpyYXJTaXRTaTB5R0hIUjZzSXgybW92UnUwdnJzVTFsc0p4VlFMTjd2cjBVNXpFdU1GK3ZwN3BYcDR0OFAvdFZUTlhmY0l5Zm5TR282cEdpZEVPRTVEODRlZVBkd3pRaWh1clUvUGZleVdpbHFmVU01aW4rMEl5Wm1TOTFQcjB6TmFYeElpSENlZ3ZOK0xjSndCdDhoa09CbmRFb3BidWV2NjNaU0xiS3EycEhkZFd5dW9IcmRTaCtSYldjUUYrelMvajFTT0V5SWNwek5UK3JvNDZSdldDTVYvMFVQZVdwOHFhcXFXaWxxZnFqWFY0M2JxWGZjSXlmWVJqckVXNFRnZHJkWGpQZnFPN1NFVTl6YnIrVVFuVlV0Rjc3OWZmdSs1MzVkVEJFS3lmVnJmdHptTDhkSWlIS2REM3pGNmM3dlpEU2VqUjBKeGIxMTN3NnRiS2xLUFpqenMyUTlMOWJpN09pUS95TzZTTUVDS1FGcDNBYVZxbkJqaE9CMzZqdEZaSXhSL1padm4zdVk5dHhHZUt2azkrbFNQTDZrZTkzWW9XMU0vRXBKTjBIeWRZekZlWW9UalJPU1J5VUxweTRzOXF4VTdJaFFIMGJkcW1ycmZ1Tlkza0ZFOTltT2ZrR3dDL2NiWWlPMmpFeHBPUnRlS2craHY3S0trZzB3aXFIZXpJeEQ3NWFyR25kc1I1TkhzMzRwK24xLzdiSHN1TjE1OHh2eWF5NDNIZFFsYlUxc3huSXkrYVIzajluUnhQMUR3TW9wRzVUZ3R6ZUZUU3pXc1dDNFVEeWNqZDFGMWdlVVBRa3NRZmF1bFdsb3FhbjBybFoxN3I3SFJ2anpwY1pYazB4NDdHc0lUV1VDcGRiNHhveFVWSUJ5blJkOHhmcklTaXRuaU9SelgxblRkODAvWGRoUFo5L1ZjS203M3NxN2VtcHFRbko3bTRnOVBiQlVnSENmVTUvRm5CSDFYdjZNbFFuRjBaMzBlY3c4bm94T0Y3OUcrdks1TzVIaFFQUTZMa0p3ZWkvSHdLc0p4ZXBvZm9WQTlqcUN4eFRPaE9KNkZoeENvdGZyVTkzV2RVVDJPb2htU3p5aEd4S0ZrOU9KR3JQWFJnWENjSG4zSGhXcUU0bnFMWjBKeFBIMnJ4bTl5RGNkVWo2TnozL3VQc2pYMUpTRTVPTTFGSC9xTmxTQWNwOWUzNXpHa2R6enk4MjlOS0VaY3ZxckdXbTltOWp5TUVDTWNwL0dla0J3Yy9jYllpbkNjbVBRZGEzNkVTV3VGSjRSaU5TNDlqTlRTL2xURlIvWDR5dC9MUVV1RTVBQ2syS041amovaFdBbkNzUTYwVm1TTVVLeE9yNnFvZ1F0czVlbXBENXVDcEVkSTlrdDFzWWQrWXowSXh6b1Fqak5FS0ZicFNuYW43TVBLcm1kOXE4ZVBWSS9WSUNUN29mbDZkcVBnTlVBUWpuWFEzSGU4MTJjMFZJa0l4YXI1cUlaYUNjYytYaWZWWTEwSXlmM1FiNHlkRUk0VmtBck5YUEZMSkJ6dmdGQ3NYdStxc2ZZeFVDdmU5ZzFRVkkvVklpUzNwSFF1ZVJQaFdCSENzUjZhcThlRTR5MWs4NDRIUXJGcVBxcWcxcjRMUGw3dnBZYy9BMkc0ODgyRG5IL3dPczNmM2JueVRjR0tRempXUS9OZEk2MFZHd3dubzZQaFpQVEE1aDNxM1hub05YYW14bjd2M3EwVnNraUkrYXQ2UFc4bTRzNUQ3bnhVK3NGNEJTMFYyQm5oV0ltbmkzdk5sZU9LNnZIUGhwT1JDMHAvRzNyTVhyTGVsVFVKSHZ2R2p1R2hwOEJFWlZJL2R4NzZXODVMYUREUVVxSDkrbDhjd3JFdW1sZXJFbzZGRzVFMW5JemN5ZXdQRlM4STI5eDVHcEZrWlNIZUtxckhaZm5EblovWXdPa0Y3ZDlkS3NmS0VJNTFvYlZDT2FuQzNScVljNHNmZkZVOXJYNytmYjF1cXNkMnVQUFRMVzBXSnVhUzMzallsQWllRVk1MTBmNW94V3Jsekl2aFpIUXN3WmcyQ2p1OFZJM2x2YmZXVWxIYjl4R1M1RGpPdkwwcWhIWW9BYm4wWFU2MTM5VFNVcUVRNFZnUldUQ2srZUxqWTljdGs0YVRrYnN4K0l0RmQrYjAyZzJ2d2ZxTm9hOCtWRi9IRTNHNDg5VmZjdjRxRlMwVmFJMXdyQS9WWTJYa3d2SzF0Tjg3QTNPUEMxMnR0eFI1ZWYxUEYvZVh5bWV5WTcydkpRWmttUUg5VnNGTDJXVG1hWW9PUENNYzYwTTRWb1JnYkpxWEhsa0RLOTEzNFhQTkFMM0hOcFVZa0xYL3Zzd1FWNHB3ckl3TUF0ZGNtZkUxR2tvOWdyRnBjNmx5K3BCTG9LQjZqTklDc3ZiZmxYNWpwUWpIT2xFOVRrd1dzUkNNN2ZKVk5kYSswcjJORTQ5ckJxZ2UyL1cxaEFLSFBDblJ2SWlXbGdyRkNNYzZhWC9Va25VNGxqNDE3dWp0OGxrMXptbDg0WjduNnZIQ3g1K0ZKRzdsUEpjejdkOWRGdUlwUmpoV3lFQnJ4VjZ1aitha3NuYk5WQXJUZk41YzVqYmIyK2Z2dytRS3U5ejVMZHVOUXVUM2VxL2dwYnlHZm1QRkNNZDYwVnFSeGhsempFMWIrQXB0VWxuTGJiTVhuK01ZejZnZW0zYVljWHVNOXV2VFRJcGdVSXB3ckpmMnU4cTN1VDJXa3g0MTdkVUd2TzdNNDI1VHVlNEk2YXUxNGh2VlkvTStacnJ6cWErNTNxRlFOVmFPY0t5VWdkYUtLcWVxZzFUVE9HSFo1cTFxTEhKOU91SXpPRkE5dHU4eXAvWUtBd3Z4S3RhMDZFYzQxazE3VmNibjZ2ZlVMdWt6TnM5YjFWaWVpdVRhWG5QbzY2bVBIRzl1S20zYnkrdzl0TkJTd1pRSzVRakh1bW0vdTl6TG9ib21ZOXR5NnkwdEVWWGozYkV3RDAzdjVEeG9tcEYxQXR4TUdrQTRWa3p1TG1mS1g2YjIzcTVkY0xLeTc4cGpyM0ZWUURqMjl2dkplZXJLMTUrSFpISTREMXE0SG5HOU1ZQndySi8ycXN5KzVRVWRNcEpPZTM4YXR2UFcveTRiSk9UK21mQzkweVdiZ3RpM2IzbEVwN1Q0YVgvOU41NXY0aEVJNFZnL0M0MzdKcXZIY2pMbGtiQjlWNTU3K0VyWlhwZnFNVlpadnNrWkcxZzN3a0k4SXdqSHlzbGRwdmFMemx1ajI1Rk9XWVNYQmQ4WDlGeEh1SzN5L1h0U1BiYlBjdlZZZTVGbTRYSG5UZ1JHT0xhQjZuRVlwVlFJYythMWFteGtESlF2K3o1dmFxa2VaOFBjVFk2UjlqaXF4b1lRamcxNHVyaS9OakR6K0wybFRVSG9OYzRHVmVOK2ZOL1VVaG16ejJMMTJFSnhoaFkrUXdqSGRsaTQ2RmlxT0ZBMXR1OHV3THpRMHNLeDE5LzM2ZUwrMXIwdlB2OU1KR0htL0NnajZMVFBKR2U3YUdNSXgzWllDTWNtTmdXUkN2ZGJCUzhGL1hpOUdaT1dpdEo2MFBjQ1RKdWg5OWkrdDRhZUJGcjR2RkUxTm9ad2JJUlV5RzZVdjlvOUk0KzNjcGpOWExvN3FWTDZWT3JUQktySFdFZjllVktxeHRvTEhRdjZqZTBoSE50aW9YbzhOVkE5THUzUmVZNThWNDNmRkx4TFlvZ25QbFNQN2JOd25yVHdPYnRtdHJFOWhHTkRqQ3pNVTEwOUxtU0RoOXlGcUJxWGZNTzBGNmg2ckgxM1Q3ek82elFUMzR4VWpTdGFLbXdpSE50RDliZ2Zxc2IyaGJqWWxQNjVDUEg3RXdyc08xYjhHMWlvR3JNUXp5akNzVDBXd3JIbTZuSHBJY2k2dVR4QjhVWVdIcFhhVWxGNzUvdUdWalk4MFA2a0M2OVRlYjZrYW96UUNNZkdHQnEwcjY1NkxLOUgrOGdmdkM1RXRZZ2JwbitGT0E3MEh0dW1OWUJhK0Z5eEk1NWhoR09icUI1M1kzR0xhL3d3RDNTeFllYjF2N3gvWDZrZTJ5ZFZXaldvR2lNR3dyRkJEVFhaYVFBQUlBQkpSRUZVaGhhN2FLc2VhKzZmdzNiZXEwWFNVc0hUaEg4ZEJwcHRTL1hZTm0zblRTdWZKNnJHaGhHTzdiSndWN3FuN0VSRzVkaXVVRlZqV2lwZThuNDhxQjZicCthOGFhaHFmQlZnOTA1RVJEZzJTaTQ0Q3dPdi9xT2luWmFzN1BpRW40VzZ5V0pEbUpkQ3RaaFFSYk5MMDNtVHFqR2lJQnpiWnFXblNjc0pqY2ZuTmdWWjJNTE02N1VPQTgyMlBUTnlNNCtmcVRodkRpZWpzWkdxY1lnNTdJaU1jR3libFF2Tys5VEQ1QlZWcjlGZXFKdEFGdUt0NS8yNHlBNWhMRkF5U3NuYUVhckdpSVp3YkpoY2NLenMyWjc2d2tnNHRta1I4TE5Edi9GNm9ZNEwxV083VWhjM1RvMDg1UW0xTmdLUkVZN3RzM0kzL1ZiYlNDQ1ljQ1kzZ1Y3Slo1R1dpdldDYkJ0TTlSaGRTTlhheXRvQUpyTmtnbkJzbktGTlFhckVqNXNJNXZhRXJCclRVdkc2VU1lSDZyRk5LWis4bmNya0krM1k5Q01qaE9NOFdMbGJkUlVwcGdOZ1YwR3F4b0tXaXRjRkNjZnlmaElnN0VrU2ptV3R5RWNqUjR1bkloa2hIR2RBcXNkM1JuNlRVMjNiU2tPdElCZWI0V1IwWXFRU2xkS2VIS2NRQ0JIWWxaVWJxWkJQdVpBQTRUZ2ZWcXJIZTV4RXNJTXJxc2JKQlRsT3hsckJrSWpjbkZrWTNWWUZmc3FGQkFqSG1aQzVpbGFxeCs5Wm5JY3RndHpzeVZNTHd2RnVRaDRuRmk1aEkvbWVXaW1pVURYT0VPRTRMNVl1T0p4TXNFbklyVmRwcWRqZG5teTg0QjNWWTJ3eE5UUk5ocXB4aGdqSEdURldQVDVrY1I0MkNIbVRSOVc0SGFySGlFb1c0WDAyY3RTcEdtZUtjSndmU3hlY21JdnpIaUw5UGVnbldOVllQbXZ2ZUg5YWVSZnFPMHIxMkpTWTJ5RmJtbVpDMVRoVGhPUE1HS3NlNzBVOEVYSUNzNEdxc1Q1VWp4R0ZzVVY0VkkwelJqak9rNlVMempzVzUwR0U3RFd1MlBpanMyRGgyTmdZeXBJRkx5N0lFd3FxeGxDQmNKd2hZOVZqNXpKMGU0VWNFK2dXckFvamZZeFdLbExhdkpQakZ3clZZK1dlTHU1anRLVloyUW12b21xY1A4Snh2aXd0ZHR1UGRJRmsyMXE5N2dKZmdHbXA2Q2RrOWRqYXpYeHBncDgzNWVtaGxaM3dLcXJHK1NNY1owcUNocVhGTGgrSGs5RlI0TCtEUlhsNmhiNDVvcVdpbjlESGorcXhYa0hQbXdiYkthZ2FGNEJ3bkRkckY1elFKMGpDc1U1M0lkdGVwQ1hnMFBneFN1MHdaR3NGMVdQVlFwODNUdzNOTkhhbVZJM3pSempPbUN4Mk9UZjBHN29MY01oQVR6aldpYXF4RGFGYlU2Z2U2eFRzdkdtd25XTCtkSEZ2cWNxTmpnakgrVHMxMW12N09XQjdCWXZ5OUFsYU5SYUVZeitDSGtlcXgyb0YrWDRhYktlb2pLM2xRUStFNDh6SjR4OXIvVkZCcGxkSUpaMUZlYm9FclJiS2paYWxSN2FhSFVaWUYwRDFXSmQ1d1BHSzF0b3AzSTM4dFlMWGdRZ0l4MlZ3NFhodTZEYzlESGlSNU9TbUIxVmplMkpVajJlWkhqdUxRbFdOcmJWVFZOeTRsWVZ3WEFDcEhsdjdZbjhNdERrSXJSVjZ4UGhNTXNMTnJ4akhrMGtBZW5ndkpzaFRRV3RGaWl0bTVaZUZjRndJV1VSZ3JaL3ZPa0I3QlpWakhXYWhMemEwVkFTeEg3cTFRczVWbHA1MDVTekVkL1RTMEdZZk5hckdoU0VjbDhYYUYzelA5NElOcWFMZitQd3owVW1NNmlDTFo4S0kwYXBDR0Vudnh2ZklzdUZrNUQ0Nzc0d2RoeStCdDdXSFFvVGpna2lsenRMR0lKVnNYZXM3NUZBOVRpdldPQ1JhS3NJSWZseXBIcXZnOVR3cGM3S3R0Y3pNYWZNcEUrRzRQTlpHdXptblBoL2x5b1dYcVJYcEJLOEtEaWVqRTRPUGJxM1lsK01iR3RYamRCWUJibUN2TGJaVHNPRkhtUWpIaFpISFE5YnVoUGNDakhkamtIc2FWSTN6UVBVNGIxNi9vOFBKNk16Z0xwVjNiUGhSTHNKeGdaNHU3azhOWG5RT1BZZDZIcFdsRWFzYVNEZ09LOWJ4cFhxY2hyZnpvenhsc0RhMnJXTE5RdGtJeCtXeU9QLzF2U3pvNkUwcTZOYjZyNjJMVWpXbXBTS0t2Uml0RlZTUGs3anl0UUJOK293dFZsL1BueTd1ZzIyYkRmMEl4NFdTeFhrV3B6YWNlZXcvNXBGWlhGU044MEwxT0U4K2o3ZkZQdU01bnprUWpzczJOcmd3elZ2L3NlRWJCSXRpVlkzZDUrSjlsa2RRbnlqaG1PcHhWRDZyeHBjRys0eWRLWXZ3UURndW1OR2Q4eW81NGZvS1d2U1Z4VUhWT0Q5Uldpc0VsYnc0dkJ4bmFYK3plSlBxRnVFeDZoT0U0OUk5WGR5ZkdkdzVyL0kxLzFpcUpPZCtYaEkyaUxucW0zQWNWOHpxOFV6Ujc1MmpjeDlWWTJsN3M3amdlV0YwTFE0Q0lCeWpNbHc5L1dNNEdSMTcrSE1zem42MkpFclZUMW9xck8yK1pkMzdBRnU4YjhKVG5uQVdQcjZuOGxtdzJHZGN5VXhqZHNMRE04SXhLbG1WKzhYb2tianV1MEJQMmt1b0dJUnhMcjNkTVZBMVRpTlc5ZmlXcHp6QmpEMzEyYnIzYUYvcDcvaWFtVHhGQlo0Ump2Rk1aaDliZkd6cFpZR2U5Sm14T00rdjJLdStDY2RweER6dUZtZTBhM2ZqbzgvVzhBSzhpdUlJVmhHTzBXVDFzYVd2QlhvV3AzZG81cXNhdFJVdEZVbTlpOVZhd1ZNZTcrWStqcWZoQlhqT0YyWWFZeFhoR044WmYyejVUcllvN1V3dXZGUWYvZmdTc1oyaTRuMUxMdHJ4bDgrVjFUWXdiVTc2M3NES3VvK3ZSbi8vbVR3MUJWNGdIR09WNWNlV0gvdnVvRWRmb3hkM0NTNDRoT08wb2g1LytYeFpuTEtqU2U5ZDRHUzloK1hSWnp5RndGcUVZN3lRd1dQTHIzMG1XTWpqWVI4VE1FbzFqeDJVYUtsUUlWcHJSY01KL2NlOUhQZDV6NHhQcHFob3A4QnJDTWY0U1FhUExUdE5zSkNUL2EzaFJTV3BMWHc4cHUyQXFyRU9zYXZIZFJzVTZ3UzZjZWU1Mnk0QnVYR3V0RGlab3FLZEF0c1FqckdXNGVrVmxWUXlXcDMwQ2NaZW5DU3F4QkNPZFlqK1Bzam5qVWZqM1hVTnlHZkd6NVY4WnZBcXdqRmVZL2tFc25OQUpoaDc4U0h5QXJ3bTJtQjBTUEkreUJpeUR3cVBoeFd0QXJLTWJMTTZtY0w1UkRzRnRpRWNZeU01Z1h3eWZJUU9KZlJ1UkREMjRrUEU3YUZma1A1eXF6MlB1ZG5ydXlGUFYvTDVJeUIzdDFOQWxpMzdMUWZqT3piN3dDNEl4M2lWbkVnc3J3by9sRXJIVHdqR1hpUUx4aUpKR01OR3lhcjRCT1RlWGczSU1nbm9EMXUvMGdzTDJpbXdLOEl4ZG1GOWM0ejNxd0daWU55Yit6ejhtamdZVjRSamRaSytIL0o1L0kxRmVwMnREY2dTakszT01xNjVUWWtlZGJ3VWFFYzR4bFp5UXJGK3gvMDlJQk9NZTNNTE5ZK1Y5TzBSam5VNVNQMXFwUGY5MlBDQzR0UmVCR1JwbGJIZWluRGxZNHRzbElOd2pKM0lpY1g2NWhqdnBXZXVUekIyRjl6L3l1UGJFcXRUTjRxQ2NjVU5qanB2TmJ3ZytYd2V5K2UxTk82ODlMdWNwN3JlSU5RQitWak9sNWI3K3QweG1DcDRIVEJrc0Z3dWViK3dFeXF1M3l1bXozTjhwYUp5V2NqeGNCZmNVMDJMV1lhVGthdFMvcVBncGVDbFh6UTl2cFliNHROQ0ZtN09aS1RpOC9Ibm5QM3NWNlpUb0MwcXg5aFpZL2U4RWl1bUw0SnhKZFdwcDR2N0krTWJwdXpDTGNnOFVyaktPL2tqZkt5bDZuMlJ6MjBKYlJadXg3ZWo1bzJKbks5S2JqRmhiQnM2SVJ5akZUblJsUGFJNnFkZzNDUWJwdnhxZktySE9ndTV1QndyWGNoQ09OWXA5amJTV3pWdVpEOWxlSE4vSjlYNnRUdStGUnlRYnhqYmhxNEl4MmhOVm9SZkZYTGtYZzNHTmJuNEhrc3Y4anpxS3d6RHZiOEh5aTh1aEdPZDFDNlNsTS96UVNibkwzZWUrWDJYbTljQ0EvS2NzVzNvZzNDTXJxWUZuR2gzQ3NaTjdzYmg2ZUwrd0hCSXZwSXExTGpON3cxWTRUN1g3dlB0UHVkR1EvSmM1b3NmdEpuQVVGQkFYa2pmTmVjdmRFWTRSaWR5NGpuSnVQKzRkVEJ1TWhhU0Z5dWhtRm1neUo3N25LK0VaTzNuc2xrakZIZWFMMTVJUUo3U1o0eSttRmFCWG9hVGtRdklmMloyRkhzRjQzVmtKTkpZYmlpMHJKcWZ5ZnpTYTR0Vmx1RmtkS3RsZEJoZXVKTFFhWXBNZGhqTGo1YnBEaTZ3dStyd3BjeHY5aUxqS1JZbVAzdlFoM0NNM29hVGtWc0k4am1USXptWHlReEJ3cUpjbEU3azV6aEJVSjdKK0xscjZ4Vml3ckZhZDlKL2I1YU1DYXh2Wm1NSHlJVUUxK3VRTjY0WkJ1U1pMTG9FZXZzL0RpSDZjcXVrcFRLYVExRFpsd3Rpa0cyUjVVSjNXZi81Y3R6cW42TUFZZG1GNFFlNUNON1NNb0VJekgvRzVIdmlidnBQSlNnM3Y2Tyt3K1JpNVR2cXJVSzh4VUZHaTFvWDh2NEFYbEE1aGhjWlZpSE9ueTd1bzQrc2t3dHhmVEYrMDFqNS8yYkRzYTB2ckpXRWt1OC9FUyt5U1F3bkk5Y1M4akhuMzlHb0w1dkdpdVZDYm1ycjcrcFJZM3pkcGh0Y2Q1TmFWNEFmNUo5djVYc2EvV1pDTmpDeXZ2TmRFeHQ5d0N2Q01ieko4SVJMLzVwaW1iWHo1Q1Q3Y0d6WmNESnk1N1N2R2YxS0g3b3VVQVEyWVZvRnZKRTc5NXpDNVB2aFpNUkpGMEFXTWd6R1Z3UmpoRUE0aGxjeWQvTlRSa2ZWQmVSYmFSdUJMc3d4MVltK2RvV0drOUUwczJCOHg1TTloRUk0aG5leUMxVk9PK2k1aFlZRVpIM29NZFNKY0t5TVBBSDdJNk5mYVNZTHA0RWdDTWNJUXU3b2N4bzA3eGJEUFVoZk5YU2djcXdUNzRzUzdvWitPQm01cDNudk0vcTEzQ0prZHZCRVVJUmpoSFJzZEF2bFRmYWxna3hBVm9EVjZUcnh2dWpRbUNEMExyTmY3WVRQR0VJakhDT1lUTGVZZHBNNC9wYUZMVWd2NTIxd0xlTDlVRUJ1NEI4eTNBSHZRKzRqS3FFRDRSaEJ5UjEranNQWnY4b29NYVJGZjZzdVZQUVNreTM5YitWSlYwN09tVXlCV0FqSENFNEM4b2NNai9SbnQ5Q0ZoWHBKVVVYU2hYQ2NrRXlrK0RPaldmTzFxeFNiTXFGY2hHTkVJWGY4T1kxNHE3MlhQdVJjdG1HMWhuQ3NDKzlISWhsT3BLZ3hzZzNSc1VNZW9wSVRlRTRycDJ2UGUvdXpVQ1MrNFdUMExjTkttVVdMcDR0N25xSkVsdUhXL1Uwek9hOHltUUpSVVRsR1ZGSUJ5R2tHY3ExZXFNZWp2L2lvVnVwd1hmb0JpRTBXM2owU2pBRy9DTWVJVGdMeVhhWkgvZy82a0tNamxPbkFUVXBFTWpIbjcweWZtakRMR0VrUmpwSEtTY1pqbitoRGpvdHdyQVB2UXdTeXNjZGxabHRCTjlHaWh1UUl4MGhDS2dMSEdRZmtla2M5dGpnTlRENUxOMW4va3ZyZFVPVUxUOW9vYmpOZHQxRVJqS0VGNFJqSkZCQ1EzZVBPUDRlVDBabUMxNUk3cXBacE1YODJzTWI4NGh6N2kydGpnakUwWUZvRmtwUDJnNGZNSnc3TVpOdFROcTBJaEtrVnljeWZMdTVwSVFwRTFpKzREWWMrWnZrTC92Q0JUVDZnQlpWakpDZUI4VGl6YmFaWDBXWVJIaFg2TkFnMGdUVGFLQWpHUUVTRVk2alEyR1k2NTRCY3Qxa3d6U0lNTHE1cGNGTVNnSXlGekwyTm9pSVlReVBhS3FCS28xS1MrK1B4dWJSWjBGL25VY2FiekdoMXhlNWxmc21Ocy9zY3Y4dnA5OXFBWUF5VnFCeERsVUlxeU02K2JCcHlxdUMxNUlUakdSZkgyeU5wdTNva0dBTnBVVG1HU2dWVmtDdFpyTWNxYlUrb0hrZHovblJ4ejQ2UUhoUzA2SzVHTUlacVZJNmhVa0VWNUVwNkN0bDYycDlwSVorYmxCWlVqZjBZVGtiSE1xMkhZQXdvUWVVWXFoVldRYTVrVyswcFZlUitwRjNscytYZlFibFBUeGYzTE1Ucm9jQnFjVVV3aGhXRVk2aFhZRUIydmp4ZDNGT1o2MkU0R1QwVXNOSS9oZG5UeGYxUmViKzJQMUl0dnBTMUI2VWdHTU1NMmlxZ1hxUEZJdGVkOU5iNTdNS2QzQmlnRzZZb2hNRng3Y2hWaTRlVGtkdk44UytDTWFBWGxXT1lJWThoUzVqN3VlcmNQWDZWN2JiUkF1MFYzdkZFbzZQaFpEU1dtZEFsUFFGYnlNaktXd1d2QmRnWjRSaW1GQnlRNTlLTGZLM2d0WmhDZTRVM2QwOFg5OGVaL0M3UnlOTWZGNHJmRnZJcjExd3dQbWI5QkN3aUhNT2NnZ055SlF2MnhyTGxObll3bkl3T1pCcEFTUlU3MzF6UU9lRHB4ZTdrUERVdDlNa0Z3UmltRVk1aFZ1SHpiTCs0YWhSaFpUZXlBT292QzY5VnFWOEpPcnVURm9yVHd2cUthOHh0aDNtRVk1aFdlRUNlU3k4eUMxMTJJSUhscS9vWHFnK0xxWFpVY0F0RmJTWVZZMjdhWVJyaEdPYko1aGwvRlB4TzNrbEladEhMRnNQSjZLeXd1Yko5c1FCdkI5SkNjVmI0em94M3N2aU9ZQXp6Q01mSUFsWEJaMWNTa3VsSGZnWGJTKy9zNnVuaW5yRnRyMmowRlU4TDcybm5zNEtzRUk2UkRla3J2V2JoRmYzSTJ4Q1F0eUxzYkZGNFgzRVRUeGVRSGNJeHNpSTlmOWRjc0o1WGk1OFJramNqSUc5RU1IN0ZjREk2a2U5VzZlZVlpbjUwNUlwd2pPd1VQdXB0MVVMbUkzTUJXNE4rOVo5OGVycTRQMVAybWxTUUoxT25CUysyYTJKekQyU05jSXdzc1VEbUoweTIyS0RRbmN0V2NSTzFBYUg0SjR4cVEvWUl4OGdhMndmL2hKQzhSdUh0T0hPcEFoSjJHZ2pGYXpHUkFrVWdIQ043VkFiWElpU3ZrS2NON25pOFUvWEN3cnFSS2lCaFJ4Q0tONklYSGNVZ0hLTUlMTlRiaUlWN0s2UVArVFR6bXluYUtGYklUZlNZVUx3V0MrOVFGTUl4aWlHVndXc3VmbXZWSWZtU09jblBuNVVET1I0NVZwR3BGamN3a3UxVkM5bnhqcFliRklWd2pPS3dTOXBXVjFKSkx2NkNLSS9ZTHpNSlRuTUp4Y1ZQR0dEempwMndGVFNLUlRoR2tlaEQzc21kaE9SckE2ODFLT1BWUmZyTGhiUlhUWmxpczlYNTA4WDlWUGxyQklJaEhLTlljcUc4WkI3eVZ2Tkd5MFhSVlNSaklabFFMT2duM2htOTZDaGVSVGhHNlFxZFVOREhsWVRrb2gvTnl5NXBZNldmbXh0NWo0cXUrRXZmK0pqV2laMHh2eGdRaEdPQW5kSzZvSnI4STREVlFUbmxFNGlaM09SZGw3NmdVdm1OaTFaWFVqR212eGpGcXdqSHdBK01lK3ZzU2tJWmxjcXFPbTc4aFB3Y3pXV0w5T2NmQXZIemQ3ZHVuYUJLdkR2YUtJQTFDTWRBQTIwV3Zjemw1dUtTUjdQZncvSlI0K2ROeDU1WHR6RFNWZlFlNnAvU3czQ2xxMnB2RlcwVXdBYUVZMkFOcGxuMFZyZGRGUCtZdjBrQzNUOHQvcFgvOHFqN0I3bDVQWkVmYm1DN081ZkZtbnkyZ0RVSXg4QUdFbVN1cVVyMVJqOXN3M0F5MnZtayszUnhQNGp4bWpRakVIdTFrR3B4OGVNWmdkY1Fqb0V0aHBPUkc5MzFtZVBrUlIyVWIwdDluRXM0M281QUhJUnJ6em1oV2d4c1J6Z0dkcERaVG1sYTFEM0sxeVdOaGlNY3J5ZUw2bzdwSWZadUlTMFVaNW45WGtBd2hHTmdSMUxOT21YcjZTQVdNbm5oT3ZmcEM0VGpmOG4zcVo3dWNjS05aeEFzdWdNNklCd0RMVkZGam1LMk1xb3NtMGZCSllkanFRNmZTQ0JtdDdxd3ZqeGQzSi9tL0FzQ29SQ09nUTZvSWtlWFRWZ3VLUnczV2lYcUg2YS9oRWUxR09pSmNBejBRQlU1bVRvc1AxaHJ3OGcxSE1zTll6TU1VeG1PajJveDRBSGhHT2lKS3JJS2kyWllsbzB5VkZhWGN3bkhjbU40MUFqRTNDQ21jeWM3M1ZFdEJqd2dIQU9lU0ZnNFk2VzlHbTRheG1Nak5EOXFDQS9Xd3JITSs2NjN4ajZTZitZenJnT1RLSUFBQ01lQVo4eEZWbThtb2ZsQi92TXg1aWc1cmVGWStvUHJDUkoxSUtZMVFxOGJxUllYdjdFTzRCdmhHQWhBcW0yWGhBdHozT1BwYnhLY0s2azZWejdEYzZwdzNBaS9CMnQrYUltd1l5NmhtRjN1Z0VBSXgwQkF3OG5vUkZvdENCOTVxRnMxS2duUWRWL3pZK04vZDc1dGF1SHdFWTRiclE2MWVqRmM3Ymp4djlNQ2tZOHY3bnpDTG5kQVdJUmpJREJac0RlbDFRSkFSM2N5bm8wV0NpQUN3akVRQ2EwV0FGcWloUUpJZ0hBTVJNWnNaQUJiTEtSOWdwbkZRQUtFWXlBUm1Xb3haZGN3QUExWFVpMm1yeGhJaEhBTUpNUUdJZ0FFRzNrQVNoQ09BUVdrSDltRjVQZThIMEJSNXJMWUx0cXNiUUN2SXh3RGlrZy84aW1MOW9Ec3pXVjN1MHZlYWtBWHdqR2dFQ0VaeUJaYlBnUEtFWTRCeFlhVDBWaENNcE10QU5zV3NpRVFtM2dBeWhHT0FRTUl5WUJaaEdMQUdNSXhZQWdoR1RDRFVBd1lSVGdHREpLUTdHWWtIL0wrQWFvUWlnSGpDTWVBWVN6Y0E5U1l5M2Z4bWxBTTJFWTRCaklnSWRsVmt0L3hmZ0pSTVpJTnlBemhHTWdJbTRrQTBkeEpLR2J6RGlBemhHTWdRN0l0dGFza2oxbThCM2gxSmFINGtjTUs1SWx3REdST0Z1K042VXNHT25PdEU1Y3NzZ1BLUURnR0NqR2NqSTZrbWt6TEJiQWIxenB4U1Q4eFVCYkNNVkFZYWJtb1I4SFJjZ0c4dEdoVWlXbWRBQXBFT0FZS0psTXV4bFNUQWFyRUFQNUZPQVpRVjVOUDJGZ0VoWEc5eE5kVWlRRTBFWTRCdkNEajRLWVNsbW03UUk2dVpMT09hOTVkQUtzSXh3QTJHazVHSnhLUzNjOGVSd3FHM1VpVm1CM3NBTHlLY0F4Z0p4S1V4K3pDQjBObXNyanVra0FNWUZlRVl3Q3ROUHFUVHdqS1VLZ094TmYwRVFQb2duQU1vQmRhTDZCQTNUSnhTeUFHMEJmaEdJQTNNaHJ1aE1WOENHeFJoMkY2aUFINFJqZ0dFSVJNdmFpRE1sdFhvNjlaSXd6ZmNqUUJoRUk0QmhDRnRGOGN5dyt6bExGTnN6cE11d1NBYUFqSEFLS1Rxdkp4NDRjV0RDenFJQ3hoK0tINEl3SWdDY0l4Z09RSXkwVWlEQU5RaVhBTVFCMFpGK2RDOHBIOEp6M0w5cm1lNFFmYUpBQm9SemdHWU1Kd01qcVNzRnovRUpqMW1qZUM4QU1MNkFCWVFqZ0dZRllqTUI4MEtzM01XbzdycnFxcVJ3bkRCR0VBNWhHT0FXUkZXakthb2JuK1QvcVl1MXRJK0gyVUh4ZUFIMm1OQUpBandqR0FZc2dtSlpWVW1lc1EvWWJSY3MvbWpmRGIvSGxna3cwQUpTRWNBOERMaW5NbDRibVNpdk9CL0xQbGxvMDcrYzl2VWdHdTVEKy9VUUVHZ0pjSXh3RFFrb3llTzJqOFc4ZHIvb1IxLzlzbUIxS3AzZFhqbXY5L0hYYWYwZnNMQU4wUWpnRUFBQUR4SHc0RUFBQUE4Qy9DTVFBQUFDQUl4d0FBQUlBZ0hBTUFBQUNDY0F3QUFBQUl3akVBQUFBZ0NNY0FBQUNBSUJ3REFBQUFnbkFNQUFBQUNNSXhBQUFBSUFqSEFBQUFnQ0FjQXdBQUFJSndEQUFBQUFqQ01RQUFBQ0FJeHdBQUFJQWdIQU1BQUFDQ2NBd0FBQUFJd2pFQUFBQWdDTWNBQUFDQUlCd0RBQUFBZ25BTUFBQUFDTUl4QUFBQUlBakhBQUFBZ0NBY0F3QUFBSUp3REFBQUFBakNNUUFBQUNBSXh3QUFBSUFnSEFNQUFBQ0NjQXdBQUFBSXdqRUFBQUFnQ01jQUFBQ0FJQndEQUFBQWduQU1BQUFBQ01JeEFBQUFJQWpIQUFBQWdDQWNBd0FBQUlKd0RBQUFBQWpDTVFBQUFDQUl4d0FBQUlBZ0hBTUFBQUNDY0F3QUFBQUl3akVBQUFBZ0NNY0FBQUNBSUJ3REFBQUFnbkFNQUFBQUNNSXhBQUFBSUFqSEFBQUFnQ0FjQXdBQUFJSndEQUFBQUFqQ01RQUFBQ0FJeHdBQUFJQWdIQU1BQUFDQ2NBd0FBQUFJd2pFQUFBQWcvbzhEQVpSdE1CZ2M3M0lBbHN2bGJlbkhDdEJtMSs5dlZWVVB5K1h5RzI4Z3NOMWd1Vnh5bUlEQ0RBYURnNnFxenFxcWV0ZnlONTlYVlhYdC90M2xjdm5JNXdhSWJ6QVl2Skh2Ny91V2Y3bjcvdDdLOS9lQnR3NVlqM0FNRkdZd0dCekpCWEt2NTI5K1hsWFZLZFVvSUI2NXNYM3c4UDI5cWFwcXpQY1grQms5eDVseUo5REJZSEE1R0F5K0RRYURaWXVmYi9MdnZlbDZaTnhqdnNGZ2NOM3k3M1UvajRQQjRMVDA5eTZDYXc4WFZ1ZWpDOWw5UGlzQVdydjA5UDExVDQwZTVXWVpRTVBPbFdPNVczVS91L1kzbGM3ZDJkK211Q3YzVkJsY1ZGVjExUGJSK1dBd0dGZFY5YlhIMyt2TWxzc2xKK3dBUEwwL3EyYnV2RUFGQ2doTHp1MS9lLzVMM0xuK2dPOHY4TU9yQy9Ja0VFK3JxanFwcW1xZjQ5YmVZREE0WHk2WDA4aC9yWS9LNEo3MHRKM3MraS9JNThWSDhEb2NEQWJUNVhKNTV1SFB3a3NoYm00UFhYdUZuQ3NBaExQeitiaUZQYmxtVVBnQ3hNYTJDbm04L1k4OE9pVVlkL2ZSdFNuRStzdWtNdWpyL1hyWDhwRzV6M0EwOXZobjRZZURRTWZpbzl3Y0FRZ25WSUI5MjJMcUJaQzluOEt4QzBPRHdjQzFCSHptN2ZmbXZZVFdHSHlmNE5xME4vajh1dzg5L2xuNElXUzdDcFZqd0M0S0VvQjRFWTZsU25oTE1Ba2l4T093ZFZKVzcvamM2T2RqSWM4bTlJa0RZYjBOK0tkVE9RYkVhdVg0bW9BVFRLd1YvYjdEOFU2ek1BTThVcDk1L3ZPS0YyRlZlc2dMTjRDd2FKOEV4UGR3N0JaQWNYSExndGNUWElzVnpMN0RNU3VuL1dQa0dtQVVQY0ZBUE0vaFdOb3BtQzhiVnZEZHhCSlhiNU5Vck5FS2JROEFBR3hSVjQ1UEF2Y2lJa0k0RGhCUTI3em1sSDgzZGhPNmNyemdmUUNDb1hJTVJGS0hZMWFwaG1jeEhMZXAzdnF1U2xJNTlpOTA1WmozRExCcnpuc0gvS3NPeC9RYWg1ZDc1ZGgzVlpMS3NYK2hLOGYwaVFQaGhLNGNjODRGeEg5bzhvOG05M0RzdFNyWmR0dHE3SVRLTVlCTk9PY0M0aitzWUk5aUVTbnNwUXpIUG52V2Vid1hSdWgxQlZTT2dYQkNQK0VsSEFQaVA2eGdqK0kyMHQvajlVWm4xMEFmWUg0dUoyblBJc3c0cnFnY0E2WngzZ1VFbGVNNHJpUDlQVDQzY0dsVHZmWDlHYUlDNlYrTTd6bnZHeEJBcEp0YndqRWdxQnlIdDRnWWpuMUsxbTlNQlRLSTROL3o1WExKK3dhRUVlUG1sbkFNQ0NySDRaMjEyR1d1c3dBTEs5dThaaXJIK2pIakdMQXJ4czB0NFJnUS8vSDhLQjR2dWRhRXMwakh4SGY0WWNaeFhwaFVBZGdWK3VhV1JkQkF3Mzg0R01HNFN0cEpqS3F4U05rZXc5TUgvWmh4RE5nVit2eE8xUmhvY09INGpnUGluUXZHeDhaN01KTzk5dVZ5R1d1NlIwbW9IQU4yaGI2NUpSd0REUzRjWDNKQXZKb2xDc2ErVDI1dEFxclB2NXVidFRCQ3p6am00Z3FFUStVWWlPZy95K1hTaGVNckRucHZybHI4WmJsY0hpV3FHRjk3WEJSMTNySWR4T2NOMXFuSFB3dU1nUUp5d00wdEVORnp6L0Z5dVJ4WFZmV2hxcW9iRHY3T0ZsTGxQSytxNnZmbGN2bG11VndtQzNZU1pzY2VBdktzYlVDVk5vaFBQZi9lU200dWFLbndqekZRZ0ZIYzNBTHhEWmJMWmJMRFBoZ01YSmo3R3ZpditXL0VSWEhKeVlsMDZoWUR0cXcyekdUc1hPY3E4R0F3T0pHQS9xN2x2M29qZnpmQk9JREJZT0ErRDMrRS9EdVd5K1VnMWU4SDVFekdkUDRWK0ZmOGhWRnV3QS8vbC9oWUhJVCtDMG9LeHRXUGpSakdpZjd1YTZNYm51U09NVkNBWGN3NEJpSkxQY290ZERpZUJmN3pBUXRDZjgrNHNBTGhjSE1MUkpaN09HYjJLa0E0Qml4alVnVVFXZTV0RldabXJ3NEdnemVCVDRJUHNWcE1jdnBkTWtFNEJ1eGl4akVRV2Vwd3ZCLzR6N2NVb055aWl6OEQvdm0vdFp4ZDNFZE92MHNPUW4vUHVMZ0M0VkE1QmlKTDFsWWgxY1hRTE8zYWxkTU9adXpHcHNSZ01BaSs2SldMS3hBVU00NkJ5RkpXam1QTWJyUlVPUTU2c3hDNURTR24zeVc0Umh2SzBZN0h6bFhOSDNkY1lVNDRWa3pHZEZYeVB2bDhyN1k5V2RuMTgyTkM0enZVYk9uYXBiM3JvWEdkcVA5WnpiRmh4ckV0UFQ2SGo0MzNvZjduckw2anNjZzVkZGZXenZvNC8zUytUQm1PcVJ5L0ZQSWtHSHRxaDluZlJiNVliYjVjdFdtYm5SRmxKdlNKL0YxdDJ4NCt5NTl4dDhQZkcyTmNZdlFUdUZURWorWDMyL1dtWXBYclhaK0dmYVgva3BEVC9Ia2IrSy84dk1OcmF2N1hldHYyQi9tNTFub1RLdS85a2J6LzlUSHRXbDFkK3o3SXNibVRpK2V0ZkZaU1hFK3kzTUJIUXVSeHk2TEFxbS9MNWZJazdDdmRiT1YzT0E3NE9adzF2cGUzaVQ2SHJmVTRSOSsyMlZCTnpxMzF0YlRUZVhVd0dMak4wMDZYeStYWjkvL1JiUUtTNGtkMllWdUcvRW4xdTNYNWtROStxT054Ry9sM3ViWDB1OGlYOWt5cVJsMWYxL0VPZjgrQmg3OW45Y2Y5V1Vldi9KMmh2MmVQa1Q5Ylk0L2ZsYlBBci9WRXRsWi9ESDJ1Qy9EelRTNFdXczZQUi9MZENYbWUzUHBabC9kejYzZmQ0Kzg5RGYxN0pmaisrcm8rWENmNkhKNG0vaHgrazgvaGlaYnY1NXIzdU0veDJla2E3L2xhVVA5Y2Z2L3pFeDdBczhBZm9BZU5INXhYamtmSVl4RTBCRmorWGVUaTR5T3NqbC81Tzk3SXlTelVNWEVYN1RjYi91NlFmKzh5MW8yWFZBVjhoMHp2RnhkNXIwK05CdUoxUDVlK2oxR0xZM21nK0ZnK3ltdGIrNzN6ZUF5eXVMbVZVT203YURLTjlOcmZ5SFVpWlNEZTlQTk5zdFJCcXU5cGdQZjQxV3VLaE9LUTU0VG42MExLQXhteXVyajFBR3Y2a1MrNTdEM0dBQUFmQjBsRVFWUmZ5R01SdFFKazVYZnhIQnpYdmk2UDRYdmJ6OW9MUllUdldmRHdGREFnZUx1Z05FSnhqUGM2OXMvR0c3OUE3L2RSaEpzNlh6L2ZRb1prMlhFMDVPc1BmcDJVTUJQaXRXOThZdWJwZFI4WStod3U1YlVtQ2NueW5mVjE3bHQ3d3hib0JtdmozNTl5RXhCbU4vNlF6WFNIeGdLalVMejhMb1BCd0oxSTNvZDZrYTRQYWpBWXVDL3lIeEZXbTFmeUNIOGQwek9PNVgzYTJqL2J3ZHhYci9SZ01KaktjZmdjNmIyT2JlZit2ejVjaitKZ01IQmg4TytRMzAzUDl1UjlmNVIxQkw2WnZrN0s5L2RyaUQ4N1ZPK3Q2eVdXMS8yUG9jOWhKYS8xWVRBWVJQbSsxcVRuOTlianVlK25OVGp5Ty8wZFlhM0c4OS92ZnFlVTRmZ3c4Sjl2S1J5SFBnSG1OTjJoOSs4aVljYjNTZTk3Q0pVL1A5WVhlUnV6TTQ0RDM4RDBucE10RjlHWU4wQ3BCUDBNeVhFOGxURHlUdldSMk15OS8zKzZjTzk1VEtuWkdjZnlub2I2L3Q3dDhQOXByWEdqYXlrVU56M2ZyQTBHZzRjWVl6emxzKzR6R0svKythN0k5QkNvUVBLYU4wbW1WVVNhY1V6bCtJZVlxMXRWVjQ3bHN4Zml6dnBBL3V4ckphSFk5SXpqUURjd1RiM0NjWUJxU1pIa09GNUh1SW1MeFlYN1cxZEY5dlJrd3VTTVk2bWlod3cwWGplQmtuUGxwWlp6dHdlSFVrVStEanpkNGl6RVo3UXgzU2ZJbjcrTFZKVmpaamUreEZ6Z0hYbjRYVTREZmRrT0pMaHJPcm1hRE1keW9RcjlhTER6eFpWZzdFZmpDVXN1d2JoV0I1TmUxem1yTTQ2bFNIRHArODlkY2UzckQ1SWdyKzNjN2NPZTNLZ0YrUnpKZVRwVUFhTnV4MGwxanYyV0toeFRPWDRwcHhuSElTdkhQbjZYc1ljL1k1Mzl4QmY1ZGRVQnF6T09Md09mRkdkZFgzZWh3ZGo3STJ4cG1mbkQ5NStyaVB0ODlHMnhzSHFkREYzdFcvaXFoa3JyeDU4WmY1OURCdVNRTStKRHQ5Mit5bjIrc3EwY0c5dFpKdVJKa0g1aklSV0NYRStDNjQ1TjZIQTg5LzBIeW9MTzBCV2NUbFhqMFAxMWluazdsMHAvOGJYaG5zNDI5bnMrL2pkM2N4dTRtbGp6VWpVT3VOaFhHeDgzYXVzazI0QWxobHdyeDk0djJvR0Z2RXVLZlpNUU10ajByUmFFN29kT0tVVTREdkhaaXJGalhkZEh2dGVGdGxKNDZlOXMzRnhZWFhUWHhXR1A2UUhtYm00alRUYnhzWmcyNkxRaWhmWjl0cnJJVFZCdTdWQzE1eWRsdVZhT3pWU05JeXhPekdsdjlyNVY4Qmc5Zktta2FLc0lVWFVLSFp3NlBaSWREQWJqREhzU2QrV3J2L01zOWVQU1JENTNmS3h0N2Z2N0psSTFzZGZuc2NCZ1hIdm5jZHhnalBVc1NlVmFPYWJmK0lkb3g4TEFqT1BzdjlBcnJGV09RL1dETjdXK3NBYWNjR0xCbFk4RnZRVUhrdHBaaDM4bjlQZlg5eFNER0cxck4zMCtqM0tUeStld3Y1eWZ3ajUvTDFLRlkyWWMvOENOd3U3NlhxUnpmUXprZWdmWFBXbzBNK05ZQXFqV2xvcHB6cCtkVnl4ODNCUVFTSjY5N1ZBOENCMk9mYTlIaVhFRDJibHFMTWMveUlZa2h1ekw5eEdiUFg4dlVtNENFaEtWNHg5aUhndTE4NW9qemZ4VncrQ000Mm1FcXROOHcwM0VOakZDdTBiVHZndTJwSjNBVjdYS3VyYWZvOUEzWk40cXh4SzRRci9lUmRkdzNKaEJEejgzTVRsWGpwL1BlZEhEY1lSSDd4V1Y0eDhpVCszUVBLODU1M0M4YnNTZG1YQXNRVjVsMVZndStpVXV3anRmTHBjK0Z2Q0VIc3RueWJ0ZGIxb2ozZHg2cVJ4TDhJeHhBM1RkNHhyQTUvQ0gvVWd6dEsxS0U0NGpvWEw4cjloVE96VFBhODQ1SEtlWVZPSHp4aXZXTGtoZHdsNkpqeUEvTFpmTDNqY3JNcVdoeEFWNHI5bDFRVlNNODVXdnluR29qWlZXZGJwWmswVm9KVTFJMlVYZjgxck9pNU9UdFZVRXJ4d2JtM0VjOGlRWSt6aG9udGVjY3poZTl6NmJHQU1WOGNKMTAvYThJTlc3a2laVXVCdlFYNWZMWmU4cVlNU25BZGJzZXYyTGNYUHJZNkdsSzRoODlQT0tYdFcxSmFxaXJXZXRyR2NVOTFGUE04cXhjbXh0eG5ISVBxM1k0VGhrNWJqdmJFdkNjZmkvczVWSTI4eld1bHdnUzdpQXVEN09tNnFxZmw4dWwwZStkaDZMV0UyMFp0Y2J3ZERmMzk2N2pVYnU0KzNVSnl0UEwwcGNUTHZOZnRmV25VaXRzY245WDRJWEVQckFXcHB4YkhHVGh0ZG92aGpTVnVHWGo4OVdyTjNtdWxhZFlsMEU3bnh0dExHakIvbk1mUE1ZaHIrTHRFdmFhK2J5T3o0MGZ0ZFZSL0lkT1lyOWRNQ0ZpeDAranhZbVZWeEdDcDZMTHYzdkVTZmdiTEtRNy9XRG5DL1huVE9QNUlscmpKMUJWeDFITEU1WThYMnIvQlRoT0RSYUtuNklPZU00ZElNL2xlUE4xbFZ2Vkk4SWxMbTNzZnBSdTY3T0R0bnVzWkJxOXBtUHg5dktwSmdKWFIvUHl4M2JaNzZmVHhxYlY4U3FNdTV5TGxJOTQzZ3dHSnhGN09QdDJoWVJZd0xPcW5xaXh0bU9ONTR2cm11eUFEaldoa05kUDJQWnp6aXVFclZWaEg3VENjYy9aRE9wd29OY0g2MXQ2cVZWTzBzODhvWVE4NDVWcDlBM2V5Zkw1ZkkwdDJDY3FHcjh4WjFMNVhpMi9seTY5OEI5UnBiTHBYdnQ1MkZlNGdzYXduSGZqVFJpOUJsWGpadWVMbUl2cGoyWHorRzQ2eE1aK1J5NjhQbkovOHY3U1JIdEVTMTkvNnhST1U0cnAzQWNORXowV0l5Ujg0emp4Ym9MUUlRdHlhdXVuNjBFTzZWMXJXS0cvRHhmOWZrOEt4Y3prTGpQLzdIUDFoQTNwV013R0ZTQnc5OHVvVVRsakdNSnhqRTMwdWgwQXhscDduTHQrVHk4WEM2OTlWKzdSYkdEd2VCYjRHUGQ5VHFSYTZqKzBMeTVqbG81WnNieFQ0S0d0cHhtSFBlVVl6aWVTVERZMUU4Wld0dkpEMjhTQk9PN0hyTjZRMzVtY2w0OUh5c2N6NlJLNTcxbldzYllKVnZZclhYR3NYeC9Zd2JqZVkvSktiRStoL1VObXZlRmlYTHV1dHZoLzlvVll4Yi90WkJnL09KYWtXUGwyTkpqeXBBbndVWEFQM3Vka0lHczd3bENXemllUzdpc3E0ZWJGZzNWVm04cUg3YWNqSVBmcUxTNThaS0wvWFdDazNHZjN0ZFFOL0x6RUlGT0F4bkxGMldCbGxUcVFwN3JYU2o3STlDZnZlMzdxV3JHY1dNcVJld0ZZNTBXMDBVZXdYZ1MrUHQ4cG5DY3BMYlhjOWRZOExocDRXUHRZT1g3OVUzV0tmeDBMb2tkam1QTU9MWjA0UWw1RW94OUhLZ2N2KzVHTGpDM0hTcjZiUi9CaDY0YzcxeFZHd3dHMDBSanZjNTd0aTZFK2p4YmVyTFZWcXpSZDUxN09sc0krZWR2dTBsVU0rTlkyaE5pYmRMVGROZWpHaHZyYy9nbFFudFVydTFYZmN6bFdub2Q4dmpuVmptT1hTM3RLMlNWSlhZRlhmT000MVRCZlNZWGxqN2JubmFSZkZLRnRGQ2RKWHAwdC9Bd01TSFU2eVljOTlNbk5PM01YWFNsOXppRjVET081ZnQ3bXFoS3VIWXRSUXN4V2lyY0U2RGdVMW5jZFdNd0dDeTBqRWxOT09PNDdSU1EzbktySEp1cEdrZm9LNHQ5TERUUE9JNjlqL3lWUEtwSmRkY2YrdmZkR1BBaWp5TGFwTmNqOThDVEtySU14OUpTRWVNY1VNSjIzc2ttVmNqN09FMzgvZTAwZGFUNmNWMk5jVU1lYzF6aFEyRTdkVGJONVZqSExqQmxWem1tMy9pSGFNY2lRdEMzVWpsMnZVOVRCYTA5VVN2SEVpYkhVamxNUFRMdnlrTmxVZk0yNkZyRnFDamRSVjVrbkVyVXdvbWN2K3ViMnRUZjM3dWUyNWZIK0J4MkdnK1ppVmlWNDRWY1M1TWQ1OWpoT1BUZEQvM0dQOFE4RnRxblFZU3VKSGdmNWROVDhNcUpWSWlQNVVmTERPbVpweDJ4UWw0QXNseU1GK21pbWN1VWoyMExqRU9mVDk5SWhmaFl5UTF0YmVHaE5TZEdhMC9zejJIT0cxaXRjeVhCT0draGdjcHhPdGxVamlPTXBPc3o0emhHMVhpcUpSaEgrbjAvUi9nNzJvb3h3UUJyeUdjdStBMW96TzlZaEUxZ1hoTTZyTDVQdkwzM0ppY2V2cjh4YnRKaW4rczFiV0FWK3ZqTzNDWXFnZitPblVTYmN4eXBrWnZLc1lqOGFGL3puYTI2bWIrQnBieW9weFJqZ29FUDJuZVM3Q0xIUUpMa2U1VHhoa1hiZk9pN1JrTnVhRUwzdmM5aXR2WUV6azFiRjJZbW9HWTZSNHJ0bzBPaWN2eXYyRk03UXY0dWZXY2N4NWo1cTJuY1RvN2hhNXNQaWxwYXRvazFaaXFtR0VFeTl2c2JzbnIxMmsxY2llSDR5bE52YVl6UFlld2UySkNmd3k1NUtYUnJySm9NRjdPdEl2Z0hseG5IMzhVK0RxVlhqalVwN2ZmMWRXR041YjFVZzI3bGljTTN6OS9YYnduT2d6RXF4OUZ1UU9YOUNSa0NYZ3NBeGZXWGVueU1IdVBjRi9OemVCQzQvVVZqWGxMem1tS0c0OUFWTFdZYy94RDc3aXZrQ1YzN3BJcVEyM3QyVVZMbDJPZUZOYWI5a0JjOW1jKzdrQXZON1E0N0t2WVZPcFRNWXZXU1MvOTA2SnV0MTg1cEpZVmozOS9mMEovRFJhd2J6OGF1aENHMWFnK0oxQnBMNVRnQVpoei9FUHRZYUZvd3NJcktjWjYreEJqQ2I5aWVWRCtmSzZBU21HOWsvcmEzaTY1Y3hFUDNlVWFwMWpXMk9ROTlQbnZ0L0Z6S3pXMklHOXNzTW9aOHAyNGpMSExWdVB1ZW1od1hzK2M0OUplZWZ1TUVtSEdzN2dSVHdzWDFBOEc0azNkVlZmMDVHQXdlWlJTZkR6RnV4b0plTUYwWUdRd0dwL0wzaEE0azI2cmdKZHpjZnZJZGpITzVTWlB2NVdPazZTOXR2MWZCSzhlYXBnMVJPVTVEZTZCc28vUVp4OXJrL1B1NlZvRmpZMnNMTkhLVjBhOFNDTWM5RjVURytQNEhtUTdRMkx4bUhIR0h6MjFWKzV4dmJrUE9nemM3bFVnK2h5ZVJOMkhSdUlCNXJ1QTFmQmN6SEljKytWQTVUcVAwR2NkcUtzZVJmdDlVWmpJSE5jWVlwUkoyWWF2a1F2elhZREE0WHk2WFhUZFBpWEl1Yy8yT1BjOEZ4eEk4aitUbk9OR1c5OXRDU2E0M3Q3UEE0eFpqZkE3ZnlPZm9vV3VGVTRMd0cvbjgxWi9GRkcySlhjSng2TXF4cXZOdWxIQWNhYUE2bFdNUmViUVlreXIweVBYMzdSUGV1aWdsSE5jK3lrWC91TU5GUDhiMy82L3FSOTkwMVZoc3VNMkJzdlVRczlmQ1ljWTN0ekYyUEl2eE9meWovb2VWUmEvYmFQc2NSdDFRcDRYeXduR2tSMFZVanRNSStkNHk0N2lkM0M2dWN3K1AvVnR6ZjE4amlKWENWU3h2cFVMYjVseWE0bHkyRjJIZWFnamJ0aDNPN2VZMjVyYjZLYzU5dVg0T053bjl1Nm9LeDdFVzVESGorQ1hObTJhMHBmbUVUdVhZcnJxL09OWE54MDBPQjdHbHd3NXRRcVh1Nk5iV2ZJZDUzTG5kM0I1RnJGQ1d1ak5vVzRzZTRUaTBJc014TTQ1ZjBqejZySzJRN3kwemp0dko2ZUs2RjNoM3FHMnM3TGpuMitGZ01HaHo4Y3pwWEJiU0xwL2wzQUplekZZbzdPYXNTM3RMcEJuSFZJNERzRFRqT1BTeGlGMXAwN3lBaE1xeGJkT0VmWmpYQm0rNmZma1k2V0pZaXBzZG40RGtWam4rR0dIVVo4MWllME5zTStVak1La2NCMkNwM3ppYkUyQ0U0TUtNNDNaeXU3anVwYW8rU1hWRjYrUEhHTFplUkNNdHRMWnUzdUlKU0k3SGszbmtPaXg2UG9tTE1lT1l5bkVBOUJ2L0VET3dhVCtaTStQWXZtbkU2dE1MVW1XWlpYaE1kL0YyaC9CYjBsYmxYU3hrL09DdXhac2NqK2Y3MERkUm1ZK3c5Q1hrR0QwZlZNMDRyaUtHNHhUekpMVmlVc1dPbUhHOHU0d3ZFSHVKcTAvamd0c3I2Qm50cnN1R05ibmV6SWQrQXNNVGpOZDk4TEF3TXZReFZqYytNM2c0anZUb1RlTWU0WnVvRFpRZE1LbENqNXgvMy9jSnE4Y1BpUmNHcG5TeTVlK21MM205MXNFNDgrcm5XM3JZay9td3c1U1VYWVQrZkpZWGpubjA5cE9jUW96bWtYU2x6VGpPWGJMcXNWUmRmaSt3Z3J4SFgzRnJNeGxoMXZZUmR1N0htZDdqdUZ5YndxK2VnbkZGNVRpTTRCVWZRc3Azc1VlTHNUdWVIakVyTXd2NXJKMVhWZlZKZ21ObzcxTldueVFnSHhmWWcwdzQzdDJYNVhKNTFIRmhVY3dpVXYzOXZaTHY3MjhSYnZ4YzlYamJrNGl1cUVxL2RON3hCdTAxb1Z0ajFZWGpHRHZrTVNUK3BaQVhtOWhUTzBLT3oySEdzUzZ6MXhaMURBWURkNkY5SC9nVm42YThFTHJmWFFLNjY4WDluT3AxUlBiYStadHorNy9jWi8rMDUycjdHRGNodjc3eS9UMkw4SmsrSzNoK2VBeDM4am4wV2l5TTlQU0l5bkVBMWtKS3lEdXdhS3RSRFR4dXBYTHMxN2N0bFlocHBPcFQwaXFSbXp3Z1V5eCtrVkNVZTZ2RmF6ZVpwWWRqOS83L3Nsd3V4eDdHVUlXK21aOXQrZjZlUmZnczd3OEdnMUw3OTBOeXUzbit0bHd1USswb0d1T3BCdUVZMmRDK21VbHBNNDVEZS9YR0srSmNZQld6aDEwWWNxRkl6bThmTXQ1dW1yYUtsMmJTaXZCZlQ2RzRGdm80di9wVVViNi9NYWFUMEh2c2grc3AvaUkzWnllQlcwdURud08welRpdU1nbkhaa0pLaEdwcnpMWUs3WDFlcGMwNERyMUQxQzZmclJqVnAwTk4xU2VwSkYvS0JXb2cvWnRmSkN5WE9pTTVKd3Q1THo5SkVIRzluSjIyNE4waTlNMzgxcWVLc25ncjlMeFpWejBtSUhmam5wTC9mM3RuZTl6R2tZVGhtU3IvSnk4QzBoRWNMZ0xSRVpDT1FGUUVoNHRBVkFTQ0loQVZnYWtJREVaZ01BSURHUkFSN05YUXZkSVNCRUhzYkwrOVBUdnZVN1ZsKzRlSjNmbnNlYWMvUG9sN3pIbTZ3VEl5S3RGajAxMk80MkRrYzh6YSt6OFpmUUZVQkdvY0Y1RGp1S1RDTXhvY3M3aytHdmt1cHMxVkt3cGJGUm0zejhhdXBLSHJpZ1M3LzUzRHFhUmFtL0w2ZWk5dGVkNTVrTi83SUlmQXBWenpyZ3dMSjZBUDg4Y2E4Mmx1ZlFXL1N5cnNnemhnb05qSWVtTTFEamZ0K091TXd6RkZ3T295VlFTMGNXeVVtNVNaS295UmZrVXZEa093dUFwMnM3QWIrZUVlKzcwTHVaNUYrdFkvK1M0cXBpbUNJdW9PWWdOSVJrWnE2OCtPUGxlVHBmaDN2MkJueko5bXpQbHVuNnpHTk5ROEhlYlRuQkpsRjdtK3QyWGhTMUdRMXdmRzRXeEg5T3E3Rmo5MittYnQwYjJneGh6SHdVQTVwci94TkVFYlk5NkRXd0tWNC8wWXFzZUxHT05kUWVvVGhIVEZMeHMwS2xPSXkzRytSMGtyT1F1Q3Q4TTgxZU1qMlhPek1FV3hya3JsR08xenpCekgwd1NWcjdKbGFKOWFCQkI0V3RUaHluSFA3MTBZK0pHZHNMenhENUNieTZGK3IrMkFpTUxWWWQ3STkvakVTM0F0ZVpQcWNoeUhLUmpIeEJhNUFyd0UvNmo3dEVqZ3YrK05YdDhyaHJURmxlbDg0bVYzUFhCb0xxSVBpTFgwcmNmRHZFWFFxMVpaK0twdmo1RFVtdU00VE1BNExpM0hNWG9RV0d3bUZvdm1VRVZxU2xsQmpnR2U0N2p2LzJDb1BsRTl4akxteGxWTEdqbDNoM201a2JYWVh6VU8wZWdiakpwRnZpcHpIQWNxeDdZWU9OdGJiQ1pvWTJTckVDRStwYXdnSHNqOVhndjErS05SNEs5bmtJZWpNZDBxYWpHT3ZSN21MZWJ2K3dJS1NwMVZmRU5WWlk3ak1BSGptUDdHejRFcWlCSVpqMDRkcGRHblh0SWlXZUVoeC9FTGpOVGp3TUlDT040NHFLTG53VWtCaHBNR0xnL3podXJ4SU45am83Z2pkSnlOVjZyTWNSd01qR1BtT0g0SnNrakNPOVFKVi82dWhSRXlhS0Zqam1NSVEwNzJWdXBUemVyeEtOOXVaSlRVNERiaitUQnZNWDgxeXNLaml3L1ZXdmE2eWt3VkFXa2NNOGZ4cTZBTksvWE5SQXpPcFVIVWFpZ2hVMFdGT1k2ekZ6QlJqeTBDR0d1T2ZFZUpFTWVvaG1qbHA0UnJkKzlrN3prRitSNmo5OVZrd05lb0hxUEZKcmZCbEVqbHVIWS93TmRBRDRhUG1wdEp4ekMyS01lOEtjRGZPRkE1N28yRituZHBkRkJ3aFFQRDBXSXUzRTNWNTlOWkFaL1hzSmkvN3dhV2hiY1F5bTRyUEtpaHY5ZnRYb28wanVHTFdhRTVqaTBHdzFMamxDdUwxZHJJTUE1S2lmeVo0MWlab1FFVGhVVytsd2JTY0RtbTN5M1c0RE5aMDZnZzV6Rm96eEhCNHB2QmV3Nlp2eGI3Nm9tTXc1b080ZWpiNGlxVll5NWsrN0dheEgvRUdOTkV2dTZqdXFRTktBWGV4UmpYVWlYSndwV2lSYU1jTUhNYzY2SjFiVjZLNzJJeFNJQXNxakplY0dRY0J6bWcvNVZLRzA5TVJmWld3T2MxTE9idjJRRDEyR29jcHYzd3p4amo3ZFRqSEl3T28yNlZZMlQ1YVBRQ1ZscU80eGJMd2ZCT25xOHh4bzFzZHV1ZFRlKzBjNUJCWnowNGhJWkxSV0NPWTNWVUFpYVNlaHhqdkRjWVk0c3BIOHpGTUx5U0FDRjBXNzQ1SDlPY2xiWEZLdmo2bzdpT2ZaZWJwcVhYVkZCT1VEbk1wemFPTVg0REg4WVNOemxsNGFWc3ZjWDYwdkplL09Idk8rTndhdTUyRm9kUXQ4b3gwamltY3J3SFdXUXNONU9XTTNuR05JQVBvYUVhQitZNFZrZlQ4RWpxMDUvZzkvMTNVcDhrRUZBRk1VaG44b3lsV3M0a2pzUEt4U24wNlB1bGdkRzB5MlZicVRQR3VKVjV1WkxOZGxjQXNHQ1ZxZEM2SytCemdCdURmajRUVjZFY3BmcHVoUDJ0RmFDNjQ3QWRmNDhqN0JkcnhjT2loWXVpMy8yMGFScklJNE9pQVQ0TDFMdWpIMUcza0cxVDRuT3EwZTRHMzMzamJDd1Y5YjFpU0tIZmVhMzByaGV5NGRZNEh4OTd0bE9OYmZSaTNDVlYzOWw4VU4wblJjU0FqNzJjL1VBT2o3V1B3WFljemhYNitnYmR6K2o5Y2NpRDlEbXVyUkJESDFpODVEbmZOUHppbU9NWWdyWUNaNUV2OUV6OGNiTkk0eWo1NjR2S2ZXbnd2aDQ1ZW8yU2dFdTN5ZndOT1pOWWp6NWozR1VCbndQY0dPUVV6aW9MTDRycGQ4d3JGVVVhaDUrVFgvVEFsNjQyVTBWQUJlVFJTRGxNMHpSMzNFeWVvUlhzd1J6SCtxZ2F4N0tCbVVTKzU2eERFb1N5ZHV4K1pFWGZ6REdzVXZpVHI0NkN0VlQzU1ptL0ZqbkY1NWx0V0hPKzgxM2VEOHhhVlcyTzR3RE1WbEdWa1pLSm1rOWs0WHhSOUpIaW9Vd2ZoTyttaFNIVlczMFN3OWlxMkkxbnRobkc4WjJCb2xnU2I0N3hRbkljNzJOaHBCNzNYaWNNMDBhV3dwQ2JPaXJIQUdpa3ZBMk40MzhXV0UxRGlUbU9sVUZrQWpCVWorYzkxZU5iR3NaUFpHVUxvSHI4REM4QjZlcjdwUFMxaFVLYld4YWU0L0FuUTl6Q3FzMXhIRXBXanAwWktiMFJBK0ZMWWErdHpWeTVINW5qV0JlazY0K1ZlbnpVNzZUOHVjYVpJRHlUMVRkTjB5em9MdmFEWTR5NlVuSWM3OE5DUFE1VWo4ZWg5aHpIQVZ3RUJNbFVqQlNMNEFhdmZOZE10eVhVbHVNWS9iMndkRmlHNnZGL2oxU2ZMRXJrbHNCUU55ZUxnTXNTOExEeHcvWkpZL1U0NXhCeFRUZWZKM0lQcTFYbk9BNUE0N2lrM0kyalVmRlY1TGJRVGRUYlNSZTlnS0Z6eGM0OXFFOFN0RUozQ2dVM0oxSHRhcjhSQzBmNmJKZStUM3BXajljODhENlJteGtMSGxEcXZXaEtxY3J4WklLaTVDcXl0dlF6RjRXNnhkU21IRVBubVNQZnhTRVIzVlBpV21OZU5rMHpyOUFGYVplK0FZMElvQ2xEWmF4WUdLQlpaZUhsWnRMaWRzb3p1ZU1RYlJ5N1YvVlJ4akhhZUoyRWN0emh1cUxONUVQQlpUYTl2VGRhN2JUWTRLM1VwME11UEt6bUdjSW5TVEdweFVYRkJ2TDlrYTRwNkhTQjhIejZZb0JhK0pubitzSFh0TGZ1c2hrd3A5RjduWHNiQUdVY282OWpKNVZPUzA3Z1Z4WDRTSDBBK0JsM1FSK2FQT1U0Umh0MEQ0aE1GYnZJMkxkd3NUbWtQdFVlaUplSzhLaTZkMG0vMW1vZ2UzQ1YyNGlMaXdWVzh6ZjNkMm9kaDBOdTVhclpTMThEWlJ5akorWFVsT1BXUjJxcWszaHJZQmdIZzNFSE54WjdnUFkzTmt1bUwrcUdTV0VRZzk4b2pTK2lycWxUcVlGOGY0eFJhcERqMkN4VnFIenZKNE9meWxXUDIzRllrL3ZpZzdoc1ppRjlpaFRyNmxTTzVkb2NlZFV5eVVJTTBtNFhFMHREc3hFZlk0dkZHdm9iRmtwcUQ1REs4WU5SZi8xQUREVDB1SDlOUGE0eHFyMDlzRUo5Ump1R1NRMituMXNuL3VzYjYwcHhjdk9BN3VPelhQVTRqY09tYWE2TWpIZ1BhQng0a1c1MTFTckhBZW1vWDNxTzQwUElKTDZZeUNST1Vlc3pLeDlqR1Jlb1NIbHYrVnRSeXZHWW1VU3VETlNkZlVhL2grQXBTKzVsWHBvY2dHUk5TMlBxOXdrZlJMWTlBNDJSeXJGS1lHVmZwSS9SQnZJaXB5eDhpeGp4djAwOEg3ZFdYQS95cHExYW4rUDJxaFN4MFZWeE5TS1QrRCtGcXNqcG5YOUxxdFFJaS9RTjZCclh5bi92V0ZESzhYeXNnTW1PdW9OTUJiWlBmYW9sMzNnN0x5L0d1QVdSUGVGOGdpcHlheGg3MlBBL0dQb2F2MEFNWktTdzA3c3MvQzdTUHJPSnFzaHE3b3V5UnZ4UDQyL3R3Zi90ZjlNMHNFZlVyZFFJamVKempYeG5qNCtvREV2bGRrUThTOWtrUm0xSDBMZ2IvYnQydmxGN1BEd0ZoVHI2dml2eDhVYU0wL1dlMzVzQmY0L3o4bVY3bjR1S1gzcmJya1NGSDN2K05wNzJSdG16VVBNcHJWV25ISWZQbm5YT09EeXlqYlRiNTliVFd2VHFkeHRObEVWTmpRcHN4NWtNMUVkbmszS0JtcGdEMnVwVXJzczF2bkhoY0N4b2pvRzBVWjg3L01aVFVYVVI0MzMreW05ZXk3Z3AzVkJ1NTZXN2Z0MXA3M041ejlMYWV6M0VHRlUyanJNTWRLUCt2UUgxN1ExZ3Jaa1hPQTdiUW1JcWg0VUQ3WE90dEE2ckhXelFUNVFQaHlOSitHOUVFZXFibjNVckJncWp6UVVKTExxU0U3cGxLcXF0TE1acGNiL3puck5ZMmlrdGVwY1ovL3RXRm1IVDRKWmppREZxVE56dk1xKzh1WXc4UTN3TXIrVEo2Y2Q5Yk1Wd2ZOWHRSMzczUWc2bE05bEEwYmxwYzJublpUTHNseVhtRXBmMGhHMC9lMHV2OXlBYisxTGFkOUNjVVp5L3Q4cjVxU0dJSzFPN1gybmxaLzhWNFI0azQ3RGRYNzNOOTQwWThDc1poMlo5TCt2aFhBemxzNHcvY1MrM2swWEVqSmtaeDg5KzlCK0Q1ZlJJdjhtMENLMm1ISVNuZ2JUcFRKU1lkaVBQM1dEYWplQlJKbUg3ejZMN29lKzQ4MncweGhoekQ0cFAvZW5kSUQ2RWJGNm5Da0ZOdDdtYmF5ZnJ4YXdUSE5sOW54bXdTRXZYVUZ2THZKeGNCcC9PbXRaZDE3VGJkTk5KMGJqdS9IczdQeUJySHVldnl2dzFXYVAzN0szbm1jYmhJYllkUDl6SHpyOUR4MkV1SW5hZUg5bUhyZTNnS2R2VG00eGlIQk43M3NpcnljTUhJY3FJRWZEWGdMLzZwWk5KNDNHS0JuQU9vbURsQnFTeUhZa2FRL0pWbDN6QXFRRWF4NFFRb293WWNPc0JTdWU5cEhRa2hCQmlERExQTVNHRTFNcmRBTU40NDZTWUJDR0VWQW1OWTBJSVVVVDhTWWNFOGp5Q0t5QVNRZ2c1QU4wcUNDRkVpUmhqVW56L1VQcHo5NUl1cktoQUZrSUlLUjBheDRRUW9vQkVjSytVTXlwc0pTZXpTYWxuUWdnaE5JNEpJVVNGR09NS21KOVhyU3dzSVlTUXc5RG5tQkJDQmhKanZBVVhybGlJTWswSUlRUU1qV05DQ0JtQStCbS9CN2ZoaVZRWUpZUVFBb1p1RllRUW9zQ2Vxbmx0ZFRmTmFsci9Zc0VlUWdqQlF1T1lFRUtBaUR2RTdjRDBiaTMwUFNhRUVEQjBxeUNFRUNDU2l1MWE2UmVZLzVnUVFzRFFPQ2FFRUR4YXJoQTBqZ2toQkF5TlkwSUl3YU9sSEJOQ0NBRkQ0NWdRUW9ERUdPY2hoTTlzWTBJSUtZTmYyRStFRUtLTEJPRWxvL2hLT1ZzRk0xVVFRZ2dZR3NlRUVLS0VjbWFLZmF6WVY0UVFnb1Z1RllRUW9vQzRUL3dOTkl3VFMvWVZJWVJnWVo1alFnZ1ppSlNQUmxmSjJ6Wk5jOHErSW9RUUxGU09DU0ZrQUtJWW93M2p4SUw5UkFnaGVLZ2NFMEpJSnVKai9MZEIrMjFUam1NcEtFSUlJUVFJbFdOQ0NNbm54cWp0RmpTTUNTSEVCaXJIaEJDU1NZd3hwVlk3QWJmZlE5TTBySXhIQ0NGR1VEa21oSkFNWW93WEZvWnhDT0dDL1VNSUlYYlFPQ2FFRUo4OEdjWk4wN0R3QnlHRUdFTGptQkJDOGtBcXV0OXBHQk5DeURqUU9DYUVFRDlzUWdpL04wMXpSY09ZRUVMR2djWXhJWVNNVHpLS1B6Uk5jOTQwelIzN2d4QkN4dU1YdGowaGhHUXh0RnJkdlpTRHZtdWFac1V1SUlRUUh6Q1ZHeUdFWkJCanZBNGhuUGY0UDVmeXp6VnpGaE5DaUZOQ0NQOEhtUUVCclZpMmxEOEFBQUFBU1VWT1JLNUNZSUk9IiBhbHQ9IiIgd2lkdGg9IjEyMCIgaGVpZ2h0PSIxMjAiLz4KXV0+CiAgICAgICAgPC94c2w6dGV4dD4KPC9kaXY+CjxkaXYgc3R5bGU9ImNsZWFyOmJvdGg7Ii8+Cjx0YWJsZSBpZD0ia3VueWUiPgogICAgICAgIDx0Ym9keT4KICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGFyaWg6PC94c2w6dGV4dD4KICAgICAgICA8L3RoPgogICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpJc3N1ZURhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguLDYsMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4sMSw0KSIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RmF0dXJhIE5vOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6YXBwbHktdGVtcGxhdGVzLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICA8L3RkPgo8L3RyPgo8dHI+CiAgICAgICAgPHRoIHN0eWxlPSJ3aWR0aDoxMDVweDsiPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsOWemVsbGXFn3Rpcm1lIE5vOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQgc3R5bGU9IndpZHRoOjExMHB4OyI+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6Q3VzdG9taXphdGlvbklEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC90ZD4KPC90cj4KPHRyPgogICAgICAgIDx0aD4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5TZW5hcnlvOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6UHJvZmlsZUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC90ZD4KPC90cj4KPHRyPgogICAgICAgIDx0aD4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5GYXR1cmEgVGlwaTo8L3hzbDp0ZXh0PgogICAgICAgIDwvdGg+CiAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOkludm9pY2VUeXBlQ29kZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpBY2NvdW50aW5nQ29zdCAhPScnIj4KPHRyPgogICAgICAgIDx0aD4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5GYXR1cmEgQWx0IFRpcGk6PC94c2w6dGV4dD4KICAgICAgICA8L3RoPgogICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpBY2NvdW50aW5nQ29zdCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+CjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkFkZGl0aW9uYWxEb2N1bWVudFJlZmVyZW5jZVtjYmM6RG9jdW1lbnRUeXBlID0gJ8SwYWRlIEVkaWxlbiBGYXR1cmEnXSBvciAvL24xOkludm9pY2UvY2FjOkJpbGxpbmdSZWZlcmVuY2UvY2FjOkludm9pY2VEb2N1bWVudFJlZmVyZW5jZVtjYmM6RG9jdW1lbnRUeXBlID0gJ8SwYWRlIEVkaWxlbiBGYXR1cmEnXSI+Cjx4c2w6Y2hvb3NlPgogICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkJpbGxpbmdSZWZlcmVuY2UvY2FjOkludm9pY2VEb2N1bWVudFJlZmVyZW5jZVtjYmM6RG9jdW1lbnRUeXBlID0gJ8SwYWRlIEVkaWxlbiBGYXR1cmEnXSI+CiAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBzdHlsZT0idmVydGljYWwtYWxpZ246dG9wOyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsSwYWRlIEZhdHVyYSBObzo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QmlsbGluZ1JlZmVyZW5jZS9jYWM6SW52b2ljZURvY3VtZW50UmVmZXJlbmNlW2NiYzpEb2N1bWVudFR5cGUgPSAnxLBhZGUgRWRpbGVuIEZhdHVyYSddIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0icG9zaXRpb24oKSAhPTEiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOklEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGggc3R5bGU9InZlcnRpY2FsLWFsaWduOnRvcDsiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7EsGFkZSBGYXR1cmEgVGFyaWhpOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpCaWxsaW5nUmVmZXJlbmNlL2NhYzpJbnZvaWNlRG9jdW1lbnRSZWZlcmVuY2VbY2JjOkRvY3VtZW50VHlwZSA9ICfEsGFkZSBFZGlsZW4gRmF0dXJhJ10iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJwb3NpdGlvbigpICE9MSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOklzc3VlRGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpJc3N1ZURhdGUsNiwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyhjYmM6SXNzdWVEYXRlLDEsNCkiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2VbY2JjOkRvY3VtZW50VHlwZSA9ICfEsGFkZSBFZGlsZW4gRmF0dXJhJ10iPgogICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICA8dGggc3R5bGU9InZlcnRpY2FsLWFsaWduOnRvcDsiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7EsGFkZSBGYXR1cmEgTm86PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkFkZGl0aW9uYWxEb2N1bWVudFJlZmVyZW5jZVtjYmM6RG9jdW1lbnRUeXBlID0gJ8SwYWRlIEVkaWxlbiBGYXR1cmEnXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9InBvc2l0aW9uKCkgIT0xIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRoIHN0eWxlPSJ2ZXJ0aWNhbC1hbGlnbjp0b3A7Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+xLBhZGUgRmF0dXJhIFRhcmloaTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWRkaXRpb25hbERvY3VtZW50UmVmZXJlbmNlW2NiYzpEb2N1bWVudFR5cGUgPSAnxLBhZGUgRWRpbGVuIEZhdHVyYSddIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0icG9zaXRpb24oKSAhPTEiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpJc3N1ZURhdGUsOSwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyhjYmM6SXNzdWVEYXRlLDYsMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoY2JjOklzc3VlRGF0ZSwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgIDwveHNsOndoZW4+CjwveHNsOmNob29zZT4KPC94c2w6aWY+IAo8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Im4xOkludm9pY2UvY2FjOkRlc3BhdGNoRG9jdW1lbnRSZWZlcmVuY2UiPgogICAgICAgIDx0cj4KICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsSwcnNhbGl5ZSBObzo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjYWxsLXRlbXBsYXRlIG5hbWU9InJlbW92ZUxlYWRpbmdaZXJvcyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9Im9yaWdpbmFsU3RyaW5nIiBzZWxlY3Q9ImNiYzpJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjYWxsLXRlbXBsYXRlPgogICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICA8L3RyPgogICAgICAgIDx0cj4KICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsSwcnNhbGl5ZSBUYXJpaGk6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6SXNzdWVEYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4sOSwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguLDYsMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICA8L3RyPgo8L3hzbDpmb3ItZWFjaD4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOk9yZGVyUmVmZXJlbmNlIj4KPHRyPgogICAgICAgIDx0aD4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5TaXBhcmnFnyBObzo8L3hzbDp0ZXh0PgogICAgICAgIDwvdGg+CiAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Im4xOkludm9pY2UvY2FjOk9yZGVyUmVmZXJlbmNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2lwYXJpxZ8gVGFyaWhpOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0ibjE6SW52b2ljZS9jYWM6T3JkZXJSZWZlcmVuY2UiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOklzc3VlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguLDksMikiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4sMSw0KSIvPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvdGQ+CjwvdHI+CjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOlBheW1lbnRNZWFucy9jYmM6UGF5bWVudER1ZURhdGUgb3IgLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMvY2JjOlBheW1lbnREdWVEYXRlIj4KPHhzbDpjaG9vc2U+CiAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6UGF5bWVudFRlcm1zL2NiYzpQYXltZW50RHVlRGF0ZSI+CiAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U29uIMOWZGVtZSBUYXJpaGk6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNiYzpQYXltZW50RHVlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw5LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw2LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50TWVhbnMvY2JjOlBheW1lbnREdWVEYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5Tb24gw5ZkZW1lIFRhcmloaTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UvY2FjOlBheW1lbnRNZWFucyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYmM6UGF5bWVudER1ZURhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw5LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiw2LDIpIi8+LTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLiwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgPC94c2w6b3RoZXJ3aXNlPgo8L3hzbDpjaG9vc2U+CjwveHNsOmlmPiAKPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOklzc3VlVGltZSI+Cjx0cj4KICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+T2x1xZ9tYSBaYW1hbsSxOjwveHNsOnRleHQ+CiAgICAgICAgPC90aD4KICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NiYzpJc3N1ZVRpbWUiLz4KICAgICAgICA8L3RkPgo8L3RyPgo8L3hzbDppZj4gCgogICAgICAgIDwvdGJvZHk+CjwvdGFibGU+CgoKICAgICAgICA8L2Rpdj4KICAgICAgICAKPC9kaXY+CgoKPC9kaXY+Cgo8ZGl2IGNsYXNzPSJzYXRpcmxhciI+CiAgICAgICAgCiAgICAgICAgCiAgICAgICAgPHRhYmxlIGlkPSJtYWxIaXptZXRUYWJsb3N1Ij4KICAgICAgICAgICAgICAgIDx0Ym9keT4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBkYXRhLWlkPSJTQVRJUkxBUl9TSVJBTk8iPgogICAgICAgIDx4c2w6dGV4dD5TxLFyYSBObzwveHNsOnRleHQ+CjwvdGg+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9NQUxISVpNRVQiIGNsYXNzPSJhbGlnbkxlZnQiPgogICAgICAgIDx4c2w6dGV4dD5NYWwgSGl6bWV0PC94c2w6dGV4dD4KPC90aD4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpJdGVtL2NiYzpEZXNjcmlwdGlvbiI+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9BQ0lLTEFNQSI+CiAgICAgICAgPHhzbDp0ZXh0PkHDp8Sxa2xhbWE8L3hzbDp0ZXh0Pgo8L3RoPgo8L3hzbDppZj4gCjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9NSUtUQVIiPgogICAgICAgIDx4c2w6dGV4dD5NaWt0YXI8L3hzbDp0ZXh0Pgo8L3RoPgo8dGggZGF0YS1pZD0iU0FUSVJMQVJfQklSSU1GSVlBVCI+CiAgICAgICAgPHhzbDp0ZXh0PkJpcmltIEZpeWF0PC94c2w6dGV4dD4KPC90aD4KPHRoIGRhdGEtaWQ9IlNBVElSTEFSX01IVFVUQVJJIiBjbGFzcz0ibWhDb2x1bW4iPgogICAgICAgIDx4c2w6dGV4dD5NYWwgSGl6bWV0IFR1dGFyxLE8L3hzbDp0ZXh0Pgo8L3RoPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPScwMDE1JyI+Cjx0aCBkYXRhLWlkPSJTQVRJUkxBUl9LRFZPUkFOSSI+CiAgICAgICAgPHhzbDp0ZXh0PktEViBPcmFuxLE8L3hzbDp0ZXh0Pgo8L3RoPgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSA9JzAwMTUnIj4KPHRoIGRhdGEtaWQ9IlNBVElSTEFSX0tEVlRVVEFSSSI+Cgk8eHNsOnRleHQ+S0RWIFR1dGFyxLE8L3hzbDp0ZXh0Pgo8L3RoPgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQ9J0lIUkFDQVQnIj4KCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5VGVybXMvY2JjOklEW0BzY2hlbWVJRD0nSU5DT1RFUk1TJ10iPgoJCTx0aCBkYXRhLWlkPSJTQVRJUkxBUl9JSFJBQ0FUIj4KCQkJCTxzcGFuPgoJCQkJCQk8eHNsOnRleHQ+VGVzbGltIMWeYXJ0xLE8L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpQYWNrYWdpbmdUeXBlQ29kZSI+CgkJPHRoPgoJCQkJPHNwYW4+CgkJCQkJCTx4c2w6dGV4dD5FxZ95YSBLYXAgQ2luc2k8L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpJRCI+CgkJPHRoPgoJCQkJPHNwYW4+CgkJCQkJCTx4c2w6dGV4dD5LYXAgTm88L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpRdWFudGl0eSI+CgkJPHRoPgoJCQkJPHNwYW4+CgkJCQkJCTx4c2w6dGV4dD5LYXAgQWRldDwveHNsOnRleHQ+CgkJCQk8L3NwYW4+CgkJPC90aD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5QWRkcmVzcyI+CgkJPHRoPgoJCQkJPHNwYW4+CgkJCQkJCTx4c2w6dGV4dD5UZXNsaW0vQmVkZWwgw5ZkZW1lIFllcmk8L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6U2hpcG1lbnRTdGFnZS9jYmM6VHJhbnNwb3J0TW9kZUNvZGUiPgoJCTx0aD4KCQkJCTxzcGFuPgoJCQkJCQk8eHNsOnRleHQ+R8O2bmRlcmlsbWUgxZ5la2xpPC94c2w6dGV4dD4KCQkJCTwvc3Bhbj4KCQk8L3RoPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6UmVxdWlyZWRDdXN0b21zSUQiPgoJCTx0aD4KCQkJCTxzcGFuPgoJCQkJCQk8eHNsOnRleHQ+R1TEsFA8L3hzbDp0ZXh0PgoJCQkJPC9zcGFuPgoJCTwvdGg+CjwveHNsOmlmPgo8L3hzbDppZj4gCgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDphcHBseS10ZW1wbGF0ZXMgc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC90Ym9keT4KICAgICAgICA8L3RhYmxlPgo8L2Rpdj4KCgo8L3hzbDpmb3ItZWFjaD4KCjxkaXYgaWQ9InRvcGxhbWxhckNvbnRhaW5lciI+CiAgICAgICAgCiAgICAgICAgPGRpdiBjbGFzcz0idG9wbGFtbGFyIj4KICAgICAgICAKICAgICAgICA8ZGl2IGNsYXNzPSJ0b3BsYW1UYWJsbyI+Cgo8dGFibGUgaWQ9InRvcGxhbWxhciI+Cgk8dGJvZHk+CgkJCQk8dHI+CgkJCTx0aD4KCQkJCTx4c2w6dGV4dD5NYWwgSGl6bWV0IFRvcGxhbSBUdXRhcsSxOjwveHNsOnRleHQ+CgkJCTwvdGg+CgkJCTx0ZD4KCQkJCTx4c2w6dmFsdWUtb2YKCQkJCQkJCQkJc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CgkJCQk8eHNsOmlmCgkJCQkJCQkJCXRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50L0BjdXJyZW5jeUlEIj4KCQkJCQk8eHNsOnRleHQ+IDwveHNsOnRleHQ+CgkJCQkJPHNwYW4+CgkJCQkJCTx4c2w6aWYKCQkJCQkJCQkJCXRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgoJCQkJCQkJPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KCQkJCQkJPC94c2w6aWY+CgkJCQkJCTx4c2w6aWYKCQkJCQkJCQkJCXRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgoJCQkJCQkJPHhzbDp2YWx1ZS1vZgoJCQkJCQkJCQkJCXNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkxpbmVFeHRlbnNpb25BbW91bnQvQGN1cnJlbmN5SUQiCgkJCQkJCQkJCQkvPgoJCQkJCQk8L3hzbDppZj4KCQkJCQk8L3NwYW4+CgkJCQk8L3hzbDppZj4KCQkJPC90ZD4KCQk8L3RyPgoJCTx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQ9J0lIUkFDQVQnIj4KCQk8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk5ha2xpeWU6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpEZWNsYXJlZEZvckNhcnJpYWdlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NiYzpEZWNsYXJlZEZvckNhcnJpYWdlVmFsdWVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcigvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYmM6RGVjbGFyZWRGb3JDYXJyaWFnZVZhbHVlQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NiYzpEZWNsYXJlZEZvckNhcnJpYWdlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoMCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNob29zZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+Cgk8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRD0nSUhSQUNBVCciPgoJCTx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2lnb3J0YTo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcigvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpJbnN1cmFuY2VWYWx1ZUFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2JjOkluc3VyYW5jZVZhbHVlQW1vdW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2JjOkluc3VyYW5jZVZhbHVlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYmM6SW5zdXJhbmNlVmFsdWVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2JjOkluc3VyYW5jZVZhbHVlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpvdGhlcndpc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKDAsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpvdGhlcndpc2U+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJPC94c2w6aWY+CgkJICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkFsbG93YW5jZVRvdGFsQW1vdW50ICE9MCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VG9wbGFtIMSwc2tvbnRvOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOkFsbG93YW5jZVRvdGFsQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpBbGxvd2FuY2VUb3RhbEFtb3VudC9AY3VycmVuY3lJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpBbGxvd2FuY2VUb3RhbEFtb3VudC9AY3VycmVuY3lJRCIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpDaGFyZ2VUb3RhbEFtb3VudCAhPTAiPgogICAgICAgICAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRvcGxhbSBBcnTEsXLEsW06PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpDaGFyZ2VUb3RhbEFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6Q2hhcmdlVG90YWxBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6Q2hhcmdlVG90YWxBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT0nMDAxNSddIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LRFYgTWF0cmFoxLEgPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4oJTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlBlcmNlbnQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4pOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi9jYmM6VGF4YWJsZUFtb3VudFsuLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGU9JzAwMTUnXSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIuL2NiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIuL2NiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRoIGNsYXNzPSJzdW1UaXRsZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VmVyZ2kgSGFyacOnIFR1dGFyOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KCiAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpUYXhFeGNsdXNpdmVBbW91bnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpUYXhFeGNsdXNpdmVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpUYXhFeGNsdXNpdmVBbW91bnQvQGN1cnJlbmN5SUQiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICA8L3RyPgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtub3Qoc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUsJzgnKSBhbmQgKHN0cmluZy1sZW5ndGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUpID0zKSldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGggY2xhc3M9InN1bVRpdGxlIGlzLWxvbmcte3N0cmluZy1sZW5ndGgoY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUpID4gMTV9Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5IZXNhcGxhbmFuIDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gKCU8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpQZXJjZW50Ii8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+KSA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGUgPiAwIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAoPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZSIvPikKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PjogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlciguLi8uLi9jYmM6VGF4QW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi4vLi4vY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi4vLi4vY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0ibjE6SW52b2ljZS9jYWM6V2l0aGhvbGRpbmdUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUsJzYnKV0iPgogICAgICAgICAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoIGNsYXNzPSJzdW1UaXRsZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGV2a2lmYXRhIFRhYmkgxLDFn2xlbSBUdXRhcsSxIChLRFYpOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGNsYXNzPSJzdW1WYWx1ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iZm9ybWF0LW51bWJlcihzdW0objE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYmM6VGF4YWJsZUFtb3VudFsuLi8uLi8uLi9jYWM6V2l0aGhvbGRpbmdUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUsJzYnKV0gYW5kIC4uL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSA9MDAxNV0pLCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJuMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJCQkJPC94c2w6aWY+CgkJCQk8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDEsMWfbGVtIMOcemVyaW5kZW4gSGVzLiBLRFY6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQgY2xhc3M9InN1bVZhbHVlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKHN1bShuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYmM6VGF4YWJsZUFtb3VudFtzdGFydHMtd2l0aCguLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUsJzYnKV0pLCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJuMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJCQkJPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldIj4KCiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNicpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+S0RWIFRldmtpZmF0IFR1dGFyxLEgKDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSIvPik6CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGNsYXNzPSJzdW1WYWx1ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKGNiYzpUYXhBbW91bnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gVEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDEsMWfbGVtIFR1dGFyxLEgKMOWVFYpPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2JjOlRheGFibGVBbW91bnRbLi4vLi4vLi4vY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0JykgYW5kIC4uL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSA9MDA3MV1dKSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJuMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJCQkJPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UZXZraWZhdGEgVGFiaSDEsMWfbGVtIMOcemVyaW5kZW4gSGVzLiDDllRWPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoc3VtKG4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsL2NiYzpUYXhhYmxlQW1vdW50W3N0YXJ0cy13aXRoKC4uL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpXSksICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gVEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ibjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KCQkJCTwveHNsOmlmPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpXSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+w5ZUViBUZXZraWZhdCBUdXRhcsSxICg8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUiLz4pOgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCBjbGFzcz0ic3VtVmFsdWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi4vY2JjOlRheEFtb3VudCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CgkJCQk8L3hzbDppZj4KICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJzdW0objE6SW52b2ljZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT05MDE1XS9jYmM6VGF4YWJsZUFtb3VudCk+MCI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGV2a2lmYXRhIFRhYmkgxLDFn2xlbSBUdXRhcsSxOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQgY2xhc3M9InN1bVZhbHVlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKHN1bShuMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZVtjYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT05MDE1XS9jYmM6TGluZUV4dGVuc2lvbkFtb3VudCksICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJuMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICA8L3RyPgoJCQkJPC94c2w6aWY+CiAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0ic3VtKG4xOkludm9pY2UvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtjYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGU9OTAxNV0vY2JjOlRheGFibGVBbW91bnQpPjAiPgogICAgICAgICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGV2a2lmYXRhIFRhYmkgxLDFn2xlbSDDnHplcmluZGVuIEhlcy4gS0RWOjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQgY2xhc3M9InN1bVZhbHVlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJmb3JtYXQtbnVtYmVyKHN1bShuMTpJbnZvaWNlL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWxbY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlPTkwMTVdL2NiYzpUYXhhYmxlQW1vdW50KSwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Im4xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdGQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CgkJCQk8L3hzbDppZj4KICAgICAgICAgICAgICAgIDx0cj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5WZXJnaWxlciBEYWhpbCBUb3BsYW0gVHV0YXI6PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90aD4KICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGNsYXNzPSJzdW1WYWx1ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpMZWdhbE1vbmV0YXJ5VG90YWwiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlRheEluY2x1c2l2ZUFtb3VudCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC4sICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6VGF4SW5jbHVzaXZlQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbC9jYmM6VGF4SW5jbHVzaXZlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICA8dHIgY2xhc3M9InBheWFibGVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICA8dGg+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsOWZGVuZWNlayBUdXRhcjo8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpMZWdhbE1vbmV0YXJ5VG90YWwiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2JjOlBheWFibGVBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlciguLCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpMZWdhbE1vbmV0YXJ5VG90YWwvY2JjOlBheWFibGVBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUlknIG9yIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlRMPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6TGVnYWxNb25ldGFyeVRvdGFsL2NiYzpQYXlhYmxlQW1vdW50L0BjdXJyZW5jeUlEIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgIDwvdHI+CgoJPC90Ym9keT4KPC90YWJsZT4KPC9kaXY+CgoKPC9kaXY+CgoKPC9kaXY+Cgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6UHJvZmlsZUlEICE9ICdZT0xDVUJFUkFCRVJGQVRVUkEnIGFuZCAvL24xOkludm9pY2UvY2JjOlByb2ZpbGVJRCA9ICdJSFJBQ0FUJyI+CiAgICAgICAgPGRpdiBpZD0iaWhyYWNhdEJpbGdpbGVyaSI+CgogICAgICAgICAgICAgICAgPHRhYmxlIHdpZHRoPSIxMDAlIiBib3JkZXI9IjAiPgogICAgICAgICAgICAgICAgICAgICAgICA8dGJvZHk+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoIGFsaWduPSJsZWZ0Ij5FxZ95YSBCaWxnaWxlcmk8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRoIGFsaWduPSJsZWZ0Ij5BZHJlcyBCaWxnaWxlcmk8L3RoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRyPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIHZhbGlnbj0idG9wIiB3aWR0aD0iNTAlIj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VGVzbGltIMWeYXJ0xLE6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeVRlcm1zL2NiYzpJRFtAc2NoZW1lSUQ9J0lOQ09URVJNUyddIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5Hw7ZuZGVyaWxtZSDFnmVrbGk6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6U2hpcG1lbnRTdGFnZS9jYmM6VHJhbnNwb3J0TW9kZUNvZGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJUcmFuc3BvcnRNb2RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2l0aC1wYXJhbSBuYW1lPSJUcmFuc3BvcnRNb2RlVHlwZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aXRoLXBhcmFtPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmNhbGwtdGVtcGxhdGU+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkdUxLBQIE5vOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOkdvb2RzSXRlbS9jYmM6UmVxdWlyZWRDdXN0b21zSUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkXFn3lhIEthcCBDaW5zaTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpUcmFuc3BvcnRIYW5kbGluZ1VuaXQvY2FjOkFjdHVhbFBhY2thZ2UvY2JjOlBhY2thZ2luZ1R5cGVDb2RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNhbGwtdGVtcGxhdGUgbmFtZT0iUGFja2FnaW5nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2l0aC1wYXJhbSBuYW1lPSJQYWNrYWdpbmdUeXBlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndpdGgtcGFyYW0+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2FsbC10ZW1wbGF0ZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx0ZCB2YWlnbj0idG9wIiB3aWR0aD0iNTAlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PsOcbGtlOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzL2NhYzpDb3VudHJ5L2NiYzpOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7FnmVoaXI6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MvY2JjOkNpdHlOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7EsGzDp2U6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MvY2JjOkNpdHlTdWJkaXZpc2lvbk5hbWUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlBvc3RhIEtvZHU6IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MvY2JjOlBvc3RhbFpvbmUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlNva2FrOiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzL2NiYzpTdHJlZXROYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5ObzogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5QWRkcmVzcy9jYmM6QnVpbGRpbmdOdW1iZXIiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIgLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkJpbmEgQWTEsTogPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6RGVsaXZlcnkvY2FjOkRlbGl2ZXJ5QWRkcmVzcy9jYmM6QnVpbGRpbmdOYW1lIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+CiAgICAgICAgICAgICAgICAgICAgICAgIDwvdGJvZHk+CiAgICAgICAgICAgICAgICA8L3RhYmxlPgoKICAgICAgICA8L2Rpdj4KPC94c2w6aWY+CjxkaXYgaWQ9Im5vdGxhciI+CiAgICAgICAgCiAgICAgICAgPHRhYmxlPgogICAgICAgICAgICAgICAgPHRib2R5PgogICAgICAgICAgICAgICAgICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYmM6Tm90ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iMT0xIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNhbGwtdGVtcGxhdGUgbmFtZT0icmVwTkwiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aXRoLXBhcmFtIG5hbWU9InBUZXh0IiBzZWxlY3Q9Ii4iIC8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Y2FsbC10ZW1wbGF0ZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0ibjE6SW52b2ljZS9jYWM6VGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlLCc4JykgYW5kIChzdHJpbmctbGVuZ3RoKC4vY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlKSA9MyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7ICI+w5Z6ZWwgTWF0cmFoIEtvZHU6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZSIvPiAtIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbiIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsgIj7DlnplbCBNYXRyYWggRGV0YXnEsTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICg8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlIi8+KQogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IE9yYW46ICU8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlBlcmNlbnQiLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBWZXJnaSBUdXRhcsSxOgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlcihjYmM6VGF4QW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEID0gJ1RSWScgb3IgY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEICE9ICdUUlknIGFuZCBjYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEICE9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgVmVyZ2kgTWF0cmFoxLE6CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKGNiYzpUYXhhYmxlQW1vdW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJjYmM6VGF4YWJsZUFtb3VudC9AY3VycmVuY3lJRCA9ICdUUlknIG9yIGNiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEICE9ICdUUlknIGFuZCBjYmM6VGF4YWJsZUFtb3VudC9AY3VycmVuY3lJRCAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNiYzpUYXhhYmxlQW1vdW50L0BjdXJyZW5jeUlEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSJuMTpJbnZvaWNlL2NhYzpXaXRoaG9sZGluZ1RheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZSwnNCcpIG9yIHN0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc2JyldIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImRpc3BsYXk6aW5saW5lLWJsb2NrO2ZvbnQtd2VpZ2h0OmJvbGQ7ICB2ZXJ0aWNhbC1hbGlnbjogdG9wO3BhZGRpbmctcmlnaHQ6IDRweDsiPlRFVkvEsEZBVCBERVRBWUk6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImRpc3BsYXk6aW5saW5lLWJsb2NrOyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Im4xOkludm9pY2UvY2FjOldpdGhob2xkaW5nVGF4VG90YWwvY2FjOlRheFN1YnRvdGFsW3N0YXJ0cy13aXRoKC4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlLCc0Jykgb3Igc3RhcnRzLXdpdGgoLi9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUsJzYnKV0iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOlRheFR5cGVDb2RlIi8+IC0gPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUvY2JjOk5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGRpdiBzdHlsZT0iY2xlYXI6Ym90aCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY291bnQoLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2UvY2FjOkF0dGFjaG1lbnQvY2JjOkVtYmVkZGVkRG9jdW1lbnRCaW5hcnlPYmplY3QpICZndDsgMSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+xLBMQVZFIETDlkvDnE1BTkxBUjwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkFkZGl0aW9uYWxEb2N1bWVudFJlZmVyZW5jZSBhbmQgLy9uMTpJbnZvaWNlL2NhYzpBZGRpdGlvbmFsRG9jdW1lbnRSZWZlcmVuY2UvY2JjOkRvY3VtZW50VHlwZSE9J1hTTFQnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6QWRkaXRpb25hbERvY3VtZW50UmVmZXJlbmNlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkRvY3VtZW50VHlwZSE9J1hTTFQnIGFuZCBub3QoLi9jYmM6RG9jdW1lbnRUeXBlQ29kZSkiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+IEJlbGdlIE5vOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIuL2NiYzpJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOklzc3VlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+IEJlbGdlIFRhcmloaTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOklzc3VlRGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOklzc3VlRGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOklzc3VlRGF0ZSwxLDQpIi8+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkRvY3VtZW50VHlwZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+IEJlbGdlIFRpcGk6IDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOkRvY3VtZW50VHlwZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NhYzpBdHRhY2htZW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij4gQmVsZ2UgQWTEsTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2FjOkF0dGFjaG1lbnQvY2JjOkVtYmVkZGVkRG9jdW1lbnRCaW5hcnlPYmplY3QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIuL2NhYzpBdHRhY2htZW50L2NiYzpFbWJlZGRlZERvY3VtZW50QmluYXJ5T2JqZWN0L0BmaWxlbmFtZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50TWVhbnMiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPsOWREVNRSDFnkVLTMSwPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOlBheW1lbnRNZWFucyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+w5ZkZW1lIMWeZWtsaTogPC9zcGFuPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmNob29zZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJ1paWiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkRpxJ9lcjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6UGF5bWVudE1lYW5zQ29kZSAgPSAnMjAnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD7Dh2VrPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICc0MiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkhhdmFsZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJzYnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LcmVkaTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOlBheW1lbnRNZWFuc0NvZGUgID0gJzQ4JyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+S3JlZGkgS2FydMSxPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICcxMCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk5ha2l0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpQYXltZW50TWVhbnNDb2RlICA9ICc0OSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk90b21hdGlrIMOWZGVtZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6UGF5bWVudE1lYW5zQ29kZSAgPSAnNjAnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5TZW5ldDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6UGF5bWVudE1lYW5zQ29kZSAgPSAnMSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlPDtnpsZcWfbWUgS2Fwc2FtxLFuZGE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICYjMTYwOyYjMTYwOyYjMTYwOwoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpQYXltZW50RHVlRGF0ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPlNvbiDDlmRlbWUgVGFyaWhpOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpQYXltZW50RHVlRGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpQYXltZW50RHVlRGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKGNiYzpQYXltZW50RHVlRGF0ZSwxLDQpIi8+JiMxNjA7JiMxNjA7JiMxNjA7CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlBheW1lbnRDaGFubmVsQ29kZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPsOWZGVtZSBLYW5hbMSxOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iY2JjOlBheW1lbnRDaGFubmVsQ29kZSIvPiYjMTYwOyYjMTYwOyYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNhYzpQYXllZUZpbmFuY2lhbEFjY291bnQvY2JjOklEIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+IElCQU4gLyBIZXNhcCBObzogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpQYXllZUZpbmFuY2lhbEFjY291bnQvY2JjOklEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAoPHhzbDppZiB0ZXN0PSJjYWM6UGF5ZWVGaW5hbmNpYWxBY2NvdW50L2NiYzpDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciBjYWM6UGF5ZWVGaW5hbmNpYWxBY2NvdW50L2NiYzpDdXJyZW5jeUNvZGUgPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2FjOlBheWVlRmluYW5jaWFsQWNjb3VudC9jYmM6Q3VycmVuY3lDb2RlICE9ICdUUlknIGFuZCBjYWM6UGF5ZWVGaW5hbmNpYWxBY2NvdW50L2NiYzpDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iY2FjOlBheWVlRmluYW5jaWFsQWNjb3VudC9jYmM6Q3VycmVuY3lDb2RlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4pCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOkluc3RydWN0aW9uTm90ZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij7DlmRlbWUgxZ5la2xpIEHDp8Sxa2xhbWFzxLE6PC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImNiYzpJbnN0cnVjdGlvbk5vdGUiLz4mIzE2MDsmIzE2MDsmIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VQZXJpb2QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkZBVFVSQSBEw5ZORU0gQsSwTEfEsExFUsSwPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VQZXJpb2QiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6U3RhcnREYXRlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+QmHFn2xhbmfEscOnIFRhcmloaTo8L3NwYW4+JiMxNjA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOlN0YXJ0RGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOlN0YXJ0RGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOlN0YXJ0RGF0ZSwxLDQpIi8+JiMxNjA7JiMxNjA7JiMxNjA7CgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkVuZERhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij5CaXRpxZ8gVGFyaWhpOjwvc3Bhbj4mIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJzdWJzdHJpbmcoLi9jYmM6RW5kRGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOkVuZERhdGUsNiwyKSIKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgLz4tPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9InN1YnN0cmluZyguL2NiYzpFbmREYXRlLDEsNCkiLz4mIzE2MDsmIzE2MDsmIzE2MDsKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYmM6RHVyYXRpb25NZWFzdXJlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+RMO2bmVtIFBlcml5b2R1IC8gU8O8cmVzaTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOkR1cmF0aW9uTWVhc3VyZSIvPiYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSIuL2NiYzpEdXJhdGlvbk1lYXN1cmUvQHVuaXRDb2RlICA9ICdNT04nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5BeTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6RHVyYXRpb25NZWFzdXJlL0B1bml0Q29kZSAgPSAnREFZJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+R8O8bjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iLi9jYmM6RHVyYXRpb25NZWFzdXJlL0B1bml0Q29kZSAgPSAnSFVSJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2FhdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9Ii4vY2JjOkR1cmF0aW9uTWVhc3VyZS9AdW5pdENvZGUgID0gJ0FOTiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PlnEsWw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgJiMxNjA7CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOkRlc2NyaXB0aW9uIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+RmF0dXJhIETDtm5lbSBBw6fEsWtsYW1hc8SxOjwvc3Bhbj4mIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NiYzpEZXNjcmlwdGlvbiIvPiYjMTYwOyYjMTYwOyYjMTYwOwogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpSZWNlaXB0RG9jdW1lbnRSZWZlcmVuY2UiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkFMSU5ESSBCxLBMR8SwTEVSxLA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOlJlY2VpcHREb2N1bWVudFJlZmVyZW5jZSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPkJlbGdlIE51bWFyYXPEsTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2JjOklEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpJc3N1ZURhdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij4gQmVsZ2UgVGFyaWhpOiA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOklzc3VlRGF0ZSw5LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOklzc3VlRGF0ZSw2LDIpIgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPi08eHNsOnZhbHVlLW9mIHNlbGVjdD0ic3Vic3RyaW5nKC4vY2JjOklzc3VlRGF0ZSwxLDQpIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpEb2N1bWVudFR5cGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij4gQmVsZ2UgVGlwaTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2JjOkRvY3VtZW50VHlwZSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxici8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6UGF5bWVudFRlcm1zL2NiYzpQZW5hbHR5U3VyY2hhcmdlUGVyY2VuIG9yIC8vbjE6SW52b2ljZS9jYWM6UGF5bWVudFRlcm1zL2NiYzpBbW91bnQgb3IgLy9uMTpJbnZvaWNlL2NhYzpQYXltZW50VGVybXMvY2JjOk5vdGUiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3BhbiBzdHlsZT0iZm9udC13ZWlnaHQ6Ym9sZDsiPsOWREVNRSBLT8WeVUxMQVJJPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgoKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaCBzZWxlY3Q9Ii8vbjE6SW52b2ljZS9jYWM6UGF5bWVudFRlcm1zIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4vY2JjOlBlbmFsdHlTdXJjaGFyZ2VQZXJjZW50Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+R2VjaWttZSBDZXphIE9yYW7EsTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiAlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC4vY2JjOlBlbmFsdHlTdXJjaGFyZ2VQZXJjZW50LCAnIyMjLiMjMCwwMCcsICdldXJvcGVhbicpIi8+JiMxNjA7JiMxNjA7JiMxNjA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpBbW91bnQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4gc3R5bGU9ImZvbnQtd2VpZ2h0OmJvbGQ7Ij5HZWNpa21lIENlemEgVHV0YXLEsTo8L3NwYW4+JiMxNjA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iZm9ybWF0LW51bWJlciguL2NiYzpBbW91bnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpBbW91bnQvQGN1cnJlbmN5SUQiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSWScgb3IgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSA9ICdUUkwnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUTDwveHNsOnRleHQ+JiMxNjA7JiMxNjA7JiMxNjA7CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJZJyBhbmQgLy9uMTpJbnZvaWNlL2NiYzpEb2N1bWVudEN1cnJlbmN5Q29kZSAhPSAnVFJMJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpQcmljZS9jYmM6UHJpY2VBbW91bnQvQGN1cnJlbmN5SUQiLz4mIzE2MDsmIzE2MDsmIzE2MDsKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgoKCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpOb3RlIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuIHN0eWxlPSJmb250LXdlaWdodDpib2xkOyI+QcOnxLFrbGFtYTogPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2JjOk5vdGUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmZvci1lYWNoIHNlbGVjdD0iLy9uMTpJbnZvaWNlL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpQZXJjZW50PTAgYW5kIGNhYzpUYXhDYXRlZ29yeS9jYWM6VGF4U2NoZW1lL2NiYzpUYXhUeXBlQ29kZT0mYXBvczswMDE1JmFwb3M7IGFuZCBub3QoY2FjOlRheENhdGVnb3J5L2NiYzpUYXhFeGVtcHRpb25SZWFzb25Db2RlID4gMCkiPiAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+VmVyZ2kgxLBzdGlzbmEgTXVhZml5ZXQgU2ViZWJpOiA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGJyLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpmb3ItZWFjaD4KCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIvL24xOkludm9pY2UvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbFtub3QoLi9jYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbkNvZGU9cHJlY2VkaW5nOjoqKV0iPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZSA+IDAgYW5kIG5vdChzdGFydHMtd2l0aCguL2NhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZSwnOCcpIGFuZCAoc3RyaW5nLWxlbmd0aCguL2NhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZSkgPTMpKSI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPGI+VmVyZ2kgxLBzdGlzbmEgTXVhZml5ZXQgU2ViZWJpOiA8L2I+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9ImNhYzpUYXhDYXRlZ29yeS9jYmM6VGF4RXhlbXB0aW9uUmVhc29uQ29kZSIvPiAtIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2JjOlRheEV4ZW1wdGlvblJlYXNvbiIvPiAtIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSJjYWM6VGF4Q2F0ZWdvcnkvY2JjOk5hbWUiLz4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8YnIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD4KICAgICAgICAgICAgICAgICAgICAgICAgPC90cj4KICAgICAgICAgICAgICAgIDwvdGJvZHk+CiAgICAgICAgPC90YWJsZT4KPC9kaXY+CgoKICAgICAgICAgICAgICAgIDwvZGl2PgogICAgICAgICAgICAgICAgCiAgICAgICAgPC9kaXY+CjwvYm9keT4KCjwvaHRtbD4KCjwveHNsOnRlbXBsYXRlPgogICAgICAgIAo8eHNsOnRlbXBsYXRlIG1hdGNoPSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lIj4KICAgICAgICAKICAgICAgICA8dHI+CiAgICAgICAgICAgICAgICA8dGQ+CiAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iZm9ybWF0LW51bWJlciguL2NiYzpJRCwgJyMnKSIvPgogICAgICAgIDwvc3Bhbj4KPC90ZD4KPHRkIGNsYXNzPSJ3cmFwIj4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpJdGVtL2NiYzpOYW1lIi8+CiAgICAgICAgICAgICAgICA8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpJdGVtL2NiYzpCcmFuZE5hbWUiLz4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOkl0ZW0vY2JjOk1vZGVsTmFtZSIvPgogICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi9jYWM6SXRlbS9jYWM6Q29tbW9kaXR5Q2xhc3NpZmljYXRpb24vY2JjOkl0ZW1DbGFzc2lmaWNhdGlvbkNvZGUiLz4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOkl0ZW0vY2FjOkl0ZW1JbnN0YW5jZS9jYmM6U2VyaWFsSUQiLz4KICAgICAgICA8L3NwYW4+CjwvdGQ+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6SXRlbS9jYmM6RGVzY3JpcHRpb24iPgo8dGQgY2xhc3M9IndyYXAiPgogICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4vY2FjOkl0ZW0vY2JjOkRlc2NyaXB0aW9uIi8+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8L3hzbDppZj4gCjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2YKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSJmb3JtYXQtbnVtYmVyKC4vY2JjOkludm9pY2VkUXVhbnRpdHksICcjLiMjIy4jIyMsIyMjIyMjIyMnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpJbnZvaWNlZFF1YW50aXR5L0B1bml0Q29kZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2ggc2VsZWN0PSIuL2NiYzpJbnZvaWNlZFF1YW50aXR5Ij4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICcyNiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VG9uPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ1NFVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+U2V0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0JYJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LdXR1PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0xUUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TFQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnSFVSJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5TYWF0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ05JVScgb3IgQHVuaXRDb2RlICA9ICdDNjInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkFkZXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS0dNJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5LRzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLSk8nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PmtKPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dSTSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdNR00nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk1HPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ05UJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5OZXQgVG9uPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dUJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5HVDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdNVFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk08L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTU1UJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NTTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLVE0nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PktNPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ01MVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+TUw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTU1RJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NTTM8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQ0xUJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5DTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdDTUsnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkNNMjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdDTVEnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkNNMzwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdDTVQnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PkNNPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0RNVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+RE08L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTVRLJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5NMjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdNVFEnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0Pk0zPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0RBWSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEfDvG48L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnTU9OJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQXk8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnUEEnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBQYWtldDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLV0gnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLV0g8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnRDYxJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gRGFraWthPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0Q2MiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFNhbml5ZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdBTk4nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBZxLFsPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0FGRiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEFGxLBGPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0FZUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEFsdMSxbiBBeWFyxLE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnQjMyJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS0cvTWV0cmUgS2FyZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdDQ1QnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBUb24gQmHFn8SxbmEgVGHFn8SxbWEgS2FwYXNpdGVzaTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdDUFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBBZGV0LcOHaWZ0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0QzMCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEJyw7x0IEthbG9yaSBEZcSfZXJpPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0Q0MCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEJpbiBMaXRyZTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdHRkknIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBGSVNTSUxFIMSwem90b3AgR3JhbcSxPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dNUyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEfDvG3DvMWfPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0dSTSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEdyYW08L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnSDYyJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gWcO8eiBBZGV0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0syMCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEtpbG9ncmFtIFBvdGFzeXVtIE9rc2l0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0s1OCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEt1cnV0dWxtdcWfIE5ldCBBxJ/EsXJsxLFrbMSxIEtnLjwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLNjInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvZ3JhbS1BZGV0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tGTyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IERpZm9zZm9yIFBlbnRhb2tzaXQgS2lsb2dyYW3EsTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLR00nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvZ3JhbTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLSDYnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvZ3JhbS1CYcWfPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tITyciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEhpZHJvamVuIFBlcm9rc2l0IEtpbG9ncmFtxLE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS01BJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gTWV0aWwgQW1pbmxlcmluIEtpbG9ncmFtxLE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS05JJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQXpvdHVuIEtpbG9ncmFtxLE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnS09IJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gS2cuIFBvdGFzeXVtIEhpZHJva3NpdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLUEgnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLZyBQb3Rhc3l1bSBPa3NpZDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLUFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvZ3JhbS3Dh2lmdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLU0QnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiAlOTAgS3VydSDDnHLDvG4gS2cuPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tTSCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFNvZHl1bSBIaWRyb2tzaXQgS2cuPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tVUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFVyYW55dW0gS2lsb2dyYW3EsTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdLV0gnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLaWxvd2F0dCBTYWF0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0tXVCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEtpbG93YXR0PC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0xQQSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IFNhZiBBbGtvbCBMaXRyZXNpPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ0xUUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IExpdHJlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ01UUiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IE1ldHJlPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ05DTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEjDvGNyZSBBZGVkaTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdOQ1InIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBLYXJhdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdPTVYnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBPVFYgTWFrdHUgVmVyZ2k8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnT1RCJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gT1RWIGJpcmltIGZpeWF0xLE8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnUFInIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiDDh2lmdDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlICA9ICdSOSciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IEJpbiBNZXRyZSBLw7xwPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgID0gJ1QzJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQmluIEFkZXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSAgPSAnVFdIJyI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDxzcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gQmluIEtpbG93YXR0IFNhYXQ8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSA9ICdHUk8nIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBHcm9zYTwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOndoZW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOndoZW4gdGVzdD0iQHVuaXRDb2RlID0gJ0RaTiciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IETDvHppbmU8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3NwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDp3aGVuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp3aGVuIHRlc3Q9IkB1bml0Q29kZSA9ICdNV0gnIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiBNRUdBV0FUVCBTQUFUICgxMDAwIGtXLmgpPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC9zcGFuPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6d2hlbj4KCQkJCQkJCQkJCTx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgPSAnWVJEJyI+CgkJCQkJCQkJCQkJPHNwYW4+CgkJCQkJCQkJCQkJCTx4c2w6dGV4dD4gWWFyZDwveHNsOnRleHQ+CgkJCQkJCQkJCQkJPC9zcGFuPgoJCQkJCQkJCQkJPC94c2w6d2hlbj4KCQkJCQkJCQkJCTx4c2w6d2hlbiB0ZXN0PSJAdW5pdENvZGUgPSAnRE1LJyI+CgkJCQkJCQkJCQkJPHNwYW4+CgkJCQkJCQkJCQkJCTx4c2w6dGV4dD4gRGVzaW1ldHJla2FyZTwveHNsOnRleHQ+CgkJCQkJCQkJCQkJPC9zcGFuPgoJCQkJCQkJCQkJPC94c2w6d2hlbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6b3RoZXJ3aXNlPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iQHVuaXRDb2RlIi8+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwvc3Bhbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOm90aGVyd2lzZT4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDpjaG9vc2U+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8dGQ+CiAgICAgICAgPHNwYW4+CiAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNlbGVjdD0iZm9ybWF0LW51bWJlciguL2NhYzpQcmljZS9jYmM6UHJpY2VBbW91bnQsICcjIyMuIyMwLCMjIyMjIyMjJywgJ2V1cm9wZWFuJykiLz4KICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi9jYWM6UHJpY2UvY2JjOlByaWNlQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NhYzpQcmljZS9jYmM6UHJpY2VBbW91bnQvQGN1cnJlbmN5SUQiLz4KICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICA8L3NwYW4+CjwvdGQ+Cjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi9jYmM6TGluZUV4dGVuc2lvbkFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50L0BjdXJyZW5jeUlEIj4KICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp0ZXh0PiA8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgPSAnVFJZJyBvciAvL24xOkludm9pY2UvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD5UTDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSWScgYW5kIC8vbjE6SW52b2ljZS9jYmM6RG9jdW1lbnRDdXJyZW5jeUNvZGUgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuL2NiYzpMaW5lRXh0ZW5zaW9uQW1vdW50L0BjdXJyZW5jeUlEIi8+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPScwMDE1JyI+Cjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6Zm9yLWVhY2gKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZWN0PSIuL2NhYzpUYXhUb3RhbC9jYWM6VGF4U3VidG90YWwvY2FjOlRheENhdGVnb3J5L2NhYzpUYXhTY2hlbWUiPgogICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9ImNiYzpUYXhUeXBlQ29kZT0nMDAxNScgIj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDppZiB0ZXN0PSIuLi8uLi9jYmM6UGVyY2VudCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+ICU8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHhzbDp2YWx1ZS1vZgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi4vLi4vY2JjOlBlcmNlbnQsICcjIyMuIyMwLDAwJywgJ2V1cm9wZWFuJykiCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgICAgICAgICA8L3hzbDppZj4KICAgICAgICAgICAgICAgIDwveHNsOmZvci1lYWNoPgogICAgICAgIDwvc3Bhbj4KPC90ZD4KPC94c2w6aWY+IAo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZS9jYmM6VGF4VHlwZUNvZGUgPScwMDE1JyI+Cjx0ZD4KICAgICAgICA8c3Bhbj4KICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgPHhzbDpmb3ItZWFjaAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9Ii4vY2FjOlRheFRvdGFsL2NhYzpUYXhTdWJ0b3RhbC9jYWM6VGF4Q2F0ZWdvcnkvY2FjOlRheFNjaGVtZSI+CiAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iY2JjOlRheFR5cGVDb2RlPScwMDE1JyAiPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6dGV4dD4gPC94c2w6dGV4dD4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxlY3Q9ImZvcm1hdC1udW1iZXIoLi4vLi4vY2JjOlRheEFtb3VudCwgJyMjIy4jIzAsMDAnLCAnZXVyb3BlYW4nKSIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIDx4c2w6aWYgdGVzdD0iLi4vLi4vY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCI+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+IDwveHNsOnRleHQ+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQgPSAnVFJZJyBvciAuLi8uLi9jYmM6VGF4QW1vdW50L0BjdXJyZW5jeUlEID0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnRleHQ+VEw8L3hzbDp0ZXh0PgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOmlmIHRlc3Q9Ii4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQgIT0gJ1RSWScgYW5kIC4uLy4uL2NiYzpUYXhBbW91bnQvQGN1cnJlbmN5SUQgIT0gJ1RSTCciPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8eHNsOnZhbHVlLW9mIHNlbGVjdD0iLi4vLi4vY2JjOlRheEFtb3VudC9AY3VycmVuY3lJRCIvPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC94c2w6aWY+CiAgICAgICAgICAgICAgICAgICAgICAgIDwveHNsOmlmPgogICAgICAgICAgICAgICAgPC94c2w6Zm9yLWVhY2g+CiAgICAgICAgPC9zcGFuPgo8L3RkPgo8L3hzbDppZj4gCjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NiYzpQcm9maWxlSUQ9J0lIUkFDQVQnIj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlUZXJtcy9jYmM6SURbQHNjaGVtZUlEPSdJTkNPVEVSTVMnXSI+CgkJPHRkIGNsYXNzPSJsaW5lVGFibGVUZCIgYWxpZ249InJpZ2h0Ij4KCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlUZXJtcy9jYmM6SURbQHNjaGVtZUlEPSdJTkNPVEVSTVMnXSI+CgkJCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJCQk8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CgkJCQk8L3hzbDpmb3ItZWFjaD4KCQk8L3RkPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlRyYW5zcG9ydEhhbmRsaW5nVW5pdC9jYWM6QWN0dWFsUGFja2FnZS9jYmM6UGFja2FnaW5nVHlwZUNvZGUiPgoJCTx0ZCBjbGFzcz0ibGluZVRhYmxlVGQiIGFsaWduPSJyaWdodCI+CgkJCQk8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KCQkJCTx4c2w6Zm9yLWVhY2ggc2VsZWN0PSJjYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpUcmFuc3BvcnRIYW5kbGluZ1VuaXQvY2FjOkFjdHVhbFBhY2thZ2UvY2JjOlBhY2thZ2luZ1R5cGVDb2RlIj4KCQkJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQkJCTx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJQYWNrYWdpbmciPgoJCQkJCQkJCTx4c2w6d2l0aC1wYXJhbSBuYW1lPSJQYWNrYWdpbmdUeXBlIj4KCQkJCQkJCQkJCTx4c2w6dmFsdWUtb2Ygc2VsZWN0PSIuIi8+CgkJCQkJCQkJPC94c2w6d2l0aC1wYXJhbT4KCQkJCQkJPC94c2w6Y2FsbC10ZW1wbGF0ZT4KCQkJCTwveHNsOmZvci1lYWNoPgoJCTwvdGQ+CjwveHNsOmlmPgo8eHNsOmlmIHRlc3Q9Ii8vbjE6SW52b2ljZS9jYWM6SW52b2ljZUxpbmUvY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpJRCI+CgkJPHRkIGNsYXNzPSJsaW5lVGFibGVUZCIgYWxpZ249InJpZ2h0Ij4KCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlRyYW5zcG9ydEhhbmRsaW5nVW5pdC9jYWM6QWN0dWFsUGFja2FnZS9jYmM6SUQiPgoJCQkJCQk8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KCQkJCQkJPHhzbDphcHBseS10ZW1wbGF0ZXMvPgoJCQkJPC94c2w6Zm9yLWVhY2g+CgkJPC90ZD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpUcmFuc3BvcnRIYW5kbGluZ1VuaXQvY2FjOkFjdHVhbFBhY2thZ2UvY2JjOlF1YW50aXR5Ij4KCQk8dGQgY2xhc3M9ImxpbmVUYWJsZVRkIiBhbGlnbj0icmlnaHQiPgoJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQk8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6VHJhbnNwb3J0SGFuZGxpbmdVbml0L2NhYzpBY3R1YWxQYWNrYWdlL2NiYzpRdWFudGl0eSI+CgkJCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJCQk8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CgkJCQk8L3hzbDpmb3ItZWFjaD4KCQk8L3RkPgo8L3hzbDppZj4KPHhzbDppZiB0ZXN0PSIvL24xOkludm9pY2UvY2FjOkludm9pY2VMaW5lL2NhYzpEZWxpdmVyeS9jYWM6RGVsaXZlcnlBZGRyZXNzIj4KCQk8dGQgY2xhc3M9ImxpbmVUYWJsZVRkIiBhbGlnbj0icmlnaHQiPgoJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQk8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkRlbGl2ZXJ5L2NhYzpEZWxpdmVyeUFkZHJlc3MiPgoJCQkJCQk8eHNsOnRleHQ+JiMxNjA7PC94c2w6dGV4dD4KCQkJCQkJPHhzbDphcHBseS10ZW1wbGF0ZXMvPgoJCQkJPC94c2w6Zm9yLWVhY2g+CgkJPC90ZD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpTaGlwbWVudFN0YWdlL2NiYzpUcmFuc3BvcnRNb2RlQ29kZSI+CgkJPHRkIGNsYXNzPSJsaW5lVGFibGVUZCIgYWxpZ249InJpZ2h0Ij4KCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJPHhzbDpmb3ItZWFjaCBzZWxlY3Q9ImNhYzpEZWxpdmVyeS9jYWM6U2hpcG1lbnQvY2FjOlNoaXBtZW50U3RhZ2UvY2JjOlRyYW5zcG9ydE1vZGVDb2RlIj4KCQkJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQkJCTx4c2w6Y2FsbC10ZW1wbGF0ZSBuYW1lPSJUcmFuc3BvcnRNb2RlIj4KCQkJCQkJCQk8eHNsOndpdGgtcGFyYW0gbmFtZT0iVHJhbnNwb3J0TW9kZVR5cGUiPgoJCQkJCQkJCQkJPHhzbDp2YWx1ZS1vZiBzZWxlY3Q9Ii4iLz4KCQkJCQkJCQk8L3hzbDp3aXRoLXBhcmFtPgoJCQkJCQk8L3hzbDpjYWxsLXRlbXBsYXRlPgoJCQkJPC94c2w6Zm9yLWVhY2g+CgkJPC90ZD4KPC94c2w6aWY+Cjx4c2w6aWYgdGVzdD0iLy9uMTpJbnZvaWNlL2NhYzpJbnZvaWNlTGluZS9jYWM6RGVsaXZlcnkvY2FjOlNoaXBtZW50L2NhYzpHb29kc0l0ZW0vY2JjOlJlcXVpcmVkQ3VzdG9tc0lEIj4KCQk8dGQgY2xhc3M9ImxpbmVUYWJsZVRkIiBhbGlnbj0icmlnaHQiPgoJCQkJPHhzbDp0ZXh0PiYjMTYwOzwveHNsOnRleHQ+CgkJCQk8eHNsOmZvci1lYWNoIHNlbGVjdD0iY2FjOkRlbGl2ZXJ5L2NhYzpTaGlwbWVudC9jYWM6R29vZHNJdGVtL2NiYzpSZXF1aXJlZEN1c3RvbXNJRCI+CgkJCQkJCTx4c2w6dGV4dD4mIzE2MDs8L3hzbDp0ZXh0PgoJCQkJCQk8eHNsOmFwcGx5LXRlbXBsYXRlcy8+CgkJCQk8L3hzbDpmb3ItZWFjaD4KCQk8L3RkPgo8L3hzbDppZj4KPC94c2w6aWY+IAoKICAgICAgICA8L3RyPgo8L3hzbDp0ZW1wbGF0ZT4KCjwveHNsOnN0eWxlc2hlZXQ+Cg==";
            #endregion
            attachment.AppendChild(embeddedDocumentBinaryObject);
            additionalDocumentReference.AppendChild(attachment);

            root.AppendChild(additionalDocumentReference);

            additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = gidenFatura.GidenFaturaId;
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            additionalDocumentReference.AppendChild(issueDate);
            documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            characterSetCode = doc.CreateAttribute("characterSetCode");
            characterSetCode.Value = "UTF-8";
            encodingCode = doc.CreateAttribute("encodingCode");
            encodingCode.Value = "Base64";
            mimeCode = doc.CreateAttribute("mimeCode");
            mimeCode.Value = "application/xml";
            embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            attachment.AppendChild(embeddedDocumentBinaryObject);
            additionalDocumentReference.AppendChild(attachment);

            #endregion

            #endregion

            // Faturaya ilişkin imza, adres vb. bilgiler yer almaktadır

            #region Signature

            var signature = doc.CreateElement("cac", "Signature", xmlnscac.Value);
            var signatureIdAttr = doc.CreateAttribute("schemeID");
            signatureIdAttr.Value = "VKN_TCKN";
            var signatureId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            signatureId.Attributes.Append(signatureIdAttr);
            signatureId.InnerText = "3250566851";
            signature.AppendChild(signatureId);
            var signatoryParty = doc.CreateElement("cac", "SignatoryParty", xmlnscac.Value);
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var signatureIdAttr2 = doc.CreateAttribute("schemeID");
            signatureIdAttr2.Value = "VKN";
            var signaturePartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            signaturePartyId.Attributes.Append(signatureIdAttr2);
            signaturePartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(signaturePartyId);
            signatoryParty.AppendChild(partyIdentification);

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            signatoryParty.AppendChild(postalAddress);
            signature.AppendChild(signatoryParty);

            #endregion

            var digitalSignatureAttachment = doc.CreateElement("cac", "DigitalSignatureAttachment", xmlnscac.Value);
            var externalReference = doc.CreateElement("cac", "ExternalReference", xmlnscac.Value);
            var uri = doc.CreateElement("cbc", "URI", xmlnscbc.Value);
            uri.InnerText = "#Signature";
            externalReference.AppendChild(uri);
            digitalSignatureAttachment.AppendChild(externalReference);
            signature.AppendChild(digitalSignatureAttachment);

            root.AppendChild(signature);

            #endregion

            // Burada sabit değerler verilmekte olup, Faturayı kesen kuruma veya firmaya ait bilgiler
            // yer almaktadır. Bu kısımda örnek olarak TÜRKŞEKER Fabrikaları Genel Müdürlüğü bilgileri
            // kullanılmıştır. Ancak değiştirip test edilebilir.

            #region AccountingSupplierParty

            var accountingSupplierParty = doc.CreateElement("cac", "AccountingSupplierParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            webSiteUri.InnerText = "https://www.turkseker.gov.tr";
            party.AppendChild(webSiteUri);
            var accountingSupplierPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr.Value = "VKN_TCKN";
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr2.Value = "VKN";
            var accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
            accountingSupplierPartyPartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(accountingSupplierPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(gidenFatura.AltBirimAd))
            {
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr2.Value = "SUBENO";
                accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                accountingSupplierPartyPartyId.InnerText = gidenFatura.AltBirimAd;
                partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                party.AppendChild(partyIdentification);
            }

            var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
            var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            partyNameReal.InnerText = "TÜRKİYE ŞEKER FABRİKALARI A.Ş.";
            partyName.AppendChild(partyNameReal);
            party.AppendChild(partyName);

            #region Postal Address

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = "3250566851";
            postalAddress.AppendChild(postalAddressId);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxSchemeName.InnerText = "Ankara Kurumlar";
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
            telephone.InnerText = "(312) 4585500";
            contact.AppendChild(telephone);
            var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
            telefax.InnerText = "(312) 4585800";
            contact.AppendChild(telefax);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            electronicMail.InnerText = "maliisler@turkseker.gov.tr";
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            accountingSupplierParty.AppendChild(party);

            root.AppendChild(accountingSupplierParty);

            #endregion

            // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
            {
                partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = gidenFatura.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.Adres))
                streetName.InnerText = gidenFatura.Adres;
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                citySubdivisionName.InnerText = gidenFatura.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                cityName.InnerText = gidenFatura.IlAd;
            postalAddress.AppendChild(cityName);
            postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                taxSchemeName.InnerText = gidenFatura.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                electronicMail.InnerText = gidenFatura.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
            {
                if (gidenFatura.VergiNo.Length == 11)
                {
                    var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                    if (liste.Count > 1)
                    {
                        var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                        var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                        firstName.InnerText = liste[0];
                        person.AppendChild(firstName);
                        var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                        var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                        familyName.InnerText = surname;
                        //familyName.InnerText = liste[1];
                        person.AppendChild(familyName);
                        party.AppendChild(person);
                    }
                }
            }

            accountingCustomerParty.AppendChild(party);

            root.AppendChild(accountingCustomerParty);

            #endregion

            if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
            {
                if (gidenFatura.VergiNo.Length == 10)
                {
                    // Bu kısımda fatura kesilen firma bilgileri yer almaktadır (Eğer sadece Tüzel Kişilikse, şahıs şirketleri için bu yapılmamaktadır)

                    #region BuyerCustomerParty

                    var buyerCustomerParty = doc.CreateElement("cac", "BuyerCustomerParty", xmlnscac.Value);
                    party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                    webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                    party.AppendChild(webSiteUri);
                    var buyerCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                    buyerCustomerPartyIdAttr.Value = "VKN";
                    partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                    var buyerCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    buyerCustomerPartyPartyId.Attributes.Append(buyerCustomerPartyIdAttr);
                    buyerCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
                    partyIdentification.AppendChild(buyerCustomerPartyPartyId);
                    party.AppendChild(partyIdentification);

                    if (gidenFatura.VergiNo.Length == 10)
                    {
                        partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                        partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                        partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                        partyName.AppendChild(partyNameReal);
                        party.AppendChild(partyName);
                    }

                    #region Postal Address

                    postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                    postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    postalAddressId.InnerText = gidenFatura.VergiNo;
                    postalAddress.AppendChild(postalAddressId);
                    room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                    postalAddress.AppendChild(room);
                    streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.Adres))
                        streetName.InnerText = gidenFatura.Adres;
                    postalAddress.AppendChild(streetName);
                    buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                    //buildingNumber.InnerText = "";
                    postalAddress.AppendChild(buildingNumber);
                    citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                        citySubdivisionName.InnerText = gidenFatura.IlceAd;
                    postalAddress.AppendChild(citySubdivisionName);
                    cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                        cityName.InnerText = gidenFatura.IlAd;
                    postalAddress.AppendChild(cityName);
                    postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                    //postalZone.InnerText = "";
                    postalAddress.AppendChild(postalZone);
                    country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                    countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    countryName.InnerText = "Türkiye";
                    country.AppendChild(countryName);
                    postalAddress.AppendChild(country);
                    party.AppendChild(postalAddress);

                    #endregion

                    partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                    taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                    taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                        taxSchemeName.InnerText = gidenFatura.VergiDairesi;
                    taxScheme.AppendChild(taxSchemeName);
                    partyTaxScheme.AppendChild(taxScheme);
                    party.AppendChild(partyTaxScheme);

                    contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                    electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                        electronicMail.InnerText = gidenFatura.EPostaAdresi;
                    contact.AppendChild(electronicMail);
                    party.AppendChild(contact);

                    if (gidenFatura.VergiNo.Length == 11)
                    {
                        var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                        if (liste.Count > 1)
                        {
                            var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                            var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                            firstName.InnerText = liste[0];
                            person.AppendChild(firstName);
                            var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                            var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                            familyName.InnerText = surname;
                            //familyName.InnerText = liste[1];
                            person.AppendChild(familyName);
                            party.AppendChild(person);
                        }
                    }

                    buyerCustomerParty.AppendChild(party);

                    root.AppendChild(buyerCustomerParty);

                    #endregion
                }
            }

            if (!string.IsNullOrEmpty(gidenFatura.BankaAd))
            {
                // Eğer Banka bilgileri mevcutsa burada banka bilgileri yer almaktadır

                #region PaymentMeans

                var paymentMeans = doc.CreateElement("cac", "PaymentMeans", xmlnscac.Value);
                var paymentMeansCode = doc.CreateElement("cbc", "PaymentMeansCode", xmlnscbc.Value);
                //paymentMeansCode.InnerText = "1";
                
                // Burada 1 verildiği zaman Sözleşme Kapsamında yazıyor metinde
                // ZZZ ise Diğer anlamında geliyor
                paymentMeansCode.InnerText = "ZZZ";
                paymentMeans.AppendChild(paymentMeansCode);
                var payeeFinancialAccount = doc.CreateElement("cac", "PayeeFinancialAccount", xmlnscac.Value);
                var payeeFinancialAccountId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IbanNo))
                    payeeFinancialAccountId.InnerText = gidenFatura.IbanNo;
                payeeFinancialAccount.AppendChild(payeeFinancialAccountId);
                var currencyCode = doc.CreateElement("cbc", "CurrencyCode", xmlnscbc.Value);
                currencyCode.InnerText = "TRY";
                payeeFinancialAccount.AppendChild(currencyCode);
                var paymentNote = doc.CreateElement("cbc", "PaymentNote", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.BankaAd) && !string.IsNullOrEmpty(gidenFatura.BankaSube))
                    paymentNote.InnerText = gidenFatura.BankaAd + " - " + gidenFatura.BankaSube;
                payeeFinancialAccount.AppendChild(paymentNote);
                paymentMeans.AppendChild(payeeFinancialAccount);
                root.AppendChild(paymentMeans);

                #endregion
            }

            #region KDVlerin Belirlenmesi

            var faturaKdvListesi = new List<FaturaKdvDTO>();
            foreach (var item in gidenFaturaDetayListesi)
            {
                var kodKdvTuruOran = item.KdvOran ?? 0;
                if (faturaKdvListesi.All(j => j.KdvOran != kodKdvTuruOran))
                {
                    var faturaKdv = new FaturaKdvDTO
                    {
                        KdvOran = kodKdvTuruOran,
                        KdvTutari = item.KdvTutari,
                        KdvHaricTutar = item.KdvHaricTutar
                    };
                    faturaKdvListesi.Add(faturaKdv);
                }
                else
                {
                    var index = faturaKdvListesi.FindIndex(j => j.KdvOran == kodKdvTuruOran);
                    faturaKdvListesi[index].KdvTutari += item.KdvTutari;
                    faturaKdvListesi[index].KdvHaricTutar += item.KdvHaricTutar;
                }

                if (faturaKdvListesi.Count > 0)
                    faturaKdvListesi = faturaKdvListesi.OrderBy(j => j.KdvOran).ToList();
            }

            #endregion

            #region TaxTotal

            var sayac = 1;
            foreach (var item in faturaKdvListesi)
            {
                // Burada vergi bilgileri yer almaktadır

                #region TaxTotal

                var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                var currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount.RemoveAllAttributes();
                taxAmount.Attributes.Append(currencyIdGeneral);
                taxAmount.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxTotal.AppendChild(taxAmount);
                var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                taxableAmount.Attributes.Append(currencyIdGeneral);
                taxableAmount.InnerText = Decimal.Round((decimal) item.KdvHaricTutar, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyIdGeneral = doc.CreateAttribute("currencyID");
                currencyIdGeneral.Value = "TRY";
                taxAmount2.Attributes.Append(currencyIdGeneral);
                taxAmount2.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                var calculationSequenceNumeric = doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                calculationSequenceNumeric.InnerText = sayac + ".0";
                taxSubTotal.AppendChild(calculationSequenceNumeric);
                var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = Decimal.Round((decimal) item.KdvOran, 0, MidpointRounding.AwayFromZero).ToString();
                taxSubTotal.AppendChild(percent);
                var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                if (item.KdvTutari == 0)
                {
                    if (kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                    {
                        var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "325";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }
                    else
                    {
                        var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "350";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText = "Diğerleri";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }
                }

                if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "701";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText =
                        "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }

                var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = "0015";
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                root.AppendChild(taxTotal);

                #endregion

                sayac++;
            }

            #endregion

            // Burada vergi bilgileri yer almaktadır (Açıklama Satırı haline getirildi)

            #region Açıklama Satırı (TaxTotal)

            //var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            //var currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            //taxAmount.RemoveAllAttributes();
            //taxAmount.Attributes.Append(currencyId);
            //taxAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxTotal.AppendChild(taxAmount);
            //var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            //var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            //taxableAmount.RemoveAllAttributes();
            //currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //taxableAmount.Attributes.Append(currencyId);
            //taxableAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxSubTotal.AppendChild(taxableAmount);
            //var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            //taxAmount2.RemoveAllAttributes();
            //currencyId = doc.CreateAttribute("currencyID");
            //currencyId.Value = "TRY";
            //taxAmount2.Attributes.Append(currencyId);
            //taxAmount2.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            //taxSubTotal.AppendChild(taxAmount2);
            //var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            //if (gidenFatura.KdvHaricTutar != 0)
            //    percent.InnerText = Decimal.Round(((decimal)gidenFatura.KdvTutari * 100) / (decimal)gidenFatura.KdvHaricTutar, 0, MidpointRounding.AwayFromZero).ToString();
            //else
            //    percent.InnerText = "0";
            //taxSubTotal.AppendChild(percent);
            //var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            //if (gidenFatura.KdvTutari == 0)
            //{
            //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
            //    taxExemptionReasonCode.InnerText = "325";
            //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
            //    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
            //    taxCategory.AppendChild(taxExemptionReasonCode);
            //    taxCategory.AppendChild(taxExemptionReason);
            //}
            //if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
            //{
            //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
            //    taxExemptionReasonCode.InnerText = "701";
            //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
            //    taxExemptionReason.InnerText = "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
            //    taxCategory.AppendChild(taxExemptionReasonCode);
            //    taxCategory.AppendChild(taxExemptionReason);
            //}
            //var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            //var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            //taxTypeCode.InnerText = "0015";
            //taxScheme2.AppendChild(taxTypeCode);
            //taxCategory.AppendChild(taxScheme2);
            //taxSubTotal.AppendChild(taxCategory);
            //taxTotal.AppendChild(taxSubTotal);
            //root.AppendChild(taxTotal);

            #endregion

            // KDV, KDV Hariç Tutar ve Toplam Tutar bilgileri yer almaktadır

            #region LegalMonetaryTotal

            var legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", xmlnscac.Value);
            var lineExtensionAmount = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmount.RemoveAllAttributes();
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmount.Attributes.Append(currencyId);
            lineExtensionAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(lineExtensionAmount);
            var taxExclusiveAmount = doc.CreateElement("cbc", "TaxExclusiveAmount", xmlnscbc.Value);
            taxExclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxExclusiveAmount.Attributes.Append(currencyId);
            taxExclusiveAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxExclusiveAmount);
            var taxInclusiveAmount = doc.CreateElement("cbc", "TaxInclusiveAmount", xmlnscbc.Value);
            taxInclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxInclusiveAmount.Attributes.Append(currencyId);
            taxInclusiveAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxInclusiveAmount);
            var payableAmount = doc.CreateElement("cbc", "PayableAmount", xmlnscbc.Value);
            payableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            payableAmount.Attributes.Append(currencyId);
            payableAmount.InnerText = Decimal
                .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(payableAmount);
            root.AppendChild(legalMonetaryTotal);

            #endregion

            sayac = 1;
            // Her bir fatura edtayı için hazırlanan bölümdür
            foreach (var item in gidenFaturaDetayListesi)
            {
                var detayBilgisi = item.BosluklariKaldir();

                #region InvoiceLine

                // Ölçü Birimi, KDV Hariç Tutar, Para Birimi gibi bilgiler yer almaktadır

                #region MetaData

                var invoiceLine = doc.CreateElement("cac", "InvoiceLine", xmlnscac.Value);
                var invoiceLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                invoiceLineId.InnerText = sayac.ToString();
                invoiceLine.AppendChild(invoiceLineId);
                var quantity = doc.CreateAttribute("unitCode");
                quantity.Value = !string.IsNullOrEmpty(detayBilgisi.GibKisaltma) ? detayBilgisi.GibKisaltma : "KGM";
                var invoicedQuantity = doc.CreateElement("cbc", "InvoicedQuantity", xmlnscbc.Value);
                invoicedQuantity.RemoveAllAttributes();
                invoicedQuantity.Attributes.Append(quantity);
                invoicedQuantity.InnerText = Decimal
                    .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                invoiceLine.AppendChild(invoicedQuantity);
                var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
                lineExtensionAmountDetail.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                lineExtensionAmountDetail.Attributes.Append(currencyId);
                lineExtensionAmountDetail.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                invoiceLine.AppendChild(lineExtensionAmountDetail);

                #endregion

                // Varsa iskonto bilgileri yer almaktadır

                #region AllowanceCharge

                var allowanceCharge = doc.CreateElement("cac", "AllowanceCharge", xmlnscac.Value);
                var chargeIndicator = doc.CreateElement("cbc", "ChargeIndicator", xmlnscbc.Value);
                chargeIndicator.InnerText = "false";
                allowanceCharge.AppendChild(chargeIndicator);
                decimal amount2 = 0;
                var toplamTutar = (decimal) detayBilgisi.KdvHaricTutar;
                var kodIskontoTuruOran = detayBilgisi.IskontoOran ?? 0;
                if (kodIskontoTuruOran > 0)
                {
                    toplamTutar = Decimal.Round((decimal) detayBilgisi.Miktar * (decimal) detayBilgisi.BirimFiyat, 4,
                        MidpointRounding.AwayFromZero);
                    var toplamTutar2 = Decimal.Round(toplamTutar * (100 - kodIskontoTuruOran) * (decimal) 0.01, 4,
                        MidpointRounding.AwayFromZero);
                    amount2 = toplamTutar - toplamTutar2;
                }

                var multiplierFactorNumeric = doc.CreateElement("cbc", "MultiplierFactorNumeric", xmlnscbc.Value);
                multiplierFactorNumeric.InnerText = Decimal
                    .Round(kodIskontoTuruOran * (decimal) 0.01, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                allowanceCharge.AppendChild(multiplierFactorNumeric);
                var amount = doc.CreateElement("cbc", "Amount", xmlnscbc.Value);
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                amount.RemoveAllAttributes();
                amount.Attributes.Append(currencyId);
                amount.InnerText = Decimal.Round(amount2, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                allowanceCharge.AppendChild(amount);
                var baseAmount = doc.CreateElement("cbc", "BaseAmount", xmlnscbc.Value);
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                baseAmount.RemoveAllAttributes();
                baseAmount.Attributes.Append(currencyId);
                baseAmount.InnerText = Decimal.Round(toplamTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                allowanceCharge.AppendChild(baseAmount);
                invoiceLine.AppendChild(allowanceCharge);

                #endregion

                // Vergi bilgileri yer almaktadır

                #region TaxTotal

                var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount.RemoveAllAttributes();
                taxAmount.Attributes.Append(currencyId);
                taxAmount.InnerText = Decimal.Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxTotal.AppendChild(taxAmount);
                var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxableAmount.Attributes.Append(currencyId);
                taxableAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount2.Attributes.Append(currencyId);
                taxAmount2.InnerText = Decimal.Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero)
                    .ToString().Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                if (detayBilgisi.KdvOran != null)
                    percent.InnerText = Decimal.Round((decimal) detayBilgisi.KdvOran, 2, MidpointRounding.AwayFromZero)
                        .ToString("N1").Replace(",", ".");
                taxSubTotal.AppendChild(percent);
                var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                if (detayBilgisi.KdvTutari == 0)
                {
                    if (kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                    {
                        var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "325";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }
                    else
                    {
                        var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "350";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText = "Diğerleri";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }
                }

                var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxTypeName.InnerText = "KDV";
                taxScheme2.AppendChild(taxTypeName);
                var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = "0015";
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                invoiceLine.AppendChild(taxTotal);

                #endregion

                // Fatura kesilen ürün detayı ve açıklama bilgileri yer almaktadır

                #region Item

                var invoiceItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
                if (!string.IsNullOrEmpty(detayBilgisi.MalzemeFaturaAciklamasi))
                {
                    var description = doc.CreateElement("cbc", "Description", xmlnscbc.Value);
                    description.InnerText = detayBilgisi.MalzemeFaturaAciklamasi;
                    invoiceItem.AppendChild(description);
                }

                var detayBilgisiName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(detayBilgisi.FaturaUrunTuru))
                    detayBilgisiName.InnerText = detayBilgisi.FaturaUrunTuru;
                invoiceItem.AppendChild(detayBilgisiName);
                invoiceLine.AppendChild(invoiceItem);

                #endregion

                // Birim fiyat bilgileri yer almaktadır

                #region Price

                var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
                var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
                priceAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                priceAmount.Attributes.Append(currencyId);
                priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                    ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".")
                    : "0.00";
                price.AppendChild(priceAmount);
                invoiceLine.AppendChild(price);

                #endregion

                root.AppendChild(invoiceLine);

                #endregion

                sayac++;
            }

            #endregion

            doc.AppendChild(root);

            #region Dosya Kaydı

            var fileName = aktarilacakKlasorAdi + "/" + gidenFatura.GidenFaturaId + ".xml";
            doc.Save(fileName);

            #endregion

            return fileName;
        }

        /// <summary>
        /// Giden Fatura Bilgisi Üzerinden E-Arşiv Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="gidenFatura">Giden Fatura Bilgisi</param>
        /// <param name="gidenFaturaDetayListesi">Giden Fatura Detay Listesi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        /// <param name="mustahsilMakbuzuMu">Müstahsil Makbuzu Mu Bilgisi</param>
        /// <returns>Kaydedilen Dosyanın Bilgisayardaki Adresi</returns>
        public static string EArsivXMLOlustur(GidenFaturaDTO gidenFatura,
            List<GidenFaturaDetayDTO> gidenFaturaDetayListesi, string aktarilacakKlasorAdi, bool mustahsilMakbuzuMu)
        {
            if (!mustahsilMakbuzuMu)
            {
                #region Şahıs Şirketi veya Personel Kaydından Gelecekse

                var doc = new XmlDocument();
                var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = doc.DocumentElement;
                doc.InsertBefore(declaration, root);
                //doc.AppendChild(doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

                root = doc.CreateElement("Invoice");

                gidenFatura = gidenFatura.BosluklariKaldir();

                var kodSatisTuruKod = 0;
                if (gidenFatura.SatisTuruKod != null)
                    kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;

                var kodFaturaTuruKod = 0;
                if (gidenFatura.FaturaTuruKod != null)
                    kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;

                var kodFaturaGrupTuruKod = 0;
                if (gidenFatura.FaturaGrupTuruKod != null)
                    kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

                #region Standart ve Faturaya Bağlı Bilgiler

                // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
                // yer almaktadır

                #region Standart Bilgiler

                root.RemoveAllAttributes();

                var locationAttribute = "xsi:schemaLocation";
                var location = doc.CreateAttribute("xsi", "schemaLocation",
                    "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
                location.Value =
                    "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
                //var location = doc.CreateAttribute(schemaLocation);
                //location.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
                var xmlns = doc.CreateAttribute("xmlns");
                xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
                var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
                xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
                var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
                xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
                var xmlnscac = doc.CreateAttribute("xmlns:cac");
                xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
                xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                var xmlnsext = doc.CreateAttribute("xmlns:ext");
                xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
                var xmlnsxades = doc.CreateAttribute("xmlns:xades");
                //xmlnsxades.Value = "http://uri.etsi.org/01903/v1.3.2#";
                xmlnsxades.Value = "http://uri.etsi.org/01903/v1.3.2";
                var xmlnsds = doc.CreateAttribute("xmlns:ds");
                //xmlnsds.Value = "http://www.w3.org/2000/09/xmldsig#";
                xmlnsds.Value = "http://www.w3.org/2000/09/xmldsig";

                root.Attributes.Append(location);
                root.Attributes.Append(xmlns);
                root.Attributes.Append(xmlnsxsi);
                root.Attributes.Append(xmlnscac);
                root.Attributes.Append(xmlnsext);
                root.Attributes.Append(xmlnsds);
                root.Attributes.Append(xmlnsxades);
                root.Attributes.Append(xmlnscbc);
                root.Attributes.Append(xmlnsn4);

                var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
                var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
                var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
                extUbl2.AppendChild(extUbl3);
                extUbl.AppendChild(extUbl2);

                root.AppendChild(extUbl);

                var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
                versionId.InnerText = "2.1";
                root.AppendChild(versionId);

                var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
                customizationId.InnerText = "TR1.2";
                root.AppendChild(customizationId);

                var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
                profileId.InnerText = "EARSIVFATURA";
                root.AppendChild(profileId);

                var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                    id.InnerText = gidenFatura.GibNumarasi;
                else
                    id.InnerText = "ABC" + gidenFatura.DuzenlemeTarihi?.Year +
                                   DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
                root.AppendChild(id);

                var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
                copyIndicator.InnerText = "false";
                root.AppendChild(copyIndicator);

                var gidenFaturaId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
                gidenFaturaId.InnerText = gidenFatura.GidenFaturaId.Length == 36
                    ? gidenFatura.GidenFaturaId.ToUpper()
                    : Guid.NewGuid().ToString().ToUpper();
                root.AppendChild(gidenFaturaId);

                var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
                issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
                root.AppendChild(issueDate);

                var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
                issueTime.InnerText = gidenFatura.DuzenlemeTarihi?.ToString("HH:mm:ss") ?? string.Empty;
                root.AppendChild(issueTime);

                var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
                invoiceTypeCode.InnerText = "SATIS";
                if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                    invoiceTypeCode.InnerText = "ISTISNA";
                if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                    invoiceTypeCode.InnerText = "IHRACKAYITLI";
                if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                    invoiceTypeCode.InnerText = "IADE";
                root.AppendChild(invoiceTypeCode);

                #region İrsaliye Bilgileri

                #region İrsaliye ve Tonaj Düzenlemesi

                var gidenFaturaDetayListesi2 = new List<GidenFaturaDetayDTO>();
                foreach (var item in gidenFaturaDetayListesi)
                {
                    if (!string.IsNullOrEmpty(item.IrsaliyeNo) && !string.IsNullOrEmpty(item.PlakaNo))
                    {
                        if (!gidenFaturaDetayListesi2.Any(j =>
                                j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo))
                        {
                            item.Tonaj = item.Miktar;
                            gidenFaturaDetayListesi2.Add(item);
                        }
                        else
                        {
                            var index = gidenFaturaDetayListesi2.FindIndex(j =>
                                j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo);
                            gidenFaturaDetayListesi2[index].Tonaj += item.Miktar;
                        }
                    }
                    else
                        gidenFaturaDetayListesi2.Add(item);
                }
                //if (gidenFaturaDetayListesi2.Count < 1)
                //    gidenFaturaDetayListesi2.Add(new GidenFaturaDetayDTO());

                #endregion İrsaliye ve Tonaj Düzenlemesi

                if (gidenFaturaDetayListesi2.Count > 0)
                {
                    foreach (var item in gidenFaturaDetayListesi2)
                    {
                        var irsaliyeNote = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                        irsaliyeNote.InnerText = "";
                        if (!string.IsNullOrEmpty(item.PlakaNo))
                            irsaliyeNote.InnerText += "Plaka No: " + item.PlakaNo;
                        if (!string.IsNullOrEmpty(item.SevkIrsaliyesiNo))
                            irsaliyeNote.InnerText += " İrsaliye No: " + item.SevkIrsaliyesiNo;
                        if (item.SevkIrsaliyeTarihi != null)
                            irsaliyeNote.InnerText +=
                                " İrsaliye Tarihi: " + item.SevkIrsaliyeTarihi?.ToShortDateString();
                        if (item.YuklemeFormuNo != null)
                            irsaliyeNote.InnerText += " Yükleme Formu No: " + item.YuklemeFormuNo;
                        if (item.Tonaj != null)
                            irsaliyeNote.InnerText += " Tonaj: " + item.Tonaj;
                        if ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                            kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode() &&
                            !string.IsNullOrEmpty(gidenFatura.Aciklama))
                            irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                        if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                            root.AppendChild(irsaliyeNote);
                    }
                }

                #endregion

                var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
                root.AppendChild(sendType);

                var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
                documentCurrencyCode.InnerText = "TRY";
                root.AppendChild(documentCurrencyCode);

                var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
                lineCountNumeric.InnerText = gidenFaturaDetayListesi.Count.ToString();
                root.AppendChild(lineCountNumeric);

                #endregion

                // Faturaya ilişkin imza, adres vb. bilgiler yer almaktadır

                #region Signature

                var signature = doc.CreateElement("cac", "Signature", xmlnscac.Value);
                var signatureIdAttr = doc.CreateAttribute("schemeID");
                signatureIdAttr.Value = "VKN_TCKN";
                var signatureId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                signatureId.Attributes.Append(signatureIdAttr);
                signatureId.InnerText = "3250566851";
                signature.AppendChild(signatureId);
                var signatoryParty = doc.CreateElement("cac", "SignatoryParty", xmlnscac.Value);
                var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var signatureIdAttr2 = doc.CreateAttribute("schemeID");
                signatureIdAttr2.Value = "VKN";
                var signaturePartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                signaturePartyId.Attributes.Append(signatureIdAttr2);
                signaturePartyId.InnerText = "3250566851";
                partyIdentification.AppendChild(signaturePartyId);
                signatoryParty.AppendChild(partyIdentification);

                #region Postal Address

                var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                streetName.InnerText = "Mithatpaşa Caddesi";
                postalAddress.AppendChild(streetName);
                var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                buildingNumber.InnerText = "14";
                postalAddress.AppendChild(buildingNumber);
                var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                citySubdivisionName.InnerText = "Çankaya";
                postalAddress.AppendChild(citySubdivisionName);
                var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                cityName.InnerText = "Ankara";
                postalAddress.AppendChild(cityName);
                var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                postalZone.InnerText = "06100";
                postalAddress.AppendChild(postalZone);
                var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                signatoryParty.AppendChild(postalAddress);
                signature.AppendChild(signatoryParty);

                #endregion

                var digitalSignatureAttachment = doc.CreateElement("cac", "DigitalSignatureAttachment", xmlnscac.Value);
                var externalReference = doc.CreateElement("cac", "ExternalReference", xmlnscac.Value);
                var uri = doc.CreateElement("cbc", "URI", xmlnscbc.Value);
                uri.InnerText = "#Signature";
                externalReference.AppendChild(uri);
                digitalSignatureAttachment.AppendChild(externalReference);
                signature.AppendChild(digitalSignatureAttachment);

                root.AppendChild(signature);

                #endregion

                // Burada sabit değerler verilmekte olup, Faturayı kesen kuruma veya firmaya ait bilgiler
                // yer almaktadır. Bu kısımda örnek olarak TÜRKŞEKER Fabrikaları Genel Müdürlüğü bilgileri
                // kullanılmıştır. Ancak değiştirip test edilebilir.

                #region AccountingSupplierParty

                var accountingSupplierParty = doc.CreateElement("cac", "AccountingSupplierParty", xmlnscac.Value);
                var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                webSiteUri.InnerText = "https://www.turkseker.gov.tr";
                party.AppendChild(webSiteUri);
                var accountingSupplierPartyIdAttr = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr.Value = "VKN_TCKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr2.Value = "VKN";
                var accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                accountingSupplierPartyPartyId.InnerText = "3250566851";
                partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                party.AppendChild(partyIdentification);

                if (!string.IsNullOrEmpty(gidenFatura.AltBirimAd))
                {
                    partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                    accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                    accountingSupplierPartyIdAttr2.Value = "SUBENO";
                    accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                    accountingSupplierPartyPartyId.InnerText = gidenFatura.AltBirimAd;
                    partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                    party.AppendChild(partyIdentification);
                }

                var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = "TÜRKİYE ŞEKER FABRİKALARI A.Ş.";
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);

                #region Postal Address

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = "3250566851";
                postalAddress.AppendChild(postalAddressId);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                streetName.InnerText = "Mithatpaşa Caddesi";
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                buildingNumber.InnerText = "14";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                citySubdivisionName.InnerText = "Çankaya";
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                cityName.InnerText = "Ankara";
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                postalZone.InnerText = "06100";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                party.AppendChild(postalAddress);

                #endregion

                var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxSchemeName.InnerText = "Ankara Kurumlar";
                taxScheme.AppendChild(taxSchemeName);
                partyTaxScheme.AppendChild(taxScheme);
                party.AppendChild(partyTaxScheme);

                var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                telephone.InnerText = "(312) 4585500";
                contact.AppendChild(telephone);
                var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                telefax.InnerText = "(312) 4585800";
                contact.AppendChild(telefax);
                var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                electronicMail.InnerText = "maliisler@turkseker.gov.tr";
                contact.AppendChild(electronicMail);
                party.AppendChild(contact);

                accountingSupplierParty.AppendChild(party);

                root.AppendChild(accountingSupplierParty);

                #endregion

                // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

                #region AccountingCustomerParty

                var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
                party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                party.AppendChild(webSiteUri);
                var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                accountingCustomerPartyIdAttr.Value = "TCKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
                accountingCustomerPartyPartyId.InnerText = gidenFatura.GercekKisiTcKimlikNo;
                partyIdentification.AppendChild(accountingCustomerPartyPartyId);
                party.AppendChild(partyIdentification);

                #region Postal Address

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = gidenFatura.GercekKisiTcKimlikNo;
                postalAddress.AppendChild(postalAddressId);
                var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                postalAddress.AppendChild(room);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IkametgahAdresi))
                    streetName.InnerText = gidenFatura.IkametgahAdresi;
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                //buildingNumber.InnerText = "";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                    citySubdivisionName.InnerText = gidenFatura.IlceAd;
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                    cityName.InnerText = gidenFatura.IlAd;
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                //postalZone.InnerText = "";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                party.AppendChild(postalAddress);

                #endregion

                partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                //if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                //    taxSchemeName.InnerText = gidenFatura.VergiDairesi;
                taxScheme.AppendChild(taxSchemeName);
                partyTaxScheme.AppendChild(taxScheme);
                party.AppendChild(partyTaxScheme);

                contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.CepTelefonNo))
                    telephone.InnerText = gidenFatura.CepTelefonNo;
                contact.AppendChild(telephone);
                telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                //if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
                //    telefax.InnerText = gidenFatura.FaksNo;
                contact.AppendChild(telefax);
                electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                    electronicMail.InnerText = gidenFatura.EPostaAdresi;
                contact.AppendChild(electronicMail);
                party.AppendChild(contact);

                var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                firstName.InnerText = gidenFatura.GercekKisiAd;
                person.AppendChild(firstName);
                var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                familyName.InnerText = gidenFatura.GercekKisiSoyad;
                person.AppendChild(familyName);
                party.AppendChild(person);

                accountingCustomerParty.AppendChild(party);

                root.AppendChild(accountingCustomerParty);

                #endregion

                if (!string.IsNullOrEmpty(gidenFatura.BankaAd))
                {
                    // Eğer Banka bilgileri mevcutsa burada banka bilgileri yer almaktadır

                    #region PaymentMeans

                    var paymentMeans = doc.CreateElement("cac", "PaymentMeans", xmlnscac.Value);
                    var paymentMeansCode = doc.CreateElement("cbc", "PaymentMeansCode", xmlnscbc.Value);
                    //paymentMeansCode.InnerText = "1";
                    
                    // Burada 1 verildiği zaman Sözleşme Kapsamında yazıyor metinde
                    // ZZZ ise Diğer anlamında geliyor
                    paymentMeansCode.InnerText = "ZZZ";
                    paymentMeans.AppendChild(paymentMeansCode);
                    var payeeFinancialAccount = doc.CreateElement("cac", "PayeeFinancialAccount", xmlnscac.Value);
                    var payeeFinancialAccountId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.IbanNo))
                        payeeFinancialAccountId.InnerText = gidenFatura.IbanNo;
                    payeeFinancialAccount.AppendChild(payeeFinancialAccountId);
                    var currencyCode = doc.CreateElement("cbc", "CurrencyCode", xmlnscbc.Value);
                    currencyCode.InnerText = "TRY";
                    payeeFinancialAccount.AppendChild(currencyCode);
                    var paymentNote = doc.CreateElement("cbc", "PaymentNote", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.BankaAd) && !string.IsNullOrEmpty(gidenFatura.BankaSube))
                        paymentNote.InnerText = gidenFatura.BankaAd + " - " + gidenFatura.BankaSube;
                    payeeFinancialAccount.AppendChild(paymentNote);
                    paymentMeans.AppendChild(payeeFinancialAccount);
                    root.AppendChild(paymentMeans);

                    #endregion
                }

                #region KDVlerin Belirlenmesi

                var faturaKdvListesi = new List<FaturaKdvDTO>();
                foreach (var item in gidenFaturaDetayListesi)
                {
                    var kodKdvTuruOran = item.KdvOran ?? 0;
                    if (faturaKdvListesi.All(j => j.KdvOran != kodKdvTuruOran))
                    {
                        var faturaKdv = new FaturaKdvDTO
                        {
                            KdvOran = kodKdvTuruOran,
                            KdvTutari = item.KdvTutari,
                            KdvHaricTutar = item.KdvHaricTutar
                        };
                        faturaKdvListesi.Add(faturaKdv);
                    }
                    else
                    {
                        var index = faturaKdvListesi.FindIndex(j => j.KdvOran == kodKdvTuruOran);
                        faturaKdvListesi[index].KdvTutari += item.KdvTutari;
                        faturaKdvListesi[index].KdvHaricTutar += item.KdvHaricTutar;
                    }

                    if (faturaKdvListesi.Count > 0)
                        faturaKdvListesi = faturaKdvListesi.OrderBy(j => j.KdvOran).ToList();
                }

                #endregion

                #region TaxTotal

                var sayac = 1;
                foreach (var item in faturaKdvListesi)
                {
                    // Burada vergi bilgileri yer almaktadır

                    #region TaxTotal

                    var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                    var currencyIdGeneral = doc.CreateAttribute("currencyID");
                    currencyIdGeneral.Value = "TRY";
                    var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount.RemoveAllAttributes();
                    taxAmount.Attributes.Append(currencyIdGeneral);
                    taxAmount.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                        .ToString().Replace(",", ".");
                    taxTotal.AppendChild(taxAmount);
                    var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                    var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                    taxableAmount.RemoveAllAttributes();
                    currencyIdGeneral = doc.CreateAttribute("currencyID");
                    currencyIdGeneral.Value = "TRY";
                    taxableAmount.Attributes.Append(currencyIdGeneral);
                    taxableAmount.InnerText = Decimal
                        .Round((decimal) item.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxSubTotal.AppendChild(taxableAmount);
                    var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount2.RemoveAllAttributes();
                    currencyIdGeneral = doc.CreateAttribute("currencyID");
                    currencyIdGeneral.Value = "TRY";
                    taxAmount2.Attributes.Append(currencyIdGeneral);
                    taxAmount2.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                        .ToString().Replace(",", ".");
                    taxSubTotal.AppendChild(taxAmount2);
                    var calculationSequenceNumeric =
                        doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                    calculationSequenceNumeric.InnerText = sayac + ".0";
                    taxSubTotal.AppendChild(calculationSequenceNumeric);
                    var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                    percent.InnerText = Decimal.Round((decimal) item.KdvOran, 0, MidpointRounding.AwayFromZero)
                        .ToString();
                    taxSubTotal.AppendChild(percent);
                    var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                    if (item.KdvTutari == 0)
                    {
                        if (kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                        {
                            var taxExemptionReasonCode =
                                doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                            taxExemptionReasonCode.InnerText = "325";
                            var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                            taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                            taxCategory.AppendChild(taxExemptionReasonCode);
                            taxCategory.AppendChild(taxExemptionReason);
                        }
                        else
                        {
                            var taxExemptionReasonCode =
                                doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                            taxExemptionReasonCode.InnerText = "350";
                            var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                            taxExemptionReason.InnerText = "Diğerleri";
                            taxCategory.AppendChild(taxExemptionReasonCode);
                            taxCategory.AppendChild(taxExemptionReason);
                        }
                    }

                    if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                    {
                        var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "701";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText =
                            "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }

                    var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                    var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    taxTypeName.InnerText = "Katma Değer Vergisi";
                    var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                    taxTypeCode.InnerText = "0015";
                    taxScheme2.AppendChild(taxTypeName);
                    taxScheme2.AppendChild(taxTypeCode);
                    taxCategory.AppendChild(taxScheme2);
                    taxSubTotal.AppendChild(taxCategory);
                    taxTotal.AppendChild(taxSubTotal);
                    root.AppendChild(taxTotal);

                    #endregion
                }

                #endregion

                // Burada vergi bilgileri yer almaktadır

                #region Açıklama Satırı (TaxTotal)

                //var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                //var currencyId = doc.CreateAttribute("currencyID");
                //currencyId.Value = "TRY";
                //var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                //taxAmount.RemoveAllAttributes();
                //taxAmount.Attributes.Append(currencyId);
                //taxAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
                //taxTotal.AppendChild(taxAmount);
                //var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                //var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                //taxableAmount.RemoveAllAttributes();
                //currencyId = doc.CreateAttribute("currencyID");
                //currencyId.Value = "TRY";
                //taxableAmount.Attributes.Append(currencyId);
                //taxableAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
                //taxSubTotal.AppendChild(taxableAmount);
                //var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                //taxAmount2.RemoveAllAttributes();
                //currencyId = doc.CreateAttribute("currencyID");
                //currencyId.Value = "TRY";
                //taxAmount2.Attributes.Append(currencyId);
                //taxAmount2.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
                //taxSubTotal.AppendChild(taxAmount2);
                //var calculationSequenceNumeric = doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                //calculationSequenceNumeric.InnerText = "1.0";
                //taxSubTotal.AppendChild(calculationSequenceNumeric);
                //var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                //if (gidenFatura.KdvHaricTutar != 0)
                //    percent.InnerText = Decimal.Round(((decimal)gidenFatura.KdvTutari * 100) / (decimal)gidenFatura.KdvHaricTutar, 0, MidpointRounding.AwayFromZero).ToString();
                //else
                //    percent.InnerText = "0";
                //taxSubTotal.AppendChild(percent);
                //var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                //if (gidenFatura.KdvTutari == 0)
                //{
                //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                //    taxExemptionReasonCode.InnerText = "325";
                //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                //    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                //    taxCategory.AppendChild(taxExemptionReasonCode);
                //    taxCategory.AppendChild(taxExemptionReason);
                //}
                //if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                //{
                //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                //    taxExemptionReasonCode.InnerText = "701";
                //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                //    taxExemptionReason.InnerText = "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
                //    taxCategory.AppendChild(taxExemptionReasonCode);
                //    taxCategory.AppendChild(taxExemptionReason);
                //}
                //var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                //var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                //taxTypeName.InnerText = "Katma Değer Vergisi";
                //var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                //taxTypeCode.InnerText = "0015";
                //taxScheme2.AppendChild(taxTypeName);
                //taxScheme2.AppendChild(taxTypeCode);
                //taxCategory.AppendChild(taxScheme2);
                //taxSubTotal.AppendChild(taxCategory);
                //taxTotal.AppendChild(taxSubTotal);
                //root.AppendChild(taxTotal);

                #endregion

                // KDV, KDV Hariç Tutar ve Toplam Tutar bilgileri yer almaktadır

                #region LegalMonetaryTotal

                var legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", xmlnscac.Value);
                var lineExtensionAmount = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
                lineExtensionAmount.RemoveAllAttributes();
                var currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                lineExtensionAmount.Attributes.Append(currencyId);
                lineExtensionAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(lineExtensionAmount);
                var taxExclusiveAmount = doc.CreateElement("cbc", "TaxExclusiveAmount", xmlnscbc.Value);
                taxExclusiveAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxExclusiveAmount.Attributes.Append(currencyId);
                taxExclusiveAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(taxExclusiveAmount);
                var taxInclusiveAmount = doc.CreateElement("cbc", "TaxInclusiveAmount", xmlnscbc.Value);
                taxInclusiveAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxInclusiveAmount.Attributes.Append(currencyId);
                taxInclusiveAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(taxInclusiveAmount);
                var allowanceTotalAmount = doc.CreateElement("cbc", "AllowanceTotalAmount", xmlnscbc.Value);
                allowanceTotalAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                allowanceTotalAmount.Attributes.Append(currencyId);
                allowanceTotalAmount.InnerText = "0.00";
                legalMonetaryTotal.AppendChild(allowanceTotalAmount);
                var payableAmount = doc.CreateElement("cbc", "PayableAmount", xmlnscbc.Value);
                payableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                payableAmount.Attributes.Append(currencyId);
                payableAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(payableAmount);
                root.AppendChild(legalMonetaryTotal);

                #endregion

                sayac = 1;
                // Her bir fatura detayı için hazırlanan bölümdür
                foreach (var item in gidenFaturaDetayListesi)
                {
                    var detayBilgisi = item.BosluklariKaldir();

                    #region InvoiceLine

                    // Ölçü Birimi, KDV Hariç Tutar, Para Birimi gibi bilgiler yer almaktadır

                    #region MetaData

                    var invoiceLine = doc.CreateElement("cac", "InvoiceLine", xmlnscac.Value);
                    var invoiceLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    invoiceLineId.InnerText = sayac.ToString();
                    invoiceLine.AppendChild(invoiceLineId);
                    var quantity = doc.CreateAttribute("unitCode");
                    quantity.Value = !string.IsNullOrEmpty(detayBilgisi.GibKisaltma) ? detayBilgisi.GibKisaltma : "KGM";
                    var invoicedQuantity = doc.CreateElement("cbc", "InvoicedQuantity", xmlnscbc.Value);
                    invoicedQuantity.RemoveAllAttributes();
                    invoicedQuantity.Attributes.Append(quantity);
                    invoicedQuantity.InnerText = Decimal
                        .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    invoiceLine.AppendChild(invoicedQuantity);
                    var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
                    lineExtensionAmountDetail.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    lineExtensionAmountDetail.Attributes.Append(currencyId);
                    lineExtensionAmountDetail.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    invoiceLine.AppendChild(lineExtensionAmountDetail);

                    #endregion

                    // Varsa iskonto bilgileri yer almaktadır

                    #region AllowanceCharge

                    var allowanceCharge = doc.CreateElement("cac", "AllowanceCharge", xmlnscac.Value);
                    var chargeIndicator = doc.CreateElement("cbc", "ChargeIndicator", xmlnscbc.Value);
                    chargeIndicator.InnerText = "false";
                    allowanceCharge.AppendChild(chargeIndicator);
                    decimal amount2 = 0;
                    var toplamTutar = (decimal) detayBilgisi.KdvHaricTutar;
                    var kodIskontoTuruOran = detayBilgisi.IskontoOran ?? 0;
                    if (kodIskontoTuruOran > 0)
                    {
                        toplamTutar = Decimal.Round((decimal) detayBilgisi.Miktar * (decimal) detayBilgisi.BirimFiyat,
                            4, MidpointRounding.AwayFromZero);
                        var toplamTutar2 = Decimal.Round(toplamTutar * (100 - kodIskontoTuruOran) * (decimal) 0.01,
                            4, MidpointRounding.AwayFromZero);
                        amount2 = toplamTutar - toplamTutar2;
                    }

                    var multiplierFactorNumeric = doc.CreateElement("cbc", "MultiplierFactorNumeric", xmlnscbc.Value);
                    multiplierFactorNumeric.InnerText = Decimal
                        .Round(kodIskontoTuruOran * (decimal) 0.01, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    allowanceCharge.AppendChild(multiplierFactorNumeric);
                    var amount = doc.CreateElement("cbc", "Amount", xmlnscbc.Value);
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    amount.RemoveAllAttributes();
                    amount.Attributes.Append(currencyId);
                    amount.InnerText = Decimal.Round(amount2, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    allowanceCharge.AppendChild(amount);
                    var baseAmount = doc.CreateElement("cbc", "BaseAmount", xmlnscbc.Value);
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    baseAmount.RemoveAllAttributes();
                    baseAmount.Attributes.Append(currencyId);
                    baseAmount.InnerText = Decimal.Round(toplamTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    allowanceCharge.AppendChild(baseAmount);
                    invoiceLine.AppendChild(allowanceCharge);

                    #endregion

                    // Vergi bilgileri yer almaktadır

                    #region TaxTotal

                    var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount.RemoveAllAttributes();
                    taxAmount.Attributes.Append(currencyId);
                    taxAmount.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxTotal.AppendChild(taxAmount);
                    var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                    var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                    taxableAmount.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    taxableAmount.Attributes.Append(currencyId);
                    taxableAmount.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxSubTotal.AppendChild(taxableAmount);
                    var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount2.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    taxAmount2.Attributes.Append(currencyId);
                    taxAmount2.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxSubTotal.AppendChild(taxAmount2);
                    var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                    if (detayBilgisi.KdvOran != null)
                        percent.InnerText = Decimal
                            .Round((decimal) detayBilgisi.KdvOran, 2, MidpointRounding.AwayFromZero).ToString("N1")
                            .Replace(",", ".");
                    taxSubTotal.AppendChild(percent);
                    var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                    var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                    var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    taxTypeName.InnerText = "KDV";
                    taxScheme2.AppendChild(taxTypeName);
                    var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                    taxTypeCode.InnerText = "0015";
                    taxScheme2.AppendChild(taxTypeCode);
                    taxCategory.AppendChild(taxScheme2);
                    taxSubTotal.AppendChild(taxCategory);
                    taxTotal.AppendChild(taxSubTotal);
                    invoiceLine.AppendChild(taxTotal);

                    #endregion

                    // Fatura kesilen ürün detayı ve açıklama bilgileri yer almaktadır

                    #region Item

                    var invoiceItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
                    if (!string.IsNullOrEmpty(detayBilgisi.MalzemeFaturaAciklamasi))
                    {
                        var description = doc.CreateElement("cbc", "Description", xmlnscbc.Value);
                        description.InnerText = detayBilgisi.MalzemeFaturaAciklamasi;
                        invoiceItem.AppendChild(description);
                    }

                    var detayBilgisiName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(detayBilgisi.FaturaUrunTuru))
                        detayBilgisiName.InnerText = detayBilgisi.FaturaUrunTuru;
                    invoiceItem.AppendChild(detayBilgisiName);
                    invoiceLine.AppendChild(invoiceItem);

                    #endregion

                    // Birim fiyat bilgileri yer almaktadır

                    #region Price

                    var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
                    var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
                    priceAmount.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    priceAmount.Attributes.Append(currencyId);
                    priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                        ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                            .Replace(",", ".")
                        : "0.00";
                    price.AppendChild(priceAmount);
                    invoiceLine.AppendChild(price);

                    #endregion

                    root.AppendChild(invoiceLine);

                    #endregion

                    sayac++;
                }

                #endregion

                doc.AppendChild(root);

                #region Dosya Kaydı

                var fileName = aktarilacakKlasorAdi + "/" + gidenFatura.GidenFaturaId + ".xml";
                doc.Save(fileName);

                #endregion

                return fileName;

                #endregion
            }
            else
            {
                #region Tüzel Kişi Bilgisinden Gelecekse

                var doc = new XmlDocument();
                var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = doc.DocumentElement;
                doc.InsertBefore(declaration, root);
                //doc.AppendChild(doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

                root = doc.CreateElement("Invoice");

                var kodSatisTuruKod = 0;
                if (gidenFatura.SatisTuruKod != null)
                    kodSatisTuruKod = gidenFatura.SatisTuruKod ?? 0;

                var kodFaturaTuruKod = 0;
                if (gidenFatura.FaturaTuruKod != null)
                    kodFaturaTuruKod = gidenFatura.FaturaTuruKod ?? 0;

                var kodFaturaGrupTuruKod = 0;
                if (gidenFatura.FaturaGrupTuruKod != null)
                    kodFaturaGrupTuruKod = gidenFatura.FaturaGrupTuruKod ?? 0;

                #region Standart ve Faturaya Bağlı Bilgiler

                // Faturanın türü, düzenleme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
                // yer almaktadır

                #region Standart Bilgiler

                root.RemoveAllAttributes();

                var locationAttribute = "xsi:schemaLocation";
                var location = doc.CreateAttribute("xsi", "schemaLocation",
                    "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
                location.Value =
                    "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
                //var location = doc.CreateAttribute(schemaLocation);
                //location.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ../xsdrt/maindoc/UBL-Invoice-2.1.xsd";
                var xmlns = doc.CreateAttribute("xmlns");
                xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
                var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
                xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
                var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
                xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
                var xmlnscac = doc.CreateAttribute("xmlns:cac");
                xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
                xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                var xmlnsext = doc.CreateAttribute("xmlns:ext");
                xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
                var xmlnsxades = doc.CreateAttribute("xmlns:xades");
                //xmlnsxades.Value = "http://uri.etsi.org/01903/v1.3.2#";
                xmlnsxades.Value = "http://uri.etsi.org/01903/v1.3.2";
                var xmlnsds = doc.CreateAttribute("xmlns:ds");
                //xmlnsds.Value = "http://www.w3.org/2000/09/xmldsig#";
                xmlnsds.Value = "http://www.w3.org/2000/09/xmldsig";

                root.Attributes.Append(location);
                root.Attributes.Append(xmlns);
                root.Attributes.Append(xmlnsxsi);
                root.Attributes.Append(xmlnscac);
                root.Attributes.Append(xmlnsext);
                root.Attributes.Append(xmlnsds);
                root.Attributes.Append(xmlnsxades);
                root.Attributes.Append(xmlnscbc);
                root.Attributes.Append(xmlnsn4);

                var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
                var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
                var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
                extUbl2.AppendChild(extUbl3);
                extUbl.AppendChild(extUbl2);

                root.AppendChild(extUbl);

                var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
                versionId.InnerText = "2.1";
                root.AppendChild(versionId);

                var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
                customizationId.InnerText = "TR1.2";
                root.AppendChild(customizationId);

                var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
                profileId.InnerText = "EARSIVFATURA";
                root.AppendChild(profileId);

                var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.GibNumarasi))
                    id.InnerText = gidenFatura.GibNumarasi;
                else
                    id.InnerText = "ABC" + gidenFatura.DuzenlemeTarihi?.Year +
                                   DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
                root.AppendChild(id);

                var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
                copyIndicator.InnerText = "false";
                root.AppendChild(copyIndicator);

                var gidenFaturaId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
                gidenFaturaId.InnerText = gidenFatura.GidenFaturaId.Length == 36
                    ? gidenFatura.GidenFaturaId.ToUpper()
                    : Guid.NewGuid().ToString().ToUpper();
                root.AppendChild(gidenFaturaId);

                var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
                issueDate.InnerText = gidenFatura.DuzenlemeTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
                root.AppendChild(issueDate);

                var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
                issueTime.InnerText = gidenFatura.DuzenlemeTarihi?.ToString("HH:mm:ss") ?? string.Empty;
                root.AppendChild(issueTime);

                var invoiceTypeCode = doc.CreateElement("cbc", "InvoiceTypeCode", xmlnscbc.Value);
                invoiceTypeCode.InnerText = "SATIS";
                if (gidenFatura.KdvTutari == 0 || gidenFaturaDetayListesi.Any(j => j.KdvTutari == 0))
                    invoiceTypeCode.InnerText = "ISTISNA";
                if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                    invoiceTypeCode.InnerText = "IHRACKAYITLI";
                if (kodFaturaTuruKod == FaturaTur.Iade.GetHashCode())
                    invoiceTypeCode.InnerText = "IADE";
                root.AppendChild(invoiceTypeCode);

                #region İrsaliye Bilgileri

                #region İrsaliye ve Tonaj Düzenlemesi

                var gidenFaturaDetayListesi2 = new List<GidenFaturaDetayDTO>();
                foreach (var item in gidenFaturaDetayListesi)
                {
                    if (!string.IsNullOrEmpty(item.IrsaliyeNo) && !string.IsNullOrEmpty(item.PlakaNo))
                    {
                        if (!gidenFaturaDetayListesi2.Any(j =>
                                j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo))
                        {
                            item.Tonaj = item.Miktar;
                            gidenFaturaDetayListesi2.Add(item);
                        }
                        else
                        {
                            var index = gidenFaturaDetayListesi2.FindIndex(j =>
                                j.IrsaliyeNo == item.IrsaliyeNo && j.PlakaNo == item.PlakaNo);
                            gidenFaturaDetayListesi2[index].Tonaj += item.Miktar;
                        }
                    }
                    else
                        gidenFaturaDetayListesi2.Add(item);
                }
                //if (gidenFaturaDetayListesi2.Count < 1)
                //    gidenFaturaDetayListesi2.Add(new GidenFaturaDetayDTO());

                #endregion İrsaliye ve Tonaj Düzenlemesi

                if (gidenFaturaDetayListesi2.Count > 0)
                {
                    foreach (var item in gidenFaturaDetayListesi2)
                    {
                        var irsaliyeNote = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                        irsaliyeNote.InnerText = "";
                        if (!string.IsNullOrEmpty(item.PlakaNo))
                            irsaliyeNote.InnerText += "Plaka No: " + item.PlakaNo;
                        if (!string.IsNullOrEmpty(item.SevkIrsaliyesiNo))
                            irsaliyeNote.InnerText += " İrsaliye No: " + item.SevkIrsaliyesiNo;
                        if (item.SevkIrsaliyeTarihi != null)
                            irsaliyeNote.InnerText +=
                                " İrsaliye Tarihi: " + item.SevkIrsaliyeTarihi?.ToShortDateString();
                        if (item.YuklemeFormuNo != null)
                            irsaliyeNote.InnerText += " Yükleme Formu No: " + item.YuklemeFormuNo;
                        if (item.Tonaj != null)
                            irsaliyeNote.InnerText += " Tonaj: " + item.Tonaj;
                        if ((item.FaturaUrunTuru.Contains("Şartname") || item.FaturaUrunTuru.Contains("Kira")) &&
                            kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Diger.GetHashCode() &&
                            !string.IsNullOrEmpty(gidenFatura.Aciklama))
                            irsaliyeNote.InnerText += " Açıklama: " + gidenFatura.Aciklama;
                        if (!string.IsNullOrEmpty(irsaliyeNote.InnerText))
                            root.AppendChild(irsaliyeNote);
                    }
                }

                #endregion

                var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
                sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
                root.AppendChild(sendType);

                var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
                documentCurrencyCode.InnerText = "TRY";
                root.AppendChild(documentCurrencyCode);

                var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
                lineCountNumeric.InnerText = gidenFaturaDetayListesi.Count.ToString();
                root.AppendChild(lineCountNumeric);

                #endregion

                // Faturaya ilişkin imza, adres vb. bilgiler yer almaktadır

                #region Signature

                var signature = doc.CreateElement("cac", "Signature", xmlnscac.Value);
                var signatureIdAttr = doc.CreateAttribute("schemeID");
                signatureIdAttr.Value = "VKN_TCKN";
                var signatureId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                signatureId.Attributes.Append(signatureIdAttr);
                signatureId.InnerText = "3250566851";
                signature.AppendChild(signatureId);
                var signatoryParty = doc.CreateElement("cac", "SignatoryParty", xmlnscac.Value);
                var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var signatureIdAttr2 = doc.CreateAttribute("schemeID");
                signatureIdAttr2.Value = "VKN";
                var signaturePartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                signaturePartyId.Attributes.Append(signatureIdAttr2);
                signaturePartyId.InnerText = "3250566851";
                partyIdentification.AppendChild(signaturePartyId);
                signatoryParty.AppendChild(partyIdentification);

                #region Postal Address

                var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                streetName.InnerText = "Mithatpaşa Caddesi";
                postalAddress.AppendChild(streetName);
                var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                buildingNumber.InnerText = "14";
                postalAddress.AppendChild(buildingNumber);
                var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                citySubdivisionName.InnerText = "Çankaya";
                postalAddress.AppendChild(citySubdivisionName);
                var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                cityName.InnerText = "Ankara";
                postalAddress.AppendChild(cityName);
                var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                postalZone.InnerText = "06100";
                postalAddress.AppendChild(postalZone);
                var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                signatoryParty.AppendChild(postalAddress);
                signature.AppendChild(signatoryParty);

                #endregion

                var digitalSignatureAttachment = doc.CreateElement("cac", "DigitalSignatureAttachment", xmlnscac.Value);
                var externalReference = doc.CreateElement("cac", "ExternalReference", xmlnscac.Value);
                var uri = doc.CreateElement("cbc", "URI", xmlnscbc.Value);
                uri.InnerText = "#Signature";
                externalReference.AppendChild(uri);
                digitalSignatureAttachment.AppendChild(externalReference);
                signature.AppendChild(digitalSignatureAttachment);

                root.AppendChild(signature);

                #endregion

                // Burada sabit değerler verilmekte olup, Faturayı kesen kuruma veya firmaya ait bilgiler
                // yer almaktadır. Bu kısımda örnek olarak TÜRKŞEKER Fabrikaları Genel Müdürlüğü bilgileri
                // kullanılmıştır. Ancak değiştirip test edilebilir.

                #region AccountingSupplierParty

                var accountingSupplierParty = doc.CreateElement("cac", "AccountingSupplierParty", xmlnscac.Value);
                var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                webSiteUri.InnerText = "https://www.turkseker.gov.tr";
                party.AppendChild(webSiteUri);
                var accountingSupplierPartyIdAttr = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr.Value = "VKN_TCKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr2.Value = "VKN";
                var accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                accountingSupplierPartyPartyId.InnerText = "3250566851";
                partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                party.AppendChild(partyIdentification);

                if (!string.IsNullOrEmpty(gidenFatura.AltBirimAd))
                {
                    partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                    accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                    accountingSupplierPartyIdAttr2.Value = "SUBENO";
                    accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                    accountingSupplierPartyPartyId.InnerText = gidenFatura.AltBirimAd;
                    partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                    party.AppendChild(partyIdentification);
                }

                var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = "TÜRKİYE ŞEKER FABRİKALARI A.Ş.";
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);

                #region Postal Address

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = "3250566851";
                postalAddress.AppendChild(postalAddressId);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                streetName.InnerText = "Mithatpaşa Caddesi";
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                buildingNumber.InnerText = "14";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                citySubdivisionName.InnerText = "Çankaya";
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                cityName.InnerText = "Ankara";
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                postalZone.InnerText = "06100";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                party.AppendChild(postalAddress);

                #endregion

                var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxSchemeName.InnerText = "Ankara Kurumlar";
                taxScheme.AppendChild(taxSchemeName);
                partyTaxScheme.AppendChild(taxScheme);
                party.AppendChild(partyTaxScheme);

                var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                telephone.InnerText = "(312) 4585500";
                contact.AppendChild(telephone);
                var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                telefax.InnerText = "(312) 4585800";
                contact.AppendChild(telefax);
                var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                electronicMail.InnerText = "maliisler@turkseker.gov.tr";
                contact.AppendChild(electronicMail);
                party.AppendChild(contact);

                accountingSupplierParty.AppendChild(party);

                root.AppendChild(accountingSupplierParty);

                #endregion

                // Bu kısımda fatura kesilen firma bilgileri yer almaktadır

                #region AccountingCustomerParty

                var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
                party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                party.AppendChild(webSiteUri);
                var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                accountingCustomerPartyIdAttr.Value = "TCKN";
                if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                    accountingCustomerPartyIdAttr.Value = "VKN";
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
                accountingCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
                partyIdentification.AppendChild(accountingCustomerPartyPartyId);
                party.AppendChild(partyIdentification);

                if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 10)
                {
                    partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                    partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                    partyName.AppendChild(partyNameReal);
                    party.AppendChild(partyName);
                }

                #region Postal Address

                postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                postalAddressId.InnerText = gidenFatura.VergiNo;
                postalAddress.AppendChild(postalAddressId);
                var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                postalAddress.AppendChild(room);
                streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.Adres))
                    streetName.InnerText = gidenFatura.Adres;
                postalAddress.AppendChild(streetName);
                buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                //buildingNumber.InnerText = "";
                postalAddress.AppendChild(buildingNumber);
                citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                    citySubdivisionName.InnerText = gidenFatura.IlceAd;
                postalAddress.AppendChild(citySubdivisionName);
                cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                    cityName.InnerText = gidenFatura.IlAd;
                postalAddress.AppendChild(cityName);
                postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                //postalZone.InnerText = "";
                postalAddress.AppendChild(postalZone);
                country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                countryName.InnerText = "Türkiye";
                country.AppendChild(countryName);
                postalAddress.AppendChild(country);
                party.AppendChild(postalAddress);

                #endregion

                partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                    taxSchemeName.InnerText = gidenFatura.VergiDairesi;
                taxScheme.AppendChild(taxSchemeName);
                partyTaxScheme.AppendChild(taxScheme);
                party.AppendChild(partyTaxScheme);

                contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.TelefonNo))
                    telephone.InnerText = gidenFatura.TelefonNo;
                contact.AppendChild(telephone);
                telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
                    telefax.InnerText = gidenFatura.FaksNo;
                contact.AppendChild(telefax);
                electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                    electronicMail.InnerText = gidenFatura.EPostaAdresi;
                contact.AppendChild(electronicMail);
                party.AppendChild(contact);

                if (!string.IsNullOrEmpty(gidenFatura.VergiNo) && gidenFatura.VergiNo.Length == 11)
                {
                    var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                    if (liste.Count > 1)
                    {
                        var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                        var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                        firstName.InnerText = liste[0];
                        person.AppendChild(firstName);
                        var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                        var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                        familyName.InnerText = surname;
                        //familyName.InnerText = liste[1];
                        person.AppendChild(familyName);
                        party.AppendChild(person);
                    }
                }

                accountingCustomerParty.AppendChild(party);

                root.AppendChild(accountingCustomerParty);

                #endregion

                if (!string.IsNullOrEmpty(gidenFatura.VergiNo))
                {
                    if (gidenFatura.VergiNo.Length == 10)
                    {
                        // Bu kısımda fatura kesilen firma bilgileri yer almaktadır (Eğer sadece Tüzel Kişilikse, şahıs şirketleri için bu yapılmamaktadır)

                        #region BuyerCustomerParty

                        var buyerCustomerParty = doc.CreateElement("cac", "BuyerCustomerParty", xmlnscac.Value);
                        party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                        webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                        party.AppendChild(webSiteUri);
                        var buyerCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                        buyerCustomerPartyIdAttr.Value = "VKN";
                        partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                        var buyerCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                        buyerCustomerPartyPartyId.Attributes.Append(buyerCustomerPartyIdAttr);
                        buyerCustomerPartyPartyId.InnerText = gidenFatura.VergiNo;
                        partyIdentification.AppendChild(buyerCustomerPartyPartyId);
                        party.AppendChild(partyIdentification);

                        if (gidenFatura.VergiNo.Length == 10)
                        {
                            partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                            partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                            partyNameReal.InnerText = gidenFatura.TuzelKisiAd;
                            partyName.AppendChild(partyNameReal);
                            party.AppendChild(partyName);
                        }

                        #region Postal Address

                        postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                        postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                        postalAddressId.InnerText = gidenFatura.VergiNo;
                        postalAddress.AppendChild(postalAddressId);
                        room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                        postalAddress.AppendChild(room);
                        streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.Adres))
                            streetName.InnerText = gidenFatura.Adres;
                        postalAddress.AppendChild(streetName);
                        buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                        //buildingNumber.InnerText = "";
                        postalAddress.AppendChild(buildingNumber);
                        citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.IlceAd))
                            citySubdivisionName.InnerText = gidenFatura.IlceAd;
                        postalAddress.AppendChild(citySubdivisionName);
                        cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.IlAd))
                            cityName.InnerText = gidenFatura.IlAd;
                        postalAddress.AppendChild(cityName);
                        postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                        //postalZone.InnerText = "";
                        postalAddress.AppendChild(postalZone);
                        country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                        countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                        countryName.InnerText = "Türkiye";
                        country.AppendChild(countryName);
                        postalAddress.AppendChild(country);
                        party.AppendChild(postalAddress);

                        #endregion

                        partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                        taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                        taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.VergiDairesi))
                            taxSchemeName.InnerText = gidenFatura.VergiDairesi;
                        taxScheme.AppendChild(taxSchemeName);
                        partyTaxScheme.AppendChild(taxScheme);
                        party.AppendChild(partyTaxScheme);

                        contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                        telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.TelefonNo))
                            telephone.InnerText = gidenFatura.TelefonNo;
                        contact.AppendChild(telephone);
                        telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.FaksNo))
                            telefax.InnerText = gidenFatura.FaksNo;
                        contact.AppendChild(telefax);
                        electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                        if (!string.IsNullOrEmpty(gidenFatura.EPostaAdresi))
                            electronicMail.InnerText = gidenFatura.EPostaAdresi;
                        contact.AppendChild(electronicMail);
                        party.AppendChild(contact);

                        if (gidenFatura.VergiNo.Length == 11)
                        {
                            var liste = gidenFatura.TuzelKisiAd.Split(' ').ToList();
                            if (liste.Count > 1)
                            {
                                var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                                var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                                firstName.InnerText = liste[0];
                                person.AppendChild(firstName);
                                var surname = gidenFatura.TuzelKisiAd.Substring(liste[0].Length + 1);
                                var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                                familyName.InnerText = surname;
                                //familyName.InnerText = liste[1];
                                person.AppendChild(familyName);
                                party.AppendChild(person);
                            }
                        }

                        buyerCustomerParty.AppendChild(party);

                        root.AppendChild(buyerCustomerParty);

                        #endregion
                    }
                }

                if (!string.IsNullOrEmpty(gidenFatura.BankaAd))
                {
                    // Eğer Banka bilgileri mevcutsa burada banka bilgileri yer almaktadır

                    #region PaymentMeans

                    var paymentMeans = doc.CreateElement("cac", "PaymentMeans", xmlnscac.Value);
                    var paymentMeansCode = doc.CreateElement("cbc", "PaymentMeansCode", xmlnscbc.Value);
                    //paymentMeansCode.InnerText = "1";
                    
                    // Burada 1 verildiği zaman Sözleşme Kapsamında yazıyor metinde
                    // ZZZ ise Diğer anlamında geliyor
                    paymentMeansCode.InnerText = "ZZZ";
                    paymentMeans.AppendChild(paymentMeansCode);
                    var payeeFinancialAccount = doc.CreateElement("cac", "PayeeFinancialAccount", xmlnscac.Value);
                    var payeeFinancialAccountId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.IbanNo))
                        payeeFinancialAccountId.InnerText = gidenFatura.IbanNo;
                    payeeFinancialAccount.AppendChild(payeeFinancialAccountId);
                    var currencyCode = doc.CreateElement("cbc", "CurrencyCode", xmlnscbc.Value);
                    currencyCode.InnerText = "TRY";
                    payeeFinancialAccount.AppendChild(currencyCode);
                    var paymentNote = doc.CreateElement("cbc", "PaymentNote", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(gidenFatura.BankaAd) && !string.IsNullOrEmpty(gidenFatura.BankaSube))
                        paymentNote.InnerText = gidenFatura.BankaAd + " - " + gidenFatura.BankaSube;
                    payeeFinancialAccount.AppendChild(paymentNote);
                    paymentMeans.AppendChild(payeeFinancialAccount);
                    root.AppendChild(paymentMeans);

                    #endregion
                }

                #region KDVlerin Belirlenmesi

                var faturaKdvListesi = new List<FaturaKdvDTO>();
                foreach (var item in gidenFaturaDetayListesi)
                {
                    var kodKdvTuruOran = item.KdvOran ?? 0;
                    if (faturaKdvListesi.All(j => j.KdvOran != kodKdvTuruOran))
                    {
                        var faturaKdv = new FaturaKdvDTO
                        {
                            KdvOran = kodKdvTuruOran,
                            KdvTutari = item.KdvTutari,
                            KdvHaricTutar = item.KdvHaricTutar
                        };
                        faturaKdvListesi.Add(faturaKdv);
                    }
                    else
                    {
                        var index = faturaKdvListesi.FindIndex(j => j.KdvOran == kodKdvTuruOran);
                        faturaKdvListesi[index].KdvTutari += item.KdvTutari;
                        faturaKdvListesi[index].KdvHaricTutar += item.KdvHaricTutar;
                    }

                    if (faturaKdvListesi.Count > 0)
                        faturaKdvListesi = faturaKdvListesi.OrderBy(j => j.KdvOran).ToList();
                }

                #endregion

                #region TaxTotal

                var sayac = 1;
                foreach (var item in faturaKdvListesi)
                {
                    // Burada vergi bilgileri yer almaktadır

                    #region TaxTotal

                    var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                    var currencyIdGeneral = doc.CreateAttribute("currencyID");
                    currencyIdGeneral.Value = "TRY";
                    var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount.RemoveAllAttributes();
                    taxAmount.Attributes.Append(currencyIdGeneral);
                    taxAmount.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                        .ToString().Replace(",", ".");
                    taxTotal.AppendChild(taxAmount);
                    var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                    var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                    taxableAmount.RemoveAllAttributes();
                    currencyIdGeneral = doc.CreateAttribute("currencyID");
                    currencyIdGeneral.Value = "TRY";
                    taxableAmount.Attributes.Append(currencyIdGeneral);
                    taxableAmount.InnerText = Decimal
                        .Round((decimal) item.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxSubTotal.AppendChild(taxableAmount);
                    var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount2.RemoveAllAttributes();
                    currencyIdGeneral = doc.CreateAttribute("currencyID");
                    currencyIdGeneral.Value = "TRY";
                    taxAmount2.Attributes.Append(currencyIdGeneral);
                    taxAmount2.InnerText = Decimal.Round((decimal) item.KdvTutari, 2, MidpointRounding.AwayFromZero)
                        .ToString().Replace(",", ".");
                    taxSubTotal.AppendChild(taxAmount2);
                    var calculationSequenceNumeric =
                        doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                    calculationSequenceNumeric.InnerText = sayac + ".0";
                    taxSubTotal.AppendChild(calculationSequenceNumeric);
                    var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                    percent.InnerText = Decimal.Round((decimal) item.KdvOran, 0, MidpointRounding.AwayFromZero)
                        .ToString();
                    taxSubTotal.AppendChild(percent);
                    var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                    if (item.KdvTutari == 0)
                    {
                        if (kodFaturaGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                        {
                            var taxExemptionReasonCode =
                                doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                            taxExemptionReasonCode.InnerText = "325";
                            var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                            taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                            taxCategory.AppendChild(taxExemptionReasonCode);
                            taxCategory.AppendChild(taxExemptionReason);
                        }
                        else
                        {
                            var taxExemptionReasonCode =
                                doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                            taxExemptionReasonCode.InnerText = "350";
                            var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                            taxExemptionReason.InnerText = "Diğerleri";
                            taxCategory.AppendChild(taxExemptionReasonCode);
                            taxCategory.AppendChild(taxExemptionReason);
                        }
                    }

                    if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                    {
                        var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                        taxExemptionReasonCode.InnerText = "701";
                        var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                        taxExemptionReason.InnerText =
                            "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
                        taxCategory.AppendChild(taxExemptionReasonCode);
                        taxCategory.AppendChild(taxExemptionReason);
                    }

                    var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                    var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    taxTypeName.InnerText = "Katma Değer Vergisi";
                    var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                    taxTypeCode.InnerText = "0015";
                    taxScheme2.AppendChild(taxTypeName);
                    taxScheme2.AppendChild(taxTypeCode);
                    taxCategory.AppendChild(taxScheme2);
                    taxSubTotal.AppendChild(taxCategory);
                    taxTotal.AppendChild(taxSubTotal);
                    root.AppendChild(taxTotal);

                    #endregion
                }

                #endregion

                // Burada vergi bilgileri yer almaktadır

                #region Açıklama Satırı (TaxTotal)

                //var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                //var currencyId = doc.CreateAttribute("currencyID");
                //currencyId.Value = "TRY";
                //var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                //taxAmount.RemoveAllAttributes();
                //taxAmount.Attributes.Append(currencyId);
                //taxAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
                //taxTotal.AppendChild(taxAmount);
                //var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                //var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                //taxableAmount.RemoveAllAttributes();
                //currencyId = doc.CreateAttribute("currencyID");
                //currencyId.Value = "TRY";
                //taxableAmount.Attributes.Append(currencyId);
                //taxableAmount.InnerText = Decimal.Round((decimal)gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
                //taxSubTotal.AppendChild(taxableAmount);
                //var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                //taxAmount2.RemoveAllAttributes();
                //currencyId = doc.CreateAttribute("currencyID");
                //currencyId.Value = "TRY";
                //taxAmount2.Attributes.Append(currencyId);
                //taxAmount2.InnerText = Decimal.Round((decimal)gidenFatura.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
                //taxSubTotal.AppendChild(taxAmount2);
                //var calculationSequenceNumeric = doc.CreateElement("cbc", "CalculationSequenceNumeric", xmlnscbc.Value);
                //calculationSequenceNumeric.InnerText = "1.0";
                //taxSubTotal.AppendChild(calculationSequenceNumeric);
                //var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                //if (gidenFatura.KdvHaricTutar != 0)
                //    percent.InnerText = Decimal.Round(((decimal)gidenFatura.KdvTutari * 100) / (decimal)gidenFatura.KdvHaricTutar, 0, MidpointRounding.AwayFromZero).ToString();
                //else
                //    percent.InnerText = "0";
                //taxSubTotal.AppendChild(percent);
                //var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                //if (gidenFatura.KdvTutari == 0)
                //{
                //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                //    taxExemptionReasonCode.InnerText = "325";
                //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                //    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                //    taxCategory.AppendChild(taxExemptionReasonCode);
                //    taxCategory.AppendChild(taxExemptionReason);
                //}
                //if (kodSatisTuruKod == SatisTur.IhracKayitli.GetHashCode())
                //{
                //    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                //    taxExemptionReasonCode.InnerText = "701";
                //    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                //    taxExemptionReason.InnerText = "3065 sayılı Katma Değer Vergisi kanununun 11/1-c maddesi kapsamında ihraç edilmek şartıyla teslim edildiğinden Katma Değer Vergisi tahsil edilmemiştir.";
                //    taxCategory.AppendChild(taxExemptionReasonCode);
                //    taxCategory.AppendChild(taxExemptionReason);
                //}
                //var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                //var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                //taxTypeName.InnerText = "Katma Değer Vergisi";
                //var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                //taxTypeCode.InnerText = "0015";
                //taxScheme2.AppendChild(taxTypeName);
                //taxScheme2.AppendChild(taxTypeCode);
                //taxCategory.AppendChild(taxScheme2);
                //taxSubTotal.AppendChild(taxCategory);
                //taxTotal.AppendChild(taxSubTotal);
                //root.AppendChild(taxTotal);

                #endregion

                // KDV, KDV Hariç Tutar ve Toplam Tutar bilgileri yer almaktadır

                #region LegalMonetaryTotal

                var legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", xmlnscac.Value);
                var lineExtensionAmount = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
                lineExtensionAmount.RemoveAllAttributes();
                var currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                lineExtensionAmount.Attributes.Append(currencyId);
                lineExtensionAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(lineExtensionAmount);
                var taxExclusiveAmount = doc.CreateElement("cbc", "TaxExclusiveAmount", xmlnscbc.Value);
                taxExclusiveAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxExclusiveAmount.Attributes.Append(currencyId);
                taxExclusiveAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(taxExclusiveAmount);
                var taxInclusiveAmount = doc.CreateElement("cbc", "TaxInclusiveAmount", xmlnscbc.Value);
                taxInclusiveAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxInclusiveAmount.Attributes.Append(currencyId);
                taxInclusiveAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(taxInclusiveAmount);
                var allowanceTotalAmount = doc.CreateElement("cbc", "AllowanceTotalAmount", xmlnscbc.Value);
                allowanceTotalAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                allowanceTotalAmount.Attributes.Append(currencyId);
                allowanceTotalAmount.InnerText = "0.00";
                legalMonetaryTotal.AppendChild(allowanceTotalAmount);
                var payableAmount = doc.CreateElement("cbc", "PayableAmount", xmlnscbc.Value);
                payableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                payableAmount.Attributes.Append(currencyId);
                payableAmount.InnerText = Decimal
                    .Round((decimal) gidenFatura.FaturaTutari, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                legalMonetaryTotal.AppendChild(payableAmount);
                root.AppendChild(legalMonetaryTotal);

                #endregion

                sayac = 1;
                // Her bir fatura edtayı için hazırlanan bölümdür
                foreach (var item in gidenFaturaDetayListesi)
                {
                    var detayBilgisi = item.BosluklariKaldir();

                    #region InvoiceLine

                    // Ölçü Birimi, KDV Hariç Tutar, Para Birimi gibi bilgiler yer almaktadır

                    #region MetaData

                    var invoiceLine = doc.CreateElement("cac", "InvoiceLine", xmlnscac.Value);
                    var invoiceLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    invoiceLineId.InnerText = sayac.ToString();
                    invoiceLine.AppendChild(invoiceLineId);
                    var quantity = doc.CreateAttribute("unitCode");
                    quantity.Value = !string.IsNullOrEmpty(detayBilgisi.GibKisaltma) ? detayBilgisi.GibKisaltma : "KGM";
                    var invoicedQuantity = doc.CreateElement("cbc", "InvoicedQuantity", xmlnscbc.Value);
                    invoicedQuantity.RemoveAllAttributes();
                    invoicedQuantity.Attributes.Append(quantity);
                    invoicedQuantity.InnerText = Decimal
                        .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    invoiceLine.AppendChild(invoicedQuantity);
                    var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
                    lineExtensionAmountDetail.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    lineExtensionAmountDetail.Attributes.Append(currencyId);
                    lineExtensionAmountDetail.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    invoiceLine.AppendChild(lineExtensionAmountDetail);

                    #endregion

                    // Varsa iskonto bilgileri yer almaktadır

                    #region AllowanceCharge

                    var allowanceCharge = doc.CreateElement("cac", "AllowanceCharge", xmlnscac.Value);
                    var chargeIndicator = doc.CreateElement("cbc", "ChargeIndicator", xmlnscbc.Value);
                    chargeIndicator.InnerText = "false";
                    allowanceCharge.AppendChild(chargeIndicator);
                    decimal amount2 = 0;
                    var toplamTutar = (decimal) detayBilgisi.KdvHaricTutar;
                    var kodIskontoTuruOran = detayBilgisi.IskontoOran ?? 0;
                    if (kodIskontoTuruOran > 0)
                    {
                        toplamTutar = Decimal.Round((decimal) detayBilgisi.Miktar * (decimal) detayBilgisi.BirimFiyat,
                            4, MidpointRounding.AwayFromZero);
                        var toplamTutar2 = Decimal.Round(toplamTutar * (100 - kodIskontoTuruOran) * (decimal) 0.01,
                            4, MidpointRounding.AwayFromZero);
                        amount2 = toplamTutar - toplamTutar2;
                    }

                    var multiplierFactorNumeric = doc.CreateElement("cbc", "MultiplierFactorNumeric", xmlnscbc.Value);
                    multiplierFactorNumeric.InnerText = Decimal
                        .Round(kodIskontoTuruOran * (decimal) 0.01, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    allowanceCharge.AppendChild(multiplierFactorNumeric);
                    var amount = doc.CreateElement("cbc", "Amount", xmlnscbc.Value);
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    amount.RemoveAllAttributes();
                    amount.Attributes.Append(currencyId);
                    amount.InnerText = Decimal.Round(amount2, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    allowanceCharge.AppendChild(amount);
                    var baseAmount = doc.CreateElement("cbc", "BaseAmount", xmlnscbc.Value);
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    baseAmount.RemoveAllAttributes();
                    baseAmount.Attributes.Append(currencyId);
                    baseAmount.InnerText = Decimal.Round(toplamTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    allowanceCharge.AppendChild(baseAmount);
                    invoiceLine.AppendChild(allowanceCharge);

                    #endregion

                    // Vergi bilgileri yer almaktadır

                    #region TaxTotal

                    var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount.RemoveAllAttributes();
                    taxAmount.Attributes.Append(currencyId);
                    taxAmount.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxTotal.AppendChild(taxAmount);
                    var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                    var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                    taxableAmount.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    taxableAmount.Attributes.Append(currencyId);
                    taxableAmount.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvHaricTutar, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxSubTotal.AppendChild(taxableAmount);
                    var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                    taxAmount2.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    taxAmount2.Attributes.Append(currencyId);
                    taxAmount2.InnerText = Decimal
                        .Round((decimal) detayBilgisi.KdvTutari, 2, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".");
                    taxSubTotal.AppendChild(taxAmount2);
                    var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                    if (detayBilgisi.KdvOran != null)
                        percent.InnerText = Decimal
                            .Round((decimal) detayBilgisi.KdvOran, 2, MidpointRounding.AwayFromZero).ToString("N1")
                            .Replace(",", ".");
                    taxSubTotal.AppendChild(percent);
                    var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                    var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                    var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    taxTypeName.InnerText = "KDV";
                    taxScheme2.AppendChild(taxTypeName);
                    var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                    taxTypeCode.InnerText = "0015";
                    taxScheme2.AppendChild(taxTypeCode);
                    taxCategory.AppendChild(taxScheme2);
                    taxSubTotal.AppendChild(taxCategory);
                    taxTotal.AppendChild(taxSubTotal);
                    invoiceLine.AppendChild(taxTotal);

                    #endregion

                    // Fatura kesilen ürün detayı ve açıklama bilgileri yer almaktadır

                    #region Item

                    var invoiceItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
                    if (!string.IsNullOrEmpty(detayBilgisi.MalzemeFaturaAciklamasi))
                    {
                        var description = doc.CreateElement("cbc", "Description", xmlnscbc.Value);
                        description.InnerText = detayBilgisi.MalzemeFaturaAciklamasi;
                        invoiceItem.AppendChild(description);
                    }

                    var itemName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(detayBilgisi.FaturaUrunTuru))
                        itemName.InnerText = detayBilgisi.FaturaUrunTuru;
                    invoiceItem.AppendChild(itemName);
                    invoiceLine.AppendChild(invoiceItem);

                    #endregion

                    // Birim fiyat bilgileri yer almaktadır

                    #region Price

                    var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
                    var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
                    priceAmount.RemoveAllAttributes();
                    currencyId = doc.CreateAttribute("currencyID");
                    currencyId.Value = "TRY";
                    priceAmount.Attributes.Append(currencyId);
                    priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                        ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                            .Replace(",", ".")
                        : "0.00";
                    price.AppendChild(priceAmount);
                    invoiceLine.AppendChild(price);

                    #endregion

                    root.AppendChild(invoiceLine);

                    #endregion

                    sayac++;
                }

                #endregion

                doc.AppendChild(root);

                #region Dosya Kaydı

                var fileName = aktarilacakKlasorAdi + "/" + gidenFatura.GidenFaturaId + ".xml";
                doc.Save(fileName);

                #endregion

                return fileName;

                #endregion
            }
        }

        /// <summary>
        /// Müstahsil Makbuzu Bilgisi Üzerinden E-Müstahsil Sistemine Göndermek İçin
        /// XML Dosyası Oluşturmaya yarayan metottur
        /// </summary>
        /// <param name="mustahsilMakbuzu">Müstahsil Makbuzu Bilgisi</param>
        /// <param name="mustahsilMakbuzuDetayListesi">Müstahsil Makbuzu Detay Bilgisi</param>
        /// <param name="aktarilacakKlasorAdi">Oluşturulan Dosyanın Aktarılacağı Klasör</param>
        public static string EMustahsilXMLOlustur(MustahsilMakbuzuDTO mustahsilMakbuzu,
            List<MustahsilMakbuzuDetayDTO> mustahsilMakbuzuDetayListesi, string aktarilacakKlasorAdi)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(declaration, root);
            doc.AppendChild(
                doc.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"general.xslt\""));

            root = doc.CreateElement("CreditNote");

            mustahsilMakbuzu = mustahsilMakbuzu.BosluklariKaldir();

            var kodMakbuzGrupTuruKod = 0;
            if (mustahsilMakbuzu.MakbuzGrupTuruKod != null)
                kodMakbuzGrupTuruKod = mustahsilMakbuzu.MakbuzGrupTuruKod ?? 0;

            #region Standart ve Müstahsil Makbuzuna Bağlı Bilgiler

            // Makbuzun türü, kesilme tarihi, UBL Versiyon Numarası, GİB Numarası gibi bilgiler
            // yer almaktadır

            #region Standart Bilgiler

            root.RemoveAllAttributes();
            var locationAttribute = "xsi:schemaLocation";
            var location = doc.CreateAttribute("xsi", "schemaLocation",
                "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
            location.Value =
                "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2 ../xsdrt/maindoc/UBL-CreditNote-2.1.xsd";
            var xmlns = doc.CreateAttribute("xmlns");
            xmlns.Value = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
            var xmlnsn4 = doc.CreateAttribute("xmlns:n4");
            xmlnsn4.Value = "http://www.altova.com/samplexml/other-namespace";
            var xmlnsxsi = doc.CreateAttribute("xmlns:xsi");
            xmlnsxsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            var xmlnscac = doc.CreateAttribute("xmlns:cac");
            xmlnscac.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var xmlnscbc = doc.CreateAttribute("xmlns:cbc");
            xmlnscbc.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var xmlnsext = doc.CreateAttribute("xmlns:ext");
            xmlnsext.Value = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
            root.Attributes.Append(location);
            root.Attributes.Append(xmlns);
            root.Attributes.Append(xmlnsn4);
            root.Attributes.Append(xmlnsxsi);
            root.Attributes.Append(xmlnscac);
            root.Attributes.Append(xmlnscbc);
            root.Attributes.Append(xmlnsext);
            var extUbl = doc.CreateElement("ext", "UBLExtensions", xmlnsext.Value);
            var extUbl2 = doc.CreateElement("ext", "UBLExtension", xmlnsext.Value);
            var extUbl3 = doc.CreateElement("ext", "ExtensionContent", xmlnsext.Value);
            var extUbl4 = doc.CreateElement("n4", "auto-generated_for_wildcard", xmlnsn4.Value);
            extUbl3.AppendChild(extUbl4);
            extUbl2.AppendChild(extUbl3);
            extUbl.AppendChild(extUbl2);

            root.AppendChild(extUbl);

            var versionId = doc.CreateElement("cbc", "UBLVersionID", xmlnscbc.Value);
            versionId.InnerText = "2.1";
            root.AppendChild(versionId);

            var customizationId = doc.CreateElement("cbc", "CustomizationID", xmlnscbc.Value);
            customizationId.InnerText = "TR1.2.1";
            root.AppendChild(customizationId);

            var profileId = doc.CreateElement("cbc", "ProfileID", xmlnscbc.Value);
            profileId.InnerText = "EARSIVBELGE";
            root.AppendChild(profileId);

            var id = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.MustahsilMakbuzuNo))
                id.InnerText = mustahsilMakbuzu.MustahsilMakbuzuNo;
            else
                id.InnerText = "ABC" + mustahsilMakbuzu.MustahsilMakbuzuTarihi?.Year +
                               DateTime.UtcNow.Ticks.ToString().Substring(0, 9);
            root.AppendChild(id);

            var copyIndicator = doc.CreateElement("cbc", "CopyIndicator", xmlnscbc.Value);
            copyIndicator.InnerText = "false";
            root.AppendChild(copyIndicator);

            var mustahsilMakbuzuId = doc.CreateElement("cbc", "UUID", xmlnscbc.Value);
            mustahsilMakbuzuId.InnerText = mustahsilMakbuzu.MustahsilMakbuzuId.Length == 36
                ? mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            root.AppendChild(mustahsilMakbuzuId);

            var issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = mustahsilMakbuzu.MustahsilMakbuzuTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            root.AppendChild(issueDate);

            var issueTime = doc.CreateElement("cbc", "IssueTime", xmlnscbc.Value);
            issueTime.InnerText = mustahsilMakbuzu.MustahsilMakbuzuTarihi?.ToString("HH:mm:ss") ?? string.Empty;
            root.AppendChild(issueTime);

            var creditNoteTypeCode = doc.CreateElement("cbc", "CreditNoteTypeCode", xmlnscbc.Value);
            creditNoteTypeCode.InnerText = "MUSTAHSILMAKBUZ";
            root.AppendChild(creditNoteTypeCode);

            var sendType = doc.CreateElement("cbc", "Note", xmlnscbc.Value);
            sendType.InnerText = "Gönderim Şekli: ELEKTRONIK";
            root.AppendChild(sendType);

            var documentCurrencyCode = doc.CreateElement("cbc", "DocumentCurrencyCode", xmlnscbc.Value);
            documentCurrencyCode.InnerText = "TRY";
            root.AppendChild(documentCurrencyCode);

            var lineCountNumeric = doc.CreateElement("cbc", "LineCountNumeric", xmlnscbc.Value);
            lineCountNumeric.InnerText = mustahsilMakbuzuDetayListesi.Count.ToString();
            root.AppendChild(lineCountNumeric);

            #region AdditionalDocumentReference

            var additionalDocumentReference = doc.CreateElement("cac", "AdditionalDocumentReference", xmlnscac.Value);
            var additionalDocumentReferenceId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            additionalDocumentReferenceId.InnerText = mustahsilMakbuzu.MustahsilMakbuzuId.Length == 36
                ? mustahsilMakbuzu.MustahsilMakbuzuId.ToUpper()
                : Guid.NewGuid().ToString().ToUpper();
            additionalDocumentReference.AppendChild(additionalDocumentReferenceId);
            issueDate = doc.CreateElement("cbc", "IssueDate", xmlnscbc.Value);
            issueDate.InnerText = mustahsilMakbuzu.MustahsilMakbuzuTarihi?.Date.ToString("yyyy-MM-dd") ?? string.Empty;
            additionalDocumentReference.AppendChild(issueDate);
            var documentType = doc.CreateElement("cbc", "DocumentType", xmlnscbc.Value);
            documentType.InnerText = "XSLT";
            additionalDocumentReference.AppendChild(documentType);
            var attachment = doc.CreateElement("cac", "Attachment", xmlnscac.Value);
            var embeddedDocumentBinaryObject = doc.CreateElement("cbc", "EmbeddedDocumentBinaryObject", xmlnscbc.Value);
            var characterSetCode = doc.CreateAttribute("characterSetCode");
            characterSetCode.Value = "UTF-8";
            var encodingCode = doc.CreateAttribute("encodingCode");
            encodingCode.Value = "Base64";
            var mimeCode = doc.CreateAttribute("mimeCode");
            mimeCode.Value = "application/xml";
            embeddedDocumentBinaryObject.Attributes.Append(characterSetCode);
            embeddedDocumentBinaryObject.Attributes.Append(encodingCode);
            embeddedDocumentBinaryObject.Attributes.Append(mimeCode);
            attachment.AppendChild(embeddedDocumentBinaryObject);
            additionalDocumentReference.AppendChild(attachment);

            root.AppendChild(additionalDocumentReference);

            #endregion

            #endregion

            // Faturaya ilişkin imza, adres vb. bilgiler yer almaktadır

            #region Signature

            var signature = doc.CreateElement("cac", "Signature", xmlnscac.Value);
            var signatureIdAttr = doc.CreateAttribute("schemeID");
            signatureIdAttr.Value = "VKN_TCKN";
            var signatureId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            signatureId.Attributes.Append(signatureIdAttr);
            signatureId.InnerText = "3250566851";
            signature.AppendChild(signatureId);
            var signatoryParty = doc.CreateElement("cac", "SignatoryParty", xmlnscac.Value);
            var partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var signatureIdAttr2 = doc.CreateAttribute("schemeID");
            signatureIdAttr2.Value = "VKN";
            var signaturePartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            signaturePartyId.Attributes.Append(signatureIdAttr2);
            signaturePartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(signaturePartyId);
            signatoryParty.AppendChild(partyIdentification);

            #region Postal Address

            var postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            var buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            var citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            var cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            var postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            var country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            var countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            signatoryParty.AppendChild(postalAddress);
            signature.AppendChild(signatoryParty);

            #endregion

            var digitalSignatureAttachment = doc.CreateElement("cac", "DigitalSignatureAttachment", xmlnscac.Value);
            var externalReference = doc.CreateElement("cac", "ExternalReference", xmlnscac.Value);
            var uri = doc.CreateElement("cbc", "URI", xmlnscbc.Value);
            uri.InnerText = "#Signature";
            externalReference.AppendChild(uri);
            digitalSignatureAttachment.AppendChild(externalReference);
            signature.AppendChild(digitalSignatureAttachment);

            root.AppendChild(signature);

            #endregion

            // Burada sabit değerler verilmekte olup, Makbuzu kesen kuruma veya firmaya ait bilgiler
            // yer almaktadır. Bu kısımda örnek olarak TÜRKŞEKER Fabrikaları Genel Müdürlüğü bilgileri
            // kullanılmıştır. Ancak değiştirip test edilebilir.

            #region AccountingSupplierParty

            var accountingSupplierParty = doc.CreateElement("cac", "AccountingSupplierParty", xmlnscac.Value);
            var party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            var webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            webSiteUri.InnerText = "https://www.turkseker.gov.tr";
            party.AppendChild(webSiteUri);
            var accountingSupplierPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr.Value = "VKN_TCKN";
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
            accountingSupplierPartyIdAttr2.Value = "VKN";
            var accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
            accountingSupplierPartyPartyId.InnerText = "3250566851";
            partyIdentification.AppendChild(accountingSupplierPartyPartyId);
            party.AppendChild(partyIdentification);

            if (!string.IsNullOrEmpty(mustahsilMakbuzu.AltBirimAd))
            {
                partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                accountingSupplierPartyIdAttr2 = doc.CreateAttribute("schemeID");
                accountingSupplierPartyIdAttr2.Value = "SUBENO";
                accountingSupplierPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                accountingSupplierPartyPartyId.Attributes.Append(accountingSupplierPartyIdAttr2);
                accountingSupplierPartyPartyId.InnerText = mustahsilMakbuzu.AltBirimAd;
                partyIdentification.AppendChild(accountingSupplierPartyPartyId);
                party.AppendChild(partyIdentification);
            }

            var partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
            var partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            partyNameReal.InnerText = "TÜRKİYE ŞEKER FABRİKALARI A.Ş.";
            partyName.AppendChild(partyNameReal);
            party.AppendChild(partyName);

            #region Postal Address

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            var postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = "3250566851";
            postalAddress.AppendChild(postalAddressId);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            streetName.InnerText = "Mithatpaşa Caddesi";
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            buildingNumber.InnerText = "14";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            citySubdivisionName.InnerText = "Çankaya";
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            cityName.InnerText = "Ankara";
            postalAddress.AppendChild(cityName);
            postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            postalZone.InnerText = "06100";
            postalAddress.AppendChild(postalZone);
            country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            var partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            var taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxSchemeName.InnerText = "Ankara Kurumlar";
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            var contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            var telephone = doc.CreateElement("cbc", "Telephone", xmlnscbc.Value);
            telephone.InnerText = "(312) 4585500";
            contact.AppendChild(telephone);
            var telefax = doc.CreateElement("cbc", "Telefax", xmlnscbc.Value);
            telefax.InnerText = "(312) 4585800";
            contact.AppendChild(telefax);
            var electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            electronicMail.InnerText = "maliisler@turkseker.gov.tr";
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            accountingSupplierParty.AppendChild(party);

            root.AppendChild(accountingSupplierParty);

            #endregion

            // Bu kısımda makbuz kesilen firma bilgileri yer almaktadır

            #region AccountingCustomerParty

            var accountingCustomerParty = doc.CreateElement("cac", "AccountingCustomerParty", xmlnscac.Value);
            party = doc.CreateElement("cac", "Party", xmlnscac.Value);
            webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
            party.AppendChild(webSiteUri);
            var accountingCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
            accountingCustomerPartyIdAttr.Value = "TCKN";
            if (mustahsilMakbuzu.VergiNo.Length == 10)
                accountingCustomerPartyIdAttr.Value = "VKN";
            partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
            var accountingCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            accountingCustomerPartyPartyId.Attributes.Append(accountingCustomerPartyIdAttr);
            accountingCustomerPartyPartyId.InnerText = mustahsilMakbuzu.VergiNo;
            partyIdentification.AppendChild(accountingCustomerPartyPartyId);
            party.AppendChild(partyIdentification);

            if (mustahsilMakbuzu.VergiNo.Length == 10)
            {
                partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                partyNameReal.InnerText = mustahsilMakbuzu.TuzelKisiAd;
                partyName.AppendChild(partyNameReal);
                party.AppendChild(partyName);
            }

            #region Postal Address

            postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
            postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
            postalAddressId.InnerText = mustahsilMakbuzu.VergiNo;
            postalAddress.AppendChild(postalAddressId);
            var room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
            postalAddress.AppendChild(room);
            streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.Adres))
                streetName.InnerText = mustahsilMakbuzu.Adres;
            postalAddress.AppendChild(streetName);
            buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
            //buildingNumber.InnerText = "";
            postalAddress.AppendChild(buildingNumber);
            citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlceAd))
                citySubdivisionName.InnerText = mustahsilMakbuzu.IlceAd;
            postalAddress.AppendChild(citySubdivisionName);
            cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlAd))
                cityName.InnerText = mustahsilMakbuzu.IlAd;
            postalAddress.AppendChild(cityName);
            postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
            //postalZone.InnerText = "";
            postalAddress.AppendChild(postalZone);
            country = doc.CreateElement("cac", "Country", xmlnscac.Value);
            countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            countryName.InnerText = "Türkiye";
            country.AppendChild(countryName);
            postalAddress.AppendChild(country);
            party.AppendChild(postalAddress);

            #endregion

            partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
            taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiDairesi))
                taxSchemeName.InnerText = mustahsilMakbuzu.VergiDairesi;
            taxScheme.AppendChild(taxSchemeName);
            partyTaxScheme.AppendChild(taxScheme);
            party.AppendChild(partyTaxScheme);

            contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
            electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
            if (!string.IsNullOrEmpty(mustahsilMakbuzu.EPostaAdresi))
                electronicMail.InnerText = mustahsilMakbuzu.EPostaAdresi;
            contact.AppendChild(electronicMail);
            party.AppendChild(contact);

            if (mustahsilMakbuzu.VergiNo.Length == 11)
            {
                var liste = mustahsilMakbuzu.TuzelKisiAd.Split(' ').ToList();
                if (liste.Count > 1)
                {
                    var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                    var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                    firstName.InnerText = liste[0];
                    person.AppendChild(firstName);
                    var surname = mustahsilMakbuzu.TuzelKisiAd.Substring(liste[0].Length + 1);
                    var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                    familyName.InnerText = surname;
                    //familyName.InnerText = liste[1];
                    person.AppendChild(familyName);
                    party.AppendChild(person);
                }
            }

            accountingCustomerParty.AppendChild(party);

            root.AppendChild(accountingCustomerParty);

            #endregion

            if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiNo))
            {
                if (mustahsilMakbuzu.VergiNo.Length == 10)
                {
                    // Bu kısımda makbuz kesilen firma bilgileri yer almaktadır (Eğer sadece Tüzel Kişilikse, şahıs şirketleri için bu yapılmamaktadır)

                    #region BuyerCustomerParty

                    var buyerCustomerParty = doc.CreateElement("cac", "BuyerCustomerParty", xmlnscac.Value);
                    party = doc.CreateElement("cac", "Party", xmlnscac.Value);
                    webSiteUri = doc.CreateElement("cbc", "WebsiteURI", xmlnscbc.Value);
                    party.AppendChild(webSiteUri);
                    var buyerCustomerPartyIdAttr = doc.CreateAttribute("schemeID");
                    buyerCustomerPartyIdAttr.Value = "VKN";
                    partyIdentification = doc.CreateElement("cac", "PartyIdentification", xmlnscac.Value);
                    var buyerCustomerPartyPartyId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    buyerCustomerPartyPartyId.Attributes.Append(buyerCustomerPartyIdAttr);
                    buyerCustomerPartyPartyId.InnerText = mustahsilMakbuzu.VergiNo;
                    partyIdentification.AppendChild(buyerCustomerPartyPartyId);
                    party.AppendChild(partyIdentification);

                    if (mustahsilMakbuzu.VergiNo.Length == 10)
                    {
                        partyName = doc.CreateElement("cac", "PartyName", xmlnscac.Value);
                        partyNameReal = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                        partyNameReal.InnerText = mustahsilMakbuzu.TuzelKisiAd;
                        partyName.AppendChild(partyNameReal);
                        party.AppendChild(partyName);
                    }

                    #region Postal Address

                    postalAddress = doc.CreateElement("cac", "PostalAddress", xmlnscac.Value);
                    postalAddressId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                    postalAddressId.InnerText = mustahsilMakbuzu.VergiNo;
                    postalAddress.AppendChild(postalAddressId);
                    room = doc.CreateElement("cbc", "Room", xmlnscbc.Value);
                    postalAddress.AppendChild(room);
                    streetName = doc.CreateElement("cbc", "StreetName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.Adres))
                        streetName.InnerText = mustahsilMakbuzu.Adres;
                    postalAddress.AppendChild(streetName);
                    buildingNumber = doc.CreateElement("cbc", "BuildingNumber", xmlnscbc.Value);
                    //buildingNumber.InnerText = "";
                    postalAddress.AppendChild(buildingNumber);
                    citySubdivisionName = doc.CreateElement("cbc", "CitySubdivisionName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlceAd))
                        citySubdivisionName.InnerText = mustahsilMakbuzu.IlceAd;
                    postalAddress.AppendChild(citySubdivisionName);
                    cityName = doc.CreateElement("cbc", "CityName", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.IlAd))
                        cityName.InnerText = mustahsilMakbuzu.IlAd;
                    postalAddress.AppendChild(cityName);
                    postalZone = doc.CreateElement("cbc", "PostalZone", xmlnscbc.Value);
                    //postalZone.InnerText = "";
                    postalAddress.AppendChild(postalZone);
                    country = doc.CreateElement("cac", "Country", xmlnscac.Value);
                    countryName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    countryName.InnerText = "Türkiye";
                    country.AppendChild(countryName);
                    postalAddress.AppendChild(country);
                    party.AppendChild(postalAddress);

                    #endregion

                    partyTaxScheme = doc.CreateElement("cac", "PartyTaxScheme", xmlnscac.Value);
                    taxScheme = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                    taxSchemeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.VergiDairesi))
                        taxSchemeName.InnerText = mustahsilMakbuzu.VergiDairesi;
                    taxScheme.AppendChild(taxSchemeName);
                    partyTaxScheme.AppendChild(taxScheme);
                    party.AppendChild(partyTaxScheme);

                    contact = doc.CreateElement("cac", "Contact", xmlnscac.Value);
                    electronicMail = doc.CreateElement("cbc", "ElectronicMail", xmlnscbc.Value);
                    if (!string.IsNullOrEmpty(mustahsilMakbuzu.EPostaAdresi))
                        electronicMail.InnerText = mustahsilMakbuzu.EPostaAdresi;
                    contact.AppendChild(electronicMail);
                    party.AppendChild(contact);

                    if (mustahsilMakbuzu.VergiNo.Length == 11)
                    {
                        var liste = mustahsilMakbuzu.TuzelKisiAd.Split(' ').ToList();
                        if (liste.Count > 1)
                        {
                            var person = doc.CreateElement("cac", "Person", xmlnscac.Value);
                            var firstName = doc.CreateElement("cbc", "FirstName", xmlnscbc.Value);
                            firstName.InnerText = liste[0];
                            person.AppendChild(firstName);
                            var surname = mustahsilMakbuzu.TuzelKisiAd.Substring(liste[0].Length + 1);
                            var familyName = doc.CreateElement("cbc", "FamilyName", xmlnscbc.Value);
                            familyName.InnerText = surname;
                            //familyName.InnerText = liste[1];
                            person.AppendChild(familyName);
                            party.AppendChild(person);
                        }
                    }

                    buyerCustomerParty.AppendChild(party);

                    root.AppendChild(buyerCustomerParty);

                    #endregion
                }
            }

            // Burada vergi bilgileri yer almaktadır

            #region TaxTotal

            var taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
            var currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            var taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount.RemoveAllAttributes();
            taxAmount.Attributes.Append(currencyId);
            taxAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxTotal.AppendChild(taxAmount);
            var taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
            var taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
            taxableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxableAmount.Attributes.Append(currencyId);
            taxableAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxableAmount);
            var taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
            taxAmount2.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxAmount2.Attributes.Append(currencyId);
            taxAmount2.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            taxSubTotal.AppendChild(taxAmount2);
            var percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
            percent.InnerText = mustahsilMakbuzu.NetTutar != 0
                ? Decimal.Round(((decimal) mustahsilMakbuzu.GelirVergisi * 100) / (decimal) mustahsilMakbuzu.NetTutar,
                    0, MidpointRounding.AwayFromZero).ToString()
                : "0";
            taxSubTotal.AppendChild(percent);
            var taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
            if (mustahsilMakbuzu.GelirVergisi == 0)
            {
                if (kodMakbuzGrupTuruKod == FaturaMakbuzGrupTur.Kuspe.GetHashCode())
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "325";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText = "13/ı Yem Teslimleri";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }
                else
                {
                    var taxExemptionReasonCode = doc.CreateElement("cbc", "TaxExemptionReasonCode", xmlnscbc.Value);
                    taxExemptionReasonCode.InnerText = "350";
                    var taxExemptionReason = doc.CreateElement("cbc", "TaxExemptionReason", xmlnscbc.Value);
                    taxExemptionReason.InnerText = "Diğerleri";
                    taxCategory.AppendChild(taxExemptionReasonCode);
                    taxCategory.AppendChild(taxExemptionReason);
                }
            }

            var taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
            var taxName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
            taxName.InnerText = "GV STOPAJI";
            taxScheme2.AppendChild(taxName);
            var taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
            taxTypeCode.InnerText = "0003";
            taxScheme2.AppendChild(taxTypeCode);
            taxCategory.AppendChild(taxScheme2);
            taxSubTotal.AppendChild(taxCategory);
            taxTotal.AppendChild(taxSubTotal);
            root.AppendChild(taxTotal);

            #endregion

            // Gelir Vergisi, Net Tutar ve Toplam Tutar bilgileri yer almaktadır

            #region LegalMonetaryTotal

            var legalMonetaryTotal = doc.CreateElement("cac", "LegalMonetaryTotal", xmlnscac.Value);
            var lineExtensionAmount = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
            lineExtensionAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            lineExtensionAmount.Attributes.Append(currencyId);
            lineExtensionAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(lineExtensionAmount);
            var taxExclusiveAmount = doc.CreateElement("cbc", "TaxExclusiveAmount", xmlnscbc.Value);
            taxExclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxExclusiveAmount.Attributes.Append(currencyId);
            taxExclusiveAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxExclusiveAmount);
            var taxInclusiveAmount = doc.CreateElement("cbc", "TaxInclusiveAmount", xmlnscbc.Value);
            taxInclusiveAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            taxInclusiveAmount.Attributes.Append(currencyId);
            taxInclusiveAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                .Replace(",", ".");
            legalMonetaryTotal.AppendChild(taxInclusiveAmount);
            var payableAmount = doc.CreateElement("cbc", "PayableAmount", xmlnscbc.Value);
            payableAmount.RemoveAllAttributes();
            currencyId = doc.CreateAttribute("currencyID");
            currencyId.Value = "TRY";
            payableAmount.Attributes.Append(currencyId);
            payableAmount.InnerText = Decimal
                .Round((decimal) mustahsilMakbuzu.NetTutar - (decimal) mustahsilMakbuzu.GelirVergisi, 2,
                    MidpointRounding.AwayFromZero).ToString().Replace(",", ".");
            legalMonetaryTotal.AppendChild(payableAmount);
            root.AppendChild(legalMonetaryTotal);

            #endregion

            var sayac = 1;
            // Her bir makbuz detayı için hazırlanan bölümdür
            foreach (var item in mustahsilMakbuzuDetayListesi)
            {
                var detayBilgisi = item.BosluklariKaldir();

                #region CreditNoteLine

                #region MetaData

                var creditNoteLine = doc.CreateElement("cac", "CreditNoteLine", xmlnscac.Value);
                var creditNoteLineId = doc.CreateElement("cbc", "ID", xmlnscbc.Value);
                creditNoteLineId.InnerText = sayac.ToString();
                creditNoteLine.AppendChild(creditNoteLineId);
                var quantity = doc.CreateAttribute("unitCode");
                quantity.Value = "KGM";
                var creditedQuantity = doc.CreateElement("cbc", "CreditedQuantity", xmlnscbc.Value);
                creditedQuantity.RemoveAllAttributes();
                creditedQuantity.Attributes.Append(quantity);
                creditedQuantity.InnerText = Decimal
                    .Round((decimal) detayBilgisi.Miktar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                creditNoteLine.AppendChild(creditedQuantity);
                var lineExtensionAmountDetail = doc.CreateElement("cbc", "LineExtensionAmount", xmlnscbc.Value);
                lineExtensionAmountDetail.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                lineExtensionAmountDetail.Attributes.Append(currencyId);
                lineExtensionAmountDetail.InnerText = Decimal
                    .Round((decimal) detayBilgisi.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                creditNoteLine.AppendChild(lineExtensionAmountDetail);

                #endregion

                #region TaxTotal

                taxTotal = doc.CreateElement("cac", "TaxTotal", xmlnscac.Value);
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount.RemoveAllAttributes();
                taxAmount.Attributes.Append(currencyId);
                taxAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxTotal.AppendChild(taxAmount);
                taxSubTotal = doc.CreateElement("cac", "TaxSubtotal", xmlnscac.Value);
                taxableAmount = doc.CreateElement("cbc", "TaxableAmount", xmlnscbc.Value);
                taxableAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxableAmount.Attributes.Append(currencyId);
                taxableAmount.InnerText = Decimal
                    .Round((decimal) detayBilgisi.NetTutar, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxableAmount);
                taxAmount2 = doc.CreateElement("cbc", "TaxAmount", xmlnscbc.Value);
                taxAmount2.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                taxAmount2.Attributes.Append(currencyId);
                taxAmount2.InnerText = Decimal
                    .Round((decimal) detayBilgisi.GelirVergisi, 2, MidpointRounding.AwayFromZero).ToString()
                    .Replace(",", ".");
                taxSubTotal.AppendChild(taxAmount2);
                percent = doc.CreateElement("cbc", "Percent", xmlnscbc.Value);
                percent.InnerText = "2";
                taxSubTotal.AppendChild(percent);
                taxCategory = doc.CreateElement("cac", "TaxCategory", xmlnscac.Value);
                taxScheme2 = doc.CreateElement("cac", "TaxScheme", xmlnscac.Value);
                var taxTypeName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                taxTypeName.InnerText = "GV STOPAJI";
                taxScheme2.AppendChild(taxTypeName);
                taxTypeCode = doc.CreateElement("cbc", "TaxTypeCode", xmlnscbc.Value);
                taxTypeCode.InnerText = "0003";
                taxScheme2.AppendChild(taxTypeCode);
                taxCategory.AppendChild(taxScheme2);
                taxSubTotal.AppendChild(taxCategory);
                taxTotal.AppendChild(taxSubTotal);
                creditNoteLine.AppendChild(taxTotal);

                #endregion

                #region Item

                var creditNoteItem = doc.CreateElement("cac", "Item", xmlnscac.Value);
                var itemName = doc.CreateElement("cbc", "Name", xmlnscbc.Value);
                itemName.InnerText = detayBilgisi.IsinMahiyeti;
                creditNoteItem.AppendChild(itemName);
                creditNoteLine.AppendChild(creditNoteItem);

                #endregion

                #region Price

                var price = doc.CreateElement("cac", "Price", xmlnscac.Value);
                var priceAmount = doc.CreateElement("cbc", "PriceAmount", xmlnscbc.Value);
                priceAmount.RemoveAllAttributes();
                currencyId = doc.CreateAttribute("currencyID");
                currencyId.Value = "TRY";
                priceAmount.Attributes.Append(currencyId);
                priceAmount.InnerText = detayBilgisi.BirimFiyat != null
                    ? Decimal.Round((decimal) detayBilgisi.BirimFiyat, 4, MidpointRounding.AwayFromZero).ToString()
                        .Replace(",", ".")
                    : "0.00";
                price.AppendChild(priceAmount);
                creditNoteLine.AppendChild(price);

                #endregion

                root.AppendChild(creditNoteLine);

                #endregion

                sayac++;
            }

            #endregion

            doc.AppendChild(root);

            #region Dosya Kaydı

            var fileName = aktarilacakKlasorAdi + "/" + mustahsilMakbuzu.MustahsilMakbuzuId + ".xml";
            doc.Save(fileName);

            #endregion

            return fileName;
        }
    }
}
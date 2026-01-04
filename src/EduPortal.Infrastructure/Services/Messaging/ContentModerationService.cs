using EduPortal.Application.DTOs.Messaging;
using EduPortal.Application.Interfaces.Messaging;
using System.Text.RegularExpressions;

namespace EduPortal.Infrastructure.Services.Messaging;

/// <summary>
/// Icerik moderasyon servisi - coklu dil kufur ve telefon numarasi filtresi
/// Desteklenen diller: Turkce, Ingilizce, Almanca, Fransizca, Ispanyolca, Arapca, Rusca
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private readonly HashSet<string> _profanityList = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _whitelistedWords = new(StringComparer.OrdinalIgnoreCase);

    // Turkce'de kisa kufurler (am, got, sik vb.) - bunlar diger dillerde normal kelime olabilir
    // Bu kelimeler sadece Turkce baglamda engellenecek
    private static readonly HashSet<string> TurkishShortProfanities = new(StringComparer.OrdinalIgnoreCase)
    {
        "am", "got", "sik", "pic", "pic", "dol", "mk", "aq", "mq", "oc"
    };

    // Turkce yaygin kelimeler - Turkce baglam tespiti icin
    private static readonly HashSet<string> TurkishCommonWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Zamirler ve ekler
        "ben", "sen", "biz", "siz", "onlar", "bu", "su", "o", "bunlar", "sunlar",
        // Fiiller
        "var", "yok", "evet", "hayir", "tamam", "olmak", "etmek", "yapmak", "gelmek", "gitmek",
        "almak", "vermek", "demek", "bilmek", "istemek", "bakmak", "bulmak", "kalmak",
        // Baglaçlar
        "ve", "ile", "ama", "fakat", "ancak", "veya", "ya", "de", "da", "ki", "icin", "için",
        // Sifatlar
        "iyi", "kotu", "buyuk", "kucuk", "yeni", "eski", "guzel", "cok", "az", "hep",
        // Zaman
        "simdi", "sonra", "once", "bugun", "yarin", "dun", "hala", "artik",
        // Sorular
        "ne", "nasil", "neden", "nicin", "nerede", "kim", "hangi", "kac",
        // Okul terimleri
        "ogrenci", "ogretmen", "okul", "ders", "sinif", "odev", "veli", "not", "sinav"
    };

    // Turkce karakterler - Turkce baglam tespiti icin
    private static readonly char[] TurkishSpecificChars = { 'ı', 'ğ', 'ü', 'ş', 'ö', 'ç', 'İ', 'Ğ', 'Ü', 'Ş', 'Ö', 'Ç' };

    // Telefon numarasi regex'leri
    private static readonly Regex[] PhonePatterns = new[]
    {
        // Turkiye: +90 5XX XXX XX XX
        new Regex(@"(\+90|0090)[\s\-]?5\d{2}[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}", RegexOptions.Compiled),
        new Regex(@"0[\s\-]?5\d{2}[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}", RegexOptions.Compiled),
        new Regex(@"\b5\d{2}[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}\b", RegexOptions.Compiled),
        new Regex(@"\b5\d{9}\b", RegexOptions.Compiled),
        // Genel uluslararasi format
        new Regex(@"(\+?\d{1,3}[\s\-]?)?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}", RegexOptions.Compiled)
    };

    // E-posta regex
    private static readonly Regex EmailPattern = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ContentModerationService()
    {
        InitializeMultiLanguageProfanityList();
    }

    private void InitializeMultiLanguageProfanityList()
    {
        // ==================== TURKCE ====================
        var turkishProfanities = new[]
        {
            // Temel kufurler
            "amk", "aq", "mk", "mq", "oç", "oc", "orospu", "piç", "pic", "sik", "yarrak", "yarak",
            "göt", "got", "am", "meme", "taşak", "tasak", "döl", "dol", "kaltak", "fahişe", "fahise",
            "ibne", "pezevenk", "gavat", "kahpe", "şerefsiz", "serefsiz", "hıyar", "hiyar",
            // Fiil halleri
            "amına", "amina", "sikeyim", "sikerim", "siktir", "ananı", "anani", "ananızı",
            "orosbuçocuğu", "orosbu", "orosbucocugu", "piçkurusu",
            // Leetspeak varyasyonlari
            "s1k", "s!k", "y4rr4k", "0ç", "0rospu", "p1ç", "@mk", "@q", "p!ç", "s!kt!r",
            "s1key1m", "s1kt1r", "0r0spu"
        };

        // ==================== INGILIZCE ====================
        var englishProfanities = new[]
        {
            // Temel kufurler
            "fuck", "shit", "bitch", "asshole", "bastard", "damn", "crap", "dick", "cock",
            "pussy", "cunt", "whore", "slut", "fag", "faggot", "nigger", "nigga",
            // Fiil halleri
            "fucking", "fucked", "fucker", "motherfucker", "bullshit", "bitching",
            "shitty", "dickhead", "cocksucker", "asswipe",
            // Kisaltmalar ve varyasyonlar
            "wtf", "stfu", "gtfo", "lmfao", "af",
            // Leetspeak
            "f*ck", "sh*t", "b*tch", "a$$", "fck", "sht", "btch"
        };

        // ==================== ALMANCA ====================
        var germanProfanities = new[]
        {
            // Temel kufurler
            "scheiße", "scheisse", "scheiss", "arschloch", "arsch", "wichser", "hurensohn",
            "fotze", "schwanz", "schwuchtel", "missgeburt", "spast", "behindert",
            "vollidiot", "dummkopf", "blödmann", "blodmann", "depp", "trottel",
            "hure", "nutte", "schlampe", "wixer", "pisser", "drecksau",
            // Fiil halleri
            "verfickt", "beschissen", "verpiss", "verpisst"
        };

        // ==================== FRANSIZCA ====================
        var frenchProfanities = new[]
        {
            // Temel kufurler
            "merde", "putain", "salaud", "salope", "connard", "connasse", "enculé", "encule",
            "bite", "couilles", "nique", "niquer", "baiser", "foutre", "bordel",
            "con", "conne", "pute", "batard", "bâtard", "enfoiré", "enfoire",
            // Fiil halleri
            "niquemère", "nique ta mère", "va te faire", "ferme ta gueule",
            "fils de pute", "fdp"
        };

        // ==================== ISPANYOLCA ====================
        var spanishProfanities = new[]
        {
            // Temel kufurler
            "mierda", "puta", "puto", "pendejo", "pendeja", "cabron", "cabrón", "coño", "cono",
            "verga", "chingar", "chingada", "joder", "jodido", "culo", "marica", "maricon", "maricón",
            "gilipollas", "hostia", "cojones", "carajo", "hijoputa", "hijo de puta",
            // Fiil halleri
            "chingado", "jodete", "vete a la mierda", "me cago en",
            "pinche", "culero", "mamón", "mamon"
        };

        // ==================== ARAPCA (Latin harflerle) ====================
        var arabicProfanities = new[]
        {
            // Temel kufurler (Latin transliterasyonu)
            "kuss", "kos", "sharmouta", "sharmota", "sharmuta", "kalb", "ibn el sharmouta",
            "ya ibn el", "kol khara", "telhas", "airi", "ayre", "zeb", "zebi",
            "ahbal", "hmar", "hayawan", "manyak", "manyaka", "khawal",
            "ibn kalb", "ya kalb", "ya hmar", "khara", "toz"
        };

        // ==================== RUSCA (Latin harflerle) ====================
        var russianProfanities = new[]
        {
            // Temel kufurler (Latin transliterasyonu)
            "blyad", "blyat", "suka", "hui", "pizda", "pizdec", "ebat", "ebal", "nahui", "nahuy",
            "huinya", "huynya", "mudak", "mudila", "zalupa", "gandon", "pidor", "pidoras",
            "debil", "dolboeb", "dolboyob", "yob", "yobany", "yebanat",
            // Kiril harflerle (UTF-8)
            "блядь", "сука", "хуй", "пизда", "ебать", "нахуй", "мудак", "пидор"
        };

        // Tum listeleri birlestir
        foreach (var word in turkishProfanities) _profanityList.Add(word);
        foreach (var word in englishProfanities) _profanityList.Add(word);
        foreach (var word in germanProfanities) _profanityList.Add(word);
        foreach (var word in frenchProfanities) _profanityList.Add(word);
        foreach (var word in spanishProfanities) _profanityList.Add(word);
        foreach (var word in arabicProfanities) _profanityList.Add(word);
        foreach (var word in russianProfanities) _profanityList.Add(word);
    }

    public ContentModerationResult ValidateContent(string content)
    {
        var result = new ContentModerationResult
        {
            IsValid = true,
            DetectedIssues = new List<string>()
        };

        if (string.IsNullOrWhiteSpace(content))
        {
            return result;
        }

        // Kufur kontrolu
        var profanityCheck = CheckProfanity(content);
        if (profanityCheck.found)
        {
            result.HasProfanity = true;
            result.IsValid = false;
            result.DetectedIssues.Add($"Uygunsuz kelime tespit edildi: {string.Join(", ", profanityCheck.words)}");
            result.BlockedReason = "Mesajınızda uygunsuz ifadeler bulunmaktadır.";
        }

        // Telefon numarasi kontrolu
        var phoneCheck = CheckPhoneNumber(content);
        if (phoneCheck)
        {
            result.HasPhoneNumber = true;
            result.IsValid = false;
            result.DetectedIssues.Add("Telefon numarası tespit edildi");
            result.BlockedReason = result.BlockedReason == null
                ? "Mesajınızda telefon numarası paylaşımı yasaktır."
                : result.BlockedReason + " Ayrıca telefon numarası paylaşımı yasaktır.";
        }

        // E-posta kontrolu (opsiyonel - simdilik sadece tespit)
        if (EmailPattern.IsMatch(content))
        {
            result.HasEmail = true;
            result.DetectedIssues.Add("E-posta adresi tespit edildi");
        }

        return result;
    }

    public string SanitizeContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        var sanitized = content;
        var isTurkish = IsTurkishContent(content);

        // Kufurleri yildizla
        foreach (var word in _profanityList)
        {
            if (_whitelistedWords.Contains(word))
                continue;

            // Kisa Turkce kufurler icin baglam kontrolu
            if (TurkishShortProfanities.Contains(word) && !isTurkish)
                continue;

            var pattern = new Regex(
                $@"\b{Regex.Escape(word)}\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            sanitized = pattern.Replace(sanitized, match =>
                new string('*', match.Value.Length));
        }

        // Telefon numaralarini gizle
        foreach (var pattern in PhonePatterns)
        {
            sanitized = pattern.Replace(sanitized, match =>
            {
                if (match.Value.Length <= 4)
                    return new string('*', match.Value.Length);

                var visible = match.Value.Substring(match.Value.Length - 2);
                return new string('*', match.Value.Length - 2) + visible;
            });
        }

        return sanitized;
    }

    public async Task LoadProfanityListAsync()
    {
        await Task.CompletedTask;
    }

    public void AddBlockedWord(string word)
    {
        if (!string.IsNullOrWhiteSpace(word))
        {
            _profanityList.Add(word.ToLowerInvariant());
        }
    }

    public void AddWhitelistedWord(string word)
    {
        if (!string.IsNullOrWhiteSpace(word))
        {
            _whitelistedWords.Add(word.ToLowerInvariant());
        }
    }

    private (bool found, List<string> words) CheckProfanity(string content)
    {
        var foundWords = new List<string>();
        var lowerContent = content.ToLowerInvariant();
        var normalizedContent = NormalizeText(lowerContent);

        // Turkce baglam tespiti yap
        var isTurkish = IsTurkishContent(content);

        foreach (var word in _profanityList)
        {
            if (_whitelistedWords.Contains(word))
                continue;

            // Kisa Turkce kufurler icin ozel kontrol
            // Bu kelimeler (am, got, sik, pic vb.) sadece Turkce baglamda engellenir
            if (TurkishShortProfanities.Contains(word))
            {
                // Eger icerik Turkce degilse, kisa Turkce kufurleri engelleme
                if (!isTurkish)
                    continue;

                // Turkce baglamda kelime siniri ile kontrol et
                var wordBoundaryPattern = new Regex(
                    $@"\b{Regex.Escape(word)}\b",
                    RegexOptions.IgnoreCase);

                if (wordBoundaryPattern.IsMatch(lowerContent))
                {
                    foundWords.Add(word);
                }
            }
            else
            {
                // Diger tum kufurler icin standart kontrol
                var normalizedWord = NormalizeText(word);

                // Kelime siniri ile kontrol (tam kelime eslesmesi)
                var wordBoundaryPattern = new Regex(
                    $@"\b{Regex.Escape(word)}\b",
                    RegexOptions.IgnoreCase);

                if (wordBoundaryPattern.IsMatch(lowerContent))
                {
                    foundWords.Add(word);
                }
                // Normalize edilmis icerikte de kontrol et (leetspeak vb.)
                else if (normalizedContent.Contains(normalizedWord) && normalizedWord.Length >= 4)
                {
                    foundWords.Add(word);
                }
            }
        }

        return (foundWords.Count > 0, foundWords);
    }

    /// <summary>
    /// Icerik Turkce mi kontrol eder
    /// Turkce karakterler veya yaygin Turkce kelimeler iceriyorsa Turkce kabul edilir
    /// </summary>
    private static bool IsTurkishContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        // Turkce ozel karakterler var mi?
        if (content.Any(c => TurkishSpecificChars.Contains(c)))
            return true;

        // Kelimeleri ayir
        var words = Regex.Split(content.ToLowerInvariant(), @"\W+")
            .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 1)
            .ToList();

        if (words.Count == 0)
            return false;

        // Kac tane Turkce kelime var?
        var turkishWordCount = words.Count(w => TurkishCommonWords.Contains(w));

        // Icerik en az 2 Turkce kelime iceriyorsa veya
        // %30'dan fazla Turkce kelime iceriyorsa Turkce olarak kabul et
        return turkishWordCount >= 2 || (words.Count > 0 && (double)turkishWordCount / words.Count >= 0.3);
    }

    private bool CheckPhoneNumber(string content)
    {
        foreach (var pattern in PhonePatterns)
        {
            if (pattern.IsMatch(content))
            {
                return true;
            }
        }
        return false;
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Leetspeak donusumleri
        var normalized = text
            .Replace("1", "i")
            .Replace("!", "i")
            .Replace("3", "e")
            .Replace("4", "a")
            .Replace("@", "a")
            .Replace("0", "o")
            .Replace("5", "s")
            .Replace("$", "s")
            .Replace("7", "t")
            .Replace("8", "b");

        // Bosluk ve ozel karakterleri kaldir
        normalized = Regex.Replace(normalized, @"[\s\-_\.]+", "");

        return normalized;
    }
}

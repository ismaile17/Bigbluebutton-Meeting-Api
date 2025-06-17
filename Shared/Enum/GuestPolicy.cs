// Enum tanımı
namespace Shared.Enum
{
    public enum GuestPolicy
    {
        ALWAYS_ACCEPT = 0,
        ALWAYS_DENY = 1,
        ASK_MODERATOR = 2
    }
}


//guestPolicy(string)
//İsteğe Bağlı.

//Varsayılan değer ALWAYS_ACCEPT'tir. Toplantı için misafir politikasını ayarlar. Misafir politikası, guest=true ile katılım isteği gönderen kullanıcıların toplantıya katılıp katılamayacağını belirler.

//Olası Değerler:
//ALWAYS_ACCEPT: Misafirler her zaman kabul edilir.
//ALWAYS_DENY: Misafirler her zaman reddedilir.
//ASK_MODERATOR: Moderatöre sorulur; moderatör onaylarsa misafir kabul edilir.


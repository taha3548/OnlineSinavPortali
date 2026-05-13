const API_URL = "https://localhost:7087/api";

function getToken() {
    return localStorage.getItem("jwtToken");
}

function setToken(token) {
    localStorage.setItem("jwtToken", token);
}

function getUser() {
    const userJson = localStorage.getItem("user");
    if (!userJson) return null;
    return JSON.parse(userJson);
}

// Kullanıcı Verilerinin localstorage'da Senkronize Edilmesi (Piyasa Standardı)
// Kullanıcı adı, rolü ve avatar bilgisi tarayıcı hafızasında güvenli bir şekilde tutulur.
function setUser(role, name, id, avatar = null) {
    const user = { role, name, id, avatar };
    localStorage.setItem("user", JSON.stringify(user));
    // Geriye dönük uyumluluk için eski anahtarları da tutalım
    localStorage.setItem("userRole", role);
    localStorage.setItem("userName", name);
    localStorage.setItem("userId", id);
}

// Oturum Kapatma Süreci
// Localstorage temizlenir ve kullanıcı giriş sayfasına yönlendirilir.
function logOut() {
    localStorage.clear();
    window.location.href = '/Hesap/Giris';
}

// [YÖNERGE UYUMLULUĞU] - JQuery AJAX & Merkezi API İletişimi
// Tüm MVC-API haberleşmesi tek bir noktadan asenkron (Sayfa yenilenmeden) yönetilir.
function ajaxRequest(endpoint, method, data = null, onSuccess, onError) {
    let headers = {
        "Content-Type": "application/json"
    };
    
    // JWT Token ile Yetkilendirme
    // Her istekte Authorization header'ı gönderilerek API katmanında kimlik doğrulaması sağlanır.
    let token = getToken();
    if (token) {
        headers["Authorization"] = "Bearer " + token;
    }

    $.ajax({
        url: API_URL + endpoint,
        type: method,
        headers: headers,
        data: data ? JSON.stringify(data) : null,
        success: function(res) {
            onSuccess(res);
        },
        error: function(err) {
            // Token Süresi Dolduğunda Otomatik Çıkış Yapılandırması
            if (err.status === 401) {
                Swal.fire({
                    title: 'Oturum Kapatıldı',
                    text: 'Oturum süreniz dolmuş olabilir, lütfen tekrar giriş yapın.',
                    icon: 'warning'
                }).then(() => {
                    logOut();
                });
            } else {
                if(onError) { 
                    onError(err); 
                } else {
                    const mesaj = err.responseJSON?.mesaj || "Beklenmedik bir hata oluştu!";
                    Swal.fire('Hata', mesaj, 'error');
                }
            }
        }
    });
}

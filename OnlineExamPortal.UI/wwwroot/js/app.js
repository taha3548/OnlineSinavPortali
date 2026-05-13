function getApiBaseUrl() {
    var u = typeof window !== "undefined" && window.__API_BASE_URL__;
    if (u && typeof u === "string" && u.length > 0) {
        return u.replace(/\/+$/, "");
    }
    return "https://localhost:7087/api";
}

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
    const user = { role, name, id, userId: id, avatar };
    localStorage.setItem("user", JSON.stringify(user));
    // Geriye dönük uyumluluk için eski anahtarları da tutalım
    localStorage.setItem("userRole", role);
    localStorage.setItem("userName", name);
    localStorage.setItem("userId", id);
}

// Account/Login Process
function logOut() {
    localStorage.clear();
    window.location.href = '/Account/Login';
}

function formatAjaxErrorMessage(err) {
    if (!err) return "Bilinmeyen bir hata oluştu.";
    if (err.status === 0)
        return "API'ye bağlanılamadı. OnlineExamPortal.API projesinin çalıştığından ve appsettings.json içindeki ApiBaseUrl adresinin doğru olduğundan emin olun.";
    var body = err.responseJSON;
    if (body) {
        if (typeof body.message === "string" && body.message) return body.message;
        if (typeof body.Message === "string" && body.Message) return body.Message;
        if (typeof body.title === "string" && body.title) return body.title;
        if (body.errors && typeof body.errors === "object") {
            var parts = [];
            for (var k in body.errors) {
                var arr = body.errors[k];
                if (Array.isArray(arr)) parts.push(arr.join(" "));
            }
            if (parts.length) return parts.join(" ");
        }
    }
    if (err.status === 403) return "Bu işlem için yetkiniz yok.";
    if (err.status === 404) return "İstenen kaynak bulunamadı (404).";
    if (err.responseText && err.responseText.length < 400) return err.responseText;
    return "Sunucu hatası (HTTP " + (err.status || "?") + ").";
}

// [AJAX INTEGRATION] - JQuery AJAX & Central API Communication
function ajaxRequest(endpoint, method, data = null, onSuccess, onError) {
    let headers = {
        "Content-Type": "application/json"
    };
    
    // Authorization with JWT Token
    let token = getToken();
    if (token) {
        headers["Authorization"] = "Bearer " + token;
    }

    $.ajax({
        url: getApiBaseUrl() + endpoint,
        type: method,
        headers: headers,
        data: data ? JSON.stringify(data) : null,
        success: function(res) {
            onSuccess(res);
        },
        error: function(err) {
            // Automatic Logout on Token Expiration
            if (err.status === 401) {
                Swal.fire({
                    title: 'Oturum süresi doldu',
                    text: 'Oturumunuzun süresi dolmuş olabilir. Lütfen yeniden giriş yapın.',
                    icon: 'warning'
                }).then(() => {
                    logOut();
                });
            } else {
                if(onError) { 
                    onError(err); 
                } else {
                    const message = formatAjaxErrorMessage(err);
                    Swal.fire('Hata', message, 'error');
                }
            }
        }
    });
}
